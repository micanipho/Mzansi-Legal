# Quickstart: Persist Q&A Interaction Records (feat/023)

**Branch**: `feat/023-persist-qa-records`
**Date**: 2026-04-02

---

## What This Feature Changes

Two small, targeted changes in the Application layer. No new migrations, no new entities.

### 1. `AskQuestionRequest` — Add optional `ConversationId`

In `backend.Application/Services/RagService/DTO/AskQuestionRequest.cs`, add:

```csharp
/// <summary>
/// Optional ID of an existing Conversation to continue.
/// When null, a new Conversation is created for this question.
/// When provided, the service verifies it belongs to the current user before reusing it.
/// </summary>
public Guid? ConversationId { get; set; }
```

### 2. `RagAppService.PersistQuestionAsync` — Support Conversation Reuse

In `backend.Application/Services/RagService/RagAppService.cs`:

**a.** Update the `PersistQuestionAsync` signature:

```csharp
// Before
protected virtual async Task<Guid> PersistQuestionAsync(
    long userId, string originalText, string translatedText, Language language)

// After
protected virtual async Task<Guid> PersistQuestionAsync(
    long userId, Guid? conversationId, string originalText, string translatedText, Language language)
```

**b.** Extract conversation creation into a private helper `CreateConversationAsync(long userId, Language language)`

**c.** Inside `PersistQuestionAsync`, look up the existing conversation before creating a new one:

```csharp
Guid resolvedConversationId;

if (conversationId.HasValue)
{
    var existing = await _conversationRepository.FirstOrDefaultAsync(
        c => c.Id == conversationId.Value && c.UserId == userId);

    resolvedConversationId = existing?.Id ?? await CreateConversationAsync(userId, language);
}
else
{
    resolvedConversationId = await CreateConversationAsync(userId, language);
}
```

**d.** Update all **three** call sites in `AskAsync` to pass `request.ConversationId`:

```csharp
// Insufficient path
await PersistQuestionIfAuthenticatedAsync(userId, request.ConversationId, ...);

// Clarification path
await PersistQuestionIfAuthenticatedAsync(userId, request.ConversationId, ...);

// Grounded path
var questionId = await PersistQuestionAsync(userId.Value, request.ConversationId, ...);
```

Also update `PersistQuestionIfAuthenticatedAsync` to accept and thread through `Guid? conversationId`.

---

## How to Test Locally

1. Ensure the backend builds: `dotnet build backend/backend.sln`
2. Run unit tests: `dotnet test backend/backend.sln --filter "RagServiceTests"`
3. Start the server: `dotnet run --project backend/src/backend.Web.Host`
4. POST `/api/services/app/rag/ask` with body:
   ```json
   { "questionText": "Can my landlord evict me without a court order?" }
   ```
5. Copy the returned `conversationId` (add to `RagAnswerResult` if not already there)
6. POST again with the same `conversationId` to confirm conversation reuse:
   ```json
   { "questionText": "What is the procedure for the court order?", "conversationId": "<id from step 5>" }
   ```
7. Verify both questions appear under the same conversation via `GET /api/services/app/rag/conversations`

---

## Test Cases to Add

All tests go in `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`
using the existing `TestableRagAppService` pattern:

| Test | Expectation |
|------|-------------|
| `AskAsync_WithExistingConversationId_ReusesPersistQuestionCall` | `PersistQuestionAsync` called with the supplied `ConversationId` |
| `AskAsync_WithConversationIdBelongingToAnotherUser_CreatesNewConversation` | Ownership check forces new conversation |
| `AskAsync_InsufficientMode_PassesConversationIdThrough` | Insufficient path also threads `ConversationId` |

The `TestableRagAppService` override for `PersistQuestionAsync` must be updated to accept the new
`conversationId` parameter and expose it as `LastConversationId { get; private set; }`.

---

## No Migration Required

All entities, `DbSet<T>` registrations, FK configurations, and indexes are already in place.
Run `dotnet ef migrations list` to confirm — no pending migrations should appear after this change.
