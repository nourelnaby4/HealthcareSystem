# Coding Standards

**Status**: Controlled standard — enforced by [Constitution Principles 19 (Code Quality), 10 (Error Handling), 11 (Logging), 23 (Output Quality)](../.specify/memory/constitution.md)
**Applies to**: all code (backend and frontend).

---

## 1. Foundational Principles

Always follow:

| Principle | Meaning |
|-----------|---------|
| **SOLID** | Single responsibility, open/closed, Liskov, interface segregation, dependency inversion |
| **DRY** | No duplicated logic — extract once, reuse |
| **KISS** | Prefer the simplest correct solution |
| **YAGNI** | Don't build for speculative needs |
| **Boy Scout Rule** | Leave code cleaner than you found it |

Prefer **readable code over clever code**. Prefer **explicitness over magic**.

---

## 2. Code Quality

- **No dead code.** Remove unused members, unreachable branches, disabled tests.
- **No unused dependencies.** Remove NuGet/npm packages that aren't referenced.
- **No unnecessary abstractions.** Introduce an interface only when there is a second implementation or a seam for testing.
- **No duplicated code.** Shared logic lives in the `Shared` kernel or a module's `Application` layer — never copy-pasted across slices.
- **No placeholders / TODOs / pseudo-code** in generated deliverables. Generate complete, compiling implementations.

---

## 3. Functions & Classes

- One responsibility per class/method.
- Small methods — a method does one thing at one level of abstraction.
- Few arguments; avoid boolean flag parameters (split into two methods).
- Fail fast: validate preconditions early, return/throw early.
- Avoid deep nesting; use guard clauses.

---

## 4. Error Handling

- **Never expose stack traces** to clients.
- Return standardized `ProblemDetails` for API errors (see [api-design.md](./api-design.md)).
- Log internal exceptions with structured logging.
- Users receive safe, non-revealing messages.
- Prefer domain exceptions over returning `null` for error states.

---

## 5. Logging

- Use **structured logging** (named placeholders, not string interpolation).
- **Never** log passwords, secrets, access tokens, or PHI unless explicitly authorized.
- Log every important business/audit action with a correlation/request id.
- Use appropriate levels: `Error` (failures), `Warning` (degraded), `Information` (business events), `Debug`/`Trace` (diagnostics).

---

## 6. Async & Concurrency (backend)

- **Every** async method accepts and forwards a `CancellationToken`.
- Never use `.Result` / `.Wait()` — never block on async code.
- Prefer async all the way down; no synchronous database calls.
- Never call async code from synchronous contexts in a blocking manner.

---

## 7. Comments

- Code documents **what** is obvious; comments explain **why**.
- Complex business rules require a comment citing the rule.
- Avoid redundant comments that restate code.
- Architecture decisions belong in ADRs, not inline prose.

---

## 8. Git Discipline

- **Conventional Commits** format:

  ```
  feat(patient): add create patient endpoint
  fix(auth): validate refresh token expiration
  refactor(appointment): simplify scheduling logic
  test(patient): add integration tests
  docs(api): update OpenAPI documentation
  ```

- Meaningful, scoped commit messages. One logical change per commit.
- See [naming.md](./naming.md) for branch and file naming conventions.

---

## 9. AI Output Quality

- Generated code **must compile** and follow project conventions.
- Prefer maintainability over brevity.
- Never invent business rules, fabricate APIs, or assume a database schema.

⬅️ [Back to Docs index](./README.md)
