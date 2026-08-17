# Contract: Telemetry & Audit API (Audit & Telemetry context)

Read surface over the immutable audit log and experiment telemetry. Audit is append-only; no
mutation endpoints exist by design (FR-019, SC-006).

## GET /api/v1/audit
Query params: `targetId`, `actor`, `changeType`, `from`, `to`, `page`, `pageSize`.
Response `200`:
```json
{
  "entries": [
    { "id":"…","timestamp":"…","actor":"…","changeType":"KillSwitchEngaged","targetId":"…","beforeState":{…},"afterState":{…} }
  ],
  "page": 1, "pageSize": 50, "total": 1284
}
```
- Every configuration change, kill-switch action, and AI decision appears **exactly once** with
  actor + timestamp (SC-006). Entries are immutable — no PUT/DELETE.

## GET /api/v1/telemetry/experiments/{experimentId}
Response `200`:
```json
{
  "experimentId": "…",
  "status": "Running",
  "variants": [
    { "variantKey": "control", "exposures": 10240, "conversions": 512, "conversionRate": 0.05 },
    { "variantKey": "b",       "exposures": 10188, "conversions": 640, "conversionRate": 0.0628 }
  ],
  "window": { "from": "…", "to": "…" }
}
```
- Exposure counts reconcile with evaluations within 1% (SC-008).
- Conversion signals are host-supplied and attributed to variants.

## POST /api/v1/telemetry/conversions — host-app conversion ingest
Request: `{ "experimentId":"…", "variantKey":"…", "contextKey":"…", "goal":"purchase", "occurredAt":"…" }`
Response `202` accepted (async aggregation). Idempotent per `(contextKey, goal, experimentId)`.

**Auth**: analysts/managers read; conversion ingest via service-account token.

## Alerts & Push Notifications (Admin / Mobile App)

Critical-event alerting consumed by the Flutter Mobile App (R12, FR-026, SC-011). Requires `Admin`
role.

### POST /api/v1/alerts/devices — register a device for push
Request: `{ "deviceToken": "<fcm/apns token>", "platform": "ios" | "android" }` → `204`. Associates
the authenticated Admin with a push destination.

### GET /api/v1/alerts — in-app alert history (fail-safe)
Response `200`:
```json
{ "alerts": [ { "id":"…","type":"ErrorRateSpike","flagKey":"…","severity":"critical","detectedAt":"…","acknowledged":false } ] }
```
- Every alert that triggers a push is ALSO retained here, so critical state is never conveyed solely
  by a possibly-undelivered push (SC-011, edge case). Delivered within the 10s alert budget.

### POST /api/v1/alerts/{id}/ack — acknowledge
`204`. Marks the alert acknowledged for the acting Admin.

**Kill-switch from mobile**: the Mobile App uses the existing
[`POST /api/v1/flags/{key}/kill-switch`](management-api.md) endpoint (Admin role); confirmation is
returned on delivery, and the app shows the action as pending until confirmed under intermittent
connectivity (FR-027, spec US4 scenario 5).

**Audit note**: audit entries include the acting user's **role** (FR-019); role-denied attempts are
recorded (FR-A07, SC-006).
