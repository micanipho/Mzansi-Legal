# Implementation Plan: Intent-Aware Legal Retrieval for RAG Answers

**Branch**: `feat/021-intent-aware-rag` | **Date**: 2026-04-01 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `/specs/feat/021-intent-aware-rag/spec.md`

## Summary

Refine the existing multilingual legal RAG pipeline in measured slices instead of rebuilding it. The current `POST /api/app/qa/ask` flow already performs translation, embeddings, document-aware planning, answer-mode selection, and conditional persistence; this plan focuses on tightening the parts that determine whether the system finds the right Act from meaning, stays conservative when evidence is weak, and exposes that state clearly in the Ask UI. The removed general-knowledge fallback stays removed, no new schema or infrastructure is introduced, and all changes remain backward-compatible for existing consumers.

## Technical Context

**Language/Version**: C# on .NET 9.0 + ABP Zero 10.x; TypeScript 5 with Next.js 16 App Router for response-consumer updates  
**Primary Dependencies**: Existing `IEmbeddingAppService`, `ILanguageAppService`, `IHttpClientFactory`, `Ardalis.GuardClauses`, `next-intl`, plus current RAG helpers `RagIndexStore`, `RagDocumentProfileBuilder`, `RagSourceHintExtractor`, `RagQueryFocusBuilder`, `RagRetrievalPlanner`, and `RagConfidenceEvaluator`; no new NuGet or npm packages planned  
**Storage**: PostgreSQL 15+ via Npgsql; no new migrations planned, reusing `LegalDocuments`, `Categories`, `DocumentChunks`, `ChunkEmbeddings`, `Conversations`, `Questions`, `Answers`, and `AnswerCitations`  
**Testing**: xUnit + NSubstitute + Shouldly in `backend.Tests`; frontend validation via lint/type safety, manual keyboard/screen-reader smoke on the Ask page, and a benchmark prompt matrix captured in feature docs/tests  
**Target Platform**: Linux server deployment on Railway plus modern evergreen browsers  
**Project Type**: Web application (ABP web service + Next.js frontend)  
**Performance Goals**: Direct or cautious grounded response target <= 8 seconds under normal conditions; clarification/insufficient response <= 6 seconds; overall user-visible acceptance ceiling <= 10 seconds  
**Constraints**: No uncited general-knowledge legal fallback; preserve multilingual responses and current endpoint path; keep response contract backward-compatible; no vector database or new infrastructure in this milestone; current corpus remains legislation-first with no case-law ingestion changes in this feature  
**Scale/Scope**: ~13 seeded Acts / ~1,000 embedded chunks, single ask endpoint, one retrieval pipeline, small Ask-page UI enhancement for caution/clarification states, and benchmark-led calibration of existing retrieval weights

## Current System Snapshot

- `RagAppService` already detects language, translates to English for search, embeds the question, plans retrieval, evaluates confidence, generates grounded answers or clarification prompts, and persists only grounded answers.
- `RagIndexStore`, `RagDocumentProfileBuilder`, `RagSourceHintExtractor`, `RagQueryFocusBuilder`, `RagRetrievalPlanner`, and `RagConfidenceEvaluator` are already present in the backend and covered by unit tests.
- The next planning focus is not scaffolding; it is calibration and hardening: benchmark coverage, retrieval weight tuning, safer answer-mode thresholds, Ask-page representation, and rollout criteria.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Layer Gate**: No new domain entities are required. Refinements stay in `backend.Application/Services/RagService/`, existing DTOs, controller comments, and the current frontend Ask flow.
- [x] **Naming Gate**: Existing `IRagAppService` / `RagAppService` naming is preserved. Helper classes and enums continue using the established `Rag*` naming pattern.
- [x] **Coding Standards Gate**: The refinement path keeps ranking, confidence evaluation, prompt construction, and orchestration in focused classes so methods remain short, guard clauses stay at entry points, and scoring constants stay centralized.
- [x] **Skill Gate**: `speckit.plan`, `speckit.tasks`, and `follow-git-workflow` govern the workflow. `add-styling` applies only if the Ask-page state treatment needs dedicated styling work during implementation.
- [x] **Multilingual Gate**: All new user-facing states (`direct`, `cautious`, `clarification`, `insufficient`) remain localized through existing multilingual answer generation and frontend locale files in English, isiZulu, Sesotho, and Afrikaans.
- [x] **Citation Gate**: The `/api/app/qa/ask` contract continues to require citations for grounded legal claims and explicitly forbids the removed general-knowledge legal fallback.
- [x] **Accessibility Gate**: Any caution or clarification UI treatment remains within the accessible chat flow using semantic alert/status regions, keyboard-accessible controls, and localized text.
- [x] **ETL/Ingestion Gate**: No ingestion pipeline stages change in this feature. Existing `DocumentChunk.Keywords`, `TopicClassification`, `LegalDocument` metadata, and document/category relationships are reused read-only.

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
    |           |-- IRagAppService.cs                 [VERIFY]
    |           |-- RagAppService.cs                  [REFINE]
    |           |-- RagPromptBuilder.cs               [REFINE]
    |           |-- RagQueryFocusBuilder.cs           [REFINE]
    |           |-- RagSourceHintExtractor.cs         [REFINE]
    |           |-- RagDocumentProfileBuilder.cs      [REFINE]
    |           |-- RagRetrievalPlanner.cs            [REFINE]
    |           |-- RagConfidenceEvaluator.cs         [REFINE]
    |           |-- RagIndexStore.cs                  [VERIFY]
    |           `-- DTO/
    |               |-- RagAnswerResult.cs            [VERIFY/REFINE]
    |               |-- RagCitationDto.cs             [VERIFY]
    |               |-- RagAnswerMode.cs              [VERIFY]
    |               `-- RagConfidenceBand.cs          [VERIFY]
    |-- backend.Web.Host/
    |   `-- Controllers/
    |       `-- QaController.cs                       [VERIFY]
    `-- backend.Core/                                 [NO SCHEMA CHANGE]

backend/
`-- test/
    `-- backend.Tests/
        `-- RagServiceTests/
            |-- RagAppServiceTests.cs                 [EXPAND]
            |-- RagPromptBuilderTests.cs              [EXPAND]
            |-- RagQueryFocusBuilderTests.cs          [EXPAND]
            |-- RagDocumentProfileBuilderTests.cs     [EXPAND]
            |-- RagRetrievalPlannerTests.cs           [EXPAND]
            |-- RagConfidenceEvaluatorTests.cs        [EXPAND]
            `-- RagIndexStoreTests.cs                 [VERIFY]

frontend/
`-- src/
    |-- services/
    |   `-- qa.service.ts                             [VERIFY/REFINE]
    |-- hooks/
    |   `-- useChat.ts                                [VERIFY/REFINE]
    |-- providers/
    |   `-- chat-provider/
    |       |-- context.tsx                           [VERIFY/REFINE]
    |       `-- index.tsx                             [VERIFY/REFINE]
    |-- components/
    |   `-- chat/
    |       `-- ChatMessage.tsx                       [VERIFY/REFINE]
    `-- messages/
        |-- en.json                                   [VERIFY/REFINE]
        |-- zu.json                                   [VERIFY/REFINE]
        |-- st.json                                   [VERIFY/REFINE]
        `-- af.json                                   [VERIFY/REFINE]
```

**Structure Decision**: Keep the implementation centered in the existing backend `RagService` module and the current Ask-page chat consumer. The system is refined in place; no new route, database project, or ingestion subsystem is introduced.

## Complexity Tracking

No constitution violations require justification.

## Bit-by-Bit Refinement Map

| Slice | Goal | Exit Signal |
|-------|------|-------------|
| 1. Retrieval Inputs | Stabilize indexed chunk metadata, document profiles, and focus-query behavior | Indexed metadata remains deterministic across startup loads and supports document/category-aware ranking without extra DB calls |
| 2. Ranking Calibration | Tune document reranking, hint boosts, and supporting-document selection | Plain-language, wrong-Act-hint, and multi-source benchmark prompts consistently select the expected primary source family |
| 3. Safety Modes | Tighten `Direct`, `Cautious`, `Clarification`, and `Insufficient` thresholds | Ambiguous prompts stop routing to `Direct`; clarification and insufficiency are treated as correct safety outcomes |
| 4. Contract + UI | Keep the contract append-only while exposing mode/confidence clearly in the Ask flow | Frontend state and localized copy reflect answer posture in all four locales without breaking existing consumers |
| 5. Verification | Lock behavior with tests and smoke checks | Backend tests pass, benchmark snapshot is recorded, and Ask-page smoke cases match the contract |

## Implementation Strategy

### Slice 1 - Stabilize the Retrieval Baseline

1. Treat the current retrieval surface as the starting point:
   - `RagIndexStore`
   - `RagDocumentProfileBuilder`
   - `RagQueryFocusBuilder`
   - `RagSourceHintExtractor`
   - `RagRetrievalPlanner`
   - `RagConfidenceEvaluator`
2. Verify that startup load and metadata normalization are deterministic:
   - document/category loading
   - keyword parsing
   - alias generation
   - document centroid vectors
3. Keep all retrieval metadata in memory so `AskAsync()` stays DB-free after startup.

### Slice 2 - Calibrate Document-Aware Ranking

1. Tune the weighting between:
   - chunk semantic similarity
   - document centroid similarity
   - metadata alignment
   - keyword overlap
   - source hint boost
   - semantic breadth
2. Keep explicit Act names as boosts only, never as hard filters.
3. Limit supporting documents and per-document chunk counts so the prompt remains focused.

### Slice 3 - Tighten Confidence and Safety Behavior

1. Re-evaluate answer-mode thresholds using benchmark prompts across:
   - plain-language direct answers
   - broad ambiguous questions
   - wrong-source hints
   - unsupported topics
2. Treat `Clarification` and `Insufficient` as first-class safe responses rather than failure cases.
3. Keep persistence limited to grounded `Direct` and `Cautious` answers.

### Slice 4 - Preserve the Contract While Surfacing State

1. Keep `RagAnswerResult` append-only for consumers:
   - `answerMode`
   - `confidenceBand`
   - `clarificationQuestion`
   - existing fields unchanged
2. Keep `POST /api/app/qa/ask` as the only endpoint for this feature.
3. Ensure the Ask-page UI clearly communicates caution or clarification states with localized, accessible copy.

### Slice 5 - Benchmark and Verify Before Task Breakdown

1. Maintain a lightweight benchmark prompt matrix aligned to the research decisions:
   - plain-language questions without Act names
   - semantically equivalent variants
   - wrong explicit Act names
   - multi-source questions
   - ambiguous prompts
   - unsupported questions
2. Use that matrix to guide threshold tuning and regression tests before new implementation tasks are declared complete.
3. Capture deferred follow-ons separately instead of expanding this milestone:
   - court-hierarchy weighting once judgments join the corpus
   - human review sampling/telemetry persistence
   - broader operational analytics

## Implementation Steps

### Step 1 - Reconstruct and Lock the Current Baseline

**Action**:
1. Restore the filled plan and align it to the existing backend/frontend files.
2. Confirm the current service surface and test suite match the intended design.
3. Record the benchmark scenarios that will drive calibration.

**Expected Result**: Planning artifacts reflect the real system state instead of a greenfield scaffold.

---

### Step 2 - Verify the In-Memory Retrieval Inputs

**Action**:
1. Review `RagAppService.InitialiseAsync()` and `RagIndexStore` to confirm the startup load remains atomic and DB-free after initialization.
2. Revisit `RagDocumentProfileBuilder` to ensure document profiles carry stable metadata phrases, aliases, and centroid vectors.
3. Revisit `RagQueryFocusBuilder` to confirm focus terms narrow broad queries without deleting the user's actual meaning.

**Expected Result**: Retrieval starts from a clean, deterministic in-memory representation of documents and chunks.

---

### Step 3 - Refine Source Hints and Document Ranking

**Action**:
1. Recalibrate `RagSourceHintExtractor` boost weights for Act titles, short names, Act numbers, and categories.
2. Recalibrate `RagRetrievalPlanner` weighting across semantic, metadata, keyword, and breadth signals.
3. Ensure supporting documents are only included when they add distinct legal coverage rather than duplicate the primary source.

**Expected Result**: Plain-language and wrong-hint scenarios still converge on the correct governing Act or Act set.

---

### Step 4 - Recalibrate Confidence and Answer Modes

**Action**:
1. Revisit `RagConfidenceEvaluator` thresholds for `Direct`, `Cautious`, `Clarification`, and `Insufficient`.
2. Keep ambiguity detection strict for short, broad, or closely competing source families.
3. Preserve the rule that only grounded `Direct` and `Cautious` answers are persisted.

**Expected Result**: The system becomes more conservative when the legal signal is weak or diffuse, without suppressing confident grounded answers.

---

### Step 5 - Harden Prompt and Response Behavior

**Action**:
1. Reconfirm `RagPromptBuilder` instructions per mode.
2. Keep bounded temperature policy:
   - `Direct`: `0.2`
   - `Cautious`: `0.1`
   - `Clarification`: `0.0`
   - `Insufficient`: no open-ended legal generation
3. Ensure clarification uses one focused follow-up question and insufficiency stays deterministic.

**Expected Result**: The answer posture tracks retrieval certainty and remains legally conservative.

---

### Step 6 - Keep the Service Orchestration Coherent

**Action**:
1. Verify `RagAppService.AskAsync()` remains ordered as:
   - detect language
   - translate to English for search
   - embed the question
   - build semantic matches
   - extract source hints
   - build retrieval plan
   - evaluate confidence
   - choose response mode
   - persist only grounded answers
2. Confirm non-grounded responses never fabricate citations or produce general-knowledge legal advice.
3. Keep `IRagAppService` and controller docs aligned with the refined behavior.

**Expected Result**: The backend flow stays simple, deterministic, and safe from retrieval to response.

---

### Step 7 - Verify Contract and Ask-Page Representation

**Action**:
1. Confirm `RagAnswerResult` and `qa-ask.md` still match the implemented backend behavior.
2. Verify the Ask-page consumer propagates `answerMode`, `confidenceBand`, and `clarificationQuestion` through service types, provider state, and message rendering.
3. Confirm all four locale files provide clear caution/clarification copy with semantic status presentation.

**Expected Result**: Users can tell when the system is answering directly, being cautious, or asking for more detail.

---

### Step 8 - Expand Regression and Benchmark Coverage

**Action**:
1. Expand `RagRetrievalPlannerTests.cs` and `RagConfidenceEvaluatorTests.cs` with benchmark-style cases:
   - plain-language eviction phrasing
   - semantically equivalent variants
   - wrong Act hint
   - multi-source answer
   - ambiguous short prompt
   - unsupported topic
2. Expand `RagPromptBuilderTests.cs`, `RagQueryFocusBuilderTests.cs`, and `RagAppServiceTests.cs` for mode-specific behavior and non-persistence rules.
3. Keep test names tied to user-observable behavior rather than just internal scores.

**Expected Result**: Retrieval and safety behavior are locked down by deterministic tests, not memory of manual tuning.

---

### Step 9 - Run the Final Verification Loop

**Action**:
1. Run backend tests.
2. Smoke-test the Ask page with:
   - `"Can my landlord evict me without a court order?"` -> direct answer with citations
   - `"Can they evict me?"` -> clarification
   - semantically equivalent eviction variants -> same primary legal source family
   - wrong-source hint question -> stronger factual source still wins
   - unsupported topic -> insufficient response with no general legal advice
3. Record any deferred follow-ons instead of stretching this milestone.

**Expected Result**: The feature is ready for task breakdown and implementation refinement with a clear benchmark and verification trail.

## Dependencies & Order

```text
Step 1  (restore baseline docs)          -> current feature artifacts + existing code
Step 2  (verify retrieval inputs)        -> Step 1
Step 3  (ranking calibration)            -> Step 2
Step 4  (confidence calibration)         -> Step 3
Step 5  (prompt behavior)                -> Step 4
Step 6  (service orchestration)          -> Steps 3, 4, 5
Step 7  (contract + Ask UI verification) -> Step 6
Step 8  (tests + benchmark expansion)    -> Steps 3, 4, 5, 6, 7
Step 9  (final verification loop)        -> Step 8
```

## Critical Path

```text
Retrieval inputs -> ranking calibration -> confidence calibration -> prompt behavior
-> service orchestration verification -> contract/UI verification -> tests -> final smoke check
```

## Failure Handling

| Failure | Diagnosis | Resolution |
|---------|-----------|------------|
| Plain-language questions still require Act names | Metadata alignment or ranking weights still underweight semantic meaning | Widen candidate pool, rebalance metadata/semantic weights, and retest against benchmark prompts |
| Wrong Act name dominates the result | Hint boosts are too strong | Keep hint signals additive only and lower boost caps until factual matches win |
| Ambiguous prompts still receive `Direct` answers | Confidence thresholds or ambiguity heuristics are too permissive | Raise direct-answer thresholds and route borderline cases to `Cautious` or `Clarification` |
| Multi-source questions collapse to one partial Act | Supporting-document inclusion is too strict | Allow a second source only when it adds distinct legal coverage and citation value |
| Ask page hides the response posture | DTO fields are not fully propagated or localized | Verify `qa.service.ts`, provider state, `ChatMessage.tsx`, and locale strings together |
| Benchmarks pass locally but behavior drifts later | Calibration exists only in human memory | Keep benchmark scenarios in tests/spec artifacts and rerun them before closing tasks |

## Deliverables

| Deliverable | Location |
|-------------|----------|
| Refined research decision log | `specs/feat/021-intent-aware-rag/research.md` |
| Refined data model/design notes | `specs/feat/021-intent-aware-rag/data-model.md` |
| Refined quick implementation guide | `specs/feat/021-intent-aware-rag/quickstart.md` |
| API contract | `specs/feat/021-intent-aware-rag/contracts/qa-ask.md` |
| Refined implementation plan | `specs/feat/021-intent-aware-rag/plan.md` |
