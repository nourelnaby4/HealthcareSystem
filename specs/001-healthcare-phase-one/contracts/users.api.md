# Users API Contract

> All routes require auth. Default-deny; each route's required permission is noted.

Resource shape (read):

```json
{
  "id": "…",
  "userName": "jdoe",
  "email": "jane.doe@hospital.org",
  "firstName": "Jane",
  "lastName": "Doe",
  "status": "Active",
  "mustChangePassword": false,
  "roles": [ { "id": "…", "name": "Receptionist" } ],
  "createdAt": "2026-08-01T09:00:00Z",
  "updatedAt": "2026-08-03T12:00:00Z"
}
```

## GET `/api/v1/users`  *(perm: `users.read` / `users.manage`)*

Query: `page`, `pageSize`, `q` (userName/email/name), `status` (`Active`|`Suspended`), `role`, `sort`. Returns the [collection envelope](./common-conventions.md).

## POST `/api/v1/users`  *(perm: `users.manage`)* → `201` + `Location`

Create a staff user; a temporary password is set (returned once) with `mustChangePassword=true`.

```json
{
  "userName": "jdoe",
  "email": "jane.doe@hospital.org",
  "firstName": "Jane",
  "lastName": "Doe",
  "temporaryPassword": "TempP@ss1!",
  "roleIds": ["…"]
}
```

**409** — userName/email already exists. Emits `UserRegistered`.

## GET `/api/v1/users/{id}`  *(perm: `users.read` / `users.manage`)* → `200` / `404`.

## PATCH `/api/v1/users/{id}`  *(perm: `users.manage`)* → `200`

Partial update of mutable profile fields (email, first/last name). `204` if no content returned. Emits `UserUpdated`.

## POST `/api/v1/users/{id}:suspend`  *(perm: `users.manage`)* → `204`

Sets `Status=Suspended`, revokes sessions (`SessionRevoked`), emits `UserSuspended`.

## POST `/api/v1/users/{id}:reactivate`  *(perm: `users.manage`)* → `204`

`Status=Active`; emits `UserReactivated`.

## PUT `/api/v1/users/{id}/roles`  *(perm: `users.manage`)* → `200`

Replace the user's role set.

```json
{ "roleIds": ["…", "…"] }
```

Emits `RoleAssigned` / `RoleRevoked` per diff. **422** if assigning an unknown role.

## DELETE `/api/v1/users/{id}`  *(perm: `users.manage`)* → `204` / **422** if referenced (prefer suspend). Soft-delete only; never hard-delete audited users.
