# Tasks: Contract Analysis

**Input**: Design documents from `/specs/feat/022-contract-analysis/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: Include verification tasks for every changed behavior. This legal-AI feature requires deterministic backend tests for extraction, grounding, access control, and follow-up behavior, plus frontend lint/type validation and quickstart-guided smoke checks.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested independently.

## Format: `[ID] [P?] [Story] Description`

- `[P]` marks a task that can run in parallel with other tasks in the same phase when file ownership does not overlap.
- `[Story]` labels story-specific work back to the matching user story in `spec.md`.
- Every task includes the exact file path to change or validate.

## Phase 1: Setup (Shared Contract Analysis Scaffolding)

**Purpose**: Create the shared contract-analysis service, test, and frontend scaffolding that every story depends on.

[X] T001 Create the contract-analysis application-service scaffolding in `backend/src/backend.Application/Services/ContractService/IContractAppService.cs`, `backend/src/backend.Application/Services/ContractService/ContractAppService.cs`, and `backend/src/backend.Application/Services/ContractService/DTO/`
[X] T002 [P] Create backend contract-analysis test scaffolding in `backend/test/backend.Tests/ContractServiceTests/ContractAppServiceTests.cs`, `backend/test/backend.Tests/ContractServiceTests/ContractAnalysisServiceTests.cs`, `backend/test/backend.Tests/ContractServiceTests/ContractPromptBuilderTests.cs`, and `backend/test/backend.Tests/ContractServiceTests/ContractFollowUpServiceTests.cs`
[X] T003 [P] Create frontend contract API and provider scaffolding in `frontend/src/services/contract.service.ts`, `frontend/src/providers/contracts-provider/context.tsx`, `frontend/src/providers/contracts-provider/actions.tsx`, `frontend/src/providers/contracts-provider/reducer.tsx`, and `frontend/src/providers/contracts-provider/index.tsx`

---

## Phase 2: Foundational (Blocking Shared Upload, Grounding, and Privacy Baseline)

**Purpose**: Establish the shared extraction, grounding, DTO, and route baseline that blocks all user stories.

**Critical**: No user story work should begin until this phase is complete.

[X] T004 [P] Add shared upload, OCR-fallback, unreadable-file, and unsupported-type verification in `backend/test/backend.Tests/ContractServiceTests/ContractAnalysisServiceTests.cs`
[X] T005 [P] Add shared owner-scope, privacy, and list/detail projection verification in `backend/test/backend.Tests/ContractServiceTests/ContractAppServiceTests.cs`
[X] T006 Implement shared PDF text extraction and OCR fallback orchestration in `backend/src/backend.Application/Services/PdfIngestionService/PdfIngestionAppService.cs` and `backend/src/backend.Application/Services/ContractService/ContractAnalysisService.cs`
[X] T007 [P] Implement shared contract DTO contracts in `backend/src/backend.Application/Services/ContractService/IContractAppService.cs`, `backend/src/backend.Application/Services/ContractService/DTO/AnalyseContractRequest.cs`, `backend/src/backend.Application/Services/ContractService/DTO/ContractAnalysisDto.cs`, and `backend/src/backend.Application/Services/ContractService/DTO/ContractFlagDto.cs`
[X] T008 [P] Implement shared legislation-context and grounded-flag normalization helpers in `backend/src/backend.Application/Services/ContractService/ContractLegislationContextBuilder.cs`, `backend/src/backend.Application/Services/ContractService/ContractPromptBuilder.cs`, and `backend/src/backend.Application/Services/ContractService/ContractAnalysisService.cs`
[X] T009 Implement the shared authenticated contract route surface in `backend/src/backend.Web.Host/Controllers/ContractController.cs`

**Checkpoint**: Shared extraction, grounding, privacy, and route scaffolding are ready for story work.

---

## Phase 3: User Story 1 - Upload a Contract and Get an Analysis (Priority: P1) MVP

**Goal**: Let a signed-in user upload a supported PDF and receive a saved grounded contract analysis with score, summary, and flags.

**Independent Test**: A signed-in user can upload readable and scanned supported contracts, receive a saved analysis with citations for grounded legal claims, and get a safe failure for unreadable or unsupported files.

### Verification for User Story 1

- [X] T010 [P] [US1] Add readable-PDF, scanned-PDF, and unreadable-PDF analysis cases in `backend/test/backend.Tests/ContractServiceTests/ContractAnalysisServiceTests.cs` and `backend/test/backend.Tests/ContractServiceTests/ContractAppServiceTests.cs`
- [X] T011 [P] [US1] Add analysis JSON-shape and citation-grounding assertions in `backend/test/backend.Tests/ContractServiceTests/ContractPromptBuilderTests.cs` and `specs/feat/022-contract-analysis/quickstart.md`

### Implementation for User Story 1

- [X] T012 [P] [US1] Implement uploaded-file storage and analysis orchestration in `backend/src/backend.Application/Services/ContractService/ContractAppService.cs`
- [X] T013 [P] [US1] Implement structured contract-analysis prompt building and JSON parsing in `backend/src/backend.Application/Services/ContractService/ContractPromptBuilder.cs` and `backend/src/backend.Application/Services/ContractService/ContractAnalysisService.cs`
- [X] T014 [US1] Persist ordered `ContractAnalysis` and `ContractFlag` records from analysis results in `backend/src/backend.Application/Services/ContractService/ContractAppService.cs`
- [X] T015 [P] [US1] Implement the `POST /api/app/contract/analyse` request and response flow in `backend/src/backend.Web.Host/Controllers/ContractController.cs` and `backend/src/backend.Application/Services/ContractService/DTO/AnalyseContractRequest.cs`
- [X] T016 [P] [US1] Implement frontend upload and analyse client calls in `frontend/src/services/contract.service.ts` and `frontend/src/providers/contracts-provider/index.tsx`
- [X] T017 [P] [US1] Replace the demo upload flow with real pending, success, and error handling in `frontend/src/app/[locale]/contracts/page.tsx` and `frontend/src/components/contracts/contractData.ts`
- [X] T018 [P] [US1] Add localized upload, OCR-failure, unsupported-contract, and analysis-result copy in `frontend/src/messages/en.json`, `frontend/src/messages/zu.json`, `frontend/src/messages/st.json`, and `frontend/src/messages/af.json`

**Checkpoint**: User Story 1 should let users upload a supported contract and receive a saved analysis without depending on history or follow-up work.

---

## Phase 4: User Story 2 - Review My Saved Analyses (Priority: P2)

**Goal**: Let the uploading user list their saved analyses and open a specific result privately.

**Independent Test**: After at least one analysis exists, the same user can load `/contract/my`, open a specific analysis, and another user cannot access it.

### Verification for User Story 2

- [X] T019 [P] [US2] Add saved-history, detail-loading, and access-control cases in `backend/test/backend.Tests/ContractServiceTests/ContractAppServiceTests.cs` and `specs/feat/022-contract-analysis/quickstart.md`
- [X] T020 [P] [US2] Add list/detail DTO projection and count-aggregation assertions in `backend/test/backend.Tests/ContractServiceTests/ContractAppServiceTests.cs`

### Implementation for User Story 2

- [X] T021 [P] [US2] Implement owner-scoped list and detail DTO mapping in `backend/src/backend.Application/Services/ContractService/ContractAppService.cs`, `backend/src/backend.Application/Services/ContractService/DTO/ContractAnalysisListDto.cs`, and `backend/src/backend.Application/Services/ContractService/DTO/ContractAnalysisListItemDto.cs`
- [X] T022 [P] [US2] Implement `GET /api/app/contract/{id}` and `GET /api/app/contract/my` in `backend/src/backend.Web.Host/Controllers/ContractController.cs`
- [X] T023 [P] [US2] Replace demo `fetchAll` and `fetchById` calls with real API integration in `frontend/src/services/contract.service.ts` and `frontend/src/providers/contracts-provider/index.tsx`
- [X] T024 [P] [US2] Render saved analysis history and detail views from API data in `frontend/src/app/[locale]/contracts/page.tsx`, `frontend/src/app/[locale]/contracts/[id]/page.tsx`, and `frontend/src/app/[locale]/contracts/[id]/ContractDetailGuard.tsx`
- [X] T025 [P] [US2] Add localized history, empty-state, and access-denied copy in `frontend/src/messages/en.json`, `frontend/src/messages/zu.json`, `frontend/src/messages/st.json`, and `frontend/src/messages/af.json`

**Checkpoint**: User Story 2 should let users revisit only their own saved analyses without requiring follow-up Q&A.

---

## Phase 5: User Story 3 - Ask Follow-Up Questions About a Contract (Priority: P3)

**Goal**: Let the user ask contract-specific follow-up questions grounded in the saved contract text and current legislation corpus.

**Independent Test**: A signed-in user can open a saved analysis, ask a clause-level question, receive a grounded contract-aware answer with citations when support exists, and receive an explicit limitation when support is weak.

### Verification for User Story 3

- [ ] T026 [P] [US3] Add contract-follow-up grounding and insufficient-support cases in `backend/test/backend.Tests/ContractServiceTests/ContractFollowUpServiceTests.cs` and `specs/feat/022-contract-analysis/quickstart.md`
- [ ] T027 [P] [US3] Add contract-aware citation, answer-mode, and language-directive assertions in `backend/test/backend.Tests/ContractServiceTests/ContractPromptBuilderTests.cs` and `backend/test/backend.Tests/ContractServiceTests/ContractFollowUpServiceTests.cs`

### Implementation for User Story 3

- [ ] T028 [P] [US3] Implement follow-up request and response DTOs in `backend/src/backend.Application/Services/ContractService/DTO/AskContractQuestionRequest.cs` and `backend/src/backend.Application/Services/ContractService/DTO/ContractFollowUpAnswerDto.cs`
- [ ] T029 [P] [US3] Implement contract-aware follow-up context assembly and conservative answer modes in `backend/src/backend.Application/Services/ContractService/ContractFollowUpService.cs` and `backend/src/backend.Application/Services/ContractService/ContractLegislationContextBuilder.cs`
- [ ] T030 [US3] Implement owner-scoped `POST /api/app/contract/{id}/ask` orchestration in `backend/src/backend.Application/Services/ContractService/ContractAppService.cs` and `backend/src/backend.Web.Host/Controllers/ContractController.cs`
- [ ] T031 [P] [US3] Add frontend follow-up request handling in `frontend/src/services/contract.service.ts` and `frontend/src/providers/contracts-provider/context.tsx`
- [ ] T032 [P] [US3] Replace the generic ask-about-contract link with contract-specific follow-up UI in `frontend/src/app/[locale]/contracts/[id]/page.tsx`
- [ ] T033 [P] [US3] Add localized follow-up, insufficient-support, and citation-role copy in `frontend/src/messages/en.json`, `frontend/src/messages/zu.json`, `frontend/src/messages/st.json`, and `frontend/src/messages/af.json`

**Checkpoint**: User Story 3 should support contract-specific follow-up questions without requiring new chat persistence.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validate the full contract-analysis workflow, confirm privacy/coverage notes, and lock the final quality bar.

- [ ] T034 [P] Run the backend regression suite for contract analysis in `backend/test/backend.Tests/ContractServiceTests/ContractAppServiceTests.cs`, `backend/test/backend.Tests/ContractServiceTests/ContractAnalysisServiceTests.cs`, `backend/test/backend.Tests/ContractServiceTests/ContractPromptBuilderTests.cs`, and `backend/test/backend.Tests/ContractServiceTests/ContractFollowUpServiceTests.cs`
- [ ] T035 [P] Run frontend lint and type-safety validation for `frontend/src/services/contract.service.ts`, `frontend/src/providers/contracts-provider/context.tsx`, `frontend/src/providers/contracts-provider/actions.tsx`, `frontend/src/providers/contracts-provider/reducer.tsx`, `frontend/src/providers/contracts-provider/index.tsx`, `frontend/src/app/[locale]/contracts/page.tsx`, and `frontend/src/app/[locale]/contracts/[id]/page.tsx`
- [ ] T036 Validate upload, OCR, history, access-control, and follow-up smoke scenarios in `specs/feat/022-contract-analysis/quickstart.md`
- [ ] T037 [P] Reconcile coverage-gap, privacy, and file-metadata implementation notes in `specs/feat/022-contract-analysis/plan.md`, `specs/feat/022-contract-analysis/research.md`, and `specs/feat/022-contract-analysis/data-model.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1: Setup** -> no dependencies and can start immediately.
- **Phase 2: Foundational** -> depends on Phase 1 and blocks all user-story work.
- **Phase 3: US1** -> depends on Phase 2 and delivers the MVP.
- **Phase 4: US2** -> depends on US1 because saved-history behavior assumes successful analyses already exist.
- **Phase 5: US3** -> depends on US1 and US2 because follow-up questions require a saved analysis and the contract detail experience.
- **Phase 6: Polish** -> depends on all implemented user stories.

### User Story Dependencies

- **US1 (P1)** -> first deliverable and recommended MVP scope.
- **US2 (P2)** -> depends on US1 because list/detail behavior requires persisted analyses.
- **US3 (P3)** -> depends on US1 for saved contract context and on US2 for the detail retrieval surface.

### Within Each User Story

- Verification tasks for the story should be completed first and used to prove the intended behavior before implementation is finalized.
- Backend orchestration and DTO work should land before frontend integration for the same story.
- Each story checkpoint should pass before moving to the next priority slice.

---

## Parallel Opportunities

- T002 and T003 can run in parallel because backend test scaffolding and frontend scaffolding touch different files.
- T004 and T005 can run in parallel during the foundational phase.
- T007 and T008 can run in parallel once T006 establishes the shared extraction seam.
- T010 and T011 can run in parallel for US1.
- T012 and T013 can run in parallel for US1 before T014 integrates persistence.
- T016-T018 can run in parallel for US1 after the backend analyse contract is stable.
- T019 and T020 can run in parallel for US2.
- T021-T025 have strong parallel potential because backend DTO mapping, controller work, frontend API integration, and localization mostly touch separate files.
- T026 and T027 can run in parallel for US3.
- T028 and T029 can run in parallel for US3 before T030 integrates the endpoint.
- T031-T033 can run in parallel for US3 after the backend follow-up contract is stable.
- T034 and T035 can run in parallel during final validation.

---

## Parallel Example: User Story 1

```bash
# Verification tasks that can run in parallel for US1
Task: "Add readable-PDF, scanned-PDF, and unreadable-PDF analysis cases in backend/test/backend.Tests/ContractServiceTests/ContractAnalysisServiceTests.cs and backend/test/backend.Tests/ContractServiceTests/ContractAppServiceTests.cs"
Task: "Add analysis JSON-shape and citation-grounding assertions in backend/test/backend.Tests/ContractServiceTests/ContractPromptBuilderTests.cs and specs/feat/022-contract-analysis/quickstart.md"

# Frontend tasks that can run in parallel for US1 after the backend analyse flow is stable
Task: "Implement frontend upload and analyse client calls in frontend/src/services/contract.service.ts and frontend/src/providers/contracts-provider/index.tsx"
Task: "Replace the demo upload flow with real pending, success, and error handling in frontend/src/app/[locale]/contracts/page.tsx and frontend/src/components/contracts/contractData.ts"
Task: "Add localized upload, OCR-failure, unsupported-contract, and analysis-result copy in frontend/src/messages/en.json, frontend/src/messages/zu.json, frontend/src/messages/st.json, and frontend/src/messages/af.json"
```

---

## Parallel Example: User Story 3

```bash
# Backend tasks that can run in parallel for US3
Task: "Implement follow-up request and response DTOs in backend/src/backend.Application/Services/ContractService/DTO/AskContractQuestionRequest.cs and backend/src/backend.Application/Services/ContractService/DTO/ContractFollowUpAnswerDto.cs"
Task: "Implement contract-aware follow-up context assembly and conservative answer modes in backend/src/backend.Application/Services/ContractService/ContractFollowUpService.cs and backend/src/backend.Application/Services/ContractService/ContractLegislationContextBuilder.cs"

# Frontend tasks that can run in parallel for US3 after the endpoint contract is stable
Task: "Add frontend follow-up request handling in frontend/src/services/contract.service.ts and frontend/src/providers/contracts-provider/context.tsx"
Task: "Replace the generic ask-about-contract link with contract-specific follow-up UI in frontend/src/app/[locale]/contracts/[id]/page.tsx"
Task: "Add localized follow-up, insufficient-support, and citation-role copy in frontend/src/messages/en.json, frontend/src/messages/zu.json, frontend/src/messages/st.json, and frontend/src/messages/af.json"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3.
3. Run the US1 extraction, grounding, and upload-flow validation tasks.
4. Validate the US1 quickstart scenarios before expanding scope.
5. Stop and review the MVP before moving to history and follow-up work.

### Incremental Delivery

1. Deliver US1 to prove supported PDFs can be analyzed safely with OCR fallback and grounded flags.
2. Add US2 to make completed analyses privately retrievable and reviewable over time.
3. Add US3 to turn saved analyses into a contract-aware follow-up workflow.

### Recommended Execution Order

1. T001-T009
2. T010-T018
3. T019-T025
4. T026-T033
5. T034-T037

---

## Notes

- All tasks follow the required checklist format with task ID, optional parallel marker, optional story label, and exact file paths.
- The task list is based on the generated feat/022 design set in `specs/feat/022-contract-analysis/`.
- No explicit migration task is included because the current plan keeps MVP on the existing `ContractAnalysis` aggregate; file-metadata persistence remains a narrow implementation check rather than a planned schema expansion.
- Corpus expansion items such as `PIE` remain documented follow-on work, not implementation tasks inside this feature.
