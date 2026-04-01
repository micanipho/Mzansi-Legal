# Implementation Plan: ETL Ingestion Pipeline with Job Tracking

**Branch**: `010-etl-ingestion-pipeline` | **Date**: 2026-03-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/010-etl-ingestion-pipeline/spec.md`

## Summary

This feature introduces the `EtlPipelineAppService` orchestrator that unifies the existing Extract (`PdfIngestionAppService`), Transform (chunking), and Load (embedding via `EmbeddingAppService`) stages under a single admin-triggered pipeline, with per-stage job tracking via the existing `IngestionJob` entity. Three new fields are added to `IngestionJob` (TriggeredByUserId, EmbeddingsGenerated, CompletedAt), two new enrichment fields to `DocumentChunk` (Keywords, TopicClassification), a new `ChunkEnrichmentAppService` for LLM-based semantic metadata, four admin REST endpoints via `EtlController`, and full retry support via a full-pipeline re-run that cleans up partial state.

## Technical Context

**Language/Version**: C# on .NET 9.0
**Primary Dependencies**: ABP Zero 10.x, Entity Framework Core 9.0.5, Npgsql 9.0.x, Ardalis.GuardClauses, UglyToad.PdfPig, System.Net.Http.Json (in-box)
**Storage**: PostgreSQL 15+ via Npgsql; `float[]` stored as `real[]`; new columns on `IngestionJobs` and `DocumentChunks`
**Testing**: xUnit via ABP test helpers
**Target Platform**: Linux/Windows server (Docker-compatible)
**Project Type**: Web service (REST API, admin-only endpoints)
**Performance Goals**: Pipeline completes in ≤5 minutes per Act (per constitution SC); LLM enrichment per chunk ≤3 s
**Constraints**: Classes ≤350 lines, nesting ≤2 levels, guard clauses at top, synchronous HTTP-request-scoped execution for MVP (no background queue)
**Scale/Scope**: Admin-only; ~10–50 documents in MVP; ~50–500 chunks per document

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Layer Gate**: `IngestionJob` and `DocumentChunk` modifications remain in `backend.Core`; all new services in `backend.Application`; `EtlController` in `backend.Web.Host`; no layer inversion
- [x] **Naming Gate**: `IEtlPipelineAppService` / `EtlPipelineAppService`, `IChunkEnrichmentAppService` / `ChunkEnrichmentAppService`, `EtlController : backendControllerBase`, DTOs follow `{Entity}Dto` convention with `[AutoMap]`
- [x] **Coding Standards Gate**: Each service class ≤350 lines with stage logic split to private methods; guard clauses at top of public methods; no magic numbers (constants defined); `var` only where type is obvious
- [x] **Skill Gate**: `add-endpoint` skill applies to the admin CRUD scaffold portions; `add-service` skill applies to service scaffolding; pipeline orchestration is custom logic documented in this plan
- [x] **Multilingual Gate**: This feature exposes admin-only backend endpoints with no user-facing text output; error messages are English admin messages — no multilingual requirement
- [x] **Citation Gate**: No AI-facing Q&A endpoints introduced; the pipeline itself is not a RAG endpoint — N/A
- [x] **Accessibility Gate**: No frontend components introduced — N/A
- [x] **ETL/Ingestion Gate**: This feature IS the ETL gate implementation — `IngestionJob` tracks all pipeline stages (Queued → Extracting → Transforming → Loading → Completed/Failed) with per-stage timing, counts, and error capture ✅

**Post-Phase-1 re-check**: All gates pass after design phase. No violations to justify.

## Project Structure

### Documentation (this feature)

```text
specs/010-etl-ingestion-pipeline/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── etl-pipeline-service.md   # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── backend.Core/
│   │   └── Domains/
│   │       ├── ETL/
│   │       │   ├── IngestionJob.cs          [MODIFY] add TriggeredByUserId, EmbeddingsGenerated, CompletedAt
│   │       │   └── IngestionStatus.cs       [NO CHANGE]
│   │       └── LegalDocuments/
│   │           └── DocumentChunk.cs         [MODIFY] add Keywords, TopicClassification
│   │
│   ├── backend.EntityFrameworkCore/
│   │   └── EntityFrameworkCore/
│   │       ├── backendDbContext.cs           [MODIFY] add TriggeredByUserId FK config + SetNull rule
│   │       └── Migrations/
│   │           └── [DATE]_AddEtlPipelineOrchestratorFields.cs   [NEW]
│   │
│   ├── backend.Application/
│   │   └── Services/
│   │       ├── ChunkEnrichmentService/        [NEW]
│   │       │   ├── IChunkEnrichmentAppService.cs
│   │       │   ├── ChunkEnrichmentAppService.cs
│   │       │   └── DTO/
│   │       │       └── ChunkEnrichmentResult.cs
│   │       └── EtlPipelineService/            [NEW]
│   │           ├── IEtlPipelineAppService.cs
│   │           ├── EtlPipelineAppService.cs
│   │           └── DTO/
│   │               ├── IngestionJobDto.cs
│   │               └── IngestionJobListDto.cs
│   │
│   └── backend.Web.Host/
│       └── Controllers/
│           └── EtlController.cs               [NEW]
│
└── test/
    └── backend.Tests/
        └── Services/
            └── EtlPipelineServiceTests.cs     [NEW]
```

**Structure Decision**: Backend-only feature following the existing layered monolith structure. No frontend work in this feature. All new code follows the pattern established by `PdfIngestionAppService` and `EmbeddingAppService`.

## Phase 0: Research Output

See [research.md](research.md) for full findings. Key decisions:

| Topic | Decision |
|-------|----------|
| Duration storage | Computed in DTOs from existing start/end timestamps — no new DB columns |
| PDF retrieval | ABP `IBinaryObjectManager.GetOrNullAsync(document.OriginalPdfId)` |
| LLM enrichment | `gpt-4o-mini` chat completions; non-fatal on error; keywords + topic in JSON |
| Retry scope | Full pipeline re-run for MVP; cleans up partial chunks before re-running |
| Duplicate prevention | Check for non-terminal jobs (Status NOT IN Completed, Failed) before creating |
| Admin routing | Custom `EtlController : backendControllerBase` with explicit `[Route]` attributes |

## Phase 1: Design Output

### Data Model

See [data-model.md](data-model.md) for full details.

**Entity additions**:
- `IngestionJob`: +`TriggeredByUserId` (long?, FK → AbpUsers SetNull), +`EmbeddingsGenerated` (int), +`CompletedAt` (DateTime?)
- `DocumentChunk`: +`Keywords` (string, max 500), +`TopicClassification` (string, max 200)

**New migration**: `AddEtlPipelineOrchestratorFields`

### Service Contracts

See [contracts/etl-pipeline-service.md](contracts/etl-pipeline-service.md) for interface signatures, HTTP routes, and error contracts.

### Key Design Decisions

1. **EtlPipelineAppService owns the Load stage** — `PdfIngestionAppService` returns chunks and signals Loading; `EtlPipelineAppService` generates embeddings and persists all entities. This matches the existing caller-responsibility contract documented in `specs/008-pdf-section-chunking/contracts/pdf-ingestion-service.md`.

2. **ChunkEnrichmentAppService is separate** — Enrichment (keywords/topics) is decoupled from chunking and embedding. It can be improved or swapped independently. Failures are non-fatal.

3. **EtlController is thin** — All logic lives in `IEtlPipelineAppService`. The controller is only HTTP delegation.

4. **Retry = full re-run** — Simpler than stage-level resume for MVP. Existing chunks are deleted before re-run to prevent duplicates.

5. **No AutoMap on IngestionJobDto** — The DTO includes computed fields (durations, DocumentTitle joined from LegalDocument) that AutoMapper cannot resolve without custom configuration. Manual mapping in the service is preferred for clarity.
