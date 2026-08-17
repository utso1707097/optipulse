# contracts-gen

OpenAPI client-generation pipeline (Constitution Principle VII — Contract-First API Security).

The .NET 10 backend emits a native OpenAPI document (`openapi.json`), the single source of truth for
every client-facing endpoint. This workspace turns it into:

- **TypeScript** types + typed client for the React dashboard → `web/optipulse_dashboard/src/api/`
- **Dart** models + Dio client for the Flutter app → `mobile/optipulse_app/lib/core/`

A **CI drift gate** regenerates the spec and both clients and fails the build on any diff, so neither
client can diverge from the server or from each other.

- Contract details: [../specs/001-optipulse-platform/contracts/openapi-pipeline.md](../specs/001-optipulse-platform/contracts/openapi-pipeline.md)
- Tasks: T007 / T017 / T056 / T072 / T084 in [../specs/001-optipulse-platform/tasks.md](../specs/001-optipulse-platform/tasks.md)

> `generate.sh` and the CI wiring are created during implementation (Phase 1 setup / T007, T017).
