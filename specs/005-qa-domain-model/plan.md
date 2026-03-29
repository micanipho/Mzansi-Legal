# Implementation Plan: Q&A Domain Model for RAG System

**Branch**: `005-qa-domain-model` | **Date**: 2026-03-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/005-qa-domain-model/spec.md`

## Summary

Implement the core Q&A persistence layer for the MzansiLegal RAG assistant: four new domain entities (`Conversation`, `Question`, `Answer`, `AnswerCitation`) plus two enumerations (`Language`, `InputMethod`). Entities follow ABP DDD conventions, are configured with EF Core Fluent API, and are persisted to PostgreSQL via a single migration. The domain model enables the full citation chain: a user's conversation → question → AI answer → legislation chunk citations.

## Technical Context

**Language/Version**: C# on .NET 9.0
**Primary Dependencies**: ABP Zero 10.x, Entity Framework Core 9.0.5, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.x, Ardalis.GuardClauses
**Storage**: PostgreSQL 15+ via Npgsql
**Testing**: xUnit via ABP test helpers (integration tests for FK enforcement; unit tests for entity construction)
**Target Platform**: Linux/Windows server (Docker-compatible)
**Project Type**: Web service (backend API layer only for this feature)
**Performance Goals**: Domain model must support at least 50 questions per conversation and 10 citations per answer without schema constraints
**Constraints**: Layer separation (Core → EFCore → Application → Web.Core → Web.Host); no EF Core references in `backend.Core`
**Scale/Scope**: Single-tenant (ABP Zero multi-tenancy infrastructure present but not the focus here)

## Constitution Check

*GATE: Must pass before implementation. Re-checked after design phase.*

- [x] **Layer Gate**: All four entities go into `backend.Core/Domains/QA/`. Enums go in the same folder. DbSet registrations go in `backend.EntityFrameworkCore`. No cross-layer leakage.
- [x] **Naming Gate**: Services → `IConversationAppService` / `ConversationAppService` pattern. DTOs → `ConversationDto` in `Services/ConversationService/DTO/`. Entities → PascalCase matching the entity name. (Services/DTOs are out of scope for this feature but naming is pre-confirmed.)
- [x] **Coding Standards Gate**: All classes will have purpose comments. Methods will not exceed screen height. Guard clauses via `Ardalis.GuardClauses` applied in constructors where pre-conditions exist. No magic numbers — Language and InputMethod use named enum values.
- [x] **Skill Gate**: `add-endpoint` skill applies to any future CRUD endpoint for these entities. No applicable skill for pure domain entity + migration work — manual scaffolding is justified (no skill covers domain-only entity creation).
- [x] **Multilingual Gate**: `Language` enum includes `English`, `Zulu`, `Sesotho`, `Afrikaans` — all four required languages are first-class values at the data layer. No user-facing UI output in this feature.
- [x] **Citation Gate**: No AI-facing endpoints in this feature. Citation contract will be defined in the RAG pipeline endpoint feature (separate backlog item).
- [x] **Accessibility Gate**: No frontend components in this feature. Not applicable.

## Project Structure

### Documentation (this feature)

```text
specs/005-qa-domain-model/
├── plan.md              ← This file
├── research.md          ← Phase 0 output
├── data-model.md        ← Phase 1 output
└── tasks.md             ← Phase 2 output (/speckit.tasks — not yet created)
```

### Source Code (this feature's additions)

```text
backend/src/backend.Core/
└── Domains/
    └── QA/
        ├── Language.cs            ← New enum
        ├── InputMethod.cs         ← New enum
        ├── Conversation.cs        ← New entity
        ├── Question.cs            ← New entity
        ├── Answer.cs              ← New entity
        └── AnswerCitation.cs      ← New entity

backend/src/backend.EntityFrameworkCore/
└── EntityFrameworkCore/
    ├── backendDbContext.cs        ← Add DbSet<T> for all 4 entities + Fluent API helpers
    └── Migrations/
        └── [timestamp]_AddQADomainModel.cs   ← New EF migration
```

No changes to `backend.Application` or `backend.Web.Core` — services and DTOs are deferred to subsequent features.

## Implementation Steps

---

### Step 1 — Define Enumerations

**Action**:
1. Create `backend/src/backend.Core/Domains/QA/Language.cs` with enum values `English=0, Zulu=1, Sesotho=2, Afrikaans=3`
2. Create `backend/src/backend.Core/Domains/QA/InputMethod.cs` with enum values `Text=0, Voice=1`
3. Add XML doc comments on each enum and each value

**Expected Result**: Two enums compile cleanly in the `backend.Domains.QA` namespace and are referenceable from entity classes.

**Validation**: Project builds without errors. Enum values are accessible when creating entity instances.

---

### Step 2 — Create `Conversation` Entity

**Action**:
1. Create `backend/src/backend.Core/Domains/QA/Conversation.cs`
2. Extend `FullAuditedEntity<Guid>`
3. Add properties: `UserId` (`long`, `[Required]`), `Language`, `InputMethod`, `StartedAt` (`DateTime`, `[Required]`), `IsPublicFaq` (`bool`), `FaqCategoryId` (`Guid?`), navigation `FaqCategory` (`Category?`), collection `Questions`
4. Add `[ForeignKey(nameof(FaqCategoryId))]` on `FaqCategory`
5. Add XML doc comments on class and all public members

**Expected Result**: `Conversation` entity compiles in `backend.Core` with correct property types and navigation declarations.

**Validation**: No EF Core references in `backend.Core`. Build succeeds. Navigation property types are correct (`Category` from `backend.Domains.LegalDocuments`).

---

### Step 3 — Create `Question` Entity

**Action**:
1. Create `backend/src/backend.Core/Domains/QA/Question.cs`
2. Extend `FullAuditedEntity<Guid>`
3. Add properties: `ConversationId` (`Guid`, `[Required]`), navigation `Conversation`, `OriginalText` (`string`, `[Required]`), `TranslatedText` (`string`, `[Required]`), `Language`, `InputMethod`, `AudioFile` (`string?`, `[MaxLength(500)]`), collection `Answers`
4. Add `[ForeignKey(nameof(ConversationId))]` on `Conversation`
5. Add XML doc comments on class and all public members

**Expected Result**: `Question` entity compiles correctly. `Conversation` navigation is typed correctly.

**Validation**: Build succeeds. `AudioFile` is nullable. `OriginalText` and `TranslatedText` are non-nullable strings.

---

### Step 4 — Create `Answer` Entity

**Action**:
1. Create `backend/src/backend.Core/Domains/QA/Answer.cs`
2. Extend `FullAuditedEntity<Guid>`
3. Add properties: `QuestionId` (`Guid`, `[Required]`), navigation `Question`, `Text` (`string`, `[Required]`), `Language`, `AudioFile` (`string?`, `[MaxLength(500)]`), `IsAccurate` (`bool?`), `AdminNotes` (`string?`), collection `Citations`
4. Add `[ForeignKey(nameof(QuestionId))]` on `Question`
5. Add XML doc comments on class and all public members

**Expected Result**: `Answer` entity compiles. Nullable properties (`IsAccurate`, `AdminNotes`, `AudioFile`) are correctly typed as nullable.

**Validation**: Build succeeds. All nullable fields allow null at runtime.

---

### Step 5 — Create `AnswerCitation` Entity

**Action**:
1. Create `backend/src/backend.Core/Domains/QA/AnswerCitation.cs`
2. Extend `FullAuditedEntity<Guid>`
3. Add properties: `AnswerId` (`Guid`, `[Required]`), navigation `Answer`, `ChunkId` (`Guid`, `[Required]`), navigation `Chunk` (`DocumentChunk`), `SectionNumber` (`string`, `[Required]`, `[MaxLength(100)]`), `Excerpt` (`string`, `[Required]`), `RelevanceScore` (`decimal`, `[Required]`)
4. Add `[ForeignKey(nameof(AnswerId))]` on `Answer` and `[ForeignKey(nameof(ChunkId))]` on `Chunk`
5. Add a class-level XML comment noting the cross-aggregate nature of the `Chunk` reference
6. Add XML doc comments on all public members

**Expected Result**: `AnswerCitation` compiles with both parent-aggregate FK (`AnswerId`) and cross-aggregate FK (`ChunkId`) correctly declared.

**Validation**: Build succeeds. `DocumentChunk` navigation uses the `backend.Domains.LegalDocuments` type.

---

### Step 6 — Register DbSets in `backendDbContext`

**Action**:
1. Open `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs`
2. Add `using backend.Domains.QA;` import
3. Add four `DbSet<T>` properties in a new `// ── Q&A domain ──` section:
   - `DbSet<Conversation> Conversations`
   - `DbSet<Question> Questions`
   - `DbSet<Answer> Answers`
   - `DbSet<AnswerCitation> AnswerCitations`
4. Add XML doc comments on each DbSet

**Expected Result**: All four entities are registered in the DbContext.

**Validation**: Build succeeds. EF Core model snapshot includes the four new entity types.

---

### Step 7 — Configure EF Core Fluent API Relationships

**Action**:
1. In `backendDbContext.OnModelCreating`, add a call to `ConfigureQARelationships(modelBuilder)`
2. Implement `private static void ConfigureQARelationships(ModelBuilder modelBuilder)` with:
   - `Conversation` → `User` FK (`UserId`, `DeleteBehavior.Restrict`)
   - `Conversation` → `Category` FK (`FaqCategoryId`, nullable, `DeleteBehavior.Restrict`)
   - `Conversation` index on `UserId`
   - `Conversation` index on `(IsPublicFaq, FaqCategoryId)`
   - `Question` → `Conversation` FK (`ConversationId`, `DeleteBehavior.Cascade`)
   - `Question` index on `ConversationId`
   - `Answer` → `Question` FK (`QuestionId`, `DeleteBehavior.Cascade`)
   - `Answer` index on `QuestionId`
   - `AnswerCitation` → `Answer` FK (`AnswerId`, `DeleteBehavior.Cascade`)
   - `AnswerCitation` → `DocumentChunk` FK (`ChunkId`, `DeleteBehavior.Restrict`)
   - `AnswerCitation` index on `AnswerId`
   - `AnswerCitation` index on `ChunkId`
3. Add XML comment on the new private method explaining its responsibility
4. Keep the new method under ~60 lines; split into per-entity sub-methods if needed

**Expected Result**: All foreign keys, cascade rules, and indexes are expressed in Fluent API. No mapping errors.

**Validation**: `dotnet build` succeeds. EF Core validates the model without warnings. No duplicate index or constraint names.

---

### Step 8 — Generate EF Core Migration

**Action**:
1. From `backend/` run:
   ```bash
   dotnet ef migrations add AddQADomainModel \
     --project src/backend.EntityFrameworkCore \
     --startup-project src/backend.Web.Host
   ```
2. Review the generated `Up()` method to confirm:
   - Four new tables created: `Conversations`, `Questions`, `Answers`, `AnswerCitations`
   - All FK constraints present
   - All indexes present
   - No unexpected table changes to existing entities

**Expected Result**: Migration files generated at `backend/src/backend.EntityFrameworkCore/Migrations/[timestamp]_AddQADomainModel.cs` and snapshot updated.

**Validation**: Migration compiles without errors. `Up()` creates exactly four tables. `Down()` drops them cleanly.

---

### Step 9 — Apply Migration to PostgreSQL

**Action**:
1. Ensure the PostgreSQL database is accessible
2. From `backend/` run:
   ```bash
   dotnet ef database update \
     --project src/backend.EntityFrameworkCore \
     --startup-project src/backend.Web.Host
   ```
3. Verify in the database that tables `Conversations`, `Questions`, `Answers`, `AnswerCitations` are created with correct columns and constraints

**Expected Result**: Database schema reflects the domain model. All FK constraints are active. All indexes are present.

**Validation**: Run `\d Conversations` (psql) or equivalent to confirm columns and constraints. No pending migrations remain (`dotnet ef migrations list` shows all applied).

---

### Step 10 — Seed and Validate End-to-End Data

**Action**:
1. Write a short integration test or seed script that:
   - Inserts a `Conversation` (linked to an existing test user)
   - Inserts a `Question` linked to the Conversation
   - Inserts an `Answer` linked to the Question
   - Inserts an `AnswerCitation` linked to the Answer and an existing `DocumentChunk`
2. Query the full chain: `Conversations.Include(Questions.Include(Answers.Include(Citations.Include(Chunk))))`
3. Assert all records are returned with correct FK values

**Expected Result**: Full Conversation → Question → Answer → Citation chain is persisted and queryable. Cross-aggregate `DocumentChunk` navigation resolves correctly.

**Validation**: No FK violation exceptions. Navigation properties are populated. `RelevanceScore`, `SectionNumber`, and `Excerpt` values round-trip correctly.

---

## Dependencies & Order

```text
Step 1  (Enums)           → no dependencies
Step 2  (Conversation)    → Step 1 (Language, InputMethod)
Step 3  (Question)        → Steps 1, 2
Step 4  (Answer)          → Steps 1, 3
Step 5  (AnswerCitation)  → Steps 4; requires DocumentChunk to exist in DB (feature 004)
Step 6  (DbSets)          → Steps 2–5
Step 7  (Fluent API)      → Step 6
Step 8  (Migration)       → Step 7
Step 9  (Apply)           → Step 8; DocumentChunk table must exist
Step 10 (Seed/Validate)   → Step 9; requires at least one User and one DocumentChunk in DB
```

## Critical Path

```
Enums → Entities (1-4) → DbContext (6-7) → Migration (8) → Apply (9) → Validate (10)
```

## Failure Handling

| Failure | Diagnosis | Resolution |
|---------|-----------|------------|
| FK type mismatch (`UserId`) | `Conversation.UserId` type doesn't match `AbpUsers.Id` | Confirm ABP Zero User PK is `long`; change `UserId` type accordingly |
| Navigation property not recognized | Missing `[ForeignKey]` annotation or incorrect property name | Verify `[ForeignKey(nameof(XxxId))]` matches the FK property name exactly |
| Cross-aggregate FK error at migration | `DocumentChunks` table doesn't exist | Ensure feature 004 migration was applied before running this migration |
| Cascade conflict | Multiple cascade paths through ABP audit tables | Use `DeleteBehavior.Restrict` or `ClientSetNull` and test explicitly |
| Migration snapshot drift | Snapshot out of sync from manual edits | Run `dotnet ef migrations remove` and regenerate |
| Seed fails with FK violation | Test user or DocumentChunk missing | Ensure seed data creates/uses existing User and DocumentChunk records |

## Deliverables

| Deliverable | Location |
|-------------|----------|
| `Language.cs` enum | `backend.Core/Domains/QA/` |
| `InputMethod.cs` enum | `backend.Core/Domains/QA/` |
| `Conversation.cs` entity | `backend.Core/Domains/QA/` |
| `Question.cs` entity | `backend.Core/Domains/QA/` |
| `Answer.cs` entity | `backend.Core/Domains/QA/` |
| `AnswerCitation.cs` entity | `backend.Core/Domains/QA/` |
| Updated `backendDbContext.cs` | `backend.EntityFrameworkCore/EntityFrameworkCore/` |
| EF migration `AddQADomainModel` | `backend.EntityFrameworkCore/Migrations/` |
| PostgreSQL schema (4 new tables) | Applied to database |
| End-to-end seed/validation | Test project or manual seed script |

## Complexity Tracking

No constitution violations. This feature follows all established patterns in the codebase.
