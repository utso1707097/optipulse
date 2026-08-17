# Implementation Plan: OptiPulse Platform

**Branch**: `001-optipulse-platform` | **Date**: 2026-08-15 (rev. dual-client + auth + OpenAPI) | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-optipulse-platform/spec.md`

## Summary

OptiPulse is a high-performance feature-flagging, A/B testing, and AI micro-copy platform. The
backend is a .NET 10 Web API structured with Clean Architecture and four DDD bounded contexts
(Evaluation Engine, Flag Management, AI Gateway, Audit & Telemetry) plus a supporting Identity &
Access subdomain. The runtime evaluation path is a zero-allocation, in-memory decision engine using
deterministic MurmurHash3 bucketing to hit a sub-5ms budget; configuration changes and emergency
kill-switches propagate to all nodes in <100ms over Redis Pub/Sub. EF Core 10 persists authoritative
state to SQLite (dev) / PostgreSQL (prod); Polly guards every outbound dependency.

Two purpose-built, differentiated clients (constitution Principle V) consume the backend:

- **Web Dashboard (React)** — an always-online, lightweight console for Product & Marketing
  managers: flag creation, experiment management, micro-copy generation/approval, analytics.
  Simplified React with standard custom hooks and lightweight state — **no Redux/MobX/Zustand**.
- **Mobile App (Flutter)** — an offline-first app for Admin & DevOps engineers: real-time
  telemetry, push notifications on critical events, and instant kill-switch. Clean Architecture,
  BLoC/Cubit, HydratedBloc, Dio. **iOS/Android only — no Flutter Web.**

Authentication is a custom JWT scheme with RBAC contained strictly in the backend (Principle VI):
access + rotating refresh tokens, roles `Manager` and `Admin`, all decisions server-side; clients
treat tokens as opaque. The backend emits a native OpenAPI spec (Principle VII) that drives a
code-generation pipeline producing **TypeScript types for React** and **Dart models for Flutter**,
with a CI drift gate so neither client can diverge from the server contract.

## Technical Context

**Language/Version**: C# 14 on .NET 10 (backend); TypeScript 5.x on React 18/19 (web); Dart 3.x on
Flutter 3.x (mobile)

**Primary Dependencies**:
- Backend: ASP.NET Core Minimal APIs, `Microsoft.AspNetCore.OpenApi` (native OpenAPI),
  ASP.NET Core JWT bearer auth + authorization policies, EF Core 10, StackExchange.Redis,
  Polly v8, source-generated System.Text.Json, BenchmarkDotNet
- Web: React + Vite + TypeScript, custom hooks + React Context (auth/session only), generated typed
  API client; **no Redux/MobX/Zustand**
- Mobile: flutter_bloc, hydrated_bloc, dio, get_it/injectable, a push-notification plugin behind an
  abstraction
- Contract tooling: OpenAPI generators — TypeScript (`openapi-typescript`/Kiota) and Dart
  (`openapi-generator`/Kiota)

**Storage**: PostgreSQL (prod authoritative), SQLite (dev/test/edge); Redis for Pub/Sub + cache;
in-process immutable snapshot for evaluation; refresh-token store (EF Core, revocable)

**Testing**: xUnit + FluentAssertions + Testcontainers (Postgres/Redis); BenchmarkDotNet zero-alloc
gate; Flutter `flutter_test` + `bloc_test`; React Vitest + Testing Library; contract-drift check in
CI

**Target Platform**: Linux containers (backend, Native AOT–published); modern browsers (React web
dashboard); iOS + Android (Flutter mobile). **Flutter Web is not a target.**

**Project Type**: Web + Mobile + API — .NET Web API backend, React web dashboard, Flutter mobile app

**Performance Goals**: p99 flag evaluation < 5ms in-memory, zero steady-state allocations;
kill-switch / cache invalidation < 100ms across nodes; deterministic bucketing (100% reproducible);
rollout within ±1pp; login < 5s; critical-event push < 10s

**Constraints**: Native AOT + trim compatible backend (no unbounded reflection/dynamic/runtime
codegen on hot paths); nullable enabled; AOT/trim warnings fail the build; evaluation fails safe to
last-known-good; kill-switch fails safe to "off"; auth/authorization server-side only; clients hold
no signing secrets; CI fails on OpenAPI contract drift

**Scale/Scope**: High-throughput multi-node evaluation (≥100k eval/sec/node), global distribution;
four bounded contexts + Identity & Access; two clients

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design. (Constitution v2.0.0)*

| Principle | Requirement | Plan compliance |
|-----------|-------------|-----------------|
| I. Clean Architecture & Layer Discipline | Inward-only deps; framework behind interfaces; React exempt but API behind typed client | Backend Domain/Application/Infrastructure/Api per context; Flutter layered; React keeps API access behind the generated typed client layer. **PASS** |
| II. Zero-Allocation Performance (NON-NEGOTIABLE) | <5ms eval, MurmurHash3, zero-alloc, benchmark-gated | Lock-free immutable snapshot, MurmurHash3 over spans; BenchmarkDotNet gate asserts 0 B + sub-5ms. **PASS** |
| III. .NET 10 Modern Standards (NON-NEGOTIABLE) | .NET 10, Native AOT, modern C#, warnings=errors | `PublishAot=true`, nullable, source-gen JSON, no dynamic on hot path; AOT/trim warnings = errors. **PASS** |
| IV. Resilience & Fail-Safe Kill-Switch | Polly on all deps; <100ms Redis Pub/Sub; fail-safe last-known-good | Polly v8 pipelines on Postgres/Redis/AI/push; kill-switch precedence over Pub/Sub; last-known-good snapshot. **PASS** |
| V. Dual-Client Strategy | React always-online + lightweight; Flutter offline-first Clean Arch; no client auth logic | React (custom hooks, no Redux, always-online) + Flutter (BLoC/HydratedBloc, offline-first, iOS/Android only); both consume governed contract; neither holds auth logic. **PASS** |
| VI. Backend-Contained Auth | Custom JWT/RBAC server-side; opaque tokens; no client secrets | JWT issuance/validation + authorization policies in backend; rotating refresh tokens; roles Manager/Admin; clients opaque. **PASS** |
| VII. Contract-First API Security | Native OpenAPI authoritative; CI fails on drift across clients | `Microsoft.AspNetCore.OpenApi` emits spec; TS + Dart generated from it; CI regenerate-and-diff gate. **PASS** |

**Initial gate: PASS** (no violations; Complexity Tracking not required).

**Post-Design re-check (after Phase 1): PASS** — see note at end of file.

## Project Structure

### Documentation (this feature)

```text
specs/001-optipulse-platform/
├── plan.md              # This file
├── research.md          # Phase 0 output (adds R9 React, R10 Auth/JWT, R11 OpenAPI, R12 Push)
├── data-model.md        # Phase 1 output (adds Identity & Access, Alert entities)
├── quickstart.md        # Phase 1 output (adds React + auth + push scenarios)
├── contracts/           # Phase 1 output
│   ├── auth-api.md              # NEW: JWT login/refresh/logout + RBAC payload schema
│   ├── evaluation-api.md
│   ├── management-api.md
│   ├── ai-gateway-api.md
│   ├── telemetry-audit-api.md   # includes push/alert subscription + mobile kill-switch
│   ├── invalidation-channel.md
│   └── openapi-pipeline.md      # NEW: generator pipeline + CI drift gate
└── tasks.md             # Phase 2 output (/speckit-tasks — NOT created here)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── OptiPulse.SharedKernel/              # Cross-context value objects, Result, guards
│   ├── OptiPulse.IdentityAccess/            # SUPPORTING: JWT issuance/validation, RBAC policies,
│   │                                        #   refresh-token store, role definitions
│   ├── EvaluationEngine/{Domain,Application,Infrastructure}/
│   ├── FlagManagement/{Domain,Application,Infrastructure}/
│   ├── AiGateway/{Domain,Application,Infrastructure}/
│   ├── AuditTelemetry/{Domain,Application,Infrastructure}/  # + alerting/push publisher
│   └── OptiPulse.Api/                       # ASP.NET Core host: Minimal APIs, auth middleware,
│                                            #   native OpenAPI doc, DI composition, AOT root
├── tests/
│   ├── OptiPulse.Evaluation.Benchmarks/     # zero-alloc + sub-5ms gate
│   ├── OptiPulse.UnitTests/
│   └── OptiPulse.IntegrationTests/          # Testcontainers: Postgres + Redis + auth flows
└── OptiPulse.sln

web/                                          # React Web Dashboard (managers) — always-online
└── optipulse_dashboard/
    ├── src/
    │   ├── api/                              # GENERATED TypeScript types + thin fetch client
    │   ├── hooks/                            # custom hooks (useFlags, useExperiments, useAuth, ...)
    │   ├── context/                          # AuthContext (session/token) — the only global state
    │   ├── features/{flags,experiments,microcopy,analytics}/
    │   └── components/
    └── test/                                 # Vitest + Testing Library

mobile/                                       # Flutter Mobile App (admin/devops) — offline-first
└── optipulse_app/                            # iOS + Android only (NOT web)
    ├── lib/
    │   ├── core/                             # DI (get_it), Dio config, generated Dart models, result types
    │   └── features/
    │       ├── auth/{domain,data,presentation}/
    │       ├── telemetry/{domain,data,presentation}/     # real-time monitoring
    │       ├── alerts/{domain,data,presentation}/        # push notifications + history
    │       └── killswitch/{domain,data,presentation}/    # instant kill-switch
    └── test/                                 # bloc_test + widget tests

contracts-gen/                                # OpenAPI codegen config + CI drift check (Principle VII)
```

**Structure Decision**: Web + Mobile + API. The backend is a single .NET solution: four DDD bounded
contexts each with Domain/Application/Infrastructure, plus a supporting `OptiPulse.IdentityAccess`
project owning the custom JWT/RBAC concern (kept out of the four product contexts but reused by the
Api host, satisfying Principle VI). The **React** dashboard (`web/`) is deliberately lightweight —
custom hooks over a generated typed client, with `AuthContext` as the only cross-cutting state and
no Redux (Principle V). The **Flutter** app (`mobile/`) is offline-first, iOS/Android only, feature-
first Clean Architecture. A `contracts-gen/` workspace holds the OpenAPI generator configuration and
the CI drift gate that keeps both clients in lockstep with the backend (Principle VII).

## Development Tooling & Best-Practice Kits

Three best-practice kits are vendored under `.kits/` and adopted per the constitution's
Adopted Toolchain & Practice Baselines (v2.1.0). Full analysis in
[research.md](research.md) R13–R16.

**Live now (this repo):**
- `.claude/settings.json` + `.claude/hooks/`: **format-on-write**, **block-secrets** (pre-write),
  **bash-guard** (destructive-command block); plus a permissions allowlist for common build/test.
- `.mcp.json`: **cwm-roslyn-navigator** MCP (15 read-only code-navigation/analysis tools;
  auto-detects `backend/OptiPulse.sln` once it exists).

**Backend — `.kits/dotnet-claude-kit` (adopt / adapt / skip):**
- Adopt: 10 coding/security/perf/testing rules; `clean-architecture`, `ef-core`, `openapi`,
  `resilience`, `testing`, `modern-csharp`, DI/config/logging skills; 10 specialist sub-agents
  (architect, api-designer, ef-core-specialist, test-engineer, security-auditor, performance-analyst,
  code-reviewer, build-error-resolver, refactor-cleaner, devops-engineer); Roslyn MCP.
- Adapt: pin architecture-advisor → **Clean Architecture**; auth skill → **custom JWT/RBAC** (not
  Identity/OIDC); caching → **Redis Pub/Sub + fail-safe LKG** (not HybridCache tag-invalidation);
  extend OpenAPI with the **contract-drift CI gate**; add **AOT + warnings-as-errors** vetting.
- Skip: VSA/DDD/modular templates; **Mediator/HybridCache on the evaluation hot path** (Principle II);
  Aspire/Blazor/worker templates.
- Build custom (kit is silent): MurmurHash3 zero-alloc engine, Redis Pub/Sub bus, fail-safe LKG,
  AOT gate, OpenAPI drift gate.

**Mobile — `.kits/flutter-ai-rules` (adopt / adapt / skip):**
- Adopt: `bloc`, `flutter-best-practices`, `architecture-feature-first`, `testing`, `mocktail`,
  `patrol-e2e-testing`, `flutter-errors`, `effective-dart`, `accessibility`, `code-review`;
  `firebase-messaging` (push), `firebase-crashlytics`, `firebase-remote-config`.
- Adapt: architecture examples ChangeNotifier→**Cubit**, always include domain layer, **http→Dio**;
  API contracts → **OpenAPI→Dart codegen**.
- Skip: `riverpod`, `provider`, `flutter-change-notifier` (app state), `mockito`, Flutter-Web
  guidance, `revenuecat`, unused Firebase skills.

**Web — `.kits/claude-code-best-practices` (tooling only, not architecture):**
- Adopt: the two hooks (already wired) + permission model; adaptable `component-new` /
  `test-component` React skills.
- Skip/strip: its Zustand + React Query state guidance (violates Principle V); it provides no
  OpenAPI-client guidance. React architecture stays sourced from the constitution.

## Complexity Tracking

> No constitution violations — section intentionally empty.
