# Common API Conventions

**Versioning**: route prefix `/api/v1`. Incompatible changes bump to `/api/v2`.

**Resource naming**: plural, lowercase, kebab-case (`/api/v1/audit-entries`). JSON fields are **camelCase**.

**Collections** — every list endpoint supports:

| Query | Example | Meaning |
|-------|---------|---------|
| `page` | `?page=1` | 1-based page (default 1). |
| `pageSize` | `?pageSize=20` | Default 20, capped (e.g., 100). |
| `q` | `?q=ahmed` | Free-text search (server-defined fields). |
| `sort` | `?sort=-createdAt` | `-` = descending; multiple comma-separated. |
| filters | `?status=active` | Resource-specific filters. |

**Collection response envelope**:

```json
{
  "items": [ /* …resource objects… */ ],
  "totalItems": 142,
  "page": 1,
  "pageSize": 20
}
```

**Status codes**: `200` read/update · `201` create (+ `Location` header) · `204` delete/no-body · `400` validation · `401` unauthenticated · `403` forbidden · `404` not found · `409` conflict (duplicate) · `422` business-rule failure · `429` throttled · `500` unhandled.

**Errors** — RFC 9457 `ProblemDetails` (`application/problem+json`):

```json
{
  "type": "https://httpstatuses.io/409",
  "title": "Conflict",
  "status": 409,
  "detail": "A patient with this MRN already exists.",
  "instance": "/api/v1/patients",
  "traceId": "00-abc...-01",
  "errors": {
    "mrn": ["A patient with this MRN already exists."]
  }
}
```

Never includes stack traces, internal paths, or SQL. Every response carries a `traceId` (correlation).

**Idempotency**: mutating write endpoints accept an optional `Idempotency-Key` header where relevant (creation flows).

**Auth**: `Authorization: Bearer <accessToken>`. Tokens are JWT (`iss`, `aud`, `exp`, `sub`=UserId, `name`, plus `role` + `permission` claim arrays). Default-deny; missing/invalid → `401`; insufficient permission → `403`.
