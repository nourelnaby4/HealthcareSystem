# Integration Event Contract: PatientAdmitted (v1)

**Status**: Published by Administration via a MediatR `INotification` (in-process). **No consumer ships in Phase 1** (Clinical arrives later). Durable at-least-once delivery (Outbox/Inbox) is deferred to the phase that adds the first consumer. Versioned: bump to `PatientAdmittedV2` on incompatible change.

## Semantics

Published when a patient is registered by Administration. In Phase 1 it is published in-process; when a consumer exists it will be delivered at-least-once and consumers must be idempotent (dedupe by `id`).

## Message envelope

```json
{
  "id": "b9e1… (guid, idempotency key)",
  "type": "PatientAdmitted",
  "version": 1,
  "occurredAt": "2026-08-03T12:00:00Z",
  "payload": {
    "patientId": "… (guid)",
    "mrn": "MAIN-000123",
    "firstName": "Ahmed",
    "lastName": "Hassan",
    "dateOfBirth": "1990-05-12",
    "gender": "Male",
    "facilityId": "… (guid)"
  }
}
```

## Fields

| Field | Type | Notes |
|-------|------|-------|
| `id` | Guid | Message id = idempotency key (used for dedupe once durable delivery ships). |
| `type` | string | Always `PatientAdmitted` for v1. |
| `version` | int | Contract version. |
| `occurredAt` | DateTimeOffset (UTC) | When the patient was registered. |
| `payload.patientId` | Guid | Identity of the patient (stable across modules). |
| `payload.mrn` | string | The unique MRN (display/reference). |
| `payload.firstName/lastName` | string | Demographic snapshot. |
| `payload.dateOfBirth` | date | ISO-8601. |
| `payload.gender` | string? | Optional. |
| `payload.facilityId` | Guid | Admitting facility. |

## Delivery guarantees

- **Phase 1**: published in-process via MediatR in the same request that creates the patient. No Outbox/Inbox tables exist yet.
- **Future (when a consumer ships)**: Outbox row written in the same transaction; hosted dispatcher publishes; consumers dedupe via `InboxMessage(id)` for exactly-once effect despite at-least-once delivery.
- **No PHI beyond the snapshot**: payload contains the minimum needed to open an encounter. Consumers translate into their own model (Customer-Supplier `U/D`, Administration upstream).
- **Privacy**: this event is logged as metadata only (type/id/occurredAt) — the payload is **not** written to application logs.
