---

description: "Task list for OptiPulse Platform implementation"
---

# Tasks: OptiPulse Platform

**Input**: Design documents from `/specs/001-optipulse-platform/`

**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), [contracts/](contracts/)

**Tests**: INCLUDED — the constitution's Development Workflow & Quality Gates mandate benchmarks (zero-alloc / sub-5ms), unit tests, and integration/contract tests; the spec defines Independent Tests and acceptance scenarios. Test tasks are therefore first-class here.

**Organization**: Tasks are grouped by user story (from spec.md) to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: US1–US5 mapping to spec.md user stories
- Exact file paths included per task

## Path Conventions (from plan.md — Web + Mobile + API)

- Backend: `backend/src/<Project>/`, `backend/tests/<Project>/`
- Web Dashboard (React): `web/optipulse_dashboard/src/`
- Mobile App (Flutter): `mobile/optipulse_app/lib/`
- Contract generation: `contracts-gen/`

## Execution Order at a Glance

Phases run **top to bottom in this document** — that is the build order. Task IDs are stable
identifiers, **not** sequence numbers: they are cited by source-code comments, commit messages and
the constitution, so they are never renumbered when a phase is inserted. That is why Phase 4a and
Phase 5a carry high numbers while sitting early — both were added after the original plan, to close
gaps found in work already built.

| Order | Phase | Delivers | Tasks | Done |
|-------|-------|----------|-------|------|
| 1 | **Phase 1** | Setup | T001–T008d | ✅ |
| 2 | **Phase 2** | Foundational | T009–T017 | ✅ |
| 3 | **Phase 3** | Real-Time Flag Evaluation for Applications | T018–T031 | ✅ |
| 4 | **Phase 4** | Authentication & Role-Based Access Control | T032–T041a | ✅ |
| 5 | **Phase 4a** | Constitution v2.2.0 Remediation | T091–T095 | 4/5 |
| 6 | **Phase 5** | Manager Web Dashboard: Flags, Experiments, Micro-Copy & Analytics | T042–T062 | 9/21 |
| 7 | **Phase 5a** | Deployment Enablement | T096–T100 | 0/5 |
| 8 | **Phase 6** | Admin & DevOps Mobile App: Telemetry, Push Alerts & Instant Kill-Switch | T063–T077 | 0/15 |
| 9 | **Phase 7** | Immutable Audit Trail | T078–T083 | 0/6 |
| 10 | **Phase 8** | Polish & Cross-Cutting Concerns | T084–T090 | 0/7 |

**Where you are now**: Phases 1–4 complete. Phase 4a complete. Phase 5 backend complete (flags, experiments, analytics); its React screens and Phase 5a remain. The AI Gateway (T043, T052–T054) is deferred post-MVP.
---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Repository and toolchain initialization across all three codebases.

- [X] T001 Create repository structure (`backend/`, `web/`, `mobile/`, `contracts-gen/`) per [plan.md](plan.md)
- [X] T002 Initialize .NET 10 solution `backend/OptiPulse.sln` with per-context projects (SharedKernel, IdentityAccess, EvaluationEngine.{Domain,Application,Infrastructure}, FlagManagement.{…}, AiGateway.{…}, AuditTelemetry.{…}, OptiPulse.Api)
- [X] T003 [P] Enable `<Nullable>enable</Nullable>`, `<PublishAot>true</PublishAot>`, and `-warnaserror` (incl. AOT/trim analyzers) in `backend/Directory.Build.props`
- [X] T003a [P] Enable central package management: create `backend/Directory.Packages.props` with `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>` and pin the baseline packages (Polly via `Microsoft.Extensions.Http.Resilience`, EF Core 10 + Npgsql, StackExchange.Redis, Serilog + OpenTelemetry, `Asp.Versioning`, xUnit, Testcontainers) — no hardcoded versions in `.csproj` (constitution package baseline)
- [X] T003b Add the two baseline packages T003a claimed but never pinned (verified absent: no `Asp.Versioning` reference anywhere; `xunit` is 2.9.3). Add `Asp.Versioning.Http` to `backend/Directory.Packages.props` and adopt it in `OptiPulse.Api` **now, while only 7 endpoints exist** — Phase 5 adds ~15 more and retrofitting versioning across the larger surface is materially more expensive (constitution v2.2.0 package baseline). The xUnit v3 pin was corrected to v2 in the constitution instead of migrating, so no test-framework change is required
- [X] T004 [P] Scaffold React app `web/optipulse_dashboard/` with Vite + TypeScript; add Vitest + Testing Library; configure ESLint/Prettier (no Redux/MobX/Zustand deps)
- [X] T005 [P] Scaffold Flutter app `mobile/optipulse_app/` (iOS + Android targets only) with flutter_bloc, hydrated_bloc, dio, get_it/injectable
- [X] T006 [P] Configure backend test projects `backend/tests/OptiPulse.UnitTests`, `OptiPulse.IntegrationTests` (xUnit + FluentAssertions + Testcontainers), `OptiPulse.Evaluation.Benchmarks` (BenchmarkDotNet)
- [X] T007 [P] Add `contracts-gen/generate.sh` skeleton (export OpenAPI → TS + Dart) and README per [contracts/openapi-pipeline.md](contracts/openapi-pipeline.md)
- [X] T008 [P] Add CI workflow skeleton running `dotnet build -warnaserror`, `dotnet test`, benchmark gate, and the OpenAPI drift check in `.github/workflows/ci.yml`
- [X] T008e [P] Wire the anti-pattern gate into CI/pre-commit: adapt `.kits/dotnet-claude-kit/hooks/pre-commit-antipattern.sh` (flags `DateTime.Now`/`DateTime.UtcNow`, `async void`, `new HttpClient()`) into `.github/workflows/ci.yml` and/or a git pre-commit hook so violations fail the build (constitution v2.1.0 anti-pattern gate)
- [X] T008a [P] Author backend `backend/CLAUDE.md` importing the adopted `.kits/dotnet-claude-kit` rules, pinning Clean Architecture + custom JWT/RBAC, and excluding Mediator/HybridCache from the evaluation hot path (per constitution v2.1.0 baselines)
- [X] T008b [P] Author `web/optipulse_dashboard/CLAUDE.md` — custom hooks + single AuthContext, **no Redux/MobX/Zustand/React Query**, OpenAPI-generated typed client (overrides the web kit's Zustand/React-Query guidance)
- [X] T008c [P] Author `mobile/optipulse_app/CLAUDE.md` importing the adopted `.kits/flutter-ai-rules` skills, pinning **BLoC/Cubit + HydratedBloc + Dio** and disabling Riverpod/Provider/ChangeNotifier for app state
- [X] T008d Verify dev tooling is active: `.claude/hooks/` (format-on-write, block-secrets, bash-guard) and the `cwm-roslyn-navigator` MCP resolve `backend/OptiPulse.sln`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure every user story depends on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T009 [P] Implement `Result`, guard clauses, and shared value objects in `backend/src/OptiPulse.SharedKernel/`
- [X] T010 Create a **per-context** EF Core 10 `DbContext` (`FlagsDbContext`, `AuditDbContext`, `IdentityDbContext`) — one per bounded context/subdomain, each in its own `backend/src/<Context>/Infrastructure/Persistence/` — over the shared physical database, with SQLite/PostgreSQL provider switching (config-driven) and compiled-model setup. No single shared `AppDbContext` (preserves context isolation, Principle I)
- [X] T011 Configure the migrations framework and initial empty migration; verify SQLite (dev) and Postgres (Testcontainers) both apply
- [X] T012 [P] Implement Redis connection + named Polly v8 resilience pipelines (timeout→retry→circuit-breaker) for Postgres/Redis in `backend/src/*/Infrastructure/Resilience/`
- [X] T012a Wire the T012 resilience pipelines to actual call sites — they are currently **registered with zero consumers**, which constitution v2.2.0 Principle IV now names a violation rather than compliance (verified: the only file referencing `ResiliencePipeline` is `ResilienceExtensions.cs` itself). Apply to the management/persistence and invalidation-publish paths; the evaluation hot path stays exempt by design (Principle II / IV hot-path exemption). Remove any pipeline that has no legitimate consumer
- [X] T013 [P] Configure ASP.NET Core Minimal API host, middleware pipeline, centralized error handling (Result + ProblemDetails), and structured logging + telemetry (**Serilog + OpenTelemetry**) in `backend/src/OptiPulse.Api/Program.cs`
- [X] T014 [P] Enable native OpenAPI document generation (`Microsoft.AspNetCore.OpenApi`) and expose `openapi.json` in `backend/src/OptiPulse.Api/`
- [X] T015 [P] Implement append-only audit store + `IAuditLog` interface (insert-only; no UPDATE/DELETE grants) in `backend/src/AuditTelemetry/{Application,Infrastructure}/` per [data-model.md](data-model.md)
- [X] T016 [P] Implement Testcontainers fixtures (Postgres + Redis) shared across integration tests in `backend/tests/OptiPulse.IntegrationTests/Fixtures/`
- [X] T017 Wire `contracts-gen/generate.sh` to the built `openapi.json` and add the CI drift gate (`git diff --exit-code` over spec + generated dirs) per [contracts/openapi-pipeline.md](contracts/openapi-pipeline.md)

**Checkpoint**: Foundation ready — user stories can now begin.

---

## Phase 3: User Story 1 - Real-Time Flag Evaluation for Applications (Priority: P1) 🎯 MVP

**Goal**: Deterministic, sub-5ms, zero-allocation flag evaluation with fail-safe behavior; emits exposure telemetry.

**Independent Test**: Configure a flag with a targeting rule + 50% rollout; evaluate 10k distinct contexts → 100% determinism, ±1pp distribution, p99 < 5ms, 0 B/eval; kill Postgres 5 min → decisions continue (FailSafe).

### Tests for User Story 1

- [X] T018 [P] [US1] BenchmarkDotNet suite asserting p99 < 5ms and **0 B** allocation/eval in `backend/tests/OptiPulse.Evaluation.Benchmarks/EvaluationBenchmarks.cs`
- [X] T019 [P] [US1] Unit tests for MurmurHash3 determinism + basis-point bucket distribution in `backend/tests/OptiPulse.UnitTests/Evaluation/BucketingTests.cs`
- [X] T020 [P] [US1] Contract test for `POST /api/v1/evaluate` (+ batch, snapshot/version) per [contracts/evaluation-api.md](contracts/evaluation-api.md) in `backend/tests/OptiPulse.IntegrationTests/Evaluation/EvaluationApiTests.cs`
- [X] T021 [P] [US1] Last-known-good fail-safe tests during simulated datastore outage in `backend/tests/OptiPulse.UnitTests/Evaluation/FailSafeTests.cs` — implemented at unit level (SnapshotStore + Evaluator), which is where the fail-safe property lives; a full 5-minute live container-outage test (quickstart V5) remains for Phase 8

### Implementation for User Story 1

- [X] T022 [P] [US1] Implement zero-allocation MurmurHash3 (x86-32) over `ReadOnlySpan<byte>` in `backend/src/EvaluationEngine/OptiPulse.Evaluation.Domain/Hashing/MurmurHash3.cs`
- [X] T023 [P] [US1] Implement `EvaluationContext`/`EvaluationResult` readonly structs in `backend/src/EvaluationEngine/OptiPulse.Evaluation.Domain/`
- [X] T024 [US1] Implement `CompiledFlag` + immutable `FlagSnapshot` (FrozenDictionary) in `backend/src/EvaluationEngine/OptiPulse.Evaluation.Domain/Snapshot/` (depends on T022, T023)
- [X] T025 [US1] Implement `IEvaluator` with targeting-rule matching, rollout bucketing, sticky variant assignment, and safe-default/unknown handling in `backend/src/EvaluationEngine/OptiPulse.Evaluation.Application/Evaluator.cs`
- [X] T026 [US1] Implement lock-free snapshot store (atomic reference swap) + last-known-good retention in `backend/src/EvaluationEngine/OptiPulse.Evaluation.Infrastructure/SnapshotStore.cs`
- [X] T027 [US1] Implement Redis Pub/Sub subscriber applying deltas with version/kill-switch precedence + periodic reconciliation backstop per [contracts/invalidation-channel.md](contracts/invalidation-channel.md) in `backend/src/EvaluationEngine/OptiPulse.Evaluation.Infrastructure/InvalidationSubscriber.cs`
- [X] T028 [P] [US1] Implement async `ExposureEvent` writer (bounded channel, off hot path) in `backend/src/AuditTelemetry/OptiPulse.Audit.Infrastructure/ExposureWriter.cs`
- [X] T029 [US1] Implement `VariantExposureCount` aggregation (windowed) in `backend/src/AuditTelemetry/OptiPulse.Audit.Application/ExposureAggregator.cs` (depends on T028)
- [X] T030 [US1] Map evaluation Minimal API endpoints (`/evaluate`, `/evaluate/batch`, `/snapshot/version`) in `backend/src/OptiPulse.Api/Endpoints/EvaluationEndpoints.cs` — ⚠️ endpoints are **anonymous**; the "service-account auth" originally claimed here does not exist and is now tracked as T041a (constitution v2.2.0 Principle VI requires this be stated, not implied)
- [X] T031 [US1] Emit exposure events from evaluation when under an experiment; wire logging/`reason` codes in `backend/src/OptiPulse.Api/Endpoints/EvaluationEndpoints.cs`

**Checkpoint**: US1 is a functional, benchmarked, fail-safe evaluation service — deployable MVP.

---

## Phase 4: User Story 2 - Authentication & Role-Based Access Control (Priority: P1)

**Goal**: Custom JWT auth (login, seamless rotating refresh, logout) with backend-enforced RBAC for `Manager` vs `Admin`.

**Independent Test**: Log in as Manager and Admin; expire access → refresh yields rotated pair with no re-login; Manager attempts Admin-only action → 403; reused/rotated refresh → 401 + family revoked; 0 unauthorized successes across the role matrix.

### Tests for User Story 2

- [X] T032 [P] [US2] Contract tests for `/auth/login|refresh|logout|me` per [contracts/auth-api.md](contracts/auth-api.md), including a login-latency assertion (< 5s, SC-009), in `backend/tests/OptiPulse.IntegrationTests/Auth/AuthApiTests.cs`
- [X] T033 [P] [US2] Integration test: RBAC matrix — Manager-only vs Admin-only endpoints return 403 for the wrong role, with audited attempts, in `backend/tests/OptiPulse.IntegrationTests/Auth/RbacMatrixTests.cs`
- [X] T034 [P] [US2] Unit test: refresh-token rotation + reuse detection revokes the family in `backend/tests/OptiPulse.UnitTests/Auth/RefreshRotationTests.cs`

### Implementation for User Story 2

- [X] T035 [P] [US2] Implement `User` + `Role` entities and password hashing in `backend/src/OptiPulse.IdentityAccess/Domain/`
- [X] T036 [P] [US2] Implement revocable `RefreshToken` entity + EF Core store (token-family rotation, reuse detection) in `backend/src/OptiPulse.IdentityAccess/Infrastructure/RefreshTokenStore.cs`
- [X] T037 [US2] Implement JWT issuance/validation service (claims per [contracts/auth-api.md](contracts/auth-api.md)) in `backend/src/OptiPulse.IdentityAccess/Application/TokenService.cs` (depends on T035, T036)
- [X] T038 [US2] Register JWT bearer authentication + `Manager`/`Admin` authorization policies in `backend/src/OptiPulse.Api/Auth/AuthConfiguration.cs`
- [X] T039 [US2] Map `/auth/login`, `/auth/refresh`, `/auth/logout`, `/auth/me` endpoints in `backend/src/OptiPulse.Api/Endpoints/AuthEndpoints.cs`
- [X] T040 [US2] Write audit entries for login success/failure, logout, and role-denied attempts via `IAuditLog` in `backend/src/OptiPulse.Api/Auth/` (uses T015)
- [X] T041 [US2] Apply authorization policies to human-facing endpoints and document required roles in `backend/src/OptiPulse.Api/Endpoints/` — scope corrected: this covers the **human** (`Manager`/`Admin`) surface only, which is what FR-A05 governs
- [X] T041a Implement **service-account credentials** for the runtime evaluation surface (`/evaluate`, `/evaluate/batch`, `/snapshot/version`) in `backend/src/OptiPulse.IdentityAccess/` — a distinct credential type from human users per constitution v2.2.0 Principle VI: holds no human role, scoped to evaluation + telemetry ingest only, independently revocable. Until this lands the endpoints remain anonymous and MUST be documented as such (blocks the Principle VI claim in T087)

**Checkpoint**: Full auth + RBAC enforced server-side; US3/US4 can now build authenticated flows.

---

## Phase 4a: Constitution v2.2.0 Remediation (do BEFORE Phase 5)

**Purpose**: Close the gaps a grill-with-docs review found in the Phases 1–4 build, where governance
text and the codebase disagreed. All findings were verified against the code, not inferred. These are
sequenced before Phase 5 because each gets materially more expensive once ~15 more endpoints and a
second client land.

**Goal**: The gates enforce what the constitution says, and no task claims work that does not exist.

- [X] T091 [P] Make the OpenAPI drift gate deterministic and non-vacuous (constitution v2.2.0 Principle VII) in `contracts-gen/generate.sh` + `.github/workflows/ci.yml`: (a) **pin** the generator — `npx --yes openapi-typescript` currently resolves the latest release at run time and is not in `package.json`, so an upstream release surfaces as phantom contract drift; (b) **strip environment-specific `servers`** from the committed spec, which currently hardcodes `http://localhost:5289/` and drifts for anyone using a non-default `OPTIPULSE_OPENAPI_PORT`; (c) **fail rather than skip** when a generator is unavailable. This is now urgent because the `protect-main` ruleset makes this gate a merge blocker
- [X] T092 [P] Enforce the time-source rule the gate only half-checked (constitution v2.2.0): extend `backend/scripts/check-antipatterns.sh` to catch `DateTimeOffset.Now`/`DateTimeOffset.UtcNow` (it currently matches only `DateTime\.(Now|UtcNow)`), then inject `TimeProvider` at the **5 verified production sites** that slip past it today — `Program.cs:205`, `SnapshotStore.cs:42`, `AuditLog.cs:17`, `ExposureWriter.cs:38`. Audit timestamps and snapshot times are currently unmockable. Verify the tightened gate with a negative control before relying on it
- [X] T093 Replace the SQLite-authored EF migrations with **PostgreSQL-authored** ones (constitution v2.2.0 persistence section). `MigrateAsync` against Postgres fails today (`42804`: `DefaultOutcome` integer column vs boolean expression), so the production provider has no working schema path — only the `EnsureCreated` test strategy hides it. Author migrations against Postgres for the Flags, Audit and Identity contexts; keep SQLite on schema-creation for dev/edge. Do **not** maintain dual migration sets
- [X] T094 [P] Install `openapi-generator` (requires a JRE) in the CI drift-gate job so the **Dart** client is actually generated in `.github/workflows/ci.yml`. Today generation silently skips whenever the tool is absent — which is always, in CI — so the gate passes by diffing an empty directory and Flutter can drift from the server freely. Prerequisite for T072; until it lands, Flutter contract enforcement does not exist and MUST NOT be described as if it does

- [ ] T095 [P] Declare the API's security schemes in the generated OpenAPI document (bearer JWT for human endpoints, the `X-OptiPulse-Key` header for the service-account surface) via an OpenAPI document transformer in `backend/src/OptiPulse.Api/`. Discovered while closing T041a: adding `RequireAuthorization` changed the runtime behaviour but produced **no diff in the published contract**, so generated clients still describe protected endpoints as if they were open. Principle VII makes the spec the authoritative contract, and right now it under-describes authentication

**Checkpoint**: Constitution v2.2.0 and the codebase agree; every gate detects every pattern it names.

---

## Phase 5: User Story 3 - Manager Web Dashboard: Flags, Experiments, Micro-Copy & Analytics (Priority: P1)

**Goal**: Managers create/control flags & experiments, generate/approve AI micro-copy, and review analytics from the always-online React dashboard.

**Independent Test**: As an authenticated Manager, create a flag + experiment, generate and approve micro-copy, attach an approved variant, open analytics — all succeed and reflect in evaluation/telemetry; dashboard requires connectivity when offline.

**Depends on**: US1 (evaluation/telemetry), US2 (auth/RBAC).

### Tests for User Story 3

- [X] T042 [P] [US3] Contract tests for management API (flags CRUD, versioning/If-Match concurrency, experiments, kill-switch) per [contracts/management-api.md](contracts/management-api.md) in `backend/tests/OptiPulse.IntegrationTests/Management/ManagementApiTests.cs`
- [ ] T043 [P] [US3] Contract tests for AI Gateway (generate, review/approve, attach-only-approved, degraded provider) per [contracts/ai-gateway-api.md](contracts/ai-gateway-api.md) in `backend/tests/OptiPulse.IntegrationTests/Ai/AiGatewayApiTests.cs` ⏸️ **DEFERRED post-MVP** (product decision, 2026-08-18): the AI Gateway is not part of the MVP. Micro-copy is authored by hand until it lands; the human approval gate (FR-016) stays as specified and is the behaviour a future AI Gateway must slot behind, not replace.
- [X] T044 [P] [US3] Integration test: concurrent-edit conflict returns 409 without silent overwrite in `backend/tests/OptiPulse.IntegrationTests/Management/ConcurrencyTests.cs`
- [ ] T045 [P] [US3] React hook/component tests (useFlags, useAuth, flag-create flow) in `web/optipulse_dashboard/test/`

### Implementation for User Story 3 — Backend (Flag Management + AI Gateway)

- [X] T046 [P] [US3] Implement `Flag`, `TargetingRule`, `Rollout` aggregate + state machine in `backend/src/FlagManagement/OptiPulse.Flags.Domain/`
- [X] T047 [P] [US3] Implement `Experiment` + `Variant` aggregate (weight validation = 100%) in `backend/src/FlagManagement/OptiPulse.Flags.Domain/`
- [X] T048 [US3] Implement `IFlagRepository` (EF Core) with optimistic concurrency on `Version` in `backend/src/FlagManagement/OptiPulse.Flags.Infrastructure/FlagRepository.cs` (depends on T046, T047)
- [X] T049 [US3] Implement `IInvalidationPublisher` (Redis Pub/Sub, publish after commit) in `backend/src/FlagManagement/OptiPulse.Flags.Infrastructure/InvalidationPublisher.cs`
- [X] T050 [US3] Implement flag/experiment CQRS use cases (create/edit/status/version-restore, experiment CRUD) with audit writes in `backend/src/FlagManagement/OptiPulse.Flags.Application/`
- [X] T051 [US3] Map management endpoints (flags, experiments, kill-switch, versions) with `Manager` policy (kill-switch `Admin`) in `backend/src/OptiPulse.Api/Endpoints/ManagementEndpoints.cs`
- [ ] T052 [P] [US3] Implement `IMicroCopyGenerator` port + `MicroCopyGenerationRequest`/`Candidate` with `Draft→Approved|Rejected` state machine in `backend/src/AiGateway/{Domain,Application}/` ⏸️ **DEFERRED post-MVP** (product decision, 2026-08-18): the AI Gateway is not part of the MVP. Micro-copy is authored by hand until it lands; the human approval gate (FR-016) stays as specified and is the behaviour a future AI Gateway must slot behind, not replace.
- [ ] T053 [US3] Implement swappable LLM provider adapter (Polly-wrapped, degraded status) in `backend/src/AiGateway/OptiPulse.Ai.Infrastructure/ProviderAdapter.cs` ⏸️ **DEFERRED post-MVP** (product decision, 2026-08-18): the AI Gateway is not part of the MVP. Micro-copy is authored by hand until it lands; the human approval gate (FR-016) stays as specified and is the behaviour a future AI Gateway must slot behind, not replace.
- [ ] T054 [US3] Map AI Gateway endpoints (generate, request fetch, review) + enforce approved-only attach in management, with audit writes, in `backend/src/OptiPulse.Api/Endpoints/AiGatewayEndpoints.cs` ⏸️ **DEFERRED post-MVP** (product decision, 2026-08-18): the AI Gateway is not part of the MVP. Micro-copy is authored by hand until it lands; the human approval gate (FR-016) stays as specified and is the behaviour a future AI Gateway must slot behind, not replace.
- [X] T055 [US3] Implement analytics/experiment telemetry read endpoint (reads aggregation from T029 + conversions) in `backend/src/OptiPulse.Api/Endpoints/TelemetryEndpoints.cs` ⚠️ shared file with T071/T082 — do not run in parallel; sequence T055→T071→T082

### Implementation for User Story 3 — React Web Dashboard

- [ ] T056 [US3] Generate TypeScript types + typed client from `openapi.json` into `web/optipulse_dashboard/src/api/` (via `contracts-gen/generate.sh`)
- [ ] T057 [P] [US3] Implement `AuthContext` (opaque session, silent refresh) in `web/optipulse_dashboard/src/context/AuthContext.tsx`
- [ ] T058 [P] [US3] Implement custom hooks `useFlags`, `useExperiments`, `useMicroCopy`, `useAnalytics` over the generated client in `web/optipulse_dashboard/src/hooks/`
- [ ] T059 [US3] Build Flags + Experiments management screens in `web/optipulse_dashboard/src/features/{flags,experiments}/` (depends on T056–T058)
- [ ] T060 [US3] Build Micro-Copy generation/approval screen in `web/optipulse_dashboard/src/features/microcopy/`
- [ ] T061 [US3] Build Analytics review screen in `web/optipulse_dashboard/src/features/analytics/`
- [ ] T062 [US3] Implement always-online guard (clear "requires connectivity" state, no stale editable state) in `web/optipulse_dashboard/src/components/ConnectivityGuard.tsx`

**Checkpoint**: Managers can run the full authoring→experiment→copy→analytics loop on the web dashboard.

---

## Phase 5a: Deployment Enablement (do BEFORE the React dashboard)

**Purpose**: Make the platform reachable from a browser on another origin, runnable as a
container, and usable in an empty environment — the three things that currently block any
deployment (verified: no CORS configuration, no Dockerfile, no way to create the first user).

**Sequenced before the dashboard deliberately**: CORS and a configurable API base URL are
dashboard-shaped decisions. Seven screens written against a same-origin assumption would each
need reworking afterwards, the same way retrofitting API versioning across a large endpoint
surface was avoided by adopting it at seven endpoints (T003b).

**Goal**: `docker run` + environment variables produces a working, loginable deployment.

- [X] T096 [P] Add an explicit CORS allowlist read from configuration (`Cors:AllowedOrigins`) in `backend/src/OptiPulse.Api/Program.cs`, applied before authentication middleware. Wildcard origins are prohibited (constitution v2.3.0) — this API carries bearer credentials and an Admin kill-switch. FR-031
- [X] T097 [P] Add a multi-stage `backend/Dockerfile` (SDK build → runtime image, non-root user) and a `docker build` step in `.github/workflows/ci.yml`, so the deployable artifact is exercised on every PR rather than discovered broken at deploy time (constitution v2.3.0). FR-035
- [X] T098 Implement idempotent first-run bootstrap in `backend/src/OptiPulse.Api/` — seeds one Manager, one Admin, and one service-account credential **only when the corresponding tables are empty**, entirely from configuration (`Bootstrap:*`). No default or literal credential; outside Development the host MUST refuse to bootstrap when configuration is absent rather than invent one. The generated service-account key is written to the log exactly once at startup, never persisted in plaintext. FR-032, FR-033, FR-034
- [X] T099 [P] Integration tests for bootstrap in `backend/tests/OptiPulse.IntegrationTests/Bootstrap/BootstrapTests.cs`: seeds into an empty environment; is a no-op on second start; does NOT modify an existing account; refuses to seed outside Development without configuration. FR-034
- [X] T100 [P] Make the React API base URL configurable via `VITE_API_URL` (falling back to same-origin for local dev) in `web/optipulse_dashboard/src/api/`, so the dashboard can be served from a different origin than the API. FR-031

**Checkpoint**: an empty PostgreSQL + Redis + the container image + environment variables yields a
deployment an operator can log into.

---

## Phase 6: User Story 4 - Admin & DevOps Mobile App: Telemetry, Push Alerts & Instant Kill-Switch (Priority: P1)

**Goal**: Admins monitor live telemetry, receive push alerts on critical events, and trigger instant kill-switches from an offline-tolerant Flutter app.

**Independent Test**: As an authenticated Admin, view live telemetry, trigger a critical event → push within 10s + present in in-app history; activate kill-switch → all nodes disabled within 100ms; go offline/reconnect → deterministic reconcile with kill-switch precedence.

**Depends on**: US1 (telemetry/kill-switch propagation), US2 (auth/RBAC).

### Tests for User Story 4

- [ ] T063 [P] [US4] Integration test: kill-switch from Admin reflects on all nodes < 100ms in `backend/tests/OptiPulse.IntegrationTests/Management/KillSwitchPropagationTests.cs`
- [ ] T064 [P] [US4] Contract tests for alerts (device register, history, ack) per [contracts/telemetry-audit-api.md](contracts/telemetry-audit-api.md) in `backend/tests/OptiPulse.IntegrationTests/Alerts/AlertsApiTests.cs`
- [ ] T065 [P] [US4] Integration test: alert persisted to history even when push delivery fails in `backend/tests/OptiPulse.IntegrationTests/Alerts/AlertHistoryTests.cs`
- [ ] T066 [P] [US4] Flutter bloc_test for kill-switch (pending→confirmed) and offline reconcile in `mobile/optipulse_app/test/`

### Implementation for User Story 4 — Backend (Alerting + Telemetry)

- [ ] T067 [P] [US4] Implement `Alert`/`CriticalEvent` + `PushDevice` entities and append-to-history store in `backend/src/AuditTelemetry/OptiPulse.Audit.{Domain,Infrastructure}/`
- [ ] T068 [US4] Implement critical-event detection (error-rate/anomalous-exposure/kill-switch-change) in `backend/src/AuditTelemetry/OptiPulse.Audit.Application/AlertDetector.cs`
- [ ] T069 [US4] Implement `IAlertNotifier` push adapter (FCM/APNs, Polly-wrapped) + durable-history fallback in `backend/src/AuditTelemetry/OptiPulse.Audit.Infrastructure/AlertNotifier.cs`
- [ ] T070 [US4] Map alert endpoints (`/alerts/devices`, `/alerts`, `/alerts/{id}/ack`) with `Admin` policy in `backend/src/OptiPulse.Api/Endpoints/AlertsEndpoints.cs`
- [ ] T071 [US4] Add live telemetry read endpoint (real-time exposures/health signals) in `backend/src/OptiPulse.Api/Endpoints/TelemetryEndpoints.cs` ⚠️ shared file with T055/T082 — do not run in parallel

### Implementation for User Story 4 — Flutter Mobile App

- [ ] T072 [US4] Generate Dart models + Dio client from `openapi.json` into `mobile/optipulse_app/lib/core/` (via `contracts-gen/generate.sh`)
- [ ] T073 [P] [US4] Implement auth feature (login, opaque token storage, silent refresh) in `mobile/optipulse_app/lib/features/auth/`
- [ ] T074 [P] [US4] Implement telemetry feature (live monitoring Cubit + HydratedBloc cache) in `mobile/optipulse_app/lib/features/telemetry/`
- [ ] T075 [P] [US4] Implement alerts feature (push handler + in-app history + ack) in `mobile/optipulse_app/lib/features/alerts/`
- [ ] T076 [US4] Implement kill-switch feature (Admin action, pending→confirmed, never silently lost) in `mobile/optipulse_app/lib/features/killswitch/` (depends on T072, T073)
- [ ] T077 [US4] Implement deterministic offline reconcile on reconnect with kill-switch precedence in `mobile/optipulse_app/lib/core/reconcile/`

**Checkpoint**: Admins can monitor, get alerted, and kill-switch from mobile, online or intermittently connected.

---

## Phase 7: User Story 5 - Immutable Audit Trail (Priority: P2)

**Goal**: A tamper-proof, queryable record of all changes/decisions across both clients, with correct actor+role and reconciled experiment telemetry.

**Independent Test**: Perform changes + kill-switch + AI approval from both clients; query audit → each appears exactly once with actor, role, timestamp, before/after; verify no mutation endpoint; exposures reconcile within 1%.

**Depends on**: foundational `IAuditLog` (T015); write-sites in US2/US3/US4.

### Tests for User Story 5

- [ ] T078 [P] [US5] Integration test: every change/kill-switch/approval/role-denied appears exactly once with actor+role; records immutable in `backend/tests/OptiPulse.IntegrationTests/Audit/AuditTrailTests.cs`
- [ ] T079 [P] [US5] Integration test: experiment exposure counts reconcile with evaluations within 1% in `backend/tests/OptiPulse.IntegrationTests/Telemetry/ReconciliationTests.cs`

### Implementation for User Story 5

- [ ] T080 [US5] Implement audit query endpoint (`GET /api/v1/audit`, filters + pagination, `Manager`/`Admin` read) in `backend/src/OptiPulse.Api/Endpoints/AuditEndpoints.cs`
- [X] T081 [US5] Enforce append-only immutability at the data layer (no UPDATE/DELETE grants; add DB-level guard/test) in `backend/src/AuditTelemetry/OptiPulse.Audit.Infrastructure/`
- [X] T082 [US5] Implement conversion ingest (`POST /api/v1/telemetry/conversions`, idempotent) in `backend/src/OptiPulse.Api/Endpoints/TelemetryEndpoints.cs` ⚠️ shared file with T055/T071 — do not run in parallel
- [ ] T083 [US5] Audit-coverage sweep: ensure every mutating use case across contexts writes an `AuditEntry` with `ActorRole` in `backend/src/*/Application/`

**Checkpoint**: Complete, immutable audit + reconciled telemetry.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Hardening and final validation across all stories.

- [ ] T084 [P] Run the OpenAPI drift gate end-to-end and commit regenerated TS + Dart clients; confirm CI fails on an intentional drift in `contracts-gen/`. Must also confirm the gate fails when a generator is **missing** (not just when the spec differs) — depends on T091 + T094
- [ ] T085 [P] Verify Native AOT publish of `backend/src/OptiPulse.Api` succeeds with zero trim/AOT warnings. **Blocked on two prerequisites discovered in Phase 4** (see the note in `backend/Directory.Build.props`): (a) generate compiled models via `dotnet ef dbcontext optimize` for the Flags, Audit and Identity contexts, because AOT sets `RuntimeFeature.IsDynamicCodeSupported=false` and EF Core then refuses runtime model building outright; and (b) move `MigrateAsync` off the startup path — migrations are `RequiresDynamicCode` (IL3050) and are not AOT-supported at all, so schema work must run as a separate non-AOT tool/job. `PublishAot` must stay unset until both are done: it is not publish-only, it changes the runtimeconfig of every build and prevents the host from starting
- [ ] T086 [P] Confirm benchmark gate is enforced in CI (fails on >5ms or >0 B) per [quickstart.md](quickstart.md) V2
- [ ] T087 [P] Security hardening pass: verify no signing secrets in clients, refresh tokens revocable, all protected endpoints carry policies
- [X] T088 [P] Documentation: update `README.md` and per-client run guides — DEPLOYMENT.md added (verified against the source: every environment variable, health path and GitHub secret in it was cross-checked against the code and the workflow rather than written from memory)
- [ ] T089 Execute full [quickstart.md](quickstart.md) validation scenarios V1–V12 and record results
- [ ] T090 [P] Architecture boundary tests asserting Clean Architecture dependency direction (no Domain→Infrastructure/Api references) and bounded-context isolation (no cross-context Domain references; each context uses its own DbContext) in `backend/tests/OptiPulse.UnitTests/Architecture/BoundaryTests.cs` (FR-023, Principle I)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: no dependencies — start immediately.
- **Foundational (Phase 2)**: depends on Setup — BLOCKS all user stories.
- **US1 (Phase 3)**: after Foundational. Pure MVP — no dependency on other stories.
- **US2 (Phase 4)**: after Foundational. Independent; US3/US4 authenticated flows depend on it.
- **Remediation (Phase 4a)**: after US2. Independent of the user stories, but sequenced before Phase 5
  because T003b (API versioning) and T091 (drift-gate determinism) get materially more expensive once
  Phase 5 adds ~15 endpoints and Phase 6 adds a second client.
- **US3 (Phase 5)**: after Foundational; consumes US1 (evaluation/telemetry) + US2 (auth).
- **US4 (Phase 6)**: after Foundational; consumes US1 (kill-switch/telemetry) + US2 (auth).
- **US5 (Phase 7)**: after Foundational; audit write-sites are populated by US2/US3/US4.
- **Polish (Phase 8)**: after all targeted stories complete.

### Within Each User Story

- Tests written to fail first → Domain models → Application services → Infrastructure → API endpoints → client UI.
- Models before services; services before endpoints; backend endpoints before client generation/UI.

### Parallel Opportunities

- Setup: T003–T008 in parallel.
- Foundational: T009, T012, T013, T014, T015, T016 in parallel (T010→T011 sequential).
- US1: tests T018–T021 parallel; domain T022/T023 parallel; T028 parallel to snapshot work.
- US2: tests T032–T034 parallel; entities T035/T036 parallel.
- US3: backend (T046/T047, T052 parallel) and React (T057/T058 parallel) tracks run concurrently after T056.
- US4: backend alerting (T067) and Flutter features (T073–T075 parallel) run concurrently after T072.
- Cross-team: once Foundational is done, US1 and US2 can be built fully in parallel; US3 and US4 in parallel once US1+US2 land.

---

## Parallel Example: User Story 1

```bash
# Tests (write first, expect fail):
Task: "BenchmarkDotNet sub-5ms/zero-alloc suite (T018)"
Task: "MurmurHash3 determinism + distribution unit tests (T019)"
Task: "Evaluation API contract tests (T020)"
Task: "Fail-safe outage integration test (T021)"

# Then domain in parallel:
Task: "MurmurHash3 span implementation (T022)"
Task: "EvaluationContext/EvaluationResult structs (T023)"
```

---

## Implementation Strategy

### MVP First (User Story 1)

1. Phase 1 Setup → 2. Phase 2 Foundational → 3. Phase 3 US1 → **STOP & VALIDATE** (benchmark + fail-safe) → deploy the evaluation engine as the MVP.

### Incremental Delivery

1. Setup + Foundational → foundation ready.
2. US1 (evaluation MVP) → validate → demo.
3. US2 (auth/RBAC) → validate → demo.
4. US3 (manager web dashboard) → validate → demo.
5. US4 (admin mobile app) → validate → demo.
6. US5 (audit trail) → validate → demo.

### Parallel Team Strategy

After Foundational: Team A → US1, Team B → US2. Once both land: Team C → US3 (web), Team D → US4 (mobile). US5 folds in after write-sites exist.

---

## Notes

- [P] = different files, no incomplete dependencies.
- Constitution gates are enforced by tasks: zero-alloc/sub-5ms (T018/T086), Native AOT (T003/T085), OpenAPI drift (T017/T084/T091/T094), backend-only auth (T038/T087), service-account auth (T041a), Polly coverage (T012a), time source (T092), API versioning (T003b), Postgres schema path (T093).
- US3/US4 legitimately depend on US2 (auth) and US1 (evaluation/telemetry) — documented above; each story remains independently testable behind those foundations.
- Commit after each task or logical group; verify tests fail before implementing.
- **Best-practice kit usage** (see [plan.md](plan.md) Development Tooling + [research.md](research.md) R13–R16):
  backend work uses `.kits/dotnet-claude-kit` skills/agents + Roslyn MCP (Clean Architecture pinned,
  custom JWT, no Mediator/HybridCache on the eval hot path); Flutter push (T069/T075) uses the
  `firebase-messaging` skill and mocking uses `mocktail` (not mockito); React work follows the
  constitution (no Zustand/React Query), not the web kit's state guidance.
