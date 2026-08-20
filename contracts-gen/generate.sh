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
# openapi-generator-cli reads its version from openapitools.json at the repository root, NOT
# from this variable — passing OPENAPI_GENERATOR_VERSION in the environment did nothing, and
# the client was being generated with 7.24.0 while this line claimed 7.11.0. The pin lives in
# openapitools.json (committed); this value exists to be checked against what actually ran.
OPENAPI_GENERATOR_VERSION="7.24.0"

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
  # The binary installed by @openapitools/openapi-generator-cli is `openapi-generator-cli`,
  # NOT `openapi-generator`. This branch had never executed — it is gated on Dart source
  # existing, and none did until the app was scaffolded — so the wrong name sat here looking
  # correct. The first run that reached it would have failed the drift gate on a missing tool.
  if require_tool openapi-generator-cli "Requires a JRE; see T094. Install via 'npm i -g @openapitools/openapi-generator-cli'."; then
    DART_CLIENT="$ROOT_DIR/mobile/optipulse_app/lib/core/generated"

    # The client's OWN lockfile is preserved across regeneration. Without this the wipe below
    # deletes it, `dart pub get` re-resolves every transitive codegen dependency to whatever is
    # newest, and the committed tree becomes a function of WHEN it was generated rather than of
    # the spec. That is not hypothetical: source_gen moved 4.2.4 -> 4.3.0 between a local run
    # and a CI run and failed the drift gate on a dependency bump that had nothing to do with
    # the contract. A pinned generator whose own dependencies float is not pinned.
    SAVED_LOCK="$(mktemp)"
    if [ -f "$DART_CLIENT/pubspec.lock" ]; then
      cp "$DART_CLIENT/pubspec.lock" "$SAVED_LOCK"
    else
      SAVED_LOCK=""
    fi

    # Wipe before generating. openapi-generator writes files but never removes ones that are
    # no longer produced: when tags were added to the API, the previous single
    # opti_pulse_api_api.dart stopped being exported yet stayed on disk as committed dead
    # code that nothing referenced and no gate could notice. Regenerating into a clean
    # directory makes the output a function of the spec alone.
    rm -rf "$DART_CLIENT"

    openapi-generator-cli generate \
      -i "$OPENAPI_OUT" -g dart-dio \
      -o "$DART_CLIENT"

    # Assert the pin was honoured. A pin nobody verifies is how 7.11.0 sat in this script
    # while 7.24.0 did the work: the drift gate stayed green because both sides of the diff
    # came from the same wrong version.
    ACTUAL_VERSION="$(cat "$DART_CLIENT/.openapi-generator/VERSION")"
    if [ "$ACTUAL_VERSION" != "$OPENAPI_GENERATOR_VERSION" ]; then
      echo "ERROR: generated with openapi-generator $ACTUAL_VERSION, expected $OPENAPI_GENERATOR_VERSION." >&2
      echo "       Update openapitools.json and this script together, then regenerate." >&2
      exit 1
    fi

    # The generator hardcodes `sdk: '>=2.18.0 <4.0.0'` in the client's pubspec. That pins the
    # package to Dart language version 2.18 while the app is on 3.12, and the built_value
    # parts then fail to compile with "the language version override has to be the same in
    # the library and its part(s)". Align it with the app's floor.
    python3 - "$DART_CLIENT/pubspec.yaml" <<'NORMALISE'
import re, sys
path = sys.argv[1]
with open(path) as f:
    text = f.read()
text, n = re.subn(r"sdk: '>=2\.18\.0 <4\.0\.0'", "sdk: '>=3.12.0 <4.0.0'", text)
assert n == 1, f"expected one SDK constraint to rewrite, found {n}"
with open(path, "w") as f:
    f.write(text)
NORMALISE

    # The generator's own .gitignore excludes pubspec.lock, which is right for a hand-written
    # library and wrong here: this tree is COMMITTED and diffed by the drift gate, so its lock
    # is what keeps codegen reproducible. Left alone, a future `git rm` would untrack the lock
    # and nothing would be able to add it back.
    python3 - "$DART_CLIENT/.gitignore" <<'UNIGNORE'
import sys
path = sys.argv[1]
with open(path) as f:
    lines = f.readlines()
with open(path, "w") as f:
    for line in lines:
        if line.strip() == "pubspec.lock":
            f.write("# pubspec.lock is deliberately COMMITTED here: this generated client is\n")
            f.write("# checked in and diffed by the contract drift gate, and its lock is what\n")
            f.write("# keeps build_runner output reproducible across machines.\n")
            continue
        f.write(line)
UNIGNORE

    # dart-dio emits built_value classes whose serialisers live in .g.dart parts that only
    # build_runner can produce. Without this step the client does not compile, so the
    # generated tree is committed in a state no one can build — and the drift gate would
    # still pass, because the spec and the emitted sources agree.
    if require_tool dart "Install the Flutter SDK (which bundles Dart); see mobile/optipulse_app/README.md."; then
      # Restored BEFORE `pub get`, so resolution honours it rather than re-solving.
      if [ -n "$SAVED_LOCK" ]; then
        cp "$SAVED_LOCK" "$DART_CLIENT/pubspec.lock"
        rm -f "$SAVED_LOCK"
      fi

      (cd "$DART_CLIENT" \
        && dart pub get \
        && dart run build_runner build --delete-conflicting-outputs)
    fi
  fi
else
  echo "NOTE: Flutter client has no Dart source yet (scaffold; T063-T077)." >&2
  echo "      Dart contract enforcement does NOT exist until then — do not read a green" >&2
  echo "      drift gate as covering Flutter. Becomes mandatory automatically once lib/*.dart appears." >&2
fi

echo "==> Done. Review the diff in contracts-gen/, web/.../src/api/, and mobile/.../lib/core/generated/."
