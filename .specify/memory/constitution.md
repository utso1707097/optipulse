<!--
Sync Impact Report
==================
Version change: 1.0.0 → 2.0.0
Rationale: MAJOR bump. Principle V was materially REDEFINED from a single-client
"Cross-Platform Client Consistency" model into a differentiated Dual-Client Strategy
(always-online React web dashboard + offline-first Flutter mobile), which changes the
meaning of an existing governance principle. Two new principles were also added
(Backend-Contained Authentication & Authorization; Contract-First API Security).

Modified principles:
- V. Cross-Platform Client Consistency → V. Dual-Client Strategy (redefined)

Added principles:
- VI. Backend-Contained Authentication & Authorization (Custom JWT/RBAC, .NET 10 only)
- VII. Contract-First API Security (native OpenAPI enforcement, no contract drift)

Added/updated sections:
- Technology Stack & Performance Standards (added React web dashboard, auth, OpenAPI)
- Development Workflow & Quality Gates (added auth + contract-drift gates; Principles I–VII)

Removed sections: (none)

Follow-up TODOs: (none)

Downstream impact (informational, not modified here): specs/001-optipulse-platform
artifacts (spec.md, plan.md, contracts/) currently describe only a Flutter client and no
auth/OpenAPI governance; they should be re-evaluated against this v2.0.0 constitution.
-->

<!--
Sync Impact Report
==================
Version change: 2.0.0 → 2.1.0
Rationale: MINOR bump. Adopted three best-practice kits (.kits/) and encoded the
practice baselines they imply, plus the explicit "pins" that keep the kits from
steering the project away from existing principles. No principle removed or redefined.

Added/updated sections:
- Development Workflow & Quality Gates: added Adopted Toolchain & Practice Baselines
  (dev hooks, anti-pattern gate, package/tooling baselines, and stack pins).

Pins encoded (clarifications of existing principles, not new law):
- Backend architecture pinned to Clean Architecture (kits offer VSA/DDD/modular — not used).
- Auth pinned to custom JWT/RBAC (kit default ASP.NET Identity/OIDC — not used).
- Flutter state pinned to BLoC/Cubit + HydratedBloc (kit offers Riverpod/Provider — not used).
- React state pinned to custom hooks + AuthContext (kit refs Zustand/React Query — not used).
- Flutter networking pinned to Dio; API clients OpenAPI-generated (kit prefers http/bespoke).

Removed sections: (none). Follow-up TODOs: (none).
-->

# OptiPulse Constitution

## Core Principles

### I. Clean Architecture & Layer Discipline

Every backend and Flutter client codebase MUST adhere to Clean Architecture boundaries.
Dependencies point inward only: Domain has no outward dependencies; Application depends on
Domain; Infrastructure and API/Presentation depend on Application and Domain, never the
reverse. Business rules MUST NOT leak into controllers, EF Core entities-as-DTOs, or UI
widgets. Data access (EF Core 10), transport (Dio, HTTP), and framework concerns MUST sit
behind interfaces owned by inner layers. The React web dashboard is exempt from full Clean
Architecture layering per Principle V but MUST still keep API access behind a typed client
layer.

Rationale: OptiPulse spans a .NET backend and multiple clients; an enforced layering model
keeps the flag-evaluation core testable in isolation, portable across SQLite and PostgreSQL,
and resistant to framework churn.

### II. Zero-Allocation Performance (NON-NEGOTIABLE)

Flag evaluation MUST complete in under 5ms in-memory and MUST use the deterministic,
zero-allocation MurmurHash3 algorithm for bucketing. Hot paths MUST NOT allocate on the
managed heap: use `Span<T>`, `stackalloc`, pooled buffers, and struct-based value types;
avoid LINQ, boxing, closures, and per-call string formatting in evaluation code. Every hot
path change MUST be validated with a benchmark (e.g., BenchmarkDotNet) proving zero
steady-state allocations and the sub-5ms budget before merge.

Rationale: OptiPulse is a real-time feature-flag platform; deterministic, allocation-free
evaluation is the product's defining guarantee and directly bounds tail latency and GC pauses.

### III. .NET 10 Modern Standards (NON-NEGOTIABLE)

All backend projects MUST target .NET 10 and MUST remain Native AOT compatible. Code MUST
use modern C# language features (nullable reference types enabled, records, pattern matching,
primary constructors, collection expressions) and MUST avoid patterns incompatible with AOT
(unbounded reflection, runtime code generation, dynamic). Trimming and AOT warnings MUST be
treated as build errors, not suppressed silently.

Rationale: Native AOT compatibility delivers fast startup, low memory, and predictable
performance for the evaluation edge, and a single enforced language baseline prevents
fragmentation across services.

### IV. Resilience & Fail-Safe Kill-Switch Operations

Every outbound dependency (database, Redis, downstream services) MUST be wrapped in a Polly
resilience policy (circuit breaker plus timeout/retry as appropriate). Global cache
invalidation for emergency kill-switches MUST propagate in under 100ms via Redis Pub/Sub.
When a dependency is degraded or unavailable, evaluation MUST fail safe to the last known
good in-memory state rather than error; kill-switch semantics MUST always be honored even
under partial outage.

Rationale: A feature-flag service is on the critical path of every consumer; it must degrade
gracefully and must never prevent an operator from disabling a bad feature instantly.

### V. Dual-Client Strategy

OptiPulse ships exactly two first-party clients, each held to a deliberately different
standard:

- **Web Dashboard (React)**: A lightweight, always-online React application. It MUST use
  simplified React hooks and lightweight local state (component/hook state or a minimal store);
  it MUST NOT introduce offline-first persistence, heavy client-side domain layers, or complex
  state frameworks. It assumes network availability and reads server truth directly.
- **Mobile App (Flutter)**: An offline-first Flutter application that MUST follow Clean
  Architecture with BLoC/Cubit for state management, HydratedBloc for persisted/offline state,
  and Dio for network access behind a repository abstraction. UI MUST NOT contain business
  logic; presentation state MUST be driven exclusively through Cubits/Blocs. Cached state MUST
  reconcile deterministically with server truth on reconnect, with kill-switch state taking
  precedence.

Neither client MUST contain authentication or authorization business logic (see Principle VI),
and both MUST consume the backend exclusively through the governed API contract (Principle VII).

Rationale: The dashboard and the mobile app have different operating conditions — an operator
console on reliable networks versus an on-device SDK/app that must survive disconnection.
Forcing one architecture on both would over-engineer the web client and under-engineer the
mobile client; differentiating them keeps each fit for purpose while the backend remains the
single source of truth.

### VI. Backend-Contained Authentication & Authorization

Authentication and authorization MUST be implemented as a custom JWT-based scheme with
role-based access control (RBAC) contained strictly within the .NET 10 backend. Tokens MUST be
issued, signed, and validated server-side; RBAC role checks MUST be enforced in the backend on
every protected operation. Clients (React and Flutter) MUST treat tokens as opaque, MUST NOT
implement auth/authorization decision logic, and MUST NOT embed signing secrets. No third-party
external identity provider is required or assumed for the core scheme.

Rationale: Centralizing auth in the backend removes a class of client-side security mistakes,
keeps a single enforcement point for RBAC, and ensures every management, kill-switch, and AI
approval action (already audited under Principle IV and the workflow gates) is authorized
consistently regardless of which client initiated it.

### VII. Contract-First API Security

The backend MUST expose a native OpenAPI specification (generated from the .NET 10 API, not
hand-maintained) as the authoritative contract for every client-facing endpoint. Both the
React and Flutter clients MUST be built against this generated specification, and CI MUST fail
the build on contract drift — any endpoint, schema, or field change that is not reflected in
the published OpenAPI spec and the client contract artifacts. Breaking contract changes MUST be
versioned explicitly.

Rationale: With two independent clients consuming the same API, contract drift is the most
likely source of silent runtime breakage. A native, generated OpenAPI spec enforced in CI makes
the backend the single source of truth and prevents React and Flutter from diverging from the
server or from each other.

## Technology Stack & Performance Standards

- Backend: .NET 10 Web API, Clean Architecture, EF Core 10 with SQLite (dev/edge) and
  PostgreSQL (production), Redis Pub/Sub for cache invalidation, Polly for resilience, custom
  JWT/RBAC auth, native OpenAPI generation.
- Web Dashboard: React with simplified hooks and lightweight state; always-online; typed API
  client generated from / validated against the backend OpenAPI spec.
- Mobile App: Flutter with Clean Architecture, BLoC/Cubit, HydratedBloc, Dio; offline-first;
  API client generated from / validated against the backend OpenAPI spec.
- Performance budgets (hard requirements, verified by benchmark/telemetry):
  - In-memory flag evaluation: < 5ms, zero steady-state allocations, MurmurHash3 bucketing.
  - Global kill-switch cache invalidation: < 100ms end-to-end via Redis Pub/Sub.
- Compatibility: all backend assemblies Native AOT + trim compatible; nullable enabled;
  AOT/trim analyzer warnings fail the build.
- Security: all protected endpoints require a valid backend-issued JWT; RBAC enforced
  server-side; signing secrets never leave the backend.

## Development Workflow & Quality Gates

- Architecture: changes MUST preserve Clean Architecture dependency direction in the backend and
  Flutter client; violations block merge. The React dashboard is held to Principle V's
  lightweight standard instead.
- Performance: any change touching evaluation, hashing, caching, or invalidation MUST include a
  benchmark demonstrating the relevant budget (< 5ms / < 100ms / zero-alloc).
- Resilience: new external dependencies MUST ship with a Polly policy and a documented fail-safe
  behavior.
- Authentication: protected endpoints MUST enforce JWT validation and RBAC server-side; PRs
  adding endpoints MUST declare required roles. No auth logic may be added to clients.
- Contracts: the native OpenAPI spec MUST be regenerated and the client contract artifacts
  updated in the same change; CI MUST fail on contract drift between backend, React, and Flutter.
- Testing: Domain and Application logic MUST have unit tests; cross-layer and inter-service
  contracts MUST have integration tests.
- Reviews: every PR MUST verify compliance with Principles I–VII; deviations MUST be justified
  in writing and approved, or the PR is rejected.

### Adopted Toolchain & Practice Baselines

The project adopts three best-practice kits (kept under `.kits/`): the .NET kit (backend),
the Flutter kit (mobile), and general Claude Code practices (repo tooling). The following are
baseline requirements derived from them:

- **Dev hooks (required)**: format-on-write, a secret-blocking pre-write guard, and a
  destructive-command (force-push / `reset --hard` / `rm -rf`) guard MUST be active
  (`.claude/settings.json` + `.claude/hooks/`).
- **Anti-pattern gate (backend)**: code MUST NOT use `DateTime.Now`/`DateTime.UtcNow` directly
  (use `TimeProvider`), `async void`, or `new HttpClient()` (use `IHttpClientFactory`). The
  Result pattern is used for expected failures; broad `catch` is disallowed (reinforces Principle
  IV error handling).
- **Package/tooling baseline (backend)**: Polly via `Microsoft.Extensions.Http.Resilience`,
  xUnit v3 + Testcontainers (no in-memory DB) for integration-first tests, Serilog +
  OpenTelemetry, `Asp.Versioning`, `TimeProvider`; native OpenAPI (no Swashbuckle); versions via
  `Directory.Packages.props` (never hardcoded). The Roslyn navigator MCP is available for
  code navigation/analysis.
- **Architecture pin**: backend MUST use Clean Architecture (the kits' VSA/DDD/modular options
  are not used); auth MUST be custom JWT/RBAC (not ASP.NET Identity/external OIDC).
- **Hot-path exclusion**: the flag-evaluation hot path MUST NOT route through mediator, cache-aside,
  or other indirection layers (protects Principle II); such patterns are for management/CRUD paths only.
- **Client pins**: Flutter state = BLoC/Cubit + HydratedBloc only (no Riverpod/Provider/ChangeNotifier
  for app state); React state = custom hooks + a single AuthContext only (no Redux/MobX/Zustand/React
  Query); Flutter networking = Dio; both clients' API models MUST be OpenAPI-generated.

## Governance

This constitution supersedes all other engineering practices for OptiPulse. Amendments MUST
be proposed via pull request, documented with rationale, and approved by the project
maintainers; breaking governance changes additionally require a migration note.

Versioning follows semantic versioning: MAJOR for backward-incompatible governance or
principle removals/redefinitions, MINOR for newly added or materially expanded principles or
sections, PATCH for clarifications, wording, typo fixes, non-semantic refinements.

Compliance is reviewed at every pull request; reviewers MUST confirm adherence to all
principles and performance budgets. Complexity or deviation MUST be explicitly justified.
Runtime development guidance and agent-specific instructions live in their respective guidance
files and MUST remain consistent with this constitution.

**Version**: 2.1.0 | **Ratified**: 2026-08-15 | **Last Amended**: 2026-08-17
