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

curl -sf "$API_URL/openapi/v1.json" -o "$OPENAPI_OUT"
kill "$API_PID" 2>/dev/null || true
trap - EXIT
echo "==> Wrote $OPENAPI_OUT"

echo "==> Generating TypeScript client (React)"
if command -v npx >/dev/null 2>&1; then
  npx --yes openapi-typescript "$OPENAPI_OUT" \
    -o "$ROOT_DIR/web/optipulse_dashboard/src/api/schema.d.ts"
else
  echo "SKIP: npx not found — TypeScript client not regenerated" >&2
fi

echo "==> Generating Dart client (Flutter)"
if command -v openapi-generator >/dev/null 2>&1; then
  openapi-generator generate -i "$OPENAPI_OUT" -g dart-dio \
    -o "$ROOT_DIR/mobile/optipulse_app/lib/core/generated"
else
  echo "SKIP: openapi-generator not found — Dart client not regenerated" >&2
fi

echo "==> Done. Review the diff in contracts-gen/, web/.../src/api/, and mobile/.../lib/core/generated/."
