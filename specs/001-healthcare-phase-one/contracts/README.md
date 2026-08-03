# API Contracts — Healthcare System Phase 1 (Administration)

Base URL: `/api/v1`. Content type: `application/json` (errors: `application/problem+json`).
Authentication: bearer JWT in `Authorization: Bearer <token>` for all endpoints except login/refresh.
Authorization: default-deny; each route lists the required permission claim. See [common-conventions.md](./common-conventions.md).

| Resource group | Contract |
|----------------|----------|
| Auth | [auth.api.md](./auth.api.md) |
| Users & role assignment | [users.api.md](./users.api.md) |
| Roles & permissions | [roles.api.md](./roles.api.md) |
| Patients | [patients.api.md](./patients.api.md) |
| Facilities | [facilities.api.md](./facilities.api.md) |
| Audit log | [audit.api.md](./audit.api.md) |
| Integration events | [patient-admitted.integration.md](./patient-admitted.integration.md) |
| Conventions (envelope, pagination, errors) | [common-conventions.md](./common-conventions.md) |
