# API Design

**Status**: Controlled standard — enforced by [Constitution Principle 8 (API Design)](../.specify/memory/constitution.md)
**Applies to**: all HTTP endpoints exposed by backend modules.

---

## 1. Style

- **RESTful** APIs.
- Use the **correct HTTP verb** for the intent.
- Return **proper status codes**.
- Use **`ProblemDetails`** (RFC 9457) for all error responses.
- Expose **OpenAPI** documentation for every endpoint.
- Apply **API versioning** when a contract must evolve incompatibly.

---

## 2. HTTP Verbs

| Verb | Intent | Safe | Idempotent |
|------|--------|------|------------|
| `GET` | Read resource / collection | ✅ | ✅ |
| `POST` | Create resource / trigger action | ❌ | ❌ |
| `PUT` | Full replace | ❌ | ✅ |
| `PATCH` | Partial update | ❌ | ✅ |
| `DELETE` | Remove resource | ❌ | ✅ |

- Never overload `GET` to mutate state.
- `POST` returns `201 Created` with a `Location` header for new resources.

---

## 3. Status Codes

| Code | Use for |
|------|---------|
| `200 OK` | Successful read / update |
| `201 Created` | Resource created |
| `204 No Content` | Successful delete / no-body update |
| `400 Bad Request` | Validation / malformed input |
| `401 Unauthorized` | Missing/invalid authentication |
| `403 Forbidden` | Authenticated but not permitted |
| `404 Not Found` | Resource does not exist |
| `409 Conflict` | Concurrency / duplicate |
| `422 Unprocessable Entity` | Valid syntax, failed business rules |
| `500 Internal Server Error` | Unhandled (log internally; return safe message) |

---

## 4. Collections: Pagination, Filtering, Sorting, Search

All list endpoints MUST support:

- **Pagination** — e.g. `?page=1&pageSize=20` (or `offset`/`limit`). Return totals in the response envelope.
- **Filtering** — e.g. `?status=active`.
- **Sorting** — e.g. `?sort=-createdAt` (`-` = descending).
- **Searching** — e.g. `?q=term` for free-text.

Default to a sane page size; cap the maximum.

---

## 5. Versioning

- Version breaking changes in the route or media type (e.g. `/api/v2/...`).
- Keep old versions available until consumers migrate.
- Version **integration-event** contracts separately (`…V2`) — see [architecture.md](./architecture.md).

---

## 6. DTOs & Contracts

- Never expose domain entities directly. Map to **DTOs** at the boundary.
- Use explicit request/response models per endpoint.
- Keep DTOs flat and stable; they are the published contract.

---

## 7. Consistency

- Plural, lowercase, kebab-case resource routes: `/api/patients`, `/api/lab-test-requests`.
- Nest only one level: `/api/patients/{id}/encounters`.
- Stable field names (camelCase JSON); don't rename fields silently.

---

## 8. Errors

- All errors return `application/problem+json` (`ProblemDetails`) with `type`, `title`, `status`, `detail`, and (where useful) `errors` for validation.
- Never leak stack traces, internal paths, or SQL.
- Correlate errors with a request id returned to the client.

⬅️ [Back to Docs index](./README.md)
