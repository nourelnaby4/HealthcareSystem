# Quickstart & Validation — Healthcare System Phase 1

A **run + validation guide**, not an implementation. It proves the phase-1 feature works end-to-end against the [spec](./spec.md), using the [API contracts](./contracts/README.md) and [data model](./data-model.md). Implementation code, migrations, and full test suites belong in `tasks.md` and the implementation phase.

---

## 1. Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| .NET SDK | 10.0 | `dotnet --version` |
| Node.js | 22 | `node --version` |
| npm | 12 | `npm --version` |
| PostgreSQL | 16+ | local or Docker |
| Docker (optional) | latest | for a throwaway Postgres |

**Secrets** (from environment / secret store — never the repo):
- `ConnectionStrings__Healthcare` — PostgreSQL connection string.
- `Bootstrap__UserName`, `Bootstrap__Password`, `Bootstrap__Email` — initial administrator.
- `Jwt__SigningKey`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__AccessTokenMinutes`, `Jwt__RefreshTokenDays`.

---

## 2. Bring up dependencies

```bash
# throwaway Postgres (optional, if not running locally)
docker run --name healthcare-pg -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16
```

## 3. Backend

```bash
cd backend
dotnet restore
dotnet tool restore                       # if dotnet-format / ef tools are declared
dotnet build
dotnet ef database update --project src/Modules/Administration/Infrastructure \
                          --startup-project src/Bootstrapper/Healthcare.Api
# run the API (migrates on startup; seeds roles + bootstrap admin)
dotnet run --project src/Bootstrapper/Healthcare.Api
```

**Expected**: API starts on its configured port (e.g., `https://localhost:7xxx`), OpenAPI at `/openapi/v1.json`, and `/health` returns healthy. Console log shows seeded roles (`Administrator`, `Receptionist`, `Clinician`) and the bootstrap administrator (no password in logs).

## 4. Frontend

```bash
cd frontend/healthcare-web
npm ci
npm start            # ng serve
```

**Expected**: the SPA serves at `http://localhost:4200` and shows the login screen. Point its API base URL at the backend (via environment/config).

---

## 5. Validation scenarios (prove it works end-to-end)

Each scenario references the contract it exercises and the outcome required by the spec.

### V1 — Bootstrap admin first login & password change *(spec FR16)*
1. Sign in with bootstrap credentials → app forces **change password** (`mustChangePassword=true`).
2. Change password → `204`; subsequent calls work; old password rejected.
- *Contract*: [auth.api.md](./contracts/auth.api.md) (`login`, `change-password`).

### V2 — Role-based access is enforced *(spec FR2, scenario "Role-based access")*
1. Create a **Receptionist** user via [users.api.md](./contracts/users.api.md) (`POST /api/v1/users`).
2. Sign in as the receptionist; call `POST /api/v1/users` → **403 Forbidden** (safe ProblemDetails), and the attempt appears in the audit log.
- *Expected*: server default-deny; UI hides user-management.

### V3 — Register a patient (happy path) *(spec FR6; success criterion "< 2 min")*
1. As Receptionist, `POST /api/v1/patients` with valid demographics → **201** + `Location`; response has a server-generated **MRN**.
2. `GET /api/v1/patients/{id}` → **200**; an audit `patient.view` row is written.
3. `GET /api/v1/patients?q=<lastName>` → the patient appears in the paginated envelope.
- *Contract*: [patients.api.md](./contracts/patients.api.md).

### V4 — Validation & safe errors *(spec FR7, FR14; api-design)*
1. `POST /api/v1/patients` with a future `dateOfBirth` → **422** ProblemDetails with field errors; no partial record.
2. Unauthenticated call → **401**; malformed body → **400**. No stack traces/SQL in any error body.
- *Expected*: a `traceId` is present on every error.

### V5 — Audit trail is retrievable *(spec FR10)*
1. As Administrator, `GET /api/v1/audit-entries?resourceType=Patient` → returns the create + view entries from V3, newest first, paginated.
- *Contract*: [audit.api.md](./contracts/audit.api.md).

### V6 — Suspend a user revokes access *(spec scenario "Suspended user is blocked")*
1. Administrator `POST /api/v1/users/{id}:suspend` → **204**.
2. The suspended user's existing token is rejected (re-auth fails) → **401**.
- *Contract*: [users.api.md](./contracts/users.api.md), [auth.api.md](./contracts/auth.api.md).

### V7 — Integration event is reliable & idempotent *(spec FR12; architecture §7)*
1. Register a patient → assert an `OutboxMessage(type=PatientAdmitted)` row was written **in the same transaction** as the patient (see [data-model.md](./data-model.md) §Outbox).
2. Run/observe the hosted dispatcher → the row is marked processed; a test consumer records the `id` in `InboxMessage`.
3. Redeliver the same message id → the consumer applies **no second effect** (idempotent).
- *Contract*: [patient-admitted.integration.md](./contracts/patient-admitted.integration.md).
> This is the primary automated test of the foundation even though no production consumer exists yet.

### V8 — Facility management *(spec FR9)*
1. `POST /api/v1/facilities` (Administrator) → **201**; list + deactivate succeed.
- *Contract*: [facilities.api.md](./contracts/facilities.api.md).

---

## 6. Automated checks

```bash
# backend
dotnet test --configuration Release /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
dotnet format --verify-no-changes
dotnet list package --vulnerable --include-transitive

# frontend
cd frontend/healthcare-web
npm run build
npm test -- --watch=false      # Vitest (@angular/build:unit-test)
```

**Expected**: all tests green; format clean; no vulnerable packages; frontend build succeeds. Coverage is tracked over time (Constitution Principle 15; see [docs/testing.md](../../docs/testing.md)).

---

## 7. Definition of Done (phase 1)

All of V1–V8 pass (manually and/or as automated tests), the constitution gates in [plan.md](./plan.md) remain green, and the two documentation-drift amendments (Angular 22, Vitest) are applied. Then proceed to `/speckit.tasks` to break this into implementation tasks.
