# Phase 0 Research: OptiPulse Platform

**Date**: 2026-08-15 | **Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

The stack is pinned by the constitution and the user input, so no `NEEDS CLARIFICATION` markers
remained. This document records the key technical decisions, their rationale, and rejected
alternatives that shape Phase 1 design.

## R1. Deterministic bucketing — MurmurHash3, zero-allocation

- **Decision**: Bucket contexts with MurmurHash3 (x86, 32-bit) over the UTF-8 bytes of
  `"{flagKey}:{salt}:{contextKey}"`, mapping the hash to a `[0,10000)` bucket for basis-point
  precision. Implemented as a `static` method operating on `ReadOnlySpan<byte>` with `stackalloc`
  scratch and no heap allocation.
- **Rationale**: MurmurHash3 is fast, well-distributed, and stateless, giving identical results on
  every node without shared coordination (satisfies FR-001 determinism, SC-003). Basis points
  allow ±1pp rollout accuracy (SC-003). Span-based implementation meets Principle II (zero-alloc).
- **Alternatives considered**: SHA-256 (cryptographic, ~10x slower, allocates) — rejected, over-kill
  for non-adversarial bucketing; xxHash (faster but our budget already met and MurmurHash3 is
  constitution-mandated); `GetHashCode()` (non-deterministic across runs/nodes) — rejected.

## R2. Sub-5ms evaluation via immutable in-memory snapshot

- **Decision**: Hold all active flag/experiment configuration in an immutable snapshot object
  (`FlagSnapshot`) referenced by a single `volatile` field; readers take the reference with no lock.
  Updates build a new snapshot and swap the reference atomically (copy-on-write). Lookups use a
  frozen dictionary keyed by flag key.
- **Rationale**: Lock-free reads keep the hot path allocation-free and contention-free (Principle II,
  SC-001). Immutability makes last-known-good trivially available during outages (FR-005, SC-004).
- **Alternatives considered**: Per-request DB/Redis read (blows the 5ms budget, fails on outage) —
  rejected; `ConcurrentDictionary` mutated in place (reader/writer tearing, harder determinism) —
  rejected in favor of atomic snapshot swap; `FrozenDictionary` chosen over `Dictionary` for read
  throughput.

## R3. <100ms global invalidation over Redis Pub/Sub

- **Decision**: Flag Management publishes a compact change/kill-switch message to a Redis Pub/Sub
  channel (`optipulse:flags:invalidate`) on every committed change. Each API node subscribes and,
  on receipt, refreshes the affected entries and swaps the snapshot. Kill-switch messages carry a
  `killSwitch=true` flag applied with precedence before any re-enable.
- **Rationale**: Pub/Sub fan-out is sub-millisecond within a region; the budget is dominated by
  snapshot rebuild, kept small by applying deltas (FR-009, SC-002). Kill-switch precedence gives
  fail-safe "off" even if messages arrive out of order (edge case: partial invalidation).
- **Alternatives considered**: DB polling (latency + load) — rejected; Redis keyspace notifications
  (coupled to key layout, less explicit) — rejected; message bus (Kafka/RabbitMQ) — heavier
  operationally than the <100ms requirement needs; Pub/Sub is sufficient and already in the stack.
- **Note**: Pub/Sub is at-most-once. Mitigation: nodes periodically (e.g., every N seconds)
  reconcile against the authoritative store as a backstop; a monotonically increasing snapshot
  version detects and heals missed messages without weakening the <100ms happy path.

## R4. Resilience — Polly v8 pipelines + fail-safe fallback

- **Decision**: Wrap Postgres (EF Core), Redis, and the AI provider each in a named Polly v8
  `ResiliencePipeline` (timeout → retry with jittered backoff → circuit breaker). Evaluation never
  calls these on the hot path; when a background refresh fails, the current snapshot is retained
  (last-known-good). AI Gateway uses a shorter timeout and a circuit breaker that returns a clear
  degraded status.
- **Rationale**: Directly satisfies Principle IV and FR-005/FR-017. Separating the hot path from
  I/O means dependency failure degrades freshness, never availability.
- **Alternatives considered**: Retry-only (no breaker → cascading load) — rejected; custom
  resilience code — rejected in favor of the constitution-mandated Polly.

## R5. Persistence — EF Core 10, SQLite/PostgreSQL, AOT-safe

- **Decision**: EF Core 10 as the authoritative store behind `IFlagRepository` / audit / telemetry
  interfaces (owned by Application layers). Provider chosen by configuration: SQLite for dev/test,
  PostgreSQL for production. Use the EF Core compiled-model + AOT-compatible configuration; keep EF
  strictly in Infrastructure so Domain/Application stay reflection-free.
- **Rationale**: One ORM, two providers matches the constitution; compiled models keep startup fast
  and reduce reflection for AOT (Principle III). Repository interfaces preserve Clean Architecture.
- **Context isolation**: each bounded context/subdomain owns its **own** `DbContext`
  (`FlagsDbContext`, `AuditDbContext`, `IdentityDbContext`) over the shared physical database —
  **not** a single shared `AppDbContext` — so contexts stay isolated per Clean Architecture
  (Principle I). Cross-context references remain by ID only (see [data-model.md](data-model.md)).
- **Alternatives considered**: Dapper (more manual, but EF Core 10 AOT support is now viable and the
  constitution specifies EF Core) — rejected; separate stores per provider — unnecessary; a single
  shared `AppDbContext` spanning all contexts — rejected (couples the bounded contexts, violates
  Principle I).
- **Open consideration for tasks**: EF Core AOT support has known trim edges; the evaluation hot
  path avoids EF entirely, so any residual reflection is confined to management/audit paths that are
  not latency-critical. Integration tests via Testcontainers validate both providers.

## R6. Immutable audit + exposure telemetry

- **Decision**: Audit entries are append-only (insert-only table, no update/delete permission at the
  data layer) capturing actor, timestamp, before/after JSON, and change type. Exposure events are
  written asynchronously off the hot path via a bounded in-memory channel drained by a background
  writer, then aggregated per variant.
- **Rationale**: Append-only satisfies immutability (FR-019, SC-006). Async exposure capture keeps
  evaluation within budget while still reconciling within 1% (SC-008).
- **Alternatives considered**: Synchronous exposure writes (breaks 5ms budget) — rejected;
  event-sourcing the whole platform (over-engineered for v1) — rejected.

## R7. AI Gateway — swappable provider + human-approval gate

- **Decision**: Define an `IMicroCopyGenerator` port in `OptiPulse.Ai.Application`; implement a
  provider adapter in Infrastructure (Polly-wrapped, provider name configurable). Generated
  candidates enter a `Draft` state and require an explicit approval transition before they can be
  attached to an experiment (`Draft → Approved | Rejected`). No candidate in a non-`Approved` state
  is ever attachable or servable.
- **Rationale**: Port/adapter keeps the LLM provider an implementation detail (spec Assumptions,
  FR-016); the state machine enforces the human-in-the-loop gate (FR-016, SC-007).
- **Alternatives considered**: Direct provider SDK calls in the API layer (leaks vendor into inner
  layers) — rejected; auto-publish generated copy — rejected (violates approval requirement).

## R8. Flutter mobile app (Admin/DevOps) — Clean Architecture + offline reconciliation

- **Decision**: Flutter targets **iOS and Android only — not Flutter Web** — and serves the
  Admin/DevOps audience (telemetry monitoring, push alerts, instant kill-switch). Feature-first
  Clean Architecture (domain/data/presentation). Cubits for simple screen state, Blocs for
  multi-event flows; HydratedBloc persists last-known telemetry/flag view; Dio behind a repository.
  On reconnect, the client fetches the authoritative snapshot version and replaces local state
  deterministically, with kill-switch state taking precedence.
- **Rationale**: Matches Principle V (offline-first mobile) and FR-024/FR-028; deterministic
  replacement avoids merge ambiguity. Flutter Web is dropped because the always-online operator
  console is served by the React dashboard (R9), so a second web renderer would duplicate scope and
  violate the "differentiated clients" intent.
- **Alternatives considered**: setState/Provider (weaker separation) — rejected; local-write
  conflict merging (ambiguous, unnecessary for a read-mostly client) — rejected; **Flutter Web for
  the dashboard — rejected** in favor of a purpose-built lightweight React app (R9).

## R9. React Web Dashboard (Managers) — simplified hooks, no Redux

- **Decision**: Build the manager console as a lightweight React + TypeScript app (Vite). State is
  managed with **standard custom hooks** (`useFlags`, `useExperiments`, `useMicroCopy`,
  `useAnalytics`, `useAuth`) wrapping the generated typed API client; the only cross-cutting global
  state is a small `AuthContext` holding the opaque session. **No Redux/MobX/Zustand** and no
  offline persistence — the dashboard is always-online (Principle V, FR-029).
- **Rationale**: The dashboard operates on reliable networks and reads server truth directly;
  heavy client state would over-engineer it. Custom hooks keep data-fetching colocated and simple,
  and TypeScript types generated from OpenAPI (R11) prevent contract drift.
- **Alternatives considered**: Redux Toolkit / Zustand (unnecessary global store for a read-mostly,
  always-online console) — rejected per Principle V; Flutter Web (duplicate renderer, heavier) —
  rejected; server-driven UI (over-engineered for v1) — rejected. A lightweight query cache
  (e.g., TanStack Query) MAY be adopted inside the custom hooks if needed, but is not required and
  introduces no global store.

## R10. Custom JWT authentication + RBAC (backend-contained)

- **Decision**: Implement authentication in the backend `OptiPulse.IdentityAccess` subdomain using
  ASP.NET Core JWT bearer auth. Login issues a **short-lived access token** (~15 min) plus a
  **longer-lived rotating refresh token** (~7–30 days) stored server-side (EF Core) so it can be
  revoked; refresh rotates the token and detects reuse. Authorization uses ASP.NET Core policies
  mapping the two roles (`Manager`, `Admin`) to permitted operations; every protected endpoint
  declares its required policy. Clients treat both tokens as opaque and hold no signing secret
  (Principle VI, FR-A01–A06).
- **Rationale**: Centralizing issuance/validation/RBAC server-side gives one enforcement point and
  removes client-side auth mistakes. Rotating, revocable refresh tokens allow seamless session
  continuation (SC-009) without long-lived bearer exposure. Role-denied attempts are audited
  (FR-A07, SC-006).
- **Alternatives considered**: External IdP / OAuth2 provider (constitution mandates a custom
  backend scheme; no external IdP required) — rejected; access-token-only, no refresh (forces
  re-login, fails SC-009) — rejected; stateless non-revocable refresh (cannot revoke on logout)
  — rejected in favor of a revocable server-side store.
- **Token claims**: `sub` (user id), `role` (`Manager`|`Admin`), `name`, `iat`, `exp`, `jti`. RBAC
  decisions read `role`; clients never parse claims for authorization. Full schema in
  [contracts/auth-api.md](contracts/auth-api.md).

## R11. OpenAPI contract pipeline — TypeScript + Dart generation

- **Decision**: The .NET 10 API produces a **native OpenAPI document** via
  `Microsoft.AspNetCore.OpenApi` (emitted at build/CI to `contracts-gen/openapi.json`). A generator
  pipeline turns that single source of truth into client artifacts:
  - **React**: TypeScript types (and a thin typed fetch client) via `openapi-typescript` (or Kiota)
    → `web/optipulse_dashboard/src/api/`.
  - **Flutter**: Dart models (and a Dio-based client) via `openapi-generator` (`dart-dio`) or Kiota
    → `mobile/optipulse_app/lib/core/` generated models.
  A **CI drift gate** regenerates `openapi.json` and both client artifacts and fails the build if
  the working tree changes (i.e., someone changed an endpoint without regenerating), enforcing
  Principle VII and FR-030.
- **Rationale**: One generated spec keeps React and Flutter from diverging from the server or each
  other; the regenerate-and-diff gate makes drift a hard build failure rather than a runtime bug.
- **Alternatives considered**: Hand-written client models (drift-prone) — rejected; Swashbuckle
  (reflection-heavy, weaker AOT story than the native .NET 10 OpenAPI) — rejected; runtime schema
  validation only (catches drift too late) — rejected in favor of build-time generation + gate.
- **Versioning**: breaking contract changes are versioned explicitly (path or media-type version);
  the gate distinguishes additive (safe) from breaking changes for reviewer attention.

## R12. Push notifications for critical events (Admin mobile)

- **Decision**: The Audit & Telemetry context detects defined critical events (e.g., error-rate
  spike, anomalous exposure pattern, kill-switch state change) and publishes alerts. Delivery to the
  Flutter app uses a standard push provider (FCM/APNs) behind an `IAlertNotifier` abstraction
  (Polly-wrapped). Every alert is ALSO persisted to an in-app alert history so critical state is
  never conveyed solely by a possibly-undelivered push (FR-026, SC-011, edge case).
- **Rationale**: Push gives Admins timely awareness on the move; the durable history is the
  fail-safe when a device is offline or the push is dropped.
- **Alternatives considered**: Polling-only (misses the <10s alert budget, drains battery) —
  rejected; push-only with no history (loses alerts on delivery failure) — rejected; email/SMS
  (out of scope for v1) — deferred.

## R13. Adopt the .NET best-practice kit — Clean Architecture pinned

- **Decision**: Adopt `.kits/dotnet-claude-kit` for backend rules, skills, the 10 specialist
  sub-agents, and the Roslyn navigator MCP. Pin its architecture advisor to **Clean Architecture**
  (its ADR-001 "VSA default" is superseded by ADR-005 multi-architecture; the `clean-architecture`
  skill ships the exact Domain/Application/Infrastructure/Api layout Principle I mandates). Adopt its
  Result pattern, anti-pattern rules (no `DateTime.Now`/`async void`/`new HttpClient()`), and package
  baseline (Polly via `Microsoft.Extensions.Http.Resilience`, xUnit v3 + Testcontainers, Serilog +
  OpenTelemetry, `Asp.Versioning`, native OpenAPI).
- **Rationale**: The kit reinforces the constitution and removes boilerplate; the feared VSA conflict
  is unfounded once pinned to CA.
- **Alternatives considered**: adopt kit defaults unmodified (would offer VSA/Identity/HybridCache —
  conflicts) — rejected; ignore the kit and hand-roll conventions — rejected (loses proven tooling).
- **Guardrails**: keep the evaluation hot path free of Mediator/HybridCache indirection (Principle II);
  redirect the auth skill from ASP.NET Identity/OIDC to **custom JWT/RBAC** (Principle VI); replace the
  kit's HybridCache tag-invalidation with **Redis Pub/Sub + fail-safe LKG** (Principle IV); vet EF
  Core/Serilog/FluentValidation for **AOT/trim** before use on AOT paths (Principle III).

## R14. Adopt the Flutter kit — BLoC-only, Dio, OpenAPI

- **Decision**: Adopt `.kits/flutter-ai-rules` skills that fit: `bloc` (with HydratedCubit),
  `flutter-best-practices`, `architecture-feature-first`, `testing`, `mocktail`, `patrol-e2e-testing`,
  `flutter-errors`, `effective-dart`, `accessibility`; plus `firebase-messaging` (push notifications,
  FR-026), `firebase-crashlytics` (crash telemetry), `firebase-remote-config`.
- **Rationale**: The kit's default state solution is already **BLoC/Cubit + freezed** and it ships
  explicit HydratedCubit + offline-first guidance, directly reinforcing Principle V. Firebase
  messaging/crashlytics map onto OptiPulse's push + telemetry needs.
- **Alternatives considered**: enable the kit wholesale — rejected (its `riverpod`/`provider`/
  `change-notifier` skills contradict the BLoC mandate). 
- **Guardrails**: disable `riverpod`/`provider`/`change-notifier` for app state; treat kit examples'
  `http.Client` as **Dio**; keep the domain layer always (kit treats it as optional); ignore the kit's
  anti-OpenAPI opinion — API models are **OpenAPI→Dart generated** (Principle VII); drop Flutter-Web
  guidance.

## R15. Web kit provides tooling, not React architecture

- **Decision**: Use `.kits/claude-code-best-practices` only for repo tooling hygiene (the
  block-secrets + format-on-write hooks, permission model, Conventional Commits) and adaptable
  `component-new`/`test-component` React skills. **Do not** adopt its state-management guidance.
- **Rationale**: The kit is a generic Claude Code docs wiki, not a React best-practices set; its React
  reference material prescribes **Zustand + React Query**, forbidden by Principle V, and it has no
  OpenAPI-client guidance. React architecture therefore stays sourced from the constitution: custom
  hooks + a single `AuthContext`, always-online, OpenAPI-generated typed client.
- **Alternatives considered**: adopt its React starter as-is — rejected (banned libraries, no OpenAPI).

## R16. Live development tooling wiring

- **Decision**: Wire `.claude/settings.json` with three hooks — **format-on-write** (PostToolUse
  Write|Edit; dispatches `dotnet format` / `prettier` / `dart format` by extension), **block-secrets**
  (PreToolUse; blocks AWS keys, private keys, tokens, hardcoded passwords — Principle VI), and
  **bash-guard** (PreToolUse Bash; blocks force-push / `reset --hard` / `rm -rf`) — plus a permissions
  allowlist for common build/test commands. Register the **cwm-roslyn-navigator** MCP in `.mcp.json`.
- **Rationale**: Enforces the constitution's Adopted Toolchain baselines automatically; the Roslyn MCP
  gives token-efficient code navigation for the backend once the solution exists.
- **Alternatives considered**: per-stack settings in each app folder — deferred (a single repo-level
  config dispatching by file type is simpler for now); wiring later during implementation — rejected
  (the user opted to wire now).

## Resolved Unknowns Summary

| Item | Resolution |
|------|------------|
| Bucketing algorithm | MurmurHash3 x86-32 over span, basis-point buckets (R1) |
| Hitting <5ms | Lock-free immutable snapshot, no hot-path I/O (R2) |
| <100ms invalidation | Redis Pub/Sub + delta apply + version backstop (R3) |
| Dependency failure behavior | Polly pipelines + last-known-good snapshot (R4) |
| Storage/provider strategy | EF Core 10, SQLite dev / Postgres prod, AOT-confined (R5) |
| Audit immutability & exposure cost | Append-only + async channel writer (R6) |
| AI provider coupling & approval | Port/adapter + Draft→Approved state machine (R7) |
| Mobile (Admin) offline model | Flutter iOS/Android, HydratedBloc + version reconcile (R8) |
| Web (Manager) client | React + custom hooks, no Redux, always-online (R9) |
| Authentication & RBAC | Custom JWT, rotating revocable refresh, backend policies (R10) |
| Client contract drift | Native OpenAPI → TS + Dart codegen + CI drift gate (R11) |
| Critical-event alerting | Detect → push (FCM/APNs) behind abstraction + durable history (R12) |
| Backend best-practice kit | Adopt .NET kit, pin Clean Architecture, custom JWT, Redis invalidation (R13) |
| Mobile best-practice kit | Adopt Flutter kit BLoC/firebase skills, skip Riverpod/Provider, Dio (R14) |
| Web best-practice kit | Tooling hygiene only; React arch stays from constitution (R15) |
| Dev tooling wiring | settings.json hooks + Roslyn MCP live in-repo (R16) |

**Removed from prior revision**: Flutter Web as a target (the always-online console is now the
React dashboard, R9). No blocking clarifications remain; all items required for Phase 1 are resolved.
