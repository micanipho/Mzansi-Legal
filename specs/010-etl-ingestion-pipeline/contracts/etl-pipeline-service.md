# Contract: IEtlPipelineAppService

**Feature**: 010-etl-ingestion-pipeline
**Layer**: Application (`backend.Application/Services/EtlPipelineService/`)
**HTTP**: `backend.Web.Host/Controllers/EtlController.cs`
**Date**: 2026-03-28

---

## Application Service Interface

```csharp
namespace backend.Services.EtlPipelineService;

public interface IEtlPipelineAppService
{
    /// <summary>
    /// Triggers a full ETL pipeline run for the specified document.
    /// Creates an IngestionJob, runs Extract → Transform → Enrich → Load stages,
    /// and returns the completed or failed job record.
    /// </summary>
    /// <exception cref="EntityNotFoundException">When documentId does not exist.</exception>
    /// <exception cref="UserFriendlyException">When an active job already exists for this document.</exception>
    /// <exception cref="UserFriendlyException">When the document has no uploaded PDF.</exception>
    Task<IngestionJobDto> TriggerAsync(Guid documentId);

    /// <summary>
    /// Retries a failed ingestion job from scratch (full re-run).
    /// Cleans up any partial chunks from the previous run before re-running.
    /// </summary>
    /// <exception cref="EntityNotFoundException">When jobId does not exist.</exception>
    /// <exception cref="UserFriendlyException">When job status is not Failed.</exception>
    Task<IngestionJobDto> RetryAsync(Guid jobId);

    /// <summary>
    /// Returns all ingestion jobs ordered by creation time descending.
    /// </summary>
    Task<List<IngestionJobListDto>> GetJobsAsync();

    /// <summary>
    /// Returns the full detail of a single ingestion job by ID.
    /// </summary>
    /// <exception cref="EntityNotFoundException">When id does not exist.</exception>
    Task<IngestionJobDto> GetJobAsync(Guid id);
}
```

---

## HTTP Endpoints (EtlController)

**Controller**: `backend.Web.Host/Controllers/EtlController.cs`
**Base**: `backendControllerBase`
**Route prefix**: `api/app/admin/etl`
**Authorization**: Admin role required on all endpoints

| Method | Route | Service Method | Description |
|--------|-------|---------------|-------------|
| POST | `/api/app/admin/etl/trigger/{documentId}` | `TriggerAsync(documentId)` | Start ETL for a document |
| GET | `/api/app/admin/etl/jobs` | `GetJobsAsync()` | List all ingestion jobs |
| GET | `/api/app/admin/etl/jobs/{id}` | `GetJobAsync(id)` | Get single job detail |
| POST | `/api/app/admin/etl/retry/{jobId}` | `RetryAsync(jobId)` | Retry a failed job |

---

## Pipeline Execution Contract

### TriggerAsync — Happy Path

```
1. Validate documentId exists (EntityNotFoundException if not)
2. Validate no non-terminal job exists for document (UserFriendlyException if active job found)
3. Validate document.OriginalPdfId != null (UserFriendlyException if no PDF)
4. Create IngestionJob: Status=Queued, TriggeredByUserId=AbpSession.UserId, save to DB
5. Call IBinaryObjectManager.GetOrNullAsync(document.OriginalPdfId) → get PDF bytes
6. Call IPdfIngestionAppService.IngestAsync(request) [handles Extract + Transform + signals Loading]
   → IngestionJob transitions: Queued → Extracting → Transforming → Loading
   → Returns IReadOnlyList<DocumentChunkResult>
7. If result is empty: mark IngestionJob.Status=Completed, CompletedAt=UtcNow, return (scanned PDF)
8. For each DocumentChunkResult:
   a. Call IChunkEnrichmentAppService.EnrichAsync(chunk.Content)
      → returns (keywords, topicClassification) — non-fatal if enrichment fails
   b. Call IEmbeddingAppService.GenerateEmbeddingAsync(chunk.Content) → float[1536]
      → if fails, catch exception, mark IngestionJob.Status=Failed, CompletedAt=UtcNow, throw
   c. Create DocumentChunk entity (with Keywords, TopicClassification from step a)
   d. Create ChunkEmbedding entity (with Vector from step b)
   e. Save both via repository
   f. Increment IngestionJob.ChunksLoaded and IngestionJob.EmbeddingsGenerated
9. Update IngestionJob: Status=Completed, CompletedAt=UtcNow, LoadCompletedAt=UtcNow, save
10. Update LegalDocument: IsProcessed=true, TotalChunks=chunksLoaded, save
11. Return final IngestionJobDto
```

### RetryAsync — Happy Path

```
1. Load IngestionJob by jobId (EntityNotFoundException if not found)
2. Validate Status == Failed (UserFriendlyException if not Failed)
3. Delete all DocumentChunks for the document (cascades to ChunkEmbeddings)
4. Reset IngestionJob fields: Status=Queued, ErrorMessage=null,
   EmbeddingsGenerated=0, ChunksLoaded=0, ChunksProduced=0, CompletedAt=null,
   reset all per-stage timestamps to null
5. Save updated IngestionJob
6. Update LegalDocument: IsProcessed=false, TotalChunks=0
7. Delegate to TriggerAsync(job.DocumentId) — reuse the same execution flow
```

### Error Handling Contract

| Stage | Failure | Outcome |
|-------|---------|---------|
| Pre-checks | Document not found | EntityNotFoundException (HTTP 404) |
| Pre-checks | Active job exists | UserFriendlyException (HTTP 400) |
| Pre-checks | No PDF uploaded | UserFriendlyException (HTTP 400) |
| Extract | PdfPig throws | PdfIngestionAppService catches, marks job Failed, re-throws |
| Transform | Exception during chunking | PdfIngestionAppService catches, marks job Failed, re-throws |
| Enrich | OpenAI chat fails | Non-fatal: log warning, use empty keywords/topic, continue |
| Embed | OpenAI embeddings fail | Fatal: EtlPipelineAppService catches, marks job Failed, CompletedAt=UtcNow |
| Save chunks | DB exception | Fatal: EtlPipelineAppService catches, marks job Failed, CompletedAt=UtcNow |

---

## Chunk Enrichment Sub-Contract

### IChunkEnrichmentAppService

```csharp
namespace backend.Services.ChunkEnrichmentService;

public interface IChunkEnrichmentAppService
{
    /// <summary>
    /// Calls OpenAI chat completions to extract keywords and a topic classification
    /// from the provided text. Returns empty values on any failure (non-fatal).
    /// </summary>
    Task<ChunkEnrichmentResult> EnrichAsync(string content);
}

public class ChunkEnrichmentResult
{
    /// <summary>Comma-separated legal keywords. Empty string on enrichment failure.</summary>
    public string Keywords { get; init; } = string.Empty;

    /// <summary>Topic classification label. "Unknown" on enrichment failure.</summary>
    public string TopicClassification { get; init; } = "Unknown";
}
```

**Configuration key**: `OpenAI:EnrichmentModel` (default: `gpt-4o-mini`)

---

## Caller Responsibilities

| Responsibility | Owner |
|----------------|-------|
| Ensure document has OriginalPdfId before triggering | Admin (pre-condition validated by service) |
| Set correct admin role on the HTTP request | Caller / Auth middleware |
| Clean up partial state before retry | `EtlPipelineAppService.RetryAsync` |
| Monitor CompletedAt / Status for job completion | Admin via GET /jobs/{id} |

---

## DTO Serialization Notes

- `Status` serializes as integer (enum value) for API responses
- `TotalDuration`, `ExtractDuration`, `TransformDuration`, `LoadDuration` are `TimeSpan?` — serialized as ISO 8601 duration strings (e.g., `"00:02:34"`)
- Null timestamps mean the stage has not completed; null durations mean stage is incomplete
