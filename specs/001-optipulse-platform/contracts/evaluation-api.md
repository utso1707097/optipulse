# Contract: Evaluation API (Evaluation Engine context)

Runtime, latency-critical surface consumed by client applications/service accounts. All responses
served from the in-memory snapshot; no synchronous datastore access (R2). Auth: service-account
token. Target: p99 < 5ms (SC-001).

## POST /api/v1/evaluate

Evaluate a single flag for a context.

**Request**
```json
{
  "flagKey": "checkout.new-cta",
  "contextKey": "user-8f3a...",
  "attributes": { "country": "US", "plan": "pro" }
}
```

**Response 200**
```json
{
  "flagKey": "checkout.new-cta",
  "outcome": true,
  "variantKey": "challenger-a",
  "reason": "Rollout",
  "snapshotVersion": 4821
}
```

- `reason` ∈ `Default | TargetingMatch | Rollout | KillSwitch | Unknown | FailSafe`.
- Unknown flag → `200` with `outcome` = safe default, `reason` = `Unknown` (FR-006). Never `404`.
- Missing `contextKey` → default outcome, anonymous exposure recorded (edge case).
- Datastore degraded → still `200`, served from last-known-good, `reason` = `FailSafe` (FR-005).

## POST /api/v1/evaluate/batch

Evaluate many flags for one context in a single round-trip.

**Request**: `{ "contextKey": "...", "attributes": {...}, "flagKeys": ["a","b","c"] }`
**Response 200**: `{ "results": [ { …EvaluationResult }, … ], "snapshotVersion": 4821 }`

## GET /api/v1/snapshot/version

Returns `{ "version": 4821, "builtAt": "..." }`. Used by clients to detect staleness and by
health checks. Kill-switch precedence guarantees a killed flag never evaluates enabled even if a
newer re-enable has not yet propagated (FR-009, R3).

**Invariants**
- Deterministic: identical `flagKey`+`contextKey`+snapshot ⇒ identical result (FR-001, SC-003).
- No allocation on the evaluation path beyond the response DTO (Principle II).
- Evaluation emits an async ExposureEvent when the flag is under an experiment (FR-020, R6).
