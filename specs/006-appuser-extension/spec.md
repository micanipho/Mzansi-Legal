# Feature Specification: Extend ABP IdentityUser with AppUser Preferences

**Feature Branch**: `006-appuser-extension`
**Created**: 2026-03-28
**Status**: Draft
**Input**: User description: "Define the extension of the ABP IdentityUser to support user preferences, accessibility settings, and simplified role management."

## Clarifications

### Session 2026-03-28

- Q: Should the `Role` field be removed from `User.cs` and role assignment delegated entirely to ABP's `AbpUserRoles` join table? → A: Yes — remove the `Role` field from `User.cs`; role assignment exclusively via ABP's `AbpUserRoles` join table (Option A)
- Q: Should "Citizen" map to ABP's built-in "Default" role, and "Admin" to ABP's built-in "Admin" role? → A: Yes — "Citizen" = ABP Default role, "Admin" = ABP Admin role; no new roles seeded (Option A)
- Q: Should User Story 3 be kept as a documentation statement with no implementation tasks? → A: Yes — keep US3 as a documentation-only statement describing ABP's existing behaviour; no implementation tasks required (Option A)

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Register with Language Preference (Priority: P1)

A new citizen registers on the platform and selects their preferred language (English, Zulu, Sesotho, or Afrikaans). The system stores this preference on their profile so that all subsequent interactions can be presented in their chosen language.

**Why this priority**: Language preference is foundational to the platform's core value proposition of serving South African users in their home language. Without this, the multilingual experience cannot be delivered.

**Independent Test**: A new user can be created with a preferred language set, and on retrieval, the stored language value matches what was saved.

**Acceptance Scenarios**:

1. **Given** a new user is being registered, **When** they select "Zulu" as their preferred language, **Then** the system stores `PreferredLanguage = zu` on their profile
2. **Given** a registered user profile, **When** the profile is retrieved, **Then** the `PreferredLanguage` field reflects the value saved during registration
3. **Given** a new user is being registered with no explicit language choice, **When** their account is created, **Then** `PreferredLanguage` defaults to `en`

---

### User Story 2 - Set Accessibility Preferences (Priority: P2)

A user with dyslexia enables dyslexia-friendly mode on their account. Another user who prefers audio-based interaction enables auto-play audio. These preferences are persisted and applied on subsequent visits.

**Why this priority**: Accessibility is a legal and ethical requirement for a public-facing legal information platform. Supporting users with different needs is critical to broad adoption.

**Independent Test**: A user account can be created or updated with `DyslexiaMode = true` and `AutoPlayAudio = true`, and those values are retrieved correctly.

**Acceptance Scenarios**:

1. **Given** a user account exists, **When** the user enables dyslexia mode, **Then** `DyslexiaMode` is stored as `true` on their profile
2. **Given** a user account exists, **When** the user enables auto-play audio, **Then** `AutoPlayAudio` is stored as `true` on their profile
3. **Given** a newly created user, **When** no accessibility preferences are specified, **Then** both `DyslexiaMode` and `AutoPlayAudio` default to `false`

---

### User Story 3 - Role Classification via ABP Role System (Priority: P3) *(Documentation only — no implementation tasks)*

Users on the platform are classified as either **Citizens** (standard users) or **Admins** (platform administrators). This classification is managed entirely through ABP's built-in role system — no custom role field is added to the `User` entity.

- **Citizen** maps to ABP's built-in **"Default"** role, which is automatically assigned to all newly registered users by ABP's registration flow.
- **Admin** maps to ABP's built-in **"Admin"** role, which is seeded at application startup by `HostRoleAndUserCreator`.

No new roles need to be seeded, and no new column is added to `AbpUsers` for this purpose.

**Why this priority**: Role differentiation separates admin management capabilities from citizen Q&A interactions. ABP's existing infrastructure already provides this — no code changes are required.

**Independent Test**: Verify that a newly registered user is automatically assigned the "Default" ABP role (confirming Citizen classification). Verify that the seeded admin user holds the "Admin" ABP role. Both checks use the existing `AbpUserRoles` table.

**Acceptance Scenarios**:

1. **Given** a citizen self-registers, **When** their account is created, **Then** ABP automatically assigns the "Default" role via `AbpUserRoles` — no manual step required
2. **Given** an admin is creating a privileged user, **When** they assign the "Admin" role via ABP's role management, **Then** the user's `AbpUserRoles` entry reflects the "Admin" role
3. **Given** any user on the platform, **When** their role is queried, **Then** their classification (Citizen = Default, Admin = Admin) is derivable from `AbpUserRoles`

---

### Edge Cases

- What happens when an invalid language code is submitted (e.g., a value not in `en`, `zu`, `st`, `af`)?
- How does the system handle an update that attempts to set `PreferredLanguage` to null or empty?
- What happens if both `DyslexiaMode` and `AutoPlayAudio` are not provided during user creation — are defaults applied consistently?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST store a `PreferredLanguage` value on each user profile, restricted to the values: `en`, `zu`, `st`, `af`
- **FR-002**: System MUST default `PreferredLanguage` to `en` when no language is specified during user creation
- **FR-003**: System MUST store a `DyslexiaMode` boolean flag on each user profile, defaulting to `false`
- **FR-004**: System MUST store an `AutoPlayAudio` boolean flag on each user profile, defaulting to `false`
- **FR-005**: System MUST use ABP's built-in role system to classify users — "Citizen" maps to the ABP "Default" role; "Admin" maps to the ABP "Admin" role; no `Role` column is added to `AbpUsers`
- **FR-006**: System MUST rely on ABP's registration flow to automatically assign the "Default" role to newly registered users — no additional seeding or custom logic is required
- **FR-007**: System MUST persist all new user profile fields (`PreferredLanguage`, `DyslexiaMode`, `AutoPlayAudio`) to the database via the existing identity user store
- **FR-008**: System MUST reject invalid enumeration values for `PreferredLanguage` with a clear validation error
- **FR-009**: System MUST extend the existing identity user entity without replacing the underlying identity infrastructure

### Key Entities

- **AppUser**: An extension of the platform's identity user, representing a registered person on the platform. Carries language preference and accessibility settings. Inherits all standard identity fields (ID, username, email, password hash, etc.) from the base identity user. Role classification is managed via ABP's `AbpUserRoles` join table — not as a field on this entity.
- **PreferredLanguage (enum)**: Represents the user's chosen interface language. Allowed values: `en` (English), `zu` (Zulu), `st` (Sesotho), `af` (Afrikaans).
- **ABP Role (infrastructure)**: "Default" role = Citizen classification; "Admin" role = Admin classification. Both are seeded by ABP Zero's built-in startup infrastructure. Managed via `AbpRoles` and `AbpUserRoles` tables — not created by this feature.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All newly created user accounts correctly store the default values — `PreferredLanguage = en`, `DyslexiaMode = false`, `AutoPlayAudio = false` — with zero manual intervention required; new users are automatically assigned the ABP "Default" role by ABP's registration flow
- **SC-002**: 100% of user creation and update operations that specify a language value outside the allowed enumeration (`en`, `zu`, `st`, `af`) are rejected with a clear validation error
- **SC-003**: All three new fields (`PreferredLanguage`, `DyslexiaMode`, `AutoPlayAudio`) are present and queryable in the user data store after the migration is applied; role classification is queryable via the existing `AbpUserRoles` table
- **SC-004**: Existing user accounts and functionality are unaffected after the extension is applied — zero regressions in existing identity features

## Assumptions

- The platform's existing identity system is the built-in ABP Zero identity module and it is already operational in the current environment
- The extension mechanism adds three columns directly to `AbpUsers` via the `User` class — no new entity table is created
- Role classification ("Citizen" / "Admin") is fully handled by ABP's built-in role system; the "Default" and "Admin" roles are already seeded by `HostRoleAndUserCreator` and require no changes from this feature
- Mobile-specific accessibility settings (e.g., font size scaling, contrast mode) are out of scope for this feature
- User preference management UI (screens for users to change their own preferences) is out of scope for this spec — this covers only the data layer
- All four languages (`en`, `zu`, `st`, `af`) are treated as equal first-class options; `en` is only the default for new accounts when no preference is stated
- The database migration will be applied to the existing PostgreSQL instance managed by the project
