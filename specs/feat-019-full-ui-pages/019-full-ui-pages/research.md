# Research: Full UI Pages — MzansiLegal Design System

**Phase**: 0 — Pre-design research
**Branch**: `feat/019-full-ui-pages`
**Date**: 2026-03-31

---

## 1. Existing Frontend Inventory

### Decision
All eight page routes already have foundational implementations committed to `feat/018-auth-landing-page` (commit `f1fdb3e`). The files are deleted from the working tree on the new branch and must be restored before enhancement.

### Rationale
Starting from the existing implementations avoids duplication and preserves design decisions already validated in prior feature iterations.

### Files to Restore
| Route | File | Status |
|---|---|---|
| Home | `frontend/src/app/[locale]/page.tsx` | Exists in git, deleted in working tree |
| Auth | `frontend/src/app/[locale]/auth/page.tsx` | Exists in git, deleted in working tree |
| Auth Layout | `frontend/src/app/[locale]/auth/layout.tsx` | Exists in git, deleted in working tree |
| Ask | `frontend/src/app/[locale]/ask/page.tsx` | Exists in git, deleted in working tree |
| Contracts List | `frontend/src/app/[locale]/contracts/page.tsx` | Exists in git (minimal), deleted in working tree |
| Contract Detail | `frontend/src/app/[locale]/contracts/[id]/page.tsx` | Exists in git, deleted in working tree |
| My Rights | `frontend/src/app/[locale]/rights/page.tsx` | Exists in git, deleted in working tree |
| History | `frontend/src/app/[locale]/history/page.tsx` | Exists in git (basic empty state), deleted in working tree |
| Admin Dashboard | `frontend/src/app/[locale]/admin/dashboard/page.tsx` | Exists in git, deleted in working tree |

### Restore Command
```bash
git checkout HEAD -- frontend/
```

---

## 2. Design System Tokens

### Decision
Use the existing CSS variable system (`frontend/src/styles/globals.css`) and TypeScript token exports (`frontend/src/styles/theme.ts`) as the single source of truth.

### Token Map (from design PDFs)
| Token | CSS Variable | Value | Usage |
|---|---|---|---|
| Background | `--ml-bg` | `#fdfcf8` | Page background (warm off-white) |
| Foreground | `--ml-fg` | `#2c2c24` | Primary text (near-black warm) |
| Primary | `--ml-primary` | `#0d7377` | CTA buttons, active nav, icons |
| Secondary | `--ml-secondary` | `#c18c5d` | Financial tags, secondary CTA |
| Muted | `--ml-muted` | `#f0ebe5` | Card backgrounds, tag chips |
| Border | `--ml-border` | `#ded8cf` | Card borders, dividers |
| Destructive | `--ml-destructive` | `#a85448` | Red flag text, error states |
| Card | `--ml-card` | `#fefefa` | White card surface |
| Paper | `--ml-paper` | `#fbf7ef` | Slightly warm card variant |
| Accent | `--ml-accent` | `#e6dccd` | Subtle hover, contract tag |

### Typography
| Token | Value | Usage |
|---|---|---|
| Serif font (`fontSerif`) | Fraunces (Google Fonts) | Hero headlines, rights card titles, contract titles |
| Sans font (`fontSans`) | Nunito (Google Fonts) | Body text, navigation, labels, stats |
| Hero headline | ~68–72px, Fraunces 700–800 | "Know your rights" main heading |
| Section headline | 32–36px, Fraunces 700 | Section titles |
| Body | 15–16px, Nunito 400–500 | Default body copy |

### Border Radii (Organic System)
| Token | Value | Visual Effect |
|---|---|---|
| `R.o1` | `32px 16px 24px 32px` | Top-left rounded, asymmetric |
| `R.o2` | `16px 32px 32px 24px` | Top-right and bottom-right rounded |
| `R.o3` | `24px 24px 16px 32px` | Mostly rounded, bottom-left sharp |
| `R.o4` | `32px 32px 16px 24px` | Top rounded, bottom-left sharp |

These organic radii are applied to hero containers, stat cards, feature cards, and category cards in rotation to create the flowing organic feel shown in the designs.

### Alternatives Considered
- Replacing CSS variables with Ant Design token overrides — rejected because the CSS variable system gives cross-component consistency without requiring antd ConfigProvider scope everywhere.

---

## 3. Routing & Localisation Strategy

### Decision
Use `next-intl` with the existing `[locale]` App Router segment. All routes are prefixed with the locale (e.g., `/en/`, `/zu/`). The `appRoutes` constants and `createLocalizedPath` helper in `frontend/src/i18n/routing.ts` are used for all internal links.

### Route Table
| Page | Route Pattern | Auth Required | Role Required |
|---|---|---|---|
| Home | `/[locale]/` | No | — |
| Ask | `/[locale]/ask` | No (limited) | — |
| Contracts List | `/[locale]/contracts` | Yes | User |
| Contract Detail | `/[locale]/contracts/[id]` | Yes | User |
| My Rights | `/[locale]/rights` | No | — |
| History | `/[locale]/history` | Yes | User |
| Auth | `/[locale]/auth` | No | — |
| Admin Dashboard | `/[locale]/admin/dashboard` | Yes | Admin |

### Rationale
Public pages (Home, My Rights) are accessible without auth to reduce friction for first-time visitors. The Ask page is public but history and personalisation require auth.

---

## 4. Authentication & Guard Pattern

### Decision
Use the existing `AuthProvider` + `useAuth` hook pattern. Cookie-based JWT (`ml_token`, `ml_user`) as established in feat/018. Route guards are implemented via `useEffect` + `router.push` in each protected page — no middleware-based guards.

### Pattern
```typescript
const { user, isLoading } = useAuth();
useEffect(() => {
  if (isLoading) return;
  if (!user) router.push(createLocalizedPath(locale, appRoutes.auth));
}, [isLoading, user]);
```

### Alternatives Considered
- Next.js Middleware for auth redirection — deferred to a future hardening feature to avoid scope creep.

---

## 5. Component Styling Pattern

### Decision
Use inline `style` objects with design token references for page-level components. For complex, reusable components (chat, dashboard cards), use `antd-style` `createStyles` for scoped CSS-in-JS.

### Rationale
Inline styles with CSS variables give straightforward readability for page shells. `createStyles` avoids className collisions in reusable components. Both patterns are already in use in the codebase.

---

## 6. Pages Requiring Enhancement (vs. New Build)

| Page | Current State | Enhancement Needed |
|---|---|---|
| Home | Fully implemented | Verify organic background layer renders correctly; add missing i18n keys for all 4 languages |
| Ask/Chat | Fully implemented | Verify streaming response display; ensure citation section fully matches design |
| Auth | Fully implemented | Verify tab animation and form validation error display |
| Contract Detail | Fully implemented | Verify caution/standard clauses sections; ensure back-navigation works |
| Contracts List | Minimal (stub) | Add upload UI, contract list items with status badge, empty state |
| My Rights | Fully implemented | Verify progress bar calculation; verify TTS and share button wiring |
| History | Basic empty state | Add real conversation list from backend; styled list items with timestamps |
| Admin Dashboard | Fully implemented | Verify admin-guard redirect; add chart data from backend or mock |

---

## 7. Skills to Apply

| Task | Skill |
|---|---|
| Contracts list page (full implementation) | `add-list-page` |
| Contracts upload modal | `add-modal` |
| History page (list view) | `add-list-page` |
| Dashboard data | `add-service` |
| Auth form refinements | `add-auth-provider` |
| Any new styles | `add-styling` |

---

## 8. i18n Coverage Gap Analysis

### Decision
Audit all four language JSON files for missing keys required by new/updated components. The contracts list page, upload modal, and history list items will need new translation keys added across en, zu, st, and af.

### New Keys Required (estimated)
- `contracts.upload`, `contracts.uploadHint`, `contracts.statusAnalysing`, `contracts.statusComplete`, `contracts.empty`
- `history.conversation`, `history.questionCount`, `history.viewThread`

All keys must be present in all four language files before the feature is considered complete.
