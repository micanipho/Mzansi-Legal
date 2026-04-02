# Implementation Plan: Persist Q&A Interaction Records

**Branch**: `feat/023-persist-qa-records` | **Date**: 2026-04-02 | **Spec**: [spec.md](../feat-023-persist-qa-records/spec.md)
**Input**: Feature specification from `/specs/feat-023-persist-qa-records/spec.md`

---

## Summary

Every Q&A interaction must be stored so that conversation history, admin analytics, and FAQ curation
can use the data. All four required entities (`Conversation`, `Question`, `Answer`, `AnswerCitation`)
already exist with full DbContext registration and database indexes. The `RagAppService` already
persists questions and answers for authenticated users after grounded answers.

**The single implementation gap**: `PersistQuestionAsync` always creates a new `Conversation` per
call. To support multi-turn conversations, `AskQuestionRequest` needs an optional `ConversationId?`
field, and `PersistQuestionAsync` must look up an existing conversation for that user before creating
a new one. This is the only code change required; no new migrations, no new entities.

---

## Technical Context

**Language/Version**: C# 12 / .NET 9
**Primary Dependencies**: ABP Framework (IRepository, ApplicationService, UnitOfWorkManager), EF Core 9, NSubstitute (tests), Shouldly (tests)
**Storage**: PostgreSQL; all Q&A tables already exist and are migrated
**Testing**: xUnit; `TestableRagAppService` spy class pattern already established
**Target Platform**: Linux / Azure server
**Performance Goals**: Persistence must not add more than 50 ms to the synchronous P95 answer latency
**Constraints**: Persistence failure must not surface to the user; silently log and continue
**Scale/Scope**: MVP — expected < 10,000 conversations / month; existing indexes are sufficient
**Legal/Compliance Inputs**: POPIA — `Question.OriginalText` and `Conversation.UserId` contain personal data; soft-delete cascade already in place; full erasure tooling is out of scope for this milestone

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Layer Gate**: All new entity code is already in `backend.Core`; service changes are in `backend.Application`; no layer violations introduced
- [x] **Naming Gate**: No new services, DTOs, or controllers. `AskQuestionRequest` is an existing DTO; property addition follows existing naming conventions
- [x] **Coding Standards Gate**: `PersistQuestionAsync` is < 30 lines; will stay within limits. Guard clauses via `Ardalis.GuardClauses` already present at method entry points. No magic numbers will be introduced.
- [x] **Skill Gate**: `add-endpoint` skill is not needed (no new endpoint); `speckit-implement` will be used for task execution
- [x] **Multilingual Gate**: This feature stores the detected language on every `Question` and `Answer` record — multilingual support is preserved. No new user-facing output is added.
- [x] **Authority Gate**: Feature adds storage only; it does not alter the RAG retrieval, reranking, or answer-generation pipeline. Primary-source-first behavior is unchanged.
- [x] **Citation Gate**: `AnswerCitation` records are already linked to `DocumentChunk` (ChunkId). The RAG contract is unchanged.
- [x] **Safety Gate**: No new user-facing legal flows. Existing clarification and insufficient-information paths are unchanged. Urgent attention flagging is unchanged.
- [x] **Accessibility Gate**: No new frontend components; backend-only change.
- [x] **Data Governance Gate**: POPIA documented — storing `OriginalText` (personal data) with stated purpose (history/analytics/FAQ), soft-delete pathway exists, full erasure tooling deferred to a future milestone.
- [x] **Corpus Governance Gate**: No document or ingestion changes.
- [x] **ETL/Ingestion Gate**: No ingestion pipeline changes.

**All 12 gates pass. ✅**

---

## Project Structure

### Documentation (this feature)

```text
specs/feat/023-persist-qa-records/
├── plan.md          ← this file
├── research.md      ← Phase 0 output (complete)
├── data-model.md    ← Phase 1 output (complete)
├── quickstart.md    ← Phase 1 output (complete)
└── tasks.md         ← Phase 2 output (/speckit-tasks command — not created by /speckit-plan)
```

### Source Code (affected files only)

```text
backend/src/
├── backend.Application/
│   └── Services/
│       └── RagService/
│           ├── RagAppService.cs              ← PersistQuestionAsync signature change + conversation lookup
│           └── DTO/
│               └── AskQuestionRequest.cs     ← Add ConversationId? property

backend/test/
└── backend.Tests/
    └── RagServiceTests/
        └── RagAppServiceTests.cs             ← New tests for conversation reuse + anonymous user path
```

**Structure Decision**: This is a backend-only change, narrowly scoped to two existing files in the Application project and one test file. No migrations, no new entities, no frontend changes.

---

## Proposed Changes

### `AskQuestionRequest.cs` — Add optional `ConversationId`

```csharp
/// <summary>
/// Optional ID of an existing Conversation to continue.
/// When null, a new Conversation is created.
/// When provided, the service verifies it belongs to the current user before reusing it.
/// </summary>
public Guid? ConversationId { get; set; }
```

### `RagAppService.cs` — Update `PersistQuestionAsync`

```csharp
protected virtual async Task<Guid> PersistQuestionAsync(
    long userId,
    Guid? conversationId,       // new parameter
    string originalText,
    string translatedText,
    Language language)
{
    Guid resolvedConversationId;

    if (conversationId.HasValue)
    {
        // Reuse the existing conversation if it belongs to this user.
        var existing = await _conversationRepository.FirstOrDefaultAsync(
            c => c.Id == conversationId.Value && c.UserId == userId);

        resolvedConversationId = existing?.Id ?? await CreateConversationAsync(userId, language);
    }
    else
    {
        resolvedConversationId = await CreateConversationAsync(userId, language);
    }

    var question = new Question
    {
        ConversationId = resolvedConversationId,
        OriginalText = originalText,
        TranslatedText = translatedText,
        Language = language,
        InputMethod = InputMethod.Text
    };
    return await _questionRepository.InsertAndGetIdAsync(question);
}

private async Task<Guid> CreateConversationAsync(long userId, Language language)
{
    var conversation = new Conversation
    {
        UserId = userId,
        Language = language,
        InputMethod = InputMethod.Text,
        StartedAt = DateTime.UtcNow,
        IsPublicFaq = false
    };
    return await _conversationRepository.InsertAndGetIdAsync(conversation);
}
```

### `RagAppService.cs` — Pass `request.ConversationId` through `AskAsync`

Update all three call sites of `PersistQuestionAsync` inside `AskAsync` to include
`request.ConversationId` as the second argument.

### Return `ConversationId` in `RagAnswerResult` *(optional, for client continuity)*

Consider adding `Guid? ConversationId` to `RagAnswerResult` so the frontend can pass the ID back
on the next turn. Deferred to tasks.md — the spec does not require client-visible conversation ID
to fulfil the acceptance criteria, but doing so enables the multi-turn architecture.

---

## Verification Plan

### Automated Tests (new, in `RagAppServiceTests.cs`)

1. **Authenticated grounded answer creates new conversation when no ID supplied** — existing test continues to pass
2. **Authenticated grounded answer reuses conversation when valid ID supplied and user matches** — new test
3. **Authenticated grounded answer creates new conversation when supplied ID does not belong to user** — new test (security boundary)
4. **Authenticated insufficient response still persists question with correct conversation** — existing test updated for new parameter
5. **Anonymous user receives answer but no persistence occurs** — existing test continues to pass

Each test uses the `TestableRagAppService` override pattern that tracks `PersistedQuestionCount`,
`PersistedAnswerCount`, and the conversation ID used.

### Manual Verification

1. Start the backend and ask a question as an authenticated user
2. Copy the returned `AnswerId` and verify all four record types appear in the database
3. Submit a follow-up question with the returned `ConversationId` and verify the question appears in the same conversation
4. Check `GetConversationsAsync` returns a single conversation with both questions

---

## Complexity Tracking

No constitution violations. No complexity justification required.
