---
description: Scaffold a full ABP CRUD endpoint from entity to database
argument-hint: [EntityName]
allowed-tools: Read, Write, Edit, Glob, Grep, Bash(dotnet:*)
---

# Add ABP CRUD Endpoint

Scaffold a complete ABP Framework CRUD endpoint — entity, permissions, DTOs, AutoMapper profile, interface, app service, DbContext registration, and EF Core migration.

## Arguments

`$ARGUMENTS` — the entity name in PascalCase singular (e.g. `Invoice`, `Product`, `Expense`). If not provided, ask the user for it.

## Instructions

### Step 1 — Gather information

Parse `$ARGUMENTS` to extract:
- **EntityName**: PascalCase singular (e.g. `Invoice`)
- **EntityNames**: PascalCase plural (e.g. `Invoices`) — ask user to confirm if pluralisation is non-trivial
- **entityName**: camelCase singular (e.g. `invoice`)

Then ask the user:
1. What **fields** should the entity have? For each field, collect:
   - Property name (PascalCase)
   - C# type (`string`, `decimal`, `int`, `bool`, `DateTime`, `Guid`, etc.)
   - Is it required? Max length if string? Range if numeric?
   - Should it be filterable via the keyword search?
2. What **primary key type** should the entity use? (default: `int`)
3. Should it use `FullAuditedEntity` (soft-delete + audit trail) or `Entity` (bare)? Default: `FullAuditedEntity`.
4. Should the **list endpoint** filter by a `keyword` field? Which string field(s)?
5. Should the **list** be sorted by `CreationTime` descending (default) or another field?

### Step 2 — Discover project structure

Use Glob and Grep to locate:
- The **Core** project: find `**/Authorization/PermissionNames.cs` → parent folder is Core project root
- The **Application** project: find `**/*AppServiceBase.cs` → parent folder is Application project root
- The **EntityFrameworkCore** project: find `**/EntityFrameworkCore/*DbContext.cs` → parent folder contains DbContext
- The **namespace root** (e.g. `SpendWise`): read the namespace from `PermissionNames.cs`
- The **AuthorizationProvider**: find `**/*AuthorizationProvider.cs`
- The **DbContext file**: find `**/*DbContext.cs` (not Designer or Snapshot)
- The **EF project path**: the directory containing the DbContext `.csproj`

### Step 3 — Generate files

Generate ALL files below using the discovered paths and namespace. Replace every placeholder:
- `{Namespace}` — root namespace (e.g. `SpendWise`)
- `{EntityName}` — PascalCase singular
- `{EntityNames}` — PascalCase plural
- `{entityName}` — camelCase singular
- `{PkType}` — primary key type (e.g. `int`)
- `{AuditBase}` — `FullAuditedEntity<{PkType}>` or `Entity<{PkType}>`
- `{Fields}` — generated from user answers
- `{FilterableFields}` — WhereIf clauses for filterable fields

---

#### File A — Entity
**Path:** `{CoreProject}/{EntityName}s/{EntityName}.cs`

```csharp
using Abp.Domain.Entities.Auditing;
// Add: using System; if any DateTime fields

namespace {Namespace}.{EntityName}s
{
    public class {EntityName} : {AuditBase}
    {
        // One property per user-supplied field, e.g.:
        // public string Description { get; set; }
        // public decimal Amount { get; set; }
        // public DateTime Date { get; set; }
    }
}
```

---

#### File B — PermissionNames additions
**Edit:** `{CoreProject}/Authorization/PermissionNames.cs`

Add inside the class (before the closing `}`):
```csharp
public const string Pages_{EntityNames} = "Pages.{EntityNames}";
public const string Pages_{EntityNames}_Create = "Pages.{EntityNames}.Create";
public const string Pages_{EntityNames}_Edit = "Pages.{EntityNames}.Edit";
public const string Pages_{EntityNames}_Delete = "Pages.{EntityNames}.Delete";
```

Do NOT add `const object` declarations. Do NOT duplicate existing entries.

---

#### File C — AuthorizationProvider additions
**Edit:** `{CoreProject}/Authorization/*AuthorizationProvider.cs`

Inside `SetPermissions`, append:
```csharp
var {entityName}sPermission = context.CreatePermission(PermissionNames.Pages_{EntityNames}, L("{EntityNames}"));
{entityName}sPermission.CreateChildPermission(PermissionNames.Pages_{EntityNames}_Create, L("Create{EntityName}"));
{entityName}sPermission.CreateChildPermission(PermissionNames.Pages_{EntityNames}_Edit, L("Edit{EntityName}"));
{entityName}sPermission.CreateChildPermission(PermissionNames.Pages_{EntityNames}_Delete, L("Delete{EntityName}"));
```

---

#### File D — ExpenseDto (read DTO)
**Path:** `{ApplicationProject}/{EntityName}s/Dto/{EntityName}Dto.cs`

```csharp
using Abp.Application.Services.Dto;
using System;

namespace {Namespace}.{EntityName}s.Dto
{
    public class {EntityName}Dto : EntityDto<{PkType}>
    {
        // Mirror all entity fields, same types, no validation attributes
    }
}
```

---

#### File E — CreateUpdate DTO
**Path:** `{ApplicationProject}/{EntityName}s/Dto/CreateUpdate{EntityName}Dto.cs`

```csharp
using Abp.Application.Services.Dto;
using System;
using System.ComponentModel.DataAnnotations;

namespace {Namespace}.{EntityName}s.Dto
{
    public class CreateUpdate{EntityName}Dto : EntityDto<{PkType}>
    {
        // One property per field with validation attributes from user answers
        // [Required] [MaxLength(N)] for strings
        // [Range(min, max)] for numerics
        // DateTime fields with = DateTime.Now default if appropriate
    }
}
```

> **Critical:** `CreateUpdate{EntityName}Dto` MUST extend `EntityDto<{PkType}>` — ABP requires the update DTO to implement `IEntityDto<{PkType}>`.

---

#### File F — Paged request DTO
**Path:** `{ApplicationProject}/{EntityName}s/Dto/Paged{EntityName}ResultRequestDto.cs`

```csharp
using Abp.Application.Services.Dto;

namespace {Namespace}.{EntityName}s.Dto
{
    public class Paged{EntityName}ResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}
```

Only add extra filter properties here if the user explicitly requested them AND the entity has those fields.

---

#### File G — AutoMapper profile
**Path:** `{ApplicationProject}/{EntityName}s/{EntityName}MapProfile.cs`

```csharp
using AutoMapper;
using {Namespace}.{EntityName}s.Dto;

namespace {Namespace}.{EntityName}s
{
    public class {EntityName}MapProfile : Profile
    {
        public {EntityName}MapProfile()
        {
            CreateMap<{EntityName}, {EntityName}Dto>();
            CreateMap<CreateUpdate{EntityName}Dto, {EntityName}>()
                .ForMember(x => x.Id, opt => opt.Ignore());
        }
    }
}
```

Do NOT use `[AutoMapFrom]` / `[AutoMapTo]` attributes on DTOs — the profile is the single source of mapping truth.

---

#### File H — Interface
**Path:** `{ApplicationProject}/{EntityName}s/I{EntityName}AppService.cs`

```csharp
using Abp.Application.Services;
using {Namespace}.{EntityName}s.Dto;

namespace {Namespace}.{EntityName}s
{
    public interface I{EntityName}AppService
        : IAsyncCrudAppService<{EntityName}Dto, {PkType}, Paged{EntityName}ResultRequestDto, CreateUpdate{EntityName}Dto, CreateUpdate{EntityName}Dto>
    {
    }
}
```

---

#### File I — App Service
**Path:** `{ApplicationProject}/{EntityName}s/{EntityName}AppService.cs`

```csharp
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using {Namespace}.Authorization;
using {Namespace}.{EntityName}s.Dto;
using System.Linq;

namespace {Namespace}.{EntityName}s
{
    [AbpAuthorize(PermissionNames.Pages_{EntityNames})]
    public class {EntityName}AppService
        : AsyncCrudAppService<{EntityName}, {EntityName}Dto, {PkType}, Paged{EntityName}ResultRequestDto, CreateUpdate{EntityName}Dto, CreateUpdate{EntityName}Dto>,
          I{EntityName}AppService
    {
        public {EntityName}AppService(IRepository<{EntityName}, {PkType}> repository) : base(repository)
        {
            CreatePermissionName = PermissionNames.Pages_{EntityNames}_Create;
            UpdatePermissionName = PermissionNames.Pages_{EntityNames}_Edit;
            DeletePermissionName = PermissionNames.Pages_{EntityNames}_Delete;
        }

        protected override IQueryable<{EntityName}> CreateFilteredQuery(Paged{EntityName}ResultRequestDto input)
        {
            return Repository.GetAll()
                .WhereIf(!string.IsNullOrEmpty(input.Keyword),
                    x => x.{FilterableField}.Contains(input.Keyword));
            // Add more .WhereIf() calls for additional filterable fields
        }

        protected override IQueryable<{EntityName}> ApplySorting(IQueryable<{EntityName}> query, Paged{EntityName}ResultRequestDto input)
        {
            return query.OrderByDescending(x => x.CreationTime);
            // Change sort field/direction based on user answer
        }
    }
}
```

> **Key rules:**
> - Use `Abp.Linq.Extensions` (NOT `Abp.Collections.Extensions`) for `WhereIf` — the Collections version returns `IEnumerable<T>` which breaks the return type.
> - Class and interface must be `public`.

---

#### File J — DbContext registration
**Edit:** `{EFProject}/EntityFrameworkCore/*DbContext.cs`

Add inside the class body:
```csharp
public DbSet<{EntityName}> {EntityNames} { get; set; }
```

Add the using if needed:
```csharp
using {Namespace}.{EntityName}s;
```

---

### Step 4 — Run EF Core migration

After writing all files, run:

```
dotnet ef migrations add Add{EntityNames}Table --project {EFProjectPath} --startup-project {WebHostProjectPath}
```

Discover `{WebHostProjectPath}` by finding `**/*.Web.Host.csproj` or `**/*.Web.csproj`.

If the migration command fails, show the error and ask the user how to proceed — do NOT retry blindly.

---

### Step 5 — Verify with a build

Run `dotnet build` from the solution root (the folder containing the `.sln` file). Report any errors and fix them before finishing.

---

### Step 6 — Summary

Print a checklist of everything created/modified:
- [ ] Entity: `{path}`
- [ ] PermissionNames: updated
- [ ] AuthorizationProvider: updated
- [ ] `{EntityName}Dto`
- [ ] `CreateUpdate{EntityName}Dto`
- [ ] `Paged{EntityName}ResultRequestDto`
- [ ] `{EntityName}MapProfile`
- [ ] `I{EntityName}AppService`
- [ ] `{EntityName}AppService`
- [ ] DbContext: `{EntityNames}` DbSet added
- [ ] Migration: `Add{EntityNames}Table`
- [ ] Build: passed

---

## Common Mistakes to Avoid

| Mistake | Fix |
|---|---|
| `CreateUpdate{EntityName}Dto` doesn't extend `EntityDto<T>` | ABP's `TUpdateInput` constraint requires `IEntityDto<T>` — always extend `EntityDto<{PkType}>` |
| Using `Abp.Collections.Extensions` for `WhereIf` | Returns `IEnumerable<T>` — use `Abp.Linq.Extensions` instead |
| `internal` class/interface | ABP DI won't register it — always use `public` |
| `[AutoMapFrom]`/`[AutoMapTo]` alongside a MapProfile | Duplicate mappings cause conflicts — pick one; prefer the Profile |
| `IsActive` filter when entity has no `IsActive` property | Only add filters for fields that actually exist on the entity |
| `const object Permissions;` leftover | Invalid C# — remove any uninitialized `const object` declarations |
| Forgetting `.ForMember(x => x.Id, opt => opt.Ignore())` on CreateUpdate map | AutoMapper will try to overwrite the entity Id on update |
