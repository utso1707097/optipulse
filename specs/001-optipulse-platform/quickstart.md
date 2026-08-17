# Quickstart & Validation Guide: OptiPulse Platform

**Date**: 2026-08-15 (rev. dual-client + auth + OpenAPI) | **Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

This guide proves the feature works end-to-end. It references [contracts/](contracts/) and
[data-model.md](data-model.md) rather than restating them. Implementation code lives in the
implementation phase, not here.

## Prerequisites

- .NET 10 SDK; Docker (Testcontainers: PostgreSQL + Redis)
- Node.js 20+ (React Web Dashboard)
- Flutter 3.x SDK targeting **iOS/Android** (Flutter Web is not a target)
- Redis + Postgres available (or via Testcontainers)
- OpenAPI generators available (`openapi-typescript`/Kiota for TS; `openapi-generator`/Kiota for Dart)

## Setup

```bash
# Backend
cd backend
dotnet restore
dotnet build -warnaserror        # AOT/trim + nullable warnings fail the build (Principle III)
dotnet run --project src/OptiPulse.Api

# Contract generation (Principle VII) — regenerate spec + both clients
cd ../contracts-gen && ./generate.sh    # exports openapi.json → TS types (web) + Dart models (mobile)

# React Web Dashboard (managers)
cd ../web/optipulse_dashboard && npm ci && npm run dev

# Flutter Mobile App (admin/devops) — iOS/Android
cd ../../mobile/optipulse_app && flutter pub get
```

## Validation Scenarios

Each scenario maps to a user story / success criterion. Run against a live API instance.

### V1 — Deterministic evaluation & rollout (US1, FR-001/002, SC-003)
Create a 50% flag; evaluate 10,000 distinct `contextKey`s. **Expect**: 100% determinism on repeat;
enabled share within ±1pp. See [evaluation-api.md](contracts/evaluation-api.md).

### V2 — Sub-5ms budget & zero allocation (US1, SC-001, Principle II)
Run BenchmarkDotNet suite. **Expect**: p99 < 5ms and **0 B** allocated/eval (gate fails CI otherwise).

### V3 — Targeting rules (US1, FR-003)
Rule `country In [US]` → US matches (`reason: TargetingMatch`), non-US → default.

### V4 — Kill-switch propagation < 100ms (US4, FR-009/027, SC-002)
With ≥2 nodes, an **Admin** engages the kill-switch **from the Mobile App**. **Expect**: all nodes
return disabled (`reason: KillSwitch`) within 100ms; action attributed + audited. See
[invalidation-channel.md](contracts/invalidation-channel.md).

### V5 — Fail-safe on datastore outage (US1, FR-005, SC-004)
Stop Postgres 5 min during evaluations. **Expect**: last-known-good decisions (`reason: FailSafe`),
zero evaluation failures.

### V6 — Sticky experiment assignment (US3, FR-004)
Repeated + post-weight-change evaluation of the same `contextKey` keeps its variant.

### V7 — AI generation + approval gate (US3, FR-016, SC-007)
Manager generates ≥3 candidates; attaching a `Draft` → **422**; approve then attach → **succeeds**;
provider outage → `providerStatus: Degraded`, platform stays responsive. See
[ai-gateway-api.md](contracts/ai-gateway-api.md).

### V8 — Immutable audit & telemetry reconciliation (US5, FR-019/020, SC-006/008)
Changes from both clients + exposures. `GET /api/v1/audit`: every change once, with **actor + role**
+ timestamp; no mutation endpoint. Telemetry reconciles within 1%.

### V9 — Authentication, refresh & RBAC (US2, FR-A01–A07, SC-009/010)
1. `POST /api/v1/auth/login` as Manager and as Admin → each gets a token pair (login < 5s).
2. Let the access token expire; `POST /api/v1/auth/refresh` → new pair, **rotated** refresh token;
   work continues with zero forced re-login (SC-009).
3. Manager calls `POST /api/v1/flags/{key}/kill-switch` (Admin-only) → **403**, no state change,
   attempt audited.
4. Admin calls `POST /api/v1/flags` (Manager-only) → **403** per policy.
5. Reuse a rotated/logged-out refresh token → **401**, family revoked.
6. **Expect**: 0 unauthorized successes across the role matrix (SC-010). See
   [auth-api.md](contracts/auth-api.md).

### V10 — OpenAPI contract drift gate (FR-030, Principle VII)
Change an endpoint's schema in the backend **without** regenerating clients; run the CI pipeline.
**Expect**: `git diff --exit-code` over `openapi.json` + generated TS/Dart dirs is non-empty →
**build fails**. Regenerate → passes. See [openapi-pipeline.md](contracts/openapi-pipeline.md).

### V11 — Manager Web Dashboard (US3, FR-029, Principle V)
In the React app: log in as Manager, create a flag + experiment, generate/approve micro-copy, open
analytics. **Expect**: all succeed via generated typed client + custom hooks (no Redux store); when
offline, the dashboard clearly requires connectivity rather than showing stale editable state.

### V12 — Admin Mobile push + offline reconcile (US4, FR-026/028, SC-011)
Register a device (`POST /api/v1/alerts/devices`); trigger a critical event. **Expect**: push within
10s **and** the alert present in `GET /api/v1/alerts` history even if push is dropped. Go offline,
reconnect → cached telemetry/flag state reconciles deterministically; kill-switch state takes
precedence.

## Test Commands

```bash
# Backend unit + integration (Testcontainers: Postgres + Redis + auth flows)
cd backend && dotnet test

# Performance gate (zero-alloc + sub-5ms)
dotnet run -c Release --project tests/OptiPulse.Evaluation.Benchmarks

# Contract drift gate
cd ../contracts-gen && ./generate.sh && git diff --exit-code

# React Web Dashboard
cd ../web/optipulse_dashboard && npm test

# Flutter Mobile App
cd ../../mobile/optipulse_app && flutter test
```

## Done / Acceptance

All twelve scenarios pass, the benchmark reports 0 B/sub-5ms, the OpenAPI drift gate is green, and
`dotnet build -warnaserror` succeeds → the plan's Constitution Check (Principles I–VII) is
empirically satisfied and the feature is ready for `/speckit-tasks`.
