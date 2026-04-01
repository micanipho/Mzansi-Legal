# Data Model: AppUser Extension — 006-appuser-extension (Revised)

**Revision note**: Updated after `/speckit.clarify` session on 2026-03-28.
`AppUserRole` enum and `Role` column removed. Three fields remain.

---

## Overview

This feature extends the existing `User` entity (`backend.Core/Authorization/Users/User.cs`) with
three new fields. No new entity tables are created. Three new columns are added to the existing
`AbpUsers` PostgreSQL table via EF Core migration.

---

## Modified Entity: `User` (AppUser)

**File**: `backend/src/backend.Core/Authorization/Users/User.cs`
**Namespace**: `backend.Authorization.Users`
**Inherits**: `AbpUser<User>` → `FullAuditedEntity<long>`
**Table**: `AbpUsers` (managed by ABP Zero)

### New Fields

| Property | Type | Default | Validation | DB Column |
|---|---|---|---|---|
| `PreferredLanguage` | `Language` (enum) | `Language.English` (0) | Must be valid enum value | `PreferredLanguage` (int) |
| `DyslexiaMode` | `bool` | `false` | None | `DyslexiaMode` (bool) |
| `AutoPlayAudio` | `bool` | `false` | None | `AutoPlayAudio` (bool) |

### Existing Fields (unchanged, for context)

| Property | Type | Notes |
|---|---|---|
| `Id` | `long` | ABP Zero PK |
| `UserName` | `string` | ABP identity |
| `EmailAddress` | `string` | ABP identity |
| `TenantId` | `int?` | ABP multi-tenancy |
| `Roles` | `ICollection<UserRole>` | ABP role join — Citizen = Default, Admin = Admin |

---

## Reused Enum: `Language`

**File**: `backend/src/backend.Core/Domains/QA/Language.cs` (unchanged)
**Namespace**: `backend.Domains.QA`

```
Language
├── English   = 0  (default)
├── Zulu      = 1
├── Sesotho   = 2
└── Afrikaans = 3
```

No changes needed. The `User` class adds a `using backend.Domains.QA;` reference.

---

## Role Model (ABP infrastructure — no code changes in this feature)

Role classification is managed by ABP's built-in role system via `AbpRoles` and `AbpUserRoles`.

| Classification | ABP Role Name | Seeded By | Assignment |
|---|---|---|---|
| Citizen | `Default` | `HostRoleAndUserCreator` (existing) | Auto-assigned on registration |
| Admin | `Admin` | `HostRoleAndUserCreator` (existing) | Manually assigned by administrator |

No new roles are seeded and no changes to `HostRoleAndUserCreator` are required.

---

## EF Core Configuration Changes

**File**: `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs`

A new private method `ConfigureUserExtensions(modelBuilder)` is called from `OnModelCreating`.
It sets database-level default values for the three new fields:

```
AbpUsers.PreferredLanguage  → DEFAULT 0     (Language.English)
AbpUsers.DyslexiaMode       → DEFAULT false
AbpUsers.AutoPlayAudio      → DEFAULT false
```

No new `DbSet<T>` is required — `AbpUsers` is managed by ABP Zero's DbContext base class.

---

## Migration

**Name**: `AddAppUserPreferences`
**Project**: `backend.EntityFrameworkCore`
**Expected operations**:
- `AddColumn` — `PreferredLanguage` (int, default 0) on `AbpUsers`
- `AddColumn` — `DyslexiaMode` (bool, default false) on `AbpUsers`
- `AddColumn` — `AutoPlayAudio` (bool, default false) on `AbpUsers`

**Run command** (from `backend/` directory):
```
dotnet ef migrations add AddAppUserPreferences --project src/backend.EntityFrameworkCore --startup-project src/backend.Web.Host
```

---

## Validation Rules

| Rule | Enforcement Point |
|---|---|
| `PreferredLanguage` must be 0–3 | C# enum constraint (compile-time); EF stores as int |
| `DyslexiaMode` defaults to false | C# property initializer + EF `HasDefaultValue` |
| `AutoPlayAudio` defaults to false | C# property initializer + EF `HasDefaultValue` |
| `PreferredLanguage` defaults to English | C# property initializer + EF `HasDefaultValue` |

---

## Layer Compliance Summary

| Artifact | Layer | Location |
|---|---|---|
| `User` (extended) | Core (Domain) | `backend.Core/Authorization/Users/User.cs` |
| `Language` enum (reused) | Core (Domain) | `backend.Core/Domains/QA/Language.cs` |
| EF configuration | EntityFrameworkCore | `backendDbContext.OnModelCreating` |
| Migration | EntityFrameworkCore | `Migrations/` |

No Application, Web.Core, or Web.Host layer changes are required for this feature.
