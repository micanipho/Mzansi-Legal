# Tasks: Persist Q&A Interaction Records (feat/023)

**Branch**: `feat/023-persist-qa-records`
**Input**: Design documents from `specs/feat/023-persist-qa-records/`
**Prerequisites**: plan.md ✅ | spec.md ✅ | research.md ✅ | data-model.md ✅ | quickstart.md ✅

**Tests**: Verification tasks cover every behavior change. No new entities or migrations needed.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm the development environment is ready and all existing infrastructure is in place.
No new project structure is required for this feature.

- [ ] T001 Verify existing build passes: run `dotnet build backend/backend.sln` and confirm zero errors
- [ ] T002 [P] Verify existing tests pass: run `dotnet test backend/backend.sln --filter "RagServiceTests"` and record baseline pass/fail count

**Checkpoint**: Build is green, existing test suite passes — implementation can begin.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Update the `TestableRagAppService` spy infrastructure to track the new `conversationId`
parameter before any story test or implementation work begins. All story verification tasks depend on this.

- [X] T003 Update `TestableRagAppService.PersistQuestionAsync` override in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs` to accept the new `Guid? conversationId` parameter and expose it as a `public Guid? LastConversationId { get; private set; }` property
- [X] T004 Update `TestableRagAppService.PersistAnswerAsync` — no signature change needed; confirm it still compiles after T003

**Checkpoint**: Test spy updated — all story verification tasks can now be written against `LastConversationId`.

---

## Phase 3: User Story 1 — Full Interaction Record Saved on Ask (Priority: P1) 🎯 MVP

**Goal**: After any authenticated user asks a question, a complete chain of Conversation → Question → Answer → AnswerCitation records is written to the database with correct parent-child links.

**Independent Test**: Call `POST /api/services/app/rag/ask` as an authenticated user, then inspect the database to confirm all four record types exist and are linked; or run the unit tests from T005–T006.

### Verification — User Story 1 ⚠️

- [X] T005 [P] [US1] Add unit test `AskAsync_AuthenticatedGroundedAnswer_AnswerAndCitationsArePersisted` in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs` — asserts `PersistedQuestionCount == 1`, `PersistedAnswerCount == 1`, `LastPersistedChunkIds.Count > 0`, and `result.AnswerId != null`
- [X] T006 [P] [US1] Add unit test `AskAsync_AnonymousUser_NoPersistenceOccurs` in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs` — asserts `PersistedQuestionCount == 0` and `PersistedAnswerCount == 0` when `AbpSession.UserId` is null

### Implementation — User Story 1

- [X] T007 [US1] Add `Guid? ConversationId { get; set; }` property with XML doc comment to `backend/src/backend.Application/Services/RagService/DTO/AskQuestionRequest.cs`
- [X] T008 [US1] Add `Guid? ConversationId { get; set; }` property with XML doc comment to `backend/src/backend.Application/Services/RagService/DTO/RagAnswerResult.cs` so the frontend can persist the ID for multi-turn use
- [X] T009 [US1] Extract `CreateConversationAsync(long userId, Language language)` private helper method from the existing `PersistQuestionAsync` body in `backend/src/backend.Application/Services/RagService/RagAppService.cs`
- [X] T010 [US1] Update `PersistQuestionAsync` signature in `backend/src/backend.Application/Services/RagService/RagAppService.cs` to accept `Guid? conversationId` as the second parameter (after `long userId`)
- [X] T011 [US1] Add conversation-reuse logic inside the updated `PersistQuestionAsync`: look up existing `Conversation` by `conversationId && UserId`; fall back to `CreateConversationAsync` if not found or null — in `backend/src/backend.Application/Services/RagService/RagAppService.cs`
- [X] T012 [US1] Update `PersistQuestionIfAuthenticatedAsync` to accept and thread through `Guid? conversationId` in `backend/src/backend.Application/Services/RagService/RagAppService.cs`
- [X] T013 [US1] Update all three call sites of `PersistQuestionIfAuthenticatedAsync` / `PersistQuestionAsync` inside `AskAsync` to pass `request.ConversationId` in `backend/src/backend.Application/Services/RagService/RagAppService.cs`
- [X] T014 [US1] Populate `result.ConversationId` in the grounded-answer return block of `AskAsync` (use the `conversationId` resolved inside `PersistQuestionAsync` — return it as part of the result) in `backend/src/backend.Application/Services/RagService/RagAppService.cs`
- [ ] T015 [US1] Run `dotnet build backend/backend.sln` and fix any compilation errors introduced by the signature changes

**Checkpoint**: All US1 tests (T005, T006) pass; existing tests still pass; a real grounded question persists all four record types end-to-end.

---

## Phase 4: User Story 2 — Continuing an Existing Conversation (Priority: P2)

**Goal**: A follow-up question submitted with a valid `ConversationId` is linked to the existing `Conversation` record rather than creating a new one. An invalid or foreign `ConversationId` silently falls back to a new conversation.

**Independent Test**: Run the two new unit tests T016–T017; or POST two questions with the same `ConversationId` and verify `GetConversationsAsync` returns one conversation with two questions.

### Verification — User Story 2 ⚠️

- [X] T016 [P] [US2] Add unit test `AskAsync_WithValidConversationId_ReusesThatConversation` in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs` — asserts `LastConversationId == suppliedId` when the ID belongs to the current user
- [X] T017 [P] [US2] Add unit test `AskAsync_WithConversationIdBelongingToAnotherUser_CreatesNewConversation` in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs` — asserts a new `Conversation` is created (ownership check enforced)
- [X] T018 [P] [US2] Add unit test `AskAsync_WithNullConversationId_CreatesNewConversation` in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs` — asserts baseline behaviour still works when no ID is supplied

### Implementation — User Story 2

No additional implementation code is required beyond what was written in Phase 3 (T011 already contains the conversation lookup + ownership check + fallback logic). Phase 4 is purely verification.

- [X] T019 [US2] Run full test suite `dotnet test backend/backend.sln --filter "RagServiceTests"` and confirm T016, T017, T018 all pass alongside existing tests

**Checkpoint**: Conversation reuse is verified working; ownership guard confirmed; multi-turn architecture is functional.

---

## Phase 5: User Story 3 — Admin Analytics Access (Priority: P3)

**Goal**: Stored interaction records are queryable by date range and language. No new code is required since the schema, indexes, and `GetConversationsAsync` already exist. Verification confirms query correctness.

**Independent Test**: After seeding multiple Q&A interactions, call `GetConversationsAsync` and confirm filtering by `UserId` and ordering by `StartedAt` returns correct results.

### Verification — User Story 3 ⚠️

- [X] T020 [US3] Add unit test `GetConversationsAsync_ReturnsOnlyCurrentUserConversations_OrderedByStartedAtDescending` in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs` — seeds two conversations for different users, asserts only the current user's appear, ordered newest-first
- [X] T021 [US3] Add unit test `GetConversationsAsync_IncludesFirstQuestionAndCount` in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs` — asserts `ConversationSummaryDto.FirstQuestion` and `QuestionCount` are populated correctly

### Implementation — User Story 3

No new implementation code. All analytics queries are satisfied by the existing `GetConversationsAsync` method and database indexes.

- [X] T022 [US3] Run `dotnet test backend/backend.sln --filter "RagServiceTests"` and confirm T020, T021 pass

**Checkpoint**: All three user stories verified; entire test suite is green.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [X] T023 [P] Update XML doc comment on `IRagAppService.AskAsync` in `backend/src/backend.Application/Services/RagService/IRagAppService.cs` to document the new optional `ConversationId` parameter on the request
- [X] T024 [P] Confirm `GetConversationsAsync` controller action in `backend/src/backend.Web.Host/Controllers/QuestionController.cs` is publicly documented and returns the conversation ID so clients can send it on follow-up questions
- [ ] T025 Run quickstart manual verification steps from `specs/feat/023-persist-qa-records/quickstart.md` against a running local backend to confirm end-to-end behaviour
- [ ] T026 [P] Run `dotnet test backend/backend.sln` (full suite) and confirm zero regressions across all test projects
- [ ] T027 Commit all changes on branch `feat/023-persist-qa-records` following the `follow-git-workflow` skill conventions

---

## Dependencies & Execution Order

### Phase Dependencies

```
Phase 1 (Setup)        → no dependencies → start immediately
Phase 2 (Foundational) → depends on Phase 1 → BLOCKS all story phases
Phase 3 (US1 / P1)    → depends on Phase 2
Phase 4 (US2 / P2)    → depends on Phase 3 (reuses conversation logic from T011)
Phase 5 (US3 / P3)    → depends on Phase 2 (schema already exists; independent of US1/US2 logic)
Phase 6 (Polish)       → depends on Phases 3–5 all passing
```

### Within Each Phase

```
T003 → T004                             (foundational spy update)
T005, T006 (parallel) → T007–T014 → T015   (US1: tests first, then implement, then build)
T016, T017, T018 (parallel) → T019        (US2: tests first, all parallel, then run suite)
T020, T021 (parallel) → T022             (US3: tests first, then run suite)
```

### Parallel Opportunities

- T001 and T002 can run in parallel (different commands)
- T005 and T006 can be written in parallel (different test methods)
- T007 and T008 can be written in parallel (different DTOs)
- T016, T017, T018 can be written in parallel (different test methods)
- T020, T021 can be written in parallel (different test methods)
- T023, T024, T026 can run in parallel (different files)

---

## Parallel Example: User Story 1

```bash
# Step 1 – write verification tests in parallel (T005, T006):
Agent A: AskAsync_AuthenticatedGroundedAnswer_AnswerAndCitationsArePersisted
Agent B: AskAsync_AnonymousUser_NoPersistenceOccurs

# Step 2 – implement DTO changes in parallel (T007, T008):
Agent A: AskQuestionRequest.cs — add ConversationId property
Agent B: RagAnswerResult.cs — add ConversationId property

# Step 3 – implement service changes sequentially (T009→T010→T011→T012→T013→T014):
Agent A: RagAppService.cs — all changes sequential (same file)

# Step 4 – build check (T015):
dotnet build backend/backend.sln
```

---

## Implementation Strategy

### MVP: User Story 1 Only (Phases 1–3)

1. Complete Phase 1: Setup — confirm green build
2. Complete Phase 2: Foundational — update `TestableRagAppService`
3. Complete Phase 3: User Story 1 — write tests, implement DTO + service changes, run build
4. **STOP and VALIDATE**: `dotnet test --filter "RagServiceTests"` all green; all four record types confirmed in database
5. Deploy / demo if ready

### Incremental Delivery

- Phase 3 complete → persistence works (MVP ✅)
- Phase 4 complete → multi-turn conversations work
- Phase 5 complete → admin analytics confirmed queryable
- Phase 6 complete → documented, committed, regression-free

---

## Notes

- No new migrations needed — all entities and indexes already exist
- [P] = different files, safe to parallelize
- [US1/US2/US3] maps tasks to spec.md user stories for traceability
- Commit after each logical group (foundation, US1, US2, US3)
- Stop at each Checkpoint to validate independence before proceeding
