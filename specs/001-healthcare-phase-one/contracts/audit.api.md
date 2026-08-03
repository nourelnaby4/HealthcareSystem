# Audit Log API Contract

> Append-only data; this API is query-only. Requires auth + `audit.read` (Administrator).

Resource shape:

```json
{
  "id": "…",
  "actorId": "…",
  "actorName": "jdoe",
  "action": "patient.create",
  "resourceType": "Patient",
  "resourceId": "…",
  "occurredAt": "2026-08-03T12:00:00Z",
  "correlationId": "00-abc…-01",
  "ipAddress": "10.0.0.5",
  "detail": "Created patient MAIN-000123"
}
```

## GET `/api/v1/audit-entries`  *(perm: `audit.read`)*

Query: `page`, `pageSize`, `from`/`to` (ISO-8601 UTC), `actorId`, `action`, `resourceType`, `resourceId`, `q` (detail/actor), `sort` (default `-occurredAt`). Collection envelope.

Results are paginated and capped (audit logs grow large). No mutation endpoints exist — audit writes happen internally during command handling and sensitive reads.
