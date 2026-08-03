# Facilities API Contract

> All routes require auth + `facilities.manage` (read may allow `facilities.read`).

Resource shape:

```json
{
  "id": "…",
  "code": "MAIN",
  "name": "Main Hospital",
  "type": "Hospital",
  "address": { "city": "Cairo", "country": "EG" },
  "status": "Active",
  "createdAt": "2026-08-01T09:00:00Z",
  "updatedAt": "2026-08-03T12:00:00Z"
}
```

`type` ∈ `Hospital` | `Clinic` | `Ward` | `Department` | `Pharmacy`.

## GET `/api/v1/facilities`  *(perm: `facilities.read`/`facilities.manage`)*

Query: `page`, `pageSize`, `q` (name/code), `type`, `status`, `sort`. Collection envelope.

## POST `/api/v1/facilities`  *(perm: `facilities.manage`)* → `201` + `Location`

```json
{ "code": "MAIN", "name": "Main Hospital", "type": "Hospital", "address": { "city": "Cairo", "country": "EG" } }
```

**409** — `code` already exists. Emits `FacilityRegistered`.

## GET `/api/v1/facilities/{id}`  *(perm: `facilities.read`/`facilities.manage`)* → `200` / `404`.

## PATCH `/api/v1/facilities/{id}`  *(perm: `facilities.manage`)* → `200`

Mutable: `name`, `address`. `code`/`type` immutable → **422**. Emits `FacilityUpdated`.

## DELETE `/api/v1/facilities/{id}`  *(perm: `facilities.manage`)*

**204** if unreferenced; **422** if patients reference it → use `deactivate` instead.

## POST `/api/v1/facilities/{id}:deactivate`  *(perm: `facilities.manage`)* → `204`

`Status=Inactive`. Emits `FacilityDeactivated`.
