# Tasks: PDF Section Chunking Ingestion Service

**Input**: Design documents from `/specs/008-pdf-section-chunking/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Add PdfPig dependency and create the ETL domain folder structure.

- [x] T001 Add UglyToad.PdfPig NuGet package to `backend/src/backend.Application/backend.Application.csproj` via `dotnet add package UglyToad.PdfPig` from `backend/src/backend.Application/`
- [x] T00X Create empty domain folder `backend/src/backend.Core/Domains/ETL/` to house the new IngestionJob aggregate

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Domain types, DTOs, service interface, and database migration that ALL user stories depend on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T00X [P] Create `ChunkStrategy` enum in `backend/src/backend.Core/Domains/LegalDocuments/ChunkStrategy.cs` with values `SectionLevel = 0` and `FixedSize = 1` and a purpose XML comment per `docs/RULES.md`
- [x] T00X [P] Create `IngestionStatus` enum in `backend/src/backend.Core/Domains/ETL/IngestionStatus.cs` with values `Queued=0, Extracting=1, Transforming=2, Loading=3, Completed=4, Failed=5` and a purpose XML comment
- [x] T00X [P] Create `IngestionJob` entity in `backend/src/backend.Core/Domains/ETL/IngestionJob.cs` extending `FullAuditedEntity<Guid>` with all properties from data-model.md: `DocumentId`, `Status`, all six stage timestamps (`ExtractStartedAt/CompletedAt`, `TransformStartedAt/CompletedAt`, `LoadStartedAt/CompletedAt`), `ExtractedCharacterCount`, `ChunksProduced`, `ChunksLoaded`, `Strategy` (nullable `ChunkStrategy?`), `ErrorMessage` (MaxLength 2000) — add XML comments on all properties
- [x] T00X Add `ChunkStrategy?` property to the existing `DocumentChunk` entity at `backend/src/backend.Core/Domains/LegalDocuments/DocumentChunk.cs` with a purpose XML comment explaining it is null for chunks created before this feature
- [x] T00X [P] Create `IngestPdfRequest` DTO in `backend/src/backend.Application/Services/PdfIngestionService/DTO/IngestPdfRequest.cs` with properties: `PdfStream` (Stream, Required), `ActName` (string, Required), `DocumentId` (Guid, Required), `IngestionJobId` (Guid, Required) — add class and property XML comments
- [x] T00X [P] Create `DocumentChunkResult` DTO in `backend/src/backend.Application/Services/PdfIngestionService/DTO/DocumentChunkResult.cs` with all properties from data-model.md: `ActName`, `ChapterTitle` (nullable), `SectionNumber` (nullable), `SectionTitle` (nullable), `Content`, `TokenCount`, `SortOrder`, `Strategy` (`ChunkStrategy`) — add class and property XML comments
- [x] T00X Register `IngestionJob` in `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs`: add `DbSet<IngestionJob> IngestionJobs` with summary comment; add `ConfigureIngestionJobRelationships(modelBuilder)` call in `OnModelCreating`; implement the private method with Restrict delete from LegalDocument, indexes on `DocumentId` and `Status` — follow existing method patterns (e.g., `ConfigureContractAnalysisRelationships`)
- [x] T0XX Create `IPdfIngestionAppService` interface in `backend/src/backend.Application/Services/PdfIngestionService/IPdfIngestionAppService.cs` extending `IApplicationService` with single method `Task<IReadOnlyList<DocumentChunkResult>> IngestAsync(IngestPdfRequest request)` — add XML comments on interface and method
- [x] T0XX Generate EF migration by running `dotnet ef migrations add AddPdfIngestionEntities --project src/backend.EntityFrameworkCore --startup-project src/backend.Web.Host` from `backend/` directory — verify the generated migration creates `IngestionJobs` table and adds `ChunkStrategy` column to `DocumentChunks`
- [x] T0XX Apply the migration to the local PostgreSQL database via `dotnet ef database update --project src/backend.EntityFrameworkCore --startup-project src/backend.Web.Host` from `backend/`

**Checkpoint**: Foundation ready — `IngestionJob` table exists, `DocumentChunks.ChunkStrategy` column exists, interface is defined. User story implementation can now begin.

---

## Phase 3: User Story 1 — Ingest Legislation PDF into Section Chunks (Priority: P1) 🎯 MVP

**Goal**: Accept a PDF stream, detect SA legislation chapter/section boundaries, return a correctly ordered list of `DocumentChunkResult` objects (one per section), and track all three pipeline stages on the `IngestionJob`.

**Independent Test**: Call `IngestAsync` with a synthetic SA legislation text (≥3 sections), assert returned list is non-empty, each item has non-null `Content`, `SortOrder` is sequential from 0, `Strategy = SectionLevel`. Call with a plain-text PDF with <3 detectable sections and assert `Strategy = FixedSize`. Call with an empty stream and assert empty list returned without exception.

### Implementation for User Story 1

- [x] T0XX [US1] Create `PdfIngestionAppService` class skeleton in `backend/src/backend.Application/Services/PdfIngestionService/PdfIngestionAppService.cs` extending `ApplicationService` and implementing `IPdfIngestionAppService`: inject `IRepository<IngestionJob, Guid>` constructor parameter; declare all five private method signatures (`ExtractTextAsync`, `DetectSections`, `BuildSectionChunks`, `BuildFixedSizeChunks`, `SplitLargeSectionBySubsections`); declare the four named `const int` fields from research.md (`MinSectionsForAuto=3`, `MaxTokensPerChunk=800`, `FixedChunkTokens=500`, `OverlapTokens=50`, `CharsPerTokenEstimate=4`); add class XML comment
- [x] T0XX [US1] Implement `ExtractTextAsync(Stream pdfStream) → string` in `PdfIngestionAppService`: open stream with `PdfDocument.Open(pdfStream)`, iterate pages accumulating `page.Text` into a `StringBuilder`, return the full text string; if PdfPig throws, catch, set `IngestionJob.Status = Failed` and `ErrorMessage`, save job, re-throw — add method XML comment
- [x] T0XX [P] [US1] Implement `DetectSections(string fullText) → IReadOnlyList<DetectedSection>` in `PdfIngestionAppService`: define a private `record DetectedSection(string? ChapterTitle, string SectionNumber, string? SectionTitle, int StartIndex, int EndIndex)` in the same file; apply the `ChapterPattern` and `SectionPattern` compiled regex from research.md to find all matches; pair each section match with the most recent chapter match; return ordered list of `DetectedSection` records — add method and record XML comments
- [x] T0XX [US1] Implement `BuildSectionChunks(IReadOnlyList<DetectedSection> sections, string actName, string fullText) → List<DocumentChunkResult>` in `PdfIngestionAppService`: for each section slice the text between its `StartIndex` and the next section's `StartIndex`; set `Strategy = SectionLevel`; call `SplitLargeSectionBySubsections` if the slice exceeds `MaxTokensPerChunk` tokens; otherwise create one `DocumentChunkResult`; assign sequential `SortOrder` (0-based) across all produced chunks — add method XML comment
- [x] T0XX [US1] Implement `BuildFixedSizeChunks(string fullText, string actName) → List<DocumentChunkResult>` in `PdfIngestionAppService`: compute `windowChars = FixedChunkTokens * CharsPerTokenEstimate` and `stepChars = (FixedChunkTokens - OverlapTokens) * CharsPerTokenEstimate`; slide a window across `fullText`; create one `DocumentChunkResult` per window with `Strategy = FixedSize`, null metadata fields, sequential `SortOrder` — add method XML comment
- [x] T0XX [US1] Implement `SplitLargeSectionBySubsections(DetectedSection section, string sectionText, string actName, int sortOrderBase) → List<DocumentChunkResult>` in `PdfIngestionAppService`: apply `SubsectionPattern` compiled regex to find `(N)` markers; split text at each marker; if no subsection markers found return a single chunk (the full section even if oversized); assign `SortOrder` from `sortOrderBase` incrementing per sub-chunk; all sub-chunks inherit `ChapterTitle`, `SectionNumber`, `SectionTitle` from parent section — add method XML comment
- [x] T0XX [US1] Implement `IngestAsync(IngestPdfRequest request) → Task<IReadOnlyList<DocumentChunkResult>>` in `PdfIngestionAppService`: add guard clauses (`Guard.Against.Null(request)`, `Guard.Against.Null(request.PdfStream)`, `Guard.Against.NullOrWhiteSpace(request.ActName)`) at top; transition job through `Extracting → Transforming → Loading` stages (set timestamps, save after each transition); call `ExtractTextAsync`; if `string.IsNullOrWhiteSpace` return empty list; call `DetectSections`; branch on `sections.Count < MinSectionsForAuto` to choose strategy; set `job.ChunksProduced`; update `job.Status = Loading`, `job.LoadStartedAt`; return chunk list — add method XML comment
- [x] T0XX [US1] Add `AbpAuthorize` attribute to `PdfIngestionAppService` class to enforce authentication, consistent with existing services such as `LegalDocumentAppService`

**Checkpoint**: `PdfIngestionAppService.IngestAsync` is callable. It produces section-level chunks for SA legislation PDFs and fixed-size chunks for unstructured text. IngestionJob transitions through all stages. Empty stream returns empty list.

---

## Phase 4: User Story 2 — Preserve Legal Metadata per Chunk (Priority: P2)

**Goal**: Ensure every returned `DocumentChunkResult` carries correctly populated `ActName`, `ChapterTitle`, `SectionNumber`, and `SectionTitle` fields that accurately reflect the source legislation structure.

**Independent Test**: Call `IngestAsync` with a synthetic PDF containing two chapters and four sections. Assert each chunk's `ActName` equals the supplied Act name. Assert chunks in Chapter 1 have non-null `ChapterTitle` containing "Chapter 1". Assert chunks in Chapter 2 have a different non-null `ChapterTitle`. Assert `SectionNumber` on each chunk matches the section heading. Call with a PDF with no chapter headers and assert `ChapterTitle` is null on all chunks without service failure.

### Implementation for User Story 2

- [x] T0XX [US2] Verify `ActName` propagation in `BuildSectionChunks` and `BuildFixedSizeChunks`: confirm both methods copy `actName` parameter verbatim to every `DocumentChunkResult.ActName` — no changes needed if US1 implementation is correct; add an inline comment noting the non-empty guarantee from the guard clause in `IngestAsync`
- [x] T0XX [P] [US2] Verify chapter context propagation in `DetectSections`: confirm the most recently matched `ChapterPattern` group(1+2) is stored as `ChapterTitle` on each `DetectedSection`; for sections before any chapter match, `ChapterTitle` must be null (not throw); add a targeted comment in `DetectSections` documenting the null-chapter scenario
- [x] T0XX [P] [US2] Verify `SectionNumber` and `SectionTitle` extraction accuracy in `DetectSections`: confirm `SectionPattern` group(1) maps to `SectionNumber` and group(2) maps to `SectionTitle`; handle trailing whitespace trimming on both captured groups; add inline comment explaining the regex group mapping
- [x] T0XX [US2] Verify fixed-size chunks carry null metadata gracefully: confirm that `DocumentChunkResult` objects produced by `BuildFixedSizeChunks` have `ChapterTitle = null`, `SectionNumber = null`, `SectionTitle = null`, and `Strategy = FixedSize` — consumer code must handle these nulls; add a code comment in `BuildFixedSizeChunks` documenting this expected null contract

**Checkpoint**: Every chunk type (section-level and fixed-size) carries correct metadata. Chapter-absent PDFs are handled without errors.

---

## Phase 5: User Story 3 — Token Count Estimation per Chunk (Priority: P3)

**Goal**: Every `DocumentChunkResult` returned by `IngestAsync` carries a positive `TokenCount` value computed by the character-approximation formula `(content.Length + 3) / 4`. This count drives the 800-token section splitting threshold and the fixed-size window sizing.

**Independent Test**: Create two `DocumentChunkResult` instances manually using the `EstimateTokenCount` helper; assert their `TokenCount` values match `(content.Length + 3) / 4`. Call `IngestAsync` with a large section (>3200 chars ≈ >800 tokens) and assert that section was split into multiple sub-chunks each with `TokenCount < 800`. Assert no returned chunk has `TokenCount = 0`.

### Implementation for User Story 3

- [x] T0XX [US3] Implement `EstimateTokenCount(string text) → int` private method in `PdfIngestionAppService`: return `(text.Length + 3) / CharsPerTokenEstimate`; add XML comment explaining the formula and the `+3` rounding convention; note that the method never returns 0 for non-empty text because the minimum value for a 1-char string is 1
- [x] T0XX [US3] Apply `EstimateTokenCount` to all `DocumentChunkResult` assignments: update `BuildSectionChunks`, `BuildFixedSizeChunks`, and `SplitLargeSectionBySubsections` to call `EstimateTokenCount(chunk.Content)` when setting `TokenCount` on each result — ensure no chunk leaves with `TokenCount = 0`
- [x] T0XX [US3] Use `EstimateTokenCount` for the 800-token threshold check in `BuildSectionChunks`: replace any raw character comparison with `EstimateTokenCount(sectionText) > MaxTokensPerChunk` to trigger `SplitLargeSectionBySubsections` — ensures the splitting logic uses the same formula as the reported token count
- [x] T0XX [US3] Record `ChunksProduced` using the final chunk list count in `IngestAsync` after the strategy branch resolves: `job.ChunksProduced = chunks.Count` — this value is visible in the admin dashboard and must reflect the actual split results including subsection splits

**Checkpoint**: All three user stories are complete. Every chunk carries positive `TokenCount`. Large sections are split. The `IngestionJob.ChunksProduced` field reflects subsection splits.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Wire the service into the document upload flow, validate the full pipeline end-to-end, and ensure code quality compliance.

- [x] T0XX [P] Update `LegalDocumentAppService` in `backend/src/backend.Application/Services/LegalDocumentService/LegalDocumentAppService.cs` to inject `IPdfIngestionAppService` and `IRepository<IngestionJob, Guid>`; add a `TriggerIngestionAsync(Guid documentId, Stream pdfStream, string actName)` method that creates the `IngestionJob` (Status=Queued), calls `IngestAsync`, persists each returned chunk as a `DocumentChunk`, then updates `LegalDocument.IsProcessed = true` and `LegalDocument.TotalChunks`, and sets `job.Status = Completed` — follow the caller responsibilities defined in `contracts/pdf-ingestion-service.md`
- [x] T0XX [P] Code quality review of `PdfIngestionAppService` against `docs/RULES.md`: verify class is ≤350 lines (refactor if over); verify no method requires vertical scrolling; verify all `const` values used instead of magic numbers; verify nesting ≤2 levels in all methods; run `Ctrl+E, Ctrl+D` format before committing
- [x] T0XX Run the manual smoke test from `quickstart.md` against the local PostgreSQL container (`mzansi-pg`) using a real SA legislation PDF — verify chunks are created in the database with correct metadata, `IngestionJob` progresses to `Completed`, `LegalDocument.IsProcessed = true`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 completion — BLOCKS all user stories
  - T003–T005 can run in parallel (different files)
  - T006 depends on T003 (ChunkStrategy enum)
  - T007–T008 can run in parallel
  - T009 depends on T004 + T005 (IngestionJob + IngestionStatus)
  - T010 depends on T007 + T008 (DTOs)
  - T011 depends on T003–T010 (all entities registered)
  - T012 depends on T011 (migration generated)
- **Phase 3 (US1)**: Depends on Phase 2 completion
  - T013 first (class skeleton with constants and field declarations)
  - T014 + T015 can run in parallel after T013
  - T016–T018 depend on T015 (DetectedSection record)
  - T019 depends on T014 + T016 + T017 + T018
  - T020 after T019
- **Phase 4 (US2)**: Depends on Phase 3 completion; T022–T023 can run in parallel
- **Phase 5 (US3)**: Depends on Phase 3 completion; T026–T027 depend on T025
- **Phase 6 (Polish)**: Depends on Phases 3–5 completion

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2 — no dependency on US2 or US3
- **US2 (P2)**: Can start after Phase 3 — verifies metadata correctness in existing code paths
- **US3 (P3)**: Can start after Phase 3 — adds `EstimateTokenCount` helper and applies it

### Within Each Phase

- Enums and entities before DbContext registration
- DbContext registration before migration
- Migration before any service that writes to DB
- Class skeleton before method implementations
- Private helpers before callers within the service

---

## Parallel Execution Examples

### Phase 2 (Foundational) — Parallel group 1

```
Parallel start:
  Task T003: ChunkStrategy enum in backend.Core/Domains/LegalDocuments/ChunkStrategy.cs
  Task T004: IngestionStatus enum in backend.Core/Domains/ETL/IngestionStatus.cs
  Task T005: IngestionJob entity in backend.Core/Domains/ETL/IngestionJob.cs
  Task T007: IngestPdfRequest DTO in backend.Application/.../DTO/IngestPdfRequest.cs
  Task T008: DocumentChunkResult DTO in backend.Application/.../DTO/DocumentChunkResult.cs
→ Then T006 (extends DocumentChunk, needs T003) + T009 (DbContext, needs T004+T005)
→ Then T010 (interface, needs T007+T008)
→ Then T011 (migration, needs all above)
→ Then T012 (apply migration)
```

### Phase 3 (US1) — Parallel group

```
T013 first (skeleton + constants)
→ Parallel:
     T014: ExtractTextAsync (PdfPig usage, solo file area)
     T015: DetectSections (regex patterns, DetectedSection record)
→ T016 (BuildSectionChunks, needs T015's record type)
→ T017 and T018 can run in parallel after T015
→ T019 (IngestAsync orchestration, needs all above)
→ T020 (AbpAuthorize attribute, trivial, after T019)
```

### Phase 4 + 5 — Parallel start

```
After Phase 3 completes:
  → Phase 4 (US2): T021–T024 (metadata verification, same file as service but read-only review tasks)
  → Phase 5 (US3): T025–T028 (token count helper + apply)
  (phases 4 and 5 touch the same service file — sequence them rather than true parallelism)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete **Phase 1**: Setup — install PdfPig, create ETL folder
2. Complete **Phase 2**: Foundational — all enums, entities, migration, interface
3. Complete **Phase 3**: User Story 1 — full ingestion pipeline with IngestionJob tracking
4. **STOP and VALIDATE**: Call `IngestAsync` with a real SA legislation PDF; assert chunks are returned and `IngestionJob` reaches `Completed`
5. Deliver as a working ingestion pipeline; metadata and token count are included by US1 tasks

### Incremental Delivery

1. **Phase 1 + 2** → Database ready, interface defined
2. **Phase 3 (US1)** → Chunks produced from PDF — core value delivered
3. **Phase 4 (US2)** → Metadata verified correct — retrieval quality assured
4. **Phase 5 (US3)** → Token counts reliable — downstream embedding pipeline unblocked
5. **Phase 6** → Fully wired into document upload flow

---

## Notes

- `[P]` tasks write to different files and can be assigned to different developers or parallel agent sessions
- Every user story is independently verifiable using the test criteria stated in each phase header
- The `IngestionJob` stage tracking in T019 is mandatory per the ETL/Ingestion Gate — do not skip
- All `const` values (`MinSectionsForAuto`, `MaxTokensPerChunk`, `FixedChunkTokens`, `OverlapTokens`, `CharsPerTokenEstimate`) must be named constants, never inline literals — enforced by `docs/RULES.md`
- `PdfIngestionAppService` must stay ≤350 lines; if `BuildSectionChunks` + `SplitLargeSectionBySubsections` + `DetectSections` together push the class over the limit, extract section/subsection helpers to a `PdfChunkingHelper` static class in the same service folder
- Commit after each phase checkpoint, not after every individual task
