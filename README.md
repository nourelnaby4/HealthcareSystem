# 🏥 Healthcare System

A healthcare information system built as a **modular monolith** using **Domain-Driven Design (DDD)** and **.NET 10**.

## 🧩 Bounded Contexts (Modules)

| Module | Context | Core responsibility |
|--------|---------|---------------------|
| `Administration` | 🛡️ Access Control | Users, roles, permissions, patients, facilities, audit |
| `Clinical` | 🏥 Clinical | Encounters/visits, vitals, notes, diagnoses, orders |
| `Laboratory` | 🧪 Laboratory | Test requests, specimens, results |
| `Pharmacy` | 💊 Pharmacy | Prescriptions, dispensing, stock |
| `Insurance` | 💰 Insurance & Billing | Coverage/eligibility, claims, invoices, payments |
| `Shared` | 🔧 (kernel) | Bus abstractions, Outbox/Inbox, base types |

Modules integrate via **integration events** on an in-process bus (Outbox/Inbox). Third-party **Insurance** providers are isolated behind an **Anti-Corruption Layer** (FHIR / X12).

## 🏗️ Tech Stack

- **.NET 10**, ASP.NET Core Web API (OpenAPI)
- **PostgreSQL** — relational database (one schema per module)
- **EF Core** — data access / migrations
- Modular monolith — each module owns its Domain / Application / Infrastructure / Api layers
- Frontend: `frontend/healthcare-web` — **Angular** + **Tailwind CSS**
- Containerized via `docker/`

## 📁 Repository Layout

```
backend/
├─ src/
│  ├─ Bootstrapper/Healthcare.Api/   # composition root
│  └─ Modules/                       # Administration, Clinical, Laboratory, Pharmacy, Insurance
└─ test/
docs/                                # all project documentation
frontend/healthcare-web/
docker/
```

## 📖 Documentation

- 🌩️ **[Event Storming](./docs/event-storming/README.md)** — workflow, seeded domain storm, integration & ACL, context mapping, module mapping
- 📚 **[Docs index](./docs/README.md)**

## 🚀 Getting Started

```bash
# Backend API
dotnet run --project backend/src/Bootstrapper/Healthcare.Api/Healthcare.Api.csproj

# Frontend
cd frontend/healthcare-web
```

> Detailed setup & module guides will be added as the project is scaffolded from the Event Storming output.
