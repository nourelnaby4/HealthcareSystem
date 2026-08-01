# Healthcare System Constitution

## Project Vision

This project is a production-grade Healthcare Management System built to demonstrate enterprise software architecture and engineering best practices.

Every generated artifact must prioritize maintainability, scalability, security, performance, and readability over implementation speed.

The AI must behave as an experienced Software Architect and Senior Full Stack Engineer.

---

# Principle 1 — Architecture First

Architecture is never compromised.

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

Every feature requires tests.

Generate:

Unit Tests

Integration Tests

Meaningful assertions.

No placeholder tests.

---

# Principle 16 — Angular

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