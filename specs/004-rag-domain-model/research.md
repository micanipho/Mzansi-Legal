# Research: RAG Domain Model

**Branch**: `004-rag-domain-model` | **Date**: 2026-03-28

---

## 1. Float Vector Storage in EF Core + Npgsql (PostgreSQL)

**Decision**: Store `ChunkEmbedding.Vector` as `float[]` (C# array) mapped to PostgreSQL `real[]`.

**Rationale**: Npgsql natively maps `float[]` to PostgreSQL `real[]` without additional configuration. No extension required. The column holds exactly 1 536 elements matching OpenAI `text-embedding-ada-002` and compatible models. For MVP, cosine similarity is computed in-memory — constitution explicitly permits this. Upgrading to `pgvector` later requires only a column type migration.

**Alternatives Considered**:
- `pgvector` extension with `Vector` type: better for server-side similarity search; deferred until MVP is stable (constitution preference).
- JSON string: rejected by spec — poor query performance, no native array semantics.
- Storing on `DocumentChunk` directly: rejected by spec — separation of concerns and independent scalability.

**How to configure**: Npgsql requires no special Fluent API for `float[]`. The column is auto-created as `real[]`. A `CHECK` constraint or application-level validation enforces the 1 536-element length.

---

## 2. ABP Entity Base Class for All Entities

**Decision**: All four entities (`Category`, `LegalDocument`, `DocumentChunk`, `ChunkEmbedding`) extend `FullAuditedEntity<Guid>`.

**Rationale**: Mandatory per constitution (`docs/BACKEND_STRUCTURE.md` and Principle IV). Provides `CreationTime`, `CreatorUserId`, `LastModificationTime`, `IsDeleted`, `DeletionTime` automatically. Guid primary keys avoid sequential ID exposure.

**Alternatives Considered**: `Entity<Guid>` (no audit fields) — rejected; constitution requires full audit trail.

---

## 3. Domain Enumeration Pattern (DocumentDomain)

**Decision**: Implement as a C# `enum` with integer backing stored as `int` in PostgreSQL. Name the enum `DocumentDomain` to avoid collision with the .NET `System.Domain` namespace.

**Rationale**: ABP RefList pattern uses integer-backed enums. Stored as `int` — compact, indexed efficiently, and directly comparable. Display names are handled by ABP's localization layer in the Application layer, not in the entity.

**Alternatives Considered**:
- String column: Poor index performance and type safety.
- Separate lookup table: Over-engineering for a 2-value enum.

---

## 4. Aggregate Root Boundaries

**Decision**:
- `Category` — aggregate root, owns its lifecycle independently.
- `LegalDocument` — aggregate root, owns `DocumentChunk` and `ChunkEmbedding` as child entities.
- `DocumentChunk` — child entity of `LegalDocument`.
- `ChunkEmbedding` — child entity of `DocumentChunk`.

**Rationale**: Category is referenced by LegalDocument (FK relationship) but is an independent aggregate since categories exist independently. LegalDocument controls the lifecycle of its chunks and embeddings — deleting a document should cascade-delete all chunks and embeddings.

**ABP repository scope**: ABP generates `IRepository<Category, Guid>` and `IRepository<LegalDocument, Guid>` as aggregate root repositories. `DocumentChunk` and `ChunkEmbedding` are accessed via their parent's navigation collections or via `IRepository<DocumentChunk, Guid>` directly (ABP registers all `FullAuditedEntity` subclasses by default).

---

## 5. StoredFile Pattern for OriginalPdf

**Decision**: Store `OriginalPdfId` as a nullable `Guid` (FK to ABP's `BinaryObject` table) and `FileName` as a plain string. No direct navigation property to `BinaryObject` in the domain entity.

**Rationale**: ABP Zero's `StoredFile` / `BinaryObject` is an infrastructure concern managed by `IBinaryObjectManager`. Storing the GUID reference in the domain entity maintains clean architecture — the domain entity does not reference the file storage implementation. Actual file upload and retrieval is deferred to a later feature.

---

## 6. EF Core Fluent API Requirements

**Decision**: Use `OnModelCreating` overrides in `backendDbContext` for configuration that cannot be expressed with data annotations:

| Configuration | Entity | Reason |
|---|---|---|
| `HasMany(d => d.Chunks).WithOne(c => c.Document).OnDelete(DeleteBehavior.Cascade)` | LegalDocument → DocumentChunk | Cascade delete for owned children |
| `HasOne(c => c.Embedding).WithOne(e => e.Chunk).OnDelete(DeleteBehavior.Cascade)` | DocumentChunk → ChunkEmbedding | Cascade delete for embedding |
| `HasIndex(d => new { d.ActNumber, d.Year }).IsUnique()` | LegalDocument | Prevent duplicate acts |
| `HasIndex(c => new { c.DocumentId, c.SortOrder })` | DocumentChunk | Ordered retrieval performance |
| `HasColumnType("real[]")` (optional explicit) | ChunkEmbedding.Vector | Document intent; Npgsql infers this |

---

## 7. Migration Command

**Decision**: Run migrations from the solution root using startup project flag.

```bash
cd backend
dotnet ef migrations add AddRagDomainModel \
  --project src/backend.EntityFrameworkCore \
  --startup-project src/backend.Web.Host

dotnet ef database update \
  --project src/backend.EntityFrameworkCore \
  --startup-project src/backend.Web.Host
```

**Note**: `backend.Web.Host` must have a valid `appsettings.json` with a working PostgreSQL connection string before running `database update`.

---

## 8. Application Service Scope for This Feature

**Decision**: Scaffold minimal CRUD application services for `Category` and `LegalDocument` only. `DocumentChunk` and `ChunkEmbedding` are managed by pipeline services (future feature) and do not need public CRUD endpoints at this stage.

**Rationale**: The spec's acceptance criteria require only that data "can be inserted and queried successfully" — not a full admin UI. Services are needed to satisfy ABP's `IRepository` injection pattern and to enable Swagger testing.

**Skill to use**: `add-endpoint` skill for scaffolding each service.
