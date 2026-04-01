# Quickstart: AppUser Extension — 006-appuser-extension (Revised)

**Revision note**: Updated after `/speckit.clarify` session on 2026-03-28.
`AppUserRole` enum and `Role` column removed. Three fields only.

---

## What this feature does

Adds three new fields to the existing `User` entity (ABP Zero's AppUser):
- `PreferredLanguage` — chosen interface language (English/Zulu/Sesotho/Afrikaans), default `en`
- `DyslexiaMode` — enables dyslexia-friendly font and spacing adjustments, default `false`
- `AutoPlayAudio` — enables automatic audio playback for responses, default `false`

Role classification (Citizen / Admin) is handled by ABP's existing role system — no code changes
required for roles.

---

## Files to create or modify

| Action | File |
|---|---|
| **Modify** | `backend/src/backend.Core/Authorization/Users/User.cs` |
| **Modify** | `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs` |
| **Generate** | `backend/src/backend.EntityFrameworkCore/Migrations/XXXXXX_AddAppUserPreferences.cs` |

---

## Step-by-step

### 1. Extend `User` class

Add three properties to `backend/src/backend.Core/Authorization/Users/User.cs`:

```csharp
using backend.Domains.QA; // add at top of file — for Language enum

// Inside the User class:

/// <summary>User's preferred interface language. Defaults to English.</summary>
public Language PreferredLanguage { get; set; } = Language.English;

/// <summary>When true, dyslexia-friendly font and spacing adjustments are applied.</summary>
public bool DyslexiaMode { get; set; } = false;

/// <summary>When true, audio responses are played automatically.</summary>
public bool AutoPlayAudio { get; set; } = false;
```

### 2. Configure EF defaults in `backendDbContext`

Add a new private method and call it from `OnModelCreating`:

```csharp
// In OnModelCreating, add:
ConfigureUserExtensions(modelBuilder);

// New method:
/// <summary>
/// Applies database-level default values for the AppUser preference columns on AbpUsers.
/// C# property initializers handle object-creation defaults; these handle SQL-level defaults.
/// </summary>
private static void ConfigureUserExtensions(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>(b =>
    {
        b.Property(u => u.PreferredLanguage).HasDefaultValue(Language.English);
        b.Property(u => u.DyslexiaMode).HasDefaultValue(false);
        b.Property(u => u.AutoPlayAudio).HasDefaultValue(false);
    });
}
```

Add the required using at the top of `backendDbContext.cs`:
```csharp
using backend.Domains.QA; // for Language enum
```

### 3. Generate migration

From the `backend/` directory:

```bash
dotnet ef migrations add AddAppUserPreferences \
  --project src/backend.EntityFrameworkCore \
  --startup-project src/backend.Web.Host
```

Verify the generated migration contains **three** `AddColumn` calls on `AbpUsers`.

### 4. Apply migration

```bash
dotnet ef database update \
  --project src/backend.EntityFrameworkCore \
  --startup-project src/backend.Web.Host
```

Or run the Migrator project:

```bash
dotnet run --project src/backend.Migrator
```

### 5. Verify

Connect to PostgreSQL and confirm the three new columns:

```sql
SELECT column_name, data_type, column_default
FROM information_schema.columns
WHERE table_name = 'AbpUsers'
  AND column_name IN ('PreferredLanguage', 'DyslexiaMode', 'AutoPlayAudio');
```

Expected: 3 rows returned with correct types and defaults.

---

## Role verification (no code change — informational only)

To confirm ABP's Default role is auto-assigned to new users:

```sql
SELECT u."UserName", r."Name" AS "RoleName"
FROM "AbpUsers" u
JOIN "AbpUserRoles" ur ON u."Id" = ur."UserId"
JOIN "AbpRoles" r ON ur."RoleId" = r."Id"
WHERE u."UserName" = '<new-user-name>';
```

Expected: one row with `RoleName = Default`.

---

## Acceptance check

- [ ] `User.cs` has three new properties with initializers
- [ ] `backendDbContext` has `ConfigureUserExtensions` called in `OnModelCreating`
- [ ] Migration file generated with three `AddColumn` operations on `AbpUsers`
- [ ] Database updated — columns verified in PostgreSQL
- [ ] `new User()` object has correct default values for all three fields
- [ ] Existing tests pass (no regressions)
