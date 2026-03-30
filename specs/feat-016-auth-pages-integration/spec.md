# Feature Specification: Auth, Roles & Landing Page

**Feature Branch**: `feat/016-auth-pages-integration`
**Created**: 2026-03-30
**Status**: Draft
**Input**: User description: "I want to create auth pages and the backend integration, follow the current design and maybe experiment with a different color cause i dont like the olive color — also remember the roles, and add a landing page extracted from home (redesigning it)"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Sign In to the Application (Priority: P1)

A returning user arrives at the landing page and signs in with their email/username and password to access their personalised experience.

**Why this priority**: Login is the critical path — all other auth and role-based flows depend on it.

**Independent Test**: Can be fully tested by navigating to the auth page, submitting valid credentials, and verifying the user lands on the correct destination based on their role.

**Acceptance Scenarios**:

1. **Given** a user is on the auth page, **When** they enter valid credentials and click Sign In, **Then** they are authenticated, their session token and role are stored, and they are redirected to the appropriate destination (admin dashboard for admins, home/ask page for regular users).
2. **Given** a user submits incorrect credentials, **When** the backend rejects the request, **Then** a clear inline error message is shown without clearing the email field.
3. **Given** a user is already signed in, **When** they navigate to the auth page, **Then** they are automatically redirected based on their role.
4. **Given** a user's session expires, **When** they attempt to access a protected area, **Then** they are redirected to the auth page.

---

### User Story 2 - Create a New Account (Priority: P2)

A new user arrives at the application and wants to register for an account by providing their name, surname, username, email address, and a password.

**Why this priority**: Registration enables new users to onboard; without it the app only serves existing accounts.

**Independent Test**: Can be fully tested by completing and submitting the registration form; delivers a new account, automatic sign-in, and redirect to the home page.

**Acceptance Scenarios**:

1. **Given** a new user fills in all required fields correctly, **When** they submit the registration form, **Then** their account is created (assigned the default "User" role), they are automatically signed in, and redirected to the home page.
2. **Given** a user submits a username or email that is already taken, **When** the backend returns an error, **Then** a specific field-level error is shown indicating the conflict.
3. **Given** a user submits a form with a missing required field, **When** validation runs, **Then** the specific field is highlighted with an error and submission is prevented.
4. **Given** a user is on the sign-in view, **When** they click "Don't have an account? Register", **Then** the form switches to registration mode without a full page reload.

---

### User Story 3 - Role-Based Redirect After Sign In (Priority: P2)

After a successful sign-in, the system routes each user to the destination that matches their role — admins go to the admin dashboard, and regular users go to the main ask/home page.

**Why this priority**: Without role-based routing, admins land in the wrong place and must navigate manually; this undermines the admin experience and makes the role system invisible to users.

**Independent Test**: Can be fully tested by signing in with an admin account and verifying the redirect to `/[locale]/admin/dashboard`, then repeating with a regular user account and verifying redirect to the home page.

**Acceptance Scenarios**:

1. **Given** a signed-in admin, **When** sign-in completes, **Then** they are redirected to the admin dashboard.
2. **Given** a signed-in regular user, **When** sign-in completes, **Then** they are redirected to the home/ask page.
3. **Given** a regular user, **When** they manually navigate to `/[locale]/admin/dashboard`, **Then** they are redirected away with an "access denied" message.
4. **Given** an admin, **When** they view the navigation bar, **Then** they see an "Admin" link in addition to the standard navigation items.

---

### User Story 4 - Redesigned Landing Page (Priority: P2)

A visitor (signed-out or first-time) arrives at the root URL and sees a purpose-built landing page that clearly communicates the value of the product, shows the legal categories, trending questions, and stats, and prompts them to sign in or explore the app.

**Why this priority**: The current home page serves as both a landing page and the authenticated home simultaneously; separating them allows the landing page to be optimised for conversion and the in-app home to be optimised for productivity.

**Independent Test**: Can be fully tested by visiting the root URL as an unauthenticated visitor; the redesigned landing page renders with the new color scheme and clear calls-to-action to sign in or ask a question.

**Acceptance Scenarios**:

1. **Given** an unauthenticated visitor, **When** they land on the root URL, **Then** they see the redesigned landing page with hero, stats, legal categories, trending questions, and sign-in/try-it CTAs.
2. **Given** a signed-in user, **When** they navigate to the root URL, **Then** the landing page still renders but the CTA changes to reflect their signed-in state (e.g., "Go to app" instead of "Sign In").
3. **Given** any visitor, **When** they view the landing page, **Then** the new primary color (not olive) is used consistently throughout the page.
4. **Given** a visitor on mobile, **When** they view the landing page, **Then** all sections are fully responsive and the hero search bar remains usable.
5. **Given** a visitor, **When** they click a trending question on the landing page, **Then** they are navigated to the ask page with the question pre-filled.

---

### User Story 5 - Sign Out (Priority: P3)

A signed-in user wants to end their session and clear their credentials from the device.

**Why this priority**: Sign-out is a basic security requirement.

**Independent Test**: Can be tested by clicking Sign Out from the navigation; session is cleared and the user lands on the landing page.

**Acceptance Scenarios**:

1. **Given** a user is signed in, **When** they click Sign Out in the navigation, **Then** their session token and role are cleared and they are redirected to the landing page.
2. **Given** a user signs out, **When** they press the browser back button, **Then** they cannot access any previously protected view without signing in again.

---

### User Story 6 - Seamless Language Support for Auth (Priority: P3)

A user whose preferred language is Zulu, Sesotho, or Afrikaans arrives at the auth page and sees all labels, placeholders, error messages, and button text in their chosen language.

**Why this priority**: Multilingual support is a core product value and the translation keys for auth already exist.

**Independent Test**: Switch the locale to `zu`, `st`, or `af` then visit the auth page; all text must render in the selected language with no untranslated keys.

**Acceptance Scenarios**:

1. **Given** the app locale is set to Zulu, **When** a user views the auth page, **Then** all auth text (labels, buttons, error messages, toggle link) is displayed in Zulu.
2. **Given** the app locale changes via the language switcher, **When** the user is on the auth page, **Then** the page text updates immediately without requiring a sign-out.

---

### Edge Cases

- What happens when the backend is unreachable during login? — Show a network error message; do not clear the form.
- What happens if the user submits the form multiple times rapidly? — Disable the submit button during the in-flight request.
- What happens when the auth token is malformed or rejected on a subsequent API call? — Clear the stored token and role, then redirect to the auth page.
- What happens when a user navigates between login and register tabs with partially filled fields? — Each tab retains its own independent form state.
- What happens if a user's role cannot be determined after sign-in? — Default to the regular "User" role and redirect to the home page; log the anomaly.

## Requirements *(mandatory)*

### Functional Requirements

**Auth**

- **FR-001**: The application MUST provide a single auth page at `/[locale]/auth` containing both sign-in and registration flows toggled via a tab or link.
- **FR-002**: The sign-in form MUST accept a username or email address and a password, and submit them to the backend authentication endpoint.
- **FR-003**: On successful sign-in, the system MUST store the returned session token and the user's role in browser storage and apply the token as a Bearer token on all subsequent API requests.
- **FR-004**: The registration form MUST collect first name, surname, username, email address, and password, and submit them to the backend registration endpoint.
- **FR-005**: On successful registration, the system MUST automatically sign the user in (with the "User" role) and redirect them to the home page.
- **FR-006**: The system MUST display field-level validation errors returned by the backend next to the relevant input.
- **FR-007**: The submit button MUST be disabled and show a loading indicator while a request is in-flight.
- **FR-008**: The application MUST expose an auth context so that any component can read the current user's sign-in state, token, and role.
- **FR-009**: All auth page text MUST use the existing translation keys for en, zu, st, and af locales.
- **FR-010**: The auth pages MUST use the updated primary color and follow the established design language (grain texture surface cards, Fraunces serif headings, Nunito body text, OrganicBackground). Form inputs MUST use a pill shape (full border-radius).
- **FR-011**: The registration form MUST include a preferred language selector (en, zu, st, af) that applies the selected locale upon registration.

**Roles**

- **FR-012**: After sign-in, the system MUST read the user's role from the JWT token and store it alongside the session token.
- **FR-013**: Admin users MUST be redirected to the admin dashboard upon sign-in; regular users MUST be redirected to the ask page.
- **FR-014**: The `/[locale]/contracts` and `/[locale]/admin/dashboard` routes MUST require authentication — unauthenticated visitors are redirected to `/[locale]/auth`.
- **FR-015**: The `/[locale]/ask` page MUST be accessible without authentication; however, submitting a question when not signed in MUST redirect the user to `/[locale]/auth` instead of sending the request.
- **FR-016**: The `/[locale]/` (landing) and `/[locale]/rights` routes MUST remain fully accessible without authentication.
- **FR-017**: The navigation bar MUST show an "Admin" link only when the signed-in user has the admin role.
- **FR-018**: The navigation bar MUST display a "Sign In" link when no user is signed in, and a circular avatar showing the user's initials (first + last name) with a "Sign Out" option when signed in.

**Landing Page**

- **FR-019**: The root URL (`/[locale]`) MUST render a dedicated, redesigned landing page separate from the authenticated in-app home experience.
- **FR-020**: The landing page MUST include: a hero section with headline, description, and search/ask CTA; a stats bar (questions answered, acts indexed, languages, contracts analysed); a legal categories grid; a trending questions section; and calls-to-action for sign-in and direct app entry.
- **FR-021**: The landing page MUST use the new primary color throughout and retire the olive color globally from all shared design tokens.
- **FR-022**: The landing page hero CTA MUST adapt based on sign-in state: unauthenticated visitors see "Get Started"; signed-in users see "Go to App".
- **FR-023**: Clicking a trending question on the landing page MUST navigate to the ask page with the question pre-filled.
- **FR-024**: The history page's existing "sign in" prompt link MUST navigate to `/[locale]/auth`.

**Backend**

- **FR-025**: The backend DbMigrator MUST seed a named admin user account (distinct from the default ABP admin) with a known email and a strong password, so the frontend auth flow can be validated end-to-end in any environment.

### Key Entities

- **User Session**: Authenticated user record containing user ID, JWT access token, token expiry, and role. Persisted in browser storage and surfaced via an auth context.
- **User Role**: A label attached to the session — either "Admin" or "User" — that determines post-login routing and navigation visibility.
- **Sign-In Credentials**: Email/username and password pair submitted to obtain a session token.
- **Registration Data**: First name, surname, username, email address, and password used to create a new account.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A returning user can complete sign-in and reach their role-appropriate destination in under 60 seconds on a standard connection.
- **SC-002**: A new user can complete account registration and be automatically signed in within 90 seconds.
- **SC-003**: All form validation errors are displayed within 1 second of a failed submission.
- **SC-004**: Auth page text renders correctly in all four supported languages (en, zu, st, af) with zero visible untranslated keys.
- **SC-005**: The updated primary color is applied consistently across the landing page, auth page, and all shared UI elements with no remaining olive (#5d7052) visible in the redesigned areas.
- **SC-006**: Signing out clears the session token and role within one user action, with no credentials visible in browser storage afterwards.
- **SC-007**: An admin user is redirected to the admin dashboard within 1 second of sign-in completing.
- **SC-008**: Unauthenticated navigation to `/contracts` or `/admin/dashboard` redirects to the auth page within 1 second with no content flash.
- **SC-009**: The `/ask` page renders for unauthenticated users; submitting a question redirects to auth rather than sending the request.
- **SC-010**: The navbar displays a user avatar showing correct initials for any signed-in user.
- **SC-011**: The redesigned landing page loads and displays all sections (hero, stats, categories, trending) within 2 seconds on a standard connection.

## Assumptions

- The backend ABP Zero authentication and registration endpoints are live and accessible at the configured base URL.
- The backend returns role information (e.g., `isAdmin` flag or a `roles` array) as part of the token response or via a lightweight profile endpoint called immediately after sign-in; if no such field exists, the token will be decoded client-side to extract role claims.
- The session token and user session are stored exclusively in cookies (not localStorage); this enables middleware-based route protection and avoids XSS-accessible storage.
- New user accounts are always assigned the "User" (non-admin) role; the backend admin user is pre-seeded.
- There is no "forgot password" or password-reset flow in this scope.
- The CAPTCHA field in the registration payload is not required by the backend in the current configuration.
- The landing page is a redesign of the existing home page content — no new content sections required.
- The new primary color is **deep teal** (`#0d7377`); applied globally to replace olive.
- The preferred language selected during registration sets the active locale on the client only; it is not persisted to the backend user profile in this scope.
- Protected route enforcement uses Next.js middleware (`proxy.ts`) reading a `ml_token` cookie; in-page guards are the fallback for pages that cannot use middleware.
- Mobile responsiveness follows existing page shell conventions.
- The feat/017-roles-landing-page branch (created in error) will be deleted as its scope is fully absorbed into this spec.
