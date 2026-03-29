# Data Model: PDF Section Chunking Ingestion Service

**Feature**: 008-pdf-section-chunking
**Phase**: 1 — Design & Contracts
**Date**: 2026-03-28

---

## Overview of Changes

| Change | Type | Layer | Reason |
|---|---|---|---|
| `ChunkStrategy` enum | NEW | `backend.Core` | Tracks which chunking strategy produced a chunk |
| `IngestionStatus` enum | NEW | `backend.Core` | State machine for the ETL pipeline |
| `IngestionJob` entity | NEW | `backend.Core` | ETL/Ingestion Gate — tracks all pipeline stages |
| `DocumentChunk.ChunkStrategy` | EXTEND | `backend.Core` | Records the strategy on each persisted chunk |
| `DbSet<IngestionJob>` | NEW | `backend.EntityFrameworkCore` | EF registration of new entity |
| `PdfIngestionAppService` | NEW | `backend.Application` | Core ingestion logic |
| `IPdfIngestionAppService` | NEW | `backend.Application` | Service interface |
| `IngestPdfRequest` DTO | NEW | `backend.Application` | Input model for the service |
| `DocumentChunkResult` DTO | NEW | `backend.Application` | In-memory output model before persistence |
| Migration `AddPdfIngestionEntities` | NEW | `backend.EntityFrameworkCore` | Schema changes for IngestionJob + ChunkStrategy column |

---

## Domain Entities

### `ChunkStrategy` Enum — NEW
**File**: `backend/src/backend.Core/Domains/LegalDocuments/ChunkStrategy.cs`
**Layer**: Core (Domain)

```csharp
/// <summary>
/// Identifies the chunking strategy used to produce a DocumentChunk.
/// SectionLevel: SA legislation regex detected ≥3 sections; chunks align to chapter/section boundaries.
/// FixedSize: Fewer than 3 sections detected; chunks are fixed-width sliding windows with overlap.
/// </summary>
public enum ChunkStrategy
{
    SectionLevel = 0,
    FixedSize    = 1
}
```

---

### `IngestionStatus` Enum — NEW
**File**: `backend/src/backend.Core/Domains/ETL/IngestionStatus.cs`
**Layer**: Core (Domain)

```csharp
/// <summary>
/// Represents the current pipeline stage of an IngestionJob.
/// Terminal states: Completed and Failed.
/// </summary>
public enum IngestionStatus
{
    Queued      = 0,
    Extracting  = 1,
    Transforming = 2,
    Loading     = 3,
    Completed   = 4,
    Failed      = 5
}
```

---

### `IngestionJob` Entity — NEW
**File**: `backend/src/backend.Core/Domains/ETL/IngestionJob.cs`
**Layer**: Core (Domain)
**Table**: `IngestionJobs`

| Property | Type | Constraints | Description |
|---|---|---|---|
| `Id` (inherited) | `Guid` | PK | FullAuditedEntity base |
| `DocumentId` | `Guid` | Required, FK → LegalDocument | Parent legislation document |
| `Status` | `IngestionStatus` | Required, default: Queued | Current pipeline stage |
| `ExtractStartedAt` | `DateTime?` | Nullable | When text extraction began |
| `ExtractCompletedAt` | `DateTime?` | Nullable | When text extraction finished |
| `ExtractedCharacterCount` | `int` | Default 0 | Character count of extracted full text |
| `TransformStartedAt` | `DateTime?` | Nullable | When section parsing/chunking began |
| `TransformCompletedAt` | `DateTime?` | Nullable | When chunking finished |
| `ChunksProduced` | `int` | Default 0 | Number of DocumentChunkResults returned by Transform |
| `LoadStartedAt` | `DateTime?` | Nullable | When persistence of chunks began |
| `LoadCompletedAt` | `DateTime?` | Nullable | When persistence of chunks finished |
| `ChunksLoaded` | `int` | Default 0 | Number of chunks successfully persisted |
| `Strategy` | `ChunkStrategy?` | Nullable | Strategy detected during Transform |
| `ErrorMessage` | `string?` | MaxLength 2000 | Non-null when Status = Failed |

**Relationships**:
- Many-to-one with `LegalDocument` (Restrict on delete — job records are audit evidence)

**Indexes**:
- `(DocumentId)` — list all jobs for a given document
- `(Status)` — filter active/failed jobs in admin dashboard

---

### `DocumentChunk` Entity — EXTEND
**File**: `backend/src/backend.Core/Domains/LegalDocuments/DocumentChunk.cs`
**Change**: Add one property.

| Property | Type | Constraints | Description |
|---|---|---|---|
| `ChunkStrategy` | `ChunkStrategy?` | Nullable | Strategy that produced this chunk. Null for chunks created before this feature. |

**No other changes.** Existing properties (`ChapterTitle`, `SectionNumber`, `SectionTitle`, `Content`, `TokenCount`, `SortOrder`, `DocumentId`) are sufficient. `ChapterTitle` already stores the full chapter identifier (e.g., "Chapter 2 — Fundamental Rights").

---

## DTOs (Application Layer)

### `IngestPdfRequest` — NEW
**File**: `backend/src/backend.Application/Services/PdfIngestionService/DTO/IngestPdfRequest.cs`

| Property | Type | Description |
|---|---|---|
| `PdfStream` | `Stream` | Required. Readable byte stream of the PDF. |
| `ActName` | `string` | Required. Caller-supplied Act name used as `ActName` on all chunks. |
| `DocumentId` | `Guid` | Required. FK linking chunks to the parent LegalDocument. |
| `IngestionJobId` | `Guid` | Required. FK to the IngestionJob tracking this pipeline run. |

---

### `DocumentChunkResult` — NEW
**File**: `backend/src/backend.Application/Services/PdfIngestionService/DTO/DocumentChunkResult.cs`

Represents an in-memory chunk returned by `PdfIngestionAppService.IngestAsync`. Not persisted by the service — the caller maps each result to a `DocumentChunk` entity and saves it.

| Property | Type | Description |
|---|---|---|
| `ActName` | `string` | Act/document name (from request input). |
| `ChapterTitle` | `string?` | Full chapter identifier (e.g., "Chapter 2 — Fundamental Rights"). Null when no chapter detected. |
| `SectionNumber` | `string?` | Section number extracted from heading (e.g., "12A"). Null for fixed-size chunks. |
| `SectionTitle` | `string?` | Section heading text. Null for fixed-size chunks. |
| `Content` | `string` | Plain-text body of the chunk. |
| `TokenCount` | `int` | Estimated token count: `(Content.Length + 3) / 4`. |
| `SortOrder` | `int` | Sequential position within the document (0-based). |
| `Strategy` | `ChunkStrategy` | `SectionLevel` or `FixedSize`. |

---

## Service Interface (Application Layer)

### `IPdfIngestionAppService` — NEW
**File**: `backend/src/backend.Application/Services/PdfIngestionService/IPdfIngestionAppService.cs`

```csharp
/// <summary>
/// Extracts text from a legislation PDF stream, splits it into structured chunks,
/// and returns the results ready for the caller to persist.
/// </summary>
public interface IPdfIngestionAppService : IApplicationService
{
    /// <summary>
    /// Ingests a legislation PDF and returns ordered DocumentChunkResult objects.
    /// Updates the associated IngestionJob at each pipeline stage.
    /// </summary>
    Task<IReadOnlyList<DocumentChunkResult>> IngestAsync(IngestPdfRequest request);
}
```

---

## Database Schema Changes

### New Table: `IngestionJobs`

```sql
CREATE TABLE "IngestionJobs" (
    "Id"                       uuid         NOT NULL DEFAULT gen_random_uuid(),
    "DocumentId"               uuid         NOT NULL,
    "Status"                   integer      NOT NULL DEFAULT 0,
    "ExtractStartedAt"         timestamp    NULL,
    "ExtractCompletedAt"       timestamp    NULL,
    "ExtractedCharacterCount"  integer      NOT NULL DEFAULT 0,
    "TransformStartedAt"       timestamp    NULL,
    "TransformCompletedAt"     timestamp    NULL,
    "ChunksProduced"           integer      NOT NULL DEFAULT 0,
    "LoadStartedAt"            timestamp    NULL,
    "LoadCompletedAt"          timestamp    NULL,
    "ChunksLoaded"             integer      NOT NULL DEFAULT 0,
    "Strategy"                 integer      NULL,
    "ErrorMessage"             varchar(2000) NULL,
    -- FullAuditedEntity columns
    "CreationTime"             timestamp    NOT NULL,
    "CreatorUserId"            bigint       NULL,
    "LastModificationTime"     timestamp    NULL,
    "LastModifierUserId"       bigint       NULL,
    "IsDeleted"                boolean      NOT NULL DEFAULT false,
    "DeletionTime"             timestamp    NULL,
    "DeleterUserId"            bigint       NULL,
    CONSTRAINT "PK_IngestionJobs" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_IngestionJobs_DocumentId"
        FOREIGN KEY ("DocumentId") REFERENCES "LegalDocuments" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_IngestionJobs_DocumentId" ON "IngestionJobs" ("DocumentId");
CREATE INDEX "IX_IngestionJobs_Status"     ON "IngestionJobs" ("Status");
```

### Modified Table: `DocumentChunks`

```sql
ALTER TABLE "DocumentChunks"
    ADD COLUMN "ChunkStrategy" integer NULL;
```

---

## Entity Relationship Summary

```text
LegalDocument (existing)
    ├── DocumentChunk[] (existing + ChunkStrategy column)
    │       └── ChunkEmbedding (existing, one-to-one)
    └── IngestionJob[] (NEW)  ← ETL audit trail; 1 job per ingestion run
```

---

## Migration Name

`AddPdfIngestionEntities`

Command (from repo root):
```bash
cd backend
dotnet ef migrations add AddPdfIngestionEntities --project src/backend.EntityFrameworkCore --startup-project src/backend.Web.Host
```
