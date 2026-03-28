# Research: AppUser Extension — 006-appuser-extension (Revised)

**Revision note**: Updated after `/speckit.clarify` session on 2026-03-28.
`AppUserRole` enum and `Role` column removed from scope. Role management delegated to ABP's
built-in role system. Three fields remain: `PreferredLanguage`, `DyslexiaMode`, `AutoPlayAudio`.

---

## Decision 1: How to extend ABP IdentityUser

**Decision**: Add the three new properties directly to the existing `User` class in
`backend.Core/Authorization/Users/User.cs`.

**Rationale**: ABP Zero's `User` class (which extends `AbpUser<User>`) is already the project's
AppUser. It is the correct extension point. The canonical approach is to add columns directly to
the `User` class, which maps to `AbpUsers` in PostgreSQL.

**Alternatives considered**:
- Separate `AppUserProfile` entity linked by FK — rejected: unnecessary indirection for three
  lightweight fields; adds a join to every user query.
- ABP's `IExtraPropertyDictionary` — rejected: no type safety, no native EF default-value support,
  not queryable with LINQ.

---

## Decision 2: `PreferredLanguage` enum — reuse existing

**Decision**: Reuse the existing `Language` enum from `backend.Domains.QA` namespace.

**Rationale**: The `Language` enum already defines the four required values (English=0, Zulu=1,
Sesotho=2, Afrikaans=3) and lives in the Domain layer. Reusing it avoids duplication. The `User`
class references it via `using backend.Domains.QA;` — both reside in `backend.Core`.

**Alternatives considered**:
- Duplicating the enum in `Authorization/Users/` — rejected: violates DRY.

---

## Decision 3: Role management — ABP built-in role system (clarified 2026-03-28)

**Decision**: Role classification is fully delegated to ABP's built-in role system.
- **"Citizen"** = ABP's **"Default"** role (auto-assigned to all new registrations by ABP).
- **"Admin"** = ABP's **"Admin"** role (seeded by `HostRoleAndUserCreator` at startup).
- **No `Role` column** is added to `AbpUsers`.
- **No `AppUserRole` enum** is created.

**Rationale**: ABP Zero already seeds and manages both roles. The "Default" role is automatically
assigned to newly registered users by ABP's registration flow. No additional code, seeding, or
enum is required. Using the existing infrastructure avoids duplication and aligns with ABP
conventions.

**Alternatives considered**:
- Custom `AppUserRole` enum + column on `User.cs` — rejected in clarification session; duplicates
  ABP role infrastructure.
- Seeding a new "Citizen" role — rejected; ABP's "Default" role serves this purpose.

---

## Decision 4: Default values — two-layer approach

**Decision**: Apply defaults in both places:
1. **C# property initializers** on `User` class (new object creation).
2. **EF Core Fluent API `HasDefaultValue`** in `OnModelCreating` (SQL-level defaults for direct
   inserts and seed operations).

**Rationale**: Two-layer approach is the safest pattern in ABP Zero where seed operations may
bypass constructors.

---

## Decision 5: Enum storage

**Decision**: Store `Language` enum as integer (EF Core default). No `HasConversion` needed.

**Rationale**: Matches existing enum storage pattern in the project (`Language`, `InputMethod` on
`Conversation` and `Question`).

---

## No Remaining Unknowns

All clarifications resolved. Three fields in scope: `PreferredLanguage`, `DyslexiaMode`,
`AutoPlayAudio`. Role management is out of scope for code changes.
