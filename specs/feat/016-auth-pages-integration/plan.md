# Implementation Plan: Auth, Roles & Landing Page

**Branch**: `feat/016-auth-pages-integration` | **Date**: 2026-03-30 | **Spec**: [spec.md](../../feat-016-auth-pages-integration/spec.md)
**Input**: Feature specification — auth pages, role-based routing, landing page redesign with new color

## Summary

Build the complete authentication experience (sign-in, registration, sign-out) wired to the existing ABP Zero JWT backend, add role-based post-login routing (admin → admin dashboard, user → home), and redesign the current home page as a dedicated landing page with the new deep-teal primary color replacing olive globally. No new backend entities are required — all auth endpoints already exist.

## Technical Context

**Language/Version**: TypeScript / Next.js 16.2 (frontend); C# / .NET 9 + ABP Zero (backend — one seed change)
**Primary Dependencies**: Ant Design 6.x, next-intl 4.x, lucide-react (existing); no new npm packages required
**Storage**: JWT token stored in cookies only (`ml_token` — JS-readable, `SameSite=Lax`); no localStorage; no new DB tables
**Testing**: Manual browser testing for all flows; Swagger for backend endpoint verification
**Target Platform**: Web (desktop + mobile responsive)
**Project Type**: Web application (Next.js frontend + ABP Zero backend)
**Performance Goals**: Auth page load < 1 s; sign-in round-trip < 2 s; protected route redirect < 1 s (no content flash)
**Constraints**: Minimal backend changes (seed update only); no new npm packages; all four locales (en, zu, st, af)
**Scale/Scope**: Single-tenant; no breaking changes to existing unprotected pages

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

- [x] **Layer Gate**: No new backend entities. The one backend change (admin email/seed update) stays in `EntityFrameworkCore/Seed/Host/HostRoleAndUserCreator.cs` — the correct Infrastructure layer. Frontend changes are all within `frontend/src/`.
- [x] **Naming Gate**: No new services or DTOs. Frontend files follow file-based routing and existing naming conventions (`authService.ts`, `AuthProvider.tsx`, `useAuth.ts`).
- [x] **Coding Standards Gate**: All frontend code uses TypeScript; no hardcoded colors; BP.md Next.js guidelines applied. The one C# change is a seed-data update with no new logic.
- [x] **Skill Gate**: `add-auth-provider` (auth context), `add-service` (auth API service), `add-styling` (new color tokens), `follow-git-workflow` identified.
- [x] **Multilingual Gate**: Auth page uses existing `auth.*` translation keys. Landing page uses existing `home.*` keys. All four locales covered.
- [x] **Citation Gate**: No AI-facing endpoints introduced in this feature.
- [x] **Accessibility Gate**: Auth forms will include `aria-label`, `aria-describedby` for errors, keyboard-navigable tab switch, and visible focus rings. Landing page maintains existing skip-link and semantic HTML.
- [x] **ETL/Ingestion Gate**: No document ingestion changes in this feature.

## Project Structure

### Documentation (this feature)

```text
specs/feat/016-auth-pages-integration/
├── plan.md              ← this file
├── research.md          ← Phase 0 output
├── data-model.md        ← Phase 1 output
├── quickstart.md        ← Phase 1 output
├── contracts/           ← Phase 1 output
│   ├── auth-api.md
│   └── auth-context.md
└── tasks.md             ← Phase 2 output (/speckit.tasks — NOT created here)
```

### Source Code (repository root)

```text
frontend/src/
├── proxy.ts                                ← MODIFY: extend existing next-intl middleware to guard /contracts and /admin/dashboard (read ml_token cookie)
├── app/
│   └── [locale]/
│       ├── page.tsx                        ← MODIFY: redesigned landing page (new color, auth-aware CTA)
│       ├── auth/
│       │   └── page.tsx                    ← NEW: combined sign-in / register auth page
│       ├── contracts/
│       │   └── page.tsx                    ← MODIFY: add in-page auth guard (middleware is primary; page guard is fallback)
│       ├── ask/
│       │   └── page.tsx                    ← no change (QaChatPage handles auth check)
│       └── admin/
│           └── dashboard/
│               └── page.tsx                ← MODIFY: strengthen in-page admin guard
├── components/
│   ├── auth/
│   │   ├── SignInForm.tsx                  ← NEW: pill-shaped email + password form
│   │   └── RegisterForm.tsx               ← NEW: pill-shaped fields + language selector
│   ├── layout/
│   │   └── AppNavbar.tsx                  ← MODIFY: avatar/initials + sign-out dropdown; Admin link for admins
│   └── providers/
│       ├── AuthProvider.tsx               ← NEW (add-auth-provider skill): context, token, role, cookie write, sign-out
│       └── AntdProvider.tsx               ← MODIFY: update colorPrimary token to deep teal
├── components/chat/
│   └── QaChatPage.tsx                     ← MODIFY: intercept submit, redirect to /auth if not signed in
├── services/
│   └── authService.ts                     ← NEW (add-service skill): authenticate() and register()
├── hooks/
│   └── useAuth.ts                         ← NEW: convenience hook consuming AuthContext
└── styles/
    ├── globals.css                         ← MODIFY: --ml-primary → deep teal (#0d7377)
    └── theme.ts                            ← MODIFY: RGB.primary → "13, 115, 119"

backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/
└── HostRoleAndUserCreator.cs              ← MODIFY: update seeded admin email to admin@mzansilegal.co.za; ensure password meets strength requirements
```

## Complexity Tracking

No constitution violations — this feature is purely additive on the frontend side.
