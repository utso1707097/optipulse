# Feature Specification: OptiPulse Platform

**Feature Branch**: `001-optipulse-platform`

**Created**: 2026-08-15

**Updated**: 2026-08-15 (dual-client refactor + JWT authentication & RBAC)

**Status**: Draft

**Input**: User description: "Build OptiPulse: A high-performance feature flagging platform, A/B testing evaluation engine, and AI-driven micro-copy variant generator. Include Domain-Driven Design bounded contexts for Evaluation Engine, Flag Management, AI Gateway, and Audit & Telemetry." — refined with a dual-client strategy (React Web Dashboard for Product & Marketing managers; Flutter Mobile App for Admin & DevOps engineers) and Custom JWT authentication with role-based access control.

## User Scenarios & Testing *(mandatory)*

OptiPulse serves distinct audiences through two purpose-built clients plus a runtime SDK surface:

- **Web Dashboard (Product & Marketing Managers)** — an always-online console for creating feature flags, managing experiments, generating AI micro-copy variants, and reviewing analytics.
- **Mobile App (Admin & DevOps Engineers)** — an on-the-go, offline-tolerant operations app for real-time telemetry monitoring, receiving push notifications on critical events, and triggering instant kill-switch activations.
- **Client Applications / Services** — running software that requests flag decisions at runtime.

All human access is gated by authentication and role-based authorization: **Managers** hold authoring/experimentation permissions; **Admins/DevOps** hold operational/override permissions. The platform is organized around four capability areas (bounded contexts): Flag Management, Evaluation Engine, AI Gateway, and Audit & Telemetry.

### User Story 1 - Real-Time Flag Evaluation for Applications (Priority: P1)

A running application asks OptiPulse whether a given feature is enabled for a specific user (or context), and receives a deterministic, near-instant decision. The same user with the same flag state always resolves to the same answer, and decisions honor targeting rules and rollout percentages.

**Why this priority**: This is the core value of the platform — without fast, correct runtime evaluation nothing else matters. Delivered alone it already provides a usable feature-flagging service.

**Independent Test**: Configure a flag with a targeting rule and a percentage rollout, then request evaluations for many distinct user contexts; verify decisions return within the latency budget, are stable across repeated calls, and that the share of "enabled" results matches the configured percentage within tolerance.

**Acceptance Scenarios**:

1. **Given** a flag enabled at 50% rollout, **When** an application evaluates it for 10,000 distinct user identifiers, **Then** each identifier resolves consistently on every call and approximately 50% resolve to "enabled".
2. **Given** a flag with a targeting rule ("country = US"), **When** a US user and a non-US user are evaluated, **Then** the US user matches the rule outcome and the non-US user falls through to the default.
3. **Given** a request for an unknown flag key, **When** it is evaluated, **Then** the system returns the safe default without error and records the miss.
4. **Given** the backing datastore is temporarily unavailable, **When** evaluations continue, **Then** decisions are served from the last known good state rather than failing.

---

### User Story 2 - Authentication & Role-Based Access Control (Priority: P1)

A Product Manager or an Admin engineer signs in to their respective client, receives a session that keeps them logged in without repeated credential entry, and can only perform the actions their role permits. Managers author flags/experiments and generate copy; Admins monitor telemetry and operate kill-switches. Unauthorized or expired sessions are rejected and prompt re-authentication.

**Why this priority**: Authentication and role separation gate every human action across both clients; without it, no management, generation, or kill-switch operation can be trusted. It is foundational and safety-critical.

**Independent Test**: Sign in as a Manager and as an Admin; verify each receives a valid session, that permitted actions succeed and non-permitted actions are refused, that an expired session refreshes seamlessly, and that a revoked/invalid session is blocked.

**Acceptance Scenarios**:

1. **Given** valid Manager credentials, **When** the user logs in to the Web Dashboard, **Then** they receive an authenticated session and land on the management console with manager-permitted actions available.
2. **Given** an authenticated session whose access window has elapsed, **When** the user continues working, **Then** the session refreshes without requiring the user to re-enter credentials, and work continues uninterrupted.
3. **Given** a logged-in Manager, **When** they attempt an Admin-only action (e.g., activate a kill-switch), **Then** the action is refused with an authorization error and no state changes.
4. **Given** a logged-in Admin, **When** they attempt a Manager-only action (e.g., create a flag), **Then** the action is refused per role policy (unless the Admin role explicitly includes it), and the attempt is recorded.
5. **Given** an invalid, tampered, or expired-beyond-refresh session, **When** any protected action is attempted, **Then** it is rejected and the user is prompted to authenticate again.
6. **Given** a user logs out, **When** they attempt to reuse the prior session, **Then** it is no longer accepted.

---

### User Story 3 - Manager Web Dashboard: Flags, Experiments, Micro-Copy & Analytics (Priority: P1)

A Product or Marketing Manager uses the always-online Web Dashboard to create and control flags, define targeting rules and rollout percentages, set up A/B/n experiments, request AI-generated micro-copy variants and approve them, and review experiment analytics to decide winners.

**Why this priority**: Authoring, experimentation, and copy generation are the primary value-creation workflows and the reason the manager audience uses the platform. They depend on evaluation (US1) and auth (US2) existing.

**Independent Test**: As an authenticated Manager on the Web Dashboard, create a flag and an experiment, generate and approve micro-copy, attach an approved variant, and open the analytics view — verifying each step succeeds and is reflected in evaluation and telemetry.

**Acceptance Scenarios**:

1. **Given** an authenticated Manager, **When** they create a flag with a targeting rule and rollout and save, **Then** the flag becomes evaluable and its configuration is versioned and attributed to them.
2. **Given** a Manager, **When** they define an A/B/n experiment with weighted variants, **Then** assignments become sticky per user and distributed according to the configured weights.
3. **Given** a Manager on a micro-copy surface, **When** they request AI variants with an intent and tone, **Then** multiple distinct candidates are returned for review; only approved candidates can be attached to an experiment.
4. **Given** a running experiment, **When** the Manager opens analytics, **Then** per-variant exposures and conversion signals are presented in a way that supports choosing a winner.
5. **Given** the Web Dashboard is used, **When** the network is unavailable, **Then** the dashboard clearly indicates it requires connectivity rather than presenting stale editable state.

---

### User Story 4 - Admin & DevOps Mobile App: Telemetry, Push Alerts & Instant Kill-Switch (Priority: P1)

An Admin or DevOps engineer uses the Flutter Mobile App to monitor real-time telemetry, receive push notifications when critical events occur (e.g., error-rate spikes or anomalous exposure patterns), and instantly activate an emergency kill-switch that propagates globally — even while on the move or intermittently connected.

**Why this priority**: Operational safety — the ability to see problems and kill a bad feature immediately from anywhere — is critical to running the platform in production. The kill-switch is a safety-critical capability.

**Independent Test**: As an authenticated Admin on the Mobile App, view live telemetry, trigger a test critical event to receive a push notification, and activate a kill-switch — verifying the alert arrives promptly and the kill-switch reflects at all evaluation points within the propagation budget.

**Acceptance Scenarios**:

1. **Given** an authenticated Admin, **When** they open the Mobile App, **Then** they see real-time telemetry (exposures, key health signals) updating live.
2. **Given** a defined critical event occurs, **When** it is detected, **Then** subscribed Admins receive a push notification within the alert budget describing the event.
3. **Given** a live flag causing an incident, **When** the Admin activates the kill-switch from the Mobile App, **Then** all evaluation points serve the disabled state within the global-propagation budget, and the action is attributed and audited.
4. **Given** the Mobile App is offline, **When** the Admin reopens it after reconnecting, **Then** cached telemetry/flag state reconciles deterministically with server truth and any kill-switch state takes precedence.
5. **Given** an Admin activates a kill-switch while briefly disconnected, **When** connectivity returns, **Then** the intended kill action is applied and confirmed (or clearly reported as not yet applied), never silently lost.

---

### User Story 5 - Immutable Audit Trail (Priority: P2)

A compliance or analytics stakeholder reviews an immutable record of who changed what, when — across both clients — including flag/experiment changes, kill-switch activations, AI approval decisions, and authentication-sensitive events.

**Why this priority**: Trust and compliance depend on a reliable, tamper-proof record. Essential for a mature platform but the core service can run before it is complete.

**Independent Test**: Perform a series of changes and kill-switch activations from both clients, then query the audit log; verify every change appears exactly once with actor, role, timestamp, and before/after state, and that records cannot be altered.

**Acceptance Scenarios**:

1. **Given** a sequence of changes from Web and Mobile clients, **When** the audit log is queried, **Then** each change appears exactly once with actor, role, timestamp, and before/after state, and records cannot be altered.
2. **Given** a kill-switch activation and an AI approval, **When** the audit log is reviewed, **Then** both are recorded with the acting user and role.

---

### Edge Cases

- What happens when an evaluation request omits the user/context identifier? → System resolves using the flag's default and records an anonymous exposure.
- How does the system handle a rollout percentage change mid-experiment? → Existing assignments remain sticky; only newly-eligible contexts are affected; the change is audited.
- What happens when two managers edit the same flag concurrently? → The system prevents silent overwrite and surfaces the conflict, preserving the prior version.
- What happens when a session expires mid-action? → The in-flight action is rejected cleanly; after seamless refresh (or re-login) the user can retry; no partial change is committed.
- What happens when a Manager's and an Admin's permissions overlap on a shared read (e.g., viewing a flag)? → Both may read; only role-permitted mutations succeed.
- What happens when a push notification cannot be delivered (device offline)? → The alert is queued/retried and remains visible in the app's alert history on next open; critical state is never conveyed solely by a possibly-lost push.
- What happens when an Admin triggers a kill-switch from a flaky mobile connection? → The request is confirmed on delivery; if not yet delivered, the app shows it as pending, never as done.
- How does the system behave when global cache invalidation partially fails on some nodes? → Nodes without the update must not serve a re-enabled state for a kill-switched flag; kill-switch state fails safe to "off".
- What happens when AI-generated copy is offensive or off-brand? → It cannot reach end users without human approval; rejected candidates are retained for audit but never served.

## Requirements *(mandatory)*

### Functional Requirements

**Authentication & Authorization**

- **FR-A01**: System MUST authenticate users with credentials and issue a token-based session (custom JWT) that clients treat as opaque.
- **FR-A02**: System MUST support seamless session continuation via token refresh so users are not forced to re-enter credentials during an active working session.
- **FR-A03**: System MUST enforce role-based access control with at least two roles — **Manager** and **Admin/DevOps** — where each role's permitted actions are explicitly defined and enforced on every protected operation.
- **FR-A04**: System MUST reject invalid, tampered, expired-beyond-refresh, or logged-out sessions and prompt re-authentication, allowing no protected action to proceed.
- **FR-A05**: System MUST perform all authentication and authorization decisions server-side; neither client may hold authorization logic or signing secrets.
- **FR-A06**: System MUST attribute every management, kill-switch, and AI-approval action to the authenticated user and their role.
- **FR-A07**: System MUST record authentication-sensitive events (role-denied attempts, kill-switch activations) in the audit trail.

**Evaluation Engine**

- **FR-001**: System MUST evaluate a feature flag for a supplied context and return a deterministic decision, such that identical inputs and flag state always yield the same result.
- **FR-002**: System MUST support percentage-based rollouts whose realized distribution matches the configured percentage within a small tolerance across a large population.
- **FR-003**: System MUST support targeting rules that match on context attributes and select a rule outcome before falling through to the default.
- **FR-004**: System MUST assign users to experiment variants stickily, so a given user consistently receives the same variant for the life of the experiment unless the experiment is changed.
- **FR-005**: System MUST serve the last known good flag state when its backing datastore is unavailable, rather than failing evaluation.
- **FR-006**: System MUST return a safe default for unknown or malformed flag requests without raising an error to the caller.

**Flag Management (Manager, Web Dashboard)**

- **FR-007**: Managers MUST be able to create, edit, enable, disable, and archive flags, including targeting rules and rollout percentages.
- **FR-008**: Managers MUST be able to define A/B/n experiments with named, weighted variants.
- **FR-009**: The system MUST provide an emergency kill-switch that disables a flag globally and propagates the change to all evaluation points within the global-propagation budget.
- **FR-010**: System MUST version every flag and experiment configuration change so prior states are recoverable.
- **FR-011**: System MUST prevent conflicting concurrent edits from silently overwriting one another.

**AI Gateway (Manager, Web Dashboard)**

- **FR-014**: Managers MUST be able to request AI-generated micro-copy variants for a specified surface, intent, and stylistic constraints (e.g., tone, length).
- **FR-015**: System MUST return multiple distinct candidate variants per generation request.
- **FR-016**: System MUST require human review/approval before any generated variant can be attached to an experiment or served to end users.
- **FR-017**: System MUST degrade gracefully when the AI provider is slow or unavailable, returning a clear status and never blocking flag evaluation or management.
- **FR-018**: System MUST record the request context and the generated candidates for each generation, including which were approved or rejected.

**Admin & DevOps Operations (Mobile App)**

- **FR-025**: Admins MUST be able to view real-time telemetry (per-variant exposures and key health signals) that updates live.
- **FR-026**: System MUST deliver push notifications to subscribed Admins when defined critical events occur, within the alert budget, with the alert also retained in an in-app history.
- **FR-027**: Admins MUST be able to activate and release the emergency kill-switch from the Mobile App, with confirmation of application and never silent loss under intermittent connectivity.
- **FR-028**: The Mobile App MUST tolerate offline operation and reconcile cached state deterministically with server truth on reconnect, with kill-switch state taking precedence.

**Client Experience Boundaries**

- **FR-029**: The Web Dashboard MUST assume network availability and MUST NOT present stale editable management state when offline.
- **FR-030**: Both clients MUST consume the platform exclusively through the governed API contract and MUST NOT diverge from the server contract.

**Deployment & First-Run Operability**

- **FR-031**: The Web Dashboard MUST be able to reach the platform when served from a different origin than the API, and the set of permitted origins MUST be configurable per environment rather than fixed in code.
- **FR-032**: A newly provisioned, empty environment MUST be reachable by an operator without direct database access: the platform MUST **bootstrap** an initial Manager account, an initial Admin account, and an initial service-account credential from configuration on first run.
- **FR-033**: Bootstrap credentials MUST be supplied by configuration and MUST NOT default to any predictable or committed value. Outside development the platform MUST refuse to bootstrap rather than invent a credential.
- **FR-034**: Bootstrap MUST be idempotent and MUST NOT alter or replace existing accounts on any subsequent start.
- **FR-035**: The platform MUST be deployable as a self-contained container image configured entirely through environment variables, so that no deployment step depends on a developer workstation.

**Audit & Telemetry**

- **FR-019**: System MUST maintain an immutable audit log of all configuration changes capturing actor, role, timestamp, and before/after state.
- **FR-020**: System MUST capture per-variant exposure events when contexts are evaluated within an experiment.
- **FR-021**: Users MUST be able to query experiment telemetry (exposures and conversion signals) to determine a winning variant.
- **FR-022**: System MUST record kill-switch activations and AI generation/approval decisions in the audit trail.

**Cross-cutting**

- **FR-023**: System MUST keep the four capability areas (Evaluation Engine, Flag Management, AI Gateway, Audit & Telemetry) as distinct bounded contexts with clearly defined responsibilities and interfaces.
- **FR-024**: System MUST expose runtime evaluation to client applications and support offline-tolerant clients that reconcile with server truth on reconnect.

### Role Permission Matrix (default)

| Capability | Manager | Admin / DevOps |
|-----------|:-------:|:--------------:|
| Create / edit / archive flags | ✅ | ➖ (read) |
| Define / edit experiments | ✅ | ➖ (read) |
| Generate & approve micro-copy | ✅ | ➖ |
| Review analytics / experiment results | ✅ | ✅ |
| Real-time telemetry monitoring | ➖ (read) | ✅ |
| Receive critical push notifications | ➖ | ✅ |
| Activate / release kill-switch | ➖ | ✅ |
| View audit trail | ✅ | ✅ |

Legend: ✅ full action · ➖ (read) read-only where noted · ➖ not permitted. Overlaps default to read-only; mutations require the owning role.

### Key Entities *(include if feature involves data)*

- **User**: A human operator with credentials and exactly one primary **Role** (`Manager` or `Admin`); actions are attributed to the user + role.
- **Role**: A named set of permitted capabilities (see permission matrix); enforced server-side.
- **Session / Token**: An authenticated session comprising a short-lived access grant and a longer-lived refresh grant, each with an expiry; treated as opaque by clients.
- **Flag**: A named toggle governing a feature; has a key, default outcome, enabled/disabled state, versioned configuration, and lifecycle status (draft/active/archived).
- **Targeting Rule**: A condition over context attributes that, when matched, selects a specific outcome.
- **Rollout**: The percentage and bucketing configuration determining what share of contexts receive an outcome.
- **Experiment**: An A/B/n test associated with a flag; contains weighted **Variants** and optional conversion goals.
- **Variant**: A named option within an experiment, optionally carrying approved micro-copy content.
- **Evaluation Context**: The user/session/attributes supplied at evaluation time.
- **Exposure Event**: A record that a context was shown a particular variant/outcome at a point in time.
- **Micro-Copy Generation Request**: The intent, surface, and constraints submitted to the AI Gateway, plus its returned candidates and their review status.
- **Critical Event / Alert**: A detected condition (e.g., error-rate spike) that triggers a push notification to Admins and is retained in alert history.
- **Audit Entry**: An immutable record of a change or decision, with actor, role, timestamp, and before/after state.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 99% of runtime flag evaluations return a decision in under 5 milliseconds under sustained production-representative load.
- **SC-002**: An emergency kill-switch — including one triggered from the Mobile App — is reflected at all evaluation points within 100 milliseconds of activation.
- **SC-003**: For a 50% rollout evaluated across 10,000 distinct contexts, the realized "enabled" share falls within ±1 percentage point of target, and every context resolves identically on repeat (100% determinism).
- **SC-004**: Flag evaluation continues to serve correct last-known-good decisions with zero evaluation failures during a simulated 5-minute datastore outage.
- **SC-005**: A Manager can create a flag with a targeting rule and rollout and have it evaluable in under 3 minutes without engineering assistance.
- **SC-006**: 100% of configuration changes, kill-switch activations, and role-denied attempts appear exactly once in the audit trail with correct actor, role, and timestamp, and no audit entry can be modified after creation.
- **SC-007**: An AI micro-copy request returns at least 3 distinct usable candidate variants for at least 90% of well-formed requests, and no generated variant reaches end users without recorded human approval.
- **SC-008**: Experiment exposure counts reconcile with evaluations performed within a 1% margin, enabling a statistically supportable winner decision.
- **SC-009**: Users can log in within 5 seconds, and active sessions refresh seamlessly with zero forced re-logins during a continuous 30-minute working session.
- **SC-010**: 100% of protected actions are blocked when attempted without a valid session or the required role — zero unauthorized successes across a role-permission test matrix.
- **SC-011**: For a detected critical event, subscribed Admins receive a push notification within 10 seconds, and the alert is always present in in-app history even if the push is not delivered.

## Assumptions

- **Roles & access**: Two roles ship in v1 — Manager (authoring/experimentation/copy/analytics) and Admin/DevOps (operations/monitoring/kill-switch). The permission matrix above is the default; overlaps resolve to read-only. Additional granular roles are out of scope for v1.
- **Authentication scheme**: A custom JWT-based scheme owned entirely by the backend (per constitution). No external identity provider is required. Access grants are short-lived with a longer-lived refresh grant; refresh rotation follows standard security practice. Specific token lifetimes are an implementation detail of the plan.
- **Clients**: Web Dashboard is always-online and lightweight; Mobile App is offline-tolerant. Each client is purpose-built for its audience and consumes the same governed backend contract.
- **Push notifications**: Delivered via a standard mobile push mechanism; the provider is an implementation detail. Critical state is never conveyed solely by a push — it is always also retrievable in-app.
- **AI provider**: Micro-copy generation is powered by a swappable external AI provider behind the AI Gateway; all generated content is draft until human-approved.
- **Hosting**: The platform is deployed as a container image against a managed PostgreSQL instance and a managed Redis instance; the Web Dashboard is served as static assets from a separate origin. A demonstration deployment may run on hosting that suspends idle instances, so first-request latency after idle is a property of the host and is not evidence about the evaluation budget in SC-001.
- **Determinism mechanism**: Deterministic bucketing uses a stable hashing scheme so results are reproducible across nodes; the algorithm is an implementation concern of the plan.
- **Scale target**: High-throughput evaluation with global, multi-node distribution and near-real-time configuration propagation.
- **Data retention**: Audit entries are retained per standard compliance practice (long-lived/immutable); telemetry is retained long enough to conclude and review experiments.
- **Out of scope for v1**: Visual/what-you-see experiment editors, automated statistical stopping/auto-rollout, self-service end-user account signup, SSO/social login, and localization of AI copy beyond the requested language.
