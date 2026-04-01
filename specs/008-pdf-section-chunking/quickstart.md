# Quickstart: PDF Section Chunking Ingestion Service

**Feature**: 008-pdf-section-chunking
**Date**: 2026-03-28

---

## Prerequisites

- .NET 9 SDK installed
- Docker running with the `mzansi-pg` PostgreSQL container
- Working `backend` solution (existing `InitialCreate` + `AddContractAnalysisDomain` migrations applied)

---

## Step 1 — Add PdfPig NuGet Package

```bash
cd backend/src/backend.Application
dotnet add package UglyToad.PdfPig
```

Verify the package is listed in `backend.Application.csproj`.

---

## Step 2 — Add New Domain Files

Create the following files (see `data-model.md` for full content):

```
backend/src/backend.Core/Domains/LegalDocuments/ChunkStrategy.cs   ← ChunkStrategy enum
backend/src/backend.Core/Domains/ETL/IngestionStatus.cs             ← IngestionStatus enum
backend/src/backend.Core/Domains/ETL/IngestionJob.cs                ← IngestionJob entity
```

---

## Step 3 — Extend DocumentChunk Entity

Add `ChunkStrategy` property to `backend/src/backend.Core/Domains/LegalDocuments/DocumentChunk.cs`:

```csharp
/// <summary>
/// Strategy used to produce this chunk. Null for chunks ingested before this feature was added.
/// </summary>
public ChunkStrategy? ChunkStrategy { get; set; }
```

---

## Step 4 — Register IngestionJob in DbContext

Add to `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs`:

```csharp
// ── ETL domain ──────────────────────────────────────────────────────────
/// <summary>Ingestion pipeline jobs tracking each stage of document processing.</summary>
public DbSet<IngestionJob> IngestionJobs { get; set; }
```

And add a configuration method call in `OnModelCreating`:

```csharp
ConfigureIngestionJobRelationships(modelBuilder);
```

With implementation:

```csharp
private static void ConfigureIngestionJobRelationships(ModelBuilder modelBuilder)
{
    // Job records are audit evidence; restrict document deletion while jobs exist.
    modelBuilder.Entity<IngestionJob>()
        .HasOne<LegalDocument>()
        .WithMany()
        .HasForeignKey(j => j.DocumentId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<IngestionJob>()
        .HasIndex(j => j.DocumentId);

    modelBuilder.Entity<IngestionJob>()
        .HasIndex(j => j.Status);
}
```

---

## Step 5 — Create and Run Migration

```bash
cd backend
dotnet ef migrations add AddPdfIngestionEntities \
    --project src/backend.EntityFrameworkCore \
    --startup-project src/backend.Web.Host

dotnet ef database update \
    --project src/backend.EntityFrameworkCore \
    --startup-project src/backend.Web.Host
```

Verify migration created `IngestionJobs` table and `ChunkStrategy` column on `DocumentChunks`.

---

## Step 6 — Create the Application Service

Create service folder and files:

```
backend/src/backend.Application/Services/PdfIngestionService/
    IPdfIngestionAppService.cs
    PdfIngestionAppService.cs
    DTO/
        IngestPdfRequest.cs
        DocumentChunkResult.cs
```

See `contracts/pdf-ingestion-service.md` for full interface and DTO specifications.

---

## Step 7 — Register the Service (ABP Module)

ABP Zero auto-registers services by convention — no manual registration is required as long as `PdfIngestionAppService` follows the naming convention and lives in the `backend.Application` assembly.

Verify the service appears in Swagger under `/api/services/app/` after build (if a controller is added later).

---

## Step 8 — Run Tests

```bash
cd backend
dotnet test test/backend.Tests
```

Verify:
- `PdfIngestionServiceTests` all pass
- Section-level chunking test with synthetic SA legislation text returns ≥3 chunks
- Fixed-size fallback test with plain prose returns chunks of ≤2000 characters
- Empty stream test returns empty list with no exception

---

## Quick Smoke Test (Manual)

To manually verify the service before full integration, call it from `LegalDocumentAppService` or a test controller:

```csharp
// Create a job first
var job = new IngestionJob { DocumentId = document.Id, Status = IngestionStatus.Queued };
await _ingestionJobRepository.InsertAsync(job);

// Call the ingestion service
using var stream = File.OpenRead("test-lra.pdf");
var request = new IngestPdfRequest
{
    PdfStream      = stream,
    ActName        = "Labour Relations Act",
    DocumentId     = document.Id,
    IngestionJobId = job.Id
};

var chunks = await _pdfIngestionAppService.IngestAsync(request);

// Persist chunks
foreach (var chunk in chunks)
{
    await _chunkRepository.InsertAsync(new DocumentChunk
    {
        DocumentId     = request.DocumentId,
        ChapterTitle   = chunk.ChapterTitle,
        SectionNumber  = chunk.SectionNumber,
        SectionTitle   = chunk.SectionTitle,
        Content        = chunk.Content,
        TokenCount     = chunk.TokenCount,
        SortOrder      = chunk.SortOrder,
        ChunkStrategy  = chunk.Strategy
    });
}

// Update document and job
document.IsProcessed = true;
document.TotalChunks = chunks.Count;
job.ChunksLoaded     = chunks.Count;
job.LoadCompletedAt  = DateTime.UtcNow;
job.Status           = IngestionStatus.Completed;
```

Expected output on a standard SA legislation PDF: chunks grouped by section, each with non-empty `SectionNumber` and `Content`, all with positive `TokenCount`.
