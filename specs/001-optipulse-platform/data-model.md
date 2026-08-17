# Phase 1 Data Model: OptiPulse Platform

**Date**: 2026-08-15 | **Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

Entities are grouped by bounded context. Each context owns its aggregates; cross-context references
use identifiers only (no shared object graph), preserving DDD boundaries (FR-023).

---

## Identity & Access (supporting subdomain — backend only)

Custom JWT authentication + RBAC contained strictly in the backend (Constitution Principle VI). See
[contracts/auth-api.md](contracts/auth-api.md).

### User (Aggregate Root)

| Field | Type | Rules |
|-------|------|-------|
| Id | Guid | Identity |
| Email | string | Required, unique, login identifier |
| PasswordHash | string | Salted hash; never returned to clients |
| Name | string | Display name |
| Role | enum `Manager` \| `Admin` | Single primary role in v1 (FR-A03) |
| Status | enum `Active` \| `Disabled` | Disabled users cannot authenticate |

### RefreshToken (Entity, revocable, server-side only)

| Field | Type | Rules |
|-------|------|-------|
| Id | Guid | Identity |
| UserId | Guid | Owner |
| TokenHash | string | Hash of the opaque token; raw value never persisted in clear |
| FamilyId | Guid | Rotation family; reuse of a rotated token revokes the whole family (R10) |
| ExpiresAt | DateTimeOffset | ~7–30 days |
| RevokedAt | DateTimeOffset? | Set on logout, rotation, or reuse detection |

**Rules**: access tokens (JWT) are stateless and short-lived (~15 min) — not persisted; only refresh
tokens are stored, so they can be revoked (logout, SC-009 seamless refresh, US2 scenario 6). Clients
treat both as opaque and hold no signing secret (FR-A05).

### Role → capability mapping

Enforced by ASP.NET Core authorization policies per the spec Role Permission Matrix. `Manager`:
flag/experiment/micro-copy authoring + analytics/audit read. `Admin`: telemetry/alerts/kill-switch
+ analytics/audit read. Overlaps default to read-only.

---

## Flag Management context

### Flag (Aggregate Root)

| Field | Type | Rules |
|-------|------|-------|
| Id | Guid | Immutable identity |
| Key | string | Required, unique, `^[a-z0-9._-]{1,120}$`, immutable after create |
| Name | string | Required, ≤200 chars |
| DefaultOutcome | bool / VariantId | Safe fallback served when no rule/rollout matches or flag unknown |
| Status | enum `Draft` \| `Active` \| `Archived` | State machine below |
| KillSwitchEngaged | bool | When true, evaluation returns disabled with precedence (FR-009) |
| Version | long | Monotonic; incremented on every committed change (SC-006, R3 backstop) |
| TargetingRules | list<TargetingRule> | Ordered; first match wins |
| Rollout | Rollout (value object) | Optional percentage rollout |
| ExperimentId | Guid? | Optional link to an Experiment |
| CreatedAt / UpdatedAt | DateTimeOffset (UTC) | Audit timestamps |

**State transitions**: `Draft → Active → Archived`. `Archived` is terminal. Kill-switch may be
engaged/disengaged only while `Active`. Editing an `Archived` flag is rejected.

**Validation**: Rollout percentages 0–100 (stored as basis points 0–10000); sum of variant weights
in a linked experiment must equal 100%. Concurrent edits guarded by optimistic concurrency on
`Version` (FR-011).

### TargetingRule (Value Object, ordered within Flag)

| Field | Type | Rules |
|-------|------|-------|
| Attribute | string | Context attribute name (e.g., `country`, `plan`) |
| Operator | enum `Equals` \| `In` \| `NotEquals` \| `GreaterThan` \| `LessThan` \| `Contains` | — |
| Values | list<string> | Non-empty |
| Outcome | bool / VariantId | Result when the rule matches |

### Rollout (Value Object)

| Field | Type | Rules |
|-------|------|-------|
| Percentage | int (basis points 0–10000) | Share receiving the enabled/variant outcome |
| Salt | string | Stable per-flag salt for MurmurHash3 (R1); regenerating reshuffles buckets |

### Experiment (Aggregate Root)

| Field | Type | Rules |
|-------|------|-------|
| Id | Guid | Identity |
| FlagId | Guid | Owning flag |
| Name | string | Required |
| Status | enum `Draft` \| `Running` \| `Concluded` | — |
| Variants | list<Variant> | ≥2; weights sum to 100% (10000 bp) |
| ConversionGoal | string? | Optional named goal supplied by host app (SC-008) |

### Variant (Entity within Experiment)

| Field | Type | Rules |
|-------|------|-------|
| Id | Guid | Identity |
| Key | string | Unique within experiment |
| Weight | int (basis points) | 0–10000; sum across variants = 10000 |
| MicroCopyCandidateId | Guid? | Optional link to an approved AI candidate (FR-016) |

**Rule**: A `MicroCopyCandidateId` may only reference a candidate in `Approved` state (cross-context
invariant enforced at attach time).

---

## Evaluation Engine context (read model, in-memory)

### FlagSnapshot (Immutable, in-memory only — not persisted)

| Field | Type | Notes |
|-------|------|-------|
| Version | long | Highest applied Flag.Version; drives reconciliation (R3) |
| Flags | FrozenDictionary<string, CompiledFlag> | Keyed by flag key for O(1) lock-free lookup (R2) |
| BuiltAt | DateTimeOffset | For staleness metrics |

### CompiledFlag (Immutable struct-friendly projection)

Precomputed, allocation-free-to-read projection of a Flag + Experiment: default outcome, kill-switch
bit, compiled targeting predicates, rollout basis points + salt, and variant weight boundaries as a
sorted array for branchless bucket selection.

### EvaluationContext (readonly struct, request-scoped)

| Field | Type | Notes |
|-------|------|-------|
| FlagKey | string | Required |
| ContextKey | string | User/session identity for sticky bucketing (FR-004); optional → anonymous default (edge case) |
| Attributes | readonly span/dictionary | For targeting rules |

### EvaluationResult (readonly struct)

| Field | Type | Notes |
|-------|------|-------|
| Outcome | bool | Enabled/disabled |
| VariantKey | string? | Assigned variant if experiment |
| Reason | enum `Default` \| `TargetingMatch` \| `Rollout` \| `KillSwitch` \| `Unknown` \| `FailSafe` | Explains decision (observability) |
| SnapshotVersion | long | Which snapshot produced it |

---

## AI Gateway context

### MicroCopyGenerationRequest (Aggregate Root)

| Field | Type | Rules |
|-------|------|-------|
| Id | Guid | Identity |
| Surface | string | Where copy appears (e.g., `checkout.cta`) |
| Intent | string | What the copy should achieve |
| Tone | string? | e.g., `friendly`, `urgent` |
| MaxLength | int? | Character constraint |
| RequestedBy | ActorId | Attribution (FR-018) |
| CreatedAt | DateTimeOffset | — |
| Candidates | list<MicroCopyCandidate> | ≥3 for well-formed requests (SC-007) |
| ProviderStatus | enum `Succeeded` \| `Degraded` \| `Failed` | Degraded/Failed on provider outage (FR-017) |

### MicroCopyCandidate (Entity)

| Field | Type | Rules |
|-------|------|-------|
| Id | Guid | Identity |
| Text | string | Generated copy |
| ReviewState | enum `Draft` \| `Approved` \| `Rejected` | Default `Draft`; only `Approved` is attachable/servable (FR-016) |
| ReviewedBy | ActorId? | Set on approve/reject |
| ReviewedAt | DateTimeOffset? | — |

**State transitions**: `Draft → Approved` or `Draft → Rejected` (both terminal). No transition out
of `Approved`/`Rejected`. Rejected candidates are retained for audit but never served.

---

## Audit & Telemetry context

### AuditEntry (Immutable, append-only)

| Field | Type | Rules |
|-------|------|-------|
| Id | Guid | Identity |
| Timestamp | DateTimeOffset (UTC) | Server-assigned |
| Actor | ActorId | Who acted (FR-011) |
| ActorRole | enum `Manager` \| `Admin` \| `Service` | Role at time of action (FR-019, FR-A06) |
| ChangeType | enum `FlagCreated` \| `FlagUpdated` \| `KillSwitchEngaged` \| `KillSwitchReleased` \| `ExperimentChanged` \| `CopyGenerated` \| `CopyApproved` \| `CopyRejected` \| `RoleDenied` \| `LoginSucceeded` \| `LoginFailed` | Includes auth-sensitive events (FR-A07) |
| TargetId | Guid | Affected aggregate |
| BeforeState | json? | Null on create |
| AfterState | json? | Null on delete/archive |

**Rule**: Insert-only. The data layer grants no UPDATE/DELETE on this table (FR-019, SC-006).

### ExposureEvent (Append-only, high volume)

| Field | Type | Rules |
|-------|------|-------|
| Id | Guid/long | Identity |
| Timestamp | DateTimeOffset (UTC) | — |
| FlagKey | string | — |
| ExperimentId | Guid? | — |
| VariantKey | string? | Which variant was shown |
| ContextKey | string? | Null for anonymous (edge case) |
| SnapshotVersion | long | Traceability |

Written asynchronously off the hot path (R6). Aggregated into **VariantExposureCount** (FlagKey +
VariantKey + window → count) and joined with host-supplied conversion signals for winner analysis
(SC-008).

### Alert / CriticalEvent (Admin push + durable history)

| Field | Type | Rules |
|-------|------|-------|
| Id | Guid | Identity |
| Type | enum `ErrorRateSpike` \| `AnomalousExposure` \| `KillSwitchChanged` | Detected critical condition (FR-026) |
| FlagKey | string? | Related flag if applicable |
| Severity | enum `warning` \| `critical` | — |
| DetectedAt | DateTimeOffset | — |
| Acknowledged | bool | Per-Admin ack state |

Every alert is persisted here (durable history) **and** dispatched via push to subscribed Admins
through `IAlertNotifier` (FCM/APNs, Polly-wrapped). Push is best-effort; history is the fail-safe so
critical state is never lost if a push is undelivered (SC-011, R12).

### PushDevice

| Field | Type | Rules |
|-------|------|-------|
| Id | Guid | Identity |
| UserId | Guid | Owning Admin |
| DeviceToken | string | FCM/APNs token |
| Platform | enum `ios` \| `android` | Flutter mobile only |

### Actor (Reference, owned by Identity & Access)

| Field | Type | Rules |
|-------|------|-------|
| Id | Guid | Identity — a User or a service account |
| Kind | enum `User` \| `ServiceAccount` | Managers/Admins vs client apps |
| Role | enum `Manager` \| `Admin` \| `Service` | Attribution role (FR-A06) |
| DisplayName | string | — |

---

## Cross-context relationships

```text
User(Role) ──auth──> JWT access + RefreshToken(family) ──> every protected request (RBAC)
Flag 1──0..1 Experiment 1──2..* Variant 0..1──> MicroCopyCandidate(Approved)
Flag ──(Version, delta)──> Redis Pub/Sub ──> FlagSnapshot (Evaluation)
Any change / RoleDenied / Login ──> AuditEntry (append-only, with ActorRole)
Evaluation ──> ExposureEvent (async) ──> VariantExposureCount
Telemetry ──detect──> Alert ──> push (FCM/APNs) + durable history ──> Admin (Mobile App)
```

Cross-context links are by ID only; each context persists and validates its own aggregates.
Identity & Access is a supporting subdomain reused by all contexts via the Api host, never merged
into a product context.
