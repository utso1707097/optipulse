#!/usr/bin/env bash
# OpenAPI contract-generation pipeline (Constitution Principle VII).
#
# 1. Builds & runs the .NET 10 API host briefly, fetches its native OpenAPI
#    document (exposed at /openapi/v1.json by Microsoft.AspNetCore.OpenApi),
#    and saves it as the canonical contracts-gen/openapi.json.
# 2. Regenerates the TypeScript client (React) and Dart client (Flutter) from it.
#
# CI drift gate: run this, then `git diff --exit-code` over this directory and
# the two generated client directories. A non-empty diff means someone changed
# an endpoint without regenerating — see contracts/openapi-pipeline.md.
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
API_PROJECT="$ROOT_DIR/backend/src/OptiPulse.Api/OptiPulse.Api.csproj"
OPENAPI_OUT="$ROOT_DIR/contracts-gen/openapi.json"
RAW_SPEC="$ROOT_DIR/contracts-gen/.openapi.raw.json"
API_PORT="${OPTIPULSE_OPENAPI_PORT:-5289}"
API_URL="http://localhost:${API_PORT}"

echo "==> Building API host"
dotnet build "$API_PROJECT" -c Release

echo "==> Starting API host to export the OpenAPI document"
ASPNETCORE_URLS="$API_URL" ASPNETCORE_ENVIRONMENT=Development \
  dotnet run --project "$API_PROJECT" -c Release --no-build --no-launch-profile &
API_PID=$!
trap 'kill "$API_PID" 2>/dev/null || true' EXIT

echo "==> Waiting for API host to become ready"
for i in $(seq 1 30); do
  if curl -sf "$API_URL/openapi/v1.json" -o /dev/null 2>/dev/null; then
    break
  fi
  sleep 1
  if [ "$i" -eq 30 ]; then
    echo "ERROR: API host did not become ready in time" >&2
    exit 1
  fi
done

curl -sf "$API_URL/openapi/v1.json" -o "$RAW_SPEC"
kill "$API_PID" 2>/dev/null || true
trap - EXIT

# Normalise the spec before committing it (Principle VII: the committed contract must not
# depend on where the generator ran). The .NET OpenAPI document includes a `servers` entry
# built from the live bind address (e.g. http://localhost:5289/), so without this the gate
# reports drift for anyone using a non-default OPTIPULSE_OPENAPI_PORT. Re-serialising with
# sorted keys also makes the output byte-stable regardless of property ordering.
python3 - "$RAW_SPEC" "$OPENAPI_OUT" <<'PY'
import json, sys
with open(sys.argv[1]) as f:
    spec = json.load(f)
spec.pop("servers", None)
with open(sys.argv[2], "w") as f:
    json.dump(spec, f, indent=2, sort_keys=True)
    f.write("\n")
PY
rm -f "$RAW_SPEC"
echo "==> Wrote $OPENAPI_OUT (servers stripped, keys sorted)"

# Generator versions are PINNED. An unpinned `npx openapi-typescript` resolves whatever is
# latest at run time, which makes the drift gate non-deterministic: an upstream release shows
# up as phantom contract drift on an unrelated change. The gate is a merge blocker, so it must
# fail only for real drift.
OPENAPI_TS_VERSION="7.13.0"
OPENAPI_GENERATOR_VERSION="7.11.0"

# Principle VII: FAIL, never skip. Silently skipping generation makes the gate pass by
# diffing an unchanged (or empty) directory — enforcement that reports success while
# providing none. Set OPTIPULSE_ALLOW_MISSING_GENERATORS=1 for local runs only.
require_tool() {
  local tool="$1" hint="$2"
  if command -v "$tool" >/dev/null 2>&1; then
    return 0
  fi
  if [ "${OPTIPULSE_ALLOW_MISSING_GENERATORS:-0}" = "1" ]; then
    echo "WARN: '$tool' not found — SKIPPING generation. The drift gate is NOT enforced for" >&2
    echo "      this client in this run. Never set this in CI. ($hint)" >&2
    return 1
  fi
  echo "ERROR: '$tool' not found and OPTIPULSE_ALLOW_MISSING_GENERATORS is not set." >&2
  echo "       Refusing to skip: a skipped generator makes the drift gate vacuous." >&2
  echo "       $hint" >&2
  exit 1
}

echo "==> Generating TypeScript client (React) with openapi-typescript@$OPENAPI_TS_VERSION"
if require_tool npx "Install Node.js 22+."; then
  npx --yes "openapi-typescript@$OPENAPI_TS_VERSION" "$OPENAPI_OUT" \
    -o "$ROOT_DIR/web/optipulse_dashboard/src/api/schema.d.ts"
fi

# The Flutter client is still a scaffold until Phase 6 (T063–T077): lib/ contains no Dart at
# all. Enforcement is therefore tied to whether the client EXISTS, not to a date — the moment
# any Dart source lands, a missing generator becomes a hard failure. This is the same rule the
# mobile CI job applies to `flutter test`, and it is not a skip-in-disguise: it fails as soon
# as there is a client that could drift.
if find "$ROOT_DIR/mobile/optipulse_app/lib" -name '*.dart' -print -quit 2>/dev/null | grep -q .; then
  echo "==> Generating Dart client (Flutter) with openapi-generator@$OPENAPI_GENERATOR_VERSION"
  if require_tool openapi-generator "Requires a JRE; see T094. Install via 'npm i -g @openapitools/openapi-generator-cli'."; then
    OPENAPI_GENERATOR_VERSION="$OPENAPI_GENERATOR_VERSION" openapi-generator generate \
      -i "$OPENAPI_OUT" -g dart-dio \
      -o "$ROOT_DIR/mobile/optipulse_app/lib/core/generated"
  fi
else
  echo "NOTE: Flutter client has no Dart source yet (scaffold; T063-T077)." >&2
  echo "      Dart contract enforcement does NOT exist until then — do not read a green" >&2
  echo "      drift gate as covering Flutter. Becomes mandatory automatically once lib/*.dart appears." >&2
fi

echo "==> Done. Review the diff in contracts-gen/, web/.../src/api/, and mobile/.../lib/core/generated/."
