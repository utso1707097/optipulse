# Contract: Management API (Flag Management context)

Authoring & control surface for managers (RBAC-authorized users). Every mutating call is versioned,
attributed, audited, and publishes an invalidation message (R3). Uses optimistic concurrency via
`version` (FR-011).

## Flags

### POST /api/v1/flags — create
Request: `{ "key": "checkout.new-cta", "name": "...", "defaultOutcome": false, "targetingRules": [...], "rollout": { "percentage": 50, "salt": "..." } }`
Response `201`: full Flag with `version: 1`, `status: "Draft"`. `409` if key exists.

### GET /api/v1/flags / GET /api/v1/flags/{key}
List / fetch flag configuration including current version.

### PUT /api/v1/flags/{key} — update (requires `If-Match: <version>`)
Body: editable fields. `200` new version on success; `409` on version mismatch (concurrent edit,
FR-011); `422` if targeting/rollout invalid or flag `Archived`.

### POST /api/v1/flags/{key}/status — lifecycle transition
Body: `{ "status": "Active" | "Archived" }`. Enforces `Draft→Active→Archived`.

### POST /api/v1/flags/{key}/kill-switch — engage/release (FR-009)
Body: `{ "engaged": true }`. Publishes a **kill-switch** invalidation with precedence; MUST reflect
on all nodes < 100ms (SC-002). Audited as `KillSwitchEngaged`/`KillSwitchReleased`.

### GET /api/v1/flags/{key}/versions — recoverable history (FR-010)
Returns prior configurations; a specific version may be restored via PUT.

## Experiments

### POST /api/v1/experiments — create A/B/n (FR-008)
Body: `{ "flagKey": "...", "name": "...", "variants": [ { "key": "control", "weight": 50 }, { "key": "b", "weight": 50 } ], "conversionGoal": "purchase" }`
`422` unless ≥2 variants and weights sum to 100%.

### PUT /api/v1/experiments/{id} — update weights/variants
Sticky assignments preserved for existing contexts; only new eligibility affected (edge case,
FR-004). Audited.

### POST /api/v1/experiments/{id}/variants/{variantKey}/attach-copy
Body: `{ "candidateId": "..." }`. `422` unless candidate is `Approved` (FR-016 cross-context
invariant).

**Common errors**: `401/403` unauthorized (FR-A03/FR-A05, RBAC); `409` version conflict; `422` validation.
