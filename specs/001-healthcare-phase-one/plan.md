# Implementation Plan: Healthcare System — Phase 1 (Foundation & Administration)

**Branch**: `001-healthcare-phase-one` | **Date**: 2026-08-03 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-healthcare-phase-one/spec.md`

## Summary

Phase 1 stands up the modular-monolith foundation and the first business module, **Administration & Access Control**. The foundation delivers the `Shared` kernel (strongly-typed IDs, `Result`, in-process integration bus, Outbox/Inbox for reliable at-least-once idempotent delivery) and the solution/PostgreSQL skeleton. The Administration module implements identity & access (users, roles, permissions, JWT auth + refresh rotation, role/claim authorization), patient registration & directory, facility management, and PHI access auditing — following DDD + CQRS + Vertical Slice per the governed standards. A matching Angular SPA provides login, patient directory, and user/role management screens. The integration-event contracts (e.g. `PatientAdmitted`) and the bus plumbing ship now; downstream consumers arrive in later phases.

## Technical Context

**Language/Version**: C# 13 on **.NET 10** (backend); **TypeScript 6.0** on **Angular 22** (frontend).

> ⚠️ The constitution/standards cite **Angular 20** and Karma-based testing, but the repo (`frontend/healthcare-web/package.json`) is on **Angular 22**, TypeScript 6.0.2, and **Vitest**. See Constitution Check §16/15 and [research.md](./research.md) — recommendation is to amend the constitution to the installed versions (a superset), not downgrade.

**Primary Dependencies**:
- *Backend*: ASP.NET Core Minimal APIs, **MediatR** (CQRS), **FluentValidation**, **EF Core 10** + `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.AspNetCore.Authentication.JwtBearer`, **Serilog** (structured logging), `AspNetCore.HealthChecks`, `BCrypt.Net-Next` (password hashing), `OpenIddict` or hand-rolled JWT issuance (see research.md).
- *Frontend*: Angular 22 standalone components, **Signals**, Reactive Forms, Tailwind CSS 4, RxJS 7, Vitest.

**Storage**: **PostgreSQL** — one schema per module. Phase 1 creates the `administration` schema (identity, patients, facilities, audit) plus the cross-module `outbox`/`inbox` tables (owned by the `Shared` kernel).

**Testing**: **xUnit** + FluentAssertions (+ Coverlet for CI coverage); EF with a per-test database (Testcontainers PostgreSQL for integration, EF InMemory/in-memory provider only for pure unit). Frontend: **Vitest** (`@angular/build:unit-test`), `*.spec.ts` colocated.

**Target Platform**: Linux container (Docker) for the API; evergreen browsers for the SPA; PostgreSQL 16+ container.

**Project Type**: web-service (modular monolith API) + web-app (Angular SPA).

**Performance Goals**: patient-directory query p95 < 300 ms for paginated reads; login p95 < 500 ms; tolerate hundreds of concurrent staff; integration-event dispatch without request-thread blocking (Outbox dispatcher).

**Constraints**: default-deny authorization; PHI never logged; no synchronous DB calls; every async method forwards `CancellationToken`; HTTPS only; safe `ProblemDetails` errors; idempotent event consumers.

**Scale/Scope**: 1 module (Administration) + `Shared` kernel; ~6 aggregates; ~20 REST endpoints; ~8 routed Angular pages; Outbox/Inbox reliability for future cross-module events.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| 1 | Architecture First | ✅ PASS | Modular monolith + DDD + Clean + Vertical Slice + CQRS by design. |
| 2 | Module Isolation | ✅ PASS | Modules communicate via commands/queries/events only; no cross-schema access. Outbox/Inbox isolates integration. |
| 3 | DDD | ✅ PASS | Business logic in Domain; aggregates enforce invariants; value objects; domain events. |
| 4 | CQRS | ✅ PASS | Each mutation = Command + Validator + Handler + Response + Endpoint. Queries read-only. |
| 5 | Vertical Slice | ✅ PASS | `Patient/CreatePatient/`, `Patient/UpdatePatient/`, … per slice. |
| 6 | Dependency Rules | ✅ PASS | Api → Application → Domain; Infrastructure points inward; Domain persistence-ignorant. |
| 7 | Backend Standards | ✅ PASS | .NET 10, Minimal APIs, MediatR, FluentValidation, EF Core, PostgreSQL, DI. Adds required NuGet packages (none installed yet — expected for greenfield). |
| 8 | API Design | ✅ PASS | REST, ProblemDetails, pagination/filtering/sorting/search, `/api/v1/...` versioning, OpenAPI. |
| 9 | Validation | ✅ PASS | FluentValidation per command; IDs/dates/length/required/business rules. |
| 10 | Error Handling | ✅ PASS | ProblemDetails; no stack traces; safe messages; server-side logging. |
| 11 | Logging | ✅ PASS | Serilog structured logging; never passwords/secrets/tokens/PHI. |
| 12 | Security | ✅ PASS | AuthN (JWT) + AuthZ (role + claim policies), HTTPS, input validation, output encoding, parameterized queries, OWASP. |
| 13 | Performance | ✅ PASS | Pagination, projections, `AsNoTracking`, avoid N+1, efficient LINQ. |
| 14 | Database | ✅ PASS | EF Core migrations; one schema per module; indexes on FKs + unique MRN; non-destructive by default. |
| 15 | Testing | ⚠️ NOTE | Repo uses **Vitest**, not Karma. Doc says `npm test -- --watch=false`. **Action:** amend `docs/testing.md` §6 to Vitest (`@angular/build:unit-test`) — do not downgrade. |
| 16 | Angular | ⚠️ NOTE | Repo is **Angular 22**; constitution says 20. **Action:** amend constitution/docs `20 → 22` (superset; signals/standalone unchanged). |
| 17 | UI | ✅ PASS | Responsive, accessible, loading/error/empty/validation states, Tailwind. |
| 18 | Naming | ✅ PASS | PascalCase C#, `_camelCase` fields, `I` interfaces, kebab-case routes/files, past-tense events, imperative commands, strongly-typed IDs. |
| 19 | Code Quality | ✅ PASS | SOLID/DRY/KISS/YAGNI/Boy Scout. |
| 20–23 | Docs / AI / Git / Output | ✅ PASS | Public APIs documented (OpenAPI); conventional commits; compiles; no placeholders. |

**Gate result: PASS.** Two items (15, 16) are **documentation drift** (repo ahead of constitution), not violations — resolved by constitution amendment (see research.md). No unjustified violations; no Complexity Tracking entries required.

**Post-design re-check (after Phase 1 artifacts):** ✅ PASS — the [data model](./data-model.md), [contracts](./contracts/README.md), and [quickstart](./quickstart.md) introduce no new violations. DDD invariants, CQRS slices, module isolation (Outbox/Inbox, no cross-schema access), REST/ProblemDetails/pagination/versioning, default-deny auth + audit, one-schema-per-module with migrations + indexes, and reliable idempotent events all conform. Only outstanding items remain the two documentation amendments (Angular `20→22`, Karma→Vitest).

## Project Structure

### Documentation (this feature)

```text
specs/001-healthcare-phase-one/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── Bootstrapper/
│   │   └── Healthcare.Api/              # composition root: DI wiring, auth, bus, migrations host, health
│   │       ├── Program.cs
│   │       └── appsettings.json
│   ├── Modules/
│   │   └── Administration/
│   │       ├── Domain/                  # User, Role, Permission, Patient, Facility, AuditEntry + Events
│   │       ├── Application/             # Commands, Validators, Handlers, Policies, Projections (read models)
│   │       ├── Infrastructure/          # AdministrationDbContext, repositories, EF mappings, password hasher
│   │       ├── Api/                     # Minimal API endpoint modules (/api/v1/...)
│   │       └── IntegrationEvents/       # PatientAdmitted (published contract)
│   └── Shared/
│       ├── Kernel/                      # Base types, strongly-typed IDs (UserId, PatientId...), Result
│       ├── IntegrationBus/              # in-process pub/sub abstractions (IIntegrationEvent, IIntegrationEventHandler)
│       └── Outbox/                      # Outbox/Inbox tables, dispatcher, idempotency
└── tests/
    ├── Administration.UnitTests/
    ├── Administration.IntegrationTests/
    └── Shared.UnitTests/

frontend/healthcare-web/src/
├── main.ts
├── styles.scss                          # global styles + Tailwind layers/design tokens
└── app/
    ├── app.component.html|.scss|.ts     # root shell (<router-outlet>)
    ├── app.routes.ts                    # top-level routes → lazy layouts/features
    ├── app.config.ts                    # provideHttpClient, provideRouter, interceptors
    ├── core/                            # interceptors (auth, error), guards (auth, role), services (auth)
    ├── shared/                          # presentational components (buttons, tables, cards), pipes
    ├── layouts/                         # main-layout (header+sidebar), auth-layout (centered card)
    ├── interfaces/                      # DTO types: user, role, patient, facility, audit, common/
    └── features/
        ├── auth/                        # login page (+ change-password)
        ├── administration/              # users, roles, facilities, audit pages
        └── patients/                    # patient directory + register patient
```

**Structure Decision**: Web application (Option 2 variant) — a modular-monolith backend under `backend/src/{Bootstrapper,Modules,Shared}` (per [docs/event-storming/05-module-mapping.md](../../docs/event-storming/05-module-mapping.md) and [docs/architecture.md](../../docs/architecture.md) §4) and the Angular SPA under `frontend/healthcare-web/src/app` with the layered modular structure from [docs/frontend-architecture.md](../../docs/frontend-architecture.md) §3. Phase 1 delivers the `Administration` module and `Shared` kernel only; later phases append `Clinical`, `Laboratory`, `Pharmacy`, `Insurance` modules and their SPA feature folders.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No constitution violations require justification. (Two documentation-drift notes — Angular 22, Vitest — are resolved by amendment, not deviation.)
