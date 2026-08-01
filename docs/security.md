# Security

**Status**: Controlled standard — enforced by [Constitution Principles 9 (Validation), 10 (Error Handling), 11 (Logging), 12 (Security)](../.specify/memory/constitution.md)
**Applies to**: the entire system. Healthcare data is sensitive — security is **mandatory and non-negotiable**.

---

## 1. Core Requirements

- **Authentication** and **Authorization** on every protected endpoint.
- **Role-based** and **Claims-based** policies.
- **HTTPS** everywhere (redirect HTTP; HSTS in production).
- **Input validation** at the boundary (FluentValidation) — never trust client input.
- **Output encoding** to prevent injection in rendered output.
- **Parameterized queries** only — no string-concatenated SQL.
- Follow **OWASP Top 10** in every change.

Never generate known-vulnerable code.

---

## 2. Healthcare-Sensitive Data (PHI)

This system handles **Protected Health Information**. Treat all clinical, lab, pharmacy, insurance, and patient-identity data as PHI.

- Apply **least privilege** — a user sees only the data their role and assignment require.
- **Audit** access to PHI (who/what/when) via the `Administration` `AuditEntry` aggregate.
- **Never log** PHI, credentials, tokens, or secrets unless explicitly authorized.
- Prefer **minimal DTOs** — don't return fields the caller doesn't need.

> Where regulatory scope is required (e.g. HIPAA-style safeguards), record the requirement as a clarification and enforce the technical control (encryption, access control, audit).

---

## 3. Authentication & Tokens

- Authenticate via signed tokens (e.g. JWT) issued by `Administration`.
- Other modules read **auth claims** (a published, stable contract — `OHS + SK`), never Administration's database.
- Short-lived access tokens; secure refresh-token flow with rotation and expiration validation.
- Store secrets in configuration/secrets store, **never** in code or logs.
- Validate token signature, issuer, audience, and expiration on every request.

---

## 4. Authorization

- Authorize by **role** and **claim** policies, not by hiding UI alone.
- Enforce authorization on the **server** for every command/query.
- Default-deny: a new endpoint requires an explicit `[Authorize]` policy.

---

## 5. Input & Output

- Validate IDs, dates, lengths, required fields, and business rules before processing.
- Constrain and sanitize all free-text input.
- Encode output; never reflect raw input into HTML/SQL/commands.

---

## 6. Dependency & Supply-Chain Safety

- CI runs `dotnet list package --vulnerable --include-transitive` — it must pass.
- Keep NuGet and npm dependencies patched.
- Review new dependencies before adding them.

---

## 7. Error Handling (security lens)

- Never reveal internals: stack traces, file paths, SQL, or provider payloads.
- Return generic, safe messages to clients; log details server-side with a request id.
- Use consistent timing where feasible to avoid user-enumeration leaks.

---

## 8. External Integrations (ACL)

- The 3rd-party **Insurance Provider Gateway** (FHIR/X12) is reached **only** through the `Insurance` module's ACL.
- Translate provider payloads at the boundary; provider concepts never enter the domain.
- Apply **retries + circuit-breaker** around the gateway; rate-limit and authenticate outbound calls.

---

## 9. Secrets & Configuration

- Secrets come from environment/secret store at runtime, not from the repo.
- The `.gitignore` excludes secrets; never commit keys, connection strings with passwords, or tokens.

⬅️ [Back to Docs index](./README.md)
