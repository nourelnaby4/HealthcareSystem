# Plan: Speckit Specify — Healthcare System Phase 1 (Foundation & Administration)

> **Why this is a plan, not applied files:** The current session is in **Plan Mode** and file writes are restricted to plan Markdown files only. The `/speckit.specify` command normally creates `specs/<dir>/spec.md`, a checklist, and `.specify/feature.json`. Those writes are not permitted here, so this plan captures the **complete, verbatim speckit output** plus the exact steps for an implementation-capable agent to apply. Once applied, the spec is ready for `/speckit.clarify` or `/speckit.plan`.

---

## 1. Context gathered (Mandatory Context satisfied)

- Read all of `docs/` (architecture, backend/frontend guidelines, security, api-design, testing, naming, coding-standards) and the full `docs/event-storming/` set (domain storm, integration & ACL, context mapping, module mapping).
- **Codebase state:** greenfield. Backend = default .NET 10 Minimal API template (`WeatherForecast`); Frontend = default Angular 20 scaffold. `HealthcareSystem.slnx` exists. No modules, no Shared kernel, no real features.
- **Governance gap:** `.specify/memory/constitution.md` is cited by **every** controlled standard as the enforcing authority, but **does not exist** in the repo. Also absent: `.specify/extensions.yml`, `.specify/init-options.json`, `specs/`, `AGENTS.md`.

## 2. Decisions resolved (interactive)

| Decision | Choice |
|----------|--------|
| Phase-1 scope | **Foundation + Administration** (Shared kernel + modular-monolith skeleton + PostgreSQL; Administration module: users, roles/permissions, patients, facilities, audit; JWT auth + role-based access) |
| Frontend | **Full-stack** — Angular UI included (login, patient directory, user/role management) |
| Roles in scope | **Administrator, Receptionist, Clinician** |

No `[NEEDS CLARIFICATION]` markers remain. Remaining low-priority Open Questions have documented defaults.

## 3. speckit mechanics to apply

- **Pre/Post hooks:** `.specify/extensions.yml` absent → skip all hooks silently (both `before_specify` and `after_specify`).
- **Directory resolution:** no `.specify/init-options.json` → **sequential** numbering; `specs/` is empty → **`001`**.
- **Short name:** `healthcare-phase-one` → directory **`specs/001-healthcare-phase-one`**.
- **Spec template:** no `.specify` template/preset exists and the `specify` CLI is not guaranteed available; author `spec.md` from the standard Spec Kit structure (the content in §5 below) rather than copying a template file.

## 4. Files to create (3)

| Path | Action |
|------|--------|
| `specs/001-healthcare-phase-one/spec.md` | Create with content in §5 (verbatim) |
| `specs/001-healthcare-phase-one/checklists/requirements.md` | Create with content in §6 (verbatim) |
| `.specify/feature.json` | Create with content in §7 (verbatim). Creates the `.specify/` directory. |

## 5. `specs/001-healthcare-phase-one/spec.md` (write verbatim)

```markdown
# Healthcare System — Phase 1 (Foundation & Administration)

## Overview

Phase 1 establishes the load-bearing **foundation** of the healthcare platform and its **first business module: Administration & Access Control**. It delivers a single deployable unit with a reliable module-integration backbone, secure authentication, role-based authorization, and the ability to register and manage the people and places that every later module depends on — staff users, their roles, patients, and facilities — with audited access to sensitive health data.

With Administration in place, subsequent phases (Clinical, Laboratory, Pharmacy, Insurance & Billing) will consume **trusted identity, permissions, and patient data** through a stable published contract instead of rebuilding it. Phase 1 also ships a usable **web interface** for staff, not just APIs.

## Goals

- Establish a single modular-monolith foundation that future phases extend without rework.
- Provide reliable, at-least-once, idempotent cross-module communication so future integration events (e.g., a patient being admitted) are safe by construction.
- Let staff securely sign in and be authorized **by role and permission** to perform only their duties.
- Let authorized staff register and manage patients, facilities, and staff user accounts.
- Let administrators define roles and the permissions each role grants.
- Record who accessed sensitive health information and when, for accountability.
- Give staff a usable web experience (login, patient directory, user & role management).
- Enforce least-privilege: a user sees only what their role permits, consistently on both the server and the interface.

## Non-Goals (out of scope for phase 1)

- Clinical encounters, vitals, notes, and diagnoses (a later phase).
- The Laboratory, Pharmacy, and Insurance & Billing modules.
- The 3rd-party Insurance Provider Gateway and its anti-corruption layer.
- Downstream business reactions to integration events — the contracts and the bus that carry them are built, but no later-module subscribers ship yet.
- Patient admission triggering a clinical encounter (Clinical is not built yet).
- Patient portal / patient self-service.
- Appointments/scheduling, billing, payments, and claims.
- Mobile applications.
- Email/SMS notifications beyond in-app audit records.

## Realistic, Stress and Failure Scenarios

### Realistic

- A receptionist registers a new patient during intake; the patient appears in the directory immediately and an audit entry is recorded.
- An administrator creates a staff account and assigns the Receptionist role; the new user logs in and can register patients but cannot manage users.
- A clinician signs in, views the patient directory read-only (in readiness for clinical work), but cannot create users or change roles.

### Stress

- Many staff perform concurrent patient registrations and logins; no duplicate patient records are created (uniqueness is enforced on the patient identifier) and authentication does not fail under load.
- A large patient directory (tens of thousands of records) is searchable, with paginated results returned responsively.

### Failure

- A user enters wrong credentials several times; access is throttled or rejected without confirming whether the username or the password was wrong (no user enumeration).
- An unauthorized user calls a protected administration endpoint; the request is denied with a safe error and the attempt is audited.
- A request to register a duplicate patient (same identifier) is rejected with a clear, safe validation message; no partial record is persisted.

## User Scenarios & Testing

### Scenario: Receptionist registers a new patient

**Given** a signed-in user with the Receptionist role
**When** they submit valid patient demographics
**Then** the patient is created, appears in the directory, and an audit entry records the actor, action, and time.

### Scenario: Duplicate patient is rejected

**Given** a patient already exists with identifier "MRN-001"
**When** the receptionist submits another patient with the same identifier
**Then** the system rejects the request with a safe validation error and creates no record.

### Scenario: Role-based access is enforced

**Given** a Receptionist is signed in
**When** they attempt to reach the user-management screen or endpoint
**Then** access is denied on both the interface (hidden/disabled) and the server, and the attempt is audited.

### Scenario: First administrator bootstraps the system

**Given** a freshly installed system with a seeded initial administrator
**When** that administrator signs in with the bootstrap credentials
**Then** they are required to change their password and can then create other users and roles.

### Scenario: Suspended user is blocked

**Given** an administrator suspends a staff user
**When** that user attempts to sign in or use an existing session
**Then** access is revoked and further sign-in is refused.

## Functional Requirements

- **FR1.** The system authenticates staff with signed, short-lived access tokens and a secure refresh-token flow (with rotation and expiration validation).
- **FR2.** The system authorizes every protected action by role and permission on the server; the interface reflects the same permissions.
- **FR3.** The system lets an Administrator create, suspend, and reactivate staff user accounts and assign roles to them.
- **FR4.** The system lets an Administrator define roles and the permissions each role grants.
- **FR5.** The system provides the roles **Administrator**, **Receptionist**, and **Clinician** by default.
- **FR6.** The system lets a Receptionist or Administrator register a new patient with required demographic fields and validates them (required, format, and length constraints).
- **FR7.** The system prevents duplicate patients on a unique patient identifier and returns a safe validation error.
- **FR8.** The system lets authorized staff search, filter, sort, and paginate the patient directory.
- **FR9.** The system lets an Administrator register and manage facilities (wards, departments, or locations).
- **FR10.** The system records an audit entry (actor, action, resource, timestamp) whenever sensitive health data is created, viewed, or modified, and lets an Administrator review the audit log (paginated and filterable).
- **FR11.** The system exposes identity and permissions as a stable, published contract so future modules authorize using claims rather than direct data access.
- **FR12.** The system delivers cross-module events reliably (at-least-once) and processes them idempotently, so future subscribers observe each event exactly once.
- **FR13.** The system provides a health/status check and structured logging that never contains sensitive health data or secrets.
- **FR14.** The system enforces HTTPS and returns only safe error responses (no internals or stack traces).
- **FR15.** The web interface provides login, patient directory, and user/role management screens that handle loading, error, empty, and validation states and that enforce access control.
- **FR16.** The system supports a seeded initial administrator for first-time setup, with a forced password change on first login.

## Non-Functional Requirements

- **Security:** all sensitive health data is handled with least privilege and audited access; no such data or secrets appear in logs; authorization is default-deny; OWASP-aligned.
- **Privacy:** consistent failure timing to prevent user enumeration; safe, minimal error messages that reveal nothing internal.
- **Reliability:** at-least-once, idempotent event delivery; patient and identity records are not lost or duplicated on transient failure.
- **Usability:** staff complete core tasks (sign in, register a patient, assign a role) in a small number of steps with clear, immediate validation.
- **Performance:** patient-directory searches return paginated results responsively under typical clinic load.
- **Maintainability:** changes conform to the project's governed architecture, keeping modules isolated with no cross-module data access.
- **Accessibility & Responsiveness:** the web interface is keyboard-navigable and responsive across screen sizes.
- **Observability:** a request identifier correlates errors; logs are structured and free of sensitive data.

## Key Entities

- **Staff User** — identity, credentials, status (active/suspended), assigned roles.
- **Role** — a named set of permissions.
- **Permission** — an authorization capability granted by a role.
- **Patient** — demographics plus a unique patient identifier.
- **Facility** — a care location, ward, or department.
- **Audit Entry** — who, what, and when, for access to sensitive health data.
- **Access/Refresh Token** — short-lived proof of authentication.
- **Integration Event Contract** — published identity/permission claims (and a patient-admitted contract, defined for future use but not yet consumed).

## Success Criteria

- Staff can sign in and perform only their permitted duties, and are blocked from anything outside their role — verifiable across all three roles.
- A receptionist can register a patient end-to-end (interface and server) in under 2 minutes, including validation, and the patient appears in a paginated, searchable directory.
- Duplicate patient registration on the patient identifier is always rejected; no duplicate records exist after concurrent registration attempts.
- Every create, view, or modification of sensitive health data produces an auditable record that an Administrator can retrieve.
- The foundation reliably delivers a published integration event (at-least-once, idempotent), demonstrable via an automated test even though no downstream consumer ships yet.
- An unauthorized request to any administration action is denied and audited.
- The system is reachable only over HTTPS and returns only safe, sensitive-data-free error responses.

## Assumptions

- Implementation conforms to the project's controlled standards under `docs/` (architecture, backend/frontend guidelines, security, API design, testing, naming), which fix the technology choices; this specification intentionally remains technology-agnostic.
- A relational database is provisioned with **one schema per module**, and schema changes are applied via migrations — never by hand.
- Authentication uses signed tokens issued by Administration, with a short-lived access token and rotated refresh token, per the security standard.
- Roles in scope are **Administrator, Receptionist, Clinician**; the Clinician role is reserved with read access in readiness for the upcoming Clinical phase.
- Patient uniqueness is enforced on an MRN-style identifier; other demographics follow standard clinical-intake defaults.
- An initial administrator account is seeded at install from the secret store and forces a password change on first login.
- "Sensitive health data" includes all patient, identity, and audit data, treated per the security standard.
- The web interface is built with the documented single-page framework, using lazy-loaded features, an auth interceptor, and route guards.

## Dependencies

- The governed project standards under `docs/` define **how** this is built; this specification defines **what** and **why**.
- Future phases (Clinical, Laboratory, Pharmacy, Insurance & Billing) depend on phase 1's identity, patient, facility, and integration foundation.
- `.specify/memory/constitution.md` is referenced by every standard as the enforcing authority but **does not yet exist** in the repository — see Open Questions.

## Open Questions

1. Should the governing `constitution.md` (cited by every controlled standard) be authored as part of phase 1, or is it intentionally pending? **Recommendation:** author a minimal constitution before phase 1 so the controlled standards are genuinely enforced.
2. What is the official patient identifier for uniqueness — MRN, a national/insurance ID, or both? **Default assumption:** MRN; confirm if a national/insurance identifier is required in phase 1.
3. Are any regulatory/compliance regimes (e.g., HIPAA-style safeguards) in scope for phase 1 that require controls beyond the security standard? **Default:** follow the documented security standard; flag if stricter compliance is required.
```

## 6. `specs/001-healthcare-phase-one/checklists/requirements.md` (write verbatim)

```markdown
# Specification Quality Checklist: Healthcare System — Phase 1 (Foundation & Administration)

**Purpose**: Validate specification completeness and quality before proceeding to planning
**Created**: 2026-08-03
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] No implementation details (languages, frameworks, APIs) — requirements stay technology-agnostic; the stack is only referenced as externally governed standards/assumptions.
- [x] Focused on user value and business needs
- [x] Written for non-technical stakeholders
- [x] All mandatory sections completed

## Requirement Completeness

- [x] No [NEEDS CLARIFICATION] markers remain (critical decisions resolved with the user: scope, frontend inclusion, roles)
- [x] Requirements are testable and unambiguous
- [x] Success criteria are measurable
- [x] Success criteria are technology-agnostic (no implementation details)
- [x] All acceptance scenarios are defined
- [x] Edge cases are identified (duplicate patient, unauthorized access, credential throttling, suspended user, first-login password change)
- [x] Scope is clearly bounded (explicit Non-Goals)
- [x] Dependencies and assumptions identified

## Feature Readiness

- [x] All functional requirements have clear acceptance criteria
- [x] User scenarios cover primary flows
- [x] Feature meets measurable outcomes defined in Success Criteria
- [x] No implementation details leak into specification

## Notes

- Critical scope decisions were resolved interactively before authoring:
  - Scope = Foundation + Administration module (identity, patients, facilities, audit, JWT auth).
  - Full-stack: Angular web interface included (login, patient directory, user/role management).
  - Roles defined: Administrator, Receptionist, Clinician.
- Three lower-priority Open Questions remain (constitution authoring, official patient identifier, compliance scope) — each has a documented default assumption, so none block planning.
- Governance gap flagged: `.specify/memory/constitution.md` is cited by every controlled standard but does not yet exist in the repo.
- Items marked incomplete require spec updates before `/speckit.clarify` or `/speckit.plan`. All items currently pass.
```

## 7. `.specify/feature.json` (write verbatim)

```json
{
  "feature_directory": "specs/001-healthcare-phase-one"
}
```

## 8. Validation result (self-review against checklist) — all PASS

- Content Quality: requirements technology-agnostic; business-focused; all mandatory sections present.
- Completeness: zero `[NEEDS CLARIFICATION]` markers; FRs testable; success criteria measurable and tech-agnostic; edge cases covered; scope bounded by Non-Goals.
- Readiness: each FR maps to an acceptance scenario; primary flows covered.
No spec edits required after validation.

## 9. After application (Completion Report)

Once the three files exist, the speckit result is:
- `SPECIFY_FEATURE_DIRECTORY` = `specs/001-healthcare-phase-one`
- `SPEC_FILE` = `specs/001-healthcare-phase-one/spec.md`
- Checklist: all items PASS.
- Next phase: `/speckit.clarify` (resolve Open Questions) or `/speckit.plan` (technical plan). Recommend `/speckit.plan` since critical decisions are already settled and the three Open Questions have defaults.

## 10. Open items for the implementer / follow-ups

1. **Governance gap (recommend before phase-1 build):** author a minimal `.specify/memory/constitution.md` so the controlled standards it cites are actually enforced. All `docs/*` standards link to it.
2. Confirm the official patient identifier (MRN vs national/insurance ID) and any compliance regime before `/speckit.plan` if the defaults are wrong.
3. The branch-creation step of `/speckit.specify` is skipped (no git extension hook); if a feature branch is desired, create it manually (e.g., `feat/001-healthcare-phase-one`).
