---

description: "Task list for Healthcare System Phase 1 (Foundation & Administration)"
---

# Tasks: Healthcare System Phase 1 (Foundation & Administration)

**Input**: Design documents from `/specs/001-healthcare-phase-one/`
**Branch**: `001-healthcare-phase-one`
**Plan**: [plan.md](./plan.md) · **Spec**: [spec.md](./spec.md)

**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/, quickstart.md, constitution.md.

**Tests**: **INCLUDED** — Constitution Principle 15 mandates tests for every feature (unit + integration; meaningful assertions). Write tests alongside/after implementation per slice; verify they pass before a checkpoint.

**Migrations (hard gate)**: every schema change must (1) **generate cleanly** via `dotnet ef migrations add` and (2) **apply successfully to PostgreSQL** via `dotnet ef database update` (or auto-applied on startup). A migration task is **not done** until the schema exists in a real database — the EF InMemory provider is for unit tests only and never validates migrations.

**Organization**: Tasks grouped by user story. Backend uses **project-per-layer** within the module so DDD dependency rules are compiler-enforced by project references.

> **Note on numbering**: tasks T016, T017, T024, and T101 were removed when durable Outbox/Inbox and Architecture Tests were dropped from Phase 1 (see [plan.md](./plan.md) §Architecture Decisions). IDs are intentionally non-contiguous to keep cross-references stable.

## User Story → Priority Map

| Story | Title | Priority | Spec FRs | Contracts |
|-------|-------|----------|----------|-----------|
| US1 | Staff Authentication & Sessions | **P1 (MVP)** | FR1, FR14, FR16 | auth.api.md |
| US2 | Patient Registration & Directory | P2 | FR6, FR7, FR8, FR12 | patients.api.md, patient-admitted.integration.md |
| US3 | User & Role Management | P2 | FR2, FR3, FR4, FR5 | users.api.md, roles.api.md |
| US4 | Facility Management | P3 | FR9 | facilities.api.md |
| US5 | Audit Log Review | P3 | FR10 | audit.api.md |

## Format: `[ID] [P?] [Story] Description`

- **[P]**: parallelizable (different files, no dependency on incomplete tasks)
- **[Story]**: US1–US5 for story-phase tasks; none in Setup/Foundational/Polish
- Exact file paths included for every task

## Path Conventions (web app)

- Backend: `backend/src/...` (modular monolith), tests: `backend/tests/...`
- Frontend: `frontend/healthcare-web/src/app/...`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Solution/projects, dependencies, tooling, governance alignment.

- [ ] T001 Amend constitution/docs stack drift: update Angular `20 → 22`, test runner Karma → Vitest, and `styles.css → styles.scss` references in `.specify/memory/constitution.md`, `docs/frontend-architecture.md`, `docs/angular-guidelines.md`, `docs/testing.md`
- [ ] T002 [P] Create `backend/src/Shared/Healthcare.Shared.Kernel` project (`.csproj`: net10.0, Nullable, ImplicitUsings)
- [ ] T003 [P] Create Administration layer projects under `backend/src/Modules/Administration/`: `Healthcare.Administration.Domain`, `Healthcare.Administration.Application`, `Healthcare.Administration.IntegrationEvents`, `Healthcare.Administration.Infrastructure`, `Healthcare.Administration.Api`
- [ ] T004 [P] Create backend test projects under `backend/tests/`: `Healthcare.Administration.UnitTests`, `Healthcare.Administration.IntegrationTests`, `Healthcare.Shared.UnitTests`
- [ ] T005 Register all new projects in `HealthcareSystem.slnx` and set project references per dependency rules (Domain→Kernel; Application→Domain,IntegrationEvents; Infrastructure→Domain,Application; Api→Application,IntegrationEvents; bootstrapper `Healthcare.Api`→all)
- [ ] T006 Add NuGet packages: MediatR, FluentValidation, Microsoft.EntityFrameworkCore + Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.AspNetCore.Authentication.JwtBearer, Serilog.*(ASP.NET Core + PostgreSQL), AspNetCore.HealthChecks, BCrypt.Net-Next; test: xUnit, FluentAssertions, Microsoft.EntityFrameworkCore.InMemory, Testcontainers.PostgreSql
- [ ] T007 [P] Add `Directory.Build.props` (`backend/`) for shared TFM/Nullable/ImplicitUsings + `Directory.Packages.props` (central package management) and `.editorconfig`; wire `dotnet format` verify
- [ ] T008 [P] Add `docker-compose.yml` (repo root) for PostgreSQL 16 service and `backend/src/Bootstrapper/Healthcare.Api/Dockerfile` review
- [ ] T009 Configure `backend/src/Bootstrapper/Healthcare.Api/appsettings.json` + `appsettings.Development.json`: connection string, Jwt section, Bootstrap section, Cors, Serilog
- [ ] T010 [P] Frontend: rename `frontend/healthcare-web/src/styles.css` → `styles.scss`, confirm Tailwind 4 PostCSS in `.postcssrc.json`, add API base URL env config (`src/environments/`), set `app.routes.ts`/`app.config.ts` skeleton
- [ ] T011 [P] Frontend: add `src/styles.scss` Tailwind layers + design tokens; remove default `WeatherForecast` sample from backend

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Kernel, reliability bus, persistence base, cross-cutting concerns, auth/authorization wiring, and shared domain primitives that ALL stories depend on.

**⚠️ CRITICAL**: No user-story work may begin until this phase is complete.

### Shared kernel
- [ ] T012 [P] Create the generic `Id` base type (`record Id(Guid Value)` + `NewValue()` → `Guid.CreateVersion7()`) in `backend/src/Shared/Healthcare.Shared.Kernel/Ids/Id.cs`. Module-agnostic — concrete IDs are owned by their bounded context (T012a).
- [ ] T012a [P] Create Administration's strongly-typed IDs (`UserId`, `RoleId`, `PatientId`, `FacilityId`) in `backend/src/Modules/Administration/Healthcare.Administration.Domain/Ids/`, deriving from the kernel `Id` base. Register their EF Core value converters in `Healthcare.Administration.Infrastructure` (the generic `StronglyTypedIdConverter<TId>` lives with the module's persistence mapping, not the kernel).
- [ ] T013 [P] Create `Result`/`Result<T>` + domain error types in `backend/src/Shared/Healthcare.Shared.Kernel/Results/`
- [ ] T014 [P] Create base `Entity<TId>`, `AggregateRoot<TId>`, domain-event base, `IDomainEvent`, `IClock`/`SystemClock` in `backend/src/Shared/Healthcare.Shared.Kernel/Domain/`
- [ ] T015 [P] Create shared value objects `Email`, `Phone`, `Address`, `DateOfBirth`, `Mrn` in `backend/src/Shared/Healthcare.Shared.Kernel/ValueObjects/` with validation rules

### Integration event publishing
- (T016–T017 removed) Durable integration bus + Outbox/Inbox deferred — no cross-module consumer exists in Phase 1. `PatientAdmitted` publishes via a MediatR `INotification` in-process; see T058/T059.

### Administration persistence + DDD base
- [ ] T018 [P] Create Administration domain base: `AdministrationDbContext` schema constant, and EF Core configurations base in `backend/src/Modules/Administration/Infrastructure/Persistence/`
- [ ] T019 Create `AdministrationDbContext` (`administration` schema) + DI registration; bootstrapper applies pending migrations on startup against real PostgreSQL. Prove it end-to-end: `dotnet ef migrations add Initial -p src/Modules/Administration/Infrastructure -s src/Bootstrapper/Healthcare.Api` then `dotnet ef database update -p src/Modules/Administration/Infrastructure -s src/Bootstrapper/Healthcare.Api` against a real Postgres DB (verify the `administration` schema is created). File: `backend/src/Modules/Administration/Infrastructure/Persistence/AdministrationDbContext.cs`

### Cross-cutting (bootstrapper)
- [ ] T020 [P] Global exception handling → RFC 9457 ProblemDetails + safe messages in `backend/src/Bootstrapper/Healthcare.Api/Middleware/`
- [ ] T021 [P] Serilog structured logging + trace-id correlation middleware in `backend/src/Bootstrapper/Healthcare.Api/`
- [ ] T022 [P] Health checks (`/health` self + DB) + OpenAPI in Development in `backend/src/Bootstrapper/Healthcare.Api/Program.cs`
- [ ] T023 MediatR registration with FluentValidation pipeline behavior (validates before handler) in `backend/src/Bootstrapper/Healthcare.Api/DependencyInjection/`
- (T024 removed) Domain-event-to-Outbox interceptor — no Outbox in Phase 1.
- [ ] T025 [P] Define permission constants + authorization policy provider mapping permission claim → policy (default-deny) in `backend/src/Modules/Administration/Infrastructure/Authorization/` and register in bootstrapper
- [ ] T026 [P] JWT bearer authentication wiring (validate iss/aud/exp/signature) in `backend/src/Bootstrapper/Healthcare.Api/DependencyInjection/AuthConfiguration.cs`
- [ ] T027 Create `AuditEntry` aggregate (append-only) + `IAuditWriter` port in `backend/src/Modules/Administration/Domain/Audit/AuditEntry.cs` and `Application/Auditing/IAuditWriter.cs`
- [ ] T028 [P] Implement `IAuditWriter` (EF) and write audit rows directly from command handlers / a minimal action filter for PHI reads in `backend/src/Modules/Administration/Infrastructure/Auditing/AuditWriter.cs`

### Seed data
- [ ] T029 Permission catalog + seed roles (Administrator/Receptionist/Clinician) + bootstrap administrator (from config, `MustChangePassword=true`) in `backend/src/Modules/Administration/Infrastructure/Persistence/Seed/` and run on startup

### Frontend foundation
- [ ] T030 [P] Configure `app.config.ts`: `provideHttpClient(withInterceptors([...]))`, `provideRouter(routes, withPreloading)`, error/global error handlers
- [ ] T031 [P] Interfaces: `common/` (paged result, problem-details), `auth.interface.ts`, `user.interface.ts`, `role.interface.ts`, `patient.interface.ts`, `facility.interface.ts`, `audit.interface.ts` in `frontend/healthcare-web/src/app/interfaces/`
- [ ] T032 [P] Shared presentational components: button, card, data-table, pagination, input/form-field, spinner, empty-state in `frontend/healthcare-web/src/app/shared/components/`
- [ ] T033 [P] Layouts: `layouts/main-layout/` (header + sidebar + `<router-outlet>`) and `layouts/auth-layout/` (centered card) with `.html`/`.scss`/`.ts`
- [ ] T034 [P] `core/interceptors/error.interceptor.ts` mapping ProblemDetails → user-friendly messages/toasts

**Checkpoint**: Kernel + persistence + auth wiring + audit + seed + frontend shell ready; the initial migration applies to PostgreSQL and `/health` reports the DB healthy. User-story implementation can begin.

---

## Phase 3: User Story 1 — Staff Authentication & Sessions (Priority: P1) 🎯 MVP

**Goal**: A staff member signs in (JWT), refreshes tokens, changes password (incl. forced first-login), and signs out; suspended users are blocked. Bootstrap admin works.
**Independent Test**: [quickstart.md](./quickstart.md) V1 + V6 — login as bootstrap admin, forced password change, suspend a user revokes access.

### Tests (US1)
- [ ] T035 [P] [US1] Unit tests for password hashing + refresh-token rotation/reuse-detection in `backend/tests/Healthcare.Administration.UnitTests/Auth/`
- [ ] T036 [P] [US1] Unit tests for login throttling + suspended-user rejection in `backend/tests/Healthcare.Administration.UnitTests/Auth/`
- [ ] T037 [P] [US1] Integration tests for `/api/v1/auth/login|refresh|change-password|logout` (status codes, ProblemDetails, claims) in `backend/tests/Healthcare.Administration.IntegrationTests/Auth/`
- [ ] T038 [P] [US1] Frontend specs for AuthService + auth interceptor + login/change-password pages in `frontend/healthcare-web/src/app/features/auth/**/*.spec.ts`

### Backend (US1)
- [ ] T039 [P] [US1] `User` domain events: `UserRegistered`, `PasswordChanged`, `SessionRevoked` in `backend/src/Modules/Administration/Domain/Users/Events/`
- [ ] T040 [P] [US1] `IPasswordHasher` (BCrypt) port + impl in `backend/src/Modules/Administration/Domain/Users/IPasswordHasher.cs` and `Infrastructure/Identity/`
- [ ] T041 [P] [US1] `RefreshToken` entity + rotation/reuse-detection logic in `backend/src/Modules/Administration/Domain/Users/RefreshToken.cs`
- [ ] T042 [US1] `ITokenService` (JWT issuance: access + refresh, claims = sub/name/role/permission) in `backend/src/Modules/Administration/Application/Auth/ITokenService.cs` + impl in `Infrastructure/Identity/JwtTokenService.cs`
- [ ] T043 [US1] Login command slice: `LoginCommand`, `LoginValidator`, `LoginHandler` (throttle, verify, suspended check, issue tokens) in `backend/src/Modules/Administration/Application/Auth/Login/`
- [ ] T044 [US1] Refresh command slice: `RefreshTokenCommand`/`Validator`/`Handler` (rotate, reuse-detect) in `backend/src/Modules/Administration/Application/Auth/Refresh/`
- [ ] T045 [US1] Change-password command slice (clears `MustChangePassword`, revokes refresh chain) in `backend/src/Modules/Administration/Application/Auth/ChangePassword/`
- [ ] T046 [US1] Logout + admin `RevokeSession` command slices in `backend/src/Modules/Administration/Application/Auth/{Logout,RevokeSession}/`
- [ ] T047 [US1] Auth endpoints module (`/api/v1/auth/...`) in `backend/src/Modules/Administration/Api/AuthEndpoints.cs`

### Frontend (US1)
- [ ] T048 [P] [US1] `core/services/token.service.ts` (in-memory signal token store) + `core/services/auth.service.ts` (login/refresh/change-password/logout) in `frontend/healthcare-web/src/app/core/services/`
- [ ] T049 [US1] `core/interceptors/auth.interceptor.ts` (attach bearer; on 401 → one silent refresh then redirect) + `core/guards/auth.guard.ts`, `role.guard.ts` in `frontend/healthcare-web/src/app/core/`
- [ ] T050 [US1] `features/auth/login/` page + `features/auth/change-password/` page (Reactive Forms, validation, loading/error states) in `frontend/healthcare-web/src/app/features/auth/`
- [ ] T051 [US1] Top-level routes: `/login`, `/change-password` (auth-layout), and `main-layout` with lazy child routes in `frontend/healthcare-web/src/app/app.routes.ts`

**Checkpoint**: Authentication MVP functional and independently testable (bootstrap admin → forced change → token-protected calls).

---

## Phase 4: User Story 2 — Patient Registration & Directory (Priority: P2)

**Goal**: Authorized staff register a patient (server-generated unique MRN), browse/search/sort/paginate the directory, view/update/deactivate a patient; `PatientAdmitted` is published in-process; PHI reads audited.
**Independent Test**: [quickstart.md](./quickstart.md) V3, V4, V7 — register patient (201 + MRN), validation errors (422), audit on view, in-process event published.

### Tests (US2)
- [ ] T052 [P] [US2] Unit tests for `Patient` aggregate invariants + MRN generation/uniqueness in `backend/tests/Healthcare.Administration.UnitTests/Patients/`
- [ ] T053 [P] [US2] Integration tests for `/api/v1/patients` CRUD + pagination/filter/sort + audit-on-view in `backend/tests/Healthcare.Administration.IntegrationTests/Patients/`
- [ ] T054 [P] [US2] Integration test asserting `PatientAdmitted` is published in-process on patient create in `backend/tests/Healthcare.Administration.IntegrationTests/Patients/PatientAdmittedTests.cs`
- [ ] T055 [P] [US2] Frontend specs for patient directory + register form + detail in `frontend/healthcare-web/src/app/features/patients/**/*.spec.ts`

### Backend (US2)
- [ ] T056 [P] [US2] `Patient` aggregate + value objects + events (`PatientRegistered`, `PatientUpdated`, `PatientDeactivated`) in `backend/src/Modules/Administration/Domain/Patients/`
- [ ] T057 [US2] Sequential/formatted MRN generator (e.g. `MRN-000001`) backed by a unique DB index in `backend/src/Modules/Administration/Domain/Patients/MrnGenerator.cs`
- [ ] T058 [US2] `PatientAdmitted` contract as a plain record + MediatR `INotification` in `backend/src/Modules/Administration/IntegrationEvents/PatientAdmittedV1.cs`
- [ ] T059 [US2] CreatePatient slice: `Command`/`Validator`/`Handler`/`Response` (generate MRN, audit, publish `PatientAdmitted` via MediatR) in `backend/src/Modules/Administration/Application/Patients/CreatePatient/`
- [ ] T060 [P] [US2] UpdatePatient + DeactivatePatient slices in `backend/src/Modules/Administration/Application/Patients/{UpdatePatient,DeactivatePatient}/`
- [ ] T061 [US2] Patient query slice (search/filter/sort/paginate, `AsNoTracking` projection) + read model in `backend/src/Modules/Administration/Application/Patients/GetPatients/`
- [ ] T062 [P] [US2] EF configuration + unique MRN index + FK indexes in `backend/src/Modules/Administration/Infrastructure/Persistence/Configurations/PatientConfiguration.cs`
- [ ] T063 [P] [US2] EF migration for `administration.Patients`: generate (`dotnet ef migrations add`) **and apply to PostgreSQL** (`dotnet ef database update`) — verify the table + unique MRN index exist before closing the story (non-destructive)
- [ ] T064 [US2] Patient endpoints module (`/api/v1/patients`) in `backend/src/Modules/Administration/Api/PatientsEndpoints.cs`

### Frontend (US2)
- [ ] T065 [P] [US2] `features/patients/services/patient.service.ts` (typed via `interfaces/patient.interface.ts`) in `frontend/healthcare-web/src/app/features/patients/`
- [ ] T066 [US2] `features/patients/list/` directory page (search/filter/sort/pagination, loading/error/empty) in `frontend/healthcare-web/src/app/features/patients/list/`
- [ ] T067 [US2] `features/patients/register/` form (Reactive Forms, client-side validation mirroring backend) in `frontend/healthcare-web/src/app/features/patients/register/`
- [ ] T068 [US2] `features/patients/detail/` page (audited view) + lazy child routes under `main-layout` in `frontend/healthcare-web/src/app/features/patients/patients.routes.ts`

**Checkpoint**: Full patient registration & directory flow independently testable.

---

## Phase 5: User Story 3 — User & Role Management (Priority: P2)

**Goal**: An Administrator manages staff users (create/suspend/reactivate, assign roles) and defines roles + permission sets; role/permission claims flow into issued tokens.
**Independent Test**: [quickstart.md](./quickstart.md) V2 — create a Receptionist, sign in, denied `POST /users` (403), attempt audited.

### Tests (US3)
- [ ] T069 [P] [US3] Unit tests for `User`/`Role` aggregate invariants (suspend/reactivate, permission rules, system-role protection) in `backend/tests/Healthcare.Administration.UnitTests/Users/`
- [ ] T070 [P] [US3] Integration tests for `/api/v1/users` + `/api/v1/roles` + `/api/v1/permissions` incl. 403/409/422 + audit in `backend/tests/Healthcare.Administration.IntegrationTests/Users/`
- [ ] T071 [P] [US3] Frontend specs for user/role management pages in `frontend/healthcare-web/src/app/features/administration/**/*.spec.ts`

### Backend (US3)
- [ ] T072 [P] [US3] `User` aggregate + events (`UserRegistered`, `UserSuspended`, `UserReactivated`, `RoleAssigned`, `RoleRevoked`, `UserUpdated`) in `backend/src/Modules/Administration/Domain/Users/`
- [ ] T073 [P] [US3] `Role` aggregate + `Permission` value object + events (`RoleCreated`, `PermissionGranted`, `PermissionRevoked`) in `backend/src/Modules/Administration/Domain/Roles/`
- [ ] T074 [P] [US3] EF configurations + UserRoles join + unique indexes (UserName, Email, Role.Name) in `backend/src/Modules/Administration/Infrastructure/Persistence/Configurations/`
- [ ] T075 [P] [US3] EF migration for `administration.{Users, Roles, UserRoles, RefreshTokens}`: generate (`dotnet ef migrations add`) **and apply to PostgreSQL** — verify tables + unique indexes (UserName, Email, Role.Name) exist before closing the story
- [ ] T076 [US3] CreateUser slice (temp password, `MustChangePassword=true`, role assignment, audit) in `backend/src/Modules/Administration/Application/Users/CreateUser/`
- [ ] T077 [P] [US3] UpdateUser + SuspendUser + ReactivateUser slices in `backend/src/Modules/Administration/Application/Users/{UpdateUser,SuspendUser,ReactivateUser}/`
- [ ] T078 [US3] AssignUserRoles slice (PUT `/users/{id}/roles`, diff → events) in `backend/src/Modules/Administration/Application/Users/AssignUserRoles/`
- [ ] T079 [P] [US3] GetUsers query slice (search/filter/sort/paginate) in `backend/src/Modules/Administration/Application/Users/GetUsers/`
- [ ] T080 [US3] Role CRUD slices + GetPermissions catalog query in `backend/src/Modules/Administration/Application/Roles/`
- [ ] T081 [US3] Users + Roles + Permissions endpoints modules in `backend/src/Modules/Administration/Api/{UsersEndpoints,RolesEndpoints}.cs`

### Frontend (US3)
- [ ] T082 [P] [US3] `features/administration/services/{user.service.ts, role.service.ts}` in `frontend/healthcare-web/src/app/features/administration/`
- [ ] T083 [US3] `features/administration/users/` pages (list + create/edit + suspend/reactivate) in `frontend/healthcare-web/src/app/features/administration/users/`
- [ ] T084 [US3] `features/administration/roles/` pages (list + create/edit permissions) in `frontend/healthcare-web/src/app/features/administration/roles/`
- [ ] T085 [US3] Administration child routes guarded by `roleGuard(['Administrator'])` in `frontend/healthcare-web/src/app/features/administration/administration.routes.ts`

**Checkpoint**: Administrator can fully staff the system; permissions enforce correctly.

---

## Phase 6: User Story 4 — Facility Management (Priority: P3)

**Goal**: An Administrator registers and manages facilities (wards/departments/locations) and deactivates referenced ones.
**Independent Test**: [quickstart.md](./quickstart.md) V8 — create/list/deactivate facility; register patient referencing it.

### Tests (US4)
- [ ] T086 [P] [US4] Unit tests for `Facility` invariants (unique code, no delete when referenced) in `backend/tests/Healthcare.Administration.UnitTests/Facilities/`
- [ ] T087 [P] [US4] Integration tests for `/api/v1/facilities` CRUD + 409 + 422 (referenced) in `backend/tests/Healthcare.Administration.IntegrationTests/Facilities/`
- [ ] T088 [P] [US4] Frontend specs for facility management page in `frontend/healthcare-web/src/app/features/administration/facilities/**/*.spec.ts`

### Backend (US4)
- [ ] T089 [P] [US4] `Facility` aggregate + events (`FacilityRegistered`, `FacilityUpdated`, `FacilityDeactivated`) in `backend/src/Modules/Administration/Domain/Facilities/`
- [ ] T090 [P] [US4] EF configuration + unique `Code` index + migration for `administration.Facilities`, generated **and applied to PostgreSQL**
- [ ] T091 [P] [US4] CreateFacility / UpdateFacility / DeactivateFacility / GetFacilities slices in `backend/src/Modules/Administration/Application/Facilities/`
- [ ] T092 [US4] Facilities endpoints module in `backend/src/Modules/Administration/Api/FacilitiesEndpoints.cs`
- [ ] T093 [US4] Optional: wire Patient register form facility dropdown to `GET /api/v1/facilities` (loose `facilityId`) in `frontend/healthcare-web/src/app/features/patients/register/`

### Frontend (US4)
- [ ] T094 [US4] `features/administration/facilities/` pages (list + create/edit + deactivate) + routes (Administrator-guarded) in `frontend/healthcare-web/src/app/features/administration/facilities/`

**Checkpoint**: Facilities fully manageable; patient registration can reference a facility.

---

## Phase 7: User Story 5 — Audit Log Review (Priority: P3)

**Goal**: An Administrator queries the append-only audit log (filter by actor/action/resource/date) in a paginated view.
**Independent Test**: [quickstart.md](./quickstart.md) V5 — audit entries from patient create/view appear, newest first, paginated.

### Tests (US5)
- [ ] T095 [P] [US5] Integration tests for `/api/v1/audit-entries` filtering/pagination/sort + 403 for non-admins in `backend/tests/Healthcare.Administration.IntegrationTests/Audit/`
- [ ] T096 [P] [US5] Frontend specs for audit log page in `frontend/healthcare-web/src/app/features/administration/audit/**/*.spec.ts`

### Backend (US5)
- [ ] T097 [P] [US5] EF configuration + composite index `IX_AuditEntries_OccurredAt_ActorId` + migration for `administration.AuditEntries`, generated **and applied to PostgreSQL**
- [ ] T098 [US5] GetAuditEntries query slice (filter by from/to/actor/action/resourceType/resourceId, sort default `-occurredAt`, paginated) in `backend/src/Modules/Administration/Application/Audit/GetAuditEntries/`
- [ ] T099 [US5] Audit endpoints module (`audit.read` policy) in `backend/src/Modules/Administration/Api/AuditEndpoints.cs`

### Frontend (US5)
- [ ] T100 [US5] `features/administration/audit/` page (filters + paginated table, PHI-minimal display) + route in `frontend/healthcare-web/src/app/features/administration/audit/`

**Checkpoint**: Full audit visibility for administrators.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Quality gates spanning all stories.

- (T101 removed) Architecture tests — dependency direction enforced by project references.
- [ ] T102 [P] Run `dotnet format --verify-no-changes` and fix all findings across `backend/`
- [ ] T103 [P] Run `dotnet list package --vulnerable --include-transitive` and patch vulnerable packages
- [ ] T104 [P] Frontend: `npm run build` (strict TS, no `any`), `npm test -- --watch=false` (Vitest), Prettier check across `frontend/healthcare-web/`
- [ ] T105 [P] Security hardening review: rate-limit login, HTTPS redirect + HSTS, no PHI in logs, safe error bodies, JWT validation correctness
- [ ] T106 [P] Add `/health` (self + DB) and verify startup seeding path is idempotent
- [ ] T107 [P] Update root `README.md` + `docs/` with how to run (link [quickstart.md](./quickstart.md)) and API/OpenAPI pointer
- [ ] T108 Run full [quickstart.md](./quickstart.md) validation scenarios V1–V8 end-to-end and fix defects
- [ ] T109 Performance: verify patient-directory query uses pagination + `AsNoTracking` + projection + MRN unique index; add missing indexes flagged by EF
- [ ] T110 Drop/recreate the database from scratch and confirm **all migrations apply cleanly** on startup (bootstrapper applies pending migrations to PostgreSQL), `/health` reports the DB healthy, and seed runs idempotently — the InMemory provider must never satisfy this gate

---

## Dependencies & Execution Order

### Phase Dependencies
- **Setup (Phase 1)**: no dependencies; start immediately.
- **Foundational (Phase 2)**: depends on Setup; **BLOCKS all user stories**.
- **User Stories (Phase 3–7)**: each depends on Foundational completion.
  - Recommended order: **US1 (P1) → US2 (P2) → US3 (P2) → US4 (P3) → US5 (P3)**.
  - US2 (Patients) is independently testable using the bootstrap Administrator (no dependency on US3).
  - US3 (Users/Roles) builds on the seeded roles/permissions; no dependency on US2.
  - US4 (Facilities) is optional for US2 (patient `facilityId` is a loose reference); T093 links them.
  - US5 (Audit) reads entries written by all stories; seed/sample data keeps it independently testable.
- **Polish (Phase 8)**: depends on all desired stories being complete.

### Within Each Story
Tests (fail first or alongside) → Domain/aggregates → Application slices → Infrastructure (EF/migration) → Api endpoints → Frontend → checkpoint.

### Parallel Opportunities
- Phase 1/2 `[P]` tasks run in parallel (distinct files/projects).
- Per story: unit tests, domain entities, EF configs, and frontend services marked `[P]` run in parallel.
- Different stories can proceed in parallel after Phase 2 (e.g., US2 backend ‖ US3 frontend) if staffed — different module slices/files.

## Parallel Example: User Story 2

```text
# Parallel backend domain/config/migration (distinct files):
Task T056: Patient aggregate (Domain/Patients/)
Task T062: EF configuration (Infrastructure/Persistence/Configurations/)
Task T058: PatientAdmitted contract (IntegrationEvents/)

# Parallel tests (distinct files):
Task T052: aggregate unit tests
Task T054: integration/contract tests
Task T055: frontend specs
```

## Implementation Strategy

### MVP First (User Story 1 only)
1. Complete Phase 1 (Setup) → 2. Complete Phase 2 (Foundational) → 3. Complete Phase 3 (US1 Auth) → 4. **STOP & VALIDATE** [quickstart.md](./quickstart.md) V1/V6 → demo.

### Incremental Delivery
1. Foundation → 2. + US1 Auth (MVP) → 3. + US2 Patients → 4. + US3 Users/Roles → 5. + US4 Facilities → 6. + US5 Audit → 7. Polish. Each story adds value without breaking prior stories.

### Suggested MVP scope
**US1 (Authentication & Sessions)** — it is the security gate for every other story and is fully independently testable. A strong second increment is **US2 (Patients)**, which delivers the headline clinic value.

## Notes
- `[P]` = different files, no dependency on incomplete tasks.
- `[US#]` labels trace tasks to spec FRs/contracts.
- Commit per task or logical group (Conventional Commits, Constitution Principle 22).
- Verify the quickstart checkpoint before advancing a story.
