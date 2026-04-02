# Research: Persist Q&A Interaction Records (feat/023)

**Branch**: `feat/023-persist-qa-records`
**Date**: 2026-04-02
**Phase**: Phase 0 — Research

---

## 1. Existing Entity State

**Decision**: All four required entities (`Conversation`, `Question`, `Answer`, `AnswerCitation`) already
exist, are correctly placed in `backend.Core/Domains/QA/`, all extend `FullAuditedEntity<Guid>`, and
have correct `[Required]` / `[MaxLength]` annotations.

**Rationale**: No new entity definitions are needed. The schema work is done and migrations already exist.

**Alternatives considered**: Creating new lightweight read-model entities for analytics. Rejected —
premature optimisation; the source tables have the required indexes for direct query.

---

## 2. DbContext Registration

**Decision**: All four entities are already registered in `backendDbContext` as `DbSet<T>` properties
and have full Fluent API FK configuration in `ConfigureQARelationships` — including cascade/restrict
rules, and all required analytics indexes (`ConversationId`, `UserId`, `IsPublicFaq/FaqCategoryId`,
`QuestionId`, `AnswerId`, `ChunkId`).

**Rationale**: No DbContext changes or new migrations are required.

**Alternatives considered**: Adding composite analytics indexes. Deferred — not required for MVP queries.

---

## 3. Current Persistence Implementation

**Decision**: `RagAppService.PersistQuestionAsync` and `PersistAnswerAsync` are already implemented
and tested. Questions and answers are saved for authenticated users after grounded answers.

**Critical gap found**: `PersistQuestionAsync` creates a **new `Conversation` on every call**,
regardless of whether the caller is continuing an existing session. `AskQuestionRequest` does not
contain a `ConversationId` field.

**Rationale**: The feature spec requires conversation reuse (FR-002, User Story 2). This gap must be
closed: `AskQuestionRequest` needs an optional `ConversationId?` field, and `PersistQuestionAsync`
must look up the existing `Conversation` when a valid ID is supplied.

**Alternatives considered**:

- Session-cookie-based conversation tracking (server-side state). Rejected — ABP's repository pattern
  is stateless; passing the ID explicitly in the request is simpler and testable.
- Always creating a new conversation per request. Rejected — violates FR-002 and makes history views
  unusable for multi-turn exchanges.

---

## 4. Test Infrastructure

**Decision**: Extend the existing `TestableRagAppService` spy class in `RagAppServiceTests.cs` with
overrides for the updated `PersistQuestionAsync` signature that accepts an optional `Guid? conversationId`.
New test cases follow the same `protected virtual` override pattern already in place.

**Rationale**: Keeps tests isolated from real repositories and EF, which is the pattern already
established in the test file. No new test infrastructure is required.

---

## 5. POPIA / Data Governance

**Decision**: Personal data stored in `Question.OriginalText` and `Conversation.UserId` must be
addressable for deletion. The `FullAuditedEntity` base class provides soft-delete on `Conversation`
(cascade to `Question` → `Answer` → `AnswerCitation`). A hard-delete pathway for POPIA subject
requests is out of scope for this milestone but the cascade setup supports it.

**Data purpose**: Storing Q&A data to enable user conversation history, admin accuracy review, and
public FAQ curation — all documented purposes.

**Retention**: No automated deletion policy for MVP; records are retained indefinitely unless manually
deleted by an admin or via a future POPIA erasure workflow.

**Cross-border / vendor**: Q&A text is sent to OpenAI for answer generation (existing, pre-existing
data flow). No new vendor data transfers are introduced by this feature.

**Breach response**: Follows project-wide incident response; no feature-specific change required.

**Rationale**: The soft-delete cascade is already in place; full POPIA erasure tooling is a separate
future milestone.

---

## 6. Multi-turn Conversation Support Approach

**Decision**: Add `ConversationId? ConversationId` to `AskQuestionRequest`. Modify
`PersistQuestionAsync` to accept `Guid? conversationId`. If a valid ID is provided and belongs to
the current `userId`, reuse that `Conversation`. If the ID is null, missing, or does not belong to
the user, create a new `Conversation` as before.

**Rationale**: Minimal change to the call signature; safe for anonymous users (who always get a null
conversation ID and are not persisted); backward-compatible with existing tests.

---

## 7. Unresolved Questions

None — all NEEDS CLARIFICATION markers resolved by codebase research.
