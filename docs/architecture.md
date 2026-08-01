# Architecture

**Status**: Controlled standard — enforced by [Constitution Principle 1 (Architecture First)](../.specify/memory/constitution.md)
**Applies to**: all backend modules, the composition root, and cross-module integration.

---

## 1. Architectural Style

The system is a **Modular Monolith** built with **Domain-Driven Design (DDD)**. A single deployable process hosts independent modules that share runtime but **never** share data access or internal types.

| Pillar | Standard |
|--------|----------|
| Deployment | Single deployable unit (`Healthcare.Api`), one process |
| Module boundaries | Bounded Contexts = modules; one schema per module |
| Integration | In-process bus + **Outbox/Inbox** (reliable, at-least-once, idempotent) |
| External systems | Isolated behind an **Anti-Corruption Layer (ACL)** |
| Tactical patterns | CQRS, Clean Architecture, Vertical Slice within modules |
| Principles | SOLID, Clean Code, DRY/KISS/YAGNI |

> Architecture is never compromised for implementation speed. No generated code may violate these boundaries.

---

## 2. Bounded Contexts → Modules

| Module | Context | Core responsibility |
|--------|---------|---------------------|
| `Administration` | Access Control | Users, roles, permissions, patients, facilities, audit |
| `Clinical` | Clinical | Encounters/visits, vitals, notes, diagnoses, orders |
| `Laboratory` | Laboratory | Test requests, specimens, results |
| `Pharmacy` | Pharmacy | Prescriptions, dispensing, stock |
| `Insurance` | Insurance & Billing | Coverage/eligibility, claims, invoices, payments |
| `Shared` | Kernel | Bus abstractions, Outbox/Inbox, base types |

---

## 3. Context Mapping (strategic DDD)

Relationships between modules follow these patterns (see [event-storming/04-context-mapping.md](./event-storming/04-context-mapping.md)):

| Upstream | Downstream | Pattern |
|----------|-----------|---------|
| Administration | all modules | `OHS + SK` (auth/permission claims) |
| Administration | Clinical | Customer-Supplier (`U/D`) |
| Laboratory / Pharmacy | Clinical | Customer-Supplier (`U/D`) |
| Clinical / Pharmacy | Insurance | Customer-Supplier (`U/D`) |
| 3rd-party Insurance Provider | Insurance module | **`ACL`** (translate FHIR/X12) |

**Rules:**
- Any 3rd-party dependency → downstream with an **ACL**. Provider concepts never leak into the domain.
- Internal mutual dependency → break via an integration event so one side is clearly upstream.
- Version integration-event contracts (`…V2`) when they change.

---

## 4. Per-Module DDD Layout (mandatory, consistent across all modules)

```
backend/src/Modules/<Module>/
├─ Domain/                  # Aggregates, Entities, Value Objects, Domain Events
├─ Application/             # Commands, Command Handlers, Policies, Projections
├─ Infrastructure/          # EF Core, Repositories, external adapters (ACL)
├─ Api/                     # Minimal API endpoints
└─ IntegrationEvents/       # events published to / consumed from other modules
```

| Artifact | Code location |
|----------|---------------|
| Aggregate | `Domain/<Aggregate>.cs` |
| Domain Event | `Domain/Events/<Event>.cs` |
| Command | `Application/Commands/<Command>.cs` + handler |
| Policy | `Application/Policies/<Policy>.cs` (integration-event consumer) |
| Read Model | `Application/Projections/<View>.cs` (+ query handler) |
| Integration event | `IntegrationEvents/<Event>.cs` (pub) / consumer in target module |
| External system | `Infrastructure/<Adapter>/` behind an ACL port |

---

## 5. Dependency Direction

```
Api  →  Application  →  Domain
            ↑               ↑
        Infrastructure ─────┘
```

- Dependencies point **inward** toward the Domain.
- **Domain never depends on Infrastructure.**
- Infrastructure depends on Domain and Application abstractions.
- No module references another module's `Domain` or `Infrastructure` internals — only published `IntegrationEvents`.

---

## 6. Module Isolation Rules

Modules communicate **only** through:
- Commands (modify state, within the module)
- Queries (read-only, within the module)
- Domain Events / Integration Events (cross-module)

**Forbidden:**
- Accessing another module's `DbContext` or tables directly.
- Referencing another module's internal implementation classes.
- Shared mutable state across modules (except the tiny `Shared` kernel).

---

## 7. Cross-Context Integration Flow

See [event-storming/03-integration-and-acl.md](./event-storming/03-integration-and-acl.md) for the full flow and catalog. Summary:

```
Administration.PatientAdmitted
   └─► Clinical (Encounter → Vitals → Notes → Diagnosis)
         ├─► Laboratory (LabTestOrdered → LabResultPublished)
         ├─► Pharmacy (PrescriptionRequested → MedicationDispensed)
         └─► Insurance (EncounterDischarged → ClaimSubmitted → ACL → Invoice → Payment)
```

- Integration events flow on the in-process bus via **Outbox** (publish) / **Inbox** (consume, idempotent).
- The `Insurance` module owns the ACL to the external `InsuranceProviderGateway` (HL7 FHIR / X12 837-835).

---

## 8. Governance

This standard is **controlled** by the constitution. Any architectural deviation requires a documented Architecture Decision Record (ADR) under `docs/adr/` and constitution amendment per the Governance rules in the constitution.

⬅️ [Back to Docs index](./README.md)
