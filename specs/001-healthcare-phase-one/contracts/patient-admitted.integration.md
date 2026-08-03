# Integration Event Contract: PatientAdmitted (v1)

**Status**: Published by Administration. **No consumer ships in phase 1** (Clinical arrives later); the bus + Outbox/Inbox deliver it reliably so the contract is safe to build now. Versioned: bump to `PatientAdmittedV2` on incompatible change.

## Semantics

Published when a patient is registered by Administration (`PatientRegistered` domain event → Outbox). At-least-once delivery; consumers must be idempotent (dedupe by `id` via the Inbox).

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
| `id` | Guid | Message id = idempotency key; consumers record it in the Inbox. |
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

- **Reliability**: the Outbox row is written in the same DB transaction as the patient; a hosted dispatcher publishes pending rows to the in-process bus.
- **Idempotency**: consumers insert `InboxMessage(id)` before processing; duplicate redelivery yields exactly one effect.
- **No PHI beyond the snapshot**: payload contains the minimum needed to open an encounter. Consumers translate into their own model (Customer-Supplier `U/D`, Administration upstream).
- **Privacy**: this event is logged as metadata only (type/id/occurredAt) — the payload is **not** written to application logs.
