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

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Repository and toolchain initialization across all three codebases.

- [ ] T001 Create repository structure (`backend/`, `web/`, `mobile/`, `contracts-gen/`) per [plan.md](plan.md)
- [ ] T002 Initialize .NET 10 solution `backend/OptiPulse.sln` with per-context projects (SharedKernel, IdentityAccess, EvaluationEngine.{Domain,Application,Infrastructure}, FlagManagement.{…}, AiGateway.{…}, AuditTelemetry.{…}, OptiPulse.Api)
- [ ] T003 [P] Enable `<Nullable>enable</Nullable>`, `<PublishAot>true</PublishAot>`, and `-warnaserror` (incl. AOT/trim analyzers) in `backend/Directory.Build.props`
- [ ] T003a [P] Enable central package management: create `backend/Directory.Packages.props` with `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>` and pin the baseline packages (Polly via `Microsoft.Extensions.Http.Resilience`, EF Core 10 + Npgsql, StackExchange.Redis, Serilog + OpenTelemetry, `Asp.Versioning`, xUnit v3, Testcontainers) — no hardcoded versions in `.csproj` (constitution v2.1.0 package baseline)
- [ ] T004 [P] Scaffold React app `web/optipulse_dashboard/` with Vite + TypeScript; add Vitest + Testing Library; configure ESLint/Prettier (no Redux/MobX/Zustand deps)
- [ ] T005 [P] Scaffold Flutter app `mobile/optipulse_app/` (iOS + Android targets only) with flutter_bloc, hydrated_bloc, dio, get_it/injectable
- [ ] T006 [P] Configure backend test projects `backend/tests/OptiPulse.UnitTests`, `OptiPulse.IntegrationTests` (xUnit + FluentAssertions + Testcontainers), `OptiPulse.Evaluation.Benchmarks` (BenchmarkDotNet)
- [ ] T007 [P] Add `contracts-gen/generate.sh` skeleton (export OpenAPI → TS + Dart) and README per [contracts/openapi-pipeline.md](contracts/openapi-pipeline.md)
- [ ] T008 [P] Add CI workflow skeleton running `dotnet build -warnaserror`, `dotnet test`, benchmark gate, and the OpenAPI drift check in `.github/workflows/ci.yml`
- [ ] T008e [P] Wire the anti-pattern gate into CI/pre-commit: adapt `.kits/dotnet-claude-kit/hooks/pre-commit-antipattern.sh` (flags `DateTime.Now`/`DateTime.UtcNow`, `async void`, `new HttpClient()`) into `.github/workflows/ci.yml` and/or a git pre-commit hook so violations fail the build (constitution v2.1.0 anti-pattern gate)
- [ ] T008a [P] Author backend `backend/CLAUDE.md` importing the adopted `.kits/dotnet-claude-kit` rules, pinning Clean Architecture + custom JWT/RBAC, and excluding Mediator/HybridCache from the evaluation hot path (per constitution v2.1.0 baselines)
- [ ] T008b [P] Author `web/optipulse_dashboard/CLAUDE.md` — custom hooks + single AuthContext, **no Redux/MobX/Zustand/React Query**, OpenAPI-generated typed client (overrides the web kit's Zustand/React-Query guidance)
- [ ] T008c [P] Author `mobile/optipulse_app/CLAUDE.md` importing the adopted `.kits/flutter-ai-rules` skills, pinning **BLoC/Cubit + HydratedBloc + Dio** and disabling Riverpod/Provider/ChangeNotifier for app state
- [ ] T008d Verify dev tooling is active: `.claude/hooks/` (format-on-write, block-secrets, bash-guard) and the `cwm-roslyn-navigator` MCP resolve `backend/OptiPulse.sln`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure every user story depends on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T009 [P] Implement `Result`, guard clauses, and shared value objects in `backend/src/OptiPulse.SharedKernel/`
- [ ] T010 Create a **per-context** EF Core 10 `DbContext` (`FlagsDbContext`, `AuditDbContext`, `IdentityDbContext`) — one per bounded context/subdomain, each in its own `backend/src/<Context>/Infrastructure/Persistence/` — over the shared physical database, with SQLite/PostgreSQL provider switching (config-driven) and compiled-model setup. No single shared `AppDbContext` (preserves context isolation, Principle I)
- [ ] T011 Configure the migrations framework and initial empty migration; verify SQLite (dev) and Postgres (Testcontainers) both apply
- [ ] T012 [P] Implement Redis connection + named Polly v8 resilience pipelines (timeout→retry→circuit-breaker) for Postgres/Redis in `backend/src/*/Infrastructure/Resilience/`
- [ ] T013 [P] Configure ASP.NET Core Minimal API host, middleware pipeline, centralized error handling (Result + ProblemDetails), and structured logging + telemetry (**Serilog + OpenTelemetry**) in `backend/src/OptiPulse.Api/Program.cs`
- [ ] T014 [P] Enable native OpenAPI document generation (`Microsoft.AspNetCore.OpenApi`) and expose `openapi.json` in `backend/src/OptiPulse.Api/`
- [ ] T015 [P] Implement append-only audit store + `IAuditLog` interface (insert-only; no UPDATE/DELETE grants) in `backend/src/AuditTelemetry/{Application,Infrastructure}/` per [data-model.md](data-model.md)
- [ ] T016 [P] Implement Testcontainers fixtures (Postgres + Redis) shared across integration tests in `backend/tests/OptiPulse.IntegrationTests/Fixtures/`
- [ ] T017 Wire `contracts-gen/generate.sh` to the built `openapi.json` and add the CI drift gate (`git diff --exit-code` over spec + generated dirs) per [contracts/openapi-pipeline.md](contracts/openapi-pipeline.md)

**Checkpoint**: Foundation ready — user stories can now begin.

---

## Phase 3: User Story 1 - Real-Time Flag Evaluation for Applications (Priority: P1) 🎯 MVP

**Goal**: Deterministic, sub-5ms, zero-allocation flag evaluation with fail-safe behavior; emits exposure telemetry.

**Independent Test**: Configure a flag with a targeting rule + 50% rollout; evaluate 10k distinct contexts → 100% determinism, ±1pp distribution, p99 < 5ms, 0 B/eval; kill Postgres 5 min → decisions continue (FailSafe).

### Tests for User Story 1

- [ ] T018 [P] [US1] BenchmarkDotNet suite asserting p99 < 5ms and **0 B** allocation/eval in `backend/tests/OptiPulse.Evaluation.Benchmarks/EvaluationBenchmarks.cs`
- [ ] T019 [P] [US1] Unit tests for MurmurHash3 determinism + basis-point bucket distribution in `backend/tests/OptiPulse.UnitTests/Evaluation/BucketingTests.cs`
- [ ] T020 [P] [US1] Contract test for `POST /api/v1/evaluate` (+ batch, snapshot/version) per [contracts/evaluation-api.md](contracts/evaluation-api.md) in `backend/tests/OptiPulse.IntegrationTests/Evaluation/EvaluationApiTests.cs`
- [ ] T021 [P] [US1] Integration test: last-known-good fail-safe during simulated datastore outage in `backend/tests/OptiPulse.IntegrationTests/Evaluation/FailSafeTests.cs`

### Implementation for User Story 1

- [ ] T022 [P] [US1] Implement zero-allocation MurmurHash3 (x86-32) over `ReadOnlySpan<byte>` in `backend/src/EvaluationEngine/OptiPulse.Evaluation.Domain/Hashing/MurmurHash3.cs`
- [ ] T023 [P] [US1] Implement `EvaluationContext`/`EvaluationResult` readonly structs in `backend/src/EvaluationEngine/OptiPulse.Evaluation.Domain/`
- [ ] T024 [US1] Implement `CompiledFlag` + immutable `FlagSnapshot` (FrozenDictionary) in `backend/src/EvaluationEngine/OptiPulse.Evaluation.Domain/Snapshot/` (depends on T022, T023)
- [ ] T025 [US1] Implement `IEvaluator` with targeting-rule matching, rollout bucketing, sticky variant assignment, and safe-default/unknown handling in `backend/src/EvaluationEngine/OptiPulse.Evaluation.Application/Evaluator.cs`
- [ ] T026 [US1] Implement lock-free snapshot store (atomic reference swap) + last-known-good retention in `backend/src/EvaluationEngine/OptiPulse.Evaluation.Infrastructure/SnapshotStore.cs`
- [ ] T027 [US1] Implement Redis Pub/Sub subscriber applying deltas with version/kill-switch precedence + periodic reconciliation backstop per [contracts/invalidation-channel.md](contracts/invalidation-channel.md) in `backend/src/EvaluationEngine/OptiPulse.Evaluation.Infrastructure/InvalidationSubscriber.cs`
- [ ] T028 [P] [US1] Implement async `ExposureEvent` writer (bounded channel, off hot path) in `backend/src/AuditTelemetry/OptiPulse.Audit.Infrastructure/ExposureWriter.cs`
- [ ] T029 [US1] Implement `VariantExposureCount` aggregation (windowed) in `backend/src/AuditTelemetry/OptiPulse.Audit.Application/ExposureAggregator.cs` (depends on T028)
- [ ] T030 [US1] Map evaluation Minimal API endpoints (`/evaluate`, `/evaluate/batch`, `/snapshot/version`) with service-account auth in `backend/src/OptiPulse.Api/Endpoints/EvaluationEndpoints.cs`
- [ ] T031 [US1] Emit exposure events from evaluation when under an experiment; wire logging/`reason` codes in `backend/src/OptiPulse.Api/Endpoints/EvaluationEndpoints.cs`

**Checkpoint**: US1 is a functional, benchmarked, fail-safe evaluation service — deployable MVP.

---

## Phase 4: User Story 2 - Authentication & Role-Based Access Control (Priority: P1)

**Goal**: Custom JWT auth (login, seamless rotating refresh, logout) with backend-enforced RBAC for `Manager` vs `Admin`.

**Independent Test**: Log in as Manager and Admin; expire access → refresh yields rotated pair with no re-login; Manager attempts Admin-only action → 403; reused/rotated refresh → 401 + family revoked; 0 unauthorized successes across the role matrix.

### Tests for User Story 2

- [ ] T032 [P] [US2] Contract tests for `/auth/login|refresh|logout|me` per [contracts/auth-api.md](contracts/auth-api.md), including a login-latency assertion (< 5s, SC-009), in `backend/tests/OptiPulse.IntegrationTests/Auth/AuthApiTests.cs`
- [ ] T033 [P] [US2] Integration test: RBAC matrix — Manager-only vs Admin-only endpoints return 403 for the wrong role, with audited attempts, in `backend/tests/OptiPulse.IntegrationTests/Auth/RbacMatrixTests.cs`
- [ ] T034 [P] [US2] Unit test: refresh-token rotation + reuse detection revokes the family in `backend/tests/OptiPulse.UnitTests/Auth/RefreshRotationTests.cs`

### Implementation for User Story 2

- [ ] T035 [P] [US2] Implement `User` + `Role` entities and password hashing in `backend/src/OptiPulse.IdentityAccess/Domain/`
- [ ] T036 [P] [US2] Implement revocable `RefreshToken` entity + EF Core store (token-family rotation, reuse detection) in `backend/src/OptiPulse.IdentityAccess/Infrastructure/RefreshTokenStore.cs`
- [ ] T037 [US2] Implement JWT issuance/validation service (claims per [contracts/auth-api.md](contracts/auth-api.md)) in `backend/src/OptiPulse.IdentityAccess/Application/TokenService.cs` (depends on T035, T036)
- [ ] T038 [US2] Register JWT bearer authentication + `Manager`/`Admin` authorization policies in `backend/src/OptiPulse.Api/Auth/AuthConfiguration.cs`
- [ ] T039 [US2] Map `/auth/login`, `/auth/refresh`, `/auth/logout`, `/auth/me` endpoints in `backend/src/OptiPulse.Api/Endpoints/AuthEndpoints.cs`
- [ ] T040 [US2] Write audit entries for login success/failure, logout, and role-denied attempts via `IAuditLog` in `backend/src/OptiPulse.Api/Auth/` (uses T015)
- [ ] T041 [US2] Apply `[Authorize]` policies to existing evaluation service-account endpoints and document required roles in `backend/src/OptiPulse.Api/Endpoints/`

**Checkpoint**: Full auth + RBAC enforced server-side; US3/US4 can now build authenticated flows.

---

## Phase 5: User Story 3 - Manager Web Dashboard: Flags, Experiments, Micro-Copy & Analytics (Priority: P1)

**Goal**: Managers create/control flags & experiments, generate/approve AI micro-copy, and review analytics from the always-online React dashboard.

**Independent Test**: As an authenticated Manager, create a flag + experiment, generate and approve micro-copy, attach an approved variant, open analytics — all succeed and reflect in evaluation/telemetry; dashboard requires connectivity when offline.

**Depends on**: US1 (evaluation/telemetry), US2 (auth/RBAC).

### Tests for User Story 3

- [ ] T042 [P] [US3] Contract tests for management API (flags CRUD, versioning/If-Match concurrency, experiments, kill-switch) per [contracts/management-api.md](contracts/management-api.md) in `backend/tests/OptiPulse.IntegrationTests/Management/ManagementApiTests.cs`
- [ ] T043 [P] [US3] Contract tests for AI Gateway (generate, review/approve, attach-only-approved, degraded provider) per [contracts/ai-gateway-api.md](contracts/ai-gateway-api.md) in `backend/tests/OptiPulse.IntegrationTests/Ai/AiGatewayApiTests.cs`
- [ ] T044 [P] [US3] Integration test: concurrent-edit conflict returns 409 without silent overwrite in `backend/tests/OptiPulse.IntegrationTests/Management/ConcurrencyTests.cs`
- [ ] T045 [P] [US3] React hook/component tests (useFlags, useAuth, flag-create flow) in `web/optipulse_dashboard/test/`

### Implementation for User Story 3 — Backend (Flag Management + AI Gateway)

- [ ] T046 [P] [US3] Implement `Flag`, `TargetingRule`, `Rollout` aggregate + state machine in `backend/src/FlagManagement/OptiPulse.Flags.Domain/`
- [ ] T047 [P] [US3] Implement `Experiment` + `Variant` aggregate (weight validation = 100%) in `backend/src/FlagManagement/OptiPulse.Flags.Domain/`
- [ ] T048 [US3] Implement `IFlagRepository` (EF Core) with optimistic concurrency on `Version` in `backend/src/FlagManagement/OptiPulse.Flags.Infrastructure/FlagRepository.cs` (depends on T046, T047)
- [ ] T049 [US3] Implement `IInvalidationPublisher` (Redis Pub/Sub, publish after commit) in `backend/src/FlagManagement/OptiPulse.Flags.Infrastructure/InvalidationPublisher.cs`
- [ ] T050 [US3] Implement flag/experiment CQRS use cases (create/edit/status/version-restore, experiment CRUD) with audit writes in `backend/src/FlagManagement/OptiPulse.Flags.Application/`
- [ ] T051 [US3] Map management endpoints (flags, experiments, kill-switch, versions) with `Manager` policy (kill-switch `Admin`) in `backend/src/OptiPulse.Api/Endpoints/ManagementEndpoints.cs`
- [ ] T052 [P] [US3] Implement `IMicroCopyGenerator` port + `MicroCopyGenerationRequest`/`Candidate` with `Draft→Approved|Rejected` state machine in `backend/src/AiGateway/{Domain,Application}/`
- [ ] T053 [US3] Implement swappable LLM provider adapter (Polly-wrapped, degraded status) in `backend/src/AiGateway/OptiPulse.Ai.Infrastructure/ProviderAdapter.cs`
- [ ] T054 [US3] Map AI Gateway endpoints (generate, request fetch, review) + enforce approved-only attach in management, with audit writes, in `backend/src/OptiPulse.Api/Endpoints/AiGatewayEndpoints.cs`
- [ ] T055 [US3] Implement analytics/experiment telemetry read endpoint (reads aggregation from T029 + conversions) in `backend/src/OptiPulse.Api/Endpoints/TelemetryEndpoints.cs` ⚠️ shared file with T071/T082 — do not run in parallel; sequence T055→T071→T082

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
- [ ] T081 [US5] Enforce append-only immutability at the data layer (no UPDATE/DELETE grants; add DB-level guard/test) in `backend/src/AuditTelemetry/OptiPulse.Audit.Infrastructure/`
- [ ] T082 [US5] Implement conversion ingest (`POST /api/v1/telemetry/conversions`, idempotent) in `backend/src/OptiPulse.Api/Endpoints/TelemetryEndpoints.cs` ⚠️ shared file with T055/T071 — do not run in parallel
- [ ] T083 [US5] Audit-coverage sweep: ensure every mutating use case across contexts writes an `AuditEntry` with `ActorRole` in `backend/src/*/Application/`

**Checkpoint**: Complete, immutable audit + reconciled telemetry.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Hardening and final validation across all stories.

- [ ] T084 [P] Run the OpenAPI drift gate end-to-end and commit regenerated TS + Dart clients; confirm CI fails on an intentional drift in `contracts-gen/`
- [ ] T085 [P] Verify Native AOT publish of `backend/src/OptiPulse.Api` succeeds with zero trim/AOT warnings
- [ ] T086 [P] Confirm benchmark gate is enforced in CI (fails on >5ms or >0 B) per [quickstart.md](quickstart.md) V2
- [ ] T087 [P] Security hardening pass: verify no signing secrets in clients, refresh tokens revocable, all protected endpoints carry policies
- [ ] T088 [P] Documentation: update `README.md` and per-client run guides
- [ ] T089 Execute full [quickstart.md](quickstart.md) validation scenarios V1–V12 and record results
- [ ] T090 [P] Architecture boundary tests asserting Clean Architecture dependency direction (no Domain→Infrastructure/Api references) and bounded-context isolation (no cross-context Domain references; each context uses its own DbContext) in `backend/tests/OptiPulse.UnitTests/Architecture/BoundaryTests.cs` (FR-023, Principle I)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: no dependencies — start immediately.
- **Foundational (Phase 2)**: depends on Setup — BLOCKS all user stories.
- **US1 (Phase 3)**: after Foundational. Pure MVP — no dependency on other stories.
- **US2 (Phase 4)**: after Foundational. Independent; US3/US4 authenticated flows depend on it.
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
- Constitution gates are enforced by tasks: zero-alloc/sub-5ms (T018/T086), Native AOT (T003/T085), OpenAPI drift (T017/T084), backend-only auth (T038/T087).
- US3/US4 legitimately depend on US2 (auth) and US1 (evaluation/telemetry) — documented above; each story remains independently testable behind those foundations.
- Commit after each task or logical group; verify tests fail before implementing.
- **Best-practice kit usage** (see [plan.md](plan.md) Development Tooling + [research.md](research.md) R13–R16):
  backend work uses `.kits/dotnet-claude-kit` skills/agents + Roslyn MCP (Clean Architecture pinned,
  custom JWT, no Mediator/HybridCache on the eval hot path); Flutter push (T069/T075) uses the
  `firebase-messaging` skill and mocking uses `mocktail` (not mockito); React work follows the
  constitution (no Zustand/React Query), not the web kit's state guidance.
