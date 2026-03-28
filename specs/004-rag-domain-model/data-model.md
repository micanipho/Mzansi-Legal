# Data Model: RAG Domain Model

**Branch**: `004-rag-domain-model` | **Date**: 2026-03-28

---

## Enumeration

### `DocumentDomain`

**File**: `backend.Core/Domains/LegalDocuments/DocumentDomain.cs`

| Value | Integer | Description |
|---|---|---|
| `Legal` | 1 | Legal legislation documents |
| `Financial` | 2 | Financial regulation documents |

```csharp
public enum DocumentDomain
{
    Legal = 1,
    Financial = 2
}
```

---

## Entities

### `Category` — Aggregate Root

**File**: `backend.Core/Domains/LegalDocuments/Category.cs`
**Table**: `Categories`

| Property | Type | Constraint | Notes |
|---|---|---|---|
| `Id` | `Guid` | PK (inherited) | From `FullAuditedEntity<Guid>` |
| `Name` | `string` | `[Required] [MaxLength(200)]` | Display name of the category |
| `Icon` | `string` | `[MaxLength(100)]` | Icon identifier string |
| `Domain` | `DocumentDomain` | `[Required]` | Legal or Financial classification |
| `SortOrder` | `int` | — | Display ordering |
| `LegalDocuments` | `ICollection<LegalDocument>` | Navigation | One-to-many; ABP lazy-load |

**Audit columns (inherited)**: `CreationTime`, `CreatorUserId`, `LastModificationTime`, `LastModifierUserId`, `IsDeleted`, `DeletionTime`, `TenantId`

---

### `LegalDocument` — Aggregate Root

**File**: `backend.Core/Domains/LegalDocuments/LegalDocument.cs`
**Table**: `LegalDocuments`

| Property | Type | Constraint | Notes |
|---|---|---|---|
| `Id` | `Guid` | PK (inherited) | — |
| `Title` | `string` | `[Required] [MaxLength(500)]` | Full act title |
| `ShortName` | `string` | `[MaxLength(100)]` | Abbreviated name |
| `ActNumber` | `string` | `[MaxLength(50)]` | Official act number |
| `Year` | `int` | `[Required]` | Year of enactment |
| `FullText` | `string` | — | Full document text (unbounded) |
| `FileName` | `string` | `[MaxLength(300)]` | Original PDF file name |
| `OriginalPdfId` | `Guid?` | Nullable FK | FK to ABP BinaryObject (StoredFile) |
| `CategoryId` | `Guid` | `[Required]` FK → `Category` | |
| `Category` | `Category` | Navigation | Many-to-one |
| `IsProcessed` | `bool` | Default `false` | Whether chunking is complete |
| `TotalChunks` | `int` | Default `0` | Count of generated chunks |
| `Chunks` | `ICollection<DocumentChunk>` | Navigation | One-to-many, cascade delete |

**Indexes**:
- Unique: `(ActNumber, Year)` — prevent duplicate act registrations

---

### `DocumentChunk` — Child Entity

**File**: `backend.Core/Domains/LegalDocuments/DocumentChunk.cs`
**Table**: `DocumentChunks`

| Property | Type | Constraint | Notes |
|---|---|---|---|
| `Id` | `Guid` | PK (inherited) | — |
| `DocumentId` | `Guid` | `[Required]` FK → `LegalDocument` | |
| `Document` | `LegalDocument` | Navigation | Many-to-one |
| `ChapterTitle` | `string` | `[MaxLength(500)]` | Chapter heading |
| `SectionNumber` | `string` | `[MaxLength(50)]` | e.g., "§ 12(3)" |
| `SectionTitle` | `string` | `[MaxLength(500)]` | Section heading |
| `Content` | `string` | `[Required]` | Chunk text body |
| `TokenCount` | `int` | — | Token count for budget-aware retrieval |
| `SortOrder` | `int` | — | Reading-sequence position |
| `Embedding` | `ChunkEmbedding` | Navigation | One-to-one, cascade delete |

**Indexes**:
- Composite: `(DocumentId, SortOrder)` — ordered chunk retrieval

---

### `ChunkEmbedding` — Child Entity

**File**: `backend.Core/Domains/LegalDocuments/ChunkEmbedding.cs`
**Table**: `ChunkEmbeddings`

| Property | Type | Constraint | Notes |
|---|---|---|---|
| `Id` | `Guid` | PK (inherited) | — |
| `ChunkId` | `Guid` | `[Required]` FK → `DocumentChunk` | |
| `Chunk` | `DocumentChunk` | Navigation | One-to-one inverse |
| `Vector` | `float[]` | Length must equal 1536 | Mapped to PostgreSQL `real[]` |

**Storage note**: Npgsql maps `float[]` natively to `real[]`. The 1 536-element length is validated at the application service boundary, not at the database column level (PostgreSQL arrays are variable-length by default).

---

## Relationship Diagram

```
Category (1)
  └── LegalDocument (N)   [CategoryId FK, cascade: restrict]
        └── DocumentChunk (N)   [DocumentId FK, cascade: delete]
              └── ChunkEmbedding (1)   [ChunkId FK, cascade: delete]
```

---

## DbSet Registrations

Add to `backendDbContext.cs`:

```csharp
public DbSet<Category> Categories { get; set; }
public DbSet<LegalDocument> LegalDocuments { get; set; }
public DbSet<DocumentChunk> DocumentChunks { get; set; }
public DbSet<ChunkEmbedding> ChunkEmbeddings { get; set; }
```

---

## Fluent API Configuration (`OnModelCreating`)

```csharp
// LegalDocument → Category: restrict delete (categories should not cascade-delete documents)
modelBuilder.Entity<LegalDocument>()
    .HasOne(d => d.Category)
    .WithMany(c => c.LegalDocuments)
    .HasForeignKey(d => d.CategoryId)
    .OnDelete(DeleteBehavior.Restrict);

// LegalDocument → DocumentChunk: cascade delete
modelBuilder.Entity<DocumentChunk>()
    .HasOne(c => c.Document)
    .WithMany(d => d.Chunks)
    .HasForeignKey(c => c.DocumentId)
    .OnDelete(DeleteBehavior.Cascade);

// DocumentChunk → ChunkEmbedding: cascade delete (one-to-one)
modelBuilder.Entity<ChunkEmbedding>()
    .HasOne(e => e.Chunk)
    .WithOne(c => c.Embedding)
    .HasForeignKey<ChunkEmbedding>(e => e.ChunkId)
    .OnDelete(DeleteBehavior.Cascade);

// Unique index: prevent duplicate acts
modelBuilder.Entity<LegalDocument>()
    .HasIndex(d => new { d.ActNumber, d.Year })
    .IsUnique();

// Performance index: ordered chunk retrieval
modelBuilder.Entity<DocumentChunk>()
    .HasIndex(c => new { c.DocumentId, c.SortOrder });
```

---

## Validation Rules

| Rule | Enforcement Point |
|---|---|
| `Vector.Length == 1536` | Application service guard clause before insert |
| `ActNumber + Year` uniqueness | EF unique index (DB-level) |
| `CategoryId` must reference existing Category | EF FK constraint |
| `DocumentId` must reference existing LegalDocument | EF FK constraint |
| `ChunkId` must reference existing DocumentChunk | EF FK constraint |
| `IsProcessed` defaults to `false` | Entity constructor / C# initializer |
| `TotalChunks` defaults to `0` | Entity constructor / C# initializer |
