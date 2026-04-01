# Implementation Plan: PDF Section Chunking Ingestion Service

**Branch**: `008-pdf-section-chunking` | **Date**: 2026-03-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/008-pdf-section-chunking/spec.md`

**Note**: This plan is the output of `/speckit.plan`. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Build a `PdfIngestionAppService` (Application layer) that extracts text from a legislation PDF stream using PdfPig, splits the text into section-level `DocumentChunk` objects aligned to SA legislation boundaries, falls back to fixed-size windowing when section detection fails, and tracks each pipeline stage in a new `IngestionJob` entity per the ETL/Ingestion Gate. Two new entities are introduced: `IngestionJob` (Core) and the `ChunkStrategy` enum (Core). The existing `DocumentChunk` entity gains a `ChunkStrategy` column via migration.

## Technical Context

**Language/Version**: C# on .NET 9.0
**Primary Dependencies**: ABP Zero 10.x, EF Core 9.0.5, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.x, Ardalis.GuardClauses, UglyToad.PdfPig (NEW — `dotnet add package UglyToad.PdfPig`)
**Storage**: PostgreSQL 15+ via Npgsql
**Testing**: xUnit (via ABP test helpers)
**Target Platform**: Linux/Windows server (ABP backend service)
**Project Type**: In-process application service — no HTTP endpoint; called by other AppServices or admin controllers
**Performance Goals**: ≤10 seconds for a 50–200 page legislation PDF
**Constraints**: Text-based PDFs only (no OCR for MVP); single-document processing per call; token estimation via character approximation (no external tokeniser API)
**Scale/Scope**: Per-document calls triggered by admin upload flow; not designed for concurrent batch ingestion

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

- [x] **Layer Gate**: `IngestionJob` and `ChunkStrategy` in `backend.Core`; `PdfIngestionAppService` and DTOs in `backend.Application`; `DbSet<IngestionJob>` and migration in `backend.EntityFrameworkCore`. `DocumentChunk` extension follows existing Core entity pattern.
- [x] **Naming Gate**: Service interface `IPdfIngestionAppService`, service class `PdfIngestionAppService`, DTOs `IngestPdfRequest` / `DocumentChunkResult`, entity `IngestionJob`, enum `IngestionStatus` and `ChunkStrategy` — all follow conventions in `docs/BACKEND_STRUCTURE.md`.
- [x] **Coding Standards Gate**: Planned approach: guard clauses with Ardalis at method entry; ≤350 lines per class (service split into `ExtractText`, `DetectSections`, `BuildFixedSizeChunks`, `SplitBySubsection` sub-methods); ≤2 nesting levels via early returns; no magic numbers (constants for 500, 50, 800 token thresholds).
- [x] **Skill Gate**: `add-endpoint` will scaffold any admin-facing controller; no frontend skills needed for this feature.
- [x] **Multilingual Gate**: N/A — this is a pure backend ingestion service with no user-facing outputs or UI labels.
- [x] **Citation Gate**: N/A — not an AI-facing endpoint; produces data consumed by the RAG pipeline, not direct user responses.
- [x] **Accessibility Gate**: N/A — no frontend components introduced.
- [x] **ETL/Ingestion Gate**: ✅ FULLY ADDRESSED. New `IngestionJob` entity tracks all pipeline stages: `Queued → Extracting → Transforming → Loading → Completed/Failed`, with per-stage start/end timestamps, chunk counts, and error messages. See data-model.md.

## Project Structure

### Documentation (this feature)

```text
specs/008-pdf-section-chunking/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── pdf-ingestion-service.md
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── backend.Core/
│   │   └── Domains/
│   │       ├── LegalDocuments/
│   │       │   ├── DocumentChunk.cs          ← EXTEND: add ChunkStrategy property
│   │       │   └── ChunkStrategy.cs          ← NEW enum
│   │       └── ETL/                          ← NEW domain folder
│   │           ├── IngestionJob.cs           ← NEW entity
│   │           └── IngestionStatus.cs        ← NEW enum
│   ├── backend.Application/
│   │   └── Services/
│   │       └── PdfIngestionService/          ← NEW service folder
│   │           ├── IPdfIngestionAppService.cs
│   │           ├── PdfIngestionAppService.cs
│   │           └── DTO/
│   │               ├── IngestPdfRequest.cs
│   │               └── DocumentChunkResult.cs
│   └── backend.EntityFrameworkCore/
│       └── EntityFrameworkCore/
│           ├── backendDbContext.cs           ← ADD DbSet<IngestionJob>
│           └── Migrations/                   ← ADD migration: AddPdfIngestionEntities
└── test/
    └── backend.Tests/
        └── Services/
            └── PdfIngestionServiceTests.cs   ← NEW unit tests
```

**Structure Decision**: Web application backend (existing `backend/` structure). No frontend changes — this feature is backend-only.

## Complexity Tracking

No constitution violations. All design choices align with established patterns.
