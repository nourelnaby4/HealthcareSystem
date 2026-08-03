# Data Model — Healthcare System Phase 1 (Foundation & Administration)

**Phase 1 output**. Schemas live in PostgreSQL: the `administration` schema (module-owned) and the `integration` schema (Outbox/Inbox, owned by the `Shared` kernel). All ids are strongly-typed (`UserId`, `PatientId`, …) backed by `Guid`; aggregates enforce invariants in the Domain layer; EF mappings live in Infrastructure (Domain is persistence-ignorant). Naming follows [naming.md](../../docs/naming.md).

> This model covers the Administration module + Shared kernel only. Clinical/Laboratory/Pharmacy/Insurance aggregates arrive in later phases.

---

## Aggregate map

| Aggregate | Root | Consistency boundary | Schema |
|-----------|------|----------------------|--------|
| Identity | `User` | one staff account, its credentials, role assignments, refresh tokens | `administration` |
| Authorization | `Role` | a role and its permission set | `administration` |
| Patient | `Patient` | one patient's demographics + immutable MRN | `administration` |
| Facility | `Facility` | one care location | `administration` |
| Audit | `AuditEntry` | one immutable access record | `administration` |
| (kernel) | Outbox / Inbox rows | reliable integration delivery | `integration` |

`Permission` is a **value object** (a claim string, e.g. `patients.write`), not an aggregate. `RoleAssignment` is modeled as the `User`↔`Role` many-to-many link owned by `User`.

---

## Entity: `User` (Identity aggregate root)

| Field | Type | Notes / validation |
|-------|------|--------------------|
| `Id` | `UserId` (Guid) | PK; strongly-typed. |
| `UserName` | string | Unique, case-insensitive; 3–64 chars; alphanumeric/`.-_@`. |
| `Email` | string (Email VO) | Unique; valid email; ≤ 256 chars. |
| `PasswordHash` | string | BCrypt hash; never exposed/logging. |
| `FirstName` / `LastName` | string | 1–100 chars each. |
| `Status` | `UserStatus` enum | `Active` \| `Suspended`. |
| `MustChangePassword` | bool | True on bootstrap; cleared on change. |
| `FailedLoginCount` | int | For throttling; reset on success. |
| `LockoutEnd` | DateTimeOffset? | When throttled until. |
| `Roles` | `ICollection<Role>` | Many-to-many (UserRoles). |
| `RefreshTokens` | `ICollection<RefreshToken>` | Owned children. |
| `CreatedAt` / `UpdatedAt` | DateTimeOffset | Audit timestamps. |

**Invariants (enforced in Domain):**
- A user has ≥ 0 roles; assigning/suspending emits domain events.
- `Suspended` users cannot authenticate; reactivation flips `Status` → `Active`.
- `MustChangePassword=true` blocks all non-password-change actions until cleared.
- Throttle after `MaxFailedLogins` (default 5) within a window → set `LockoutEnd`.

**State transitions:** `Registered → Active ⇄ Suspended` (Admin actions); `Active` may carry `MustChangePassword`.

**Domain events:** `UserRegistered`, `RoleAssigned`, `RoleRevoked`, `UserSuspended`, `UserReactivated`, `PasswordChanged`, `SessionRevoked`.

---

## Entity: `Role` (Authorization aggregate root)

| Field | Type | Notes / validation |
|-------|------|--------------------|
| `Id` | `RoleId` (Guid) | PK. |
| `Name` | string | Unique; 2–64 chars; PascalCase display (e.g. `Administrator`). |
| `Description` | string? | ≤ 256 chars. |
| `Permissions` | `ICollection<Permission>` (VO) | Permission claim strings. |
| `IsSystem` | bool | True for seeded roles (Administrator/Receptionist/Clinician) — cannot be deleted. |
| `CreatedAt` / `UpdatedAt` | DateTimeOffset | — |

**Invariants:**
- A permission string matches `^[a-z][a-z0-9-]*(\.[a-z0-9-]+)+$` (e.g. `patients.write`).
- System roles cannot be deleted; their permission set may be edited only by an Administrator.
- Removing a permission from a role does not delete existing users.

**Domain events:** `RoleCreated`, `PermissionGranted`, `PermissionRevoked`.

---

## Value Object: `Permission`

| Field | Type | Validation |
|-------|------|-----------|
| `Value` | string | Required; matches permission regex; ≤ 64 chars. |

Seed permission catalog (examples): `users.manage`, `roles.manage`, `patients.write`, `patients.read`, `facilities.manage`, `audit.read`.

---

## Entity: `RefreshToken` (child of `User`)

| Field | Type | Notes |
|-------|------|-------|
| `Id` | Guid | PK. |
| `TokenHash` | string | Hash of the refresh token (never store raw). |
| `ExpiresAt` | DateTimeOffset | ≤ 7 days from issue. |
| `RevokedAt` | DateTimeOffset? | Null = active. |
| `ReplacedByTokenHash` | string? | For rotation chain. |
| `CreatedAt` | DateTimeOffset | — |

**Invariants:** single-use (revoked on refresh); reuse of a revoked token revokes the entire chain (reuse detection) → forces re-login.

---

## Entity: `Patient` (Patient aggregate root)

| Field | Type | Notes / validation |
|-------|------|--------------------|
| `Id` | `PatientId` (Guid) | PK. |
| `Mrn` | string | **Unique**, system-generated, immutable; format `<FAC>-NNNNNN` + check digit; unique index. |
| `FirstName` / `LastName` | string | 1–100 chars. |
| `DateOfBirth` | DateOnly VO | Not in future; not > 130 yrs ago. |
| `Gender` | `Gender` enum? | `Male`\|`Female`\|`Other`\|`Unknown`. |
| `NationalId` | string? | Optional; ≤ 32 chars; not used for uniqueness in phase 1. |
| `Phone` | string? (Phone VO) | E.164-ish; ≤ 20 chars. |
| `Email` | string? (Email VO) | Valid email. |
| `Address` | `Address` VO? | Street/city/state/postal/country. |
| `BloodType` | string? | `A+`…`AB-` / `Unknown`. |
| `Status` | `PatientStatus` enum | `Active` \| `Inactive` \| `Deceased`. |
| `CreatedAt` / `UpdatedAt` | DateTimeOffset | — |

**Invariants:**
- `Mrn` is unique across the system and **system-generated** (the client never supplies it), so duplicate-MRN creation is impossible by construction. A unique DB index remains as a data-integrity safety net (not a client-facing rejection path).
- Required: first/last name, date of birth.
- `Mrn` immutable after creation.

**State transitions:** `Registered → Active → Inactive` (and terminal `Deceased`).

**Domain events:** `PatientRegistered` (also published as integration event — see [contracts/patient-admitted.integration.md](./contracts/patient-admitted.integration.md)), `PatientUpdated`, `PatientDeactivated`.

---

## Value Objects (reused)

- `Email` — normalized lowercase; RFC-ish validation.
- `Phone` — digits/`+-() `, length ≤ 20.
- `Address` — `Street`, `City`, `State`, `PostalCode`, `Country`; each bounded length.
- `DateOfBirth` — `DateOnly` with range validation.

---

## Entity: `Facility` (Facility aggregate root)

| Field | Type | Notes / validation |
|-------|------|--------------------|
| `Id` | `FacilityId` (Guid) | PK. |
| `Code` | string | Unique; 2–16 chars; uppercase. |
| `Name` | string | 2–128 chars. |
| `Type` | `FacilityType` enum | `Hospital`\|`Clinic`\|`Ward`\|`Department`\|`Pharmacy`. |
| `Address` | `Address` VO? | — |
| `Status` | `FacilityStatus` enum | `Active` \| `Inactive`. |
| `CreatedAt` / `UpdatedAt` | DateTimeOffset | — |

**Invariants:** `Code` unique; cannot delete a facility referenced by patients (set `Inactive`).

**Domain events:** `FacilityRegistered`, `FacilityUpdated`, `FacilityDeactivated`.

---

## Entity: `AuditEntry` (Audit aggregate root, append-only)

| Field | Type | Notes |
|-------|------|-------|
| `Id` | Guid | PK. |
| `ActorId` | `UserId`? (Guid?) | Null for system/anonymous. |
| `ActorName` | string | Denormalized for readability. |
| `Action` | string | `patient.create`, `patient.view`, `user.suspend`, etc. |
| `ResourceType` | string | `Patient`, `User`, … |
| `ResourceId` | string? | The entity id (string for genericity). |
| `OccurredAt` | DateTimeOffset | UTC. |
| `CorrelationId` | Guid? | Request id. |
| `IpAddress` | string? | For access events. |
| `Detail` | string? | Free-text, **PHI-minimal**; ≤ 512 chars. |

**Invariants:** append-only; never updated or deleted; no PHI beyond resource id/action.

---

## Kernel: Outbox / Inbox (`integration` schema)

**OutboxMessage**

| Field | Type | Notes |
|-------|------|------|
| `Id` | Guid | PK = message id (idempotency key). |
| `Type` | string | Contract type name, e.g. `PatientAdmitted`. |
| `Payload` | string | Serialized JSON (versioned contract). |
| `OccurredAt` | DateTimeOffset | — |
| `ProcessedAt` | DateTimeOffset? | Null = pending. |
| `RetryCount` | int | — |

**InboxMessage** (consumer dedupe)

| Field | Type | Notes |
|-------|------|------|
| `Id` | Guid | PK = incoming message id. |
| `Type` | string | — |
| `ProcessedAt` | DateTimeOffset | — |

**Behavior:** written in the same transaction as domain changes; a hosted dispatcher publishes pending rows to in-process handlers; consumers check `InboxMessage` to ensure exactly-once effect despite at-least-once delivery.

---

## Relationships

```text
User 1──* UserRoles *──1 Role
User 1──* RefreshToken
User 1──* AuditEntry (as actor)
Role *──* Permission (VO collection)
Patient (references FacilityId loosely; no hard FK across future schemas in phase 1
        — stored as FacilityId Guid on Patient for the admitting facility)
AuditEntry → (loosely typed ResourceType/ResourceId)
```

> Per module isolation, cross-module links are by id only (no FKs across module schemas). Within Administration, FKs enforce integrity and carry indexes (`IX_UserRoles_UserId`, `IX_UserRoles_RoleId`, `IX_RefreshTokens_UserId`, `IX_AuditEntries_ActorId_OccurredAt`, `IX_Patients_Mrn` unique).

---

## Seed data (first run)

- **Roles:** `Administrator` (all permissions), `Receptionist` (`patients.read/write`, `patients`), `Clinician` (`patients.read`).
- **Permissions:** the catalog above.
- **Bootstrap user:** one `Administrator` from secret-store credentials with `MustChangePassword = true`.

---

## Migration & indexing notes

- EF Core migrations per module context; non-destructive by default; unique index on `Patients.Mrn`, `Users.UserName`, `Users.Email`, `Roles.Name`, `Facilities.Code`.
- Indexes back every FK; composite index on `AuditEntries(OccurredAt DESC, ActorId)` for log browsing.
- `Created/Updated` as `timestamptz`; enums stored as `text` (PostgreSQL) for readability + forward-compat.
