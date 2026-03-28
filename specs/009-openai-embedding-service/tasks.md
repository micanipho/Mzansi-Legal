# Tasks: OpenAI Embedding Service

**Input**: Design documents from `/specs/009-openai-embedding-service/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies on in-progress tasks)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Exact file paths are included in every task description

## Path Conventions

Based on plan.md structure:

```
backend/src/backend.Application/Services/EmbeddingService/
backend/src/backend.Web.Host/appsettings.json
backend/test/backend.Tests/EmbeddingServiceTests/
```

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the folder structure and wire up configuration and HTTP client so all subsequent phases have a working foundation.

- [x] T001 Create folder `backend/src/backend.Application/Services/EmbeddingService/DTO/` (empty directory placeholder; will hold `EmbeddingResult.cs`)
- [x] T002 Add `"OpenAI": { "ApiKey": "", "EmbeddingModel": "text-embedding-ada-002" }` block to `backend/src/backend.Web.Host/appsettings.json`
- [x] T003 [P] Register named `"OpenAI"` `HttpClient` with `BaseAddress = "https://api.openai.com/"` and `Timeout = 30s` in `backend/src/backend.Web.Host/Startup/Startup.cs` (BaseUrl read from config to avoid S1075 warning)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Define the shared output contract (`EmbeddingResult`) and the service interface (`IEmbeddingAppService`) that all user-story phases depend on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T004 Create `EmbeddingResult` record with properties `float[] Vector`, `string Model`, `int InputCharacterCount` in `backend/src/backend.Application/Services/EmbeddingService/DTO/EmbeddingResult.cs`
- [x] T005 [P] Create `IEmbeddingAppService` interface with `Task<EmbeddingResult> GenerateEmbeddingAsync(string text)` and XML doc comments in `backend/src/backend.Application/Services/EmbeddingService/IEmbeddingAppService.cs`

**Checkpoint**: Foundation ready — user story implementation can now begin.

---

## Phase 3: User Story 1 - Generate Embedding for Text (Priority: P1) 🎯 MVP

**Goal**: Call the OpenAI embeddings REST API with a plain-text string and return a 1,536-element `float[]` vector, with silent truncation for inputs over 30,000 characters.

**Independent Test**: Construct `EmbeddingAppService` with valid config, call `GenerateEmbeddingAsync("The Constitution of South Africa")`, assert `result.Vector.Length == 1536` and all values are in `[-1.0, 1.0]`.

### Implementation for User Story 1

- [x] T006 [US1] Create `EmbeddingHelper` static class with `TruncateToLimit(string text, int maxCharacters = 30_000)` returning the original string when under the limit and the first `maxCharacters` characters when over, in `backend/src/backend.Application/Services/EmbeddingService/EmbeddingHelper.cs`
- [x] T007 [US1] Add private sealed records `OpenAiEmbeddingRequest` (properties: `string Input`, `string Model`) and `OpenAiEmbeddingResponse` / `OpenAiEmbeddingData` to deserialise the OpenAI REST response (`data[0].embedding`) in `backend/src/backend.Application/Services/EmbeddingService/EmbeddingAppService.cs`
- [x] T008 [US1] Implement `EmbeddingAppService` class: inject `IConfiguration` and `IHttpClientFactory`; read `OpenAI:ApiKey` and `OpenAI:EmbeddingModel` in the constructor; implement `GenerateEmbeddingAsync` — truncate via `EmbeddingHelper.TruncateToLimit`, POST to `/v1/embeddings` with `Authorization: Bearer` header, deserialise response, return `EmbeddingResult` — in `backend/src/backend.Application/Services/EmbeddingService/EmbeddingAppService.cs`
- [x] T009 [P] [US1] Write unit tests for `EmbeddingHelper.TruncateToLimit`: text under 30,000 chars returns unchanged; text at exactly 30,000 chars returns unchanged; text at 30,001 chars is trimmed to 30,000; in `backend/test/backend.Tests/EmbeddingServiceTests/EmbeddingHelperTests.cs`

**Checkpoint**: `GenerateEmbeddingAsync` is callable with a real or mocked HTTP client and returns a 1,536-element vector. User Story 1 is independently functional.

---

## Phase 4: User Story 2 - Compare Two Vectors for Semantic Similarity (Priority: P2)

**Goal**: Provide a static `CosineSimilarity(float[] a, float[] b)` method that returns ~1.0 for identical vectors and throws on mismatched lengths.

**Independent Test**: Call `EmbeddingHelper.CosineSimilarity(v, v)` with any unit vector — result must be `1.0 ± 0.001`. Call with vectors of different lengths — must throw `ArgumentException`.

### Implementation for User Story 2

- [x] T010 [US2] Add `CosineSimilarity(float[] a, float[] b)` static method to `EmbeddingHelper`: guard against null inputs and different lengths; compute dot product, magnitude A, magnitude B; return `dotProduct / (magA * magB)`; return `0f` when either magnitude is zero to avoid NaN — in `backend/src/backend.Application/Services/EmbeddingService/EmbeddingHelper.cs`
- [x] T011 [P] [US2] Write unit tests for `EmbeddingHelper.CosineSimilarity`: identical vectors → `1.0 ± 0.001`; orthogonal vectors → `0.0 ± 0.001`; vectors of different lengths → throws `ArgumentException`; null input → throws `ArgumentException`; in `backend/test/backend.Tests/EmbeddingServiceTests/EmbeddingHelperTests.cs`

**Checkpoint**: `CosineSimilarity` is independently testable without any HTTP dependency. User Story 2 is complete.

---

## Phase 5: User Story 3 - Configuration via Settings (Priority: P3)

**Goal**: `EmbeddingAppService` reads `OpenAI:ApiKey` and `OpenAI:EmbeddingModel` from `appsettings.json` and raises a descriptive `InvalidOperationException` when either value is missing or empty.

**Independent Test**: Construct `EmbeddingAppService` with an `IConfiguration` stub that returns an empty string for `OpenAI:ApiKey` — constructor must throw `InvalidOperationException` with a message that names the missing key.

### Implementation for User Story 3

- [x] T012 [US3] Add `Guard.Against.NullOrWhiteSpace(_apiKey, "OpenAI:ApiKey", "OpenAI:ApiKey must be set in appsettings.json")` and matching guard for `_embeddingModel` to the `EmbeddingAppService` constructor in `backend/src/backend.Application/Services/EmbeddingService/EmbeddingAppService.cs`
- [x] T013 [P] [US3] Write unit tests for `EmbeddingAppService` constructor: missing `ApiKey` throws `InvalidOperationException`; empty `ApiKey` throws `InvalidOperationException`; missing `EmbeddingModel` throws `InvalidOperationException`; valid config constructs without error; in `backend/test/backend.Tests/EmbeddingServiceTests/EmbeddingAppServiceTests.cs`

**Checkpoint**: All three user stories are independently functional. Configuration errors surface at construction time with clear messages.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Meet coding standards from `docs/RULES.md` and verify the full pipeline end-to-end.

- [x] T014 [P] Add purpose XML doc comments (`/// <summary>`) to all public classes and methods across `IEmbeddingAppService.cs`, `EmbeddingAppService.cs`, `EmbeddingHelper.cs`, `EmbeddingResult.cs` per `docs/RULES.md`
- [x] T015 [P] Run `dotnet build backend/src/backend.Application/backend.Application.csproj` and fix any compilation errors — 0 errors ✅
- [x] T016 [P] Run `dotnet test backend/test/backend.Tests/backend.Tests.csproj --filter "EmbeddingService"` and confirm all tests pass — 19/19 ✅
- [ ] T017 Validate quickstart.md smoke test: construct service with real config, call `GenerateEmbeddingAsync` with a short string, assert `result.Vector.Length == 1536`, then assert `EmbeddingHelper.CosineSimilarity(result.Vector, result.Vector) ≈ 1.0`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — BLOCKS all user stories
- **User Stories (Phases 3–5)**: All depend on Phase 2 completion; US1 and US2 can run in parallel (different files); US3 builds on the constructor from US1
- **Polish (Phase 6)**: Depends on Phases 3–5 completion

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2 — no dependency on US2 or US3
- **US2 (P2)**: Can start after Phase 2 — `EmbeddingHelper.cs` is created in US1 (T006) but `CosineSimilarity` is an additive change to the same file; T010 depends on T006
- **US3 (P3)**: Depends on US1 constructor (T008) being written before adding guard clauses (T012); T012 is an in-place hardening of T008

### Within Each User Story

- Models/helpers before service implementation
- Service constructor before method body
- Implementation before tests (no TDD explicitly requested)
- Story complete before moving to next priority

### Parallel Opportunities

- T002 and T003 (Phase 1) can run in parallel
- T004 and T005 (Phase 2) can run in parallel
- T009 (US1 tests) can run in parallel with T007/T008 (US1 implementation) — different files
- T010 (US2 implementation) and T009 (US1 tests) can run in parallel
- T011 (US2 tests) can run in parallel with T010
- T013 (US3 tests) can run in parallel with T012
- T014, T015, T016 (Phase 6) can all run in parallel

---

## Parallel Example: User Story 1

```
# After Phase 2 completes, launch in parallel:
Task T006: Create EmbeddingHelper.TruncateToLimit in EmbeddingHelper.cs

# After T006, launch in parallel:
Task T007: Add private request/response models in EmbeddingAppService.cs
Task T009: Write TruncateToLimit unit tests in EmbeddingHelperTests.cs

# After T007:
Task T008: Implement GenerateEmbeddingAsync in EmbeddingAppService.cs
```

## Parallel Example: User Story 2

```
# US2 can start in parallel with US1's tests (T009) — different files:
Task T010: Add CosineSimilarity to EmbeddingHelper.cs
Task T011: Write CosineSimilarity tests in EmbeddingHelperTests.cs  ← parallel with T010
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (T006–T009)
4. **STOP and VALIDATE**: `GenerateEmbeddingAsync` returns a valid 1,536-element vector
5. Integrate with `PdfIngestionAppService` Loading stage

### Incremental Delivery

1. Phase 1 + 2 → Foundation ready
2. Phase 3 (US1) → Core embedding generation works → integrate with pipeline
3. Phase 4 (US2) → Similarity comparison available → enables retrieval scoring
4. Phase 5 (US3) → Config validation hardened → production-safe
5. Phase 6 → Polish and confirm all tests pass

### Single Developer Strategy

Complete phases sequentially: 1 → 2 → 3 → 4 → 5 → 6. Estimated task count: 17 tasks.

---

## Notes

- `[P]` tasks touch different files and have no dependency on in-progress tasks in the same phase
- `[Story]` label maps each task to its user story for traceability back to spec.md
- No new migrations or domain entities — `ChunkEmbedding` already exists from feature 004
- The OpenAI API key MUST NOT be committed to git; use `appsettings.Development.json` (git-ignored) locally
- `EmbeddingHelper` is a static class (pattern matching `PdfChunkingHelper` from feature 008)
- `EmbeddingAppService` is auto-registered by ABP DI conventions — no manual registration needed beyond the HttpClient named client
