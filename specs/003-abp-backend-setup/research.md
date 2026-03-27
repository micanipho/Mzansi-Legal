# Research: ABP Backend Foundation — PostgreSQL Migration

**Feature**: 003-abp-backend-setup
**Phase**: 0 — Outline & Research

---

## Decision 1: Npgsql Package Version

**Decision**: Use `Npgsql.EntityFrameworkCore.PostgreSQL` version `9.0.x` (matching `net9.0` target framework).

**Rationale**: The project targets `net9.0` and already uses `Microsoft.EntityFrameworkCore` `9.0.5`.
The Npgsql EF Core provider follows the same major.minor versioning as EF Core itself.
`Npgsql.EntityFrameworkCore.PostgreSQL` `9.0.x` is the correct match.

**Alternatives considered**:
- `8.0.x`: Mismatches the net9.0/EF Core 9 target — would cause package conflicts.
- Installing pgvector extension now: Out of scope for initial setup; can be added in the RAG pipeline feature.

---

## Decision 2: Migration Strategy — Drop and Regenerate

**Decision**: Delete all existing migration files and the `backendDbContextModelSnapshot.cs`, then
regenerate a single fresh `InitialCreate` migration using the Npgsql provider.

**Rationale**: The existing migration history spans 2017–2025 and contains SQL Server-specific
column types (`nvarchar`, `datetime2`, `bit`), SQL Server identity column extensions
(`SqlServerModelBuilderExtensions.UseIdentityColumns`, `SqlServerPropertyBuilderExtensions.UseIdentityColumn`),
and SQL Server DDL syntax in `Up()`/`Down()` methods. These cannot simply be rewritten for
PostgreSQL — the EF tooling generates provider-specific SQL and metadata automatically.
Since this is a greenfield setup with no production data to preserve, regenerating from scratch
is the safest and cleanest approach.

**Alternatives considered**:
- Manual in-place replacement of SQL Server types with PostgreSQL equivalents: High risk of subtle
  type mismatches (e.g., `nvarchar(max)` → `text`, `datetime2` → `timestamp with time zone`).
  The generated snapshot would still have stale `SqlServer*` extension method calls that will
  not compile against the Npgsql provider. Not recommended.
- Keeping old migrations and creating a new delta migration: Fails because the old migration
  `Up()` SQL is SQL Server-specific DDL and will not execute on PostgreSQL.

---

## Decision 3: Connection String Format

**Decision**: Replace the SQL Server connection strings in `appsettings.json` (Web.Host and
Migrator) with standard PostgreSQL / Npgsql format:

```
Host=localhost;Database=MzansiLegalDb;Username=postgres;Password=<password>
```

**Rationale**: Npgsql uses its own connection string format. SQL Server `Trusted_Connection` and
`TrustServerCertificate` attributes are not understood by Npgsql. Keeping them would cause a
connection failure at startup.

---

## Decision 4: `backendDbContextConfigurer.cs` — UseNpgsql

**Decision**: Replace both `builder.UseSqlServer(...)` calls with `builder.UseNpgsql(...)`.
Remove the `using System.Data.Common` reference that is no longer needed after the change, only
if it becomes unused.

**Rationale**: `UseNpgsql` is the Npgsql EF Core extension method that registers the PostgreSQL
provider. It has the same signature (accepts both a connection string and a `DbConnection`), so
the change is a direct drop-in replacement.

---

## Decision 5: DbContextModelSnapshot — Regenerated Automatically

**Decision**: Delete `backendDbContextModelSnapshot.cs`. It will be regenerated automatically
by `dotnet ef migrations add InitialCreate` with PostgreSQL-specific type annotations
(e.g., `NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn` instead of
`SqlServerPropertyBuilderExtensions.UseIdentityColumn`).

**Rationale**: The snapshot is an auto-generated file and MUST NOT be hand-edited. Regenerating
it via the EF tooling ensures all Npgsql annotations are correctly produced.

---

## Decision 6: No `OnModelCreating` Override Required for Initial Setup

**Decision**: No custom `OnModelCreating` configuration is needed for the initial setup beyond
the base ABP Zero `DbContext`.

**Rationale**: ABP Zero handles its own entity configuration internally. UTC datetime conversion
and other PostgreSQL-specific model configurations (e.g., `HasPostgresExtension("uuid-ossp")`)
can be added in a later feature when UUID primary keys or time zone handling is explicitly tested.
