# Patients API Contract

> All routes require auth. The MRN is system-generated and unique.

Resource shape:

```json
{
  "id": "…",
  "mrn": "MAIN-000123",
  "firstName": "Ahmed",
  "lastName": "Hassan",
  "dateOfBirth": "1990-05-12",
  "gender": "Male",
  "nationalId": null,
  "phone": "+201000000000",
  "email": null,
  "address": { "street": "…", "city": "Cairo", "state": null, "postalCode": null, "country": "EG" },
  "bloodType": "O+",
  "status": "Active",
  "facilityId": "…",
  "createdAt": "2026-08-03T12:00:00Z",
  "updatedAt": "2026-08-03T12:00:00Z"
}
```

## GET `/api/v1/patients`  *(perm: `patients.read`)*

Query: `page`, `pageSize`, `q` (name/MRN/phone), `status`, `gender`, `sort`. Collection envelope. Read is **audited** (`patient.view`) at the detail level.

## POST `/api/v1/patients`  *(perm: `patients.write`)* → `201` + `Location`

Register a patient. `mrn` is **not** supplied by the client — the server generates it.

```json
{
  "firstName": "Ahmed",
  "lastName": "Hassan",
  "dateOfBirth": "1990-05-12",
  "gender": "Male",
  "nationalId": null,
  "phone": "+201000000000",
  "email": null,
  "address": { "city": "Cairo", "country": "EG" },
  "bloodType": "O+",
  "facilityId": "…"
}
```

**422** — business validation (e.g., future date of birth). Emits `PatientRegistered` → writes `PatientAdmitted` to the Outbox (integration contract). The MRN is server-generated and unique, so duplicate-MRN creation is impossible; duplicate *demographic* submissions create distinct patients (person-level de-duplication is deferred).

## GET `/api/v1/patients/{id}`  *(perm: `patients.read`)* → `200` / `404`. Emits an audited `patient.view`.

## PATCH `/api/v1/patients/{id}`  *(perm: `patients.write`)* → `200`

Mutable fields only (demographics, contact, address, blood type). `mrn` and `id` are immutable → **422** if included. Emits `PatientUpdated`.

## POST `/api/v1/patients/{id}:deactivate`  *(perm: `patients.write`)* → `204`

`Status=Inactive`. Emits `PatientDeactivated`.
