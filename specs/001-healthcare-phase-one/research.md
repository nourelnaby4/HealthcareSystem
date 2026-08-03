# Research — Healthcare System Phase 1 (Foundation & Administration)

**Phase 0 output** for [plan.md](./plan.md). Resolves every NEEDS CLARIFICATION / open technical question, records best practices and integration patterns. Each entry: **Decision → Rationale → Alternatives**.

---

## 1. Authentication & token strategy (NEEDS CLARIFICATION from spec)

**Decision**: Stateless JWT access tokens (short-lived, ~10–15 min) + rotating refresh tokens (sliding, ~7 days, stored hashed server-side, single-use with reuse-detection) issued by the Administration module.

**Rationale**: The security standard mandates signed tokens issued by Administration, short-lived access tokens, and a secure refresh flow with rotation + expiration validation. JWT keeps other modules stateless (they validate signature/issuer/audience/expiry from claims — the `OHS + SK` published contract). Rotating, single-use refresh tokens with reuse detection mitigate token theft.

**Alternatives considered**:
- *OpenIddict / IdentityServer full OAuth2 server*: heavier; justified only if third-party clients / external OIDC are needed — out of scope for phase 1. Keep a thin internal issuer; can swap to OpenIddict later behind an interface.
- *Reference tokens (introspection)*: adds per-request DB hit; rejected for modularity (modules should not call Administration).
- *Cookie session*: rejected — modules must authorize from claims, not shared session state.

---

## 2. Password hashing & credential storage

**Decision**: Hash passwords with **BCrypt** (`BCrypt.Net-Next`) with a work factor ≥ 12. Store only the hash; never the plaintext or a reversible form. Never log credentials.

**Rationale**: Standard, audited KDF; aligns with OWASP password-storage guidance and the security standard's "never log passwords/secrets".

**Alternatives**: Argon2id (stronger but needs a native dependency on some platforms) — acceptable future swap behind a `IPasswordHasher` port; PBKDF2 (acceptable, weaker default). Keep BCrypt for phase 1.

---

## 3. Authorization model (role + claim policies)

**Decision**: Role-based + claims-based policies. Roles are first-class aggregates; each role aggregates a set of permissions (claims). On login, the issued JWT contains role + permission claims. ASP.NET Core authorization uses `[Authorize(Policy = "...")]` where each policy requires a permission claim (e.g., `patients.write`, `users.manage`). Default-deny: every endpoint requires an explicit authorize policy.

**Rationale**: Constitution Principle 12 mandates role **and** claim policies and default-deny. Mapping policies to permissions (not bare roles) keeps the UI and server consistent and avoids hardcoding role names in handlers.

**Alternatives**: Bare role checks (`[Authorize(Roles="Administrator")]`) — rejected; couples logic to role names and is hard to evolve.

---

## 4. Seeded initial administrator & first-login change

**Decision**: On first run (empty users table), a bootstrap administrator is created from configuration/secret store (username + temporary password from env). The user record carries a `MustChangePassword` flag enforced on login, forcing a change before any other action.

**Rationale**: Satisfies spec FR16 and the security standard (secrets from secret store, never the repo). Avoids unauthenticated "setup" endpoints.

**Alternatives**: Unauthenticated setup wizard endpoint — rejected (attack surface); manual SQL seed — rejected (no audit, secret in script).

---

## 5. Patient identity & uniqueness (spec Open Question 2)

**Decision**: Enforce uniqueness on a generated **MRN** (Medical Record Number), system-issued (e.g., sequential + check digit, or a facility-scoped format) and immutable post-creation. Capture optional national/insurance IDs as separate, non-uniqueness-guaranteed fields for now.

**Rationale**: Spec default assumption = MRN. A system-generated MRN removes receptionist-entered duplicates and is the reliable uniqueness key (spec FR7). National/insurance IDs are deferred until a compliance decision (spec Open Question 3).

**Alternatives**: National ID as the unique key — rejected for phase 1 (jurisdiction/privacy variability, not yet confirmed); composite uniqueness — premature.

---

## 6. Compliance scope (spec Open Question 3)

**Decision**: For phase 1, follow the documented [security standard](../../docs/security.md) (least privilege, PHI audit, HTTPS, no PHI in logs, safe errors). Do **not** claim a specific regulatory certification. Record any stricter requirement (e.g., HIPAA) as a follow-up if/when confirmed.

**Rationale**: No compliance regime was confirmed; the security standard already encodes the baseline controls. Premature over-engineering (e.g., field-level encryption-at-rest keys) violates YAGNI.

**Alternatives**: Assume HIPAA-level controls now — rejected without explicit requirement.

---

## 7. Integration bus & Outbox/Inbox reliability

**Decision**: In-process integration bus (`Shared/IntegrationBus`) with the **Outbox/Inbox** pattern. A command handler writes domain changes + outbox messages in the **same EF transaction**; a background dispatcher (`IHostedService`) reads unsent outbox rows and publishes to in-process subscribers; consumers record processed message ids in the **inbox** for idempotency. Messages carry an idempotency key + versioned contract.

**Rationale**: Constitution/architecture mandate reliable, at-least-once, idempotent cross-module delivery. In-process bus matches the modular-monolith single-deployable model (no external broker needed yet). Same-transaction outbox guarantees "patient saved ⇔ event recorded" without 2PC.

**Alternatives**:
- *MediatR notifications as the integration bus*: fine for in-process delivery, but not durable — Outbox wraps it for reliability.
- *External broker (RabbitMQ/Kafka)*: rejected for phase 1 (YAGNI); the abstraction allows adding it later behind `IIntegrationEventPublisher`.
- *EF `SaveChanges` interceptor vs explicit outbox write*: use MediatR pipeline/SaveChanges interceptor to capture domain events → outbox automatically.

---

## 8. EF Core multi-schema modular design

**Decision**: One `DbContext` per module, each mapped to its own PostgreSQL schema (`administration`). Each module's context is registered independently; the composition root (`Healthcare.Api`) registers all contexts, runs their migrations, and wires the bus. The outbox/inbox live in a small shared schema owned by the `Shared` kernel (a dedicated `IntegrationDbContext` or co-located table) — modules publish/consume via abstractions, never reaching another module's schema.

**Rationale**: Constitution Principle 2/6 — modules never cross-query another schema; one schema per module ([backend-guidelines §6](../../docs/backend-guidelines.md)).

**Alternatives**: Single shared `DbContext` across modules — rejected (violates isolation); schema-per-aggregate — too granular.

---

## 9. Password/token security hardening

**Decision**: Constant-time credential comparison (via BCrypt's inherent constant-time compare), generic "invalid credentials" messages, login attempt throttling/rate-limit per username + IP, account suspension enforcement on every request, and short-lived access tokens. Reject after N failed attempts with consistent timing to avoid user enumeration.

**Rationale**: Security standard (consistent timing, no enumeration, OWASP).

---

## 10. Auditing PHI access

**Decision**: An `AuditEntry` aggregate records actor (UserId), action, resource type + id, timestamp, and correlation/request id. Writes occur in-command (within the same transaction) for mutations, and via a MediatR pipeline behavior/query side for sensitive reads (e.g., patient detail views). Administrators query the audit log through a paginated/filterable read model.

**Rationale**: Security standard mandates auditing PHI access via the Administration `AuditEntry` aggregate.

---

## 11. Frontend stack alignment (Angular 22 / Vitest / Tailwind 4)

**Decision**: Build on the **installed** Angular 22 + TypeScript 6.0.2 + **Vitest** (`@angular/build:unit-test`) + Tailwind CSS 4. Amend the constitution/standards (Angular `20 → 22`, testing doc Karma → Vitest, global stylesheet `styles.css → styles.scss`) so governance matches reality. Keep all coding rules from [angular-guidelines.md](../../docs/angular-guidelines.md) (standalone, signals, reactive forms, OnPush, no `any`).

**Rationale**: Downgrading is impractical; Angular 22 is a superset of 20 (signals, standalone, control flow unchanged). The constitution's amendment procedure exists exactly for this. Amending keeps the "Constitution is supreme" rule coherent.

**Alternatives**: Downgrade to Angular 20 — rejected (wastes the installed toolchain); ignore the drift — rejected (leaves governance inconsistent).

---

## 12. Frontend auth & API integration

**Decision**: `core/interceptors/auth.interceptor.ts` attaches the bearer token from an `AuthService` signal store; `core/interceptors/error.interceptor.ts` maps `ProblemDetails` to user-friendly messages + toasts. `core/guards/` provide `authGuard` and `roleGuard(roles)`. Routes lazy-load layouts (`main-layout`, `auth-layout`) and features. `provideHttpClient(withInterceptors([...]))` in `app.config.ts`. Token + refresh stored in memory (signal) with a silent refresh via refresh token; on 401, attempt one refresh then redirect to login.

**Rationale**: Frontend architecture/angular guidelines centralize HTTP in services + interceptors, lazy-load features, and protect routes via guards.

---

## 13. HTTP API conventions for phase 1

**Decision**: REST, `/api/v1` prefix, plural kebab-case resources (`/api/v1/users`, `/api/v1/patients`, `/api/v1/roles`, `/api/v1/facilities`, `/api/v1/audit-entries`, `/api/v1/auth/login`, `/api/v1/auth/refresh`). Collections support `?page&pageSize&q&sort&status` with a `{ items, totalItems, page, pageSize }` envelope. Errors are RFC 9457 `application/problem+json`. `POST` create → `201` + `Location`. OpenAPI exposed in Development. See [contracts/](./contracts/).

**Rationale**: [api-design.md](../../docs/api-design.md) standard.

---

## 14. Performance & observability

**Decision**: `AsNoTracking` + projections for reads; pagination enforced (cap page size, default 20); unique index on MRN; indexes on all FKs; Serilog structured logging with a correlation id middleware; `/health` readiness check (DB + self). Background outbox dispatcher uses a bounded polling interval + cancellation. No request-thread blocking on the bus.

**Rationale**: Constitution Principles 11/13.

---

## Research outcome

All NEEDS CLARIFICATION items resolved. The design proceeds with: JWT + rotating refresh, BCrypt, role+permission claim policies, system-generated unique MRN, in-process bus with Outbox/Inbox, one-schema-per-module DbContexts, Angular 22 + Vitest (constitution amendment), and the REST conventions above. No blockers for Phase 1 design.
