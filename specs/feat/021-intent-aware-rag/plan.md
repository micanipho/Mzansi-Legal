# Implementation Plan: Intent-Aware Legal Retrieval for RAG Answers

**Branch**: `feat/021-intent-aware-rag` | **Date**: 2026-04-01 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `/specs/feat/021-intent-aware-rag/spec.md`

## Summary

Refine the existing South Africa-grounded multilingual RAG pipeline in measured slices instead of rebuilding it. The current `POST /api/app/qa/ask` flow already performs language detection, translation, embeddings, document-aware retrieval, answer-mode selection, and conditional persistence. This plan tightens the parts that determine whether the system finds the right legal source from plain-language facts, distinguishes binding law from official guidance, becomes more cautious when the stakes or ambiguity are high, and shows that state clearly in the Ask UI. The legislation-corpus research is incorporated as planning input: current retrieval tuning is anchored to the documents already seeded in the repo, while missing high-value bundles are recorded as follow-on corpus expansion work through the existing ETL path rather than hidden inside ranking assumptions.

## Technical Context

**Language/Version**: C# on .NET 9.0 + ABP Zero 10.x; TypeScript 5 with Next.js 16 App Router for Ask-page updates  
**Primary Dependencies**: Existing `IEmbeddingAppService`, `ILanguageAppService`, `IHttpClientFactory`, `Ardalis.GuardClauses`, `next-intl`, plus current RAG helpers `RagIndexStore`, `RagDocumentProfileBuilder`, `RagSourceHintExtractor`, `RagQueryFocusBuilder`, `RagRetrievalPlanner`, and `RagConfidenceEvaluator`; existing manifest and ETL pieces `LegislationManifest`, `LegalDocumentRegistrar`, and `IngestionJob` remain the path for any later corpus additions  
**Storage**: PostgreSQL 15+ via Npgsql; current feature slice reuses `LegalDocuments`, `Categories`, `DocumentChunks`, `ChunkEmbeddings`, `Conversations`, `Questions`, `Answers`, `AnswerCitations`, and `IngestionJobs`; no new migration planned for retrieval hardening  
**Testing**: xUnit + NSubstitute + Shouldly in `backend.Tests`; frontend validation via lint and type-safety; benchmark matrix and manual Ask-page smoke checks split between in-corpus and follow-on scenarios  
**Target Platform**: Linux server deployment on Railway plus modern evergreen browsers  
**Project Type**: Web application (ABP web service + Next.js frontend)  
**Performance Goals**: Direct or cautious grounded response target <= 8 seconds under normal conditions; clarification or insufficient response <= 6 seconds; overall user-visible ceiling <= 10 seconds  
**Constraints**: No uncited general-knowledge legal fallback; preserve multilingual responses and current endpoint path; keep response contract append-only; no new vector database or schema change for the current slice; any corpus additions must flow through the existing ETL/IngestionJob path; public-source-first and licensing-aware posture for future legal document ingestion  
**Scale/Scope**: Current seeded corpus is 13 document stubs across employment, housing, consumer, debt, privacy, safety, tax, and financial guidance; one ask endpoint; one Ask-page consumer; corpus expansion bundles from legislation research are planned follow-ons rather than hidden assumptions

## Current System Snapshot

- `RagAppService` already detects language, translates to English for search, embeds the question, plans retrieval, evaluates confidence, generates grounded answers or clarification prompts, and persists only grounded answers.
- `RagIndexStore`, `RagDocumentProfileBuilder`, `RagSourceHintExtractor`, `RagQueryFocusBuilder`, `RagRetrievalPlanner`, and `RagConfidenceEvaluator` already exist in the backend and are covered by unit tests.
- The current frontend Ask flow already consumes `answerMode`, `confidenceBand`, `clarificationQuestion`, citations, and detected language in the existing chat provider and message renderer.
- The feature planning focus is not scaffolding. It is calibration and hardening: ranking, source-role labeling, safer answer-mode thresholds, urgent escalation behavior, benchmark coverage, and a clear boundary between present corpus support and future source-bundle expansion.

## Corpus Reality Snapshot

### Current seeded source families

- Employment and labour: `LRA`, `BCEA`
- Housing: `RHA`
- Consumer and debt: `CPA`, `NCA`
- Privacy: `POPIA`
- Safety and harassment: `PHA`
- Tax and guidance mix: `TAA`, `SARS Guide`
- Constitutional baseline: `Constitution`
- Financial guidance and regulatory material: `FAIS`, `PFA`, `FSCA Materials`

### High-value gaps identified by `docs/research_legislation.md`

- Housing and eviction procedure: `PIE`
- Labour procedure: `CCMA` rules and Gazette forms
- Family safety: `Domestic Violence Act` and official DOJ forms
- Small claims: `Small Claims Courts Act` and DOJ guidance
- Administrative review and information access: `PAJA`, `PAIA`, Information Regulator forms
- Criminal rights: `Criminal Procedure Act`
- Maintenance and related forms
- Later authority layer: Constitutional Court and SCA judgments

### Planning consequence

The current plan treats corpus gaps as explicit follow-on work. Retrieval hardening must pass against documents that already exist, while benchmarks that depend on missing bundles are tracked as `NeedsCorpusExpansion` or `Escalate` rather than being misclassified as ranking failures.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Layer Gate**: No new persisted entities are required for the current slice. Refinements stay in `backend.Application/Services/RagService/`, existing DTOs, controller docs, seed metadata, and the current Ask-page consumer.
- [x] **Naming Gate**: Existing `IRagAppService` / `RagAppService` naming is preserved. Any additional computed models or DTO fields will continue the established `Rag*` naming pattern.
- [x] **Coding Standards Gate**: Ranking, confidence evaluation, source labeling, prompt construction, and orchestration remain separated into focused classes so methods stay short, guard clauses stay at entry points, and scoring constants stay centralized.
- [x] **Skill Gate**: `speckit.plan`, `speckit.tasks`, and `follow-git-workflow` govern the workflow. `add-styling` applies only if Ask-page status rendering needs dedicated styling work during implementation.
- [x] **Multilingual Gate**: All new user-facing states and labels remain localized through the current multilingual answer pipeline and the frontend locale files in English, isiZulu, Sesotho, and Afrikaans.
- [x] **Citation Gate**: The `/api/app/qa/ask` contract continues to require citations for grounded legal claims, distinguishes binding law from official guidance, and explicitly forbids general-knowledge legal fallback behavior.
- [x] **Accessibility Gate**: Any caution, clarification, escalation, or source-role UI treatment remains inside the accessible chat flow using semantic alert or status regions, keyboard-accessible controls, and localized text.
- [x] **ETL/Ingestion Gate**: The retrieval-hardening slice does not introduce a new ingestion path. Any follow-on source bundle additions must continue to use `LegislationManifest`, the current ETL pipeline, and `IngestionJob` stage tracking.

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
|-- checklists/
|   `-- requirements.md
`-- tasks.md
```

### Source Code (repository root)

```text
backend/
|-- src/
|   |-- backend.Application/
|   |   `-- Services/
|   |       `-- RagService/
|   |           |-- IRagAppService.cs                 [VERIFY/REFINE]
|   |           |-- RagAppService.cs                  [REFINE]
|   |           |-- RagPromptBuilder.cs               [REFINE]
|   |           |-- RagQueryFocusBuilder.cs           [REFINE]
|   |           |-- RagSourceHintExtractor.cs         [REFINE]
|   |           |-- RagDocumentProfileBuilder.cs      [REFINE]
|   |           |-- RagRetrievalPlanner.cs            [REFINE]
|   |           |-- RagConfidenceEvaluator.cs         [REFINE]
|   |           |-- RagIndexStore.cs                  [VERIFY]
|   |           `-- DTO/
|   |               |-- AskQuestionRequest.cs         [VERIFY]
|   |               |-- RagAnswerResult.cs            [REFINE]
|   |               |-- RagCitationDto.cs             [REFINE]
|   |               |-- RagAnswerMode.cs              [VERIFY]
|   |               `-- RagConfidenceBand.cs          [VERIFY]
|   |-- backend.Web.Host/
|   |   `-- Controllers/
|   |       `-- QaController.cs                       [VERIFY/REFINE]
|   |-- backend.Core/
|   |   `-- Domains/
|   |       |-- LegalDocuments/
|   |       |   |-- Category.cs                       [VERIFY]
|   |       |   |-- LegalDocument.cs                  [VERIFY]
|   |       |   `-- DocumentChunk.cs                  [VERIFY]
|   |       |-- QA/
|   |       |   |-- Conversation.cs                   [VERIFY]
|   |       |   |-- Question.cs                       [VERIFY]
|   |       |   |-- Answer.cs                         [VERIFY]
|   |       |   `-- AnswerCitation.cs                 [VERIFY]
|   |       `-- ETL/
|   |           `-- IngestionJob.cs                   [VERIFY/FOLLOW-ON]
|   `-- backend.EntityFrameworkCore/
|       `-- EntityFrameworkCore/
|           `-- Seed/
|               `-- Host/
|                   |-- LegislationManifest.cs        [VERIFY/FOLLOW-ON]
|                   `-- LegalDocumentRegistrar.cs     [VERIFY/FOLLOW-ON]
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
    |   `-- qa.service.ts                             [REFINE]
    |-- hooks/
    |   `-- useChat.ts                                [REFINE]
    |-- providers/
    |   `-- chat-provider/
    |       |-- context.tsx                           [REFINE]
    |       `-- index.tsx                             [REFINE]
    |-- components/
    |   `-- chat/
    |       `-- ChatMessage.tsx                       [REFINE]
    `-- messages/
        |-- en.json                                   [REFINE]
        |-- zu.json                                   [REFINE]
        |-- st.json                                   [REFINE]
        `-- af.json                                   [REFINE]
```

**Structure Decision**: Keep implementation centered in the existing backend `RagService` module and the current Ask-page chat consumer. Retrieval hardening, source labeling, and safe escalation are refined in place. Any later corpus expansion reuses the current manifest and ETL pipeline rather than introducing a separate document-loading subsystem.

## Complexity Tracking

No constitution violations require justification.

## Bit-by-Bit Refinement Map

| Slice | Goal | Exit Signal |
|-------|------|-------------|
| 1. Retrieval Inputs and Source Classification | Stabilize indexed metadata, document profiles, focus-query behavior, and derived source labels such as law vs guidance | Documents can be grouped into source families and authority types without extra DB calls or schema changes |
| 2. Ranking Calibration | Tune document reranking, hint boosts, and supporting-source selection against the current seeded corpus | Plain-language, wrong-hint, and guidance-vs-law scenarios consistently select the expected primary source family |
| 3. Safety and Escalation | Tighten `Direct`, `Cautious`, `Clarification`, and `Insufficient` thresholds with explicit risk-trigger handling | Ambiguous or urgent prompts stop routing to overconfident direct answers |
| 4. Contract and UI | Keep the ask contract append-only while exposing source-role labels, answer posture, confidence, and escalation copy in the Ask flow | Users can tell whether the response is grounded law, supporting guidance, or a clarification or escalation state |
| 5. Corpus Expansion Roadmap | Translate the legislation research into explicit follow-on source bundles and ingestion rules | Missing-source scenarios are documented as coverage gaps, not treated as hidden retrieval defects |
| 6. Verification | Lock behavior with tests and a benchmark matrix split by coverage state | Backend tests pass, frontend validation passes, and benchmark scenarios clearly separate current support from deferred bundles |

## Implementation Strategy

### Slice 1 - Stabilize the Retrieval and Labeling Baseline

1. Treat the current retrieval surface as the starting point:
   - `RagIndexStore`
   - `RagDocumentProfileBuilder`
   - `RagQueryFocusBuilder`
   - `RagSourceHintExtractor`
   - `RagRetrievalPlanner`
   - `RagConfidenceEvaluator`
2. Verify that startup load and metadata normalization remain deterministic:
   - document and category loading
   - keyword parsing
   - alias generation
   - document centroid vectors
   - source-family grouping
3. Derive source authority labels from the existing corpus manifest and curated document knowledge without introducing new schema.

### Slice 2 - Calibrate Document-Aware Ranking on Current Coverage

1. Tune the weighting between:
   - chunk semantic similarity
   - document centroid similarity
   - metadata alignment
   - keyword overlap
   - source-hint boost
   - semantic breadth
2. Keep explicit Act names as boosts only, never as hard filters.
3. Select supporting sources only when they add distinct legal or procedural value.
4. Validate against questions the current corpus should realistically answer today.

### Slice 3 - Tighten Safety, Clarification, and High-Risk Escalation

1. Re-evaluate answer-mode thresholds using benchmark prompts across:
   - direct plain-language answers
   - guidance-vs-law prompts
   - broad ambiguous questions
   - wrong-source hints
   - unsupported topics
   - urgent risk-trigger prompts
2. Treat `Clarification` and `Insufficient` as first-class safe responses.
3. Ensure high-risk triggers can still produce escalation-oriented responses even when the corpus is incomplete.
4. Keep persistence limited to grounded `Direct` and `Cautious` answers.

### Slice 4 - Preserve the Contract While Surfacing Source Role and Safety State

1. Keep `RagAnswerResult` append-only for consumers:
   - `answerMode`
   - `confidenceBand`
   - `clarificationQuestion`
   - existing fields unchanged
2. Extend citation payloads additively so the frontend can distinguish:
   - binding law vs official guidance
   - primary vs supporting source
   - generic source title and locator when the document is not an Act section
3. Keep `POST /api/app/qa/ask` as the only endpoint for this feature.
4. Ensure the Ask-page UI clearly communicates caution, clarification, escalation, and source-role state with localized, accessible copy.

### Slice 5 - Record Corpus Expansion as Explicit Follow-On Work

1. Use `docs/research_legislation.md` to define the next source bundles rather than broadening the current implementation slice silently.
2. Route any later bundle additions through:
   - `LegislationManifest`
   - existing document registration
   - current ETL pipeline
   - `IngestionJob`
3. Keep licensing review and source-authority classification attached to each follow-on bundle.

### Slice 6 - Benchmark and Verify Before Task Breakdown

1. Maintain a benchmark matrix aligned to the research decisions:
   - `InCorpusNow`
   - `NeedsCorpusExpansion`
   - `GuidanceOnly`
   - `Escalate`
2. Use that matrix to guide threshold tuning and regression tests.
3. Record deferred corpus bundles separately instead of stretching the retrieval-hardening milestone.

## Implementation Steps

### Step 1 - Restore and Align the Planning Baseline

**Action**:
1. Rebuild `plan.md` from the refined feature spec after the template reset.
2. Fold both research reports into a single set of planning decisions.
3. Generate the missing Phase 1 design artifacts.

**Expected Result**: Planning artifacts reflect the real feature state, current codebase, and legislation research rather than a generic scaffold.

---

### Step 2 - Verify the In-Memory Retrieval Inputs and Derived Source Labels

**Action**:
1. Review `RagAppService.InitialiseAsync()` and `RagIndexStore` to confirm startup load remains atomic and DB-free after initialization.
2. Revisit `RagDocumentProfileBuilder` to ensure document profiles carry stable metadata phrases, aliases, centroid vectors, and enough signals to derive source-family labels.
3. Revisit `RagQueryFocusBuilder` to confirm focus terms narrow broad queries without deleting the user's actual meaning.

**Expected Result**: Retrieval starts from a clean, deterministic in-memory representation of documents, chunks, and source-role hints.

---

### Step 3 - Refine Source Hints, Ranking, and Law-vs-Guidance Selection

**Action**:
1. Recalibrate `RagSourceHintExtractor` boost weights for Act titles, short names, Act numbers, source labels, and categories.
2. Recalibrate `RagRetrievalPlanner` weighting across semantic, metadata, keyword, and breadth signals.
3. Ensure supporting sources are included only when they add distinct legal or procedural value and are labeled correctly as law or guidance.

**Expected Result**: Plain-language, wrong-hint, and guidance-vs-law prompts converge on the correct governing source or source set.

---

### Step 4 - Recalibrate Confidence, Clarification, and Risk Triggers

**Action**:
1. Revisit `RagConfidenceEvaluator` thresholds for `Direct`, `Cautious`, `Clarification`, and `Insufficient`.
2. Keep ambiguity detection strict for short, broad, or closely competing source families.
3. Add risk-trigger handling for urgent situations so the system can escalate safely even when the corpus is incomplete.
4. Preserve the rule that only grounded `Direct` and `Cautious` answers are persisted.

**Expected Result**: The system becomes more conservative when the legal signal is weak or the stakes are high, without suppressing confident grounded answers.

---

### Step 5 - Harden Prompt and Response Behavior

**Action**:
1. Reconfirm `RagPromptBuilder` instructions per mode.
2. Keep bounded temperature policy:
   - `Direct`: `0.2`
   - `Cautious`: `0.1`
   - `Clarification`: `0.0`
   - `Insufficient`: deterministic limitation or escalation text
3. Ensure clarification uses one focused follow-up question and insufficiency never invents uncited legal advice.

**Expected Result**: The answer posture tracks retrieval certainty, source role, and risk level while staying legally conservative.

---

### Step 6 - Keep Service Orchestration and Contract Coherent

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
3. Keep `IRagAppService`, `QaController`, and the ask contract aligned with the refined behavior and additive citation labeling.

**Expected Result**: The backend flow stays simple, deterministic, and safe from retrieval to response.

---

### Step 7 - Verify Ask-Page Representation

**Action**:
1. Confirm `RagAnswerResult` and `contracts/qa-ask.md` match the intended backend behavior.
2. Verify the Ask-page consumer propagates answer mode, confidence, clarification, escalation, and source-label state through service types, provider state, and message rendering.
3. Confirm all four locale files provide clear caution, clarification, escalation, and source-role copy with semantic status presentation.

**Expected Result**: Users can tell when the system is answering directly, being cautious, asking for more detail, escalating for safety, or citing supporting guidance rather than binding law.

---

### Step 8 - Expand Regression and Coverage Benchmarks

**Action**:
1. Expand `RagRetrievalPlannerTests.cs` and `RagConfidenceEvaluatorTests.cs` with benchmark-style cases for:
   - plain-language housing questions
   - labour dismissal phrasing variants
   - wrong-source hints
   - law-vs-guidance distinction
   - ambiguous short prompts
   - unsupported topics
   - urgent escalation triggers
2. Expand `RagPromptBuilderTests.cs`, `RagQueryFocusBuilderTests.cs`, and `RagAppServiceTests.cs` for source-role labeling, non-persistence rules, and escalation behavior.
3. Keep benchmark scenarios split by current corpus coverage vs future bundle dependence.

**Expected Result**: Retrieval, source labeling, and safety behavior are locked down by deterministic tests and explicit coverage states.

---

### Step 9 - Record Follow-On Corpus Bundles Without Stretching the Current Slice

**Action**:
1. Turn the legislation research into an explicit source-bundle backlog:
   - `PIE`
   - `CCMA` rules and forms
   - `Domestic Violence Act` and forms
   - `Small Claims` bundle
   - `PAJA` and `PAIA` bundle
   - `Criminal Procedure Act`
2. Keep each bundle tied to:
   - authoritative public source
   - licensing posture
   - ETL path
   - benchmark scenarios unlocked by that bundle

**Expected Result**: Future corpus work is staged and testable without muddying the current retrieval-hardening milestone.

---

### Step 10 - Run the Final Verification Loop

**Action**:
1. Run backend tests.
2. Validate the Ask page with current in-corpus and escalation scenarios.
3. Confirm benchmark cases are marked as either:
   - passing on current corpus
   - intentionally deferred to corpus expansion
4. Record any deferred source bundles instead of stretching the current milestone.

**Expected Result**: The feature is ready for refreshed task breakdown with a clear benchmark and coverage trail.

## Dependencies & Order

```text
Step 1  (rebuild planning docs)             -> current feature artifacts + both research reports
Step 2  (verify retrieval inputs)           -> Step 1
Step 3  (ranking and source labeling)       -> Step 2
Step 4  (confidence and escalation)         -> Step 3
Step 5  (prompt behavior)                   -> Step 4
Step 6  (service orchestration + contract)  -> Steps 3, 4, 5
Step 7  (Ask UI verification)               -> Step 6
Step 8  (tests + benchmark expansion)       -> Steps 3, 4, 5, 6, 7
Step 9  (corpus bundle backlog)             -> Step 1, informed by Step 8
Step 10 (final verification loop)           -> Steps 8 and 9
```

## Critical Path

```text
Retrieval inputs -> ranking and source labeling -> confidence and escalation
-> prompt behavior -> service contract verification -> Ask UI verification
-> tests and benchmark matrix -> final smoke check
```

## Failure Handling

| Failure | Diagnosis | Resolution |
|---------|-----------|------------|
| Plain-language questions still require Act names | Metadata alignment or ranking weights still underweight semantic meaning | Widen candidate pool, rebalance metadata and semantic weights, and retest against in-corpus benchmark prompts |
| Wrong Act name dominates the result | Hint boosts are too strong | Keep hint signals additive only and lower boost caps until factual matches win |
| Guidance sources outrank binding law | Source-role labeling is missing or ranking weights are too flat | Derive authority type explicitly and boost binding-law sources above guidance where both exist |
| Ambiguous prompts still receive `Direct` answers | Confidence thresholds or ambiguity heuristics are too permissive | Raise direct-answer thresholds and route borderline cases to `Cautious` or `Clarification` |
| High-risk prompts get generic insufficiency with no escalation | Risk-trigger handling is incomplete | Add deterministic escalation messaging and validate against the escalation benchmark set |
| Benchmarks fail because the needed source is missing entirely | The scenario is a corpus gap, not a ranking defect | Mark the case `NeedsCorpusExpansion`, link it to the relevant follow-on bundle, and do not treat it as a retrieval regression |
| Ask page hides source role or response posture | DTO fields are not fully propagated or localized | Verify `qa.service.ts`, provider state, `ChatMessage.tsx`, and locale strings together |

## Deliverables

| Deliverable | Location |
|-------------|----------|
| Refined research decision log | `specs/feat/021-intent-aware-rag/research.md` |
| Refined data model and state notes | `specs/feat/021-intent-aware-rag/data-model.md` |
| Ask API contract | `specs/feat/021-intent-aware-rag/contracts/qa-ask.md` |
| Quick verification guide and coverage matrix | `specs/feat/021-intent-aware-rag/quickstart.md` |
| Refined implementation plan | `specs/feat/021-intent-aware-rag/plan.md` |
| Follow-on corpus expansion direction | Captured in `research.md`, `plan.md`, and `quickstart.md` |
