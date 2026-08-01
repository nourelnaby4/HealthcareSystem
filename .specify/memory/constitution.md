<!--
Sync Impact Report
==================
Version change: 1.0.0 (unversioned baseline) → 1.1.0 → 1.2.0 → 1.3.0
Bump type:
  - 1.0.0 → 1.1.0 (MINOR): added the Standards Reference (Controlled Documents) catalog and the Governance section.
  - 1.1.0 → 1.2.0 (MINOR): materially expanded docs/angular-guidelines.md — mandated Tailwind + HTML templates + SCSS, and the full modular architecture (core, interceptors, shared, layouts, interfaces, features → pages/components/services/routes).
  - 1.2.0 → 1.3.0 (MINOR): split the frontend architecture into its own controlled document docs/frontend-architecture.md (styling stack + modular structure), slimmed docs/angular-guidelines.md to coding rules only, and cross-linked both.

Modified principles: none renamed. Added cross-references from each principle to its detailed controlled document.

Modified controlled documents:
  - docs/angular-guidelines.md → architecture & styling content moved to docs/frontend-architecture.md; now rules-only with cross-reference.
  - docs/frontend-architecture.md → new (owns styling stack + modular structure + layer responsibilities).

Added sections:
  - Standards Reference (Controlled Documents)
  - Governance (amendment procedure, versioning policy, compliance review)

Removed sections: none.

New controlled documents (docs/):
  - docs/architecture.md          → governs Principles 1, 2, 6
  - docs/coding-standards.md      → governs Principles 10, 11, 19, 23
  - docs/backend-guidelines.md    → governs Principles 3, 4, 5, 7, 13, 14
  - docs/angular-guidelines.md    → governs Principles 16, 17 (rules)
  - docs/frontend-architecture.md → governs Principles 16, 17 (structure & styling)
  - docs/api-design.md            → governs Principle 8
  - docs/security.md              → governs Principles 9, 10, 11, 12
  - docs/testing.md               → governs Principle 15
  - docs/naming.md                → governs Principle 18

Templates requiring updates:
  - .specify/templates/constitution-template.md   ✅ unchanged (generic, still valid)
  - .specify/templates/spec-template.md            ⚠ optional: may add a "Standards Compliance" note (deferred, low priority)
  - .specify/templates/plan-template.md            ✅ unchanged
  - .specify/templates/tasks-template.md           ✅ unchanged

Follow-up TODOs: none.
-->

# Healthcare System Constitution

## Standards Reference (Controlled Documents)

The detailed, enforceable rules live in `docs/`. These documents are **controlled** by this Constitution: every principle below links to its authoritative document, and the Constitution is the supreme authority when any conflict arises.

| Document | Path | Governs Principles |
|----------|------|--------------------|
| Architecture | [docs/architecture.md](../../docs/architecture.md) | 1, 2, 6 |
| Backend Guidelines | [docs/backend-guidelines.md](../../docs/backend-guidelines.md) | 3, 4, 5, 7, 13, 14 |
| API Design | [docs/api-design.md](../../docs/api-design.md) | 8 |
| Security | [docs/security.md](../../docs/security.md) | 9, 10, 11, 12 |
| Coding Standards | [docs/coding-standards.md](../../docs/coding-standards.md) | 10, 11, 19, 23 |
| Testing | [docs/testing.md](../../docs/testing.md) | 15 |
| Angular Guidelines | [docs/angular-guidelines.md](../../docs/angular-guidelines.md) | 16, 17 |
| Frontend Architecture | [docs/frontend-architecture.md](../../docs/frontend-architecture.md) | 16, 17 |
| Naming | [docs/naming.md](../../docs/naming.md) | 18 |

**Rule:** Any change to a controlled document that alters a governing rule requires a Constitution amendment (see Governance). New standards become controlled only by being listed here.

---

## Project Vision

This project is a production-grade Healthcare Management System built to demonstrate enterprise software architecture and engineering best practices.

Every generated artifact must prioritize maintainability, scalability, security, performance, and readability over implementation speed.

The AI must behave as an experienced Software Architect and Senior Full Stack Engineer.

---

# Principle 1 — Architecture First

Architecture is never compromised.

> Authoritative detail: [docs/architecture.md](../../docs/architecture.md)

The project follows:

- Modular Monolith Architecture
- Domain Driven Design (DDD)
- Clean Architecture
- Vertical Slice Architecture
- CQRS
- SOLID Principles
- Clean Code

No generated code may violate these architectural boundaries.

---

# Principle 2 — Module Isolation

Every module is autonomous.

Modules communicate only through:

- Commands
- Queries
- Domain Events

Never access another module's database objects directly.

Never reference internal implementation of another module.

---

# Principle 3 — Domain Driven Design

Business logic belongs inside the Domain layer.

Never place business logic inside:

- Controllers
- API Endpoints
- Repositories
- DbContext

Entities enforce invariants.

Prefer Value Objects over primitive obsession.

Use Domain Events whenever side effects are required.

Rich Domain Model is preferred.

Avoid Anemic Domain Models.

---

# Principle 4 — CQRS

Every feature follows CQRS.

Commands modify state.

Queries never modify state.

Each feature contains:

- Command
- Validator
- Handler
- Response
- Endpoint

Queries are read-only.

Handlers should contain one responsibility.

---

# Principle 5 — Vertical Slice

Organize by Feature.

Never organize by technical layer.

Example:

Patient/

    CreatePatient/

    UpdatePatient/

    DeletePatient/

    GetPatient/

Each slice contains everything required for that feature.

---

# Principle 6 — Dependency Rules

Dependencies point inward.

API

↓

Application

↓

Domain

Infrastructure depends on Domain and Application.

Domain never depends on Infrastructure.

---

# Principle 7 — Backend Standards

> Authoritative detail: [docs/backend-guidelines.md](../../docs/backend-guidelines.md)

Use:

- .NET 10
- ASP.NET Core
- Minimal APIs
- MediatR
- FluentValidation
- EF Core
- PostgreSQL
- Dependency Injection

Every async method accepts CancellationToken.

Avoid synchronous database calls.

Never block threads.

---

# Principle 8 — API Design

> Authoritative detail: [docs/api-design.md](../../docs/api-design.md)

RESTful APIs.

Correct HTTP verbs.

Return proper status codes.

Use ProblemDetails.

Use pagination.

Support filtering.

Support sorting.

Support searching.

API versioning when needed.

---

# Principle 9 — Validation

> Authoritative detail: [docs/security.md](../../docs/security.md) (input) · [docs/backend-guidelines.md](../../docs/backend-guidelines.md) (FluentValidation)

Every command has FluentValidation.

Validation occurs before business logic.

Never trust client input.

Validate:

- IDs
- Dates
- Length
- Required fields
- Business rules

---

# Principle 10 — Error Handling

Never expose stack traces.

Return ProblemDetails.

Log internal exceptions.

User receives safe messages.

---

# Principle 11 — Logging

Use structured logging.

Never log passwords.

Never log secrets.

Never log access tokens.

Every important action should be logged.

---

# Principle 12 — Security

> Authoritative detail: [docs/security.md](../../docs/security.md)

Security is mandatory.

Use:

Authentication

Authorization

Role Policies

Claims Policies

HTTPS

Input Validation

Output Encoding

Parameterized Queries

Never generate vulnerable code.

Follow OWASP Top 10.

---

# Principle 13 — Performance

Avoid N+1 queries.

Use projections.

Use pagination.

Use AsNoTracking for read operations.

Avoid unnecessary allocations.

Prefer efficient LINQ.

Measure before optimizing.

---

# Principle 14 — Database

Database changes use EF Core Migrations.

Never generate destructive migrations without warning.

Use indexes appropriately.

Prefer GUIDs only when appropriate.

Relationships must enforce integrity.

---

# Principle 15 — Testing

> Authoritative detail: [docs/testing.md](../../docs/testing.md)

Every feature requires tests.

Generate:

Unit Tests

Integration Tests

Meaningful assertions.

No placeholder tests.

---

# Principle 16 — Angular

> Authoritative detail: [docs/frontend-architecture.md](../../docs/frontend-architecture.md) (structure & styling) · [docs/angular-guidelines.md](../../docs/angular-guidelines.md) (rules)

Frontend uses:

Angular 20

Standalone Components

Signals

Lazy Loading

Feature-based structure

Strict TypeScript

Reactive Forms

Use RxJS only when Signals are insufficient.

Avoid any.

Avoid duplicated logic.

---

# Principle 17 — UI

Responsive.

Accessible.

Consistent.

Loading states.

Error states.

Empty states.

Form validation.

Proper user feedback.

---

# Principle 18 — Naming

> Authoritative detail: [docs/naming.md](../../docs/naming.md)

Use meaningful names.

Avoid abbreviations.

Classes:

PascalCase

Methods:

PascalCase

Variables:

camelCase

Private fields:

_camelCase

Interfaces:

IExample

---

# Principle 19 — Code Quality

Always follow:

SOLID

DRY

KISS

YAGNI

Boy Scout Rule

Readable code over clever code.

---

# Principle 20 — Documentation

Public APIs must be documented.

Complex business rules require comments.

Architecture decisions should be explained.

Avoid redundant comments.

---

# Principle 21 — AI Behaviour

Before generating code:

Understand requirements.

Ask clarifying questions when requirements are ambiguous.

Never invent business rules.

Never fabricate APIs.

Never assume database schema.

Generate production-ready code only.

Avoid placeholders.

Avoid TODOs.

Avoid pseudo code.

Generate complete implementations whenever possible.

---

# Principle 22 — Git

Generate meaningful commit messages.

Follow Conventional Commits.

Examples:

feat(patient): add create patient endpoint

fix(auth): validate refresh token expiration

refactor(appointment): simplify scheduling logic

test(patient): add integration tests

docs(api): update swagger documentation

---

# Principle 23 — Output Quality

Generated code must compile.

Follow project conventions.

Prefer maintainability over brevity.

Prefer explicitness over magic.

No duplicated code.

No dead code.

No unused dependencies.

No unnecessary abstractions.

---

# Final Rule

If any request conflicts with this Constitution,
the Constitution always takes precedence.

The AI should act as an experienced Enterprise Software Architect focused on long-term maintainability rather than short-term implementation speed.

---

# Governance

This Constitution is the supreme authority for all generated work in the project. Where any request, instruction, or convenience conflicts with it, **the Constitution always takes precedence**.

## Amendment Procedure

1. Propose the change as a documented issue/ADR (under `docs/adr/` when architectural).
2. Update the affected controlled document(s) in `docs/`.
3. Update this Constitution: edit the principle and/or the Standards Reference table.
4. Bump the version per the policy below and refresh the Sync Impact Report (HTML comment at the top of this file).
5. Update dependent templates if a principle's structure changed.

## Versioning Policy (Semantic Versioning)

- **MAJOR**: backward-incompatible governance — a principle removed or redefined.
- **MINOR**: a new principle/section added, or materially expanded guidance (e.g., a new controlled document).
- **PATCH**: clarifications, wording, typo fixes, non-semantic refinements.

## Compliance Review

- Every spec, plan, and task list produced by Spec Kit MUST cite and conform to the controlled documents above.
- Code review verifies compliance with the Constitution and its controlled documents.
- Complexity or deviation must be justified; undocumented deviation is non-compliant.

---

**Version**: 1.3.0 | **Ratified**: 2026-08-01 | **Last Amended**: 2026-08-01