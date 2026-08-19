<!--
Sync Impact Report
==================
Version change: 2.3.0 → 2.4.0
Rationale: MINOR bump. Principle V's React state-management pin is RELAXED to permit Redux
Toolkit, at the maintainer's decision, taken before any dashboard code exists. Amended rather
than quietly violated: the code and this document disagreeing is the failure mode v2.2.0 was
written to end, and a pin that is ignored in practice provides no governance.

Modified principles:
- V. Dual-Client Strategy — the React client may now use Redux Toolkit for client state. The
  constraints that remain are the ones that protect the architecture rather than the taste:
  server state is still not cached in a client store as a second source of truth, and auth
  logic still lives only in the backend (Principle VI).

Added/updated sections:
- Adopted Toolchain & Practice Baselines: React state pin rewritten; Tailwind CSS adopted for
  styling, which the constitution previously did not address at all.

Note on the trade-off, recorded so the decision is legible later: nearly all dashboard state is
SERVER state (flags, experiments, analytics), which a store does not make easier to manage. The
original pin existed for that reason. The maintainer has chosen Redux Toolkit anyway; this
document now says so plainly instead of being contradicted by the code.
-->

<!--
Sync Impact Report
==================
Version change: 2.2.0 → 2.3.0
Rationale: MINOR bump. Added a Deployment & First-Run Bootstrap baseline. The project had no
governance covering how it reaches a running environment, and the gap became concrete when
preparing a public demonstration deployment: nothing said an initial account may not use a
predictable password, and nothing said CORS may not be opened to every origin. Both are one
careless line away from exposing a live kill-switch. No principle removed or weakened.

Added/updated sections:
- Development Workflow & Quality Gates: added Deployment & First-Run Bootstrap.

Downstream follow-up: spec.md gains FR-031..FR-034 (cross-origin access, first-run bootstrap,
credential provenance, idempotence); tasks.md gains a deployment-enablement phase.
-->

<!--
Sync Impact Report
==================
Version change: 2.1.0 → 2.2.0
Rationale: MINOR bump. Four principles were materially clarified or expanded after a
grill-with-docs review of the code built in Phases 1–4 (47/96 tasks) found governance text
that the codebase either contradicted or could not satisfy. No principle was removed, and no
performance or security guarantee was weakened. Every amendment below was verified against
the codebase rather than inferred.

Modified principles:
- III. .NET 10 Modern Standards — Native AOT requirement SCOPED to hot-path assemblies; the
  API host's AOT publish becomes an explicitly gated goal. Reason: setting PublishAot bakes
  RuntimeFeature.IsDynamicCodeSupported=false into every build's runtimeconfig, which makes
  EF Core refuse runtime model building and prevented the API host from starting at all; EF
  migrations are RequiresDynamicCode (IL3050) and are not AOT-supported in any form.
- IV. Resilience & Fail-Safe Kill-Switch Operations — resolved a direct CONTRADICTION with
  Principle II. "Every outbound dependency MUST be wrapped in Polly" collided with the
  hot-path indirection ban; in practice two resilience pipelines were registered and never
  consumed. Polly scope is now stated positively, the hot path is explicitly exempt (it
  satisfies resilience structurally instead), and an unused pipeline is now a violation.
- VI. Backend-Contained Authentication & Authorization — ADDED service accounts / SDK
  credentials as a credential type distinct from human users. Reason: the Identity subdomain
  models humans only, so the runtime evaluation endpoints could not be bound to a
  Manager/Admin role and remain anonymous; that state must be documented, not misdescribed.
- VII. Contract-First API Security — the drift gate must FAIL rather than silently skip a
  missing client generator, generator versions must be pinned, and the committed spec must
  not embed environment-specific server URLs. Reason: Dart generation silently skips whenever
  openapi-generator is absent (always, in CI), so the gate passed by diffing an empty
  directory and Flutter could drift from the server freely.

Added/updated sections:
- Technology Stack & Performance Standards: PostgreSQL declared the single migrated provider
  (SQLite is dev/edge via EnsureCreated); AOT compatibility restated at its verifiable scope.
- Development Workflow & Quality Gates: added a time-source gate; gates must enforce every
  pattern the constitution names; contract gate must fail-not-skip.
- Adopted Toolchain & Practice Baselines: corrected the xUnit pin from v3 to v2 (matches the
  repo; a v3 migration is churn with no benefit), restated Asp.Versioning as adopt-now, added
  the DateTimeOffset ban and the resilience-pipeline-consumer rule.

Removed sections: (none)

Downstream follow-up REQUIRED (not performed here):
- tasks.md contains four completions that the artifacts contradict and which must be
  corrected: T003a (claims Asp.Versioning + xUnit v3 pinned — neither is present; xunit is
  2.9.3), T012 (Polly pipelines registered with zero consumers), T030 and T041 (both claim
  "service-account auth" on evaluation endpoints, which is anonymous and has no
  service-account concept anywhere in the codebase).
- Gap-closing tasks must be appended for: Asp.Versioning adoption, drift-gate determinism,
  TimeProvider at the 5 DateTimeOffset.UtcNow sites, Polly wiring on management paths,
  service-account credentials, Postgres-authored migrations, and Dart client generation in CI.
-->

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

The evaluation hot path MUST NOT be routed through mediators, cache-aside layers, resilience
pipelines, or any other per-call indirection. Where this appears to conflict with Principle
IV's resilience mandate, Principle IV's stated hot-path exemption governs: the hot path earns
resilience structurally (in-memory snapshot reads that never block on an external dependency),
not by wrapping calls it does not make.

Rationale: OptiPulse is a real-time feature-flag platform; deterministic, allocation-free
evaluation is the product's defining guarantee and directly bounds tail latency and GC pauses.

### III. .NET 10 Modern Standards (NON-NEGOTIABLE)

All backend projects MUST target .NET 10. Code MUST use modern C# language features (nullable
reference types enabled, records, pattern matching, primary constructors, collection
expressions) and MUST avoid patterns incompatible with AOT (unbounded reflection, runtime code
generation, `dynamic`). Trimming and AOT warnings MUST be treated as build errors, not
suppressed silently.

Native AOT compatibility is required at the scope where it is verifiable and beneficial:

- **Hot-path assemblies** (`OptiPulse.Evaluation.*`) MUST build clean with the AOT and trim
  analyzers enabled and warnings as errors. This is the enforceable guarantee, and it MUST NOT
  regress.
- **The API host's Native AOT publish** is a Phase 8 goal, not a standing build property. It is
  gated on two prerequisites: (a) EF Core compiled models (`dotnet ef dbcontext optimize`) for
  every DbContext, and (b) moving schema migration off the application startup path.
- `PublishAot` MUST NOT be set as a standing project property until both prerequisites hold.
  It is not publish-only: the SDK bakes AOT feature switches — including
  `RuntimeFeature.IsDynamicCodeSupported=false` — into the runtimeconfig of *every* build,
  which makes EF Core refuse to build a model at all and prevents the host from starting.
  Asserting AOT via this property before it is genuinely achievable breaks the application
  rather than proving anything.

Rationale: Native AOT compatibility delivers fast startup, low memory, and predictable
performance for the evaluation edge. Claiming it where EF Core makes it impossible produced a
host that could not boot; scoping the claim to the assemblies that can actually honour it keeps
the guarantee real and enforced instead of aspirational.

### IV. Resilience & Fail-Safe Kill-Switch Operations

Outbound dependency calls MUST be governed by an explicit resilience policy, scoped as follows:

- **Polly-wrapped paths (REQUIRED)**: management and CRUD persistence, invalidation publishing,
  outbound provider calls (e.g. the LLM gateway, push delivery), and any other request-scoped
  I/O. Each MUST carry a circuit breaker plus timeout/retry as appropriate.
- **Hot-path exemption (REQUIRED, not optional)**: the flag-evaluation path MUST NOT be wrapped
  in a resilience pipeline, because per-call indirection violates Principle II. It satisfies
  resilience structurally instead: it reads only in-memory snapshot state, MUST NOT block on
  Redis or the database, and MUST fail safe to the last known good snapshot.
- **No decorative pipelines**: a registered resilience pipeline MUST have at least one consumer.
  A pipeline that is registered and never used is a governance violation, not compliance, and
  MUST be either wired to a call site or removed.
- **Startup MUST NOT depend on a degradable dependency**: an unreachable Redis MUST NOT prevent
  the API from starting or serving evaluations; invalidation transport is recoverable, and
  refusing to boot converts a partial outage into total unavailability.

Global cache invalidation for emergency kill-switches MUST propagate in under 100ms via Redis
Pub/Sub. When a dependency is degraded or unavailable, evaluation MUST fail safe to the last
known good in-memory state rather than error; kill-switch semantics MUST always be honored even
under partial outage.

Rationale: A feature-flag service is on the critical path of every consumer; it must degrade
gracefully and must never prevent an operator from disabling a bad feature instantly. The
previous blanket "every dependency MUST be wrapped" wording contradicted Principle II, and the
contradiction resolved itself the worst way in practice — pipelines were registered to satisfy
the letter of the rule and wired to nothing, leaving the real dependencies unprotected.

### V. Dual-Client Strategy

OptiPulse ships exactly two first-party clients, each held to a deliberately different
standard:

- **Web Dashboard (React)**: A lightweight, always-online React application. Client state MAY be
  managed with Redux Toolkit or with hooks and context; that choice is left to the maintainer
  (amended in v2.4.0). Three constraints remain, because they protect the architecture rather
  than the styling of it:
  - It MUST NOT introduce offline-first persistence. Offline-first is the Flutter client's job;
    duplicating it here would mean two different reconciliation models for the same data.
  - Server state MUST NOT become a second source of truth. A store may hold fetched data for
    rendering, but the backend remains authoritative and the dashboard reads it directly rather
    than reconciling a local copy against it.
  - It MUST NOT contain a client-side domain layer. Business rules — targeting, rollout,
    kill-switch precedence — live in the backend and are not reimplemented for display.
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

OptiPulse recognizes two distinct kinds of caller, and they MUST NOT be conflated:

- **Human users** hold a role (`Manager` or `Admin`) and authenticate with credentials that
  belong to a person. Management, kill-switch, and AI-approval operations are authorized by role.
- **Service accounts / SDK credentials** are machine callers on the runtime evaluation surface.
  They hold no human role, and MUST NOT be authorized by one. They MUST authenticate with their
  own credential type, scoped to evaluation and telemetry ingest only, and MUST be independently
  revocable.

Any endpoint that is intentionally left unauthenticated pending service-account support MUST be
documented as unauthenticated — in code and in the task ledger. Describing an anonymous endpoint
as authenticated is a governance violation in its own right, because it silently retires a
control that was never built.

Rationale: Centralizing auth in the backend removes a class of client-side security mistakes,
keeps a single enforcement point for RBAC, and ensures every management, kill-switch, and AI
approval action is authorized consistently regardless of which client initiated it. Separating
service accounts from human roles prevents the two failure modes that follow from conflating
them: granting an SDK key human privileges, or leaving the SDK surface open because no human
role fits it.

### VII. Contract-First API Security

The backend MUST expose a native OpenAPI specification (generated from the .NET 10 API, not
hand-maintained) as the authoritative contract for every client-facing endpoint. Both the
React and Flutter clients MUST be built against this generated specification, and CI MUST fail
the build on contract drift — any endpoint, schema, or field change that is not reflected in
the published OpenAPI spec and the client contract artifacts. Breaking contract changes MUST be
versioned explicitly.

The drift gate MUST be a real signal, which requires all three of:

- **Fail, never skip**: if a generator is unavailable for a client that HAS source in the
  repository, the gate MUST fail. Silently skipping generation makes the gate pass by comparing
  an unchanged directory, which reports enforcement while providing none. A client that does not
  yet exist (no source files) is exempt, and the exemption MUST be tied to the absence of that
  source — never to a date or a manual flag — so enforcement becomes mandatory automatically the
  moment the client acquires its first source file. While exempt, its lack of coverage MUST be
  stated explicitly wherever the gate reports success.
- **Pinned generators**: every generator version MUST be pinned. An unpinned generator makes the
  gate non-deterministic, so an upstream release surfaces as phantom contract drift on an
  unrelated change.
- **Environment-independent spec**: the committed specification MUST NOT embed
  environment-specific values (host, port, or `servers` URLs from the generating machine), which
  would otherwise produce drift that reflects where the generator ran rather than what changed.

Rationale: With two independent clients consuming the same API, contract drift is the most
likely source of silent runtime breakage. A native, generated OpenAPI spec enforced in CI makes
the backend the single source of truth and prevents React and Flutter from diverging from the
server or from each other — but only if the gate actually runs both generators deterministically.

## Technology Stack & Performance Standards

- Backend: .NET 10 Web API, Clean Architecture, EF Core 10 with SQLite (dev/edge) and
  PostgreSQL (production), Redis Pub/Sub for cache invalidation, Polly for resilience, custom
  JWT/RBAC auth, native OpenAPI generation.
- Persistence and schema provisioning:
  - **PostgreSQL is the single migrated provider.** EF Core migrations MUST be authored against
    PostgreSQL, because provider-specific DDL does not transfer — SQLite-authored migrations fail
    against PostgreSQL (e.g. error `42804`, a boolean expression assigned to an integer column).
  - **SQLite is a dev/edge convenience**, provisioned via `EnsureCreated`/schema creation rather
    than migrations. Maintaining dual migration sets for one model is explicitly NOT the approach.
  - Schema provisioning strategy MUST be configurable per environment, and the production path
    MUST NOT depend on a startup-time migration once Principle III's AOT goal is pursued.
- Web Dashboard: React with simplified hooks and lightweight state; always-online; typed API
  client generated from / validated against the backend OpenAPI spec.
- Mobile App: Flutter with Clean Architecture, BLoC/Cubit, HydratedBloc, Dio; offline-first;
  API client generated from / validated against the backend OpenAPI spec.
- Performance budgets (hard requirements, verified by benchmark/telemetry):
  - In-memory flag evaluation: < 5ms, zero steady-state allocations, MurmurHash3 bucketing.
  - Global kill-switch cache invalidation: < 100ms end-to-end via Redis Pub/Sub.
- Compatibility: hot-path assemblies (`OptiPulse.Evaluation.*`) MUST be Native AOT + trim
  analyzer clean with warnings as errors (Principle III); nullable enabled everywhere. Whole-host
  AOT publish is a gated Phase 8 goal, not a standing property.
- Security: all protected endpoints require a valid backend-issued credential — a JWT for human
  users, a service-account credential for machine callers; RBAC enforced server-side; signing
  secrets never leave the backend and are never committed.

## Development Workflow & Quality Gates

- Architecture: changes MUST preserve Clean Architecture dependency direction in the backend and
  Flutter client; violations block merge. The React dashboard is held to Principle V's
  lightweight standard instead.
- Performance: any change touching evaluation, hashing, caching, or invalidation MUST include a
  benchmark demonstrating the relevant budget (< 5ms / < 100ms / zero-alloc).
- Resilience: new external dependencies MUST ship with a Polly policy and a documented fail-safe
  behavior, and the policy MUST be wired to a call site (Principle IV's no-decorative-pipelines
  rule). Hot-path code is exempt by design, not by omission.
- Authentication: protected endpoints MUST enforce credential validation and RBAC server-side;
  PRs adding endpoints MUST declare the required role or service-account scope. Endpoints
  intentionally left anonymous MUST say so explicitly. No auth logic may be added to clients.
- Contracts: the native OpenAPI spec MUST be regenerated and the client contract artifacts
  updated in the same change; CI MUST fail on contract drift between backend, React, and Flutter,
  and MUST fail rather than skip when a generator is missing.
- Time source: production code MUST NOT call `DateTime.Now`, `DateTime.UtcNow`,
  `DateTimeOffset.Now`, or `DateTimeOffset.UtcNow`. Inject `TimeProvider` instead, so timestamps
  are controllable in tests. Startup composition (the point where `TimeProvider.System` is
  registered) is the only exception.
- Gates enforce what this document says: an automated gate MUST actually detect every pattern the
  constitution names. A gate that names a rule it does not check is worse than no gate, because it
  reports compliance that was never verified. Adding a banned pattern to this document REQUIRES
  extending the corresponding gate in the same change.
- Testing: Domain and Application logic MUST have unit tests; cross-layer and inter-service
  contracts MUST have integration tests.
- Task ledger integrity: a task MUST NOT be marked complete unless the artifact it claims exists.
  A completed checkbox is a factual assertion that later planning depends on.
- Reviews: every PR MUST verify compliance with Principles I–VII; deviations MUST be justified
  in writing and approved, or the PR is rejected.

### Deployment & First-Run Bootstrap

- **The deployable artifact is a container image**, and it MUST be built by CI. A build file that
  nothing exercises rots silently, and the first time anyone discovers it is when a deployment is
  already needed.
- **Initial credentials MUST come from configuration**, never from a literal in source, and MUST
  NOT have a default. Outside Development the platform MUST refuse to bootstrap rather than
  invent a credential. A demonstration deployment is a public deployment: shipping a known
  account behind an endpoint that can operate the kill-switch is the same exposure as shipping
  it in production, and "it is only a demo" is exactly the reasoning that produces the incident.
- **Bootstrap MUST be idempotent** — it establishes accounts only into an empty environment and
  MUST NOT modify existing ones. A restart is not an authorization event.
- **Cross-origin access MUST be an explicit allowlist** drawn from configuration. A wildcard
  origin on an API that carries bearer credentials and privileged operations is prohibited.
- **Runtime configuration MUST be supplied by the environment**, and secrets MUST NOT be committed
  in any form (Principle VI already forbids committing the signing key; this extends the same rule
  to database credentials, Redis credentials, and bootstrap credentials).
- **First-request latency on a suspending host is a hosting property, not a performance claim.**
  Published performance evidence MUST come from the benchmark gate (Principle II), and a
  demonstration deployment MUST NOT be presented as evidence for or against those budgets.

### Adopted Toolchain & Practice Baselines

The project adopts three best-practice kits (kept under `.kits/`): the .NET kit (backend),
the Flutter kit (mobile), and general Claude Code practices (repo tooling). The following are
baseline requirements derived from them:

- **Dev hooks (required)**: format-on-write, a secret-blocking pre-write guard, and a
  destructive-command (force-push / `reset --hard` / `rm -rf`) guard MUST be active
  (`.claude/settings.json` + `.claude/hooks/`).
- **Anti-pattern gate (backend)**: code MUST NOT use `DateTime.Now`/`DateTime.UtcNow` or
  `DateTimeOffset.Now`/`DateTimeOffset.UtcNow` directly (use `TimeProvider`), `async void`, or
  `new HttpClient()` (use `IHttpClientFactory`). The Result pattern is used for expected failures;
  broad `catch` is disallowed (reinforces Principle IV error handling).
- **Package/tooling baseline (backend)**: Polly via `Microsoft.Extensions.Http.Resilience`,
  xUnit v2 + Testcontainers (no in-memory DB) for integration-first tests, Serilog +
  OpenTelemetry, `Asp.Versioning`, `TimeProvider`; native OpenAPI (no Swashbuckle); versions via
  `Directory.Packages.props` (never hardcoded). The Roslyn navigator MCP is available for
  code navigation/analysis.
  - `Asp.Versioning` MUST be adopted for API versioning rather than hand-written `/api/v1`
    route prefixes, and MUST be introduced while the endpoint surface is small — retrofitting
    versioning across a large surface is materially more expensive.
- **Architecture pin**: backend MUST use Clean Architecture (the kits' VSA/DDD/modular options
  are not used); auth MUST be custom JWT/RBAC (not ASP.NET Identity/external OIDC).
- **Hot-path exclusion**: the flag-evaluation hot path MUST NOT route through mediator, cache-aside,
  resilience pipelines, or other indirection layers (protects Principle II); such patterns are for
  management/CRUD paths only.
- **Client pins**: Flutter state = BLoC/Cubit + HydratedBloc only (no Riverpod/Provider/ChangeNotifier
  for app state). React state = **Redux Toolkit** or hooks/context, maintainer's choice (v2.4.0);
  if Redux is used it MUST be Redux Toolkit rather than hand-written reducers and boilerplate.
  Flutter networking = Dio; both clients' API models MUST be OpenAPI-generated.
- **React styling**: **Tailwind CSS** (adopted v2.4.0; the constitution previously said nothing
  about styling). Utility classes in components are expected and are not an architecture concern.

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

When this document and the codebase disagree, that disagreement MUST be resolved explicitly in
one direction — by fixing the code, or by amending this document with rationale — and never by
leaving the contradiction in place. A principle the code silently violates provides no
governance while still implying that it does.

**Version**: 2.4.0 | **Ratified**: 2026-08-15 | **Last Amended**: 2026-08-19
