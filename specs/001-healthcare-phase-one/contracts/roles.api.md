# Roles & Permissions API Contract

> All routes require auth + `roles.manage` (read may allow `roles.read`).

Resource shape:

```json
{
  "id": "…",
  "name": "Receptionist",
  "description": "Front-desk staff: register and manage patients.",
  "isSystem": true,
  "permissions": [ "patients.read", "patients.write" ],
  "createdAt": "2026-08-01T09:00:00Z",
  "updatedAt": "2026-08-03T12:00:00Z"
}
```

## GET `/api/v1/roles`  *(perm: `roles.read`/`roles.manage`)*

Query: `page`, `pageSize`, `q` (name). Collection envelope.

## GET `/api/v1/permissions`  *(perm: `roles.read`/`roles.manage`)* → `200`

Returns the permission catalog:

```json
{ "items": [ { "value": "patients.write", "group": "patients", "description": "Register and update patients" } ] }
```

## POST `/api/v1/roles`  *(perm: `roles.manage`)* → `201` + `Location`

```json
{ "name": "Triage Nurse", "description": "…", "permissions": ["patients.read", "patients.write"] }
```

**409** — name already exists. **422** — unknown/invalid permission string. Emits `RoleCreated`.

## GET `/api/v1/roles/{id}`  *(perm: `roles.read`/`roles.manage`)* → `200` / `404`.

## PATCH `/api/v1/roles/{id}`  *(perm: `roles.manage`)* → `200`

Update `description` and/or `permissions` (add/remove). Emits `PermissionGranted`/`PermissionRevoked`.

## DELETE `/api/v1/roles/{id}`  *(perm: `roles.manage`)*

**204** if custom and unreferenced. **422** if `isSystem=true` (cannot delete seeded roles) or still assigned to users.
