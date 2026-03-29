# Tasks: Legislation Seed Data Pipeline

**Input**: Design documents from `/specs/012-legislation-seed-data/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: Unit tests included for seed classes (quality gate requirement). Integration/ETL tests excluded — ETL is covered by the existing `EtlPipelineAppService` test surface.

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US4)
- All paths relative to repo root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Folder structure and the central constants manifest that all seed classes depend on.

- [ ] T001 Create `seed-data/legislation/` and `seed-data/financial/` directories with `.gitkeep` files (PDFs are added manually and not committed)
- [ ] T002 Add `seed-data/**/*.pdf` to `.gitignore` so PDF binaries are never accidentally committed
- [ ] T003 Create `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/LegislationManifest.cs` — static class with `CategoryDefinition` and `DocumentDefinition` readonly records; define all 9 categories (Name, Domain, Icon, SortOrder) and all 13 documents (Title, ShortName, ActNumber, Year, FileName, CategoryShortName) as per `data-model.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Category seeder and its wiring into `InitialHostDbBuilder`. All user stories depend on categories existing first.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete — `LegalDocumentRegistrar` and `LegislationIngestionRunner` both require categories to already exist.

- [ ] T004 Create `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/DefaultCategoriesCreator.cs` — constructor accepts `backendDbContext`; `Create()` method iterates `LegislationManifest.Categories`, checks for existing category by `Name` (case-insensitive), inserts if absent; guard clause for null context; purpose comment on class and `Create()`
- [ ] T005 Modify `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/InitialHostDbBuilder.cs` — add `new DefaultCategoriesCreator(_context).Create();` call after `DefaultSettingsCreator` and before `context.SaveChanges()`
- [ ] T006 Build `backend.EntityFrameworkCore` and verify compilation succeeds: `dotnet build backend/src/backend.EntityFrameworkCore/backend.EntityFrameworkCore.csproj`

**Checkpoint**: Foundation ready — `DefaultCategoriesCreator` compiles and is wired. Run `dotnet run --project backend/src/backend.Migrator -- -q` and verify 9 rows in the `Categories` table.

---

## Phase 3: User Story 1 — Categories and Documents Pre-loaded on First Run (Priority: P1) 🎯 MVP

**Goal**: On first Migrator run against an empty database, 9 categories and 13 `LegalDocument` stub records are created with `IsProcessed = false`.

**Independent Test**: Run `dotnet run --project backend/src/backend.Migrator -- -q` against a clean database. Query `SELECT COUNT(*) FROM "Categories"` → 9. Query `SELECT short_name, is_processed FROM "LegalDocuments" ORDER BY short_name` → 13 rows, all `is_processed = false`.

- [ ] T007 [US1] Create `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/LegalDocumentRegistrar.cs` — constructor accepts `backendDbContext`; `Create()` method:
  - Loads all seeded categories into a `Dictionary<string, Guid>` keyed by Name for FK resolution
  - Iterates `LegislationManifest.Documents`
  - Checks for existing document by `(ShortName, Year)` — skips if found (idempotency)
  - Inserts new `LegalDocument` with `IsProcessed = false`, `TotalChunks = 0`, `FileName` set to manifest value
  - Guard clause: skip gracefully if matching category Guid not found (logs a warning)
  - Purpose comment on class and `Create()`
- [ ] T008 [US1] Modify `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/InitialHostDbBuilder.cs` — add `new LegalDocumentRegistrar(_context).Create();` call after `DefaultCategoriesCreator.Create()`
- [ ] T009 [US1] Build and smoke-test: `dotnet build backend/src/backend.EntityFrameworkCore/backend.EntityFrameworkCore.csproj` — confirm zero errors

**Checkpoint**: User Story 1 complete. Run Migrator → verify 9 categories and 13 `LegalDocument` stubs exist with `is_processed = false`. This is the independently testable MVP increment.

---

## Phase 4: User Story 2 — Documents are Chunked and Embedded Correctly (Priority: P1)

**Goal**: For each `LegalDocument` with a PDF file present on disk, the ETL pipeline runs to completion, producing `DocumentChunk` and `ChunkEmbedding` records, and setting `IsProcessed = true`.

**Independent Test**: Place at least one PDF (e.g., `seed-data/legislation/bcea-1997.pdf`) and run the Migrator. Query `SELECT d.short_name, j.status, j.chunks_loaded, j.embeddings_generated FROM "IngestionJobs" j JOIN "LegalDocuments" d ON d.id = j.document_id` — confirm one row with `status = Completed` and `chunks_loaded > 0`. Query `SELECT is_processed, total_chunks FROM "LegalDocuments" WHERE short_name = 'BCEA'` → `is_processed = true`, `total_chunks > 0`.

- [ ] T010 [US2] Create `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/LegislationIngestionRunner.cs` — class implements `ITransientDependency`; constructor accepts `IIocResolver iocResolver`; public `RunAsync()` method:
  - Resolves `backendDbContext` via `iocResolver` to load all `LegalDocument` records where `IsProcessed = false`
  - Resolves `IAbpSession` and calls `Use(tenantId: null, userId: 1L)` (host admin user seeded by `HostRoleAndUserCreator`)
  - Resolves `IEtlPipelineAppService`
  - For each unprocessed document: checks if the PDF file exists under `seed-data/legislation/` or `seed-data/financial/` (document-level file existence check only — ETL service handles actual path resolution); skips and logs warning if file absent
  - Calls `await etlService.TriggerAsync(document.Id)` in a `try/catch`
  - On success: logs document title, chunks loaded, embeddings generated
  - On failure: logs document title and exception message; continues to next document
  - Guard clause: `Guard.Against.Null(iocResolver, nameof(iocResolver))`
  - Purpose comment on class and `RunAsync()`
- [ ] T011 [US2] Modify `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/Seed/SeedHelper.cs` — update `SeedHostDb(IIocResolver iocResolver)` method to call `new LegislationIngestionRunner(iocResolver).RunAsync().GetAwaiter().GetResult()` after the existing `WithDbContext` block
- [ ] T012 [US2] Verify `backend.Migrator` project still references `backend.EntityFrameworkCore` and `backend.Core` (no new project references needed): `dotnet build backend/src/backend.Migrator/backend.Migrator.csproj`

**Checkpoint**: User Story 2 complete. With at least one PDF present, run Migrator → confirm `IngestionJob.Status = Completed`, `DocumentChunk` and `ChunkEmbedding` records created, `LegalDocument.IsProcessed = true`.

---

## Phase 5: User Story 3 — Seed Process is Idempotent (Priority: P2)

**Goal**: Running the Migrator a second time against an already-seeded database produces zero new records.

**Independent Test**: Run Migrator twice. After the second run: `SELECT COUNT(*) FROM "Categories"` = 9; `SELECT COUNT(*) FROM "LegalDocuments"` = 13; `SELECT COUNT(*) FROM "IngestionJobs"` unchanged from first run; `SELECT COUNT(*) FROM "DocumentChunks"` unchanged.

- [ ] T013 [P] [US3] Verify `DefaultCategoriesCreator.cs` already skips existing categories (idempotency built in T004) — confirm the `Name` existence check is case-insensitive using `StringComparison.OrdinalIgnoreCase` or an EF `ToLower()` query; update if needed
- [ ] T014 [P] [US3] Verify `LegalDocumentRegistrar.cs` already skips existing documents (idempotency built in T007) — confirm the `(ShortName, Year)` existence check is correct; update if needed
- [ ] T015 [US3] Verify `LegislationIngestionRunner.cs` `IsProcessed = false` guard (built in T010) prevents re-triggering ETL for already-completed documents — confirm `EtlPipelineAppService.TriggerAsync` active-job guard is a secondary safety net but the primary skip is at the runner level; update runner logic if missing

**Checkpoint**: User Story 3 complete. Run Migrator twice — second run must produce zero new rows in any seeded table.

---

## Phase 6: User Story 4 — Seed Process Provides Progress Feedback (Priority: P3)

**Goal**: Operators can determine the outcome of each document's processing solely from the Migrator console/log output.

**Independent Test**: Run Migrator with at least one valid PDF and one missing PDF. Confirm console output contains:
- `[INFO] LegislationIngestionRunner: '{title}' — N chunks, N embeddings` for successful documents
- `[WARN] LegislationIngestionRunner: PDF file not found for '{title}' ('{fileName}') — skipping` for missing files
- `[WARN] LegislationIngestionRunner: Failed to ingest '{title}' — {reason}` for any ETL failure

- [ ] T016 [P] [US4] Update `LegislationIngestionRunner.cs` — replace any raw `Console.WriteLine` calls with ABP `Logger` (inherited from `ApplicationService` base or injected via `ILogger`) using `Logger.Info(...)` and `Logger.Warn(...)` with the exact log message formats defined in `contracts/seed-manifest-contract.md` Contract 5
- [ ] T017 [P] [US4] Update `LegislationIngestionRunner.cs` — add a summary log line at the end of `RunAsync()` reporting total documents attempted, succeeded, skipped (missing file), and failed: e.g., `[INFO] LegislationIngestionRunner: Seed complete — 11 succeeded, 1 skipped (no file), 1 failed`

**Checkpoint**: User Story 4 complete. All four user stories are now independently functional.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Tests, validation, and final cleanup.

- [ ] T018 [P] Write unit tests for `DefaultCategoriesCreator` in `backend/test/backend.Tests/Seed/DefaultCategoriesCreatorTests.cs` — test: (a) inserts all 9 categories on empty DB, (b) skips existing categories (idempotency), (c) produces correct `Domain` and `SortOrder` values
- [ ] T019 [P] Write unit tests for `LegalDocumentRegistrar` in `backend/test/backend.Tests/Seed/LegalDocumentRegistrarTests.cs` — test: (a) inserts all 13 document stubs on seeded categories, (b) skips existing documents (idempotency), (c) all stubs have `IsProcessed = false` and `TotalChunks = 0`
- [ ] T020 Run full `quickstart.md` validation: place all 13 PDFs in `seed-data/`, run `dotnet run --project backend/src/backend.Migrator -- -q`, execute the 4 verification SQL queries from `quickstart.md` and confirm all expected counts
- [ ] T021 Run `dotnet build backend/backend.sln` to confirm the full solution builds cleanly with no warnings introduced by this feature

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (T003 — needs `LegislationManifest.cs`)
- **User Story 1 (Phase 3)**: Depends on Phase 2 — `LegalDocumentRegistrar` uses `LegislationManifest` and requires categories
- **User Story 2 (Phase 4)**: Depends on Phase 3 — `LegislationIngestionRunner` requires document stubs to exist
- **User Story 3 (Phase 5)**: Depends on Phase 4 — verifies idempotency of all three seed classes
- **User Story 4 (Phase 6)**: Depends on Phase 4 — adds logging to existing `LegislationIngestionRunner`
- **Polish (Phase 7)**: Depends on all user story phases complete

### User Story Dependencies

- **US1 (P1)**: Depends on Foundational (Phase 2) — categories must exist before document stubs
- **US2 (P1)**: Depends on US1 (Phase 3) — document stubs must exist before ETL runner
- **US3 (P2)**: Depends on US2 (Phase 4) — verifies idempotency end-to-end
- **US4 (P3)**: Independent of US3 — can be worked in parallel with US3 once US2 is complete

### Within Each Phase

- `LegislationManifest.cs` (T003) is a prerequisite for all seed classes — write it first
- `DefaultCategoriesCreator` (T004) before `LegalDocumentRegistrar` (T007)
- `InitialHostDbBuilder` wiring (T005, T008) after the class it wires
- `SeedHelper` modification (T011) after `LegislationIngestionRunner` (T010)
- Tests (T018, T019) can run in parallel with each other

### Parallel Opportunities

- T001, T002 (folder/gitignore setup) can run in parallel
- T013, T014 (idempotency verification) can run in parallel
- T016, T017 (logging updates) can run in parallel
- T018, T019 (unit tests) can run in parallel with each other and with T020

---

## Parallel Example: User Story 1

```text
# Run in parallel — different files, no cross-dependency:
T007: Create LegalDocumentRegistrar.cs
(T006 build check can run after T005 is done)

# Then sequentially:
T008: Wire LegalDocumentRegistrar into InitialHostDbBuilder (depends on T007)
T009: Build verification (depends on T008)
```

## Parallel Example: Polish Phase

```text
# Launch together — all different files:
T018: DefaultCategoriesCreatorTests.cs
T019: LegalDocumentRegistrarTests.cs
T020: Full quickstart.md validation
T021: Full solution build check
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001–T003)
2. Complete Phase 2: Foundational (T004–T006)
3. Complete Phase 3: User Story 1 (T007–T009)
4. **STOP and VALIDATE**: Run Migrator → confirm 9 categories + 13 document stubs in DB
5. This is the minimum needed to demonstrate the seed infrastructure works

### Incremental Delivery

1. Phase 1 + 2 → Manifest + Categories seeded (foundation)
2. Phase 3 (US1) → Document stubs seeded (MVP: catalogue is populated)
3. Phase 4 (US2) → ETL runs → documents are chunked and searchable
4. Phase 5 (US3) → Idempotency confirmed (production-safe re-runs)
5. Phase 6 (US4) → Operator visibility via logs
6. Phase 7 → Tests + validation

### Single Developer Strategy

Execute phases sequentially in order. The natural commit points are after each checkpoint.

---

## Notes

- [P] tasks = different files, no blocking dependencies between them
- `LegislationManifest.cs` is the single source of truth — do not hardcode category names or filenames anywhere else
- PDF files are **not** committed to the repo — they are placed manually in `seed-data/` per `quickstart.md`
- The existing `EtlPipelineAppService.FindSeedDataFile` already handles path resolution — `LegislationIngestionRunner` only needs to do a pre-check for operator feedback, not re-implement path lookup
- Admin user ID = 1 is guaranteed to exist after `HostRoleAndUserCreator.Create()` runs in Phase A
- `LegislationIngestionRunner` uses `.GetAwaiter().GetResult()` because `SeedHostDb` is synchronous; this is acceptable in the Migrator console context (no deadlock risk — no SynchronizationContext)
