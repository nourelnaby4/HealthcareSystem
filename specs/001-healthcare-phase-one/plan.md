# Implementation Plan: Healthcare System — Phase 1 (Foundation & Administration)

**Branch**: `001-healthcare-phase-one` | **Date**: 2026-08-04 | **Spec**: [spec.md](./spec.md)

## Summary

Phase 1 stands up the modular-monolith foundation and the **Administration** module: staff authentication (JWT + refresh), role/permission authorization, patient registration with a server-generated unique MRN, facility management, and PHI access auditing — exposed through REST and an Angular SPA.

The foundation is deliberately lean: a single `Shared` kernel (IDs, base entity types, value objects) and one `administration` schema. Cross-module messaging infrastructure (durable outbox/inbox, in-process bus) is **deferred** to the phase that introduces a second module and its first real consumer — there is nothing to deliver to in Phase 1, so building it now is premature. The `PatientAdmitted` event is defined as a plain contract and published in-process today; it gains durable delivery only when a subscriber exists.

## Technical Context

- **Backend**: C# 13 / **.NET 10**, ASP.NET Core Minimal APIs, **EF Core 10** + Npgsql (PostgreSQL), **MediatR** (thin command/query dispatch), **FluentValidation**, JWT bearer auth, **Serilog**, `BCrypt.Net-Next`.
- **Frontend**: **Angular 22** standalone components + **Signals**, Reactive Forms, **Tailwind CSS 4**, **Vitest**. TypeScript 6.
- **Storage**: **PostgreSQL** — one schema per module. Phase 1 creates the `administration` schema.
- **Testing**: xUnit + FluentAssertions; EF integration tests against Testcontainers PostgreSQL; unit tests use the EF InMemory provider. Frontend: Vitest colocated `*.spec.ts`.
- **Errors**: exceptions → RFC 9457 ProblemDetails middleware; no stack traces; safe messages; a `traceId` on every error.
- **Performance**: patient-directory reads p95 < 300 ms (pagination + `AsNoTracking` + projection); login p95 < 500 ms.
- **Constraints**: default-deny authorization; PHI never logged; HTTPS only; every async method forwards `CancellationToken`. **Every schema change ships as an EF Core migration that must generate cleanly *and* apply successfully to PostgreSQL** (`dotnet ef migrations add` + `database update`); the bootstrapper applies pending migrations on startup against a real database (never the EF InMemory provider for schema/migration verification).
- **Scope**: 1 module + kernel; ~6 aggregates; ~20 REST endpoints; ~8 Angular pages.

## Scope

**In scope**: staff auth & sessions; users, roles, permissions; patient registration & directory; facilities; audit log; Angular login/directory/management screens; health check + structured logging; seeded bootstrap admin with forced password change.

**Out of scope** (deferred): Clinical, Laboratory, Pharmacy, Insurance modules; durable outbox/inbox + in-process integration bus; 3rd-party Insurance ACL; downstream reactions to `PatientAdmitted`; patient portal; scheduling/billing; notifications.

## Architecture Decisions

Deliberate simplifications vs. the earlier plan (and the standards docs). Each keeps the functional requirement while removing premature layers.

| Decision | Rationale |
|----------|-----------|
| **No Outbox/Inbox in Phase 1.** `PatientAdmitted` is a plain record published via a single MediatR `INotification` in-process. | FR12 targets *future* cross-module delivery. Phase 1 has one module and zero consumers, so a durable dispatcher, interceptor, and inbox are pure overhead. Add them in the phase that introduces a second module. |
| **No separate `IntegrationBus` / `Outbox` projects.** Contracts live in `Administration.IntegrationEvents`; the kernel is one project. | Two empty infrastructure projects for an unused bus add ceremony and reference complexity. |
| **Exceptions over `Result<T>`.** Domain throws; a global middleware maps to ProblemDetails. | `Result` wrapping adds ceremony with no benefit when ASP.NET Core already standardizes error translation. |
| **No Architecture Tests / NetArchTest.** Dependency direction is enforced by project references; no test project. | A whole test project to assert what `csproj` references already guarantee is low value. |
| **Audit written directly in handlers** (or a minimal action filter for PHI reads), not a MediatR pipeline behavior. | A dedicated pipeline behavior + interface for one cross-cutting concern is heavier than the call site. |
| **MRN = simple sequential/formatted generator** (e.g. `MRN-000001`), uniqueness guaranteed by a DB unique index. | Check-digit algorithms are over-engineering for Phase 1 uniqueness, which the index already guarantees. |
| **Typed IDs owned by their bounded context.** Only the generic `Id` base type lives in the shared kernel; concrete IDs (`UserId`, `RoleId`, `PatientId`, `FacilityId`) live in `Administration.Domain`, colocated with their aggregates. | Keeps the kernel module-agnostic so it isn't coupled to Administration concepts; each module owns its identifiers. EF Core value converters live in the module's Infrastructure (needs EF Core), never in the kernel. |

## Functional Requirements → Approach

| FR | Phase 1 approach |
|----|------------------|
| FR1 Auth (JWT + refresh rotation) | Access token (~15 min) + server-stored hashed refresh token, single-use, rotated on refresh. |
| FR2 Role+permission authZ (server + UI) | Permission-claim policies on endpoints; UI gates via role/permission signals. Default-deny. |
| FR3 Admin user lifecycle | Create/suspend/reactivate user commands; role assignment diff. |
| FR4 Roles & permissions | Role CRUD; permissions as claim strings; system roles protected. |
| FR5 Default roles | Seed Administrator / Receptionist / Clinician. |
| FR6 Patient registration + validation | FluentValidation on create command; reject with 422 on bad demographics. |
| FR7 Server-generated unique MRN | Generated server-side (never client-supplied) + unique DB index. |
| FR8 Patient directory search/filter/sort/paginate | Query with `AsNoTracking` projection + `?page&pageSize&q&sort&status`. |
| FR9 Facility management | Facility CRUD + deactivate. |
| FR10 Audit log | Append-only `AuditEntry` written on PHI create/view/modify; paginated admin query. |
| FR11 Identity/permissions as published contract | JWT claims (`sub/role/permission`) are the contract other modules consume. |
| FR12 Reliable idempotent events | Contract `PatientAdmitted` defined; published in-process now; durable delivery deferred (see Architecture Decisions). |
| FR13 Health + structured logging | `/health` (self + DB); Serilog; no PHI/secrets in logs. |
| FR14 HTTPS + safe errors | HTTPS redirect + HSTS; ProblemDetails only. |
| FR15 Web UI | Angular login, patient directory, user/role/facility/audit screens with loading/error/empty/validation states. |
| FR16 Seeded admin + forced change | Bootstrap admin from secret store, `MustChangePassword=true`. |

## Project Structure

```text
backend/
├── src/
│   ├── Bootstrapper/Healthcare.Api/         # composition root: DI, auth, middleware, migrations, /health, OpenAPI
│   ├── Modules/Administration/
│   │   ├── Domain/                          # aggregates (User, Role, Patient, Facility, AuditEntry) + their typed IDs (UserId, RoleId, PatientId, FacilityId)
│   │   ├── Application/                     # Commands, Queries, Validators, Handlers (MediatR)
│   │   ├── Infrastructure/                  # AdministrationDbContext, EF configs + ID value converters, password hasher, token service, authorization
│   │   ├── Api/                             # Minimal API endpoint modules (/api/v1/...)
│   │   └── IntegrationEvents/               # PatientAdmitted contract (plain record)
│   └── Shared/Healthcare.Shared.Kernel/     # generic Id base, Entity/AggregateRoot, value objects (Email, Phone, Address, DateOfBirth, Mrn)
└── tests/
    ├── Administration.UnitTests/
    └── Administration.IntegrationTests/

frontend/healthcare-web/src/app/
├── app.config.ts | app.routes.ts            # provideHttpClient(+interceptors), provideRouter; lazy routes
├── core/                                    # auth service, interceptors (auth, error), guards (auth, role)
├── shared/                                  # presentational components (button, table, card, pagination, form-field)
├── layouts/                                 # main-layout (header+sidebar), auth-layout (centered card)
├── interfaces/                              # DTO types
└── features/
    ├── auth/                                # login, change-password
    ├── patients/                            # directory + register + detail
    └── administration/                      # users, roles, facilities, audit
```

**Shape**: modular monolith under `backend/src/{Bootstrapper,Modules,Shared}` and the Angular SPA under `frontend/healthcare-web/src/app`. Dependency direction is enforced by project references: `Api → Application → Domain`; `Infrastructure → Domain/Application`; `Domain` depends only on the kernel. Later phases append their module folders.

## Standards Alignment

The governed standards (`docs/`) drive *how* this is built. Two documentation-drift items must be reconciled (repo is ahead of the constitution):

- **Angular 20 → 22** and **Karma → Vitest** in `.specify/memory/constitution.md`, `docs/frontend-architecture.md`, `docs/angular-guidelines.md`, `docs/testing.md`.
- **`styles.css → styles.scss`** in the frontend docs.

The deliberate simplifications in *Architecture Decisions* (no outbox, exceptions over `Result`, no architecture tests) deviate from the most ceremony-heavy reading of the standards in service of KISS/YAGNI; the functional requirements and the modular-monolith + DDD shape are preserved.

## Implementation Phases

Breakdown lives in [tasks.md](./tasks.md). High-level order:

1. **Setup** — solution/projects, NuGet, `Directory.Build.props` + central package management, `docker-compose` Postgres, appsettings, frontend shell + Tailwind; reconcile docs drift.
2. **Foundation** — kernel (IDs, base types, value objects), `AdministrationDbContext` + schema, global error/logging/health middleware, MediatR + FluentValidation, JWT + permission policies, audit writer, seed (roles + bootstrap admin), frontend core (services, interceptors, guards, layouts, shared components).
3. **US1 — Authentication** (P1/MVP): login, refresh, change-password, logout, suspended-user block. Checkpoint: bootstrap admin → forced change → protected call.
4. **US2 — Patients** (P2): register (server MRN), directory (search/filter/sort/paginate), view/update/deactivate, audit on view, publish `PatientAdmitted` in-process.
5. **US3 — Users & Roles** (P2): user lifecycle, role/permission management, claims flow into tokens.
6. **US4 — Facilities** (P3): facility CRUD + deactivate.
7. **US5 — Audit** (P3): paginated/filterable audit log.
8. **Polish** — `dotnet format` verify, vulnerability scan, frontend build + tests, HTTPS/HSTS/login-rate-limit hardening, README/quickstart link, end-to-end quickstart validation.

## Definition of Done (database)

- All EF Core migrations for the `administration` schema **generate without errors** (`dotnet ef migrations add`).
- Those migrations **apply successfully to PostgreSQL** (`dotnet ef database update`); the API applies any pending migrations on startup against the configured database (real Postgres — the EF InMemory provider never validates migrations and must not satisfy this gate).
- On a fresh database the app starts, `/health` reports the DB healthy, and the seed (roles + bootstrap admin) runs idempotently.

## Notes

- Commit per task or logical group (Conventional Commits).
- Verify the quickstart checkpoint before advancing a user story.
- When the Clinical module is introduced, revisit FR12: add the durable outbox/inbox behind a stable `IIntegrationEventPublisher` so `PatientAdmitted` gains reliable at-least-once delivery without changing existing call sites.
