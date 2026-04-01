# Tasks: Q&A Domain Model for RAG System

**Input**: Design documents from `/specs/005-qa-domain-model/`
**Branch**: `005-qa-domain-model`
**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅

**Tests**: No test tasks generated (not explicitly requested in spec). Integration validation via seed data in Phase 3.

**Organization**: Tasks grouped by user story. All four entities are created in Phase 3 (US1) since the full chain Conversation → Question → Answer → Citation is the P1 story. Later phases verify that the shared model correctly supports US2–US4 scenarios and enforce constitution compliance.

**Constitution enforcement**: Tasks explicitly call out XML doc comments, Ardalis.GuardClauses, method length, and `docs/RULES.md` compliance requirements — per user instruction for strict adherence.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no shared dependencies)
- **[Story]**: Which user story this task belongs to
- Exact file paths based on `backend/src/` project structure

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the `QA` domain folder and confirm the existing project prerequisites are in place before any entity work begins.

- [x] T001 Create domain folder `backend/src/backend.Core/Domains/QA/` (no files yet — confirms path exists for subsequent tasks)
- [x] T002 Verify that `Ardalis.GuardClauses` NuGet package is already referenced in `backend/src/backend.Core/backend.Core.csproj`; add it if missing

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Define the two enumerations (`Language` and `InputMethod`) that are required by all four entities. No entity file can be compiled without these.

**⚠️ CRITICAL**: All entity tasks in Phase 3 depend on T003 and T004.

- [x] T003 [P] Create `Language` enum in `backend/src/backend.Core/Domains/QA/Language.cs` with values `English = 0, Zulu = 1, Sesotho = 2, Afrikaans = 3`; add XML doc comment on the enum type and on each value explaining its ISO 639-1 code (en, zu, st, af)
- [x] T004 [P] Create `InputMethod` enum in `backend/src/backend.Core/Domains/QA/InputMethod.cs` with values `Text = 0, Voice = 1`; add XML doc comment on the enum type and on each value

**Checkpoint**: Both enums compile in `backend.Domains.QA` namespace. `dotnet build` passes before proceeding.

---

## Phase 3: User Story 1 — Start a Legal Conversation (Priority: P1) 🎯 MVP

**Goal**: A user can start a conversation, submit a question, receive an AI answer, and have citations linking the answer to legislation chunks — all persisted end-to-end in PostgreSQL.

**Independent Test**: Insert one Conversation (with UserId), one Question, one Answer, and one AnswerCitation (referencing an existing DocumentChunk). Query back the full chain with navigation properties populated. Assert all FK values are correct.

### Implementation for User Story 1

- [x] T005 [P] [US1] Create `Conversation` entity in `backend/src/backend.Core/Domains/QA/Conversation.cs`:
  - Extend `FullAuditedEntity<Guid>`
  - Properties: `UserId` (`long`, `[Required]`), `Language` (`Language`, `[Required]`), `InputMethod` (`InputMethod`, `[Required]`), `StartedAt` (`DateTime`, `[Required]`), `IsPublicFaq` (`bool`), `FaqCategoryId` (`Guid?`), navigation `FaqCategory` (`Category?`) with `[ForeignKey(nameof(FaqCategoryId))]`, collection `Questions` (`ICollection<Question>`)
  - Add XML doc comment on the class and on every public property (purpose of each field)
  - No EF Core references — data annotations only in `backend.Core`

- [x] T006 [P] [US1] Create `Question` entity in `backend/src/backend.Core/Domains/QA/Question.cs`:
  - Extend `FullAuditedEntity<Guid>`
  - Properties: `ConversationId` (`Guid`, `[Required]`), navigation `Conversation` with `[ForeignKey(nameof(ConversationId))]`, `OriginalText` (`string`, `[Required]`), `TranslatedText` (`string`, `[Required]`), `Language` (`Language`, `[Required]`), `InputMethod` (`InputMethod`, `[Required]`), `AudioFile` (`string?`, `[MaxLength(500)]`), collection `Answers` (`ICollection<Answer>`)
  - Add XML doc comment on the class and on every public property
  - No EF Core references

- [x] T007 [P] [US1] Create `Answer` entity in `backend/src/backend.Core/Domains/QA/Answer.cs`:
  - Extend `FullAuditedEntity<Guid>`
  - Properties: `QuestionId` (`Guid`, `[Required]`), navigation `Question` with `[ForeignKey(nameof(QuestionId))]`, `Text` (`string`, `[Required]`), `Language` (`Language`, `[Required]`), `AudioFile` (`string?`, `[MaxLength(500)]`), `IsAccurate` (`bool?`), `AdminNotes` (`string?`), collection `Citations` (`ICollection<AnswerCitation>`)
  - Add XML doc comment on the class and on every public property (note `IsAccurate` is null = unreviewed)
  - No EF Core references

- [x] T008 [P] [US1] Create `AnswerCitation` entity in `backend/src/backend.Core/Domains/QA/AnswerCitation.cs`:
  - Extend `FullAuditedEntity<Guid>`
  - Properties: `AnswerId` (`Guid`, `[Required]`), navigation `Answer` with `[ForeignKey(nameof(AnswerId))]`, `ChunkId` (`Guid`, `[Required]`), navigation `Chunk` (`DocumentChunk`) with `[ForeignKey(nameof(ChunkId))]`, `SectionNumber` (`string`, `[Required]`, `[MaxLength(100)]`), `Excerpt` (`string`, `[Required]`), `RelevanceScore` (`decimal`, `[Required]`)
  - Add XML doc comment on the class explaining the cross-aggregate reference to `DocumentChunk`; add XML doc comment on every public property
  - No EF Core references

- [x] T009 [US1] Add `using backend.Domains.QA;` import and four `DbSet<T>` properties to `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs`:
  ```
  // ── Q&A domain ─────────────────────────────────────────────────────────────
  public DbSet<Conversation> Conversations { get; set; }
  public DbSet<Question> Questions { get; set; }
  public DbSet<Answer> Answers { get; set; }
  public DbSet<AnswerCitation> AnswerCitations { get; set; }
  ```
  - Add XML doc comment on each new DbSet property
  - Depends on T005–T008

- [x] T010 [US1] Add `ConfigureQARelationships(modelBuilder)` call inside `OnModelCreating` in `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs`, then implement the private static method with all Fluent API rules:
  - `Conversation → User`: `HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict)`
  - `Conversation → Category`: nullable FK, `OnDelete(DeleteBehavior.Restrict)`
  - `Conversation` index on `UserId`
  - `Conversation` composite index on `(IsPublicFaq, FaqCategoryId)`
  - `Question → Conversation`: `OnDelete(DeleteBehavior.Cascade)` + index on `ConversationId`
  - `Answer → Question`: `OnDelete(DeleteBehavior.Cascade)` + index on `QuestionId`
  - `AnswerCitation → Answer`: `OnDelete(DeleteBehavior.Cascade)` + index on `AnswerId`
  - `AnswerCitation → DocumentChunk`: `OnDelete(DeleteBehavior.Restrict)` + index on `ChunkId`
  - Add XML doc comment on `ConfigureQARelationships` method
  - Keep the method under 60 lines; split into per-entity private helpers if needed to comply with `docs/RULES.md` method length rule
  - Depends on T009

- [x] T011 [US1] Generate EF Core migration from `backend/` directory:
  ```bash
  dotnet ef migrations add AddQADomainModel \
    --project src/backend.EntityFrameworkCore \
    --startup-project src/backend.Web.Host
  ```
  - Review generated `Up()` method: confirm four new tables (`Conversations`, `Questions`, `Answers`, `AnswerCitations`), all FK constraints, all indexes; confirm `Down()` cleanly drops them
  - Depends on T010

- [x] T012 [US1] Apply migration to PostgreSQL from `backend/` directory:
  ```bash
  dotnet ef database update \
    --project src/backend.EntityFrameworkCore \
    --startup-project src/backend.Web.Host
  ```
  - Verify tables exist in the database using `\d` (psql) or a DB client
  - Confirm `dotnet ef migrations list` shows `AddQADomainModel` as Applied
  - Depends on T011

- [x] T013 [US1] Validate end-to-end data insertion in `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/Seed/` or a temporary integration test:
  - Insert one `Conversation` (linked to an existing seed user)
  - Insert one `Question` linked to the Conversation
  - Insert one `Answer` linked to the Question
  - Insert one `AnswerCitation` linked to the Answer and an existing `DocumentChunk`
  - Execute a query using `.Include()` chain to load Conversation → Questions → Answers → Citations → Chunk
  - Assert no FK violation exceptions and all navigation properties are populated
  - Depends on T012

**Checkpoint**: Full Conversation → Question → Answer → Citation chain is persisted and queryable. Cross-aggregate `DocumentChunk` reference resolves. US1 acceptance criteria met.

---

## Phase 4: User Story 2 — Continue an Existing Conversation (Priority: P2)

**Goal**: Verify the domain model correctly supports multiple Questions being added to a single Conversation and that the history is queryable in order.

**Independent Test**: Query a Conversation that has two Questions; assert both Questions and their Answers are returned via the navigation collection in creation order.

### Implementation for User Story 2

- [x] T014 [US2] Verify `Conversation.Questions` navigation collection is populated correctly in the seed/integration scenario from T013: add a second `Question` to the same `Conversation`, add a second `Answer` to that `Question`, then re-query the Conversation and assert `Questions.Count == 2` with both Answers accessible
- [x] T015 [US2] Confirm the `ConversationId` index in the Fluent API configuration (T010) is present in the migration's generated `Up()` method and applied in the database; document result in a code comment on the index configuration line in `backendDbContext.cs`

**Checkpoint**: Multiple Questions per Conversation work correctly. US2 scenario verified without schema changes.

---

## Phase 5: User Story 3 — Mark as Public FAQ (Priority: P3)

**Goal**: Verify that `IsPublicFaq` and `FaqCategoryId` fields on `Conversation` support filtering conversations as public FAQs by Category.

**Independent Test**: Set `IsPublicFaq = true` and assign a valid `FaqCategoryId` on a Conversation, then query `Conversations.Where(c => c.IsPublicFaq && c.FaqCategoryId == targetId)` and assert the record is returned. Set `IsPublicFaq = false` on another Conversation and assert it is excluded.

### Implementation for User Story 3

- [x] T016 [US3] Extend the seed/validation from T013 to insert two additional Conversations: one with `IsPublicFaq = true` and a valid `FaqCategoryId`, one with `IsPublicFaq = false`; query with a FAQ filter and assert correct records are returned
- [x] T017 [US3] Verify the composite index on `(IsPublicFaq, FaqCategoryId)` is present in the applied database schema using a DB client or `\d Conversations` in psql; confirm it matches the Fluent API configuration in `backendDbContext.cs` (T010)

**Checkpoint**: Public FAQ filtering by Category is queryable from the data model. US3 scenario verified without additional schema changes.

---

## Phase 6: User Story 4 — Validate Answer Accuracy (Priority: P4)

**Goal**: Verify that `IsAccurate` and `AdminNotes` fields on `Answer` support post-creation admin review and that nullable semantics are correct.

**Independent Test**: Update an existing `Answer` setting `IsAccurate = true` and `AdminNotes = "Verified by legal team"`. Re-query the Answer and assert both fields are persisted. Update a second Answer with `IsAccurate = false`. Assert the first Answer is not affected.

### Implementation for User Story 4

- [x] T018 [US4] Extend the seed/validation from T013 to update an existing Answer's `IsAccurate` and `AdminNotes` fields; re-query and assert the updates are persisted without affecting other Answer fields
- [x] T019 [US4] Verify that `IsAccurate` stored as `bool?` correctly allows three states: `null` (unreviewed), `true` (accurate), `false` (inaccurate); add a code comment on the `IsAccurate` property in `Answer.cs` explicitly documenting the three-state semantics

**Checkpoint**: Admin review fields work correctly. US4 scenario verified. All four user stories are now confirmed against the data model.

---

## Phase 7: Polish & Constitution Compliance

**Purpose**: Apply `docs/RULES.md` and `docs/BP.md` compliance checks across all new files. This phase enforces the user's requirement for strict adherence to the constitution.

- [x] T020 [P] Audit all six new files in `backend/src/backend.Core/Domains/QA/` against `docs/RULES.md`: verify every class has a purpose comment, every public property has an XML doc comment, no method exceeds screen height, no magic numbers, no dead code; fix any non-compliant items in-place
- [x] T021 [P] Audit the changes to `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs` against `docs/RULES.md`: verify the `ConfigureQARelationships` method (and any sub-methods) are within line limits, have XML doc comments, and use no magic numbers; fix any non-compliant items
- [x] T022 Verify strict layer compliance: confirm no file in `backend/src/backend.Core/Domains/QA/` contains any `using Microsoft.EntityFrameworkCore` or `using Abp.EntityFrameworkCore` reference; confirm no DTO or Application-layer type is referenced from the Core layer
- [x] T023 Run `dotnet build` on the full solution from `backend/` and confirm zero warnings and zero errors across all projects
- [x] T024 Run `dotnet ef migrations list` to confirm `AddQADomainModel` is the only pending item and is shown as Applied; confirm no other migration regressions
- [x] T025 [P] Review `backend/src/backend.Core/Domains/QA/Conversation.cs` and confirm `UserId` is typed as `long` (matching ABP Zero User PK) — document the type choice with an inline comment referencing the research decision (research.md Decision 6)
- [x] T026 [P] Review `backend/src/backend.Core/Domains/QA/AnswerCitation.cs` and confirm the `Chunk` navigation property has a class-level comment noting the cross-aggregate boundary — per constitution Principle IV ("cross-aggregate references are allowed only for traceability requirements")

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 — **BLOCKS all entity tasks**
- **User Story 1 (Phase 3)**: Depends on Phase 2 — T005–T008 can run in parallel after T003/T004
- **User Story 2 (Phase 4)**: Depends on Phase 3 (T013 must be complete for validation data)
- **User Story 3 (Phase 5)**: Depends on Phase 3 (T013 must be complete)
- **User Story 4 (Phase 6)**: Depends on Phase 3 (T013 must be complete)
- **Polish (Phase 7)**: Depends on all entity files being written (T005–T008 minimum)

### User Story Dependencies

- **US1 (P1)**: Depends on Foundational phase only — no dependencies on other stories
- **US2 (P2)**: Depends on US1 data model completion — no new entities required
- **US3 (P3)**: Depends on US1 data model completion — no new entities required
- **US4 (P4)**: Depends on US1 data model completion — no new entities required

### Within User Story 1

```
T003, T004 (parallel enums)
  ↓
T005, T006, T007, T008 (parallel entity creation)
  ↓
T009 (DbSet registration — needs all 4 entities)
  ↓
T010 (Fluent API — needs DbSets registered)
  ↓
T011 (Migration — needs Fluent API complete)
  ↓
T012 (Apply — needs Migration generated)
  ↓
T013 (Seed validation — needs DB tables applied)
```

---

## Parallel Opportunities

### Phase 2 (Foundational)

```
T003: Language.cs    ← run in parallel
T004: InputMethod.cs ← run in parallel
```

### Phase 3 (US1 entities — after T003+T004 complete)

```
T005: Conversation.cs    ← run in parallel
T006: Question.cs        ← run in parallel
T007: Answer.cs          ← run in parallel
T008: AnswerCitation.cs  ← run in parallel
```

### Phase 7 (Polish — independent file audits)

```
T020: QA domain files audit    ← run in parallel
T021: DbContext audit          ← run in parallel
T025: Conversation UserId check ← run in parallel
T026: AnswerCitation comment   ← run in parallel
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001–T002)
2. Complete Phase 2: Foundational enums (T003–T004)
3. Complete Phase 3: US1 entities, DbContext, Migration, Apply, Validate (T005–T013)
4. **STOP and VALIDATE**: Full Conversation → Citation chain works in PostgreSQL
5. Phase 7 polish before PR merge (required by constitution — non-compliant code MUST NOT be merged)

### Incremental Delivery

1. Setup + Foundational → enums ready
2. US1 entities + migration → core chain persisted (MVP)
3. US2 validation → conversation continuation confirmed
4. US3 validation → FAQ filtering confirmed
5. US4 validation → admin review confirmed
6. Polish → constitution compliance verified → PR ready

### Parallel Team Strategy

With multiple developers after Phase 2 is complete:

- Developer A: T005 (Conversation) + T006 (Question)
- Developer B: T007 (Answer) + T008 (AnswerCitation)
- Merge → Developer A or B: T009 → T010 → T011 → T012 → T013
- Both: Phase 4–6 validation in parallel
- Both: Phase 7 polish in parallel

---

## Notes

- All `[P]` tasks operate on different files and have no shared in-progress dependencies
- Each entity file must include XML doc comments on class and all public members before the task is marked complete (constitution mandate — enforced in T020/T021)
- `dotnet build` must pass after T009 before proceeding to T010
- Do NOT use `using Microsoft.EntityFrameworkCore` in any file under `backend.Core/`
- `UserId` MUST be `long` — see research.md Decision 6 and task T025
- After T013, US2–US4 can be validated in parallel (T014–T019 use the same database state)
- Tasks T020–T026 in Phase 7 are mandatory before PR merge per constitution Principle V ("All C# code MUST comply with `docs/RULES.md` and `docs/BP.md` before a PR is merged")
