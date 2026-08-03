# Auth API Contract

> Authentication endpoints. No bearer token required for `login` / `refresh`; `change-password` requires a bearer token.

## POST `/api/v1/auth/login`

Authenticate a staff user and issue tokens. Throttled per username + IP; generic error message on failure (no enumeration).

**Request**
```json
{
  "userName": "jdoe",
  "password": "P@ssw0rd!"
}
```

**200 OK**
```json
{
  "accessToken": "eyJhbGciOi...",
  "refreshToken": "8f1c...",
  "accessTokenExpiresAt": "2026-08-03T16:05:00Z",
  "mustChangePassword": false,
  "user": { "id": "...", "userName": "jdoe", "firstName": "Jane", "lastName": "Doe", "roles": ["Receptionist"] }
}
```

**401** (invalid credentials OR suspended OR locked out) — identical safe body for all to avoid enumeration:
```json
{ "title": "Unauthorized", "status": 401, "detail": "Invalid credentials." }
```

**429** — too many attempts; `Retry-After` header.

## POST `/api/v1/auth/refresh`

Rotate a refresh token (single-use; reuse revokes the chain).

**Request**
```json
{ "refreshToken": "8f1c..." }
```

**200 OK** — same shape as login (`accessToken`, `refreshToken`, …).
**401** — refresh token invalid/expired/revoked → client must re-login.

## POST `/api/v1/auth/change-password`  *(auth required)*

Required when `mustChangePassword=true` (only this + `logout` allowed until cleared).

**Request**
```json
{ "currentPassword": "P@ssw0rd!", "newPassword": "N3wP@ss!" }
```

**204 No Content** — password changed; `mustChangePassword` cleared; emits `PasswordChanged`; all refresh tokens revoked.
**400** — validation (new password fails policy / matches current).
**401** — `currentPassword` wrong.

## POST `/api/v1/auth/logout`  *(auth required)*

Revokes the caller's refresh-token chain.

**204 No Content**

## POST `/api/v1/auth/revoke-session`  *(perm: `users.manage`)*

Administrator force-revokes another user's sessions (spec: suspend → revoke active sessions).

**Request**
```json
{ "userId": "…" }
```

**204 No Content** — emits `SessionRevoked`.
