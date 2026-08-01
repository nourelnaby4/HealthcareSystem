# Testing

**Status**: Controlled standard — enforced by [Constitution Principle 15 (Testing)](../.specify/memory/constitution.md)
**Applies to**: backend (.NET) and frontend (Angular).

---

## 1. Test Pyramid

| Level | What | Scope |
|-------|------|-------|
| **Unit** | Domain logic, validators, handlers, pure functions | One class / slice, no I/O |
| **Integration** | Module + persistence + bus; HTTP endpoints | A slice against real boundaries |
| **Contract / Event** | Integration-event publishers & consumers | Cross-module contracts |
| **UI / Component** | Angular component behavior | Standalone component |

Prefer many fast unit tests, fewer integration tests, and meaningful coverage at the boundaries.

---

## 2. Mandatory Rules

- **Every feature requires tests.**
- Generate **Unit** and **Integration** tests.
- **Meaningful assertions** — verify behavior and outcomes, not just "no exception".
- **No placeholder tests.** No `Assert.True(true)` skeletons.
- Tests must be **deterministic** — no flaky timing, no reliance on shared mutable state.

---

## 3. Structure (Arrange-Act-Assert)

```
// Arrange
// Act
// Assert
```

- One logical assertion concept per test.
- Test names describe intent: `CreatePatient_WithDuplicateMrn_ThrowsConflict`.
- Test the **public behavior** of aggregates/handlers, not private internals.

---

## 4. What to Test

- **Domain:** invariants, state transitions, domain-event emission.
- **Validators:** every rule (required, length, range, business).
- **Handlers:** success path, validation failure, not-found, conflict, authorization.
- **API:** status codes, `ProblemDetails`, pagination, auth.
- **Integration events:** publish/consume round-trips, idempotency.

---

## 5. Backend (.NET)

- Framework: xUnit/NUnit (project default) + FluentAssertions where used.
- Use a real or in-memory database appropriate to the test level; isolate per test.
- Coverage is collected in CI via Coverlet (OpenCover format). Track coverage over time.
- Test command (CI): `dotnet test --configuration Release /p:CollectCoverage=true /p:CoverletOutputFormat=opencover`.

---

## 6. Frontend (Angular)

- Spec files: `*.spec.ts` colocated with the component/service.
- CI runs `npm test -- --watch=false` only if specs exist.
- Test standalone components in isolation; mock HTTP via the testing `HttpClient`.

---

## 7. Integration Events & ACL

- Verify outbound events are written to the **Outbox** on state change.
- Verify consumers are **idempotent** (duplicate delivery yields one effect).
- ACL tests assert **translation only** — provider types never leak into the domain.

⬅️ [Back to Docs index](./README.md)
