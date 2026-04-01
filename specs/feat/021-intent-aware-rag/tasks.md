# Tasks: Intent-Aware Legal Retrieval for RAG Answers

**Input**: Design documents from `/specs/feat/021-intent-aware-rag/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/qa-ask.md`, `quickstart.md`

**Tests**: Backend tests are required for this feature because the spec and plan both call for deterministic retrieval, confidence, and fallback-behavior coverage.

**Organization**: Tasks are grouped by user story so each story can be implemented and verified as an independently testable increment.

## Format: `[ID] [P?] [Story] Description`

- `[P]` marks a task that can run in parallel with other tasks in the same phase when file ownership does not overlap.
- `[Story]` labels trace work back to the corresponding user story in `spec.md`.
- Every task includes the exact file path to change.

## Phase 1: Setup (Shared Contract Scaffolding)

**Purpose**: Establish the shared response contract and enums that every later slice depends on.

- [ ] T001 Create `RagAnswerMode` enum in `backend/src/backend.Application/Services/RagService/DTO/RagAnswerMode.cs`
- [ ] T002 [P] Create `RagConfidenceBand` enum in `backend/src/backend.Application/Services/RagService/DTO/RagConfidenceBand.cs`
- [ ] T003 Extend `RagAnswerResult` with answer-mode metadata in `backend/src/backend.Application/Services/RagService/DTO/RagAnswerResult.cs`
- [ ] T004 Update response-contract documentation in `backend/src/backend.Web.Host/Controllers/QaController.cs` and `specs/feat/021-intent-aware-rag/contracts/qa-ask.md`

---

## Phase 2: Foundational (Blocking Retrieval Infrastructure)

**Purpose**: Build the shared retrieval, ranking, and prompt infrastructure that blocks all user stories.

**Critical**: No user story work should begin until this phase is complete.

- [ ] T005 Expand startup indexing, document/category loading, and keyword normalization in `backend/src/backend.Application/Services/RagService/RagAppService.cs`
- [ ] T006 [P] Create source-hint extraction scaffolding in `backend/src/backend.Application/Services/RagService/RagSourceHintExtractor.cs`
- [ ] T007 [P] Create document-candidate ranking scaffolding in `backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs`
- [ ] T008 [P] Create deterministic confidence-evaluation scaffolding in `backend/src/backend.Application/Services/RagService/RagConfidenceEvaluator.cs`
- [ ] T009 Add shared mode-aware prompt and temperature entry points in `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs`

**Checkpoint**: Shared DTOs, retrieval helpers, and prompt hooks exist so user stories can be implemented against a stable foundation.

---

## Phase 3: User Story 1 - Ask in plain language without naming an Act (Priority: P1)

**Goal**: Let users ask everyday legal questions without naming statutes and still receive grounded cited answers.

**Independent Test**: Submit plain-language legal questions that omit Act names and verify the service returns grounded answers with the correct primary source and citations.

### Tests for User Story 1

- [ ] T010 [P] [US1] Create plain-language primary-source selection tests in `backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs`
- [ ] T011 [P] [US1] Add plain-language ask-flow assertions in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`

### Implementation for User Story 1

- [ ] T012 [US1] Implement title, short-name, act-number, and category matching in `backend/src/backend.Application/Services/RagService/RagSourceHintExtractor.cs`
- [ ] T013 [US1] Implement primary document scoring from semantic strength and metadata alignment in `backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs`
- [ ] T014 [US1] Refactor direct grounded retrieval and citation assembly in `backend/src/backend.Application/Services/RagService/RagAppService.cs`
- [ ] T015 [US1] Update direct-answer prompt instructions for grounded cited responses in `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs`

**Checkpoint**: User Story 1 should now return grounded cited answers for plain-language questions without requiring explicit Act names.

---

## Phase 4: User Story 2 - Different phrasings reach the same legal meaning (Priority: P2)

**Goal**: Keep source selection stable across colloquial, formal, and paraphrased versions of the same legal issue.

**Independent Test**: Ask semantically equivalent variants of the same legal question and verify the same primary legal source is selected across variants.

### Tests for User Story 2

- [ ] T016 [P] [US2] Add paraphrase-consistency ranking cases in `backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs`
- [ ] T017 [P] [US2] Add equivalent-question source-consistency assertions in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`

### Implementation for User Story 2

- [ ] T018 [US2] Calibrate wider semantic candidate pooling and per-document chunk caps in `backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs`
- [ ] T019 [US2] Normalize colloquial phrase handling and additive hint boosts in `backend/src/backend.Application/Services/RagService/RagSourceHintExtractor.cs`
- [ ] T020 [US2] Stabilize translated-query source selection for semantically equivalent questions in `backend/src/backend.Application/Services/RagService/RagAppService.cs`

**Checkpoint**: User Story 2 should now keep the same primary source family across semantically equivalent question variants.

---

## Phase 5: User Story 3 - Multi-source answers are assembled automatically (Priority: P3)

**Goal**: Combine a primary governing source with supporting legal sources when the answer needs more than one document.

**Independent Test**: Submit a question that needs more than one source and verify the answer cites the relevant source set instead of relying on a single partial match.

### Tests for User Story 3

- [ ] T021 [P] [US3] Add supporting-document selection cases in `backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs`
- [ ] T022 [P] [US3] Add multi-source grounding and citation assertions in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`

### Implementation for User Story 3

- [ ] T023 [US3] Implement primary-plus-supporting document selection rules in `backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs`
- [ ] T024 [US3] Update grounded context construction for multi-source answers in `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs`
- [ ] T025 [US3] Persist and return multi-source grounded citations in `backend/src/backend.Application/Services/RagService/RagAppService.cs`
- [ ] T026 [US3] Extend ask-response typing for multi-citation grounded answers in `frontend/src/services/qa.service.ts`

**Checkpoint**: User Story 3 should now assemble and cite the right combination of sources when one Act alone is not enough.

---

## Phase 6: User Story 4 - The system becomes more cautious when certainty is weak (Priority: P4)

**Goal**: Switch between direct, cautious, clarification, and insufficient responses based on deterministic grounding confidence.

**Independent Test**: Submit ambiguous or weakly grounded questions and verify the service asks for clarification or returns a clearly limited response instead of presenting a confident legal conclusion.

### Tests for User Story 4

- [ ] T027 [P] [US4] Add confidence-band and answer-mode decision tests in `backend/test/backend.Tests/RagServiceTests/RagConfidenceEvaluatorTests.cs`
- [ ] T028 [P] [US4] Add mode-aware prompt and temperature tests in `backend/test/backend.Tests/RagServiceTests/RagPromptBuilderTests.cs`
- [ ] T029 [P] [US4] Add clarification, insufficiency, and no-general-fallback coverage in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`

### Implementation for User Story 4

- [ ] T030 [US4] Implement deterministic confidence scoring and answer-mode mapping in `backend/src/backend.Application/Services/RagService/RagConfidenceEvaluator.cs`
- [ ] T031 [US4] Implement cautious, clarification, and insufficient prompt paths in `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs`
- [ ] T032 [US4] Route low-confidence asks to clarification or insufficiency and stop persisting non-grounded answers in `backend/src/backend.Application/Services/RagService/RagAppService.cs`
- [ ] T033 [P] [US4] Extend mode-aware response handling in `frontend/src/services/qa.service.ts` and `frontend/src/hooks/useChat.ts`
- [ ] T034 [P] [US4] Propagate answer mode, confidence, and clarification state in `frontend/src/providers/chat-provider/context.tsx` and `frontend/src/providers/chat-provider/index.tsx`
- [ ] T035 [P] [US4] Render localized caution and clarification presentation in `frontend/src/components/chat/ChatMessage.tsx`
- [ ] T036 [P] [US4] Add localized labels and helper copy in `frontend/src/messages/en.json`, `frontend/src/messages/zu.json`, `frontend/src/messages/st.json`, and `frontend/src/messages/af.json`

**Checkpoint**: User Story 4 should now safely downgrade uncertain legal answers and clearly communicate that state in the Ask UI.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Validate the full feature, tighten regressions, and confirm the documented smoke scenarios.

- [ ] T037 [P] Run the backend regression suite for updated retrieval and mode coverage in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagPromptBuilderTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs`, and `backend/test/backend.Tests/RagServiceTests/RagConfidenceEvaluatorTests.cs`
- [ ] T038 [P] Run frontend lint and type-safety validation for the updated Ask flow in `frontend/src/services/qa.service.ts`, `frontend/src/hooks/useChat.ts`, `frontend/src/providers/chat-provider/context.tsx`, `frontend/src/providers/chat-provider/index.tsx`, and `frontend/src/components/chat/ChatMessage.tsx`
- [ ] T039 Validate the quickstart smoke scenarios in `specs/feat/021-intent-aware-rag/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- Phase 1 -> no dependencies and can start immediately.
- Phase 2 -> depends on Phase 1 and blocks all user stories.
- Phase 3 -> depends on Phase 2 and delivers the MVP.
- Phase 4 -> depends on Phase 3 because it tunes the same retrieval planner, hint extraction, and ask-flow files for paraphrase consistency.
- Phase 5 -> depends on Phase 3 because multi-source assembly extends the grounded retrieval path already introduced for plain-language answers.
- Phase 6 -> depends on Phase 2 and Phase 3; frontend work in this phase also depends on the response contract from Phase 1.
- Phase 7 -> depends on all implemented user stories.

### User Story Dependencies

- US1 (P1) -> first deliverable and recommended MVP scope.
- US2 (P2) -> builds on the US1 retrieval baseline to make source selection stable across phrasing variants.
- US3 (P3) -> builds on the US1 retrieval baseline to add supporting-source assembly.
- US4 (P4) -> builds on the foundational evaluator/prompt scaffolding plus the grounded retrieval path from US1.

### Within Each User Story

- Write or extend tests first and confirm they fail before implementation.
- Complete backend retrieval and prompt changes before wiring the frontend for that story.
- Finish the story-specific checkpoint before moving to the next priority slice.

---

## Parallel Opportunities

- T001-T002 can be split across two DTO files in parallel.
- T006-T008 can run in parallel because each task owns a new backend service file.
- T010-T011, T016-T017, T021-T022, and T027-T029 can run in parallel within their test phases.
- T033-T036 can run in parallel once T032 has finalized the backend response shape.
- T037 and T038 can run in parallel during final validation.

---

## Parallel Example: User Story 1

```bash
# Tests that can run in parallel for US1
Task: "Create plain-language primary-source selection tests in backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs"
Task: "Add plain-language ask-flow assertions in backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs"
```

## Parallel Example: User Story 4

```bash
# Frontend tasks that can run in parallel for US4 after backend response modes are stable
Task: "Extend mode-aware response handling in frontend/src/services/qa.service.ts and frontend/src/hooks/useChat.ts"
Task: "Propagate answer mode, confidence, and clarification state in frontend/src/providers/chat-provider/context.tsx and frontend/src/providers/chat-provider/index.tsx"
Task: "Render localized caution and clarification presentation in frontend/src/components/chat/ChatMessage.tsx"
Task: "Add localized labels and helper copy in frontend/src/messages/en.json, frontend/src/messages/zu.json, frontend/src/messages/st.json, and frontend/src/messages/af.json"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3.
3. Run the US1 tests and plain-language smoke checks.
4. Stop and validate the MVP before expanding scope.

### Incremental Delivery

1. Deliver US1 to prove plain-language retrieval works without Act names.
2. Add US2 to make source selection stable across paraphrases.
3. Add US3 to support multi-source grounded answers.
4. Add US4 to safely downgrade weakly grounded answers and surface that state in the UI.

### Recommended Execution Order

1. T001-T009
2. T010-T015
3. T016-T020
4. T021-T026
5. T027-T036
6. T037-T039

---

## Notes

- All tasks follow the required checklist format with task ID, optional parallel marker, optional story label, and exact file path.
- No database migration tasks are included because the plan and research explicitly rule out schema changes for this feature.
- The canonical implementation folder for this feature is `specs/feat/021-intent-aware-rag/`; the older `specs/feat-021-intent-aware-rag/` path should not be used for task execution.
