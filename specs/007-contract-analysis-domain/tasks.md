# Tasks: Contract Analysis Domain Model

**Input**: Design documents from `/specs/007-contract-analysis-domain/`
**Prerequisites**: plan.md ✅ | spec.md ✅ | research.md ✅ | data-model.md ✅

**Tests**: Not requested in the feature specification — no test tasks included.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Exact file paths included in every task description

## Path Conventions

- Backend domain: `backend/src/backend.Core/Domains/ContractAnalysis/`
- Backend EFCore: `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm the project baseline is clean before adding new files.

- [ ] T001 Verify the backend project builds with zero errors from the current branch baseline by running `dotnet build backend/backend.sln` and confirming no compilation errors exist before any new files are added

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create the two reference-list enums that both domain entities depend on. These have no dependencies on each other and can be created in parallel.

**⚠️ CRITICAL**: No entity files can be created until both enums exist in their correct namespaces.

- [ ] T002 [P] Create `ContractType` enum in `backend/src/backend.Core/Domains/ContractAnalysis/ContractType.cs` with values `Employment = 0`, `Lease = 1`, `Credit = 2`, `Service = 3` — follow the XML doc-comment style of `backend/src/backend.Core/Domains/QA/Language.cs`
- [ ] T003 [P] Create `FlagSeverity` enum in `backend/src/backend.Core/Domains/ContractAnalysis/FlagSeverity.cs` with values `Red = 0`, `Amber = 1`, `Green = 2` — follow the same XML doc-comment style as `ContractType`

**Checkpoint**: Both enums compile. Entity creation can now begin.

---

## Phase 3: User Story 1 — Store Contract Analysis Result (Priority: P1) 🎯 MVP

**Goal**: Persist a complete `ContractAnalysis` record linked to an authenticated `AppUser`, including file reference, extracted text, contract type, health score, summary, language, and analysis timestamp.

**Independent Test**: Run `dotnet build` successfully, then apply the migration and verify the `ContractAnalyses` table exists in PostgreSQL with `UserId` (NOT NULL), `HealthScore` (check constraint 0–100), and `IX_ContractAnalyses_UserId` index. Create a row manually via psql/pgAdmin and confirm the FK and check constraint are enforced.

### Implementation for User Story 1

- [ ] T004 [US1] Create `ContractAnalysis` entity in `backend/src/backend.Core/Domains/ContractAnalysis/ContractAnalysis.cs` extending `FullAuditedEntity<Guid>` — include all properties from `data-model.md` (`UserId long [Required]`, `OriginalFileId Guid?`, `ExtractedText string`, `ContractType ContractType [Required]`, `HealthScore int [Required][Range(0,100)]`, `Summary string`, `Language Language [Required]` from `backend.Domains.QA`, `AnalysedAt DateTime [Required]`, `Flags virtual ICollection<ContractFlag>`) — add class and property XML doc-comments following the style in `backend/src/backend.Core/Domains/QA/Conversation.cs`
- [ ] T005 [US1] Register `DbSet<ContractAnalysis> ContractAnalyses` in `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs` under a new `// ── Contract Analysis domain ──` comment section, and add a call to `ConfigureContractAnalysisRelationships(modelBuilder)` in `OnModelCreating` — create the private stub method with an index on `ContractAnalysis.UserId` (`IX_ContractAnalyses_UserId`) following the pattern of `ConfigureQuestionRelationships` in the same file

**Checkpoint**: User Story 1 domain model complete. `ContractAnalysis` can be registered and its table created with the `UserId` index.

---

## Phase 4: User Story 2 — Store Individual Contract Flags (Priority: P2)

**Goal**: Persist `ContractFlag` records owned by a `ContractAnalysis`, with cascade delete enforced — so deleting a `ContractAnalysis` automatically removes all its flags.

**Independent Test**: Apply the migration and verify the `ContractFlags` table exists with `ContractAnalysisId` (NOT NULL FK), `IX_ContractFlags_ContractAnalysisId` index, and cascade delete behavior (delete a `ContractAnalysis` row via psql and confirm its child `ContractFlag` rows are removed).

### Implementation for User Story 2

- [ ] T006 [US2] Create `ContractFlag` entity in `backend/src/backend.Core/Domains/ContractAnalysis/ContractFlag.cs` extending `FullAuditedEntity<Guid>` — include all properties from `data-model.md` (`ContractAnalysisId Guid [Required]`, navigation `ContractAnalysis` with `[ForeignKey(nameof(ContractAnalysisId))]`, `Severity FlagSeverity [Required]`, `Title string [Required][MaxLength(200)]`, `Description string [Required]`, `ClauseText string [Required]`, `LegislationCitation string [MaxLength(1000)]`, `SortOrder int` defaulting to 0) — add class and property XML doc-comments following the style of `backend/src/backend.Core/Domains/QA/Answer.cs`
- [ ] T007 [US2] Register `DbSet<ContractFlag> ContractFlags` in `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs` under the same `// ── Contract Analysis domain ──` section added in T005
- [ ] T008 [US2] Expand `ConfigureContractAnalysisRelationships` in `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs` to configure the `ContractFlag → ContractAnalysis` FK with `OnDelete(DeleteBehavior.Cascade)` and add `IX_ContractFlags_ContractAnalysisId` index — follow the exact pattern of `ConfigureQuestionRelationships` and `ConfigureAnswerRelationships` in the same file

**Checkpoint**: User Story 2 domain model complete. `ContractFlag` can be persisted and cascade-deleted with its parent analysis.

---

## Phase 5: User Story 3 — Query Flags by Severity Across All Contracts (Priority: P3)

**Goal**: Enable efficient filtering of `ContractFlag` records by `Severity` across all `ContractAnalysis` records in the system by ensuring the supporting database index exists.

**Independent Test**: Apply the migration, confirm `IX_ContractFlags_Severity` exists in PostgreSQL (`\d ContractFlags` in psql), then run a cross-analysis query: `SELECT * FROM "ContractFlags" WHERE "Severity" = 0` and confirm the query plan uses the index (via `EXPLAIN ANALYZE`).

### Implementation for User Story 3

- [ ] T009 [US3] Add `IX_ContractFlags_Severity` index to the `ConfigureContractAnalysisRelationships` method in `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs` — add the index immediately after the `IX_ContractFlags_ContractAnalysisId` index added in T008, following the same `modelBuilder.Entity<ContractFlag>().HasIndex(f => f.Severity)` pattern

**Checkpoint**: All three user stories' domain model requirements are met. Ready for migration.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Generate and apply the EF Core migration; verify all database constraints, indexes, and cascade behavior are correct end-to-end.

- [ ] T010 Generate EF Core migration named `AddContractAnalysisDomain` by running `dotnet ef migrations add AddContractAnalysisDomain --project src/backend.EntityFrameworkCore --startup-project src/backend.Web.Host` from the `backend/` directory — review the generated migration file in `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/Migrations/` to confirm it creates `ContractAnalyses` table, `ContractFlags` table, all FK constraints, the check constraint on `HealthScore`, and all three indexes (`IX_ContractAnalyses_UserId`, `IX_ContractFlags_ContractAnalysisId`, `IX_ContractFlags_Severity`)
- [ ] T011 Apply the migration against the local development PostgreSQL database using the ABP Migrator (`dotnet run --project src/backend.Migrator` from `backend/`) and verify via psql or pgAdmin that: both tables exist, `UserId` is NOT NULL on `ContractAnalyses`, `ContractAnalysisId` is NOT NULL on `ContractFlags`, cascade delete is active, all three indexes are present, and the `HealthScore` check constraint rejects values outside 0–100

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — BLOCKS all entity creation
- **US1 (Phase 3)**: Depends on Phase 2 (enums must exist) — `ContractAnalysis` entity needs `ContractType` (T002) and `Language` (already exists)
- **US2 (Phase 4)**: Depends on Phase 3 (T004 must be complete — `ContractFlag` has FK to `ContractAnalysis`)
- **US3 (Phase 5)**: Depends on Phase 4 (T008 must be complete — extends the same Fluent API method)
- **Polish (Phase 6)**: Depends on all user story phases complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational — no dependency on US2 or US3
- **User Story 2 (P2)**: Depends on US1 complete (ContractFlag has FK to ContractAnalysis entity)
- **User Story 3 (P3)**: Can start alongside US2 (only adds an index to the existing Fluent API method) — or sequentially after US2 for simplicity

### Parallel Opportunities

- **T002 and T003** (Phase 2): Fully parallel — different files, no shared dependencies
- **T004 and T003** (if T002 done): T004 starts once T002 completes; T003 can still be running
- **T010 and T011**: Strictly sequential — apply after generate

---

## Parallel Example: Foundational Phase

```bash
# Run T002 and T003 simultaneously (different files, no dependencies):
Task A: "Create ContractType enum in backend/src/backend.Core/Domains/ContractAnalysis/ContractType.cs"
Task B: "Create FlagSeverity enum in backend/src/backend.Core/Domains/ContractAnalysis/FlagSeverity.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Verify clean build (T001)
2. Complete Phase 2: Create both enums (T002, T003)
3. Complete Phase 3: Create `ContractAnalysis` entity + DbContext registration (T004, T005)
4. Generate + apply migration for `ContractAnalyses` table only (T010, T011)
5. **STOP and VALIDATE**: Confirm `ContractAnalyses` table exists with all columns, `UserId` NOT NULL, HealthScore check constraint, and `IX_ContractAnalyses_UserId` index

### Incremental Delivery

1. Phase 1 + Phase 2 + Phase 3 → `ContractAnalyses` table (MVP)
2. Phase 4 → `ContractFlags` table with cascade delete
3. Phase 5 → `IX_ContractFlags_Severity` index (cross-analysis query support)
4. Phase 6 → Full migration applied and verified end-to-end

---

## Notes

- `Language` enum is **not created** — it is imported from `backend.Domains.QA.Language` (research decision D1)
- `UserId` is `long`, not `Guid` — ABP Zero User PK type (research decision D3)
- `OriginalFileId` is `Guid?` — ABP BinaryObject reference pattern (research decision D2)
- All new C# files must have purpose comments on the class and all public members (RULES.md)
- DbContext file changes in T005, T007, T008, T009 are all to the **same file** — do not parallelize those tasks
- `SortOrder` on `ContractFlag` has no `[Required]` annotation — EF will default it to 0 (int default)
- Commit after each phase checkpoint to preserve incremental progress
