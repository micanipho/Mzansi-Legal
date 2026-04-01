# Tasks: Extend ABP IdentityUser with AppUser Preferences

**Input**: Design documents from `/specs/006-appuser-extension/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, quickstart.md ✅
**Revision**: Regenerated after `/speckit.clarify` — 3 fields only; no `AppUserRole` enum; role management via ABP built-in roles (no tasks required).

**Tests**: No test tasks — not explicitly requested in the specification.

**Organization**: Tasks grouped by user story to enable independent implementation and
verification of each story increment.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Exact file paths included in every description

---

## Phase 1: Foundational (Blocking Prerequisites)

**Purpose**: No shared infrastructure needs to be created from scratch — the project already has
`User.cs` and `backendDbContext.cs`. This phase is a lightweight readiness check only.

- [x] T001 Confirm current branch is `006-appuser-extension` and `backend/src/backend.Core/Authorization/Users/User.cs` exists with no uncommitted conflicts

**Checkpoint**: Working directory is clean; files to be modified are confirmed present.

---

## Phase 2: User Story 1 — Register with Language Preference (Priority: P1) 🎯 MVP

**Goal**: Store `PreferredLanguage` on `User` using the existing `Language` enum from
`backend.Domains.QA`. Default to `Language.English` when no preference is provided.

**Independent Test**: Instantiate `new User()` in C# — confirm `PreferredLanguage == Language.English`.
After migration, run `SELECT column_name, data_type, column_default FROM information_schema.columns WHERE table_name = 'AbpUsers' AND column_name = 'PreferredLanguage'` — confirm 1 row with default `0`.

### Implementation for User Story 1

- [x] T002 [US1] Add `using backend.Domains.QA;` directive and `PreferredLanguage` property with XML doc comment and initializer `Language.English` to `backend/src/backend.Core/Authorization/Users/User.cs`
- [x] T003 [US1] Add `ConfigureUserExtensions` private static method to `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs` — call it from `OnModelCreating`, add `using backend.Domains.QA;`, and set `HasDefaultValue(Language.English)` for `PreferredLanguage` on `modelBuilder.Entity<User>()`

**Checkpoint**: Solution builds; `new User().PreferredLanguage == Language.English`; `backendDbContext.cs` compiles with the new Fluent API method.

---

## Phase 3: User Story 2 — Set Accessibility Preferences (Priority: P2)

**Goal**: Store `DyslexiaMode` and `AutoPlayAudio` boolean flags on `User`. Both default to
`false` on new user creation.

**Independent Test**: Instantiate `new User()` in C# — confirm `DyslexiaMode == false` and `AutoPlayAudio == false`.
After migration, run `SELECT column_name, data_type, column_default FROM information_schema.columns WHERE table_name = 'AbpUsers' AND column_name IN ('DyslexiaMode', 'AutoPlayAudio')` — confirm 2 rows each with default `false`.

### Implementation for User Story 2

- [x] T004 [US2] Add `DyslexiaMode` property (bool, initializer `false`, XML doc comment) and `AutoPlayAudio` property (bool, initializer `false`, XML doc comment) to `backend/src/backend.Core/Authorization/Users/User.cs`
- [x] T005 [US2] Extend `ConfigureUserExtensions` in `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs` — add `HasDefaultValue(false)` for `DyslexiaMode` and `HasDefaultValue(false)` for `AutoPlayAudio` inside the existing `modelBuilder.Entity<User>()` lambda

**Checkpoint**: Solution builds; `new User().DyslexiaMode == false && new User().AutoPlayAudio == false`; both `HasDefaultValue` calls present in `ConfigureUserExtensions`.

---

## Phase 4: User Story 3 — Role Classification via ABP Role System (Priority: P3) *(Documentation only)*

**Goal**: Confirm that ABP's built-in "Default" and "Admin" roles satisfy the Citizen/Admin
classification requirement. No code changes required.

**Independent Test**: Run `SELECT r."Name" FROM "AbpRoles" r WHERE r."Name" IN ('Default', 'Admin')` — confirm both roles exist in the seeded database.

### Implementation for User Story 3

- [x] T006 [US3] Verify ABP role seeds exist by querying `SELECT "Name" FROM "AbpRoles" WHERE "Name" IN ('Default', 'Admin')` against the PostgreSQL instance and confirming both rows are present — no code changes required; document result in a comment or PR description

**Checkpoint**: Both ABP roles confirmed present; no entity, enum, or migration changes needed for US3.

---

## Phase 5: Migration & Database Update

**Purpose**: Consolidate all entity changes (T002–T004) into a single EF Core migration and
apply it to PostgreSQL.

**⚠️ CRITICAL**: T002, T003, T004, and T005 must all be complete before generating the migration.

- [x] T007 Generate EF Core migration by running from `backend/`: `dotnet ef migrations add AddAppUserPreferences --project src/backend.EntityFrameworkCore --startup-project src/backend.Web.Host` — confirm the generated file in `backend/src/backend.EntityFrameworkCore/Migrations/` contains exactly **three** `AddColumn` operations on `AbpUsers` (`PreferredLanguage`, `DyslexiaMode`, `AutoPlayAudio`)
- [x] T008 Apply migration to PostgreSQL by running from `backend/`: `dotnet ef database update --project src/backend.EntityFrameworkCore --startup-project src/backend.Web.Host` — confirm command exits without errors

**Checkpoint**: Migration applied; no errors reported; `AbpUsers` now has three new columns.

---

## Phase 6: Polish & Validation

**Purpose**: Verify schema correctness, default values, and build health across all stories.

- [x] T009 [P] Verify all three new columns exist in PostgreSQL with correct types and defaults by running: `SELECT column_name, data_type, column_default FROM information_schema.columns WHERE table_name = 'AbpUsers' AND column_name IN ('PreferredLanguage', 'DyslexiaMode', 'AutoPlayAudio')` — confirm 3 rows returned with `integer`/`boolean` types and correct defaults
- [x] T010 [P] Confirm solution builds with no errors or warnings by running `dotnet build` from `backend/` and reviewing output
- [x] T011 Verify user creation defaults by querying a newly seeded or created user record in PostgreSQL — confirm `PreferredLanguage = 0`, `DyslexiaMode = false`, `AutoPlayAudio = false`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 1)**: No dependencies — start immediately
- **US1 (Phase 2)**: Start after T001; independent of US2 and US3
- **US2 (Phase 3)**: Can follow immediately after Phase 2 checkpoint; independent of US1 and US3
- **US3 (Phase 4)**: No code dependencies; can run at any time after DB is accessible (T006 is a query-only check)
- **Migration (Phase 5)**: Depends on **T002, T003, T004, T005** — all entity and EF changes must be complete
- **Polish (Phase 6)**: Depends on **T008** (migration applied)

### User Story Dependencies

- **US1 (P1)**: Independent — no dependency on US2 or US3
- **US2 (P2)**: Independent — no dependency on US1 or US3
- **US3 (P3)**: No code dependency — documentation/verification only

### Within Each User Story

- Property addition to `User.cs` must precede EF configuration in `backendDbContext.cs`
- All entity changes (T002–T005) must precede migration generation (T007)
- Migration (T007) must precede database update (T008)
- Database update (T008) must precede schema validation (T009–T011)

### Parallel Opportunities

- T006 (US3 role verification) can run at any time in parallel with US1 and US2 work
- T009 and T010 are marked [P] — schema verification and build check are independent after T008

---

## Parallel Example: US1 and US3 together

```text
# Start simultaneously after T001:
Task T002: Add PreferredLanguage to User.cs (backend.Core/Authorization/Users/User.cs)
Task T006: Verify ABP Default and Admin roles exist in AbpRoles (DB query only)

# After T002 completes:
Task T003: Add ConfigureUserExtensions to backendDbContext.cs

# After T003 completes, continue with US2:
Task T004: Add DyslexiaMode and AutoPlayAudio to User.cs
Task T005: Add HasDefaultValue calls to ConfigureUserExtensions
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: T001
2. Complete Phase 2: T002, T003
3. Complete Phase 5 (partial): T007, T008 — generates migration for `PreferredLanguage` only
4. **STOP and VALIDATE**: Confirm `PreferredLanguage` column in `AbpUsers` with default `0`
5. Proceed to US2 and then regenerate migration

### Incremental Delivery (Recommended)

1. T001 → T002 → T003 (US1 entity + EF config)
2. T004 → T005 (US2 entity + EF config)
3. T006 (US3 verification — parallel at any point)
4. T007 → T008 (single migration covering all three columns)
5. T009 → T010 → T011 (validate)

### Single-Developer Sequential Path

```
T001 → T002 → T003 → T004 → T005 → T006 → T007 → T008 → T009 → T010 → T011
```

---

## Notes

- No frontend changes — data layer only
- No new application services or DTOs — out of scope
- `Language` enum in `backend.Core/Domains/QA/Language.cs` is reused — do NOT create a duplicate
- No `AppUserRole` enum — role classification uses ABP's existing "Default" and "Admin" roles
- `ConfigureUserExtensions` is added once (T003) and extended in place (T005) — same method
- The single migration `AddAppUserPreferences` covers all three columns — do not split by story
- Suggested commit points: after T005 (all entity changes), after T008 (migration applied), after T011 (verified)
