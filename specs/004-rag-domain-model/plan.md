# Implementation Plan: RAG Domain Model

**Branch**: `004-rag-domain-model` | **Date**: 2026-03-28 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/004-rag-domain-model/spec.md`

## Summary

Define and persist the four core domain entities — `Category`, `LegalDocument`, `DocumentChunk`, and `ChunkEmbedding` — needed to store legislation and support a RAG retrieval pipeline. The approach follows ABP Zero's layered architecture: entities in `backend.Core`, DbSets and EF mappings in `backend.EntityFrameworkCore`, and CRUD application services in `backend.Application`.

## Technical Context

**Language/Version**: C# on .NET 9.0
**Primary Dependencies**: ABP Zero 10.x, Entity Framework Core 9.0.5, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.x
**Storage**: PostgreSQL 15+ via Npgsql; float vectors stored as `real[]` (PostgreSQL array type)
**Testing**: xUnit via ABP test helpers; integration tests against real PostgreSQL instance
**Target Platform**: ASP.NET Core web service hosted on Linux/Docker
**Project Type**: web-service (REST API, ABP Zero layered monolith)
**Performance Goals**: Standard CRUD response times; bulk vector insert to be optimized in pipeline feature
**Constraints**: No pgvector extension at MVP — cosine similarity is in-memory; vector dimension fixed at 1536
**Scale/Scope**: Hundreds of legislation documents, thousands of chunks, thousands of embeddings for MVP

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Layer Gate**: All entities placed in `backend.Core/Domains/LegalDocuments/`; DbSets in `backend.EntityFrameworkCore`; services and DTOs in `backend.Application` — correct per `docs/BACKEND_STRUCTURE.md`
- [x] **Naming Gate**: `CategoryAppService`, `ICategoryAppService`, `CategoryDto`; `LegalDocumentAppService`, `ILegalDocumentAppService`, `LegalDocumentDto` — all follow `{Entity}AppService` / `I{Entity}AppService` / `{Entity}Dto` conventions
- [x] **Coding Standards Gate**: Entities are small, single-responsibility classes well within 350-line limit; guard clause for vector length enforced at service boundary using `Ardalis.GuardClauses`; no magic numbers (1536 stored as `const int EmbeddingDimension = 1536`)
- [x] **Skill Gate**: `add-endpoint` skill identified for scaffolding Category and LegalDocument CRUD services; no other applicable skills for pure domain/EF work
- [x] **Multilingual Gate**: No user-facing outputs in this feature — domain model and migration only; N/A for this feature
- [x] **Citation Gate**: No AI-facing endpoints in this feature; N/A
- [x] **Accessibility Gate**: No frontend components in this feature; N/A

## Project Structure

### Documentation (this feature)

```text
specs/004-rag-domain-model/
├── plan.md              ← this file
├── research.md          ← Phase 0 output
├── data-model.md        ← Phase 1 output
├── quickstart.md        ← Phase 1 output
├── contracts/
│   └── api-contracts.md ← Phase 1 output
└── tasks.md             ← Phase 2 output (/speckit.tasks — NOT created here)
```

### Source Code

```text
backend/
├── src/
│   ├── backend.Core/
│   │   └── Domains/
│   │       └── LegalDocuments/         ← NEW: all domain entities for this feature
│   │           ├── DocumentDomain.cs
│   │           ├── Category.cs
│   │           ├── LegalDocument.cs
│   │           ├── DocumentChunk.cs
│   │           └── ChunkEmbedding.cs
│   ├── backend.Application/
│   │   └── Services/
│   │       ├── CategoryService/        ← NEW
│   │       │   ├── ICategoryAppService.cs
│   │       │   ├── CategoryAppService.cs
│   │       │   └── DTO/
│   │       │       └── CategoryDto.cs
│   │       └── LegalDocumentService/   ← NEW
│   │           ├── ILegalDocumentAppService.cs
│   │           ├── LegalDocumentAppService.cs
│   │           └── DTO/
│   │               ├── LegalDocumentDto.cs
│   │               └── LegalDocumentListDto.cs
│   └── backend.EntityFrameworkCore/
│       └── EntityFrameworkCore/
│           ├── backendDbContext.cs      ← MODIFIED: add 4 DbSets + Fluent API
│           └── Migrations/             ← NEW: AddRagDomainModel migration
└── test/
    └── backend.Tests/                  ← NEW: integration tests for entities
```

**Structure Decision**: Web application (Option 2) — backend-only work for this feature; existing `backend/` project layout used.

---

## Implementation Steps

### Step 1 — Define Domain Enumeration

**Action**:
- Create `backend.Core/Domains/LegalDocuments/DocumentDomain.cs`
- Define `enum DocumentDomain { Legal = 1, Financial = 2 }`
- Add XML doc comment describing the enum purpose

**Expected Result**: Enum compiles and is accessible from all layers (Core has no outward dependencies).

**Validation**: `dotnet build backend/src/backend.Core` succeeds.

---

### Step 2 — Create Category Entity

**Action**:
- Create `backend.Core/Domains/LegalDocuments/Category.cs`
- Extend `FullAuditedEntity<Guid>`
- Add properties: `Name [Required, MaxLength(200)]`, `Icon [MaxLength(100)]`, `Domain [Required]`, `SortOrder`
- Add navigation: `ICollection<LegalDocument> LegalDocuments`
- Add class-level XML doc comment

**Expected Result**: `Category` aggregate root compiles with no EF or HTTP dependencies.

**Validation**: `dotnet build backend/src/backend.Core` succeeds.

---

### Step 3 — Create LegalDocument Entity

**Action**:
- Create `backend.Core/Domains/LegalDocuments/LegalDocument.cs`
- Extend `FullAuditedEntity<Guid>`
- Add all properties per [data-model.md](./data-model.md)
- Initialize defaults: `IsProcessed = false`, `TotalChunks = 0` in property initializers
- Add FK `CategoryId` with `[ForeignKey]` navigation to `Category`
- Add navigation: `ICollection<DocumentChunk> Chunks`

**Expected Result**: `LegalDocument` compiles; FK to `Category` correctly defined.

**Validation**: `dotnet build backend/src/backend.Core` succeeds.

---

### Step 4 — Create DocumentChunk Entity

**Action**:
- Create `backend.Core/Domains/LegalDocuments/DocumentChunk.cs`
- Extend `FullAuditedEntity<Guid>`
- Add all properties per data-model
- Add FK `DocumentId` with navigation to `LegalDocument`
- Add navigation: `ChunkEmbedding Embedding`

**Expected Result**: `DocumentChunk` compiles; PartOf `LegalDocument` correctly expressed.

**Validation**: `dotnet build backend/src/backend.Core` succeeds.

---

### Step 5 — Create ChunkEmbedding Entity

**Action**:
- Create `backend.Core/Domains/LegalDocuments/ChunkEmbedding.cs`
- Extend `FullAuditedEntity<Guid>`
- Add property `Vector float[]`
- Add constant: `public const int EmbeddingDimension = 1536;`
- Add FK `ChunkId` with navigation to `DocumentChunk`

**Expected Result**: `ChunkEmbedding` compiles; `float[]` ready for Npgsql `real[]` mapping.

**Validation**: `dotnet build backend/src/backend.Core` succeeds.

---

### Step 6 — Register DbSets in backendDbContext

**Action**:
- Open `backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs`
- Add four `DbSet<T>` properties: `Categories`, `LegalDocuments`, `DocumentChunks`, `ChunkEmbeddings`

**Expected Result**: EF Core is aware of all four entities.

**Validation**: `dotnet build backend/src/backend.EntityFrameworkCore` succeeds.

---

### Step 7 — Add Fluent API Configuration

**Action**:
- In `backendDbContext.OnModelCreating`, add configurations from [data-model.md](./data-model.md#fluent-api-configuration-onmodelcreating):
  - LegalDocument → Category: `OnDelete(DeleteBehavior.Restrict)`
  - DocumentChunk → LegalDocument: `OnDelete(DeleteBehavior.Cascade)`
  - ChunkEmbedding → DocumentChunk: one-to-one, `OnDelete(DeleteBehavior.Cascade)`
  - Unique index: `LegalDocument(ActNumber, Year)`
  - Composite index: `DocumentChunk(DocumentId, SortOrder)`

**Expected Result**: Schema constraints and cascade rules correctly expressed.

**Validation**: `dotnet build` succeeds; no EF mapping warnings.

---

### Step 8 — Scaffold Application Services

**Action** (use `add-endpoint` skill for each):
- `CategoryAppService` — extends `AsyncCrudAppService<Category, CategoryDto, Guid>`
- `LegalDocumentAppService` — extends `AsyncCrudAppService<LegalDocument, LegalDocumentDto, Guid>` with override for list query to exclude `FullText`
- Add `[AbpAuthorize]` to both service classes
- `CategoryDto` — `[AutoMap(typeof(Category))]`, flat properties
- `LegalDocumentDto` — includes `FullText`; `LegalDocumentListDto` — excludes `FullText`
- Add guard clause in `LegalDocumentAppService.CreateAsync`: validate `Vector.Length == ChunkEmbedding.EmbeddingDimension` when embedding is created

**Expected Result**: Services compile and are auto-exposed by ABP as REST endpoints.

**Validation**: `dotnet build backend/src/backend.Application` succeeds.

---

### Step 9 — Generate EF Core Migration

**Action**:
```bash
cd backend
dotnet ef migrations add AddRagDomainModel \
  --project src/backend.EntityFrameworkCore \
  --startup-project src/backend.Web.Host
```

**Expected Result**: Migration file created in `backend.EntityFrameworkCore/EntityFrameworkCore/Migrations/`.

**Validation**:
- Migration file exists and is non-empty
- `dotnet build` succeeds after migration generation

---

### Step 10 — Apply Migration to PostgreSQL

**Action**:
```bash
dotnet ef database update \
  --project src/backend.EntityFrameworkCore \
  --startup-project src/backend.Web.Host
```

**Expected Result**: PostgreSQL contains tables `Categories`, `LegalDocuments`, `DocumentChunks`, `ChunkEmbeddings`.

**Validation**:
```sql
\dt  -- all four tables visible
\d "ChunkEmbeddings"  -- Vector column is of type real[]
\d "LegalDocuments"   -- unique index on ActNumber + Year visible
\d "DocumentChunks"   -- composite index on DocumentId + SortOrder visible
```

---

### Step 11 — Integration Test: Seed and Query

**Action**:
- Write xUnit integration test in `backend.Tests`:
  1. Insert 1 Category (Domain = Legal)
  2. Insert 1 LegalDocument linked to the Category
  3. Insert 5 DocumentChunks linked to the LegalDocument, with SortOrder 1–5
  4. Insert 5 ChunkEmbeddings, each with `new float[1536]` (all zeros acceptable for structural test)
  5. Assert chunks returned in SortOrder sequence
  6. Assert referential integrity: deleting LegalDocument cascades to DocumentChunks and ChunkEmbeddings

**Expected Result**: All assertions pass; cascade delete verified.

**Validation**: `dotnet test backend/test/backend.Tests` passes.

---

## Dependencies & Order

```
Step 1 (enum)
  → Step 2 (Category)
    → Step 3 (LegalDocument)
      → Step 4 (DocumentChunk)
        → Step 5 (ChunkEmbedding)
          → Step 6 (DbSets)
            → Step 7 (Fluent API)
              → Step 8 (Services) [parallel to 6–7]
              → Step 9 (Migration)
                → Step 10 (DB Update)
                  → Step 11 (Tests)
```

---

## Failure Handling

| Failure | Diagnosis | Resolution |
|---|---|---|
| DB connection error on migration | Check `ConnectionStrings:Default` in `appsettings.json` of `backend.Web.Host` | Verify PostgreSQL service is running and credentials are correct |
| Migration error: FK constraint | Check `OnDelete` behavior on the failing relationship | Ensure Restrict/Cascade is set correctly per data-model.md |
| `real[]` column not created | Npgsql version mismatch | Ensure `Npgsql.EntityFrameworkCore.PostgreSQL` ≥ 8.x in `backend.EntityFrameworkCore.csproj` |
| Unique index violation | Duplicate ActNumber + Year in test data | Use distinct Act numbers per test record |
| Cascade delete not working | Missing `.OnDelete(DeleteBehavior.Cascade)` in Fluent API | Re-check Step 7 configuration and regenerate migration |
| Vector length mismatch | Guard clause missing or not triggered | Verify guard clause in service CreateAsync before persist |

---

## Complexity Tracking

No constitution violations. All gates pass. Standard layered architecture applied.
