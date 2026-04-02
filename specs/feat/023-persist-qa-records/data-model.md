# Data Model: Persist Q&A Interaction Records (feat/023)

**Branch**: `feat/023-persist-qa-records`
**Date**: 2026-04-02
**Phase**: Phase 1 — Design

> All entities are pre-existing in `backend.Core/Domains/QA/`. This document records the
> authoritative field-level model, validation rules, and relationship constraints that the
> implementation plan and tasks must honour.

---

## Entity: Conversation

**Layer**: `backend.Core/Domains/QA/Conversation.cs`
**Base class**: `FullAuditedEntity<Guid>` (provides `Id`, `CreationTime`, `CreatorUserId`,
`LastModificationTime`, soft-delete columns)

| Field           | Type          | Required | Constraints / Notes                                                                   |
| --------------- | ------------- | -------- | ------------------------------------------------------------------------------------- |
| `UserId`        | `long`        | ✅       | FK → `AbpUsers.Id`; `DeleteBehavior.Restrict`; indexed                                |
| `Language`      | `Language`    | ✅       | RefList enum (English, Zulu, Sesotho, Afrikaans)                                      |
| `InputMethod`   | `InputMethod` | ✅       | RefList enum (Text, Voice)                                                            |
| `StartedAt`     | `DateTime`    | ✅       | UTC; set at creation                                                                  |
| `IsPublicFaq`   | `bool`        | —        | Default `false`; `true` only when admin promotes to FAQ                               |
| `FaqCategoryId` | `Guid?`       | —        | FK → `Categories.Id`; nullable; `DeleteBehavior.Restrict`; indexed with `IsPublicFaq` |

**Relationships**:

- Has many `Question` (cascade delete)
- Optionally belongs to `Category` (for FAQ classification)

**Validation rules**:

- `UserId` must always match the currently authenticated user's ID before insert
- A Conversation record must be reused (not duplicated) when a valid `ConversationId` is supplied
  in the request and that conversation belongs to the same user

---

## Entity: Question

**Layer**: `backend.Core/Domains/QA/Question.cs`
**Base class**: `FullAuditedEntity<Guid>`

| Field            | Type          | Required | Constraints / Notes                                                 |
| ---------------- | ------------- | -------- | ------------------------------------------------------------------- |
| `ConversationId` | `Guid`        | ✅       | FK → `Conversations.Id`; `DeleteBehavior.Cascade`; indexed          |
| `OriginalText`   | `string`      | ✅       | Raw user input; max 30,000 chars (enforced in DTO)                  |
| `TranslatedText` | `string`      | ✅       | English translation; equals `OriginalText` when language is English |
| `Language`       | `Language`    | ✅       | Detected language of `OriginalText`                                 |
| `InputMethod`    | `InputMethod` | ✅       | Text or Voice                                                       |
| `AudioFile`      | `string?`     | —        | Storage key/URL; max 500 chars; null for text-only questions        |

**Relationships**:

- Belongs to `Conversation` (cascade from parent)
- Has many `Answer`

**Validation rules**:

- Must be linked to a `Conversation` owned by the same `UserId`
- `TranslatedText` must be non-empty even if no translation occurred (copy of `OriginalText`)

---

## Entity: Answer

**Layer**: `backend.Core/Domains/QA/Answer.cs`
**Base class**: `FullAuditedEntity<Guid>`

| Field        | Type       | Required | Constraints / Notes                                                             |
| ------------ | ---------- | -------- | ------------------------------------------------------------------------------- |
| `QuestionId` | `Guid`     | ✅       | FK → `Questions.Id`; `DeleteBehavior.Cascade`; indexed                          |
| `Text`       | `string`   | ✅       | Full AI-generated answer text; no specific max length enforced at DB level      |
| `Language`   | `Language` | ✅       | Language of the generated answer (always matches the user's detected language)  |
| `AudioFile`  | `string?`  | —        | TTS storage key; null for text-only answers; max 500 chars                      |
| `IsAccurate` | `bool?`    | —        | Three-state admin review flag: null=unreviewed, true=accurate, false=inaccurate |
| `AdminNotes` | `string?`  | —        | Free-text admin review notes; null until reviewed                               |

**Relationships**:

- Belongs to `Question`
- Has many `AnswerCitation`

**Validation rules**:

- Only persisted when `RagAnswerMode` is `Direct` or `Cautious` (see `ShouldPersistAnswer`)
- `Language` must match the language recorded on the parent `Question`

---

## Entity: AnswerCitation

**Layer**: `backend.Core/Domains/QA/AnswerCitation.cs`
**Base class**: `FullAuditedEntity<Guid>`

| Field            | Type      | Required | Constraints / Notes                                          |
| ---------------- | --------- | -------- | ------------------------------------------------------------ |
| `AnswerId`       | `Guid`    | ✅       | FK → `Answers.Id`; `DeleteBehavior.Cascade`; indexed         |
| `ChunkId`        | `Guid`    | ✅       | FK → `DocumentChunks.Id`; `DeleteBehavior.Restrict`; indexed |
| `SectionNumber`  | `string`  | ✅       | e.g. "§ 12(3)"; max 100 chars; copied at citation time       |
| `Excerpt`        | `string`  | ✅       | Grounding text from chunk; truncated to 500 chars if longer  |
| `RelevanceScore` | `decimal` | ✅       | Cosine similarity 0.0–1.0; application layer enforces range  |

**Relationships**:

- Belongs to `Answer` (cascade from parent)
- Cross-aggregate read reference to `DocumentChunk` (restrict — cannot delete chunk while citation exists)

**Validation rules**:

- One `AnswerCitation` per `RetrievedChunk` used in the answer
- Zero citations is valid (some grounded answers may not have chunk references)
- `Excerpt` must never exceed 500 characters (truncate in application layer before insert)

---

## Relationship Diagram

```
Conversation (UserId → AbpUsers)
  └── Question (ConversationId → Conversation, cascade)
        └── Answer (QuestionId → Question, cascade)
              └── AnswerCitation (AnswerId → Answer, cascade)
                    └── [cross-agg ref] DocumentChunk (ChunkId → DocumentChunks, restrict)
```

---

## Key Change from Current Implementation

| Aspect                              | Current                  | Target                                                   |
| ----------------------------------- | ------------------------ | -------------------------------------------------------- |
| Conversation per call               | Always creates a new one | Reuses when valid `ConversationId` provided              |
| `AskQuestionRequest.ConversationId` | Not present              | `Guid? ConversationId` added                             |
| `PersistQuestionAsync` signature    | `(long userId, ...)`     | `(long userId, Guid? conversationId, ...)`               |
| Conversation ownership check        | Not applicable           | Must verify `Conversation.UserId == userId` before reuse |

---

## Indexes (existing, no changes needed)

| Table           | Index fields                 | Purpose                                |
| --------------- | ---------------------------- | -------------------------------------- |
| Conversations   | `UserId`                     | User history queries                   |
| Conversations   | `IsPublicFaq, FaqCategoryId` | Public FAQ filtering                   |
| Questions       | `ConversationId`             | Questions within a conversation        |
| Answers         | `QuestionId`                 | Answers for a question                 |
| AnswerCitations | `AnswerId`                   | Citations for an answer                |
| AnswerCitations | `ChunkId`                    | Reverse lookup: answers citing a chunk |
