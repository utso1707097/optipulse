# Contract: AI Gateway API (AI Gateway context)

Micro-copy variant generation behind a swappable, Polly-wrapped provider port (R7). Generation is
non-blocking to evaluation/management (FR-017). Human-approval gate enforced before any variant is
attachable/servable (FR-016).

## POST /api/v1/microcopy/generate
Request:
```json
{ "surface": "checkout.cta", "intent": "increase upgrade clicks", "tone": "confident", "maxLength": 40, "count": 5 }
```
Response `201`:
```json
{
  "requestId": "…",
  "providerStatus": "Succeeded",
  "candidates": [
    { "id": "…", "text": "Upgrade & save today", "reviewState": "Draft" },
    { "id": "…", "text": "Unlock Pro in one tap", "reviewState": "Draft" }
  ]
}
```
- Returns ≥3 distinct candidates for well-formed requests (SC-007).
- Provider slow/unavailable → `200/201` with `providerStatus: "Degraded"|"Failed"` and possibly
  empty `candidates`; MUST NOT block or error the platform (FR-017). Circuit-breaker open ⇒
  `Degraded` fast-fail.
- Request context + candidates recorded for audit (FR-018).

## GET /api/v1/microcopy/requests/{requestId}
Returns the request with candidates and their current review states.

## POST /api/v1/microcopy/candidates/{id}/review
Request: `{ "decision": "Approve" | "Reject" }`
Response `200`: candidate with `reviewState` = `Approved`/`Rejected`, `reviewedBy`, `reviewedAt`.
- Only `Draft` candidates are reviewable; re-review of terminal state ⇒ `409`.
- Approval/rejection audited as `CopyApproved`/`CopyRejected` (FR-022).
- Only `Approved` candidates may be attached to experiment variants (enforced by Management API).

**Auth**: RBAC — generation and review require authorized manager/reviewer roles.
