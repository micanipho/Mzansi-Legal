# Quickstart: RAG Domain Model

**Branch**: `004-rag-domain-model` | **Date**: 2026-03-28

This guide shows the minimum steps to implement and validate the domain model from scratch.

---

## Prerequisites

- .NET 9 SDK installed
- PostgreSQL 15+ running locally
- `appsettings.json` in `backend/src/backend.Web.Host/` has a valid `ConnectionStrings:Default` pointing to your PostgreSQL instance
- You are on branch `004-rag-domain-model`

---

## Step 1 — Create the Domain Enumeration

Create file:
```
backend/src/backend.Core/Domains/LegalDocuments/DocumentDomain.cs
```

```csharp
/// <summary>
/// Classifies a category as belonging to either the legal or financial domain.
/// </summary>
public enum DocumentDomain
{
    Legal = 1,
    Financial = 2
}
```

---

## Step 2 — Create Domain Entities

Create these files under `backend/src/backend.Core/Domains/LegalDocuments/`:

- `Category.cs`
- `LegalDocument.cs`
- `DocumentChunk.cs`
- `ChunkEmbedding.cs`

Full implementations are defined in [data-model.md](./data-model.md).

---

## Step 3 — Register DbSets

Add to `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs`:

```csharp
public DbSet<Category> Categories { get; set; }
public DbSet<LegalDocument> LegalDocuments { get; set; }
public DbSet<DocumentChunk> DocumentChunks { get; set; }
public DbSet<ChunkEmbedding> ChunkEmbeddings { get; set; }
```

---

## Step 4 — Add Fluent API Configuration

In `backendDbContext.OnModelCreating`, add the relationship and index configurations from [data-model.md → Fluent API Configuration](./data-model.md#fluent-api-configuration-onmodelcreating).

---

## Step 5 — Build the Solution

```bash
cd backend
dotnet build
```

Resolve any compilation errors before proceeding.

---

## Step 6 — Generate EF Migration

```bash
cd backend
dotnet ef migrations add AddRagDomainModel \
  --project src/backend.EntityFrameworkCore \
  --startup-project src/backend.Web.Host
```

Verify the generated migration file in:
```
backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/Migrations/
```

---

## Step 7 — Apply Migration to PostgreSQL

```bash
dotnet ef database update \
  --project src/backend.EntityFrameworkCore \
  --startup-project src/backend.Web.Host
```

Confirm tables exist:
```sql
\dt -- in psql: should list Categories, LegalDocuments, DocumentChunks, ChunkEmbeddings
```

---

## Step 8 — Scaffold Application Services

Use the `add-endpoint` skill to scaffold:
1. `ICategoryAppService` + `CategoryAppService` + `CategoryDto`
2. `ILegalDocumentAppService` + `LegalDocumentAppService` + `LegalDocumentDto` + `LegalDocumentListDto`

See [api-contracts.md](./contracts/api-contracts.md) for the expected DTO shapes.

---

## Step 9 — Verify via Swagger

Run the host:
```bash
cd backend
dotnet run --project src/backend.Web.Host
```

Open `https://localhost:44311/swagger`. Authenticate via `POST /api/TokenAuth/Authenticate`, then test:

1. Create a Category (Domain = 1 for Legal)
2. Create a LegalDocument referencing the CategoryId
3. Verify GET returns both records

---

## Step 10 — Seed Test Data (Manual Verification)

Insert via Swagger or direct SQL:

```sql
-- Verify all 4 tables are correct
SELECT COUNT(*) FROM "Categories";
SELECT COUNT(*) FROM "LegalDocuments";
SELECT COUNT(*) FROM "DocumentChunks";
SELECT COUNT(*) FROM "ChunkEmbeddings";
```

For embedding vector verification:
```sql
-- Check vector is stored as real[]
SELECT pg_typeof("Vector") FROM "ChunkEmbeddings" LIMIT 1;
-- Expected: real[]
SELECT array_length("Vector", 1) FROM "ChunkEmbeddings" LIMIT 1;
-- Expected: 1536
```

---

## Troubleshooting

| Problem | Solution |
|---|---|
| Migration fails: "table already exists" | Check if a previous partial migration was applied; roll back or delete the migration file |
| `real[]` column not created | Ensure Npgsql version ≥ 8.x in `backend.EntityFrameworkCore.csproj` |
| FK constraint error on Category delete | Expected — Categories with documents cannot be deleted (restrict behavior) |
| Vector array_length ≠ 1536 | Application service guard clause is not enforcing length; check service validation |
