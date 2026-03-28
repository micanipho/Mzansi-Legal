# Tasks: RAG Domain Model

**Input**: Design documents from `/specs/004-rag-domain-model/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅

**Tests**: Not explicitly requested in the spec. Integration test seed task included in Phase 6 for validation.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- All paths are relative to the repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the `LegalDocuments` domain folder and confirm branch readiness.

- [x] T001 Create folder `backend/src/backend.Core/Domains/LegalDocuments/` (the domain container for all entities and enum in this feature)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The `DocumentDomain` enum must exist before `Category` can be defined. This blocks ALL user stories.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T002 Create `DocumentDomain` enum with values `Legal = 1, Financial = 2` in `backend/src/backend.Core/Domains/LegalDocuments/DocumentDomain.cs`
- [x] T003 Verify `dotnet build backend/src/backend.Core` succeeds after adding the enum

**Checkpoint**: Enum compiles — user story implementation can now begin.

---

## Phase 3: User Story 1 — Store and Categorise Legal Documents (Priority: P1) 🎯 MVP

**Goal**: Administrators can create Categories and LegalDocuments, link them via FK, and retrieve both records with their relationship intact.

**Independent Test**: Insert one Category (Domain = Legal) and one LegalDocument referencing it; query both back with all properties intact; attempt FK violation and confirm rejection.

### Implementation for User Story 1

- [x] T004 [P] [US1] Create `Category` entity extending `FullAuditedEntity<Guid>` with properties `Name [Required, MaxLength(200)]`, `Icon [MaxLength(100)]`, `Domain [Required]`, `SortOrder`, and navigation `ICollection<LegalDocument> LegalDocuments` in `backend/src/backend.Core/Domains/LegalDocuments/Category.cs`
- [x] T005 [P] [US1] Create `LegalDocument` entity extending `FullAuditedEntity<Guid>` with all properties from data-model.md (Title, ShortName, ActNumber, Year, FullText, FileName, OriginalPdfId, CategoryId FK, IsProcessed=false, TotalChunks=0, navigation `Category` and `ICollection<DocumentChunk> Chunks`) in `backend/src/backend.Core/Domains/LegalDocuments/LegalDocument.cs`
- [x] T006 [US1] Add `DbSet<Category> Categories` and `DbSet<LegalDocument> LegalDocuments` to `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs`
- [x] T007 [US1] Add Fluent API in `backendDbContext.OnModelCreating`: `LegalDocument → Category` with `OnDelete(DeleteBehavior.Restrict)`; unique index on `LegalDocument(ActNumber, Year)` — in `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs`
- [x] T008 [US1] Create `CategoryDto` with `[AutoMap(typeof(Category))]` and all flat properties in `backend/src/backend.Application/Services/CategoryService/DTO/CategoryDto.cs`
- [x] T009 [US1] Create `ICategoryAppService` extending `IAsyncCrudAppService<CategoryDto, Guid>` in `backend/src/backend.Application/Services/CategoryService/ICategoryAppService.cs`
- [x] T010 [US1] Create `CategoryAppService` extending `AsyncCrudAppService<Category, CategoryDto, Guid>` with `[AbpAuthorize]` in `backend/src/backend.Application/Services/CategoryService/CategoryAppService.cs`
- [x] T011 [US1] Create `LegalDocumentListDto` (excludes `FullText`) and `LegalDocumentDto` (includes `FullText`), both with `[AutoMap(typeof(LegalDocument))]`, in `backend/src/backend.Application/Services/LegalDocumentService/DTO/`
- [x] T012 [US1] Create `ILegalDocumentAppService` extending `IAsyncCrudAppService<LegalDocumentDto, Guid>` in `backend/src/backend.Application/Services/LegalDocumentService/ILegalDocumentAppService.cs`
- [x] T013 [US1] Create `LegalDocumentAppService` extending `AsyncCrudAppService<LegalDocument, LegalDocumentDto, Guid>` with `[AbpAuthorize]` and `GetAllAsync` override using `LegalDocumentListDto` to exclude `FullText` from list queries in `backend/src/backend.Application/Services/LegalDocumentService/LegalDocumentAppService.cs`
- [x] T014 [US1] Verify `dotnet build` succeeds across Core, EntityFrameworkCore, and Application projects
- [x] T015 [US1] Generate EF migration: `dotnet ef migrations add AddCategoryAndLegalDocument --project backend/src/backend.EntityFrameworkCore --startup-project backend/src/backend.Web.Host`
- [x] T016 [US1] Apply migration: `dotnet ef database update --project backend/src/backend.EntityFrameworkCore --startup-project backend/src/backend.Web.Host`
- [x] T017 [US1] Verify in PostgreSQL: confirm `Categories` and `LegalDocuments` tables exist with correct columns and the unique index on `(ActNumber, Year)` is present

**Checkpoint**: User Story 1 is complete. Run the host, authenticate via Swagger, create a Category and a LegalDocument, verify retrieval. Test FK rejection by supplying an invalid CategoryId.

---

## Phase 4: User Story 2 — Break Documents into Searchable Chunks (Priority: P2)

**Goal**: A processing service can insert DocumentChunks linked to a LegalDocument and retrieve them in SortOrder sequence. Deleting a LegalDocument cascades to its chunks.

**Independent Test**: Insert a LegalDocument (reuse from US1), insert five DocumentChunks with SortOrder 1–5, query ordered chunks; verify cascade delete removes all chunks when document is deleted.

### Implementation for User Story 2

- [x] T018 [US2] Create `DocumentChunk` entity extending `FullAuditedEntity<Guid>` with properties `DocumentId FK`, `ChapterTitle [MaxLength(500)]`, `SectionNumber [MaxLength(50)]`, `SectionTitle [MaxLength(500)]`, `Content [Required]`, `TokenCount`, `SortOrder`, and navigations `LegalDocument Document` and `ChunkEmbedding Embedding` in `backend/src/backend.Core/Domains/LegalDocuments/DocumentChunk.cs`
- [x] T019 [US2] Add `DbSet<DocumentChunk> DocumentChunks` to `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs`
- [x] T020 [US2] Add Fluent API in `backendDbContext.OnModelCreating`: `DocumentChunk → LegalDocument` with `OnDelete(DeleteBehavior.Cascade)`; composite index on `DocumentChunk(DocumentId, SortOrder)` — in `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs`
- [x] T021 [US2] Verify `dotnet build` succeeds
- [x] T022 [US2] Generate additive EF migration: covered by `AddCategoryAndLegalDocument` migration (all entities registered at once)
- [x] T023 [US2] Apply migration: covered by single migration apply — `DocumentChunks` table confirmed in DB
- [x] T024 [US2] Verify in PostgreSQL: confirm `DocumentChunks` table exists with `DocumentId` FK, the composite index on `(DocumentId, SortOrder)` is present, and cascade delete works via SQL test (delete a LegalDocument row, confirm chunk rows are removed)

**Checkpoint**: User Story 2 is complete. Five chunks can be inserted and queried back in SortOrder; cascade delete confirmed.

---

## Phase 5: User Story 3 — Attach Embedding Vectors to Chunks (Priority: P3)

**Goal**: A processing service can store a 1 536-element `float[]` vector per DocumentChunk and retrieve the full vector without precision loss. Deleting a DocumentChunk cascades to its embedding.

**Independent Test**: Insert a DocumentChunk (reuse from US2), insert a ChunkEmbedding with `new float[1536]` (all 0.5f), read back and assert `array_length(Vector, 1) = 1536` and all values preserved; verify cascade delete.

### Implementation for User Story 3

- [x] T025 [US3] Create `ChunkEmbedding` entity extending `FullAuditedEntity<Guid>` with properties `ChunkId FK`, navigation `DocumentChunk Chunk`, `float[] Vector`, and constant `public const int EmbeddingDimension = 1536` in `backend/src/backend.Core/Domains/LegalDocuments/ChunkEmbedding.cs`
- [x] T026 [US3] Add `DbSet<ChunkEmbedding> ChunkEmbeddings` to `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs`
- [x] T027 [US3] Add Fluent API in `backendDbContext.OnModelCreating`: one-to-one `ChunkEmbedding → DocumentChunk` with `HasForeignKey<ChunkEmbedding>(e => e.ChunkId)` and `OnDelete(DeleteBehavior.Cascade)` — in `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs`
- [x] T028 [US3] Verify `dotnet build` succeeds
- [x] T029 [US3] Generate additive EF migration: covered by `AddCategoryAndLegalDocument` migration (all entities registered at once)
- [x] T030 [US3] Apply migration: covered by single migration apply — `ChunkEmbeddings` table confirmed in DB
- [x] T031 [US3] Verify in PostgreSQL: `ChunkEmbeddings` table exists, `Vector` column type confirmed `real[]`, `array_length` = 1536 verified via pg_catalog query

**Checkpoint**: User Story 3 is complete. All three user stories are independently functional.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, code standards check, and quickstart walkthrough.

- [ ] T032 [P] Run `quickstart.md` end-to-end: create one Category, one LegalDocument, five DocumentChunks, five ChunkEmbeddings via Swagger; confirm all records queryable and SC-001 through SC-006 pass
- [x] T033 [P] Review all five new entity/enum files against `docs/RULES.md`: confirm class-level XML doc comments present, no magic numbers (1536 replaced by `ChunkEmbedding.EmbeddingDimension`), no method exceeds one screen, guard clause on vector length present in any service that accepts `Vector`
- [ ] T034 Commit all changes on branch `004-rag-domain-model` with message `feat: add RAG domain model (Category, LegalDocument, DocumentChunk, ChunkEmbedding) with EF migrations`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Phase 2 — can start after enum compiles
- **US2 (Phase 4)**: Depends on Phase 3 being complete (needs LegalDocument to exist in DB for FK)
- **US3 (Phase 5)**: Depends on Phase 4 being complete (needs DocumentChunk to exist in DB for FK)
- **Polish (Phase 6)**: Depends on all user story phases complete

### User Story Dependencies

- **US1 (P1)**: Blocked only by Foundational (Phase 2)
- **US2 (P2)**: Blocked by US1 — needs `LegalDocuments` table to exist for FK
- **US3 (P3)**: Blocked by US2 — needs `DocumentChunks` table to exist for FK

### Within Each User Story

- Entity files (`[P]` marked) can be created in parallel by different developers
- DbSet + Fluent API must follow entity creation
- Migration must follow DbSet registration
- DB apply must follow migration generation
- Verification must follow DB apply

### Parallel Opportunities

- T004 and T005 (Category + LegalDocument entities) can be created in parallel
- T008 and T011 (CategoryDto + LegalDocumentDto) can be created in parallel after T004/T005
- T009 and T012 (service interfaces) can be created in parallel
- T010 and T013 (service implementations) can be created in parallel after interfaces
- T032 and T033 (Polish) can run in parallel

---

## Parallel Example: User Story 1

```text
# Parallel entity creation (T004 + T005):
Developer A: Category.cs
Developer B: LegalDocument.cs

# Parallel DTO creation (T008 + T011):
Developer A: CategoryDto.cs
Developer B: LegalDocumentDto.cs + LegalDocumentListDto.cs

# Parallel service interface creation (T009 + T012):
Developer A: ICategoryAppService.cs
Developer B: ILegalDocumentAppService.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (enum) — blocks all stories
3. Complete Phase 3: User Story 1 (Category + LegalDocument + services + migration)
4. **STOP and VALIDATE**: Categories and LegalDocuments fully functional via Swagger
5. Demonstrate: create a category, attach a document, retrieve both

### Incremental Delivery

1. Setup + Foundational → enum available
2. User Story 1 → Category + LegalDocument CRUD in PostgreSQL (MVP!)
3. User Story 2 → DocumentChunk storage and ordered retrieval
4. User Story 3 → ChunkEmbedding vector storage
5. Each story adds a pipeline layer without breaking the previous layer

### Parallel Team Strategy

With two developers:
- Developer A: US1 entities + services (T004, T005, T008–T013)
- Developer B: Constitution review + quickstart prep (T033)
- Both converge on T014–T017 (build, migrate, verify)

---

## Notes

- `[P]` tasks operate on different files and have no unresolved dependencies
- Each user story's checkpoint marks an independently testable and demonstrable increment
- US2 and US3 do not need Application services — chunks and embeddings are managed by the (future) ingestion pipeline
- The `EmbeddingDimension` constant in `ChunkEmbedding` eliminates the magic number `1536` throughout the codebase
- Migrations are incremental (one per user story phase) to keep each phase reversible independently
- Commit after each phase checkpoint to keep the git history aligned with story completion
