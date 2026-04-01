# Quickstart: ETL Ingestion Pipeline

**Feature**: 010-etl-ingestion-pipeline
**Audience**: Developer implementing this feature

---

## Prerequisites

1. Branch `010-etl-ingestion-pipeline` is checked out
2. PostgreSQL running (Docker: `docker exec -it mzansi-pg psql -U postgres`)
3. `.env` / `appsettings.Development.json` has:
   - `OpenAI:ApiKey` — your OpenAI key
   - `OpenAI:EmbeddingModel` — `text-embedding-ada-002`
   - `OpenAI:EnrichmentModel` — `gpt-4o-mini` (NEW for this feature)

---

## Build Order

Implement in this dependency order:

```
1. backend.Core              → IngestionJob additions + DocumentChunk additions
2. backend.EntityFrameworkCore → Add migration
3. backend.Application       → ChunkEnrichmentAppService (new)
4. backend.Application       → EtlPipelineAppService (new orchestrator)
5. backend.Web.Host          → EtlController (new admin endpoints)
6. backend.Tests             → Unit tests for EtlPipelineAppService
```

---

## Step 1 — Update Entities

**`IngestionJob.cs`** — add three fields:

```csharp
public long? TriggeredByUserId { get; set; }
public int EmbeddingsGenerated { get; set; } = 0;
public DateTime? CompletedAt { get; set; }
```

**`DocumentChunk.cs`** — add two fields:

```csharp
[MaxLength(500)]
public string Keywords { get; set; }

[MaxLength(200)]
public string TopicClassification { get; set; }
```

---

## Step 2 — Add EF Migration

```bash
cd backend
dotnet ef migrations add AddEtlPipelineOrchestratorFields \
  --project src/backend.EntityFrameworkCore \
  --startup-project src/backend.Web.Host
```

Then add FK configuration in `backendDbContext.ConfigureIngestionJobRelationships`:

```csharp
modelBuilder.Entity<IngestionJob>()
    .HasOne<User>()
    .WithMany()
    .HasForeignKey(j => j.TriggeredByUserId)
    .IsRequired(false)
    .OnDelete(DeleteBehavior.SetNull);

modelBuilder.Entity<IngestionJob>()
    .HasIndex(j => j.TriggeredByUserId);
```

---

## Step 3 — Add ChunkEnrichmentAppService

New service at `backend.Application/Services/ChunkEnrichmentService/`.

**Files to create**:
- `IChunkEnrichmentAppService.cs`
- `ChunkEnrichmentAppService.cs`
- `DTO/ChunkEnrichmentResult.cs`

Key behaviour:
- POST `https://api.openai.com/v1/chat/completions` using `gpt-4o-mini`
- Prompt instructs JSON output: `{"keywords":["..."],"topic":"..."}`
- Truncate content to 3,000 chars before sending
- On any failure (network, JSON parse, API error): return `ChunkEnrichmentResult { Keywords="", TopicClassification="Unknown" }` — non-fatal

---

## Step 4 — Add EtlPipelineAppService

New service at `backend.Application/Services/EtlPipelineService/`.

**Files to create**:
- `IEtlPipelineAppService.cs`
- `EtlPipelineAppService.cs` (≤350 lines — split stage logic to private methods)
- `DTO/IngestionJobDto.cs`
- `DTO/IngestionJobListDto.cs`

**Key injected dependencies**:

```csharp
private readonly IRepository<IngestionJob, Guid> _jobRepository;
private readonly IRepository<LegalDocument, Guid> _documentRepository;
private readonly IRepository<DocumentChunk, Guid> _chunkRepository;
private readonly IRepository<ChunkEmbedding, Guid> _embeddingRepository;
private readonly IPdfIngestionAppService _pdfIngestionService;
private readonly IEmbeddingAppService _embeddingService;
private readonly IChunkEnrichmentAppService _enrichmentService;
private readonly IBinaryObjectManager _binaryObjectManager;
```

**Guard at top of TriggerAsync**:

```csharp
Guard.Against.Null(documentId, nameof(documentId));
```

**Computed duration on DTO**:

```csharp
ExtractDuration = job.ExtractCompletedAt.HasValue && job.ExtractStartedAt.HasValue
    ? job.ExtractCompletedAt.Value - job.ExtractStartedAt.Value
    : (TimeSpan?)null;
```

---

## Step 5 — Add EtlController

New controller at `backend.Web.Host/Controllers/EtlController.cs`.

```csharp
[Route("api/app/admin/etl")]
[AbpAuthorize(/* Admin role permission */)]
public class EtlController : backendControllerBase
{
    private readonly IEtlPipelineAppService _etlService;

    public EtlController(IEtlPipelineAppService etlService)
    {
        _etlService = etlService;
    }

    [HttpPost("trigger/{documentId}")]
    public Task<IngestionJobDto> Trigger(Guid documentId)
        => _etlService.TriggerAsync(documentId);

    [HttpGet("jobs")]
    public Task<List<IngestionJobListDto>> GetJobs()
        => _etlService.GetJobsAsync();

    [HttpGet("jobs/{id}")]
    public Task<IngestionJobDto> GetJob(Guid id)
        => _etlService.GetJobAsync(id);

    [HttpPost("retry/{jobId}")]
    public Task<IngestionJobDto> Retry(Guid jobId)
        => _etlService.RetryAsync(jobId);
}
```

---

## Step 6 — Verify

**Run migration**:
```bash
cd backend
dotnet run --project src/backend.Migrator
```

**Smoke test via Swagger** (http://localhost:44311/swagger):
1. Upload a legislation PDF via `POST /api/app/legalDocument` (or use existing)
2. `POST /api/app/admin/etl/trigger/{documentId}`
3. `GET /api/app/admin/etl/jobs/{id}` → should show Completed status with chunk/embedding counts

**Check database**:
```sql
SELECT "Status", "ChunksLoaded", "EmbeddingsGenerated", "CompletedAt"
FROM "IngestionJobs"
ORDER BY "CreationTime" DESC
LIMIT 5;
```

---

## Configuration Reference

| Key | Required | Example |
|-----|----------|---------|
| `OpenAI:ApiKey` | Yes | `sk-proj-...` |
| `OpenAI:EmbeddingModel` | Yes | `text-embedding-ada-002` |
| `OpenAI:EnrichmentModel` | Yes (new) | `gpt-4o-mini` |

Add `OpenAI:EnrichmentModel` to `appsettings.json` alongside the existing OpenAI keys.
