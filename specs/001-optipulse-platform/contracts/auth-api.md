# Contract: Authentication & RBAC API (Identity & Access)

Custom JWT authentication with role-based access control, contained strictly in the .NET 10 backend
(Constitution Principle VI). Consumed identically by the **React Web Dashboard** and the **Flutter
Mobile App**. Clients treat tokens as **opaque**, store no signing secret, and perform **no**
authorization logic — every decision is server-side (FR-A01–A06). All schemas below are part of the
native OpenAPI document and drive generated TS/Dart clients (see
[openapi-pipeline.md](openapi-pipeline.md)).

## Roles

| Role | Audience | Default permissions (see spec Role Permission Matrix) |
|------|----------|-------------------------------------------------------|
| `Manager` | Product & Marketing (Web Dashboard) | create/edit flags & experiments, generate/approve micro-copy, read analytics, read audit |
| `Admin` | Admin & DevOps (Mobile App) | telemetry monitoring, receive alerts, activate/release kill-switch, read analytics, read audit |

Overlapping capabilities (analytics/audit read) are granted to both; mutations require the owning
role. Authorization is enforced by ASP.NET Core policies; each protected endpoint declares its
required policy.

## POST /api/v1/auth/login
Authenticate with credentials, receive a token pair.

**Request**
```json
{ "email": "pm@acme.io", "password": "••••••••" }
```
**Response 200**
```json
{
  "accessToken": "<jwt>",
  "refreshToken": "<opaque-rotating-token>",
  "tokenType": "Bearer",
  "expiresInSeconds": 900,
  "role": "Manager"
}
```
- `401` on bad credentials (generic message; no user enumeration).
- Login latency target < 5s end-to-end (SC-009). Attempts (success/failure) are audited.

## POST /api/v1/auth/refresh
Exchange a valid refresh token for a new token pair (rotation).

**Request**: `{ "refreshToken": "<opaque>" }`
**Response 200**: same shape as login (new `accessToken` + **new** `refreshToken`).
- The presented refresh token is invalidated on use (rotation). **Reuse detection**: if an already-
  rotated token is presented, the whole token family is revoked and `401` returned (possible theft).
- Enables seamless session continuation with zero forced re-logins during an active session
  (SC-009). `401` if the refresh token is expired, unknown, or revoked → client must re-login.

## POST /api/v1/auth/logout
Revoke the current refresh token (and its family).

**Request**: `{ "refreshToken": "<opaque>" }` → **Response 204**. Subsequent use of that token or
its descendants is rejected (spec US2 scenario 6).

## GET /api/v1/auth/me
Return the authenticated principal (requires valid access token).
**Response 200**: `{ "userId": "…", "name": "…", "role": "Manager", "email": "…" }`.

## Access token (JWT) payload schema
Signed by the backend; clients MUST NOT rely on parsing it for authorization.
```json
{
  "sub": "user-guid",
  "name": "Jane PM",
  "role": "Manager",          // "Manager" | "Admin" — single primary role (v1)
  "iat": 1755250000,
  "exp": 1755250900,          // ~15 min
  "jti": "token-guid",
  "iss": "optipulse",
  "aud": "optipulse-clients"
}
```

## Protected-request convention
All non-auth endpoints require `Authorization: Bearer <accessToken>`.

- Missing/invalid/expired access token → `401 Unauthorized` (client should attempt one silent
  refresh, then re-login).
- Authenticated but role not permitted → `403 Forbidden`, no state change, attempt audited
  (FR-A07, spec US2 scenarios 3–4).

## RBAC enforcement examples (informative)

| Endpoint | Required role |
|----------|---------------|
| `POST /api/v1/flags` (create) | `Manager` |
| `POST /api/v1/microcopy/candidates/{id}/review` | `Manager` |
| `POST /api/v1/flags/{key}/kill-switch` | `Admin` |
| `GET /api/v1/telemetry/experiments/{id}` | `Manager` or `Admin` |
| `GET /api/v1/audit` | `Manager` or `Admin` |

**Security notes**: refresh tokens are stored server-side (revocable) and never embedded in client
source; signing keys live only in the backend; failed authorization and kill-switch actions are
audited (Principle IV/VI, FR-A07).
