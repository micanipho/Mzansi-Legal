# Tasks: Intent-Aware Legal Retrieval for RAG Answers

**Input**: Design documents from `/specs/feat/021-intent-aware-rag/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/qa-ask.md`, `quickstart.md`

**Tests**: Backend tests and frontend validation are required for this feature because the specification and plan require deterministic retrieval behavior, source-role labeling, answer-mode safety, and Ask-page verification.

**Organization**: Tasks are grouped by user story so each story can be implemented and validated as an independently testable increment.

## Format: `[ID] [P?] [Story] Description`

- `[P]` marks a task that can run in parallel with other tasks in the same phase when file ownership does not overlap.
- `[Story]` labels trace story-specific work back to the corresponding user story in `spec.md`.
- Every task includes the exact file path to change or validate.

## Phase 1: Setup (Shared Contract and Coverage Scaffolding)

**Purpose**: Lock the append-only Ask contract, coverage-state benchmark language, and doc alignment that all later implementation slices depend on.

- [x] T001 Verify append-only answer and citation response fields in `backend/src/backend.Application/Services/RagService/DTO/RagAnswerResult.cs`, `backend/src/backend.Application/Services/RagService/DTO/RagCitationDto.cs`, `backend/src/backend.Application/Services/RagService/DTO/RagAnswerMode.cs`, and `backend/src/backend.Application/Services/RagService/DTO/RagConfidenceBand.cs`
- [x] T002 [P] Align Ask service and API contract documentation in `backend/src/backend.Application/Services/RagService/IRagAppService.cs`, `backend/src/backend.Web.Host/Controllers/QaController.cs`, and `specs/feat/021-intent-aware-rag/contracts/qa-ask.md`
- [x] T003 [P] Lock the coverage-state benchmark matrix and follow-on corpus notes in `specs/feat/021-intent-aware-rag/quickstart.md`, `specs/feat/021-intent-aware-rag/research.md`, and `specs/feat/021-intent-aware-rag/plan.md`

---

## Phase 2: Foundational (Blocking Retrieval and Source-Label Baseline)

**Purpose**: Stabilize the shared in-memory retrieval and source-classification baseline before story-specific calibration begins.

**Critical**: No user story work should begin until this phase is complete.

- [x] T004 [P] Add foundational source-classification coverage in `backend/test/backend.Tests/RagServiceTests/RagDocumentProfileBuilderTests.cs` and `backend/test/backend.Tests/RagServiceTests/RagIndexStoreTests.cs`
- [x] T005 Refine document-profile and in-memory source metadata derivation in `backend/src/backend.Application/Services/RagService/RagDocumentProfileBuilder.cs` and `backend/src/backend.Application/Services/RagService/RagIndexStore.cs`
- [x] T006 [P] Normalize current manifest source families and guidance cues in `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/LegislationManifest.cs`
- [x] T007 [P] Refine broad-topic focus extraction for short or generic legal prompts in `backend/src/backend.Application/Services/RagService/RagQueryFocusBuilder.cs`
- [x] T008 [P] Refine additive source-hint extraction for titles, abbreviations, and noisy labels in `backend/src/backend.Application/Services/RagService/RagSourceHintExtractor.cs`

**Checkpoint**: The in-memory retrieval baseline can classify current sources, group source families consistently, and support coverage-aware ranking without new schema or infrastructure.

---

## Phase 3: User Story 1 - Ask in everyday language and still reach the right law (Priority: P1)

**Goal**: Let ordinary users describe supported legal problems in plain language and still receive grounded answers tied to the correct primary legal source.

**Independent Test**: Submit supported housing and labour questions that do not mention Act names and verify the service returns plain-language answers grounded in the correct primary source on the first pass.

### Tests for User Story 1

- [x] T009 [P] [US1] Add plain-language primary-source ranking cases in `backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs`
- [x] T010 [P] [US1] Add plain-language grounded ask-flow assertions in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`

### Implementation for User Story 1

- [x] T011 [P] [US1] Reweight document and chunk scoring for plain-language source discovery in `backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs`
- [x] T012 [P] [US1] Tighten plain-language answer prompting for grounded non-lawyer explanations in `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs`
- [x] T013 [US1] Refine grounded answer orchestration and citation selection in `backend/src/backend.Application/Services/RagService/RagAppService.cs`

**Checkpoint**: User Story 1 should now return grounded cited answers for supported plain-language questions without requiring explicit Act names.

---

## Phase 4: User Story 2 - Different phrasings and wrong source hints still converge on the right meaning (Priority: P2)

**Goal**: Keep source selection stable across colloquial, paraphrased, and misleadingly labeled variants of the same supported legal issue.

**Independent Test**: Ask semantically equivalent versions of the same supported legal question, including a wrong Act hint, and verify the same primary source or source family is selected or the wrong hint is explicitly corrected.

### Tests for User Story 2

- [x] T014 [P] [US2] Add paraphrase and wrong-source-hint ranking cases in `backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs`
- [x] T015 [P] [US2] Add paraphrase consistency and wrong-hint ask-flow assertions in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs` and `backend/test/backend.Tests/RagServiceTests/RagQueryFocusBuilderTests.cs`

### Implementation for User Story 2

- [x] T016 [P] [US2] Expand colloquial and misleading source-hint handling in `backend/src/backend.Application/Services/RagService/RagSourceHintExtractor.cs`
- [x] T017 [P] [US2] Calibrate source-family consistency and semantic breadth scoring in `backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs`
- [x] T018 [US2] Refine focus-query generation and translated-query source selection for paraphrase stability in `backend/src/backend.Application/Services/RagService/RagQueryFocusBuilder.cs` and `backend/src/backend.Application/Services/RagService/RagAppService.cs`

**Checkpoint**: User Story 2 should now keep the same primary source family across semantically equivalent and wrong-hint question variants.

---

## Phase 5: User Story 3 - Users can see what is law, what is official guidance, and why multiple sources were used (Priority: P3)

**Goal**: Return multi-source answers that clearly distinguish binding law from supporting guidance and make the source roles visible in the Ask experience.

**Independent Test**: Ask a supported question that requires both a primary legal source and supporting guidance, then verify the answer labels the source roles clearly and keeps binding law visibly primary.

### Tests for User Story 3

- [x] T019 [P] [US3] Add law-vs-guidance and multi-source selection cases in `backend/test/backend.Tests/RagServiceTests/RagDocumentProfileBuilderTests.cs` and `backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs`
- [x] T020 [P] [US3] Add labeled citation and source-role response assertions in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs` and `backend/test/backend.Tests/RagServiceTests/RagPromptBuilderTests.cs`

### Implementation for User Story 3

- [x] T021 [P] [US3] Extend citation contract fields for source title, locator, authority type, and source role in `backend/src/backend.Application/Services/RagService/DTO/RagCitationDto.cs`, `backend/src/backend.Application/Services/RagService/DTO/RagAnswerResult.cs`, and `specs/feat/021-intent-aware-rag/contracts/qa-ask.md`
- [x] T022 [P] [US3] Derive authority type and primary-vs-supporting source labels in `backend/src/backend.Application/Services/RagService/RagDocumentProfileBuilder.cs` and `backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs`
- [x] T023 [P] [US3] Refine prompt wording for law-vs-guidance explanations in `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs`
- [x] T024 [US3] Return labeled citation sets and source-role aware answers in `backend/src/backend.Application/Services/RagService/RagAppService.cs`
- [x] T025 [P] [US3] Propagate labeled citation fields through `frontend/src/services/qa.service.ts`, `frontend/src/hooks/useChat.ts`, `frontend/src/providers/chat-provider/context.tsx`, `frontend/src/providers/chat-provider/index.tsx`, and `frontend/src/providers/chat-provider/reducer.tsx`
- [x] T026 [P] [US3] Render source-role badges and guidance notices in `frontend/src/components/chat/ChatMessage.tsx`
- [x] T027 [P] [US3] Update localized law-vs-guidance copy in `frontend/src/messages/en.json`, `frontend/src/messages/zu.json`, `frontend/src/messages/st.json`, and `frontend/src/messages/af.json`

**Checkpoint**: User Story 3 should now assemble and label the right combination of law and guidance sources when both are needed.

---

## Phase 6: User Story 4 - The system becomes more cautious or escalates when certainty or stakes are high (Priority: P4)

**Goal**: Route ambiguous, unsupported, or urgent prompts into clarification, limited-answer, or escalation-safe outcomes instead of confident legal conclusions.

**Independent Test**: Submit ambiguous, unsupported, and urgent benchmark prompts and verify the service asks for clarification, limits the answer, or includes escalation language instead of presenting a definitive legal conclusion.

### Tests for User Story 4

- [x] T028 [P] [US4] Add confidence-band, ambiguity, and risk-trigger coverage in `backend/test/backend.Tests/RagServiceTests/RagConfidenceEvaluatorTests.cs`
- [x] T029 [P] [US4] Add clarification, insufficiency, and escalation prompt coverage in `backend/test/backend.Tests/RagServiceTests/RagPromptBuilderTests.cs`
- [x] T030 [P] [US4] Add non-persistence and urgent-response orchestration coverage in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`

### Implementation for User Story 4

- [x] T031 [P] [US4] Recalibrate deterministic thresholds and risk-trigger evaluation in `backend/src/backend.Application/Services/RagService/RagConfidenceEvaluator.cs`
- [x] T032 [P] [US4] Refine clarification, insufficiency, and escalation prompts in `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs`
- [x] T033 [US4] Route urgent or under-grounded asks through clarification or escalation-safe paths in `backend/src/backend.Application/Services/RagService/RagAppService.cs`
- [x] T034 [P] [US4] Normalize clarification, insufficiency, and escalation state handling in `frontend/src/services/qa.service.ts`, `frontend/src/hooks/useChat.ts`, `frontend/src/providers/chat-provider/context.tsx`, `frontend/src/providers/chat-provider/index.tsx`, and `frontend/src/providers/chat-provider/reducer.tsx`
- [x] T035 [P] [US4] Render urgent limitation and escalation states accessibly in `frontend/src/components/chat/ChatMessage.tsx`
- [x] T036 [P] [US4] Update localized caution, clarification, and escalation copy in `frontend/src/messages/en.json`, `frontend/src/messages/zu.json`, `frontend/src/messages/st.json`, and `frontend/src/messages/af.json`

**Checkpoint**: User Story 4 should now safely downgrade weak or urgent questions and communicate that state clearly in the Ask UI.

---

## Phase 7: Polish and Cross-Cutting Concerns

**Purpose**: Validate the full feature, tighten regressions, and confirm the documented coverage-state boundaries and follow-on bundle notes.

- [x] T037 [P] Run the backend regression suite in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagPromptBuilderTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagConfidenceEvaluatorTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagDocumentProfileBuilderTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagQueryFocusBuilderTests.cs`, and `backend/test/backend.Tests/RagServiceTests/RagIndexStoreTests.cs`
- [x] T038 [P] Run frontend lint and type-safety validation for `frontend/src/services/qa.service.ts`, `frontend/src/hooks/useChat.ts`, `frontend/src/providers/chat-provider/context.tsx`, `frontend/src/providers/chat-provider/index.tsx`, `frontend/src/providers/chat-provider/reducer.tsx`, and `frontend/src/components/chat/ChatMessage.tsx`
- [x] T039 Validate the current-corpus benchmark matrix and coverage-state smoke prompts in `specs/feat/021-intent-aware-rag/quickstart.md`, `specs/feat/021-intent-aware-rag/research.md`, and `specs/feat/021-intent-aware-rag/plan.md`

---

## Dependencies and Execution Order

### Phase Dependencies

- Phase 1 -> no dependencies and can start immediately.
- Phase 2 -> depends on Phase 1 and blocks all user stories.
- Phase 3 -> depends on Phase 2 and delivers the MVP.
- Phase 4 -> depends on Phase 3 because paraphrase stability extends the retrieval baseline proven in US1.
- Phase 5 -> depends on Phase 2 and Phase 3 because source-role labeling builds on the stabilized retrieval baseline and grounded citation path.
- Phase 6 -> depends on Phase 2 and Phase 3; frontend safety rendering also depends on the append-only contract from Phase 1 and the citation/source-role propagation from Phase 5.
- Phase 7 -> depends on all implemented user stories.

### User Story Dependencies

- US1 (P1) -> first deliverable and recommended MVP scope.
- US2 (P2) -> builds on the US1 retrieval baseline to make source selection stable across phrasing variants.
- US3 (P3) -> builds on the foundational source-classification baseline and the grounded answer path from US1.
- US4 (P4) -> builds on the grounded retrieval baseline from US1 and the structured response contract from Phase 1; UI state propagation also benefits from the labeled citation path in US3.

### Within Each User Story

- Write or extend the story tests first and confirm they fail before implementation.
- Complete backend retrieval, prompt, and orchestration changes before wiring frontend behavior for that story.
- Finish the story checkpoint before moving to the next priority slice.

---

## Parallel Opportunities

- T002 and T003 can run in parallel because they touch different contract and documentation files.
- T006-T008 can run in parallel after T005 because they own different foundational files.
- T009-T010, T014-T015, T019-T020, and T028-T030 can run in parallel inside their story test phases.
- T011-T012 can run in parallel for US1 before T013 integrates the retrieval and answer changes.
- T016-T017 can run in parallel for US2 before T018 integrates focus-query and orchestration behavior.
- T021-T023 can run in parallel for US3 before T024 integrates the labeled source set into the main ask flow.
- T025-T027 can run in parallel for US3 after T024 stabilizes the backend contract.
- T031-T032 can run in parallel for US4 before T033 integrates the mode-routing behavior.
- T034-T036 can run in parallel for US4 after T033 stabilizes the backend response behavior.
- T037 and T038 can run in parallel during final validation.

---

## Parallel Example: User Story 3

```bash
# Tests that can run in parallel for US3
Task: "Add law-vs-guidance and multi-source selection cases in backend/test/backend.Tests/RagServiceTests/RagDocumentProfileBuilderTests.cs and backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs"
Task: "Add labeled citation and source-role response assertions in backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs and backend/test/backend.Tests/RagServiceTests/RagPromptBuilderTests.cs"

# Backend refinement tasks that can run in parallel for US3
Task: "Extend citation contract fields for source title, locator, authority type, and source role in backend/src/backend.Application/Services/RagService/DTO/RagCitationDto.cs, backend/src/backend.Application/Services/RagService/DTO/RagAnswerResult.cs, and specs/feat/021-intent-aware-rag/contracts/qa-ask.md"
Task: "Derive authority type and primary-vs-supporting source labels in backend/src/backend.Application/Services/RagService/RagDocumentProfileBuilder.cs and backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs"
Task: "Refine prompt wording for law-vs-guidance explanations in backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs"
```

## Parallel Example: User Story 4

```bash
# Backend safety tasks that can run in parallel for US4
Task: "Recalibrate deterministic thresholds and risk-trigger evaluation in backend/src/backend.Application/Services/RagService/RagConfidenceEvaluator.cs"
Task: "Refine clarification, insufficiency, and escalation prompts in backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs"

# Frontend tasks that can run in parallel for US4 after backend routing is stable
Task: "Normalize clarification, insufficiency, and escalation state handling in frontend/src/services/qa.service.ts, frontend/src/hooks/useChat.ts, frontend/src/providers/chat-provider/context.tsx, frontend/src/providers/chat-provider/index.tsx, and frontend/src/providers/chat-provider/reducer.tsx"
Task: "Render urgent limitation and escalation states accessibly in frontend/src/components/chat/ChatMessage.tsx"
Task: "Update localized caution, clarification, and escalation copy in frontend/src/messages/en.json, frontend/src/messages/zu.json, frontend/src/messages/st.json, and frontend/src/messages/af.json"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3.
3. Run the US1 tests and current-corpus plain-language smoke checks.
4. Stop and validate the MVP before expanding scope.

### Incremental Delivery

1. Deliver US1 to prove plain-language supported retrieval works without Act names.
2. Add US2 to keep source selection stable across paraphrases and wrong hints.
3. Add US3 to label binding law vs guidance and surface multi-source roles clearly.
4. Add US4 to safely downgrade weak or urgent prompts and communicate that state in the UI.

### Recommended Execution Order

1. T001-T008
2. T009-T013
3. T014-T018
4. T019-T027
5. T028-T036
6. T037-T039

---

## Notes

- All tasks follow the required checklist format with task ID, optional parallel marker, optional story label, and exact file path.
- No database migration tasks are included because the plan and research explicitly rule out schema changes for the retrieval-hardening slice.
- The task list deliberately separates current corpus retrieval hardening from deferred corpus expansion work so missing-source scenarios are not mistaken for ranking regressions.

