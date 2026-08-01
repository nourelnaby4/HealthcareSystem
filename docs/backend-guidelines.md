# Backend Guidelines

**Status**: Controlled standard — enforced by [Constitution Principles 3 (DDD), 4 (CQRS), 5 (Vertical Slice), 6 (Dependency Rules), 7 (Backend Standards), 13 (Performance), 14 (Database)](../.specify/memory/constitution.md)
**Applies to**: all .NET backend code.

---

## 1. Technology Stack

| Concern | Choice |
|---------|--------|
| Runtime | **.NET 10** |
| Web framework | **ASP.NET Core**, **Minimal APIs** |
| Mediation / CQRS | **MediatR** |
| Validation | **FluentValidation** |
| Data access | **EF Core** |
| Database | **PostgreSQL** — one schema per module |
| Composition | **Dependency Injection** (built-in) |
| Formatting | `dotnet format` (enforced in CI, `--verify-no-changes`) |

---

## 2. Layering & DDD

See [architecture.md](./architecture.md). Business logic **belongs in the Domain layer**. Never place business logic in:
- Controllers / API endpoints
- Repositories
- `DbContext`

- Entities and aggregates **enforce invariants**.
- Prefer **Value Objects** over primitive obsession.
- Emit **Domain Events** for side effects.
- Prefer a **rich domain model**; avoid anemic models.

---

## 3. CQRS

Every mutating feature follows CQRS and contains all five parts:

1. **Command**
2. **Validator** (FluentValidation)
3. **Handler**
4. **Response** (DTO)
5. **Endpoint** (Minimal API)

- Commands modify state. Queries are **read-only**.
- One responsibility per handler.
- Validators run before business logic — never trust client input.

---

## 4. Vertical Slice Organization

Organize **by feature**, never by technical layer:

```
Patient/
├─ CreatePatient/
├─ UpdatePatient/
├─ DeletePatient/
└─ GetPatient/
```

Each slice is self-contained and owns everything it needs.

---

## 5. Dependency Rules

```
Api  →  Application  →  Domain
            ↑               ↑
        Infrastructure ─────┘
```

- Dependencies point inward.
- **Domain never depends on Infrastructure.**
- Register infrastructure in the composition root (`Healthcare.Api`).

---

## 6. EF Core & Database

- All schema changes go through **EF Core Migrations**.
- **Never** generate destructive migrations without a warning and migration plan.
- One **schema per module** — modules never cross-query another schema.
- Use **indexes** appropriately; back foreign keys with indexes.
- Enforce referential integrity via relationships.
- Prefer strongly-typed IDs; use GUIDs only where distribution/merge is required.
- **No synchronous database calls.**

---

## 7. Performance

- **Avoid N+1 queries** — use eager loading or projections deliberately.
- Use **projections** (`.Select(...)`) to shape reads.
- Use **`AsNoTracking()`** for read-only queries.
- Always **paginate** list endpoints.
- Prefer efficient LINQ; avoid client-side evaluation.
- Avoid unnecessary allocations / large object graphs.
- **Measure before optimizing.**

---

## 8. Asynchrony

- Every async method accepts and forwards a `CancellationToken`.
- Never block (`Result`, `Wait()`, `Thread.Sleep`).
- Never block threads on async work.

---

## 9. Repositories & Abstractions

- Expose repository abstractions in `Application`; implement in `Infrastructure`.
- Keep the Domain persistence-ignorant (no EF attributes in the Domain layer).

---

## 10. Health & Build

- The solution file is `HealthcareSystem.slnx`. CI runs `dotnet format` (verify), restore, vulnerability check, build, test with coverage.
- NuGet vulnerability check (`dotnet list package --vulnerable --include-transitive`) must pass — keep dependencies patched.

⬅️ [Back to Docs index](./README.md)
