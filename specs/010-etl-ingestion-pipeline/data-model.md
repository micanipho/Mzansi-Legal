# Data Model: ETL Ingestion Pipeline with Job Tracking

**Feature**: 010-etl-ingestion-pipeline
**Date**: 2026-03-28
**Layer**: `backend.Core` (entities) | `backend.EntityFrameworkCore` (DbContext, migration)

---

## Entity Changes

### 1. `IngestionJob` — Additions (existing entity in `backend.Core/Domains/ETL/`)

Three new fields required by the orchestrator:

| Field | Type | Nullable | Default | Purpose |
|-------|------|----------|---------|---------|
| `TriggeredByUserId` | `long` | Yes | null | FK → AbpUsers.Id — records which admin triggered the job |
| `EmbeddingsGenerated` | `int` | No | 0 | Count of ChunkEmbedding records saved during the Load stage |
| `CompletedAt` | `DateTime?` | Yes | null | UTC timestamp when the job reached Completed or Failed status |

**No new duration columns**: `ExtractDuration`, `TransformDuration`, `LoadDuration`, and `TotalDuration` are computed in the DTO from paired start/end timestamps already on the entity.

**FK constraint**: `TriggeredByUserId` → `AbpUsers` with `DeleteBehavior.SetNull` (job history is preserved even if the admin user is removed).

---

### 2. `DocumentChunk` — Additions (existing entity in `backend.Core/Domains/LegalDocuments/`)

Two new fields for LLM-enriched semantic metadata:

| Field | Type | MaxLength | Nullable | Default | Purpose |
|-------|------|-----------|----------|---------|---------|
| `Keywords` | `string` | 500 | Yes | null | Comma-separated keywords extracted by LLM enrichment (e.g., "employment,termination,fair dismissal") |
| `TopicClassification` | `string` | 200 | Yes | null | Topic category assigned by LLM enrichment (e.g., "Labour Relations") |

**When null**: Chunks ingested before this feature, or chunks where enrichment failed non-fatally, will have null keywords/topic. The RAG pipeline continues to work without enrichment.

---

## New Entities

No new domain entities are introduced. The pipeline orchestration is handled entirely by the application service layer.

---

## New DTOs (in `backend.Application/Services/EtlPipelineService/DTO/`)

### `IngestionJobDto` (full detail view)

```text
Id                    Guid
DocumentId            Guid
DocumentTitle         string        (joined from LegalDocument.Title)
Status                IngestionStatus
TriggeredByUserId     long?
StartedAt             DateTime?     (= ExtractStartedAt or CreationTime)
CompletedAt           DateTime?
ErrorMessage          string?

ExtractStartedAt      DateTime?
ExtractCompletedAt    DateTime?
ExtractDuration       TimeSpan?     (computed: ExtractCompletedAt - ExtractStartedAt)
ExtractedCharacterCount int

TransformStartedAt    DateTime?
TransformCompletedAt  DateTime?
TransformDuration     TimeSpan?     (computed)
ChunksProduced        int
Strategy              ChunkStrategy?

LoadStartedAt         DateTime?
LoadCompletedAt       DateTime?
LoadDuration          TimeSpan?     (computed)
ChunksLoaded          int
EmbeddingsGenerated   int

TotalDuration         TimeSpan?     (computed: CompletedAt - ExtractStartedAt)
```

### `IngestionJobListDto` (list view — lighter)

```text
Id                    Guid
DocumentId            Guid
DocumentTitle         string
Status                IngestionStatus
StartedAt             DateTime?
CompletedAt           DateTime?
TotalDuration         TimeSpan?
ChunksLoaded          int
EmbeddingsGenerated   int
ErrorMessage          string?
```

### `TriggerEtlInput`

```text
DocumentId    Guid    (required)
```

Used as the route parameter source — no request body needed for trigger.

### `RetryEtlInput`

```text
JobId    Guid    (required)
```

Used as the route parameter source — no request body needed for retry.

---

## Database Migration

### Migration: `AddEtlPipelineOrchestratorFields`

**Table: `IngestionJobs`** — alter:

```sql
ALTER TABLE "IngestionJobs"
  ADD COLUMN "TriggeredByUserId" bigint NULL,
  ADD COLUMN "EmbeddingsGenerated" integer NOT NULL DEFAULT 0,
  ADD COLUMN "CompletedAt" timestamp NULL;

ALTER TABLE "IngestionJobs"
  ADD CONSTRAINT "FK_IngestionJobs_AbpUsers_TriggeredByUserId"
  FOREIGN KEY ("TriggeredByUserId") REFERENCES "AbpUsers" ("Id") ON DELETE SET NULL;

CREATE INDEX "IX_IngestionJobs_TriggeredByUserId" ON "IngestionJobs" ("TriggeredByUserId");
```

**Table: `DocumentChunks`** — alter:

```sql
ALTER TABLE "DocumentChunks"
  ADD COLUMN "Keywords" character varying(500) NULL,
  ADD COLUMN "TopicClassification" character varying(200) NULL;
```

---

## State Transition Model

```
Queued
  └──[TriggerAsync starts]──► Extracting
                                └──[success]──► Transforming
                                                  └──[success]──► Loading
                                                                     └──[all chunks saved]──► Completed
                                └──[exception]──► Failed
                                                  Transforming──[exception]──► Failed
                                                  Loading──[exception]──► Failed

Failed ──[RetryAsync]──► Queued (full re-run from Extract)
```

---

## Entity Relationships Summary

```
LegalDocument (1) ──────────────────────► (*) IngestionJob
     │                                           FK: DocumentId
     │                                           FK: TriggeredByUserId → AbpUsers
     │
     └──► (*) DocumentChunk
                 └──► (1) ChunkEmbedding
```

**Cross-aggregate constraints**:
- `IngestionJob.DocumentId` → `LegalDocument` with `DeleteBehavior.Restrict` (existing)
- `IngestionJob.TriggeredByUserId` → `AbpUsers` with `DeleteBehavior.SetNull` (new)
