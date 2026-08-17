# Contract: OpenAPI Client-Generation Pipeline

Enforces Constitution Principle VII (Contract-First API Security) and FR-030: the backend is the
single source of truth, and both clients are generated from its native OpenAPI spec so they cannot
drift from the server or each other.

## Source of truth
The .NET 10 Web API emits a **native OpenAPI 3.x document** via `Microsoft.AspNetCore.OpenApi`
(not Swashbuckle/reflection-heavy tooling — chosen for the better Native AOT story, R11). Every
client-facing endpoint (auth, evaluation, management, AI gateway, telemetry/audit) is described in
this document.

Canonical artifact: `contracts-gen/openapi.json` (regenerated in CI from the running/host-built API).

## Generation targets

| Client | Generator | Output | Consumed by |
|--------|-----------|--------|-------------|
| React (TypeScript) | `openapi-typescript` (or Kiota) | `web/optipulse_dashboard/src/api/` — types + thin typed fetch client | custom hooks (`useFlags`, `useAuth`, …) |
| Flutter (Dart) | `openapi-generator` `dart-dio` (or Kiota) | `mobile/optipulse_app/lib/core/` — Dart models + Dio client | repositories behind BLoC/Cubit |

Both outputs are **generated, never hand-edited**. Generated directories are marked as such and
excluded from manual modification.

## CI drift gate (hard build failure)

The pipeline runs in CI on every change touching the API or clients:

```text
1. Build backend → export openapi.json
2. Regenerate TypeScript types (React) and Dart models (Flutter) from openapi.json
3. git diff --exit-code over openapi.json + generated client dirs
   → non-empty diff = DRIFT = FAIL the build
```

**Meaning of failure**: an endpoint/schema/field changed without the spec and both client artifacts
being regenerated and committed. The fix is to regenerate and commit, not to bypass the gate.

## Versioning
- **Additive** changes (new optional field, new endpoint) regenerate cleanly and are non-breaking.
- **Breaking** changes (removed/renamed field, type change, removed endpoint) MUST be versioned
  explicitly (path segment `/api/v2/...` or media-type version) so existing generated clients keep
  working until migrated. The gate flags breaking diffs for reviewer attention.

## Relationship to other contracts
Every endpoint documented under [auth-api.md](auth-api.md), [evaluation-api.md](evaluation-api.md),
[management-api.md](management-api.md), [ai-gateway-api.md](ai-gateway-api.md), and
[telemetry-audit-api.md](telemetry-audit-api.md) appears in `openapi.json`; those Markdown files are
the human-readable design, `openapi.json` is the machine-authoritative contract, and the generated
TS/Dart artifacts are the enforced client bindings.
