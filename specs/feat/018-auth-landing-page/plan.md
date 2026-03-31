# Implementation Plan: Auth Pages and Landing Page

**Branch**: `feat/018-auth-landing-page` | **Date**: 2026-03-31 | **Spec**: [spec.md](../../feat-018-auth-landing-page/spec.md)
**Input**: Full authentication experience (sign-in, registration with language preference, role-based routing, landing page with auth-aware CTAs, admin seeding)

## Summary

Build the complete authentication experience (sign-in, registration with preferred language selector, sign-out) wired to the existing ABP Zero JWT backend, add role-based post-login routing (admin → admin dashboard, user → home), add middleware-based route protection for `/contracts` and `/admin/dashboard`, update the home page as an auth-aware landing page, and ensure an admin account is seeded during initial deployment. No new backend entities are required — all auth API endpoints already exist in ABP Zero.

> **Relationship to feat/016**: This feature supersedes `feat/016-auth-pages-integration`. The 016 plan and research are authoritative; this plan adopts all 016 decisions and adds one delta: the registration form includes a preferred language selector (`preferredLanguage`), which maps to `AppUser.PreferredLanguage` (an existing field from feat/006).

## Technical Context

**Language/Version**: TypeScript / Next.js 16.2 (frontend); C# / .NET 9 + ABP Zero (backend — seed update only)
**Primary Dependencies**: Ant Design 6.x, next-intl 4.x, lucide-react (existing); no new npm packages required
**Storage**: JWT token in cookies (`ml_token`, `ml_user`); no localStorage; no new DB tables
**Testing**: Manual browser testing for all auth flows; Swagger for backend endpoint verification
**Target Platform**: Web (desktop + mobile responsive)
**Project Type**: Web application (Next.js frontend + ABP Zero backend)
**Performance Goals**: Auth page load < 1 s; sign-in round-trip < 2 s; protected route redirect < 200 ms (no content flash)
**Constraints**: Minimal backend changes (seed update + preferred language persisted via existing AppUser field); no new npm packages; all four locales (en, zu, st, af)
**Scale/Scope**: Single-tenant; no breaking changes to existing unprotected pages

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

- [x] **Layer Gate**: No new backend entities. The one backend change (admin seed update) stays in `EntityFrameworkCore/Seed/Host/HostRoleAndUserCreator.cs` — the correct Infrastructure layer. Frontend changes are all within `frontend/src/`.
- [x] **Naming Gate**: No new backend services or DTOs. Frontend files follow Next.js file-based routing and project naming conventions (`authService.ts`, `AuthProvider.tsx`, `useAuth.ts`, `SignInForm.tsx`, `RegisterForm.tsx`).
- [x] **Coding Standards Gate**: All frontend code uses TypeScript; no hardcoded colors; BP.md Next.js guidelines applied (file-based routing, Next.js Image for any images, SSR only where needed). The one C# change is a seed update with no new logic.
- [x] **Skill Gate**: `add-auth-provider` (AuthProvider context), `add-service` (authService.ts), `add-styling` (landing page color tokens if teal not yet applied), `follow-git-workflow` identified.
- [x] **Multilingual Gate**: Auth pages use `auth.*` translation keys across all four locales. Landing page uses existing `home.*` keys with two new auth-aware CTA keys (`heroCtaGuest`, `heroCtaUser`). Preferred language selector options are rendered from a locale-aware list.
- [x] **Citation Gate**: No AI-facing endpoints introduced in this feature.
- [x] **Accessibility Gate**: Auth forms include `aria-label`, `aria-describedby` for error messages, keyboard-navigable inputs with visible focus rings. Language selector is keyboard accessible. Landing page maintains existing skip-link and semantic HTML structure.
- [x] **ETL/Ingestion Gate**: No document ingestion changes in this feature.

## Project Structure

### Documentation (this feature)

```text
specs/feat/018-auth-landing-page/
├── plan.md              ← this file
├── research.md          ← Phase 0 output
├── data-model.md        ← Phase 1 output
├── quickstart.md        ← Phase 1 output
├── contracts/
│   ├── auth-api.md      ← Phase 1 output
│   └── auth-context.md  ← Phase 1 output
└── tasks.md             ← Phase 2 output (/speckit.tasks — NOT created here)
```

### Source Code (repository root)

```text
frontend/src/
├── proxy.ts                                ← MODIFY: extend next-intl middleware to guard /contracts and /admin/dashboard (read ml_token cookie)
├── app/
│   └── [locale]/
│       ├── page.tsx                        ← MODIFY: auth-aware hero CTA (Get Started → /auth when signed out; Go to App → /ask when signed in); update olive rgba to teal rgba
│       ├── auth/
│       │   └── page.tsx                    ← NEW: combined sign-in / register page (hash-routed tabs #sign-in / #register)
│       ├── contracts/
│       │   └── page.tsx                    ← MODIFY: add in-page auth guard fallback (middleware is primary)
│       ├── ask/
│       │   └── page.tsx                    ← MODIFY: QaChatPage intercept — redirect to /auth on submit if not signed in
│       └── admin/
│           └── dashboard/
│               └── page.tsx                ← MODIFY: strengthen in-page admin guard (isAdmin check → redirect to home)
├── components/
│   ├── auth/
│   │   ├── SignInForm.tsx                  ← NEW: pill-shaped email + password form with error state
│   │   └── RegisterForm.tsx               ← NEW: pill-shaped fields + preferred language selector
│   ├── layout/
│   │   └── AppNavbar.tsx                  ← MODIFY: avatar/initials + sign-out dropdown; Admin link for admins only
│   └── providers/
│       ├── AuthProvider.tsx               ← NEW (add-auth-provider skill): context, token, role, cookie write, sign-out, preferred language
│       └── AntdProvider.tsx               ← no change (teal token already applied or unchanged)
├── services/
│   └── authService.ts                     ← NEW (add-service skill): signIn(), register() with preferredLanguage, token decode
├── hooks/
│   └── useAuth.ts                         ← NEW: convenience hook consuming AuthContext
└── styles/
    ├── globals.css                         ← MODIFY if not already updated: --ml-primary → deep teal (#0d7377)
    └── theme.ts                            ← MODIFY if not already updated: RGB.primary → "13, 115, 119"

backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/
└── HostRoleAndUserCreator.cs              ← MODIFY: update seeded admin email/password; ensure password meets ABP strength requirements
```

## Complexity Tracking

No constitution violations — this feature is purely additive on the frontend with a single backend seed update.
