# Feature Specification: Auth Pages and Landing Page

**Feature Branch**: `feat/018-auth-landing-page`
**Created**: 2026-03-31
**Status**: Draft
**Milestone**: Auth

## User Scenarios & Testing *(mandatory)*

### User Story 1 - New User Registration (Priority: P1)

A new visitor arrives at the application and wants to create an account so they can access legal assistance tools.

**Why this priority**: Registration is the entry point for all authenticated features. Without it, no user can access protected tools.

**Independent Test**: Can be fully tested by navigating to the registration page, filling in name, email, password, and preferred language, submitting, and verifying redirection to the home page with user initials visible in the navbar.

**Acceptance Scenarios**:

1. **Given** a visitor is on the registration page, **When** they enter a valid name, email address, password, and preferred language and submit, **Then** an account is created and they are redirected to the home page as a logged-in user with their initials in the navbar.
2. **Given** a visitor enters an email already registered, **When** they submit the form, **Then** an error message informs them the email is already in use.
3. **Given** a visitor submits with an invalid email format or a password that does not meet minimum length, **When** validation runs, **Then** inline error messages appear for each invalid field.

---

### User Story 2 - Returning User Login (Priority: P1)

A registered user wants to log in so they can access their account and protected legal tools.

**Why this priority**: Login is the prerequisite for all protected features and must work reliably to ensure continuity of access.

**Independent Test**: Can be fully tested by visiting the login page, entering valid credentials, and confirming the user is redirected to their intended page with their initials displayed in the navbar.

**Acceptance Scenarios**:

1. **Given** a registered user is on the login page, **When** they enter their correct email and password, **Then** they are authenticated and redirected to the page they were trying to access (or home if no redirect target).
2. **Given** a user enters incorrect credentials, **When** they submit, **Then** a clear error message is shown without revealing which field is wrong.
3. **Given** a logged-in user's session expires, **When** they attempt to access a protected route, **Then** they are redirected to the login page.

---

### User Story 3 - Protected Route Access Control (Priority: P2)

A visitor tries to access a page that requires authentication without being logged in.

**Why this priority**: Route protection is essential for securing legal documents and admin functionality.

**Independent Test**: Can be fully tested by navigating directly to `/contracts` or `/admin/dashboard` while not logged in and confirming redirection to the login page.

**Acceptance Scenarios**:

1. **Given** a visitor is not logged in, **When** they navigate to `/contracts`, **Then** they are redirected to the login page.
2. **Given** a visitor is not logged in, **When** they navigate to `/admin/dashboard`, **Then** they are redirected to the login page.
3. **Given** a visitor is not logged in and on the `/ask` page, **When** they submit a question, **Then** they are redirected to the login page before their question is processed.
4. **Given** a visitor is not logged in, **When** they visit `/` (home) or `/rights`, **Then** the pages load normally without any authentication prompt.
5. **Given** a logged-in non-admin user navigates to `/admin/dashboard`, **When** the page loads, **Then** they are redirected to the home page.

---

### User Story 4 - User Identity Display in Navbar (Priority: P2)

A logged-in user wants to see confirmation of their authentication state in the navigation bar.

**Why this priority**: Provides immediate visual confirmation of authentication and improves user trust.

**Independent Test**: Can be fully tested by logging in and confirming that the navbar shows the user's initials or avatar, and that a logout action is accessible from the navbar.

**Acceptance Scenarios**:

1. **Given** a user is logged in, **When** they view any page, **Then** the navbar displays their initials or avatar in place of login/register links.
2. **Given** a user is not logged in, **When** they view any page, **Then** the navbar shows login and register links.
3. **Given** a logged-in user clicks their avatar or initials, **When** the menu opens, **Then** a logout option is available and clicking it ends their session and returns them to the home page.

---

### User Story 5 - Landing Page Discovery (Priority: P3)

A first-time visitor arrives at the home page and wants to understand what the app offers before signing up.

**Why this priority**: The landing page drives registration conversions and explains the platform's value to potential users.

**Independent Test**: Can be fully tested by visiting `/` as an unauthenticated user and confirming the page describes the product and presents at least one call-to-action to register or log in.

**Acceptance Scenarios**:

1. **Given** a visitor opens the home URL, **When** the page loads, **Then** they see a landing page with a product description and clear calls to action (e.g., "Get Started", "Login").
2. **Given** a visitor clicks the primary call-to-action, **When** they interact, **Then** they are taken to the registration page.

---

### Edge Cases

- What happens if a user registers and immediately tries to access a protected route before any redirect completes?
- What if the user's stored authentication token is corrupted or tampered with when they return to the app?
- What if the user closes the browser without logging out — are they still considered logged in on their next visit?
- What happens if an admin tries to access `/admin/dashboard` but the admin account was deactivated?
- What if a user registers with an email that has mixed letter casing (e.g., User@Example.com vs user@example.com)?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow any visitor to register for an account by providing their full name, email address, password, and preferred language.
- **FR-002**: The system MUST allow registered users to log in using their email address and password.
- **FR-003**: The system MUST issue and persist an authentication token upon successful login or registration, surviving page reloads within the same browser session.
- **FR-004**: The system MUST restrict access to `/contracts` and `/admin/dashboard` to authenticated users only, redirecting unauthenticated visitors to the login page.
- **FR-005**: The `/` (home) and `/rights` pages MUST be accessible to unauthenticated visitors without any redirect.
- **FR-006**: The `/ask` page MUST be viewable without authentication, but MUST redirect the visitor to the login page when they attempt to submit a question.
- **FR-007**: The navbar MUST display the logged-in user's initials or avatar when authenticated, replacing the login and register navigation links.
- **FR-008**: The navbar MUST provide a logout action that clears the user's authentication token and redirects them to the home page.
- **FR-009**: The login page MUST use pill-shaped (fully rounded) input fields for the email and password inputs.
- **FR-010**: The registration page MUST include fields for full name, email, password, and a preferred language selector offering at minimum: English, Zulu, Xhosa, and Afrikaans.
- **FR-011**: The system MUST display clear, user-friendly error messages for invalid credentials, duplicate email on registration, and failed form validation — without revealing which specific field (email vs password) caused a login failure.
- **FR-012**: A pre-seeded admin account MUST exist from initial deployment, accessible without any manual registration.
- **FR-013**: Non-admin authenticated users who navigate to `/admin/dashboard` MUST be redirected to the home page.
- **FR-014**: The home page MUST serve as a landing page, presenting a product description and at least one prominent call-to-action directing visitors to register or log in.

### Key Entities

- **User Account**: Represents a registered user; has a display name, email address, preferred language, and role (standard user or admin).
- **Authentication Token**: A credential issued on successful authentication that grants access to protected resources; has an expiry period.
- **Session State**: The client-side record of whether a user is authenticated, including their display name/initials and role, derived from the stored token.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A new user can complete the full registration flow — from landing page to authenticated home page — in under 2 minutes.
- **SC-002**: A returning user can log in and reach a protected page in under 30 seconds from the login page.
- **SC-003**: 100% of unauthenticated attempts to access `/contracts` or `/admin/dashboard` result in redirection to the login page.
- **SC-004**: The logged-in user's identity (initials or avatar) appears in the navbar on every page without requiring a manual page reload after login.
- **SC-005**: All authentication error states (wrong password, duplicate email, invalid input) display a human-readable message within the current page without a full reload.
- **SC-006**: First-time testers can correctly describe what the application does after viewing the landing page alone.

## Assumptions

- Users have a modern web browser with JavaScript enabled and a stable internet connection.
- Preferred language selection at registration is stored for future personalization and does not affect the authentication flow itself.
- The pre-seeded admin account credentials are documented in the project's deployment guide and not embedded in public source code.
- Logging out on one browser tab does not automatically log the user out of other open tabs in the same browser (single-tab logout is acceptable for MVP).
- Token storage method (cookies vs localStorage) will be decided by the implementation team based on security trade-offs; the spec does not mandate one approach.
- Social login (Google, etc.) is explicitly out of scope for this milestone.
- Password reset ("forgot password") is out of scope for this milestone.
- Email verification on registration is out of scope for this milestone.
