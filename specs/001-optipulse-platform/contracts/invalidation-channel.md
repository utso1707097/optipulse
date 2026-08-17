# Contract: Cache Invalidation Channel (Redis Pub/Sub)

Internal contract between Flag Management (publisher) and every API node's Evaluation Engine
(subscriber). Delivers the <100ms global propagation and kill-switch precedence (FR-009, SC-002,
R3). This is not an HTTP contract — it is the message schema on the Redis channel.

## Channel
`optipulse:flags:invalidate`

## Message schema
```json
{
  "type": "FlagChanged" | "KillSwitch" | "ExperimentChanged",
  "flagKey": "checkout.new-cta",
  "flagId": "…",
  "newVersion": 4822,
  "killSwitchEngaged": true,
  "publishedAt": "2026-08-15T10:31:02.140Z"
}
```

## Subscriber rules
1. On receipt, if `newVersion` ≤ current snapshot version for that flag, ignore (idempotent, handles
   out-of-order/at-most-once delivery).
2. Apply the delta and atomically swap the snapshot reference (R2). Target apply latency budget is
   the dominant term of the <100ms SLA (SC-002).
3. `type = KillSwitch` with `killSwitchEngaged = true` is applied with **precedence**: a killed
   flag MUST NOT evaluate enabled even if a later re-enable message is delayed or lost (fail-safe
   "off", edge case: partial invalidation).
4. Pub/Sub is at-most-once; a periodic reconciliation loop compares snapshot version against the
   authoritative store and heals missed messages without weakening the happy path (R3 backstop).

## Publisher rules
- Publish only **after** the change is durably committed to the authoritative store, so a subscriber
  reconciling against the store never sees an older state than a delivered message.
- `newVersion` is the flag's post-commit monotonic version.
