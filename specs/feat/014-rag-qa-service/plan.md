# Implementation Plan: RAG Question-Answering Service

**Branch**: `feat/014-rag-qa-service` | **Date**: 2026-03-30 | **Spec**: [spec.md](../../014-rag-qa-service/spec.md)
**Input**: Feature specification from `/specs/014-rag-qa-service/spec.md`

## Summary

Build a `RagAppService` in the Application layer that loads all `ChunkEmbedding` vectors into memory at startup, embeds user questions via `IEmbeddingAppService`, performs in-memory cosine similarity search (top-5 above 0.7), constructs a grounded GPT-4o prompt with Act/section-labelled context, calls the OpenAI chat completions API (temperature 0.2), persists the resulting `Conversation → Question → Answer → AnswerCitation` chain to PostgreSQL, and exposes the result through a `QaController` endpoint returning answer text, structured citations, and chunk IDs.

## Technical Context

**Language/Version**: C# on .NET 9.0 + ABP Zero 10.x
**Primary Dependencies**: `System.Net.Http.Json` (in-box with .NET 9), `Ardalis.GuardClauses` (already in project), `IHttpClientFactory` (ASP.NET Core built-in), existing `IEmbeddingAppService` and `EmbeddingHelper.CosineSimilarity`
**Storage**: PostgreSQL 15+ via Npgsql — no new migrations; reuses `DocumentChunks`, `ChunkEmbeddings`, `Conversations`, `Questions`, `Answers`, `AnswerCitations` tables from prior features (005, 009)
**Testing**: xUnit via ABP test helpers (existing `backend.Tests` project)
**Target Platform**: Linux/Windows server (.NET 9, Docker-compatible)
**Project Type**: Web service — Application service + HTTP endpoint
**Performance Goals**: End-to-end answer ≤8 seconds (constitution target for text responses); embedding load at startup ≤5 seconds for ~1,000 chunks
**Constraints**: English-only question input for MVP (multilingual is a separate milestone); in-memory vector store (~1,000 chunks × 1,536 floats ≈ 6 MB); OpenAI API key required in `appsettings.json`; input question capped at 30,000 characters
**Scale/Scope**: ~1,000 legislation chunks; single-tenant ABP Zero deployment

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

- [x] **Layer Gate**: `RagAppService` lives in `backend.Application/Services/RagService/` (Application layer). `QaController` lives in `backend.Web.Host/Controllers/` (Web.Host layer). No new domain entities — reuses `Conversation`, `Question`, `Answer`, `AnswerCitation` from `backend.Core/Domains/QA/`. No cross-layer violations.
- [x] **Naming Gate**: `IRagAppService` / `RagAppService` follow the `I{Domain}AppService` / `{Domain}AppService` convention. DTOs `AskQuestionRequest`, `RagAnswerResult`, `RagCitationDto` follow `{Operation}{Input|Result}` and `{Domain}Dto` patterns consistent with `EmbeddingResult` and `DocumentChunkResult`.
- [x] **Coding Standards Gate**: Guard clauses (`Ardalis.GuardClauses`) at all method entry points. No method exceeds screen height. Nesting capped at two levels. Magic numbers replaced with named constants (`SimilarityThreshold = 0.7f`, `MaxContextChunks = 5`, `ChatTemperature = 0.2`). Classes stay under 350 lines — split into `RagPromptBuilder` helper if needed.
- [x] **Skill Gate**: `add-endpoint` skill applies to scaffolding `QaController`. `speckit.plan` and `speckit.tasks` are the governance skills in use. No CRUD scaffold needed for `RagAppService` (non-CRUD custom service).
- [x] **Multilingual Gate**: This feature scopes to English questions and English answers for MVP — explicitly stated in the spec Assumptions. The `Answer.Language` field is already persisted as `Language.English`. The constitution's multilingual requirement for this endpoint is deferred to the dedicated multilingual milestone; this is a documented, time-bound exception. The `Language` enum and `Answer.Language` column ensure zero schema changes are needed when multilingual support is added.
- [x] **Citation Gate**: The RAG endpoint defines a full contract: system prompt instructs the LLM to ONLY answer from retrieved context and ALWAYS cite section numbers; `RagCitationDto` carries Act name + section number + excerpt + relevance score; fallback returns `IsInsufficientInformation = true` when no chunk exceeds the threshold. See `contracts/qa-rag-service.md`.
- [x] **Accessibility Gate**: No new frontend components in this feature. N/A.
- [x] **ETL/Ingestion Gate**: This feature is read-only with respect to the ingestion pipeline — it reads `DocumentChunks` and `ChunkEmbeddings` but does not modify `IngestionJob` or any ETL entity. N/A.

**Result**: All 8 gates pass. Multilingual gate has a documented, scoped exception (English-only for this milestone).

## Project Structure

### Documentation (this feature)

```text
specs/feat/014-rag-qa-service/
├── plan.md              # This file
├── research.md          # Phase 0 output ✅
├── data-model.md        # Phase 1 output ✅
├── quickstart.md        # Phase 1 output ✅
├── contracts/
│   └── qa-rag-service.md   # Phase 1 output ✅
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── backend.Application/
│   │   └── Services/
│   │       └── RagService/                      # NEW — all files in this folder
│   │           ├── IRagAppService.cs
│   │           ├── RagAppService.cs
│   │           ├── RagPromptBuilder.cs          # static helper for prompt construction
│   │           └── DTO/
│   │               ├── AskQuestionRequest.cs
│   │               ├── RagAnswerResult.cs
│   │               └── RagCitationDto.cs
│   ├── backend.Web.Host/
│   │   ├── Controllers/
│   │   │   └── QaController.cs                  # NEW
│   │   └── appsettings.json                     # MODIFY — add "ChatModel" key to "OpenAI" section
│   └── backend.Core/ (no changes — entities already exist)
└── test/
    └── backend.Tests/
        └── RagServiceTests/                     # NEW
            ├── RagAppServiceTests.cs
            └── RagPromptBuilderTests.cs
```

**Structure Decision**: Single Application-layer service addition following the `EmbeddingService` and `ChunkEnrichmentService` folder pattern. Controller follows the `EtlController` pattern in `Web.Host`.

## Complexity Tracking

> No constitution violations requiring justification. Multilingual exception is scoped and time-bound.

## Implementation Steps

---

### Step 1 — Add `ChatModel` to appsettings

**Action**:
1. Open `backend/src/backend.Web.Host/appsettings.json`
2. Add `"ChatModel": "gpt-4o"` to the existing `"OpenAI"` section alongside `"EmbeddingModel"` and `"EnrichmentModel"`

**Expected Result**: `appsettings.json` OpenAI section has four keys: `ApiKey`, `BaseUrl`, `EmbeddingModel`, `EnrichmentModel`, `ChatModel`.

**Validation**: App builds and starts without configuration warnings.

---

### Step 2 — Create DTOs

**Action**:
1. Create `backend.Application/Services/RagService/DTO/AskQuestionRequest.cs`
   - `QuestionText` (`string`, `[Required]`, `[MaxLength(30_000)]`)
   - XML doc comments
2. Create `backend.Application/Services/RagService/DTO/RagCitationDto.cs`
   - `ActName` (`string`) — e.g., "Constitution of the Republic of South Africa"
   - `SectionNumber` (`string`) — e.g., "Section 26(3)"
   - `Excerpt` (`string`) — the relevant passage from the chunk
   - `RelevanceScore` (`float`) — cosine similarity score for this chunk
   - `ChunkId` (`Guid`)
   - XML doc comments
3. Create `backend.Application/Services/RagService/DTO/RagAnswerResult.cs`
   - `AnswerText` (`string`) — the LLM-generated answer; null when `IsInsufficientInformation`
   - `IsInsufficientInformation` (`bool`) — true when no chunk exceeded the similarity threshold
   - `Citations` (`List<RagCitationDto>`) — ordered by relevance score descending
   - `ChunkIds` (`List<Guid>`) — IDs of chunks used, for traceability
   - `AnswerId` (`Guid?`) — ID of the persisted `Answer` entity; null when insufficient information
   - XML doc comments

**Expected Result**: Three DTO classes compile in `backend.Services.RagService.DTO` namespace.

**Validation**: Build succeeds. All properties have XML doc comments.

---

### Step 3 — Create `IRagAppService` Interface

**Action**:
1. Create `backend.Application/Services/RagService/IRagAppService.cs`
2. Declare `Task<RagAnswerResult> AskAsync(AskQuestionRequest request)` as the single contract method
3. Declare `Task InitialiseAsync(CancellationToken cancellationToken = default)` for startup pre-loading
4. Add XML doc comments on the interface and both methods

**Expected Result**: Interface compiles in `backend.Services.RagService` namespace.

**Validation**: Build succeeds. Interface is discoverable by ABP dependency injection.

---

### Step 4 — Create `RagPromptBuilder` Static Helper

**Action**:
1. Create `backend.Application/Services/RagService/RagPromptBuilder.cs`
2. Add `public const float SimilarityThreshold = 0.7f;`
3. Add `public const int MaxContextChunks = 5;`
4. Add `public const double ChatTemperature = 0.2;`
5. Implement `BuildSystemPrompt()` returning the SA legal assistant system instruction string
6. Implement `BuildContextBlock(IEnumerable<ScoredChunk> chunks)` returning a formatted context string with each chunk labelled as `[ActName — SectionNumber]\n{Content}`
7. Implement `BuildUserPrompt(string questionText, string contextBlock)` returning the formatted user turn
8. Define internal record `ScoredChunk(Guid ChunkId, string ActName, string SectionNumber, string Excerpt, float Score, float[] Vector)`
9. Add XML doc comments on the class and all public members

**Expected Result**: Static helper compiles with no external dependencies beyond `Ardalis.GuardClauses`.

**Validation**: Build succeeds. Constants are accessible from `RagAppService`.

---

### Step 5 — Create `RagAppService`

**Action**:
1. Create `backend.Application/Services/RagService/RagAppService.cs`
2. Extend `ApplicationService`, implement `IRagAppService`
3. Inject: `IEmbeddingAppService`, `IRepository<DocumentChunk, Guid>` (for startup load), `IRepository<Conversation, Guid>`, `IRepository<Question, Guid>`, `IRepository<Answer, Guid>`, `IRepository<AnswerCitation, Guid>`, `IHttpClientFactory`, `IConfiguration`
4. Declare `private List<RagPromptBuilder.ScoredChunk> _loadedChunks` (initialised empty)
5. Implement `InitialiseAsync`:
   - Query all `DocumentChunks` with `Include(c => c.Embedding).Include(c => c.Document)` where `Embedding != null`
   - Map each to `ScoredChunk` (Vector from `Embedding.Vector`, ActName from `Document.Title`, SectionNumber from chunk's `SectionNumber`)
   - Assign to `_loadedChunks`
6. Implement `AskAsync`:
   - Guard: `QuestionText` not null or whitespace
   - Embed question via `IEmbeddingAppService.EmbedAsync`
   - Score each `_loadedChunk` with `EmbeddingHelper.CosineSimilarity`
   - Filter to score >= `SimilarityThreshold`, order descending, take `MaxContextChunks`
   - If none: return `RagAnswerResult { IsInsufficientInformation = true }`
   - Build context block + user prompt via `RagPromptBuilder`
   - Call OpenAI chat completions (`gpt-4o`, temperature 0.2) via `IHttpClientFactory`
   - Parse answer text from response
   - Persist: new `Conversation`, `Question`, `Answer`, and one `AnswerCitation` per retrieved chunk
   - Return `RagAnswerResult` with answer text, citations, chunk IDs, and answer ID
7. Keep `AskAsync` under scroll height; extract `CallChatCompletionsAsync` and `PersistQaAsync` as private methods
8. Add XML doc comments on class and all public methods

**Expected Result**: `RagAppService` compiles and resolves all injected dependencies.

**Validation**: Build succeeds. ABP DI resolves the service. `_loadedChunks` is populated after `InitialiseAsync`.

---

### Step 6 — Register Startup Initialisation via Hosted Service

**Action**:
1. Create or update `backend.Web.Host` startup to call `IRagAppService.InitialiseAsync` after the application is ready
2. Register a `BackgroundService` or use ABP's `IApplicationService` + `IAsyncInitializable` pattern, whichever matches the existing startup pattern in `Web.Host`
3. Log `[RagService] Loaded {count} chunk embeddings into memory` at startup
4. Guard: if `_loadedChunks` is empty after initialisation, log a warning (do not throw — the service still starts)

**Expected Result**: On application startup, all chunk embeddings are loaded into memory before the first request is served.

**Validation**: Application logs show the loaded chunk count. First `AskAsync` call does not trigger a DB load.

---

### Step 7 — Create `QaController`

**Action**:
1. Create `backend.Web.Host/Controllers/QaController.cs`
2. Inherit `backendControllerBase`
3. Route: `[Route("api/app/qa")]`
4. Apply `[AbpAuthorize]` (authenticated users only — conversations are user-scoped per constitution)
5. Inject `IRagAppService`
6. Implement `[HttpPost("ask")] Task<RagAnswerResult> Ask([FromBody] AskQuestionRequest request)`
7. Add XML doc comments on class and method

**Expected Result**: Controller compiles. Swagger shows `POST /api/app/qa/ask`.

**Validation**: Build succeeds. Swagger UI reflects the endpoint. Unauthenticated requests return 401.

---

### Step 8 — Write Unit Tests

**Action**:
1. Create `backend.Tests/RagServiceTests/RagPromptBuilderTests.cs`
   - Test `BuildSystemPrompt` returns non-empty string
   - Test `BuildContextBlock` includes Act name and section number for each chunk
   - Test `BuildContextBlock` with empty chunks returns empty/placeholder string
2. Create `backend.Tests/RagServiceTests/RagAppServiceTests.cs`
   - Test `AskAsync` returns `IsInsufficientInformation = true` when no chunk scores above threshold
   - Test `AskAsync` filters chunks below threshold correctly
   - Test `AskAsync` takes at most `MaxContextChunks` chunks even when more qualify
   - Mock `IEmbeddingAppService` and the HTTP client; use known vectors for deterministic results

**Expected Result**: All tests pass. Coverage of the filtering and fallback paths is confirmed.

**Validation**: `dotnet test` passes with 0 failures. No flaky tests dependent on real OpenAI calls.

---

### Step 9 — Integration Smoke Test

**Action**:
1. Ensure DB has at least one `DocumentChunk` with a populated `ChunkEmbedding.Vector` (run ETL pipeline or seed from feature 012 if needed)
2. Start the backend
3. Authenticate and call `POST /api/app/qa/ask` with `{ "questionText": "Can my landlord evict me?" }`
4. Verify:
   - Response contains `answerText` with non-empty content
   - `citations` array has at least one entry with `actName` and `sectionNumber`
   - `isInsufficientInformation` is `false`
   - `answerId` is a valid GUID
   - A new `Answer` + `AnswerCitation` records exist in the database

**Expected Result**: The RAG pipeline returns a cited answer to the canonical acceptance test question.

**Validation**: `answerId` resolves in the DB. `AnswerCitation.ChunkId` links to a real `DocumentChunk`. Answer text references Constitution Section 26(3) or equivalent.

---

## Dependencies & Order

```text
Step 1  (appsettings)         → no dependencies
Step 2  (DTOs)                → Step 1 (needs ChatModel key confirmed)
Step 3  (Interface)           → Step 2
Step 4  (RagPromptBuilder)    → no external dependencies
Step 5  (RagAppService)       → Steps 3, 4; IEmbeddingAppService (feature 009 ✓)
Step 6  (Startup init)        → Step 5
Step 7  (QaController)        → Step 3; needs auth infrastructure (existing ✓)
Step 8  (Unit Tests)          → Steps 4, 5
Step 9  (Integration test)    → Steps 6, 7; requires ETL pipeline data (feature 012 ✓)
```

## Critical Path

```
DTOs → Interface → PromptBuilder → RagAppService → Startup Init → QaController → Tests → Integration
```

## Failure Handling

| Failure | Diagnosis | Resolution |
|---------|-----------|------------|
| `_loadedChunks` is empty at startup | No chunks with embeddings in DB | Run ETL pipeline (feature 012) first to populate `ChunkEmbeddings` |
| OpenAI chat completions 401 | `ApiKey` missing or wrong in appsettings | Set `OpenAI__ApiKey` environment variable; verify Railway secret |
| Answer returned with no citations | All chunks score below 0.7 | Lower `SimilarityThreshold` temporarily; investigate chunk quality |
| `NullReferenceException` on `Chunk.Document` | `Include(c => c.Document)` missing in startup load | Add `.Include(c => c.Embedding).Include(c => c.Document)` to the startup query |
| `RagAppService` not resolved by DI | Missing ABP module registration | Register service in `backendApplicationModule` or verify ABP convention scanning |
| HTTP 401 on `POST /api/app/qa/ask` | User not authenticated | Obtain JWT token from `/api/TokenAuth/Authenticate` first |

## Deliverables

| Deliverable | Location |
|-------------|----------|
| `IRagAppService.cs` | `backend.Application/Services/RagService/` |
| `RagAppService.cs` | `backend.Application/Services/RagService/` |
| `RagPromptBuilder.cs` | `backend.Application/Services/RagService/` |
| `AskQuestionRequest.cs` | `backend.Application/Services/RagService/DTO/` |
| `RagAnswerResult.cs` | `backend.Application/Services/RagService/DTO/` |
| `RagCitationDto.cs` | `backend.Application/Services/RagService/DTO/` |
| `QaController.cs` | `backend.Web.Host/Controllers/` |
| Updated `appsettings.json` | `backend.Web.Host/` |
| `RagAppServiceTests.cs` | `backend.Tests/RagServiceTests/` |
| `RagPromptBuilderTests.cs` | `backend.Tests/RagServiceTests/` |
