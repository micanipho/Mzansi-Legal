# Implementation Plan: Intent-Aware Legal Retrieval for RAG Answers

**Branch**: `feat/021-intent-aware-rag` | **Date**: 2026-04-01 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `/specs/feat/021-intent-aware-rag/spec.md`

## Summary

Upgrade the existing multilingual RAG Q&A flow from chunk-only similarity filtering to intent-aware legal source discovery. The implementation keeps the current `POST /api/app/qa/ask` endpoint and existing persistence model, but adds richer in-memory source metadata, document-level reranking, explicit source-hint handling, adaptive answer modes, and dynamic generation temperature so users can ask plain-language legal questions without naming Acts explicitly. The current general-knowledge fallback will be removed in favor of clarification or grounded insufficiency responses.

## Technical Context

**Language/Version**: C# on .NET 9.0 + ABP Zero 10.x; TypeScript 5 with Next.js 16 App Router for response-consumer updates  
**Primary Dependencies**: Existing `IEmbeddingAppService`, `ILanguageAppService`, `IHttpClientFactory`, `Ardalis.GuardClauses`, `next-intl`; no new NuGet or npm packages planned  
**Storage**: PostgreSQL 15+ via Npgsql; no new migrations planned, reusing `LegalDocuments`, `Categories`, `DocumentChunks`, `ChunkEmbeddings`, `Conversations`, `Questions`, `Answers`, and `AnswerCitations`  
**Testing**: xUnit + NSubstitute + Shouldly in `backend.Tests`; frontend validation via lint/type safety and manual keyboard/screen-reader smoke on the Ask page  
**Target Platform**: Linux server deployment on Railway plus modern evergreen browsers  
**Project Type**: Web application (ABP web service + Next.js frontend)  
**Performance Goals**: Direct or cautious grounded response target <= 8 seconds under normal conditions; clarification/insufficient response <= 6 seconds; overall user-visible acceptance ceiling <= 10 seconds  
**Constraints**: No uncited general-knowledge legal fallback; preserve multilingual responses and current endpoint path; keep response contract backward-compatible for existing frontend consumers; no vector database or new infrastructure in this milestone  
**Scale/Scope**: ~13 seeded Acts / ~1,000 embedded chunks, single ask endpoint, one retrieval pipeline, small Ask-page UI enhancement for caution/clarification states

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Layer Gate**: No new domain entities are required. Retrieval helpers, enums, and DTOs stay in `backend.Application/Services/RagService/`; existing `DocumentChunk`, `LegalDocument`, `Category`, `Question`, `Answer`, and `AnswerCitation` remain in their current layers.
- [x] **Naming Gate**: Existing `IRagAppService` / `RagAppService` naming is preserved. New helper classes and DTO/enums follow established `Rag*` naming under the `RagService` folder structure.
- [x] **Coding Standards Gate**: The plan splits retrieval ranking, confidence selection, and prompt construction into focused classes to keep methods under scroll height, preserve guard clauses, avoid deep nesting, and centralize magic numbers as named constants.
- [x] **Skill Gate**: `speckit.plan`, `speckit.tasks`, and `follow-git-workflow` govern the workflow. `add-styling` applies if the Ask-page caution/clarification presentation needs a dedicated style treatment during implementation. No `add-endpoint` scaffold is needed because the feature reuses the existing endpoint.
- [x] **Multilingual Gate**: All new user-facing states (`direct`, `cautious`, `clarification`, `insufficient`) will remain localized through existing multilingual answer generation and localized frontend labels in English, isiZulu, Sesotho, and Afrikaans.
- [x] **Citation Gate**: The updated `/api/app/qa/ask` contract will explicitly define direct/cautious/clarification behavior, citation expectations for grounded answers, and the removal of general-knowledge fallback behavior.
- [x] **Accessibility Gate**: Any new frontend cue for caution or clarification will be rendered in the existing accessible chat flow using semantic alert/status regions, keyboard-accessible controls, and localized text.
- [x] **ETL/Ingestion Gate**: No ingestion pipeline stages change in this feature. Existing `DocumentChunk.Keywords` and `TopicClassification` enrichment metadata are reused read-only.

## Project Structure

### Documentation (this feature)

```text
specs/feat/021-intent-aware-rag/
|-- spec.md
|-- plan.md
|-- research.md
|-- data-model.md
|-- quickstart.md
|-- contracts/
|   `-- qa-ask.md
`-- tasks.md
```

### Source Code (repository root)

```text
backend/
`-- src/
    |-- backend.Application/
    |   `-- Services/
    |       `-- RagService/
    |           |-- IRagAppService.cs                 [MODIFY - contract unchanged]
    |           |-- RagAppService.cs                  [MODIFY]
    |           |-- RagPromptBuilder.cs               [MODIFY]
    |           |-- RagRetrievalPlanner.cs            [NEW]
    |           |-- RagConfidenceEvaluator.cs         [NEW]
    |           |-- RagSourceHintExtractor.cs         [NEW]
    |           `-- DTO/
    |               |-- RagAnswerResult.cs           [MODIFY]
    |               |-- RagCitationDto.cs            [MODIFY - comments/contract notes only if needed]
    |               |-- RagAnswerMode.cs             [NEW]
    |               `-- RagConfidenceBand.cs         [NEW]
    |-- backend.Web.Host/
    |   `-- Controllers/
    |       `-- QaController.cs                      [MODIFY - doc comments only; route unchanged]
    `-- backend.Core/                                [NO SCHEMA CHANGE]

backend/
`-- test/
    `-- backend.Tests/
        `-- RagServiceTests/
            |-- RagAppServiceTests.cs                [MODIFY]
            |-- RagPromptBuilderTests.cs             [MODIFY]
            |-- RagRetrievalPlannerTests.cs          [NEW]
            `-- RagConfidenceEvaluatorTests.cs       [NEW]

frontend/
`-- src/
    |-- services/
    |   `-- qa.service.ts                            [MODIFY]
    |-- hooks/
    |   `-- useChat.ts                               [MODIFY]
    |-- providers/
    |   `-- chat-provider/
    |       |-- context.tsx                          [MODIFY]
    |       `-- index.tsx                            [MODIFY]
    |-- components/
    |   `-- chat/
    |       `-- ChatMessage.tsx                      [MODIFY]
    `-- messages/
        |-- en.json                                  [MODIFY]
        |-- zu.json                                  [MODIFY]
        |-- st.json                                  [MODIFY]
        `-- af.json                                  [MODIFY]
```

**Structure Decision**: Keep the implementation centered in the existing backend `RagService` module and the current Ask-page chat consumer. No new route, database project, or ingestion module is introduced.

## Complexity Tracking

No constitution violations require justification.

## Implementation Strategy

### Slice 1 - Retrieval Foundation

1. Expand the in-memory chunk index so every loaded retrieval record carries:
   - `DocumentId`
   - full Act title
   - Act short name
   - Act number/year label when available
   - category name
   - section title/number
   - chunk keywords
   - topic classification
   - embedding vector
2. Replace the current "top-5 above 0.7" retrieval rule with a two-step approach:
   - wider semantic candidate generation from chunk embeddings
   - document-level reranking based on semantic strength plus metadata alignment
3. Treat explicit Act names as boosts, not hard filters, so a mistaken Act name does not override clearly stronger factual matches.

### Slice 2 - Confidence and Answer Behavior

1. Introduce a retrieval decision object that computes:
   - answer mode
   - confidence band
   - selected documents
   - selected chunks
   - optional clarification question
2. Define clear transitions:
   - `Direct`: enough aligned support to answer confidently with citations
   - `Cautious`: grounded answer available, but evidence is broad, diffuse, or only partially aligned
   - `Clarification`: likely domain identified, but a decisive fact is missing or the question is too ambiguous
   - `Insufficient`: the corpus cannot responsibly support the question
3. Use answer mode to drive both prompt wording and temperature selection.

### Slice 3 - Contract and Frontend

1. Extend `RagAnswerResult` with backward-compatible fields:
   - `answerMode`
   - `confidenceBand`
   - `clarificationQuestion`
   - existing `detectedLanguageCode` remains
2. Keep the existing endpoint and existing fields so current consumers continue to work.
3. Update the Ask-page chat UI to surface caution or clarification states with localized copy instead of relying only on raw answer text.

### Slice 4 - Testing and Calibration

1. Add deterministic unit tests for:
   - source hint extraction
   - document reranking
   - confidence/mode selection
   - prompt selection and temperature policy
   - removal of the general-knowledge fallback
2. Add integration-style assertions for semantically equivalent questions resolving to the same primary source.
3. Run manual smoke checks on the Ask page for keyboard navigation, localized caution copy, and citation rendering.

## Implementation Steps

### Step 1 - Canonicalize the Retrieval Index

**Action**:
1. Update the startup load in `RagAppService.InitialiseAsync()` to include `Document.Category`.
2. Replace the current minimal loaded record with a richer indexed chunk shape.
3. Parse `DocumentChunk.Keywords` once at load time into a reusable normalized collection.

**Expected Result**: Retrieval logic can reason about document title, short name, category, and enrichment metadata without extra database calls during `AskAsync()`.

---

### Step 2 - Add Explicit Source Hint Extraction

**Action**:
1. Create `RagSourceHintExtractor.cs`.
2. Match translated question text against known document titles, short names, and common title fragments.
3. Capture category-level hints from metadata terms when present.
4. Ensure hint matches boost ranking but do not exclude stronger semantically matched documents.

**Expected Result**: A user can name an Act if they know it, but they do not have to.

---

### Step 3 - Implement Hybrid Document Reranking

**Action**:
1. Create `RagRetrievalPlanner.cs`.
2. Generate a wider semantic candidate pool from chunk embeddings using a softer floor than the final answer threshold.
3. Group candidates by document and compute a document score from:
   - top chunk similarity
   - average strength of the top chunks
   - topic/category alignment
   - keyword overlap
   - explicit source hint boost
4. Select the strongest primary document plus supporting documents when they materially contribute new legal support.
5. Cap per-document chunk count so one long Act does not crowd out genuinely relevant supporting sources.

**Expected Result**: Source selection becomes document-aware and better aligned to meaning rather than only lexical proximity.

---

### Step 4 - Add Confidence and Answer Mode Evaluation

**Action**:
1. Create `RagConfidenceEvaluator.cs`.
2. Compute confidence from retrieval signals such as:
   - primary document score
   - score gap between the top document and the next alternative
   - number of aligned high-scoring chunks
   - whether selected chunks agree on topic/category
   - whether the question remains ambiguous after source selection
3. Map confidence to `RagAnswerMode` and `RagConfidenceBand`.

**Expected Result**: The service makes a deterministic, testable decision about whether to answer directly, answer cautiously, ask for clarification, or decline.

---

### Step 5 - Update Prompt and Temperature Policy

**Action**:
1. Extend `RagPromptBuilder` to support mode-specific system instructions.
2. Define bounded temperature policy:
   - `Direct`: `0.2`
   - `Cautious`: `0.1`
   - `Clarification`: `0.0`
   - `Insufficient`: no answer-generation call when a templated insufficiency response is safer
3. Keep citation instructions strict for `Direct` and `Cautious`.
4. For `Clarification`, instruct the model to ask one focused follow-up question and avoid legal conclusions.

**Expected Result**: Response tone becomes more conservative as certainty drops, without making grounded answers robotic.

---

### Step 6 - Refactor `RagAppService.AskAsync()`

**Action**:
1. Keep the existing multilingual flow: detect language, translate to English for search.
2. Replace the current single-threshold retrieval path with:
   - semantic candidate generation
   - source hint extraction
   - retrieval planning
   - confidence evaluation
3. Remove the current general-knowledge fallback path entirely.
4. Return either:
   - grounded direct answer
   - grounded cautious answer
   - clarification response
   - explicit insufficient-grounding response
5. Persist only grounded direct/cautious answers in the existing Q&A chain to preserve current `AnswerId` expectations.

**Expected Result**: The endpoint behaves safely when grounding is weak and materially better when users do not mention Act names.

---

### Step 7 - Extend the API DTO Contract

**Action**:
1. Add `RagAnswerMode` and `RagConfidenceBand` enums.
2. Extend `RagAnswerResult` with:
   - `AnswerMode`
   - `ConfidenceBand`
   - `ClarificationQuestion`
3. Keep existing fields intact (`answerText`, `isInsufficientInformation`, `citations`, `chunkIds`, `answerId`, `detectedLanguageCode`).
4. Update controller comments and contract docs to reflect the new behavior.

**Expected Result**: The frontend gets structured state for caution/clarification while existing consumers remain compatible.

---

### Step 8 - Update the Ask-Page Consumer

**Action**:
1. Extend `frontend/src/services/qa.service.ts` types with the new response fields.
2. Propagate the fields through `useChat`, `chat-provider`, and `ChatMessage`.
3. Add localized labels/messages for:
   - clarification needed
   - cautious answer
   - insufficient information
4. Render the new state with semantic alert/status treatment and no extra interaction cost.

**Expected Result**: Users can immediately tell when the system is answering confidently versus asking for more detail.

---

### Step 9 - Add Backend Tests

**Action**:
1. Add `RagRetrievalPlannerTests.cs`:
   - plain-language eviction phrasing selects housing/constitutional sources
   - semantically equivalent variants keep the same primary source
   - wrong Act hint does not outrank stronger factual matches
2. Add `RagConfidenceEvaluatorTests.cs`:
   - high-confidence document set -> `Direct`
   - mixed but grounded support -> `Cautious`
   - ambiguous request -> `Clarification`
   - no responsible grounding -> `Insufficient`
3. Extend `RagAppServiceTests.cs` and `RagPromptBuilderTests.cs` for dynamic temperature and no-general-fallback behavior.

**Expected Result**: The retrieval and response-mode decisions are protected by deterministic tests.

---

### Step 10 - Verify End-to-End Behavior

**Action**:
1. Run backend tests.
2. Smoke-test the Ask page with:
   - `"Can my landlord evict me without a court order?"` -> direct answer with citations
   - `"Can they evict me?"` -> clarification
   - semantically equivalent eviction variants -> same primary legal source
   - unsupported topic -> insufficient response with no general legal advice
3. Check localized UI labels for all supported languages.

**Expected Result**: The feature meets the spec's user-facing behavior before task breakdown begins.

## Dependencies & Order

```text
Step 1  (index metadata)         -> existing RAG + document/category models
Step 2  (source hints)           -> Step 1
Step 3  (retrieval planner)      -> Steps 1, 2
Step 4  (confidence evaluator)   -> Step 3
Step 5  (prompt/temperature)     -> Step 4
Step 6  (RagAppService flow)     -> Steps 3, 4, 5
Step 7  (DTO + contract)         -> Step 6
Step 8  (frontend consumer)      -> Step 7
Step 9  (tests)                  -> Steps 3, 4, 5, 6, 7
Step 10 (smoke verification)     -> Steps 6, 7, 8, 9
```

## Critical Path

```text
Indexed metadata -> hybrid retrieval planner -> confidence/mode selection -> prompt policy
-> RagAppService refactor -> DTO contract -> tests -> frontend cue -> smoke verification
```

## Failure Handling

| Failure | Diagnosis | Resolution |
|---------|-----------|------------|
| Plain-language questions still require Act names | Document reranking still overweights explicit hints or exact wording | Reduce hint boost, widen semantic candidate pool, and recalibrate source scoring against benchmark prompts |
| Wrong Act name in the question dominates the result | Hint extraction is acting as a filter instead of a boost | Keep hint signals additive only; never discard stronger semantic candidates |
| Broad questions still receive overconfident answers | Confidence evaluator is too permissive | Tighten confidence thresholds and route more cases to `Cautious` or `Clarification` |
| Retrieval quality improves but latency regresses | Candidate pool or context size is too large | Cap candidate pool, cap chunks per document, and skip generation for `Insufficient` responses |
| Frontend shows no distinction between direct and clarification modes | New DTO fields are not propagated through chat state | Update `qa.service.ts`, chat state types, and `ChatMessage.tsx` together |
| Existing clients break on response changes | Response shape became incompatible | Keep all existing fields and add only optional/append-only metadata |

## Deliverables

| Deliverable | Location |
|-------------|----------|
| Research decision log | `specs/feat/021-intent-aware-rag/research.md` |
| Data model/design notes | `specs/feat/021-intent-aware-rag/data-model.md` |
| Quick implementation guide | `specs/feat/021-intent-aware-rag/quickstart.md` |
| API contract | `specs/feat/021-intent-aware-rag/contracts/qa-ask.md` |
| Implementation plan | `specs/feat/021-intent-aware-rag/plan.md` |
