# 🏥 Event Storming — Healthcare Modular Monolith

Event Storming design and workflow for the **modular monolith** (.NET 10) DDD system.

**Bounded contexts:** 🏥 Clinical · 🧪 Laboratory · 💊 Pharmacy · 💰 Insurance & Billing · 🛡️ Administration & Access Control — plus **3rd-party Insurance integration**.

---

## 📚 Documents in this folder

| Doc | What it covers |
|-----|----------------|
| [01-workflow.md](./01-workflow.md) | Notation legend, workshop phases, facilitator runbook & checklist |
| [02-domain-storm.md](./02-domain-storm.md) | Seeded storm per context: aggregates, events, commands, policies, read models |
| [03-integration-and-acl.md](./03-integration-and-acl.md) | Cross-context event flows, integration-event catalog, 3rd-party Insurance ACL |
| [04-context-mapping.md](./04-context-mapping.md) | Upstream/downstream context mapping (Customer-Supplier, ACL, OHS/PL, …) |
| [05-module-mapping.md](./05-module-mapping.md) | How storm artifacts map to the modular-monolith module & folder structure |

---

## 📦 Bounded Contexts → Modules

| Module | Context | Core responsibility |
|--------|---------|---------------------|
| `Administration` | 🛡️ Access Control | Users, roles, permissions, patients, facilities, audit |
| `Clinical` | 🏥 Clinical | Encounters/visits, vitals, notes, diagnoses, orders |
| `Laboratory` | 🧪 Laboratory | Test requests, specimens, results |
| `Pharmacy` | 💊 Pharmacy | Prescriptions, dispensing, stock |
| `Insurance` | 💰 Insurance & Billing | Coverage/eligibility, claims, invoices, payments |
| `Shared` | (kernel) | Bus abstractions, Outbox/Inbox, base types |

---

## ⏭️ Where to start

1. New to Event Storming? → [`01-workflow.md`](./01-workflow.md)
2. Want the domain model? → [`02-domain-storm.md`](./02-domain-storm.md)
3. Integrating modules / the 3rd-party insurer? → [`03-integration-and-acl.md`](./03-integration-and-acl.md)
4. Deciding module relationships? → [`04-context-mapping.md`](./04-context-mapping.md)
5. Ready to scaffold code? → [`05-module-mapping.md`](./05-module-mapping.md)
