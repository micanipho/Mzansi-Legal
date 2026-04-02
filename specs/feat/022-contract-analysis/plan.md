# Implementation Plan: Contract Analysis

**Branch**: `feat/022-contract-analysis` | **Date**: 2026-04-02 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `/specs/feat/022-contract-analysis/spec.md`

## Summary

Add an authenticated contract-analysis workflow on top of the existing legal AI stack instead of creating a second isolated system. The backend already has a persisted `ContractAnalysis` / `ContractFlag` aggregate, PdfPig is already installed, and the multilingual RAG pipeline already knows how to retrieve South African legal sources conservatively. This plan wires those pieces together into four user-facing capabilities: upload and analyze a contract PDF, revisit saved analyses, ask contract-specific follow-up questions, and fail safely when the contract is unreadable, unsupported, or not well-grounded in the current corpus. The repo research is part of the design input: current analysis must stay primary-source-first and citation-backed, while missing high-value authority bundles such as PIE remain explicit follow-on corpus work rather than hidden assumptions.

## Technical Context

**Language/Version**: C# on .NET 9.0 + ABP Zero 10.x; TypeScript 5 with Next.js 16 App Router for the contracts experience  
**Primary Dependencies**: Existing `IEmbeddingAppService`, `ILanguageAppService`, `IHttpClientFactory`, `Ardalis.GuardClauses`, `UglyToad.PdfPig`, `next-intl`, Ant Design, current RAG helpers `RagIndexStore`, `RagSourceHintExtractor`, `RagRetrievalPlanner`, `RagConfidenceEvaluator`, and current OpenAI-compatible chat + embedding configuration  
**Storage**: PostgreSQL 15+ via Npgsql; reuse existing `ContractAnalyses`, `ContractFlags`, `LegalDocuments`, `DocumentChunks`, `ChunkEmbeddings`, and ABP binary object storage; no contract-specific persistence expansion is planned for MVP follow-up Q&A  
**Testing**: xUnit + NSubstitute + Shouldly in `backend.Tests`; frontend validation with lint and type-safety; manual upload, OCR fallback, access-control, and follow-up smoke scenarios  
**Target Platform**: Linux server deployment on Railway plus modern evergreen desktop/mobile browsers  
**Project Type**: Web application with ABP web/API backend and Next.js frontend  
**Performance Goals**: Text-first PDF analysis target <= 90 seconds for normal documents; OCR fallback target <= 120 seconds; saved-analysis detail <= 3 seconds; follow-up question response <= 10 seconds  
**Constraints**: No uncited definitive legal claims; supported contract families limited to employment, lease, credit, and service; unreadable or unsupported contracts must fail safely; uploads remain private to the owning user; contract content must not be logged in plaintext; contract text sent to external AI vendors is subject to POPIA-aware vendor and cross-border handling  
**Scale/Scope**: Pilot-scale authenticated contract uploads and follow-up questions with high document sensitivity, moderate file sizes, and a contract-law surface grounded only in the corpus already seeded in the repo unless explicitly expanded later  
**Legal/Compliance Inputs**: `docs/deep-research-report.md`, `docs/research_legislation.md`, `docs/rag-pipeline.md`, POPIA-oriented governance notes in the constitution, and the repo's primary-source-first citation rules

## Current System Snapshot

- `ContractAnalysis` and `ContractFlag` already exist as persisted domain entities, are registered in `backendDbContext`, and already have an EF migration in the repo.
- `PdfPig` is already installed and the current PDF ingestion path already contains a reliable word-level extraction approach for South African PDFs.
- The multilingual RAG flow already supports language handling, conservative answer modes, citation formatting, and source-role labeling for legislation-backed responses.
- The frontend already has authenticated contracts list/detail routes and a contracts provider, but they are still powered by demo data.
- There is no current contract-analysis API surface, no current upload endpoint, and no current contract-specific follow-up endpoint.

## Coverage Reality Snapshot

### Current seeded authority that supports MVP contract analysis

- Employment: `BCEA`, `LRA`
- Lease basics: `Rental Housing Act`, `Constitution`
- Credit: `NCA`, `CPA`
- Consumer/service fairness baseline: `CPA`, `Constitution`

### Contract-analysis gaps called out by repo research

- Lease termination and eviction procedure: `PIE` is not yet in the seeded corpus
- Contract-law interpretation depth: leading case law and regulations are not yet part of the indexed authority layer
- Sector-specific service contracts: many domain-specific rules fall outside the current general corpus

### Planning consequence

The analyzer must downgrade unsupported or weakly grounded findings into plain-language caution or "needs review" language instead of manufacturing strong legal conclusions. The feature can still score and summarize a contract, but legal red flags only count as definitive when the cited corpus actually supports them.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Layer Gate**: Persisted entities stay in `backend.Core/Domains/ContractAnalysis`, orchestration stays in `backend.Application/Services`, and thin upload routing stays in `backend.Web.Host/Controllers`.
- [x] **Naming Gate**: Public app-service entry points follow `IContractAppService` / `ContractAppService`; helper services such as `ContractAnalysisService` remain focused internal collaborators rather than ad-hoc controller logic.
- [x] **Coding Standards Gate**: Pdf extraction, OCR fallback, classification, legislation retrieval, prompt building, JSON parsing, and follow-up orchestration are planned as small focused classes to respect the repo's guard-clause, method-length, and low-nesting rules.
- [x] **Skill Gate**: `speckit.plan`, `speckit.tasks`, and `follow-git-workflow` govern the implementation workflow; `add-styling` only applies if the contracts routes need dedicated presentation work beyond the current shell.
- [x] **Multilingual Gate**: Analysis summaries, flag descriptions, errors, and follow-up answers will honor English, isiZulu, Sesotho, and Afrikaans via locale-aware output, while Act names and section citations remain in English.
- [x] **Authority Gate**: The feature remains primary-source-first, uses the existing legislation corpus for binding authority, and treats any guidance-style material as supporting context only.
- [x] **Citation Gate**: Analysis findings and follow-up answers that make legal claims must include legislation citations; if controlling authority is missing, the response must downgrade confidence instead of guessing.
- [x] **Safety Gate**: Unsupported contract types, unreadable files, and weakly grounded follow-up questions explicitly return safe limitation or review-needed responses.
- [x] **Accessibility Gate**: The upload, history, detail, and follow-up flows remain keyboard accessible, localized, and mobile-friendly within the existing Next.js contracts routes.
- [x] **Data Governance Gate**: Contract uploads are private-by-default, scoped to the authenticated user, excluded from plaintext logging, and treated as high-sensitivity content under POPIA-aware vendor processing assumptions.
- [x] **Corpus Governance Gate**: Contract legal support still depends on official-source-first legislation bundles already seeded in the repo; missing bundles such as PIE remain tracked as explicit follow-on ingestion work.
- [x] **ETL/Ingestion Gate**: This feature does not create a second legislation-ingestion path. Any corpus expansion unlocked by contract-analysis gaps must still flow through `LegislationManifest`, the ETL pipeline, and `IngestionJob`.

## Project Structure

### Documentation (this feature)

```text
specs/feat/022-contract-analysis/
|-- spec.md
|-- plan.md
|-- research.md
|-- data-model.md
|-- quickstart.md
|-- contracts/
|   |-- contract-analyse.md
|   |-- contract-get.md
|   |-- contract-list.md
|   `-- contract-ask.md
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
|   |       |-- ContractService/
|   |       |   |-- IContractAppService.cs                 [NEW]
|   |       |   |-- ContractAppService.cs                  [NEW]
|   |       |   |-- ContractAnalysisService.cs             [NEW]
|   |       |   |-- ContractPromptBuilder.cs               [NEW]
|   |       |   |-- ContractLegislationContextBuilder.cs   [NEW]
|   |       |   |-- ContractFollowUpService.cs             [NEW/OPTIONAL SPLIT]
|   |       |   `-- DTO/
|   |       |       |-- AnalyseContractRequest.cs          [NEW]
|   |       |       |-- ContractAnalysisDto.cs             [NEW]
|   |       |       |-- ContractAnalysisListDto.cs         [NEW]
|   |       |       |-- ContractAnalysisListItemDto.cs     [NEW]
|   |       |       |-- ContractFlagDto.cs                 [NEW]
|   |       |       |-- AskContractQuestionRequest.cs      [NEW]
|   |       |       `-- ContractFollowUpAnswerDto.cs       [NEW]
|   |       |-- PdfIngestionService/
|   |       |   `-- PdfIngestionAppService.cs              [REFINE/SHARE EXTRACTION]
|   |       `-- RagService/
|   |           |-- RagIndexStore.cs                       [REUSE]
|   |           |-- RagSourceHintExtractor.cs              [REUSE]
|   |           |-- RagRetrievalPlanner.cs                 [REUSE]
|   |           |-- RagConfidenceEvaluator.cs              [REUSE]
|   |           `-- RagPromptBuilder.cs                    [REFERENCE]
|   |-- backend.Web.Host/
|   |   `-- Controllers/
|   |       `-- ContractController.cs                      [NEW]
|   |-- backend.Core/
|   |   `-- Domains/
|   |       |-- ContractAnalysis/
|   |       |   |-- ContractAnalysis.cs                    [VERIFY]
|   |       |   |-- ContractFlag.cs                        [VERIFY]
|   |       |   |-- ContractType.cs                        [VERIFY]
|   |       |   `-- FlagSeverity.cs                        [VERIFY]
|   |       |-- LegalDocuments/
|   |       |   |-- LegalDocument.cs                       [REUSE]
|   |       |   `-- DocumentChunk.cs                       [REUSE]
|   |       `-- QA/
|   |           |-- Question.cs                            [REFERENCE ONLY]
|   |           `-- Answer.cs                              [REFERENCE ONLY]
|   `-- backend.EntityFrameworkCore/
|       `-- EntityFrameworkCore/
|           |-- backendDbContext.cs                        [VERIFY]
|           `-- Seed/
|               `-- Host/
|                   `-- LegislationManifest.cs             [REFERENCE FOR COVERAGE]
`-- test/
    `-- backend.Tests/
        |-- ContractServiceTests/
        |   |-- ContractAppServiceTests.cs                 [NEW]
        |   |-- ContractAnalysisServiceTests.cs            [NEW]
        |   |-- ContractPromptBuilderTests.cs              [NEW]
        |   `-- ContractFollowUpServiceTests.cs            [NEW]
        `-- RagServiceTests/
            `-- existing retrieval tests                   [REFERENCE]

frontend/
`-- src/
    |-- app/
    |   `-- [locale]/
    |       `-- contracts/
    |           |-- page.tsx                               [REFINE]
    |           |-- [id]/
    |           |   |-- page.tsx                           [REFINE]
    |           |   `-- ContractDetailGuard.tsx            [VERIFY]
    |           `-- components/                            [NEW/OPTIONAL]
    |-- providers/
    |   `-- contracts-provider/
    |       |-- actions.tsx                                [REFINE]
    |       |-- context.tsx                                [REFINE]
    |       |-- reducer.tsx                                [REFINE]
    |       `-- index.tsx                                  [REFINE]
    |-- services/
    |   `-- contract.service.ts                            [NEW]
    |-- components/
    |   `-- contracts/
    |       `-- contractData.ts                            [REMOVE DEMO / REPLACE TYPES]
    `-- messages/
        |-- en.json                                        [REFINE]
        |-- zu.json                                        [REFINE]
        |-- st.json                                        [REFINE]
        `-- af.json                                        [REFINE]
```

**Structure Decision**: Keep the user journeys inside the existing contracts pages and provider, replace demo contract data with a real authenticated contract API, and place all contract-analysis orchestration in a dedicated backend application-service module that reuses the current PDF and RAG infrastructure instead of duplicating it.

## Complexity Tracking

No constitution violations require justification.

## Decision-Driven Implementation Map

| Slice | Goal | Exit Signal |
|-------|------|-------------|
| 1. Upload and extraction | Accept authenticated PDF uploads, extract readable text, and fail safely when unreadable | Text-first PDFs extract directly and low-text PDFs trigger OCR fallback before analysis fails |
| 2. Classification and legislation context | Identify supported contract family and gather the most relevant current-law context | Employment, lease, credit, and service contracts map to grounded source bundles or an explicit unsupported result |
| 3. Structured analysis generation | Produce validated score, summary, and flags with citations | Analysis JSON is parsed, normalized, and only grounded legal claims keep definitive red-flag posture |
| 4. Persistence and history | Save and retrieve user-owned analyses | Users can list only their own analyses and open a saved result without re-uploading |
| 5. Contract-specific follow-up | Answer clause-level questions using contract text plus legislation | Follow-up responses stay contract-aware, cited, multilingual, and safe when support is weak |
| 6. Frontend replacement and verification | Replace demo data and confirm upload/detail/follow-up behavior | Contracts pages use real APIs, locale strings exist, and core smoke tests pass |

## Implementation Strategy

### Slice 1 - Build a shared contract upload and extraction path

1. Add a thin authenticated `ContractController` route surface for multipart upload and simple detail/list/follow-up endpoints.
2. Keep business logic out of the controller by forwarding upload streams into `IContractAppService`.
3. Reuse the existing PdfPig extraction approach from `PdfIngestionAppService`, but extract the text-reading behavior into a contract-safe helper instead of reusing legislation chunking directly.
4. If direct extraction yields fewer than 100 meaningful characters, invoke OCR fallback through the OpenAI-compatible vision path.
5. Return a clear user-facing failure when the file is empty, protected, unreadable, or still too thin after OCR.

### Slice 2 - Classify supported contracts and retrieve current authority

1. Detect contract type from the first 500 characters using a constrained prompt that can only return `Employment`, `Lease`, `Credit`, or `Service`.
2. Refuse to continue as a normal analysis when classification confidence is weak or the document is outside the supported families.
3. Build a legislation retrieval query from contract type, extracted key terms, and high-signal clauses.
4. Reuse `RagIndexStore`, `RagSourceHintExtractor`, and `RagRetrievalPlanner` to gather source chunks rather than inventing a separate search subsystem.
5. Keep current corpus gaps explicit:
   - employment relies mainly on `BCEA` and `LRA`
   - credit relies mainly on `NCA` and `CPA`
   - service relies mainly on `CPA`
   - lease uses `RHA` and `Constitution`, but some eviction/procedure issues must downgrade because `PIE` is not seeded yet

### Slice 3 - Generate and validate structured contract analysis

1. Build a contract-analysis system prompt that frames the model as a South African contract analyst and requires JSON output only.
2. Pass the full contract text plus the retrieved legislation context into the analysis prompt.
3. Require a structured response shape:
   - `healthScore`
   - `summary`
   - `flags[]` with severity, title, description, clause text, and legislation citation
4. Parse the JSON deterministically and validate score range, required fields, severity values, and citation presence for legal claims.
5. Downgrade or remove findings that overstate legal certainty without grounded legislation support.

### Slice 4 - Persist analysis and expose private history endpoints

1. Persist successful analyses into the existing `ContractAnalysis` aggregate and persist each finding into `ContractFlag`.
2. Reuse existing ownership fields so every analysis is scoped to the authenticated user.
3. Expose:
   - `POST /api/app/contract/analyse`
   - `GET /api/app/contract/{id}`
   - `GET /api/app/contract/my`
4. Keep list/detail payloads focused on user-visible analysis data, not raw extracted text.
5. Treat stable filename/display metadata as an implementation check: use existing stored-file metadata where possible, and only add a narrow persistence field if the current binary-object path cannot surface a usable display name.

### Slice 5 - Reuse RAG safety behavior for contract follow-up questions

1. Add `POST /api/app/contract/{id}/ask`.
2. Load the user's contract analysis and combine:
   - the follow-up question
   - relevant contract clauses or extracted excerpts
   - the already-retrieved or freshly retrieved legislation context
3. Reuse the existing conservative answer-mode ideas from the RAG stack so contract follow-up answers can still return grounded, cautious, or insufficient outcomes.
4. Keep follow-up responses multilingual using the request locale or current app language, while leaving legislation titles and section numbers in English.
5. For MVP, keep follow-up question persistence out of scope unless implementation proves a saved thread is necessary; the acceptance criteria only require correct contract-aware answers, not stored chat history.

### Slice 6 - Replace demo contracts UI and verify end-to-end behavior

1. Replace demo data in the contracts provider with real API calls for list and detail.
2. Add upload state, error state, and empty-state handling to the contracts routes.
3. Replace the current "ask about contract" generic Ask-page bridge with a contract-aware follow-up flow tied to the selected analysis.
4. Add locale strings for upload, analysis status, unreadable/unsupported failures, follow-up limitations, and citation labels in `en`, `zu`, `st`, and `af`.
5. Verify keyboard accessibility, status messaging, and mobile presentation on the contracts pages.

## Implementation Steps

### Step 1 - Rebuild the planning set around the real repo state

**Action**:
1. Use the existing contract aggregate, demo contracts UI, RAG flow, and PDF ingestion code as the baseline.
2. Fold the repo research docs into explicit contract-analysis decisions.
3. Generate the missing design artifacts for data, contracts, quickstart, and implementation slices.

**Expected Result**: The feature plan reflects the current codebase and current legal-corpus reality instead of assuming a blank-slate contracts system.

---

### Step 2 - Create the contract upload and extraction seam

**Action**:
1. Add the thin controller + app-service contract for authenticated upload.
2. Refactor or wrap existing PdfPig extraction into a shared helper.
3. Add the OCR fallback trigger for thin-text PDFs.

**Expected Result**: A single upload path can reliably produce readable contract text or a safe failure.

---

### Step 3 - Add classification and legislation-context retrieval

**Action**:
1. Build constrained type detection.
2. Build a contract-term query planner against the existing RAG corpus.
3. Encode coverage downgrades for unsupported or thinly supported issues.

**Expected Result**: Contract analysis runs against the right law bundle and does not overstate certainty when the corpus is incomplete.

---

### Step 4 - Generate, validate, and persist contract analysis output

**Action**:
1. Implement the structured contract-analysis prompt and JSON parser.
2. Normalize score, summary, and flag output.
3. Persist `ContractAnalysis` and `ContractFlag` records for successful analyses.

**Expected Result**: Users receive a stable saved analysis result with grounded flags and plain-language output.

---

### Step 5 - Expose history and detail retrieval

**Action**:
1. Implement owner-scoped list and detail queries.
2. Shape DTOs for the contracts pages.
3. Keep raw extracted text off list/detail responses unless explicitly needed.

**Expected Result**: Users can revisit their own analyses without seeing other users' data.

---

### Step 6 - Add contract-specific follow-up Q&A

**Action**:
1. Implement the contract-aware follow-up endpoint.
2. Reuse conservative RAG behavior for answer modes, multilingual output, and citations.
3. Return explicit limitation text when contract or legislation support is too weak.

**Expected Result**: Follow-up questions become a contract-aware legal guidance flow instead of a generic ask route.

---

### Step 7 - Replace demo UI wiring

**Action**:
1. Add a real `contract.service.ts`.
2. Replace demo provider calls with authenticated backend requests.
3. Update contracts list/detail routes and locale content.

**Expected Result**: The contracts UI is backed by the actual API and no longer depends on mock records.

---

### Step 8 - Verify access control, OCR fallback, and corpus-aware safety

**Action**:
1. Add backend tests for extraction fallback, classification, grounded-flag validation, access control, and follow-up limitation handling.
2. Run frontend lint/type validation.
3. Execute manual smoke scenarios across text-first PDFs, scanned PDFs, unsupported files, history access, and contract follow-up questions.

**Expected Result**: The feature is ready for task breakdown with the right safety and coverage expectations documented.

## Dependencies & Order

```text
Step 1  (rebuild planning docs)             -> spec + repo research + current codebase
Step 2  (upload + extraction seam)          -> Step 1
Step 3  (classification + legal retrieval)  -> Step 2
Step 4  (analysis generation + persistence) -> Step 3
Step 5  (history + detail retrieval)        -> Step 4
Step 6  (contract follow-up Q&A)            -> Steps 3, 4, 5
Step 7  (frontend replacement)              -> Steps 4, 5, 6
Step 8  (verification loop)                 -> Steps 2 through 7
```

## Critical Path

```text
Upload and extraction -> classification and law retrieval -> structured analysis generation
-> persistence and history -> contract-specific follow-up -> frontend wiring -> verification
```

## Failure Handling

| Failure | Diagnosis | Resolution |
|---------|-----------|------------|
| PDF extracts almost no text | Image-only or protected file | Trigger OCR fallback; if still thin, return unreadable guidance |
| Contract type cannot be classified | Mixed content or unsupported agreement | Return unsupported-type guidance and stop before speculative analysis |
| Analyzer emits red flags without grounded law | Prompt or parser allowed unsupported legal claims | Reject, downgrade, or relabel findings until citations are grounded |
| Lease analysis overclaims eviction procedure | Current corpus lacks `PIE` | Mark the issue as review-needed and record the corpus gap explicitly |
| User can access another user's analysis | Ownership filter missing on detail/list/follow-up | Scope every query by `AbpSession.UserId` and cover with access-control tests |
| Follow-up answer ignores contract text | Contract context was not loaded or surfaced into the prompt | Require contract excerpts plus legislation context in follow-up orchestration |
| Contracts UI still shows demo data | Provider/service not fully switched over | Remove demo fetches and verify real list/detail flows end-to-end |

## Deliverables

| Deliverable | Location |
|-------------|----------|
| Research decision log | `specs/feat/022-contract-analysis/research.md` |
| Contract-analysis data model | `specs/feat/022-contract-analysis/data-model.md` |
| API contracts | `specs/feat/022-contract-analysis/contracts/` |
| Quick verification guide | `specs/feat/022-contract-analysis/quickstart.md` |
| Filled implementation plan | `specs/feat/022-contract-analysis/plan.md` |
