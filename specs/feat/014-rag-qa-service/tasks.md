# Tasks: RAG Question-Answering Service

**Input**: Design documents from `/specs/feat/014-rag-qa-service/`
**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ contracts/ ✅ quickstart.md ✅

**Branch**: `feat/014-rag-qa-service`
**Tech Stack**: C# / .NET 9.0 + ABP Zero 10.x | PostgreSQL 15+ (no new migrations) | xUnit

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Tests are included for the core paths — no TDD mandate, but tests are required by constitution

---

## Phase 1: Setup

**Purpose**: Configuration prerequisite needed before any application code is written

- [x] T001 Add `"ChatModel": "gpt-4o"` key to the `"OpenAI"` section in `backend/src/backend.Web.Host/appsettings.json` (alongside existing `EmbeddingModel`, `EnrichmentModel`, `ApiKey`, `BaseUrl` keys)

**Checkpoint**: `dotnet build` succeeds with updated appsettings; no new warnings

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: DTOs, interface, prompt helper, and service skeleton that ALL user stories depend on. No user story implementation can begin until this phase is complete.

**⚠️ CRITICAL**: Complete all foundational tasks before starting any user story phase

- [x] T002 [P] Create `AskQuestionRequest.cs` DTO with `[Required] [MaxLength(30_000)] string QuestionText` property and XML doc comment in `backend/src/backend.Application/Services/RagService/DTO/AskQuestionRequest.cs`
- [x] T003 [P] Create `RagCitationDto.cs` DTO with properties `Guid ChunkId`, `string ActName`, `string SectionNumber`, `string Excerpt`, `float RelevanceScore` and XML doc comments in `backend/src/backend.Application/Services/RagService/DTO/RagCitationDto.cs`
- [x] T004 [P] Create `RagAnswerResult.cs` DTO with properties `string? AnswerText`, `bool IsInsufficientInformation`, `List<RagCitationDto> Citations`, `List<Guid> ChunkIds`, `Guid? AnswerId` and XML doc comments in `backend/src/backend.Application/Services/RagService/DTO/RagAnswerResult.cs`
- [x] T005 Create `IRagAppService.cs` interface declaring `Task<RagAnswerResult> AskAsync(AskQuestionRequest request)` and `Task InitialiseAsync(CancellationToken cancellationToken = default)` with XML doc comments on interface and both methods in `backend/src/backend.Application/Services/RagService/IRagAppService.cs` — depends on T002, T004
- [x] T006 Create `RagPromptBuilder.cs` static class with: `public const float SimilarityThreshold = 0.7f`, `public const int MaxContextChunks = 5`, `public const double ChatTemperature = 0.2`, internal record `ScoredChunk(Guid ChunkId, string ActName, string SectionNumber, string Excerpt, float Score, float[] Vector)`, and static methods `BuildSystemPrompt()`, `BuildContextBlock(IEnumerable<ScoredChunk> chunks)`, `BuildUserPrompt(string questionText, string contextBlock)` — all with XML doc comments in `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs`
- [x] T007 Create `RagAppService.cs` class skeleton: extends `ApplicationService`, implements `IRagAppService`, injects `IEmbeddingAppService`, `IRepository<DocumentChunk, Guid>`, `IRepository<Conversation, Guid>`, `IRepository<Question, Guid>`, `IRepository<Answer, Guid>`, `IRepository<AnswerCitation, Guid>`, `IHttpClientFactory`, `IConfiguration`, declares `private List<RagPromptBuilder.ScoredChunk> _loadedChunks = new()`, stubs both interface methods (throw `NotImplementedException` temporarily) in `backend/src/backend.Application/Services/RagService/RagAppService.cs` — depends on T005, T006
- [x] T008 Implement `InitialiseAsync` in `RagAppService.cs`: query all `DocumentChunk` records with `.Include(c => c.Embedding).Include(c => c.Document)` where `c.Embedding != null`, map each to `ScoredChunk` (Vector from `Embedding.Vector`, ActName from `Document.Title`, SectionNumber from `chunk.SectionNumber`, Excerpt from `chunk.Content`), assign result to `_loadedChunks` — depends on T007

**Checkpoint**: `dotnet build` succeeds with no errors. DI container resolves `IRagAppService`. `_loadedChunks` is populated after `InitialiseAsync` is called.

---

## Phase 3: User Story 1 — Legal Question Answered with Citations (Priority: P1) 🎯 MVP

**Goal**: A user submits a legal question and receives an answer that cites the specific Act name and section number from the legislation corpus.

**Independent Test**: `POST /api/app/qa/ask` with `{ "questionText": "Can my landlord evict me?" }` returns a response with `isInsufficientInformation: false`, non-null `answerText`, and a `citations` array containing at least one entry with `actName` and `sectionNumber`.

- [x] T009 [P] [US1] Write unit tests for similarity scoring in `RagAppServiceTests.cs`: test that chunks below 0.7 are excluded, test that exactly 5 chunks are returned when more than 5 qualify (highest scores win), test that returned chunks are ordered by score descending in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`
- [x] T010 [P] [US1] Write unit tests for `RagPromptBuilder` in `RagPromptBuilderTests.cs`: test `BuildSystemPrompt` returns a non-empty string containing "ONLY answer", test `BuildContextBlock` includes Act name and section number for each chunk in the expected `[ActName — SectionNumber]` format, test `BuildUserPrompt` includes the question text in `backend/test/backend.Tests/RagServiceTests/RagPromptBuilderTests.cs`
- [x] T011 [US1] Implement the happy-path body of `AskAsync` in `RagAppService.cs`: (1) guard `QuestionText` not null/whitespace via `Guard.Against.NullOrWhiteSpace`, (2) call `IEmbeddingAppService.EmbedAsync(request.QuestionText)` to get the question vector, (3) score each entry in `_loadedChunks` using `EmbeddingHelper.CosineSimilarity`, (4) filter to score ≥ `SimilarityThreshold`, order descending, take `MaxContextChunks`, (5) if any chunks qualify call `CallChatCompletionsAsync` then `PersistQaAsync` and return the full result — depends on T008
- [x] T012 [US1] Implement private `Task<string> CallChatCompletionsAsync(string systemPrompt, string userPrompt)` in `RagAppService.cs`: (1) build the OpenAI chat request body with `model` from `IConfiguration["OpenAI:ChatModel"]`, `temperature` = `ChatTemperature`, and messages array (`system` + `user`), (2) POST to `{BaseUrl}v1/chat/completions` using named `IHttpClientFactory` client with `Authorization: Bearer {ApiKey}` header, (3) parse response JSON and return `choices[0].message.content` — depends on T011
- [x] T013 [US1] Implement private `Task<Guid> PersistQaAsync(long userId, string questionText, string answerText, IEnumerable<RagPromptBuilder.ScoredChunk> usedChunks)` in `RagAppService.cs`: create and insert `Conversation` (Language = English), `Question` (OriginalText, TranslatedText = questionText), `Answer` (Text = answerText, Language = English), and one `AnswerCitation` per chunk (ChunkId, SectionNumber, Excerpt, RelevanceScore cast to decimal), return the `Answer.Id` — depends on T011
- [x] T014 [US1] Create `QaController.cs`: inherit `backendControllerBase`, apply `[Route("api/app/qa")]` and `[AbpAuthorize]`, inject `IRagAppService`, implement `[HttpPost("ask")] public Task<RagAnswerResult> Ask([FromBody] AskQuestionRequest request)` with XML doc comments on class and method in `backend/src/backend.Web.Host/Controllers/QaController.cs` — depends on T005

**Checkpoint**: User Story 1 is fully functional. `POST /api/app/qa/ask` returns a cited answer. Unit tests T009 and T010 pass. `Answer` + `AnswerCitation` records appear in the database after a successful call.

---

## Phase 4: User Story 2 — Insufficient Information Response (Priority: P2)

**Goal**: When no legislation chunk scores ≥ 0.7 against the question embedding, the service returns `isInsufficientInformation: true` without calling the LLM or writing to the database.

**Independent Test**: `POST /api/app/qa/ask` with a question on a topic not in the corpus (e.g., `"What is the maximum altitude for drones?"`) returns `{ "isInsufficientInformation": true, "answerText": null, "citations": [] }`. No new rows appear in `Answers` or `AnswerCitations` tables.

- [x] T015 [US2] Add the insufficient-information short-circuit to `AskAsync` in `RagAppService.cs`: immediately after the chunk scoring step, if the filtered list is empty return `new RagAnswerResult { IsInsufficientInformation = true, Citations = [], ChunkIds = [], AnswerText = null, AnswerId = null }` — no LLM call, no DB write — depends on T011
- [x] T016 [P] [US2] Write unit test for the insufficient-information path in `RagAppServiceTests.cs`: seed `_loadedChunks` with a known vector, embed a question that produces a cosine similarity below 0.7 with all chunks, assert the returned `RagAnswerResult.IsInsufficientInformation` is `true` and `AnswerId` is null in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs` — depends on T015

**Checkpoint**: User Stories 1 and 2 both work independently. Submitting an out-of-scope question no longer triggers an LLM call. `dotnet test` passes with 0 failures.

---

## Phase 5: User Story 3 — Answer Available Immediately on First Query (Priority: P3)

**Goal**: All chunk embeddings are loaded into memory at application startup so the first query has no cold-start loading delay.

**Independent Test**: Restart the backend; immediately submit `POST /api/app/qa/ask`. Confirm the startup log line `[RagService] Loaded {N} chunk embeddings into memory` appears before the HTTP port is open. Verify response time for the first request is within 8 seconds.

- [x] T017 [US3] Create `RagStartupService.cs` as an `IHostedService` (extend `BackgroundService`): in `ExecuteAsync`, resolve `IRagAppService` from `IServiceProvider`, call `await ragService.InitialiseAsync(stoppingToken)`, log `"[RagService] Loaded {count} chunk embeddings into memory"` using `ILogger<RagStartupService>`; if `_loadedChunks` count is zero after initialisation log a warning `"[RagService] WARNING: No chunk embeddings found. Run ETL pipeline."` in `backend/src/backend.Web.Host/Startup/RagStartupService.cs`
- [x] T018 [US3] Register `RagStartupService` in the Web.Host startup: add `services.AddHostedService<RagStartupService>()` in the service registration section of `backend/src/backend.Web.Host/Startup.cs` (or equivalent host-builder configuration file) — depends on T017

**Checkpoint**: Restart the backend and observe the startup log. First `POST /api/app/qa/ask` request completes within 8 seconds. Log confirms chunks loaded before requests are served.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validation, edge-case hardening, and integration verification across all user stories

- [ ] T019 [P] Verify `POST /api/app/qa/ask` with empty `questionText` returns HTTP 400 (guard clause triggers) — test via curl per quickstart.md
- [ ] T020 [P] Verify unauthenticated `POST /api/app/qa/ask` (no Bearer token) returns HTTP 401 (`[AbpAuthorize]` enforced)
- [x] T021 Run `dotnet test backend/test/backend.Tests` and confirm all unit tests pass with 0 failures
- [ ] T022 Run the canonical acceptance test: `POST /api/app/qa/ask` with `{ "questionText": "Can my landlord evict me?" }` and verify the response `answerText` cites Constitution Section 26(3) (success criterion SC-004)
- [ ] T023 [P] Query the database after T022 and verify `Conversations`, `Questions`, `Answers`, `AnswerCitations` tables each gained one new row linked to the correct user and chunk IDs

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 — **BLOCKS all user story phases**
- **US1 (Phase 3)**: Depends on Phase 2 — core happy path
- **US2 (Phase 4)**: Depends on Phase 3 (adds short-circuit to `AskAsync`)
- **US3 (Phase 5)**: Depends on Phase 2 — startup wiring for `InitialiseAsync`
- **Polish (Phase 6)**: Depends on Phases 3, 4, and 5 all complete

### User Story Dependencies

- **US1 (P1)**: Requires Foundational phase. No dependency on US2 or US3 (but requires `InitialiseAsync` to have run in test setup).
- **US2 (P2)**: Requires US1 `AskAsync` to exist (adds a guard inside it). Cannot be implemented independently of US1.
- **US3 (P3)**: Requires Foundational `InitialiseAsync` (T008). Independent of US1 and US2 — wires startup only.

### Within Each Phase

- T002, T003, T004 can run in parallel (different files)
- T005 depends on T002 + T004 (uses both DTOs)
- T006 is independent — no external DTOs needed
- T007 depends on T005 + T006
- T008 depends on T007
- T009, T010 can run in parallel (different test files)
- T011 → T012 → T013 are sequential in `RagAppService.cs`
- T014 is parallel to T011–T013 (different file: `QaController.cs`)

### Parallel Opportunities

```text
Phase 2 (parallel group):   T002 || T003 || T004 || T006
Phase 3 (parallel group):   T009 || T010 || T014
Phase 4 (parallel group):   T016 (with T015 already done)
Phase 6 (parallel group):   T019 || T020 || T023
```

---

## Parallel Example: User Story 1

```
# Parallel start after Phase 2 completes:

Agent A: T009 — unit tests for similarity scoring
Agent B: T010 — unit tests for RagPromptBuilder
Agent C: T014 — QaController (no dependency on T011)

# Sequential after T009/T010 confirm test structure:

Agent A: T011 → T012 → T013 (AskAsync → CallChatCompletions → PersistQa)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001)
2. Complete Phase 2: Foundational (T002–T008)
3. Complete Phase 3: User Story 1 (T009–T014)
4. **STOP and VALIDATE**: `POST /api/app/qa/ask` returns a cited answer
5. Deploy/demo the citation-grounded RAG response

### Incremental Delivery

1. Phase 1 + 2 → Foundation ready (6 MB in-memory store, interface, prompt builder)
2. Phase 3 (US1) → Cited answers live → **MVP demo**
3. Phase 4 (US2) → Hallucination protection active → **trust milestone**
4. Phase 5 (US3) → Zero cold-start latency → **performance milestone**
5. Phase 6 → Polish complete → **production-ready**

---

## Notes

- No new EF Core migrations — all domain tables exist from features 004, 005, 009
- `EmbeddingHelper.CosineSimilarity` already exists in `backend.Application/Services/EmbeddingService/EmbeddingHelper.cs` — reuse directly
- `IEmbeddingAppService` is already registered in ABP DI — inject without additional setup
- The OpenAI named `HttpClient` pattern follows `EmbeddingAppService` — check that file for the exact `IHttpClientFactory` registration
- `AbpSession.UserId` provides the authenticated user's ID inside `RagAppService` (inherited from `ApplicationService`)
- Commit after each phase checkpoint to enable clean rollback
