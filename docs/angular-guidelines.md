# Angular Guidelines

**Status**: Controlled standard — enforced by [Constitution Principles 16 (Angular), 17 (UI)](../.specify/memory/constitution.md)
**Applies to**: `frontend/healthcare-web` — Angular 22.

> **Architecture & styling stack** (folder structure, layouts/pages/modules/interceptors/interfaces, Tailwind + HTML + SCSS) are defined in [frontend-architecture.md](./frontend-architecture.md). This document defines the **coding rules**.

---

## 1. Framework & Style

| Concern | Choice |
|---------|--------|
| Framework | **Angular 22** |
| Components | **Standalone components** (no NgModules) |
| Template | **HTML** (`.html`) — one template file per component |
| Styling | **Tailwind CSS** + **SCSS** (`.scss`) component-scoped |
| State | **Signals** first |
| Forms | **Reactive Forms** |
| Language | **Strict TypeScript** |
| Build | Node 22, `npm ci`, `npm run build` |

---

## 2. Component Rules

- Use **standalone components** exclusively.
- Use **Signals** (`signal`, `computed`, `effect`) for component/local state.
- Use **`input()` / `output()` / `model()`** signal-based APIs over `@Input`/`@Output` decorators.
- Prefer `ChangeDetectionStrategy.OnPush`.
- **Avoid `any`** — define and reuse types/interfaces that mirror the backend DTOs (see [frontend-architecture.md](./frontend-architecture.md) §interfaces).

---

## 3. RxJS

- Use **RxJS only when Signals are insufficient** (streams, debounced search, websockets).
- Avoid manual subscriptions without `takeUntilDestroyed` / async pipe.
- Prefer the **async pipe** and `toSignal`/`toObservable` interop over manual `subscribe`.

---

## 4. Forms

- Use **Reactive Forms** (`FormGroup`, `FormControl`, `FormBuilder`).
- Validate on the client; mirror backend validation rules.
- Surface validation errors inline with clear messages.

---

## 5. HTTP & State

- Centralize HTTP in **services**; never call `HttpClient` from components/pages.
- Use **interceptors** (in `core/interceptors/`) for auth tokens and error mapping.
- Map API errors to user-friendly messages; never expose raw stack traces.
- Service methods return typed responses using `interfaces/` types.

---

## 6. UI / UX

Every interactive view must handle:

- **Loading** states (skeletons/spinners)
- **Error** states (retry affordance)
- **Empty** states (guidance, no raw blank screens)
- **Form validation** feedback

The UI must be:
- **Responsive** (mobile → desktop) via Tailwind responsive utilities
- **Accessible** (semantic HTML, ARIA where needed, keyboard navigable, sufficient contrast)
- **Consistent** (shared design tokens + components in `shared/`)

---

## 7. TypeScript Discipline

- `strict: true`.
- No `any`; no `// @ts-ignore` without a justified comment.
- Define one **interface** per data entity under `interfaces/`; prefer `type` only for unions/intersections.

⬅️ [Back to Docs index](./README.md)
