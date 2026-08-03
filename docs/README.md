# 📚 Documentation

All project documentation lives here. The **Standards** section lists controlled documents that the [Constitution](../.specify/memory/constitution.md) enforces.

## 📐 Standards (controlled by the Constitution)

| Standard | Path | What it governs |
|----------|------|-----------------|
| 🏗️ Architecture | [architecture.md](./architecture.md) | Modular monolith, DDD, bounded contexts, context mapping, module layout, integration |
| 🖥️ Frontend Architecture | [frontend-architecture.md](./frontend-architecture.md) | Angular modular structure, layouts/pages/modules, Tailwind+HTML+SCSS, interceptors/interfaces |
| 💻 Coding Standards | [coding-standards.md](./coding-standards.md) | SOLID/DRY/KISS/YAGNI, code quality, errors, logging, async, git |
| 🧩 Backend Guidelines | [backend-guidelines.md](./backend-guidelines.md) | .NET 10, Minimal APIs, CQRS, EF Core, PostgreSQL, performance |
| 🅰️ Angular Guidelines | [angular-guidelines.md](./angular-guidelines.md) | Angular 22, standalone components, Signals, Tailwind, RxJS |
| 🌐 API Design | [api-design.md](./api-design.md) | REST verbs, status codes, ProblemDetails, pagination/versioning |
| 🔒 Security | [security.md](./security.md) | AuthN/AuthZ, OWASP, PHI handling, secrets, ACL |
| 🧪 Testing | [testing.md](./testing.md) | Test pyramid, unit/integration, coverage, idempotency |
| 🔤 Naming | [naming.md](./naming.md) | C#, TS, DB, API, and Git naming conventions |

## 🌩️ Design & Discovery

| Area | Path | Description |
|------|------|-------------|
| 🌩️ Event Storming | [event-storming/](./event-storming/README.md) | Workflow, seeded domain storm, integration & ACL, context mapping, module mapping |

> Add new topics as subfolders (e.g. `adr/`, `runbooks/`) and link them above. New standards must be referenced in the Constitution to become controlled.
