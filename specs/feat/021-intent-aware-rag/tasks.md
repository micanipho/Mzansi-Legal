# Tasks: Intent-Aware Legal Retrieval for RAG Answers

**Input**: Design documents from `/specs/feat/021-intent-aware-rag/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/qa-ask.md`, `quickstart.md`

**Tests**: Include verification tasks for every changed behavior. This legal-AI feature requires deterministic backend tests, multilingual benchmark coverage, frontend lint/type validation, and quickstart-guided smoke checks.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested independently.

## Format: `[ID] [P?] [Story] Description`

- `[P]` marks a task that can run in parallel with other tasks in the same phase when file ownership does not overlap.
- `[Story]` labels story-specific work back to the matching user story in `spec.md`.
- Every task includes the exact file path to change or validate.

## Phase 1: Setup (Shared Contract and Benchmark Scaffolding)

**Purpose**: Lock the multilingual ask contract, benchmark expectations, and shared test scaffolding that every story depends on.

- [X] T001 Align multilingual ask contract comments and DTO surfaces in `backend/src/backend.Application/Services/LanguageService/ILanguageAppService.cs`, `backend/src/backend.Application/Services/RagService/DTO/AskQuestionRequest.cs`, `backend/src/backend.Application/Services/RagService/DTO/RagAnswerResult.cs`, `backend/src/backend.Application/Services/RagService/DTO/RagCitationDto.cs`, `backend/src/backend.Web.Host/Controllers/QaController.cs`, and `specs/feat/021-intent-aware-rag/contracts/qa-ask.md`
- [ ] T002 [P] Refresh multilingual benchmark, persistence-check, and deferred-corpus expectations in `specs/feat/021-intent-aware-rag/plan.md`, `specs/feat/021-intent-aware-rag/research.md`, and `specs/feat/021-intent-aware-rag/quickstart.md`
- [ ] T003 [P] Add shared multilingual prompt fixtures and assertion helpers in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagPromptBuilderTests.cs`, and `backend/test/backend.Tests/RagServiceTests/RagConfidenceEvaluatorTests.cs`

---

## Phase 2: Foundational (Blocking Language Routing and Retrieval Baseline)

**Purpose**: Stabilize the language-routing, persistence, and source-classification baseline before user-story-specific work begins.

**Critical**: No user story work should begin until this phase is complete.

- [X] T004 [P] Add foundational language-detection and translation coverage in `backend/test/backend.Tests/LanguageServiceTests/LanguageAppServiceTests.cs`
- [ ] T005 [P] Add foundational question-persistence and source-metadata coverage in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagDocumentProfileBuilderTests.cs`, and `backend/test/backend.Tests/RagServiceTests/RagIndexStoreTests.cs`
- [X] T006 Implement supported-language detection, translation, and English fallback rules in `backend/src/backend.Application/Services/LanguageService/LanguageAppService.cs` and `backend/src/backend.Application/Services/LanguageService/ILanguageAppService.cs`
- [X] T007 [P] Normalize multilingual question-history handling in `backend/src/backend.Core/Domains/QA/Question.cs`, `backend/src/backend.Core/Domains/QA/Conversation.cs`, and `backend/src/backend.Application/Services/RagService/RagAppService.cs`
- [ ] T008 [P] Normalize source-family and authority metadata derivation in `backend/src/backend.Application/Services/RagService/RagDocumentProfileBuilder.cs`, `backend/src/backend.Application/Services/RagService/RagIndexStore.cs`, `backend/src/backend.Application/Services/RagService/RagSourceMetadata.cs`, and `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/LegislationManifest.cs`
- [ ] T009 [P] Refine translated focus-query and source-hint preparation baselines in `backend/src/backend.Application/Services/RagService/RagQueryFocusBuilder.cs` and `backend/src/backend.Application/Services/RagService/RagSourceHintExtractor.cs`

**Checkpoint**: The system has a stable multilingual normalization baseline, canonical question persistence fields, and consistent source metadata before story work starts.

---

## Phase 3: User Story 1 - Ask in everyday language and still reach the right law (Priority: P1) MVP

**Goal**: Let users ask supported legal questions in English, isiZulu, Sesotho, or Afrikaans and still reach the correct primary source with answer prose in their own language.

**Independent Test**: Submit supported housing and labour questions in English, isiZulu, Sesotho, and Afrikaans without naming an Act and verify the service returns grounded answers tied to the correct primary source with citations kept in English.

### Verification for User Story 1

- [ ] T010 [P] [US1] Add multilingual plain-language retrieval and source-selection cases in `backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs` and `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`
- [X] T011 [P] [US1] Add language-directive and English-citation prompt coverage in `backend/test/backend.Tests/RagServiceTests/RagPromptBuilderTests.cs`

### Implementation for User Story 1

- [ ] T012 [P] [US1] Reweight translated-question retrieval scoring for multilingual plain-language discovery in `backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs`
- [ ] T013 [P] [US1] Implement same-language answer directives with English citation invariants in `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs`
- [X] T014 [US1] Implement end-to-end multilingual routing and question persistence in `backend/src/backend.Application/Services/RagService/RagAppService.cs`
- [X] T015 [P] [US1] Propagate detected-language-aware ask responses through `frontend/src/services/qa.service.ts`, `frontend/src/hooks/useChat.ts`, `frontend/src/providers/chat-provider/actions.tsx`, `frontend/src/providers/chat-provider/context.tsx`, `frontend/src/providers/chat-provider/reducer.tsx`, and `frontend/src/providers/chat-provider/index.tsx`
- [ ] T016 [P] [US1] Render multilingual answer state and citation language cues in `frontend/src/components/chat/ChatMessage.tsx`, `frontend/src/components/chat/CitationList.tsx`, and `frontend/src/components/chat/AskExperience.tsx`
- [ ] T017 [P] [US1] Update localized ask-flow copy for multilingual grounded answers in `frontend/src/messages/en.json`, `frontend/src/messages/zu.json`, `frontend/src/messages/st.json`, and `frontend/src/messages/af.json`

**Checkpoint**: User Story 1 should return grounded answers for supported multilingual prompts without requiring Act names.

---

## Phase 4: User Story 2 - Different phrasings and wrong source hints still converge on the right meaning (Priority: P2)

**Goal**: Keep source selection stable across paraphrases, colloquial wording, multilingual variants, and wrong legal-source hints.

**Independent Test**: Ask semantically equivalent English, isiZulu, Sesotho, and Afrikaans variants of the same issue, including a wrong Act hint, and verify the same primary source or source family is selected or the hint is corrected.

### Verification for User Story 2

- [ ] T018 [P] [US2] Add paraphrase, colloquial, and multilingual-equivalence retrieval cases in `backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs` and `backend/test/backend.Tests/RagServiceTests/RagQueryFocusBuilderTests.cs`
- [ ] T019 [P] [US2] Add wrong-source-hint and translated-question orchestration coverage in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs` and `backend/test/backend.Tests/RagServiceTests/RagConfidenceEvaluatorTests.cs`

### Implementation for User Story 2

- [ ] T020 [P] [US2] Expand colloquial and misleading-source hint extraction in `backend/src/backend.Application/Services/RagService/RagSourceHintExtractor.cs`
- [ ] T021 [P] [US2] Calibrate multilingual focus-query normalization for paraphrase stability in `backend/src/backend.Application/Services/RagService/RagQueryFocusBuilder.cs` and `backend/src/backend.Application/Services/LanguageService/LanguageAppService.cs`
- [ ] T022 [US2] Rebalance source-family consistency scoring and wrong-hint correction in `backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs` and `backend/src/backend.Application/Services/RagService/RagAppService.cs`

**Checkpoint**: User Story 2 should keep the same source family across semantically equivalent and wrong-hint prompts.

---

## Phase 5: User Story 3 - Users can see what is law, what is official guidance, and why multiple sources were used (Priority: P3)

**Goal**: Return multi-source answers that clearly distinguish binding law from supporting official guidance and expose those roles in the Ask experience.

**Independent Test**: Ask a supported question that needs both binding law and official guidance, then verify the answer labels the source roles clearly and keeps binding law visibly primary.

### Verification for User Story 3

- [ ] T023 [P] [US3] Add law-vs-guidance and multi-source ranking cases in `backend/test/backend.Tests/RagServiceTests/RagDocumentProfileBuilderTests.cs` and `backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs`
- [ ] T024 [P] [US3] Add labeled citation and source-role response assertions in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs` and `backend/test/backend.Tests/RagServiceTests/RagPromptBuilderTests.cs`

### Implementation for User Story 3

- [ ] T025 [P] [US3] Extend labeled citation contract fields in `backend/src/backend.Application/Services/RagService/DTO/RagCitationDto.cs`, `backend/src/backend.Application/Services/RagService/DTO/RagAnswerResult.cs`, and `specs/feat/021-intent-aware-rag/contracts/qa-ask.md`
- [ ] T026 [P] [US3] Derive authority-type and primary-vs-supporting source roles in `backend/src/backend.Application/Services/RagService/RagDocumentProfileBuilder.cs`, `backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs`, and `backend/src/backend.Application/Services/RagService/RagSourceMetadata.cs`
- [ ] T027 [US3] Return source-role-aware citation sets in `backend/src/backend.Application/Services/RagService/RagAppService.cs`
- [ ] T028 [P] [US3] Propagate source-role fields through `frontend/src/services/qa.service.ts`, `frontend/src/hooks/useChat.ts`, `frontend/src/providers/chat-provider/actions.tsx`, `frontend/src/providers/chat-provider/context.tsx`, `frontend/src/providers/chat-provider/reducer.tsx`, and `frontend/src/providers/chat-provider/index.tsx`
- [ ] T029 [P] [US3] Render binding-law versus guidance indicators in `frontend/src/components/chat/ChatMessage.tsx` and `frontend/src/components/chat/CitationList.tsx`
- [ ] T030 [P] [US3] Update localized source-role copy in `frontend/src/messages/en.json`, `frontend/src/messages/zu.json`, `frontend/src/messages/st.json`, and `frontend/src/messages/af.json`

**Checkpoint**: User Story 3 should clearly label binding law and guidance whenever both appear in the same answer.

---

## Phase 6: User Story 4 - The system becomes more cautious or escalates when certainty or stakes are high (Priority: P4)

**Goal**: Route ambiguous, unsupported, or urgent prompts into clarification, limited-answer, or escalation-safe outcomes without losing multilingual behavior or question-history traceability.

**Independent Test**: Submit ambiguous, unsupported, and urgent prompts across supported languages and verify the system asks for clarification, limits the answer, or escalates instead of presenting a definitive uncited legal conclusion.

### Verification for User Story 4

- [ ] T031 [P] [US4] Add ambiguity, urgency, and unsupported-topic confidence cases in `backend/test/backend.Tests/RagServiceTests/RagConfidenceEvaluatorTests.cs` and `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`
- [X] T032 [P] [US4] Add clarification and insufficiency language-routing prompt cases in `backend/test/backend.Tests/RagServiceTests/RagPromptBuilderTests.cs`

### Implementation for User Story 4

- [ ] T033 [P] [US4] Recalibrate ambiguity and risk-trigger thresholds in `backend/src/backend.Application/Services/RagService/RagConfidenceEvaluator.cs`
- [ ] T034 [P] [US4] Implement localized clarification and insufficiency prompt behavior in `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs`
- [X] T035 [US4] Decouple question persistence from grounded-answer persistence in `backend/src/backend.Application/Services/RagService/RagAppService.cs` and `backend/src/backend.Core/Domains/QA/Question.cs`
- [ ] T036 [P] [US4] Propagate clarification, insufficiency, and urgent-attention state through `frontend/src/services/qa.service.ts`, `frontend/src/hooks/useChat.ts`, `frontend/src/providers/chat-provider/actions.tsx`, `frontend/src/providers/chat-provider/context.tsx`, `frontend/src/providers/chat-provider/reducer.tsx`, and `frontend/src/providers/chat-provider/index.tsx`
- [ ] T037 [P] [US4] Render accessible clarification and escalation states in `frontend/src/components/chat/ChatMessage.tsx`, `frontend/src/components/chat/ChatThread.tsx`, and `frontend/src/components/chat/AskExperience.tsx`
- [ ] T038 [P] [US4] Update localized clarification and escalation copy in `frontend/src/messages/en.json`, `frontend/src/messages/zu.json`, `frontend/src/messages/st.json`, and `frontend/src/messages/af.json`

**Checkpoint**: User Story 4 should safely downgrade weak or urgent multilingual prompts and preserve question history without persisting non-grounded answers.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Run the full verification loop, reconcile deferred corpus work, and confirm the documented multilingual behavior matches the shipped implementation.

- [ ] T039 [P] Run the backend regression suite in `backend/test/backend.Tests/LanguageServiceTests/LanguageAppServiceTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagPromptBuilderTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagConfidenceEvaluatorTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagDocumentProfileBuilderTests.cs`, `backend/test/backend.Tests/RagServiceTests/RagQueryFocusBuilderTests.cs`, and `backend/test/backend.Tests/RagServiceTests/RagIndexStoreTests.cs`
- [X] T040 [P] Run frontend lint and type-safety validation for `frontend/src/services/qa.service.ts`, `frontend/src/hooks/useChat.ts`, `frontend/src/providers/chat-provider/actions.tsx`, `frontend/src/providers/chat-provider/context.tsx`, `frontend/src/providers/chat-provider/reducer.tsx`, `frontend/src/providers/chat-provider/index.tsx`, `frontend/src/components/chat/ChatMessage.tsx`, `frontend/src/components/chat/CitationList.tsx`, `frontend/src/components/chat/ChatThread.tsx`, and `frontend/src/components/chat/AskExperience.tsx`
- [ ] T041 Validate the multilingual benchmark matrix and persistence checks in `specs/feat/021-intent-aware-rag/quickstart.md`, `specs/feat/021-intent-aware-rag/spec.md`, and `specs/feat/021-intent-aware-rag/plan.md`
- [ ] T042 [P] Reconcile deferred corpus-bundle and licensing follow-on notes in `specs/feat/021-intent-aware-rag/research.md`, `specs/feat/021-intent-aware-rag/quickstart.md`, and `specs/feat/021-intent-aware-rag/plan.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1: Setup** -> no dependencies and can start immediately.
- **Phase 2: Foundational** -> depends on Phase 1 and blocks all user-story work.
- **Phase 3: US1** -> depends on Phase 2 and delivers the MVP.
- **Phase 4: US2** -> depends on Phase 3 because paraphrase stability builds on the multilingual routing and retrieval baseline proven in US1.
- **Phase 5: US3** -> depends on Phase 2 and Phase 3 because source-role labeling builds on the stabilized retrieval and multilingual answer path.
- **Phase 6: US4** -> depends on Phase 2 and Phase 3; frontend state handling also benefits from the structured contract and source-role propagation from Phase 5.
- **Phase 7: Polish** -> depends on all implemented user stories.

### User Story Dependencies

- **US1 (P1)** -> first deliverable and recommended MVP scope.
- **US2 (P2)** -> builds on US1 to keep multilingual source selection stable across paraphrases and wrong hints.
- **US3 (P3)** -> builds on the foundational source-classification baseline and the grounded answer path from US1.
- **US4 (P4)** -> builds on the multilingual routing baseline from US1 and the structured response contract; source-role labeling from US3 improves the frontend safety states.

### Within Each User Story

- Verification tasks for the story should be completed first and used to prove the intended behavior before implementation is finalized.
- Backend retrieval, prompt, and orchestration changes should land before frontend consumption work for the same story.
- Each story checkpoint should pass before moving to the next priority slice.

---

## Parallel Opportunities

- T002 and T003 can run in parallel because they touch different documentation and test files.
- T004-T009 contain several parallelizable foundational slices because language service, source metadata, and focus-query baselines touch separate files.
- T010 and T011 can run in parallel for US1.
- T012 and T013 can run in parallel for US1 before T014 integrates the end-to-end ask flow.
- T015-T017 can run in parallel for US1 after T014 stabilizes the backend contract.
- T018 and T019 can run in parallel for US2.
- T020 and T021 can run in parallel for US2 before T022 integrates the final ranking behavior.
- T023 and T024 can run in parallel for US3.
- T025, T026, and T028-T030 can run in parallel for US3 once T027 is ready to integrate the backend contract.
- T031 and T032 can run in parallel for US4.
- T033 and T034 can run in parallel for US4 before T035 integrates persistence behavior.
- T036-T038 can run in parallel for US4 after T035 stabilizes the backend response flow.
- T039 and T040 can run in parallel during final validation.

---

## Parallel Example: User Story 1

```bash
# Verification tasks that can run in parallel for US1
Task: "Add multilingual plain-language retrieval and source-selection cases in backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs and backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs"
Task: "Add language-directive and English-citation prompt coverage in backend/test/backend.Tests/RagServiceTests/RagPromptBuilderTests.cs"

# Backend implementation tasks that can run in parallel for US1
Task: "Reweight translated-question retrieval scoring for multilingual plain-language discovery in backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs"
Task: "Implement same-language answer directives with English citation invariants in backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs"
```

## Parallel Example: User Story 4

```bash
# Backend safety tasks that can run in parallel for US4
Task: "Recalibrate ambiguity and risk-trigger thresholds in backend/src/backend.Application/Services/RagService/RagConfidenceEvaluator.cs"
Task: "Implement localized clarification and insufficiency prompt behavior in backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs"

# Frontend state tasks that can run in parallel for US4 after backend behavior is stable
Task: "Propagate clarification, insufficiency, and urgent-attention state through frontend/src/services/qa.service.ts, frontend/src/hooks/useChat.ts, frontend/src/providers/chat-provider/actions.tsx, frontend/src/providers/chat-provider/context.tsx, frontend/src/providers/chat-provider/reducer.tsx, and frontend/src/providers/chat-provider/index.tsx"
Task: "Render accessible clarification and escalation states in frontend/src/components/chat/ChatMessage.tsx, frontend/src/components/chat/ChatThread.tsx, and frontend/src/components/chat/AskExperience.tsx"
Task: "Update localized clarification and escalation copy in frontend/src/messages/en.json, frontend/src/messages/zu.json, frontend/src/messages/st.json, and frontend/src/messages/af.json"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3.
3. Run the US1 multilingual retrieval and prompt tests.
4. Validate the US1 benchmark prompts from `quickstart.md`.
5. Stop and review the MVP before expanding scope.

### Incremental Delivery

1. Deliver US1 to prove multilingual plain-language retrieval and same-language answers work.
2. Add US2 to stabilize source selection across paraphrases, colloquial wording, and wrong hints.
3. Add US3 to expose law-vs-guidance source roles clearly.
4. Add US4 to safely downgrade weak or urgent prompts while preserving question-history traceability.

### Recommended Execution Order

1. T001-T009
2. T010-T017
3. T018-T022
4. T023-T030
5. T031-T038
6. T039-T042

---

## Notes

- All tasks follow the required checklist format with task ID, optional parallel marker, optional story label, and exact file paths.
- The prerequisite script still resolves to the wrong git branch in this workspace, so this task list was generated from the explicit feat/021 documents rather than the branch-derived spec-kit path.
- No migration tasks are included because the plan and research explicitly keep this slice schema-neutral.
- Corpus expansion remains a documented follow-on, not an implementation task inside this retrieval-hardening milestone.
