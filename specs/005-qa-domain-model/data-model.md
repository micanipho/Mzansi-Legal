# Data Model: Q&A Domain (005-qa-domain-model)

**Date**: 2026-03-28
**Branch**: `005-qa-domain-model`
**Layer**: `backend.Core` (Domain) + `backend.EntityFrameworkCore` (DbContext)

---

## Entity Relationship Diagram

```
AppUser (long Id)
  └──< Conversation (UserId FK → long)
            ├── FaqCategory? → Category (Guid, nullable FK)
            └──< Question (ConversationId FK → Guid)
                      └──< Answer (QuestionId FK → Guid)
                                └──< AnswerCitation (AnswerId FK → Guid)
                                          └── DocumentChunk (ChunkId FK → Guid, cross-aggregate)
```

---

## Enumerations

### `Language` enum

```csharp
// backend/src/backend.Core/Domains/QA/Language.cs
namespace backend.Domains.QA;

/// <summary>Supported user-facing languages for the MzansiLegal assistant.</summary>
public enum Language
{
    English = 0,
    Zulu = 1,
    Sesotho = 2,
    Afrikaans = 3
}
```

Stored as `int` in PostgreSQL. Maps to ISO codes: en=0, zu=1, st=2, af=3.

### `InputMethod` enum

```csharp
// backend/src/backend.Core/Domains/QA/InputMethod.cs
namespace backend.Domains.QA;

/// <summary>Mechanism by which a user submitted their question to the assistant.</summary>
public enum InputMethod
{
    Text = 0,
    Voice = 1
}
```

---

## Entities

### `Conversation`

**File**: `backend/src/backend.Core/Domains/QA/Conversation.cs`
**Namespace**: `backend.Domains.QA`
**Extends**: `FullAuditedEntity<Guid>`

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| `UserId` | `long` | Required, FK → `AbpUsers.Id` | Matches ABP Zero User PK type |
| `Language` | `Language` | Required | Enum — preferred language of this conversation |
| `InputMethod` | `InputMethod` | Required | Enum — primary input mode |
| `StartedAt` | `DateTime` | Required | UTC timestamp of conversation start |
| `IsPublicFaq` | `bool` | Required, default false | Marks conversation as a curated public FAQ |
| `FaqCategoryId` | `Guid?` | Nullable, FK → `Categories.Id` | Restrict-delete |
| `FaqCategory` | `Category?` | Navigation | Nullable — only set when `IsPublicFaq=true` |
| `Questions` | `ICollection<Question>` | Navigation | Owned child collection |

**Indexes**: `(UserId)`, `(IsPublicFaq, FaqCategoryId)` for FAQ filtering.

---

### `Question`

**File**: `backend/src/backend.Core/Domains/QA/Question.cs`
**Namespace**: `backend.Domains.QA`
**Extends**: `FullAuditedEntity<Guid>`

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| `ConversationId` | `Guid` | Required, FK → `Conversations.Id` | Cascade-delete |
| `Conversation` | `Conversation` | Navigation | Parent |
| `OriginalText` | `string` | Required | The raw text as submitted by the user |
| `TranslatedText` | `string` | Required | Translation of OriginalText; may equal OriginalText |
| `Language` | `Language` | Required | Enum — language of this specific question |
| `InputMethod` | `InputMethod` | Required | Enum — how this question was submitted |
| `AudioFile` | `string?` | Nullable, MaxLength 500 | Opaque file reference for voice input |
| `Answers` | `ICollection<Answer>` | Navigation | Owned child collection |

**Indexes**: `(ConversationId)` for ordered question retrieval.

---

### `Answer`

**File**: `backend/src/backend.Core/Domains/QA/Answer.cs`
**Namespace**: `backend.Domains.QA`
**Extends**: `FullAuditedEntity<Guid>`

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| `QuestionId` | `Guid` | Required, FK → `Questions.Id` | Cascade-delete |
| `Question` | `Question` | Navigation | Parent |
| `Text` | `string` | Required | Full text of the AI-generated answer |
| `Language` | `Language` | Required | Enum — language of the answer |
| `AudioFile` | `string?` | Nullable, MaxLength 500 | Opaque file reference for TTS output |
| `IsAccurate` | `bool?` | Nullable | Admin review flag; null = unreviewed |
| `AdminNotes` | `string?` | Nullable | Free-text admin commentary |
| `Citations` | `ICollection<AnswerCitation>` | Navigation | Owned child collection |

**Indexes**: `(QuestionId)`.

---

### `AnswerCitation`

**File**: `backend/src/backend.Core/Domains/QA/AnswerCitation.cs`
**Namespace**: `backend.Domains.QA`
**Extends**: `FullAuditedEntity<Guid>`

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| `AnswerId` | `Guid` | Required, FK → `Answers.Id` | Cascade-delete (citation owned by answer) |
| `Answer` | `Answer` | Navigation | Parent |
| `ChunkId` | `Guid` | Required, FK → `DocumentChunks.Id` | Restrict-delete (cross-aggregate) |
| `Chunk` | `DocumentChunk` | Navigation | Cross-aggregate reference |
| `SectionNumber` | `string` | Required, MaxLength 100 | e.g. "§ 12(3)" |
| `Excerpt` | `string` | Required | Relevant text fragment from the chunk |
| `RelevanceScore` | `decimal` | Required | Cosine similarity score [0.0–1.0] |

**Indexes**: `(AnswerId)`, `(ChunkId)` for reverse-lookup.

---

## EF Core Configuration (Fluent API)

All configuration goes into `backendDbContext.OnModelCreating` via private static helper methods, following the existing pattern.

### Conversation
```csharp
// FK to User (long) — restrict delete (cannot delete a user while conversations exist)
modelBuilder.Entity<Conversation>()
    .HasOne<User>()
    .WithMany()
    .HasForeignKey(c => c.UserId)
    .OnDelete(DeleteBehavior.Restrict);

// FK to Category (nullable) — restrict delete
modelBuilder.Entity<Conversation>()
    .HasOne(c => c.FaqCategory)
    .WithMany()
    .HasForeignKey(c => c.FaqCategoryId)
    .IsRequired(false)
    .OnDelete(DeleteBehavior.Restrict);

// Indexes
modelBuilder.Entity<Conversation>()
    .HasIndex(c => c.UserId);
modelBuilder.Entity<Conversation>()
    .HasIndex(c => new { c.IsPublicFaq, c.FaqCategoryId });
```

### Question
```csharp
// FK to Conversation — cascade delete
modelBuilder.Entity<Question>()
    .HasOne(q => q.Conversation)
    .WithMany(c => c.Questions)
    .HasForeignKey(q => q.ConversationId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<Question>()
    .HasIndex(q => q.ConversationId);
```

### Answer
```csharp
// FK to Question — cascade delete
modelBuilder.Entity<Answer>()
    .HasOne(a => a.Question)
    .WithMany(q => q.Answers)
    .HasForeignKey(a => a.QuestionId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<Answer>()
    .HasIndex(a => a.QuestionId);
```

### AnswerCitation
```csharp
// FK to Answer — cascade delete
modelBuilder.Entity<AnswerCitation>()
    .HasOne(ac => ac.Answer)
    .WithMany(a => a.Citations)
    .HasForeignKey(ac => ac.AnswerId)
    .OnDelete(DeleteBehavior.Cascade);

// FK to DocumentChunk — restrict delete (cross-aggregate reference)
modelBuilder.Entity<AnswerCitation>()
    .HasOne(ac => ac.Chunk)
    .WithMany()
    .HasForeignKey(ac => ac.ChunkId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<AnswerCitation>()
    .HasIndex(ac => ac.AnswerId);
modelBuilder.Entity<AnswerCitation>()
    .HasIndex(ac => ac.ChunkId);
```

---

## DbSet Registrations (backendDbContext additions)

```csharp
// ── Q&A domain ──────────────────────────────────────────────────────────────
public DbSet<Conversation> Conversations { get; set; }
public DbSet<Question> Questions { get; set; }
public DbSet<Answer> Answers { get; set; }
public DbSet<AnswerCitation> AnswerCitations { get; set; }
```

---

## Delete Behaviour Summary

| Relationship | Delete Behaviour | Rationale |
|---|---|---|
| Conversation → User | Restrict | Cannot delete users who have conversation history |
| Conversation → Category | Restrict | Category deletion blocked while FAQs reference it |
| Question → Conversation | Cascade | Questions are owned by their conversation |
| Answer → Question | Cascade | Answers are owned by their question |
| AnswerCitation → Answer | Cascade | Citations are owned by their answer |
| AnswerCitation → DocumentChunk | Restrict | Cross-aggregate: chunk deletion blocked while cited |

---

## Namespace Summary

All new entities live in `backend.Domains.QA` namespace, matching the physical folder `backend/src/backend.Core/Domains/QA/`.
