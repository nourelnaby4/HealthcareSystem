# Naming Conventions

**Status**: Controlled standard — enforced by [Constitution Principle 18 (Naming)](../.specify/memory/constitution.md)
**Applies to**: all backend and frontend code, files, folders, database objects, APIs, and Git artifacts.

---

## 1. General Rules

- Use **meaningful, intention-revealing names**.
- **Avoid abbreviations** (except well-known acronyms: `Id`, `Url`, `Dto`, `Mrn`).
- One concept = one word (don't mix `User`/`Account`/`Member` for the same idea).

---

## 2. C# / .NET

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace, class, record, struct | **PascalCase** | `PatientRegistry`, `EncounterAggregate` |
| Interface | **`I` + PascalCase** | `IPatientRepository` |
| Method, property, event | **PascalCase** | `AdmitPatient`, `OccurredOn` |
| Public field | **PascalCase** | (prefer properties) |
| Private field | **`_camelCase`** | `_clock` |
| Local variable, parameter | **camelCase** | `encounterId` |
| Constant / static readonly | **PascalCase** | `MaxPageSize` |
| Generic type parameter | **`T` + PascalCase** | `TKey`, `TEntity` |
| Async method | **`Async` suffix** | `GetPatientAsync` |

- Folder/namespace must match: `Modules/Administration/Domain/Patients` → namespace `...Administration.Domain.Patients`.
- One public type per file; filename matches the type name.

---

## 3. Domain Events & Commands

| Element | Convention | Example |
|---------|-----------|---------|
| Domain event | **Past-tense PascalCase** | `PatientAdmitted`, `LabResultPublished` |
| Integration event | **Past-tense PascalCase** + version suffix on change | `EncounterDischarged`, `EncounterDischargedV2` |
| Command | **Imperative PascalCase** | `AdmitPatient`, `OrderLabTest` |
| Handler | **`<Command>Handler`** | `AdmitPatientHandler` |
| Validator | **`<Command>Validator`** | `AdmitPatientValidator` |

---

## 4. Database (PostgreSQL / EF Core)

| Element | Convention | Example |
|---------|-----------|---------|
| Schema | one per module, **lowercase** | `administration`, `clinical` |
| Table | **PascalCase** (EF default) or snake_case per project choice — be consistent | `Patients` |
| Column | match C# property casing consistently | `DateOfBirth` |
| Foreign key | **`<ReferencedEntity>Id`** | `PatientId` |
| Index | **`IX_<Table>_<Columns>`** | `IX_Encounters_PatientId` |

- Use strongly-typed IDs (`PatientId`) rather than raw `Guid`/`int` in domain code.

---

## 5. REST / API

- Routes: plural, lowercase, **kebab-case**: `/api/patients`, `/api/lab-test-requests`.
- JSON fields: **camelCase**: `dateOfBirth`, `encounterId`.
- Query params: lowercase words: `?page=1&pageSize=20&sort=-createdAt`.

---

## 6. Angular / TypeScript

| Element | Convention | Example |
|---------|-----------|---------|
| File (component) | **kebab-case.suffix.ts** | `patient-list.component.ts` |
| File (service) | **kebab-case.service.ts** | `patient.service.ts` |
| Class / interface | **PascalCase** | `PatientListComponent`, `Patient` |
| Interface (DTO) | no `I` prefix (DTO shapes) | `Patient` |
| Method, property | **camelCase** | `loadPatients()` |
| Constant | **PascalCase** or **UPPER_SNAKE** per project | `ApiBaseUrl` |
| CSS class | **kebab-case** (Tailwind utilities preferred) | `patient-card` |
| Selector | **kebab-case prefix** | `app-patient-list` |

---

## 7. Git

| Element | Convention | Example |
|---------|-----------|---------|
| Branch | **`<type>/<scope>-<short-desc>`** | `feat/administration-patient-onboarding` |
| Commit | **Conventional Commits** | `feat(patient): add create patient endpoint` |

Commit types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `perf`, `build`, `ci`.

⬅️ [Back to Docs index](./README.md)
