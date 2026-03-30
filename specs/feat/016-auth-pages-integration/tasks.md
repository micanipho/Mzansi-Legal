# Tasks: Auth, Roles & Landing Page

**Input**: Design documents from `specs/feat/016-auth-pages-integration/`
**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ contracts/ ✅

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to

---

## Phase 1: Setup — Color System & Backend Seed

**Purpose**: Replace the olive color globally and update the admin seed before any new UI is built. All new components inherit the correct color automatically.

- [ ] T001 Update `--ml-primary` to `#0d7377`, `--ml-primary-strong` to `#0a5e62`, `--ml-primary-fg` to `#f0fafa` in `frontend/src/styles/globals.css`
- [ ] T002 [P] Update `RGB.primary` to `"13, 115, 119"` and recompute `C.primary`, `C.primaryFg`, `C.primaryStrong` in `frontend/src/styles/theme.ts`
- [ ] T003 [P] Update `colorPrimary` Ant Design token to `"#0d7377"` in `frontend/src/components/providers/AntdProvider.tsx`
- [ ] T004 [P] Replace all hardcoded `rgba(93,112,82,…)` occurrences with `rgba(13,115,119,…)` in `frontend/src/app/[locale]/page.tsx`
- [ ] T005 [P] Update `HostRoleAndUserCreator.cs` — change the seeded admin email from `admin@aspnetboilerplate.com` to `admin@mzansilegal.co.za` and update the default password to a stronger value (document the new credentials in `backend/src/backend.Web.Host/appsettings.json` under a `Seed:AdminPassword` config key, NOT hardcoded) in `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/HostRoleAndUserCreator.cs`

**Checkpoint**: Start dev server — landing page renders teal. Backend migrator seeds admin user with updated email.

---

## Phase 2: Foundational — Auth Infrastructure

**Purpose**: Core auth service, context, cookie write, and middleware. ALL user stories depend on this phase.

**⚠️ CRITICAL**: No auth user story work can begin until this phase is complete.

- [ ] T006 Create `frontend/src/services/authService.ts` using `add-service` skill — export `authenticate(credentials: SignInCredentials): Promise<AuthenticateResult>` (POST `/api/TokenAuth/Authenticate`) and `registerUser(data: RegisterData): Promise<{ canLogin: boolean }>` (POST `/api/services/app/Account/Register`); read base URL from `NEXT_PUBLIC_BASE_URL`
- [ ] T007 [P] Create `frontend/src/hooks/useAuth.ts` — export `useAuth()` hook that reads `AuthContext` and throws a descriptive error if used outside `AuthProvider`
- [ ] T008 Create `frontend/src/components/providers/AuthProvider.tsx` using `add-auth-provider` skill — implement `AuthContext` with `{ user: AuthUser | null, isLoading: boolean, signIn(), register(), signOut() }`; on mount read `ml_user` cookie (via `document.cookie` parse), validate `expiresAt`, restore session or clear stale cookies; include JWT base64url decode helper to extract `isAdmin` from `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` claim; **no localStorage usage**
- [ ] T009 Implement cookie write/clear helpers inside `AuthProvider` — on `signIn`/`register` write two cookies: `ml_token` (raw JWT, `path=/`, `SameSite=Lax`, `max-age=${expireInSeconds}`) and `ml_user` (JSON of `AuthUser` with token field omitted, same attributes); on `signOut` clear both by setting `max-age=0`; read `ml_token` from `document.cookie` when attaching Bearer headers to API calls
- [ ] T010 Extend `frontend/src/proxy.ts` (the existing next-intl middleware) — after the `/chat` redirect logic, check if the request path matches `/(en|zu|st|af)/contracts` or `/(en|zu|st|af)/admin` patterns AND the `ml_token` cookie is absent; if so, redirect to `/${locale}/auth?from=${encodedPath}`; leave all other routes unaffected
- [ ] T011 Wire `AuthProvider` into `frontend/src/app/[locale]/layout.tsx` wrapping `{children}` after `<AntdProvider>`

**Checkpoint**: Visit `/en/contracts` without a session cookie → redirected to `/en/auth`. No visual change on unprotected pages.

---

## Phase 3: User Story 1 — Sign In (Priority: P1) 🎯 MVP

**Goal**: A returning user can sign in, get a session stored in localStorage + cookie, and be redirected to their role-appropriate destination.

**Independent Test**: Navigate to `/en/auth`, enter admin credentials → redirected to `/en/admin/dashboard`. Enter regular user credentials → redirected to `/en/ask`. Check DevTools → Application → Cookies: `ml_token` and `ml_user` present. No localStorage entries written.

- [ ] T012 [US1] Create `frontend/src/app/[locale]/auth/page.tsx` — page shell using `page-shell page-shell--compact` CSS class, `OrganicBackground`, Fraunces serif heading using `auth.loginTitle` i18n key; manage tab state (`"signin" | "register"`) initialised from URL hash; redirect to home if `user !== null` on mount
- [ ] T013 [US1] Create `frontend/src/components/auth/SignInForm.tsx` — email/username field (`auth.emailLabel`) and password field (`auth.passwordLabel`) both with pill-shaped border-radius (`9999px`); submit button (`auth.loginButton`) using `C.primary` background; loading state disables button; calls `signIn()` from `useAuth()`; displays backend error message inline below the form without clearing the email field
- [ ] T014 [US1] Update `AuthProvider.signIn()` redirect — after storing session, call `router.push` to `createLocalizedPath(locale, 'admin/dashboard')` if `user.isAdmin`, else `createLocalizedPath(locale, appRoutes.ask)`; also handle `?from=` return URL if present in the current URL
- [ ] T015 [US1] Render `<SignInForm />` in the `"signin"` tab of `frontend/src/app/[locale]/auth/page.tsx`; wire toggle link using `auth.switchToRegister` i18n key

**Checkpoint**: Sign in with valid credentials → `ml_token` + `ml_user` cookies set, redirected to correct destination. Wrong password → inline error. Already signed in → redirected away from auth page. DevTools → Application → Cookies confirms no localStorage entries.

---

## Phase 4: User Story 2 — Registration (Priority: P2)

**Goal**: A new user can register with all required fields including a language preference, and be automatically signed in afterwards.

**Independent Test**: Complete the registration form on `/en/auth#register` → auto sign-in → redirect to `/en/ask`. Check `ml_access_token` exists. Duplicate username → field error shown.

- [ ] T016 [US2] Add missing i18n keys to all four locale files (`en.json`, `zu.json`, `st.json`, `af.json`): `auth.firstNameLabel`, `auth.surnameLabel`, `auth.userNameLabel`, `auth.languageLabel` — with appropriate translations per locale
- [ ] T017 [US2] Create `frontend/src/components/auth/RegisterForm.tsx` — pill-shaped fields for: first name (`auth.firstNameLabel`), surname (`auth.surnameLabel`), username (`auth.userNameLabel`), email (`auth.emailLabel`), password (`auth.passwordLabel`); Ant Design `Select` component for preferred language (`auth.languageLabel`) with options en/zu/st/af; submit button (`auth.registerButton`) with loading state; calls `register()` from `useAuth()`; inline error display
- [ ] T018 [US2] Update `AuthProvider.register()` — call `authService.registerUser()`, then if `canLogin === true` automatically call `authService.authenticate()` with the same credentials; decode JWT, store session (localStorage + cookie), update state; after auto sign-in apply the selected language by calling `router.replace(createLocalizedPath(selectedLocale, appRoutes.ask))`
- [ ] T019 [US2] Render `<RegisterForm />` in the `"register"` tab of `frontend/src/app/[locale]/auth/page.tsx`; wire toggle link using `auth.switchToLogin` i18n key

**Checkpoint**: Fill all registration fields including language → submit → auto sign-in → redirect to ask page in selected locale. Duplicate username → field error.

---

## Phase 5: User Story 3 — Role-Based Routing & Protected Routes (Priority: P2)

**Goal**: Admins land on the admin dashboard. Regular users land on ask. Protected routes (`/contracts`, `/admin/dashboard`) block unauthenticated access. `/ask` shows a page but blocks submission.

**Independent Test**: Sign in as admin → lands on `/admin/dashboard`. Sign in as user → lands on `/ask`. Visit `/en/contracts` without session → redirected to `/en/auth`. Visit `/en/ask` without session → page loads; type a question → redirected to `/en/auth`.

- [ ] T020 [US3] Add Admin nav link in `frontend/src/components/layout/AppNavbar.tsx` — read `useAuth()`, render an "Admin" link to `/[locale]/admin/dashboard` only when `user?.isAdmin === true`
- [ ] T021 [US3] Strengthen in-page admin guard in `frontend/src/app/[locale]/admin/dashboard/page.tsx` — if `isLoading` show spinner; if `!user?.isAdmin` show "Access Denied" message and redirect to home page after 3 seconds (middleware is the primary guard; this is the fallback)
- [ ] T022 [US3] Add in-page auth guard to `frontend/src/app/[locale]/contracts/page.tsx` — if `isLoading` show spinner; if `!user` redirect to `/[locale]/auth?from=/[locale]/contracts` (middleware is primary; page guard is fallback for stale cookie edge cases)
- [ ] T023 [US3] Update `frontend/src/components/chat/QaChatPage.tsx` — intercept the `sendMessage` call inside the submit handler; if `user === null`, call `router.push(createLocalizedPath(locale, 'auth'))` instead of sending the request; show a brief "Sign in to ask a question" prompt before redirecting

**Checkpoint**: Protected routes are inaccessible without session. `/ask` renders freely but submitting redirects to auth. Admin dashboard inaccessible to regular users.

---

## Phase 6: User Story 4 — Landing Page Redesign (Priority: P2)

**Goal**: The root URL shows the redesigned landing page with teal color and an auth-aware hero CTA.

**Independent Test**: Visit `/en` while signed out → teal colors throughout, hero CTA reads "Get Started" linking to `/en/auth`. Sign in and return → CTA reads "Go to App" linking to `/en/ask`.

- [ ] T024 [US4] Add `home.heroCtaGuest` and `home.heroCtaUser` i18n keys to all four locale files (`en.json`, `zu.json`, `st.json`, `af.json`) with appropriate translations
- [ ] T025 [US4] Update hero CTA in `frontend/src/app/[locale]/page.tsx` — use `useAuth()` to render: signed-out → button labelled `t("heroCtaGuest")` linking to `/[locale]/auth`; signed-in → button labelled `t("heroCtaUser")` linking to `appRoutes.ask`
- [ ] T026 [US4] Update `frontend/src/app/[locale]/history/page.tsx` — replace the plain-string `/${locale}/auth` sign-in link with `createLocalizedPath(locale, 'auth')` using the routing helper

**Checkpoint**: No olive color visible. Hero CTA changes based on sign-in state. History sign-in link navigates to auth page correctly.

---

## Phase 7: User Story 5 — Sign Out & Navbar Avatar (Priority: P3)

**Goal**: Signed-in users see an avatar with their initials in the navbar and can sign out in one click, clearing all session data.

**Independent Test**: Sign in → navbar shows circular avatar with correct initials. Click "Sign Out" → navbar reverts to "Sign In" link. LocalStorage and `ml_token` cookie both cleared.

- [ ] T027 [US5] Implement `AuthProvider.signOut()` — clear `ml_token` and `ml_user` cookies by setting `max-age=0`; reset `user` state to `null`; no localStorage to touch
- [ ] T028 [US5] Update `frontend/src/components/layout/AppNavbar.tsx` to render auth-aware navbar state:
  - **Signed out**: "Sign In" link → `/[locale]/auth`
  - **Signed in**: circular avatar (40×40 px, teal background, white text, `border-radius: 9999px`) showing user initials (first letter of `user.name` + first letter of `user.surname`); clicking the avatar shows a dropdown with the user's full name and a "Sign Out" option that calls `signOut()`; also shows the "Admin" link (from T020) when `user.isAdmin`

**Checkpoint**: Sign in → avatar with initials visible in navbar. Click avatar → dropdown with name + Sign Out. Click Sign Out → `ml_token` and `ml_user` cookies gone (DevTools → Cookies), navbar reverts to "Sign In". Browser back → protected routes redirect to auth.

---

## Phase 8: User Story 6 — i18n for Auth (Priority: P3)

**Goal**: All auth page text renders correctly in Zulu, Sesotho, and Afrikaans with no untranslated keys visible.

**Independent Test**: Switch locale to `/zu/auth` → all labels, placeholders, and button text display in Zulu. Repeat for `/st/auth` and `/af/auth`. No `[missing: auth.*]` placeholders.

- [ ] T029 [P] [US6] Complete `auth.*` key translations in `frontend/src/messages/zu.json` — verify `firstNameLabel`, `surnameLabel`, `userNameLabel`, `languageLabel` (added in T016) are correctly translated in Zulu
- [ ] T030 [P] [US6] Complete `auth.*` key translations in `frontend/src/messages/st.json` — same as T029 for Sesotho
- [ ] T031 [P] [US6] Complete `auth.*` key translations in `frontend/src/messages/af.json` — same as T029 for Afrikaans
- [ ] T032 [US6] Verify `SignInForm.tsx` and `RegisterForm.tsx` use `useTranslations('auth')` for all visible strings — no hardcoded English labels, placeholders, or error messages

**Checkpoint**: Switch locale to each of zu/st/af while on `/auth` → all text updates correctly.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, a11y, cleanup, and branch housekeeping.

- [ ] T033 [P] Grep for remaining hardcoded olive colors (`#5d7052`, `rgba(93,112,82`, `rgba(93, 112, 82`) in `frontend/src/` — replace any found with teal equivalents (`#0d7377`, `rgba(13,115,119`)
- [ ] T034 Validate keyboard accessibility on the auth page: tab order through fields is logical, Enter submits the active form, tab toggle (signin ↔ register) is keyboard-operable, error messages linked via `aria-describedby`
- [ ] T035 Add `autoComplete` attributes to all auth form fields in `SignInForm.tsx` and `RegisterForm.tsx` — `username`, `current-password`, `new-password`, `given-name`, `family-name`, `email` — for browser autofill support
- [ ] T036 Verify the `?from=` return URL is respected after sign-in — navigate to `/en/contracts` while signed out, sign in, verify redirect back to `/en/contracts` rather than the default destination
- [ ] T037 [P] Delete `feat/017-roles-landing-page` branch: `git branch -d feat/017-roles-landing-page`
- [ ] T038 Run all scenarios from `specs/feat/016-auth-pages-integration/quickstart.md` and confirm all pass

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — T002/T003/T004/T005 all parallel with T001
- **Phase 2 (Foundational)**: Depends on Phase 1. T007 parallel with T008; T009 after T008; T010 after T009; T011 after T008
- **Phases 3–8 (User Stories)**: All depend on Phase 2 completion
- **Phase 9 (Polish)**: Depends on all desired stories complete

### User Story Dependencies

| Story | Depends on | Notes |
|---|---|---|
| US1 (P1) Sign In | Phase 2 only | Start here after foundation |
| US2 (P2) Register | US1 (shares auth page shell) | Tab added to existing page |
| US3 (P2) Role Routing | US1 (extends signIn action from T014) | T022/T023 independent of US1 |
| US4 (P2) Landing Page | Phase 2 only | Fully independent of US1/US2/US3 |
| US5 (P3) Sign Out | US1 (extends AuthProvider) | T028 extends navbar started in T020 |
| US6 (P3) i18n | US1+US2 (forms must exist) | T029–T031 fully parallel |

### Parallel Opportunities

```
Phase 1:   T001 │ T002 │ T003 │ T004 │ T005   (all in parallel)
Phase 2:   T006 │ T007 │ T008  →  T009  →  T010  →  T011
Phase 8:   T029 │ T030 │ T031                (all locale files in parallel)
Phase 9:   T033 │ T037                        (grep + branch delete in parallel)
```

---

## Implementation Strategy

### MVP (User Story 1 only — T001–T015)

1. Phase 1: Color + seed (T001–T005)
2. Phase 2: Auth infrastructure (T006–T011)
3. Phase 3: Sign In (T012–T015)
4. **STOP AND VALIDATE**: Sign in works, session stored, role-based redirect fires
5. Deployable MVP — users can authenticate

### Incremental Delivery

| Milestone | Tasks | What works |
|---|---|---|
| **Auth MVP** | T001–T015 | Sign-in, session, role redirect |
| **Full Auth** | + T016–T019 | Registration + language selection |
| **Protected Routes** | + T020–T023 | Contracts/admin guarded, ask intercepts |
| **Landing** | + T024–T026 | Teal landing page, auth-aware CTA |
| **Complete Loop** | + T027–T028 | Avatar, sign-out |
| **i18n Complete** | + T029–T032 | All 4 locales for auth |
| **PR Ready** | + T033–T038 | Polish, a11y, cleanup |

---

## Notes

- [P] tasks = different files, no in-phase dependencies — safe to run simultaneously
- Skills to invoke: `add-auth-provider` (T008), `add-service` (T006), `add-styling` (T001–T004)
- Admin seed (`HostRoleAndUserCreator.cs`) already exists — T005 is a targeted update only
- `proxy.ts` is the existing middleware (not `middleware.ts`) — extend it, do not replace it
- Commit after each checkpoint to keep rollback safe
- No new npm packages required — everything uses existing Ant Design + Next.js primitives
