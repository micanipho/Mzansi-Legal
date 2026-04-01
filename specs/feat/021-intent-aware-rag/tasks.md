# Tasks: Intent-Aware Legal Retrieval for RAG Answers

**Input**: Design documents from `/specs/feat/021-intent-aware-rag/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/qa-ask.md`, `quickstart.md`

**Tests**: Backend tests and frontend validation are required for this feature because the specification and plan require deterministic retrieval behavior, answer-mode safety, and Ask-page verification.

**Organization**: Tasks are grouped by user story so each story can be implemented and validated as an independently testable increment.

## Format: `[ID] [P?] [Story] Description`

- `[P]` marks a task that can run in parallel with other tasks in the same phase when file ownership does not overlap.
- `[Story]` labels trace story-specific work back to the corresponding user story in `spec.md`.
- Every task includes the exact file path to change or validate.

## Phase 1: Setup (Shared Contract & Benchmark Scaffolding)

**Purpose**: Lock the shared contract and benchmark guidance that all later implementation slices depend on.

- [ ] T001 Verify append-only answer-mode response fields in `backend/src/backend.Application/Services/RagService/DTO/RagAnswerResult.cs`, `backend/src/backend.Application/Services/RagService/DTO/RagAnswerMode.cs`, and `backend/src/backend.Application/Services/RagService/DTO/RagConfidenceBand.cs`
- [ ] T002 [P] Align service and API contract documentation in `backend/src/backend.Application/Services/RagService/IRagAppService.cs`, `backend/src/backend.Web.Host/Controllers/QaController.cs`, and `specs/feat/021-intent-aware-rag/contracts/qa-ask.md`
- [ ] T003 [P] Preserve benchmark smoke scenarios and execution notes in `specs/feat/021-intent-aware-rag/quickstart.md`

---

## Phase 2: Foundational (Blocking Retrieval Baseline)

**Purpose**: Stabilize the shared in-memory retrieval baseline before story-specific calibration begins.

**Critical**: No user story work should begin until this phase is complete.

- [ ] T004 [P] Add foundational baseline coverage in `backend/test/backend.Tests/RagServiceTests/RagIndexStoreTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagDocumentProfileBuilderTests.cs`, and `backend/test/backend.Tests/RagServiceTests/RagQueryFocusBuilderTests.cs`
- [ ] T005 Refine startup indexing and atomic cache replacement in `backend/src/backend.Application/Services/RagService/RagAppService.cs` and `backend/src/backend.Application/Services/RagService/RagIndexStore.cs`
- [ ] T006 [P] Refine metadata phrase, alias, and centroid generation in `backend/src/backend.Application/Services/RagService/RagDocumentProfileBuilder.cs`
- [ ] T007 [P] Refine focus-query extraction for generic legal filler terms in `backend/src/backend.Application/Services/RagService/RagQueryFocusBuilder.cs`
- [ ] T008 [P] Lock deterministic prompt temperatures and non-grounded helper behavior in `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs`

**Checkpoint**: The in-memory index, document profiles, focus-query behavior, and prompt helpers are stable enough for story-by-story calibration.

---

## Phase 3: User Story 1 - Ask in plain language without naming an Act (Priority: P1)

**Goal**: Let users ask everyday legal questions without naming statutes and still receive grounded answers with citations.

**Independent Test**: Submit plain-language legal questions that omit Act names and verify the service returns grounded answers with the correct primary source family and citations.

### Tests for User Story 1

- [ ] T009 [P] [US1] Add plain-language primary-source ranking cases in `backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs`
- [ ] T010 [P] [US1] Add plain-language ask-flow and citation assertions in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`

### Implementation for User Story 1

- [ ] T011 [P] [US1] Refine additive source-hint extraction for plain-language questions in `backend/src/backend.Application/Services/RagService/RagSourceHintExtractor.cs`
- [ ] T012 [P] [US1] Reweight primary document scoring for plain-language retrieval in `backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs`
- [ ] T013 [P] [US1] Tighten direct-answer prompt instructions for grounded cited responses in `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs`
- [ ] T014 [US1] Refine grounded answer orchestration and citation assembly in `backend/src/backend.Application/Services/RagService/RagAppService.cs`

**Checkpoint**: User Story 1 should now return grounded cited answers for plain-language legal questions without requiring explicit Act names.

---

## Phase 4: User Story 2 - Different phrasings reach the same legal meaning (Priority: P2)

**Goal**: Keep source selection stable across colloquial, formal, and paraphrased versions of the same legal issue.

**Independent Test**: Ask semantically equivalent versions of the same legal question and verify the same primary legal source family is selected across variants.

### Tests for User Story 2

- [ ] T015 [P] [US2] Add paraphrase and colloquial-term benchmark cases in `backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs`
- [ ] T016 [P] [US2] Add paraphrase consistency assertions in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs` and `backend/test/backend.Tests/RagServiceTests/RagQueryFocusBuilderTests.cs`

### Implementation for User Story 2

- [ ] T017 [P] [US2] Refine colloquial-term alias handling in `backend/src/backend.Application/Services/RagService/RagSourceHintExtractor.cs` and `backend/src/backend.Application/Services/RagService/RagDocumentProfileBuilder.cs`
- [ ] T018 [P] [US2] Calibrate semantic candidate pooling and metadata specificity in `backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs`
- [ ] T019 [P] [US2] Refine focus-query generation for semantically equivalent phrasings in `backend/src/backend.Application/Services/RagService/RagQueryFocusBuilder.cs`
- [ ] T020 [US2] Stabilize translated-query source selection across equivalent questions in `backend/src/backend.Application/Services/RagService/RagAppService.cs`

**Checkpoint**: User Story 2 should now keep the same primary source family across semantically equivalent question variants.

---

## Phase 5: User Story 3 - Multi-source answers are assembled automatically (Priority: P3)

**Goal**: Combine a primary governing source with supporting legal sources when the answer needs more than one document.

**Independent Test**: Submit a question that needs more than one source and verify the answer cites the relevant source set instead of relying on a single partial match.

### Tests for User Story 3

- [ ] T021 [P] [US3] Add supporting-document selection cases in `backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs`
- [ ] T022 [P] [US3] Add multi-source grounding and citation assertions in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs` and `backend/test/backend.Tests/RagServiceTests/RagPromptBuilderTests.cs`

### Implementation for User Story 3

- [ ] T023 [P] [US3] Refine primary-plus-supporting document selection and per-document chunk caps in `backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs`
- [ ] T024 [P] [US3] Refine multi-source context ordering and citation wording in `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs`
- [ ] T025 [US3] Refine multi-source answer persistence and citation return handling in `backend/src/backend.Application/Services/RagService/RagAppService.cs`

**Checkpoint**: User Story 3 should now assemble and cite the right combination of sources when one Act alone is not enough.

---

## Phase 6: User Story 4 - The system becomes more cautious when certainty is weak (Priority: P4)

**Goal**: Switch between direct, cautious, clarification, and insufficient responses based on deterministic grounding confidence.

**Independent Test**: Submit ambiguous or weakly grounded questions and verify the service asks for clarification or returns a clearly limited response instead of presenting a confident legal conclusion.

### Tests for User Story 4

- [ ] T026 [P] [US4] Add confidence-band and answer-mode threshold coverage in `backend/test/backend.Tests/RagServiceTests/RagConfidenceEvaluatorTests.cs`
- [ ] T027 [P] [US4] Add clarification and insufficiency prompt behavior coverage in `backend/test/backend.Tests/RagServiceTests/RagPromptBuilderTests.cs`
- [ ] T028 [P] [US4] Add no-general-fallback and non-persistence coverage in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`

### Implementation for User Story 4

- [ ] T029 [P] [US4] Recalibrate deterministic confidence thresholds in `backend/src/backend.Application/Services/RagService/RagConfidenceEvaluator.cs`
- [ ] T030 [P] [US4] Refine clarification and insufficiency prompt paths in `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs`
- [ ] T031 [US4] Route low-confidence asks through clarification or insufficiency paths in `backend/src/backend.Application/Services/RagService/RagAppService.cs`
- [ ] T032 [P] [US4] Normalize mode-aware Ask API handling in `frontend/src/services/qa.service.ts` and `frontend/src/hooks/useChat.ts`
- [ ] T033 [P] [US4] Propagate answer mode and clarification state in `frontend/src/providers/chat-provider/context.tsx` and `frontend/src/providers/chat-provider/index.tsx`
- [ ] T034 [P] [US4] Refine caution and clarification rendering in `frontend/src/components/chat/ChatMessage.tsx`
- [ ] T035 [P] [US4] Update localized mode labels and helper copy in `frontend/src/messages/en.json`, `frontend/src/messages/zu.json`, `frontend/src/messages/st.json`, and `frontend/src/messages/af.json`

**Checkpoint**: User Story 4 should now safely downgrade weakly grounded answers and clearly communicate that state in the Ask UI.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Validate the full feature, tighten regressions, and confirm the documented benchmark smoke scenarios.

- [ ] T036 [P] Run the backend regression suite in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagPromptBuilderTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagConfidenceEvaluatorTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagDocumentProfileBuilderTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagQueryFocusBuilderTests.cs`, and `backend/test/backend.Tests/RagServiceTests/RagIndexStoreTests.cs`
- [ ] T037 [P] Run frontend lint and type-safety validation for `frontend/src/services/qa.service.ts`, `frontend/src/hooks/useChat.ts`, `frontend/src/providers/chat-provider/context.tsx`, `frontend/src/providers/chat-provider/index.tsx`, and `frontend/src/components/chat/ChatMessage.tsx`
- [ ] T038 Validate the benchmark smoke scenarios and deferred-follow-on notes in `specs/feat/021-intent-aware-rag/quickstart.md` and `specs/feat/021-intent-aware-rag/plan.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- Phase 1 -> no dependencies and can start immediately.
- Phase 2 -> depends on Phase 1 and blocks all user stories.
- Phase 3 -> depends on Phase 2 and delivers the MVP.
- Phase 4 -> depends on Phase 3 because paraphrase consistency extends the same retrieval baseline introduced for plain-language answers.
- Phase 5 -> depends on Phase 3 because multi-source assembly extends the grounded retrieval path established for plain-language answers.
- Phase 6 -> depends on Phase 2 and Phase 3; frontend work in this phase also depends on the stable response contract from Phase 1.
- Phase 7 -> depends on all implemented user stories.

### User Story Dependencies

- US1 (P1) -> first deliverable and recommended MVP scope.
- US2 (P2) -> builds on the US1 retrieval baseline to make source selection stable across phrasing variants.
- US3 (P3) -> builds on the US1 retrieval baseline to add supporting-source assembly.
- US4 (P4) -> builds on the foundational evaluator/prompt baseline plus the grounded retrieval path from US1.

### Within Each User Story

- Write or extend the story tests first and confirm they fail before implementation.
- Complete backend retrieval and prompt changes before wiring any frontend behavior for that story.
- Finish the story checkpoint before moving to the next priority slice.

---

## Parallel Opportunities

- T002 and T003 can run in parallel because they touch different contract and documentation files.
- T006-T008 can run in parallel after T005 because each task owns a different foundational backend file.
- T009-T010, T015-T016, T021-T022, and T026-T028 can run in parallel inside their story test phases.
- T011-T013 can run in parallel for US1 before T014 integrates them into the main ask flow.
- T017-T019 can run in parallel for US2 because they own different backend files.
- T023-T024 can run in parallel for US3 before T025 integrates the retrieval results into the service.
- T029-T030 can run in parallel for US4 before T031 integrates the mode-routing behavior.
- T032-T035 can run in parallel for US4 after T031 stabilizes the backend response behavior.
- T036 and T037 can run in parallel during final validation.

---

## Parallel Example: User Story 1

```bash
# Tests that can run in parallel for US1
Task: "Add plain-language primary-source ranking cases in backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs"
Task: "Add plain-language ask-flow and citation assertions in backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs"

# Backend refinement tasks that can run in parallel for US1
Task: "Refine additive source-hint extraction for plain-language questions in backend/src/backend.Application/Services/RagService/RagSourceHintExtractor.cs"
Task: "Reweight primary document scoring for plain-language retrieval in backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs"
Task: "Tighten direct-answer prompt instructions for grounded cited responses in backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs"
```

## Parallel Example: User Story 4

```bash
# Frontend tasks that can run in parallel for US4 after backend mode routing is stable
Task: "Normalize mode-aware Ask API handling in frontend/src/services/qa.service.ts and frontend/src/hooks/useChat.ts"
Task: "Propagate answer mode and clarification state in frontend/src/providers/chat-provider/context.tsx and frontend/src/providers/chat-provider/index.tsx"
Task: "Refine caution and clarification rendering in frontend/src/components/chat/ChatMessage.tsx"
Task: "Update localized mode labels and helper copy in frontend/src/messages/en.json, frontend/src/messages/zu.json, frontend/src/messages/st.json, and frontend/src/messages/af.json"
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
2. Add US2 to keep source selection stable across paraphrases.
3. Add US3 to support multi-source grounded answers.
4. Add US4 to safely downgrade weakly grounded answers and surface that state in the UI.

### Recommended Execution Order

1. T001-T008
2. T009-T014
3. T015-T020
4. T021-T025
5. T026-T035
6. T036-T038

---

## Notes

- All tasks follow the required checklist format with task ID, optional parallel marker, optional story label, and exact file path.
- No database migration tasks are included because the plan and research explicitly rule out schema changes for this feature.
- `frontend/src/services/qa.service.ts` is the Ask-flow API client for this feature; the older `frontend/src/services/qaService.ts` file is outside the scope of this task list unless the feature scope changes.
