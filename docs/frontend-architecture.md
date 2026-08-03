# Frontend Architecture

**Status**: Controlled standard — enforced by [Constitution Principles 16 (Angular), 17 (UI)](../.specify/memory/constitution.md)
**Applies to**: `frontend/healthcare-web` — the Angular 22 frontend. Companion to [angular-guidelines.md](./angular-guidelines.md) (rules) and [architecture.md](./architecture.md) (system/backend).

---

## 1. Technology Stack

| Concern | Choice |
|---------|--------|
| Framework | **Angular 22** |
| Components | **Standalone components** (no NgModules) |
| Template | **HTML** (`.html`) — one template file per component |
| Styling | **Tailwind CSS** (utility-first) + **SCSS** (`.scss`) component-scoped |
| State | **Signals** first |
| Forms | **Reactive Forms** |
| Language | **Strict TypeScript** |
| Build | Node 22, `npm ci`, `npm run build` |

---

## 2. Styling Stack — Tailwind + HTML + SCSS

- **Templates are HTML files** (`<component>.component.html`). Do not inline large template strings.
- **Tailwind CSS** is the primary styling system — compose the UI with utility classes in the HTML template.
- **SCSS** (`<component>.component.scss`) is for component-scoped styles only, used when Tailwind utilities are insufficient:
  - `:host` rules, complex animations/keyframes, deep selectors, component-specific theming.
  - Keep SCSS small; never duplicate what Tailwind already provides.
- Global styles and Tailwind layers live in `src/styles.scss`. Define design tokens (colors, spacing) as Tailwind theme config — keep them consistent app-wide.
- Every component declares `styleUrl`/`styleUrls` pointing to its SCSS file (even if mostly empty) for consistency.

---

## 3. Project Structure (Modular)

The app is organized by **concern layer** at the top and by **feature module** below. Every feature is a self-contained module of **pages**, **components**, **services**, and its own **routes**.

```
src/
├─ main.ts
├─ index.html
├─ styles.scss                 # global styles + Tailwind layers/design tokens
└─ app/
   ├─ app.component.html|.scss|.ts   # root shell (renders <router-outlet>)
   ├─ app.routes.ts                  # top-level routes → lazy layouts/features
   │
   ├─ core/                          # singletons, imported ONCE in root
   │  ├─ interceptors/               # HTTP interceptors (auth, error, logging)
   │  ├─ guards/                     # route guards (auth, role)
   │  ├─ services/                   # app-wide services (auth, config, layout state)
   │  └─ models/                     # core-only types
   │
   ├─ shared/                        # reusable, dumb building blocks (no business logic)
   │  ├─ components/                 # presentational UI components (buttons, tables, cards)
   │  ├─ directives/
   │  └─ pipes/
   │
   ├─ layouts/                       # shell layouts that compose pages
   │  ├─ main-layout/                # header + sidebar + <router-outlet>
   │  └─ auth-layout/                # centered card for login/register
   │
   ├─ interfaces/                    # one TypeScript interface/type per data entity (DTOs)
   │  ├─ patient.interface.ts
   │  ├─ encounter.interface.ts
   │  ├─ user.interface.ts
   │  └─ common/                     # pagination, api-response envelopes, etc.
   │
   └─ features/                      # feature MODULES — one per backend domain
      └─ <feature>/                  # e.g. administration, clinical, laboratory
         ├─ <feature>.routes.ts      # lazy feature routes
         ├─ pages/                   # routed PAGE components (compose shared + feature components)
         ├─ components/              # feature-scoped components
         ├─ services/                # feature HTTP/state services
         └─ models/                  # feature-specific types
```

---

## 4. Layer Responsibilities

| Layer | Responsibility | May depend on |
|-------|----------------|---------------|
| `core` | App-wide singletons: interceptors, guards, auth/global services | `shared`, `interfaces` |
| `core/interceptors` | HTTP middleware (auth-token attach, error mapping, logging) | `interfaces` |
| `shared` | Dumb, reusable presentational components/directives/pipes | `interfaces` only |
| `layouts` | Shell templates that host pages via `<router-outlet>` | `shared`, `core` |
| `interfaces` | Pure TypeScript types for every data entity/DTO | nothing |
| `features/<feature>` | A domain module: pages + components + services + routes | `shared`, `core`, `interfaces` |

---

## 5. Pages, Components & Modules

- **Pages** (`features/<feature>/pages/`) are routed components. A page composes **shared components** and **feature components** and calls feature services — it contains no heavy business logic.
- **Feature modules** are self-contained: no feature imports another feature's components/services. Cross-feature communication goes through the backend API or `core` state, never direct imports.
- **Interfaces** define the shape of **every** data entity once; components and services import from `interfaces/`, never redefine shapes inline.
- **Layouts** wrap pages; routes select a layout, and pages render inside its `<router-outlet>`.
- **Shared components** are presentational and reusable across features; they never call services directly.

---

## 6. Routing & Lazy Loading

```
app.routes.ts
   └─ layouts (lazy)
        └─ features (lazy)
             └─ pages (loadComponent)
```

- **Lazy-load** every feature route (`loadComponent` / `loadChildren`).
- Top-level routes select a **layout**; feature routes are grouped under the layout.
- Guards (`core/guards/`) protect routes by authentication and role/claim.

---

## 7. State & Data Flow

- **Services** own HTTP access; never call `HttpClient` from components/pages.
- **Interceptors** (`core/interceptors/`) attach auth tokens, map errors to user-friendly messages, and log.
- Service methods are **typed** using `interfaces/` types.
- Prefer **Signals** for component and feature state; use RxJS only when Signals are insufficient.

---

## 8. UI / UX Expectations

Every interactive view must handle **loading**, **error**, **empty**, and **validation** states, and must be responsive, accessible, and consistent. See [angular-guidelines.md](./angular-guidelines.md) §UI/UX for the detailed rules.

---

## 9. Governance

This standard is **controlled** by the constitution. Any frontend-architecture deviation requires a documented rationale and a constitution amendment per the Governance rules.

⬅️ [Back to Docs index](./README.md)
