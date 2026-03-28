# Implementation Plan: OpenAI Embedding Service

**Branch**: `009-openai-embedding-service` | **Date**: 2026-03-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/009-openai-embedding-service/spec.md`

## Summary

Build an `EmbeddingAppService` in the Application layer that calls the OpenAI embeddings REST API to convert a text string into a 1,536-dimensional `float[]` vector, with character-limit truncation and a static `CosineSimilarity` helper. The service reads credentials and model name from `appsettings.json` and plugs into the existing RAG ingestion pipeline to populate `ChunkEmbedding.Vector` during the Loading stage.

## Technical Context

**Language/Version**: C# on .NET 9.0 + ABP Zero 10.x
**Primary Dependencies**: `System.Net.Http.Json` (in-box with .NET 9), `Ardalis.GuardClauses` (already in project), `IHttpClientFactory` (ASP.NET Core built-in)
**Storage**: No new tables — `ChunkEmbedding` entity and `real[]` column already exist in PostgreSQL via prior migration
**Testing**: xUnit via ABP test helpers (existing `backend.Tests` project)
**Target Platform**: Linux/Windows server (.NET 9)
**Project Type**: Internal application service; no HTTP endpoint exposed — called by the ingestion pipeline
**Performance Goals**: Single embedding call ≤ 3 seconds under normal network conditions
**Constraints**: Input text capped at 30,000 characters; OpenAI `text-embedding-ada-002` output is fixed at 1,536 dimensions
**Scale/Scope**: ~1,000 chunks per legislation Act; ~$0.10 per Act at current OpenAI pricing

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Layer Gate**: `EmbeddingAppService` lives in `backend.Application/Services/EmbeddingService/` — Application layer, correct per `docs/BACKEND_STRUCTURE.md`. No new domain entity; `ChunkEmbedding` (Core layer) already exists.
- [x] **Naming Gate**: `IEmbeddingAppService` / `EmbeddingAppService` follow the `I{Entity}AppService` / `{Entity}AppService` convention. `EmbeddingHelper` follows the `{Domain}Helper` static-helper convention (matching `PdfChunkingHelper`). `EmbeddingResult` DTO follows `{Entity}Dto` → adapted to `{Operation}Result` for non-entity output (consistent with `DocumentChunkResult` from feature 008).
- [x] **Coding Standards Gate**: Guard clauses (`Ardalis.GuardClauses`) at all method entry points. No method will exceed scroll-visible length. Nesting capped at two levels. Magic numbers replaced with named constants (`MaxInputCharacters = 30_000`, `ChunkEmbedding.EmbeddingDimension = 1536`).
- [x] **Skill Gate**: No CRUD scaffold needed — service is a non-CRUD integration. `speckit.plan` and `speckit.tasks` are the applicable skills. `add-endpoint` is N/A for this feature.
- [x] **Multilingual Gate**: No user-facing output. Service produces numerical vectors consumed internally; no text output to translate.
- [x] **Citation Gate**: No AI-facing HTTP endpoint exposed. This is an internal utility service called by the pipeline — not a RAG answer endpoint. N/A.
- [x] **Accessibility Gate**: No frontend components. N/A.
- [x] **ETL/Ingestion Gate**: This service is called in the Loading stage of the existing `IngestionJob` pipeline. The caller (Loading stage owner) is responsible for updating `IngestionJob` status. The service itself does not modify `IngestionJob` — it only returns a vector. No new ingestion stage is introduced.

**Result**: All 8 gates pass. No violations to justify.

## Project Structure

### Documentation (this feature)

```text
specs/009-openai-embedding-service/
├── plan.md              # This file
├── research.md          # Phase 0 output ✅
├── data-model.md        # Phase 1 output ✅
├── quickstart.md        # Phase 1 output ✅
├── contracts/
│   └── embedding-service.md   # Phase 1 output ✅
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── backend.Application/
│   │   └── Services/
│   │       └── EmbeddingService/          # NEW — all files in this folder
│   │           ├── IEmbeddingAppService.cs
│   │           ├── EmbeddingAppService.cs
│   │           ├── EmbeddingHelper.cs
│   │           └── DTO/
│   │               └── EmbeddingResult.cs
│   └── backend.Web.Host/
│       └── appsettings.json               # MODIFY — add "OpenAI" section
└── test/
    └── backend.Tests/
        └── EmbeddingServiceTests/         # NEW — unit tests
            ├── EmbeddingHelperTests.cs
            └── EmbeddingAppServiceTests.cs
```

**Structure Decision**: Single-project Application layer addition, following the `PdfIngestionService` folder pattern introduced in feature 008.

## Complexity Tracking

> No constitution violations — section not required.
