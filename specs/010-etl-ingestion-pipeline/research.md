# Research: ETL Ingestion Pipeline with Job Tracking

**Feature**: 010-etl-ingestion-pipeline
**Date**: 2026-03-28

---

## 1. Existing Infrastructure Audit

### Decision: Reuse existing IngestionJob entity with targeted additions

**What exists:**
- `IngestionJob` entity in `backend.Core/Domains/ETL/` — has Status, per-stage timestamps (ExtractStartedAt/CompletedAt, TransformStartedAt/CompletedAt, LoadStartedAt/CompletedAt), ChunksProduced, ChunksLoaded, ExtractedCharacterCount, Strategy, ErrorMessage
- `IngestionStatus` enum — Queued, Extracting, Transforming, Loading, Completed, Failed
- `DbSet<IngestionJob>` in `backendDbContext` with FK → LegalDocument and indexes on DocumentId + Status
- `PdfIngestionAppService` — performs Extract (PdfPig) and Transform (section/fixed-size chunking) stages, signals Loading start
- `EmbeddingAppService` — generates float[1536] via OpenAI embeddings endpoint
- `LegalDocument.IsProcessed`, `LegalDocument.TotalChunks`, `LegalDocument.OriginalPdfId` (FK → ABP BinaryObject)
- `DocumentChunk` + `ChunkEmbedding` entities and DbSets

**What is missing:**
- `IngestionJob.TriggeredByUserId` (long, FK → AbpUsers) — needed to record who triggered the job
- `IngestionJob.EmbeddingsGenerated` (int) — needed to count embeddings in Load stage
- `IngestionJob.CompletedAt` (DateTime?) — overall completion timestamp for TotalDuration calculation
- `DocumentChunk.Keywords` (string) — needed for LLM enrichment output (comma-separated)
- `DocumentChunk.TopicClassification` (string) — needed for LLM enrichment output
- `EtlPipelineAppService` — the orchestrator that ties all stages together
- `ChunkEnrichmentAppService` — calls OpenAI chat completions to extract keywords + topic
- `EtlController` — admin HTTP endpoints for trigger, list, get, retry

**Duration fields decision**: Duration values (ExtractDuration, TransformDuration, LoadDuration, TotalDuration) are computed on read from the paired start/end timestamps already on `IngestionJob`. No additional columns are required — the DTOs will expose computed `TimeSpan?` properties.

**Rationale**: Avoids double-writing durations and keeps the source of truth in timestamps.

---

## 2. PDF Stream Retrieval

### Decision: Use ABP IBinaryObjectManager to retrieve PDF bytes

ABP Zero provides `IBinaryObjectManager` (registered in DI as a transient). To get a `Stream` from a stored PDF:

```csharp
var binaryObject = await _binaryObjectManager.GetOrNullAsync(document.OriginalPdfId.Value);
var stream = new MemoryStream(binaryObject.Bytes);
```

**Guard**: If `OriginalPdfId` is null, the pipeline fails the job immediately with "Document has no uploaded PDF file."

**Rationale**: Consistent with how the rest of the ABP backend retrieves stored files. No new storage abstraction needed.

---

## 3. LLM Chunk Enrichment Strategy

### Decision: Single OpenAI chat completion call per chunk for keywords + topic

The Transform stage currently parses structure (chapters/sections) but does not enrich chunks with semantic metadata. A separate enrichment step using OpenAI chat completions will:

1. Receive chunk content (truncated to 3,000 chars to stay within cost/latency budget)
2. Return a structured JSON response: `{ "keywords": ["term1","term2","term3"], "topic": "Category Name" }`
3. Parse and store in DocumentChunk.Keywords (comma-separated, max 500 chars) and DocumentChunk.TopicClassification (max 200 chars)

**Model**: `gpt-4o-mini` (configurable via `OpenAI:EnrichmentModel` in appsettings) — lowest cost for extraction tasks.

**Prompt pattern** (zero-shot extraction):
```
Extract 3-5 legal keywords and a topic classification from this South African legislation excerpt.
Respond ONLY with valid JSON: {"keywords":["...",...],"topic":"..."}.

EXCERPT:
{content}
```

**Error handling**: If the OpenAI call fails or returns unparseable JSON, log the error, set Keywords = "" and TopicClassification = "Unknown", and continue — enrichment failure is non-fatal so the pipeline does not fail the entire job.

**Rationale**: Keywords and topic classification are enrichment metadata, not core functionality. A degraded result (empty keywords) is better than a failed ingestion job. The admin can re-run enrichment as part of a retry.

---

## 4. Retry Logic Design

### Decision: Determine resume stage from completed-timestamp presence on IngestionJob

The retry stage is inferred at runtime rather than stored explicitly:

| Condition | Resume Stage |
|-----------|-------------|
| `ExtractCompletedAt` is null | Restart from Extract |
| `TransformCompletedAt` is null | Restart from Transform (reuse stored full text) |
| `LoadCompletedAt` is null (or EmbeddingsGenerated < ChunksProduced) | Restart from Load |
| All present | No retry needed (mark Completed) |

**Challenge**: For Transform resume, the full extracted text is not stored on IngestionJob — it is re-extracted. For Load resume, the chunks that were already saved need to be cleaned up or checked.

**MVP Decision**: For retry, always re-run from Extract regardless of prior progress. This is simpler to implement and avoids partial-state cleanup. The per-stage timestamps from the original run are reset. The previous incomplete chunks are deleted before re-running.

**Rationale**: MVP prioritizes correctness over efficiency. Re-extraction is fast (< 10s for most Acts). Stage-level resume adds significant complexity (stored intermediate state, partial chunk cleanup). This can be optimized in a later iteration.

---

## 5. Duplicate Job Prevention

### Decision: Check for active jobs on document before creating a new one

Before creating an `IngestionJob`, query for any existing job with the same `DocumentId` and `Status NOT IN (Completed, Failed)`. If found, throw `UserFriendlyException("A pipeline job is already active for this document.")`.

**Rationale**: Prevents two concurrent jobs from writing conflicting chunks and embeddings to the same document.

---

## 6. Admin Controller vs ABP Dynamic API

### Decision: Custom controller in Web.Host with explicit route attributes

The 4 endpoints (`trigger`, `jobs`, `jobs/{id}`, `retry`) do not follow ABP's standard CRUD URL pattern and require non-standard verbs and route segments. A custom `EtlController : backendControllerBase` in `backend.Web.Host/Controllers/` provides full route control.

`IEtlPipelineAppService` is still defined in the Application layer and injected into the controller — the controller is a thin delegation layer only.

**Rationale**: Consistent with constitution rule "no business logic in Web.Host". ABP dynamic API would expose routes like `/api/services/app/etlPipeline/triggerAsync` which doesn't match the spec's required routes.

---

## 7. Token Count Approximation

The current PdfChunkingHelper calculates `TokenCount = (Content.Length + 3) / 4` (4 chars per token). This approximation is sufficient for budgeting. No change needed.

---

## 8. Migration Plan

Two new migrations are needed:
1. `AddEtlPipelineExtensions` — adds `TriggeredByUserId`, `EmbeddingsGenerated`, `CompletedAt` to `IngestionJobs`
2. `AddChunkEnrichmentFields` — adds `Keywords`, `TopicClassification` to `DocumentChunks`

These can be combined into one migration: `AddEtlPipelineOrchestratorFields`.

---

## Summary: All NEEDS CLARIFICATION Resolved

| Topic | Decision |
|-------|----------|
| Duration storage | Computed from timestamps in DTOs — no new columns |
| PDF retrieval | ABP IBinaryObjectManager |
| LLM enrichment model | gpt-4o-mini via chat completions, non-fatal on error |
| Retry scope | Full re-run (not stage-level resume) for MVP |
| Duplicate prevention | Check for non-terminal jobs before creating new one |
| Admin routing | Custom controller with explicit route attributes |
