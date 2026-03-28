# Implementation Plan: Extend ABP IdentityUser with AppUser Preferences

**Branch**: `006-appuser-extension` | **Date**: 2026-03-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/006-appuser-extension/spec.md`
**Revision**: Regenerated after `/speckit.clarify` — `AppUserRole` enum and `Role` column removed; role management delegated to ABP's built-in role system. Three fields in scope only.

## Summary

Extend the existing `User` entity (`backend.Core/Authorization/Users/User.cs`) with three new
fields: `PreferredLanguage` (reusing the existing `Language` enum from `Domains.QA`),
`DyslexiaMode` (bool), and `AutoPlayAudio` (bool). EF Core Fluent API default values are added to
`backendDbContext`. A single migration adds three columns to `AbpUsers`. Role classification
(Citizen / Admin) is handled entirely by ABP's existing "Default" and "Admin" roles — no enum,
no column, and no seeding changes are required.

## Technical Context

**Language/Version**: C# on .NET 9.0
**Primary Dependencies**: ABP Zero 10.x, Entity Framework Core 9.0.5, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.x, Ardalis.GuardClauses
**Storage**: PostgreSQL 15+ via Npgsql
**Testing**: xUnit (via ABP test helpers)
**Target Platform**: Linux server (Docker)
**Project Type**: web-service (ABP Zero REST API)
**Performance Goals**: Standard — no new query-critical paths introduced
**Constraints**: Must extend `AbpUser<User>`; must not replace identity infrastructure; role management must use ABP's built-in role system
**Scale/Scope**: Single entity modification; 3 new columns on `AbpUsers`

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Layer Gate**: `User` (modified) resides in `backend.Core` (Domain layer). EF configuration in `backend.EntityFrameworkCore`. No cross-layer violations. No `AppUserRole` enum to place.
- [x] **Naming Gate**: No new services, DTOs, or controllers introduced. No naming conflicts — `AppUserRole` enum removed; no collision risk with ABP's `Role` entity.
- [x] **Coding Standards Gate**: All new properties include purpose comments; no magic numbers (enum value named via existing `Language` enum); no method length or nesting issues for simple property additions.
- [x] **Skill Gate**: No applicable skill for "extend existing entity with properties". `add-endpoint` does not apply — no new endpoint. Implementation follows BACKEND_STRUCTURE.md guidance directly.
- [x] **Multilingual Gate**: `PreferredLanguage` field is the data-layer foundation for multilingual UX. No user-facing output in this feature (data layer only).
- [x] **Citation Gate**: No AI-facing endpoints in this feature. Not applicable.
- [x] **Accessibility Gate**: No frontend components. `DyslexiaMode` and `AutoPlayAudio` are data-layer fields to be consumed by the frontend in a future feature.

## Project Structure

### Documentation (this feature)

```text
specs/006-appuser-extension/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── backend.Core/
│   │   ├── Authorization/
│   │   │   └── Users/
│   │   │       └── User.cs                  ← MODIFIED: add 3 new properties + using directive
│   │   └── Domains/
│   │       └── QA/
│   │           └── Language.cs              ← UNCHANGED: reused as-is
│   └── backend.EntityFrameworkCore/
│       └── EntityFrameworkCore/
│           ├── backendDbContext.cs           ← MODIFIED: add ConfigureUserExtensions
│           └── Migrations/
│               └── XXXXXX_AddAppUserPreferences.cs  ← GENERATED
```

**Structure Decision**: Backend-only modification. No frontend, no Application layer, no Web.Core
or Web.Host changes in scope.

## Complexity Tracking

> No Constitution Check violations. This table is not applicable.

---

## Implementation Steps

### Step 1 — Extend `User` class

**Action**: Add three properties to `User.cs`:
- `PreferredLanguage Language` — initializer `Language.English`
- `DyslexiaMode bool` — initializer `false`
- `AutoPlayAudio bool` — initializer `false`

Add `using backend.Domains.QA;` at the top of `User.cs` to resolve the `Language` enum.

**Expected Result**: `User` class compiles with all three new fields; default values are applied
when a `new User()` is instantiated.

**Validation**: Build succeeds; `new User()` in a unit test has correct default field values.

---

### Step 2 — Configure EF Core default values

**Action**: Add a private static method `ConfigureUserExtensions(ModelBuilder modelBuilder)` to
`backendDbContext.cs` and call it from `OnModelCreating`. The method calls `HasDefaultValue` for
each of the three new fields on `modelBuilder.Entity<User>()`. Add `using backend.Domains.QA;`
to `backendDbContext.cs` for the `Language` enum reference.

**Expected Result**: EF migration scaffold emits `defaultValue` parameters on the three new
columns. SQL-level defaults are set correctly.

**Validation**: Build succeeds; no EF warning about missing default values for non-nullable columns.

---

### Step 3 — Generate EF Core migration

**Action**: Run from `backend/`:
```
dotnet ef migrations add AddAppUserPreferences \
  --project src/backend.EntityFrameworkCore \
  --startup-project src/backend.Web.Host
```

**Expected Result**: A new migration file in `Migrations/` containing **three** `AddColumn` calls
on `AbpUsers`, each with a `defaultValue`.

**Validation**: Migration file exists; it references the three expected column names; build passes.

---

### Step 4 — Apply migration to PostgreSQL

**Action**: Run:
```
dotnet ef database update \
  --project src/backend.EntityFrameworkCore \
  --startup-project src/backend.Web.Host
```

**Expected Result**: `AbpUsers` table has three new columns.

**Validation**:
```sql
SELECT column_name, data_type, column_default
FROM information_schema.columns
WHERE table_name = 'AbpUsers'
  AND column_name IN ('PreferredLanguage', 'DyslexiaMode', 'AutoPlayAudio');
```
Returns 3 rows with correct types (`integer` / `boolean`) and defaults.

---

### Step 5 — Verify user creation defaults

**Action**: Inspect a newly created user record (via seed or registration) in PostgreSQL.

**Expected Result**:
- `PreferredLanguage = 0` (English)
- `DyslexiaMode = false`
- `AutoPlayAudio = false`

**Validation**: Database query or integration test confirms all three default values are stored
correctly. Existing user records and functionality are unaffected.

---

## Dependencies & Order

```
Step 1 (extend User.cs)
  └── Step 2 (EF default config)
        └── Step 3 (generate migration)
              └── Step 4 (apply migration)
                    └── Step 5 (verify)
```

## Failure Handling

| Failure | Diagnosis | Remedy |
|---|---|---|
| Build error referencing `Language` | Missing `using backend.Domains.QA;` in `User.cs` or `backendDbContext.cs` | Add the using directive |
| Migration missing columns | `ConfigureUserExtensions` not called in `OnModelCreating` | Add the call and regenerate |
| Default values wrong in DB | `HasDefaultValue` not applied | Verify Fluent API config and re-generate migration |
| `AbpUsers` not found at migration | Wrong `--startup-project` | Use `src/backend.Web.Host` |
| Existing tests fail | EF model snapshot mismatch | Run `dotnet ef migrations add` to sync snapshot |

## Deliverables

| Deliverable | File | Status |
|---|---|---|
| Extended `User` entity (3 fields) | `backend.Core/Authorization/Users/User.cs` | To implement |
| EF configuration update | `backend.EntityFrameworkCore/backendDbContext.cs` | To implement |
| Migration file (3 columns) | `backend.EntityFrameworkCore/Migrations/*_AddAppUserPreferences.cs` | To generate |
| Updated PostgreSQL schema | `AbpUsers` table | To apply |
| Verified defaults | DB query / test evidence | To verify |
