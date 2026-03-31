# Tasks: Auth Pages and Landing Page

**Input**: Design documents from `specs/feat/018-auth-landing-page/`
**Branch**: `feat/018-auth-landing-page`
**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ contracts/ ✅ quickstart.md ✅

**Tests**: Not requested — no test tasks generated.

**Organization**: Tasks grouped by user story (US1–US5) in priority order. Phase 2 (Foundational) must complete before any user story work begins.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no shared dependencies)
- **[Story]**: User story label (US1–US5)
- Tests not included unless requested

---

## Phase 1: Setup (Color Migration)

**Purpose**: Apply the deep teal primary color (`#0d7377`) that replaces olive across the frontend. Must be done before any new UI components are built to ensure consistent theming.

- [x] T001 Update `--ml-primary`, `--ml-primary-strong`, `--ml-primary-fg` CSS variables from olive to deep teal in `frontend/src/styles/globals.css`
- [x] T002 [P] Update `RGB.primary` to `"13, 115, 119"` and recompute `C.primary`, `C.primaryFg`, `C.primaryStrong` in `frontend/src/styles/theme.ts`
- [x] T003 [P] Update `colorPrimary` Ant Design token to `"#0d7377"` in `frontend/src/components/providers/AntdProvider.tsx`
- [x] T004 [P] Replace all `rgba(93,112,82,…)` references with `rgba(13,115,119,…)` in `frontend/src/app/[locale]/page.tsx`

**Checkpoint**: `npm run dev` — app loads, navbar and cards show teal instead of olive.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core auth infrastructure shared by ALL user stories. No user story work can begin until this phase is complete.

**⚠️ CRITICAL**: `AuthProvider` → `useAuth` → all pages. Complete in order T005 → T006 → T007 → T008 → T009.

- [x] T005 Create `frontend/src/services/authService.ts` using `add-service` skill — implement `signIn(credentials: SignInCredentials): Promise<AuthenticateResultModel>` (calls `POST /api/TokenAuth/Authenticate`) and `register(data: RegisterData): Promise<void>` (calls `POST /api/services/app/Account/Register` then auto-calls signIn). Include `decodeJwtPayload(token: string): JwtPayload` helper for client-side role extraction.
- [x] T006 Create `frontend/src/components/providers/AuthProvider.tsx` using `add-auth-provider` skill — implement `AuthContext` with `AuthContextValue` shape from `data-model.md`; on mount read `ml_user` cookie, validate `expiresAt`, restore session or clear; `signIn` action: call authService, decode JWT for role/name, write `ml_token` + `ml_user` cookies, redirect by role; `register` action: call authService.register, merge `preferredLanguage` into `AuthUser` before writing `ml_user` cookie; `signOut` action: clear both cookies via `max-age=0`, reset user to null. Export `AuthContext`.
- [x] T007 Create `frontend/src/hooks/useAuth.ts` — export `useAuth()` hook that calls `useContext(AuthContext)` from `AuthProvider.tsx`; throw if used outside provider.
- [x] T008 Wrap `{children}` in `AuthProvider` inside `frontend/src/app/[locale]/layout.tsx` (place `<AuthProvider>` inside `<AntdProvider>` wrapping `{children}`).
- [x] T009 Create `frontend/src/app/[locale]/auth/page.tsx` — shell page with hash-based tab routing: default tab is `#sign-in`, second tab is `#register`; include tab-switch links ("Don't have an account? Register" / "Already have an account? Sign in"); render `<SignInForm>` or `<RegisterForm>` based on active hash; page is unprotected (no auth guard).
- [x] T010 [P] Add `auth` namespace i18n keys to all four locale files: `frontend/src/messages/en.json`, `frontend/src/messages/zu.json`, `frontend/src/messages/st.json`, `frontend/src/messages/af.json` — keys: `signInTitle`, `registerTitle`, `emailLabel`, `passwordLabel`, `nameLabel`, `languageLabel`, `signInButton`, `registerButton`, `switchToRegister`, `switchToSignIn`, `invalidCredentials`, `emailTaken`, `signOutMenuItem`, `adminDashboardLink`. Translate to each respective language.

**Checkpoint**: App compiles, `AuthProvider` wraps the tree, `/[locale]/auth` route loads, `useAuth()` returns `{ user: null, isLoading: false }` for an unauthenticated visitor.

---

## Phase 3: User Story 1 — New User Registration (Priority: P1) 🎯 MVP

**Goal**: A visitor can register for an account by providing name, email, password, and preferred language — and land on the home page as a signed-in user with their initials in the navbar.

**Independent Test**: Navigate to `/en/auth#register`, complete the form, confirm redirect to home page and user initials visible in navbar. Inspect `ml_user` cookie to confirm `preferredLanguage` is stored.

- [x] T011 [US1] Create `frontend/src/components/auth/RegisterForm.tsx` — pill-shaped (`borderRadius: 9999`) Ant Design `Input` fields for full name, email, password; `Select` component for preferred language with options: English (`en`), isiZulu (`zu`), Sesotho (`st`), Afrikaans (`af`); validation: all fields required, email must be valid format, password min 6 chars; submit calls `register()` from `useAuth()`; display inline error from `emailTaken` i18n key on 400 duplicate email response; use `auth.*` i18n keys for labels and buttons.
- [x] T012 [US1] Wire `<RegisterForm>` into the register tab of `frontend/src/app/[locale]/auth/page.tsx` — on successful registration the `AuthProvider.register()` action handles redirect (to home page for new users, always non-admin).
- [x] T013 [US1] Verify `AuthProvider.register()` in `frontend/src/components/providers/AuthProvider.tsx` correctly merges `preferredLanguage` from `RegisterData` into `AuthUser` before writing `ml_user` cookie (update T006 implementation if needed).

**Checkpoint**: Registration flow fully works end-to-end. `ml_user` cookie contains `preferredLanguage`. Initials appear in navbar after redirect.

---

## Phase 4: User Story 2 — Returning User Login (Priority: P1)

**Goal**: A registered user can sign in with email and password and be redirected to the correct destination based on their role.

**Independent Test**: Navigate to `/en/auth`, enter valid credentials, confirm admin redirects to `/en/admin/dashboard` and regular user redirects to `/en/`. Enter invalid credentials, confirm generic error message shown without revealing which field is wrong.

- [x] T014 [US2] Create `frontend/src/components/auth/SignInForm.tsx` — pill-shaped (`borderRadius: 9999`) Ant Design `Input` and `Input.Password` fields for email/username and password; validation: both fields required; submit calls `signIn()` from `useAuth()`; display `invalidCredentials` i18n key on 401 response; use `auth.*` i18n keys for labels and button.
- [x] T015 [US2] Wire `<SignInForm>` into the sign-in tab of `frontend/src/app/[locale]/auth/page.tsx`.
- [x] T016 [US2] Update the seeded admin account in `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/HostRoleAndUserCreator.cs` — set admin email to `admin@mzansilegal.co.za`; set a strong default password that meets ABP Zero's password strength requirements (uppercase, lowercase, digit, min 8 chars); add comment documenting that this password must be changed post-deployment.

**Checkpoint**: Sign-in works for both regular and admin accounts. Role-based redirect confirmed. Invalid credentials show generic error.

---

## Phase 5: User Story 3 — Protected Route Access Control (Priority: P2)

**Goal**: Unauthenticated visitors cannot access `/contracts` or `/admin/dashboard`; they are redirected to `/auth`. Non-admin authenticated users redirected away from `/admin/dashboard`. Submitting on `/ask` without auth redirects to `/auth`.

**Independent Test**: (1) Clear cookies, navigate directly to `/en/contracts` → expect redirect to `/en/auth`. (2) Navigate to `/en/admin/dashboard` → expect redirect to `/en/auth`. (3) Sign in as regular user, navigate to `/en/admin/dashboard` → expect redirect to home. (4) Visit `/en/ask`, type a question, submit → expect redirect to `/en/auth`.

- [x] T017 Extend `frontend/src/proxy.ts` — add route protection logic before the `next-intl` handler: check for `ml_token` cookie on requests matching `/*/contracts` and `/*/admin/dashboard` patterns; if cookie absent, return `NextResponse.redirect` to `/[locale]/auth`; preserve the `next-intl` locale routing for all other paths.
- [x] T018 [P] [US3] Add in-page auth guard fallback to `frontend/src/app/[locale]/contracts/page.tsx` — use `useAuth()`: if `!user && !isLoading` call `router.push(createLocalizedPath(locale, 'auth'))` and return null; show loading state while `isLoading` is true.
- [x] T019 [P] [US3] Strengthen admin guard in `frontend/src/app/[locale]/admin/dashboard/page.tsx` — use `useAuth()`: if `isLoading` show spinner; if `!user` redirect to `/auth`; if `user && !user.isAdmin` redirect to home; only render dashboard content when `user.isAdmin === true`.
- [x] T020 [US3] Add submit guard to `frontend/src/components/chat/QaChatPage.tsx` — in the submit handler (or `useChat` hook), check `user` from `useAuth()`; if `!user`, call `router.push(createLocalizedPath(locale, 'auth'))` and return early before the API call.

**Checkpoint**: All three protection scenarios confirmed per Independent Test above.

---

## Phase 6: User Story 4 — User Identity Display in Navbar (Priority: P2)

**Goal**: Logged-in users see their initials (or avatar) in the navbar instead of Sign In / Register links. A dropdown provides Sign Out (and Admin Dashboard for admins).

**Independent Test**: Sign in, confirm initials appear in navbar on every page. Click initials, confirm Sign Out option. Sign out, confirm login link returns. Sign in as admin, confirm "Admin Dashboard" link visible in dropdown.

- [x] T021 [US4] Update `frontend/src/components/layout/AppNavbar.tsx` — consume `useAuth()` to read `user`; when signed out: show "Sign In" link → `/[locale]/auth`; when signed in: show user initials avatar (first letter(s) of `user.name`, max 2 chars, uppercase) using Ant Design `Avatar` component; clicking avatar opens a dropdown `Menu` with "Admin Dashboard" item (shown only when `user.isAdmin`) linking to `/[locale]/admin/dashboard`, and "Sign Out" item that calls `signOut()` from `useAuth()`; use `auth.signOutMenuItem` and `auth.adminDashboardLink` i18n keys.

**Checkpoint**: Navbar correctly shows initials when signed in, login link when signed out, admin-only items only visible to admins.

---

## Phase 7: User Story 5 — Landing Page Discovery (Priority: P3)

**Goal**: The home page serves as a landing page with a product description and auth-aware hero CTAs directing visitors to register or access the app.

**Independent Test**: (1) Signed out: visit `/en/` — hero shows "Get Started" button linking to `/en/auth`. (2) Signed in: visit `/en/` — hero shows "Go to App" button linking to `/en/ask`.

- [x] T022 [US5] Update hero section in `frontend/src/app/[locale]/page.tsx` — import `useAuth()` and replace the static search bar CTA or add a primary hero button that adapts: when `!user` render a "Get Started" button linking to `/[locale]/auth` using `t("heroCtaGuest")`; when `user` render a "Go to App" button linking to `/[locale]/ask` using `t("heroCtaUser")`; style as pill-shaped primary button using `C.primary` color tokens.
- [x] T023 [P] [US5] Add `heroCtaGuest` and `heroCtaUser` translation keys to `frontend/src/messages/en.json`, `frontend/src/messages/zu.json`, `frontend/src/messages/st.json`, `frontend/src/messages/af.json` under the `home` namespace.

**Checkpoint**: Auth-aware CTA appears and links to the correct destination in both signed-in and signed-out states. All four locale variants render correctly.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Accessibility, locale completeness, and end-to-end validation.

- [x] T024 [P] Review `frontend/src/components/auth/SignInForm.tsx` and `frontend/src/components/auth/RegisterForm.tsx` for accessibility — verify each input has `aria-label` or associated `<label>`, error messages use `aria-describedby` to link to the relevant input, focus rings are visible, Tab key navigation moves correctly through all fields and the language selector.
- [x] T025 [P] Verify all four locale files (`en.json`, `zu.json`, `st.json`, `af.json`) contain every key defined in `contracts/auth-context.md` under both the `auth` and `home` namespaces — no missing keys.
- [x] T026 Run the full manual validation from `specs/feat/018-auth-landing-page/quickstart.md` — complete all registration, sign-in, route protection, ask page guard, and landing page CTA scenarios; confirm all pass.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately; tasks T001–T004 are all parallelizable
- **Foundational (Phase 2)**: Depends on Phase 1 completing — **BLOCKS all user stories**
  - T005 → T006 → T007 → T008 → T009 must run in order
  - T010 can run in parallel once T005 starts (different files)
- **User Stories (Phase 3–7)**: All depend on Phase 2 completion
- **Polish (Phase 8)**: Depends on all desired user stories completing

### User Story Dependencies

| Story | Depends On | Can Parallelize With |
|---|---|---|
| US1 (Registration) — Phase 3 | Phase 2 complete | US2 (different form component) |
| US2 (Login) — Phase 4 | Phase 2 complete | US1 (different form component) |
| US3 (Route Protection) — Phase 5 | Phase 2 complete | US4 (different files) |
| US4 (Navbar Identity) — Phase 6 | Phase 2 complete | US3, US5 |
| US5 (Landing Page) — Phase 7 | Phase 2 complete; teal (Phase 1) | US3, US4 |

### Within Each Phase

- Models/types → services → components → page integration
- T005 (authService) must complete before T006 (AuthProvider)
- T006 (AuthProvider) must complete before T007 (useAuth)
- T007 must complete before T008 (layout wrapping)
- T008 must complete before any form components can be tested in-browser

---

## Parallel Example: Phase 2 (Foundational)

```text
Sequential chain (cannot parallel):
  T005 authService.ts
    → T006 AuthProvider.tsx
      → T007 useAuth.ts
        → T008 layout.tsx wrapping
          → T009 auth/page.tsx shell

Parallel with the above chain:
  T010 i18n keys (all 4 locale files) — can start any time
```

## Parallel Example: Phase 3 + Phase 4 (both P1)

```text
With two developers after Phase 2 completes:
  Dev A: T011 RegisterForm → T012 wire to auth page → T013 verify cookie
  Dev B: T014 SignInForm → T015 wire to auth page → T016 backend seed update
```

## Parallel Example: Phase 5 + Phase 6 (both P2)

```text
With two developers after Phase 2 completes:
  Dev A: T017 proxy.ts middleware → T018 contracts guard → T019 admin guard → T020 ask guard
  Dev B: T021 AppNavbar update
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 Only)

1. Complete Phase 1: Color migration
2. Complete Phase 2: Foundational auth infrastructure
3. Complete Phase 3: US1 — Registration
4. Complete Phase 4: US2 — Login
5. **STOP and VALIDATE**: Both registration and login flows work end-to-end
6. Deploy/demo: Users can register, login, see initials (navbar update is quick add-on)

### Incremental Delivery

1. Phase 1 + Phase 2 → auth infrastructure ready
2. Phase 3 + Phase 4 → users can register and log in (P1 complete) → Deploy/Demo
3. Phase 5 → protected routes enforced → Deploy/Demo
4. Phase 6 → navbar shows user identity → Deploy/Demo
5. Phase 7 → landing page CTA adapts → Deploy/Demo
6. Phase 8 → polish and validation

---

## Notes

- [P] tasks = different files, no shared dependencies in that phase
- [Story] label maps each task to its user story for traceability
- `add-auth-provider` and `add-service` skills MUST be used for T006 and T005 respectively
- `follow-git-workflow` skill applies for branch management throughout
- No test tasks generated (not requested in spec)
- Commit after each phase checkpoint to preserve working state
- T016 (backend seed) requires backend `dotnet run` restart to take effect
