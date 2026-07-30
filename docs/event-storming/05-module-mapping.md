# 05 — Output → Modular Monolith Mapping

How each Event Storming artifact maps to a concrete location in the codebase.

---

## Solution / module layout

```
backend/src/
├─ Bootstrapper/
│  └─ Healthcare.Api/                 # composition root, wires modules + bus
├─ Modules/
│  ├─ Administration/
│  ├─ Clinical/
│  ├─ Laboratory/
│  ├─ Pharmacy/
│  └─ Insurance/
│     └─ Infrastructure/
│        └─ InsuranceProviderGateway/  # ACL + outbound/inbound adapters
└─ Shared/
   ├─ Kernel/                          # base types, IDs, result
   ├─ IntegrationBus/                  # in-process pub/sub abstractions
   └─ Outbox/                          # reliability for integration events
```

---

## Per-module DDD layout (consistent across all modules)

```
Modules/<Module>/
├─ Domain/                  # Aggregates, Entities, Value Objects, Domain Events
├─ Application/             # Commands, Command Handlers, Policies, Projections
├─ Infrastructure/          # EF Core, Repositories, external adapters (ACL)
├─ Api/                     # Controllers / minimal API endpoints
└─ IntegrationEvents/       # events published to / consumed from other modules
```

---

## Artifact → code mapping

| Storm artifact | Code location |
|----------------|---------------|
| 🟡 Aggregate | `Domain/<Aggregate>.cs` |
| 🔶 Domain Event | `Domain/Events/<Event>.cs` |
| 🔵 Command | `Application/Commands/<Command>.cs` + handler |
| 🟣 Policy | `Application/Policies/<Policy>.cs` (integration-event consumer) |
| 🟩 Read Model | `Application/Projections/<View>.cs` (+ query handler) |
| Cross-context 🔶 event | `IntegrationEvents/<Event>.cs` (pub) / consumer in target module |
| 🟥 External System | `Infrastructure/<Adapter>/` behind an ACL port |
| 📦 Bounded Context | the module project itself |

---

⬅️ [04 — Context Mapping](./04-context-mapping.md) · 🏠 [Index](./README.md)
