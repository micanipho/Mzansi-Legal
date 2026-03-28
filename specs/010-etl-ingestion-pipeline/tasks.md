# Tasks: ETL Ingestion Pipeline with Job Tracking

**Input**: Design documents from `/specs/010-etl-ingestion-pipeline/`
**Prerequisites**: plan.md complete | spec.md complete | research.md complete | data-model.md complete | contracts/ complete | quickstart.md complete

**Tests**: Unit tests included in Polish phase (not TDD - added for constitution PR compliance).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)

---

## Phase 1: Setup (Configuration)

**Purpose**: Add the one new configuration key required before any implementation begins.

- [X] T001 Add `OpenAI:EnrichmentModel` key (value: `gpt-4o-mini`) to `backend/src/backend.Web.Host/appsettings.json` alongside existing `OpenAI:ApiKey` and `OpenAI:EmbeddingModel` keys

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Entity additions, migration, and shared DTOs/interfaces that ALL user stories depend on.

**CRITICAL**: No user story work can begin until this phase is complete.

- [X] T002 [P] Add `TriggeredByUserId` (long?), `EmbeddingsGenerated` (int default 0), and `CompletedAt` (DateTime?) fields with XML doc comments to `IngestionJob` entity in `backend/src/backend.Core/Domains/ETL/IngestionJob.cs`
- [X] T003 [P] Add `Keywords` (`[MaxLength(500)]`, string?) and `TopicClassification` (`[MaxLength(200)]`, string?) fields with XML doc comments to `DocumentChunk` entity in `backend/src/backend.Core/Domains/LegalDocuments/DocumentChunk.cs`
- [X] T004 Add `TriggeredByUserId` FK configuration to `ConfigureIngestionJobRelationships` in `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs`: HasOne\<User\>() with HasForeignKey, IsRequired(false), OnDelete(SetNull), and HasIndex
- [X] T005 Generate EF Core migration `AddEtlPipelineOrchestratorFields` from `backend/src/backend.EntityFrameworkCore` targeting `backend/src/backend.Web.Host` (adds TriggeredByUserId + FK, EmbeddingsGenerated, CompletedAt to IngestionJobs; adds Keywords + TopicClassification to DocumentChunks)
- [X] T006 Apply the migration by running `dotnet run --project backend/src/backend.Migrator` and verify the new columns exist in PostgreSQL
- [X] T007 [P] Create `IngestionJobDto` class with all fields from data-model.md (Id, DocumentId, DocumentTitle, Status, TriggeredByUserId, StartedAt, CompletedAt, ErrorMessage, all per-stage timestamps, computed TimeSpan? duration properties, ChunksProduced, ChunksLoaded, EmbeddingsGenerated, Strategy, TotalDuration) in `backend/src/backend.Application/Services/EtlPipelineService/DTO/IngestionJobDto.cs`
- [X] T008 [P] Create `IngestionJobListDto` class with list-view fields (Id, DocumentId, DocumentTitle, Status, StartedAt, CompletedAt, TotalDuration computed property, ChunksLoaded, EmbeddingsGenerated, ErrorMessage) in `backend/src/backend.Application/Services/EtlPipelineService/DTO/IngestionJobListDto.cs`
- [X] T009 [P] Create `ChunkEnrichmentResult` DTO (Keywords string default empty, TopicClassification string default "Unknown") in `backend/src/backend.Application/Services/ChunkEnrichmentService/DTO/ChunkEnrichmentResult.cs`
- [X] T010 [P] Create `IChunkEnrichmentAppService` interface (single method: `Task<ChunkEnrichmentResult> EnrichAsync(string content)`) in `backend/src/backend.Application/Services/ChunkEnrichmentService/IChunkEnrichmentAppService.cs`
- [X] T011 Create `IEtlPipelineAppService` interface with all 4 methods (TriggerAsync, RetryAsync, GetJobsAsync, GetJobAsync) with XML doc comments and exception documentation per `specs/010-etl-ingestion-pipeline/contracts/etl-pipeline-service.md` in `backend/src/backend.Application/Services/EtlPipelineService/IEtlPipelineAppService.cs`

**Checkpoint**: Foundation ready - all entities updated, migration applied, DTOs and interfaces defined. User story implementation can begin.

---

## Phase 3: User Story 1 - Trigger ETL Pipeline for Uploaded Document (Priority: P1) MVP

**Goal**: Admin can POST to `/api/app/admin/etl/trigger/{documentId}` and the full Extract -> Transform -> Enrich -> Load pipeline runs, returning a completed `IngestionJobDto`.

**Independent Test**: Upload any legislation PDF, call `POST /api/app/admin/etl/trigger/{documentId}` via Swagger, and verify the response shows `Status: 4 (Completed)` with non-zero `ChunksLoaded` and `EmbeddingsGenerated`. Check `DocumentChunks` and `ChunkEmbeddings` tables in PostgreSQL confirm new rows.

### Implementation for User Story 1

- [X] T012 [US1] Implement `ChunkEnrichmentAppService`: inject `IHttpClientFactory` and `IConfiguration` (reads `OpenAI:ApiKey` and `OpenAI:EnrichmentModel`), truncate content to 3,000 chars, POST to `https://api.openai.com/v1/chat/completions` with `gpt-4o-mini` and a JSON-extraction prompt, deserialize response JSON into keywords array + topic string, return fallback `ChunkEnrichmentResult` (Keywords="", TopicClassification="Unknown") on any exception - non-fatal in `backend/src/backend.Application/Services/ChunkEnrichmentService/ChunkEnrichmentAppService.cs`
- [X] T013 [US1] Create `EtlPipelineAppService` class skeleton: extend `ApplicationService`, implement `IEtlPipelineAppService`, inject all repositories (IngestionJob, LegalDocument, DocumentChunk, ChunkEmbedding), `IPdfIngestionAppService`, `IEmbeddingAppService`, `IChunkEnrichmentAppService`, and `IBinaryObjectManager` in `backend/src/backend.Application/Services/EtlPipelineService/EtlPipelineAppService.cs`
- [X] T014 [US1] Implement `TriggerAsync(Guid documentId)` in `EtlPipelineAppService`: guard clauses, active-job duplicate check, OriginalPdfId null check, create IngestionJob (Status=Queued, TriggeredByUserId from AbpSession), retrieve PDF bytes via IBinaryObjectManager, call PdfIngestionAppService.IngestAsync, loop over chunks calling EnrichAsync + GenerateEmbeddingAsync + save DocumentChunk + ChunkEmbedding, update IngestionJob (Completed, CompletedAt, EmbeddingsGenerated, ChunksLoaded, LoadCompletedAt), update LegalDocument (IsProcessed=true, TotalChunks), catch stage exceptions (mark Failed, CompletedAt, rethrow), return IngestionJobDto - in `backend/src/backend.Application/Services/EtlPipelineService/EtlPipelineAppService.cs`
- [X] T015 [US1] Create `EtlController` with `[Route("api/app/admin/etl")]`, `[AbpAuthorize]`, inject `IEtlPipelineAppService`, implement `[HttpPost("trigger/{documentId}")]` delegating to `TriggerAsync` in `backend/src/backend.Web.Host/Controllers/EtlController.cs`

**Checkpoint**: User Story 1 fully functional. Admins can trigger the pipeline and receive a completed IngestionJobDto in the API response.

---

## Phase 4: User Story 2 - Monitor Pipeline Progress in Real-Time (Priority: P2)

**Goal**: Admin can `GET /api/app/admin/etl/jobs` to list all jobs and `GET /api/app/admin/etl/jobs/{id}` to see per-stage durations, chunk counts, and embeddings counts for any job.

**Independent Test**: After triggering a job (US1), call `GET /api/app/admin/etl/jobs` and verify the list contains an entry with the document title and Completed status. Call `GET /api/app/admin/etl/jobs/{id}` and verify all per-stage duration fields are non-null and `EmbeddingsGenerated` matches `ChunksLoaded`.

### Implementation for User Story 2

- [X] T016 [US2] Implement `GetJobsAsync()` in `EtlPipelineAppService`: query all IngestionJobs ordered by CreationTime descending, join DocumentTitle from LegalDocument, map to `List<IngestionJobListDto>` with computed TotalDuration - in `backend/src/backend.Application/Services/EtlPipelineService/EtlPipelineAppService.cs`
- [X] T017 [US2] Implement `GetJobAsync(Guid id)` in `EtlPipelineAppService`: load IngestionJob by id (throw EntityNotFoundException if missing), join DocumentTitle, map to `IngestionJobDto` with all computed TimeSpan? duration properties - in `backend/src/backend.Application/Services/EtlPipelineService/EtlPipelineAppService.cs`
- [X] T018 [US2] Add `[HttpGet("jobs")]` and `[HttpGet("jobs/{id}")]` endpoint methods to `EtlController` delegating to `GetJobsAsync` and `GetJobAsync` in `backend/src/backend.Web.Host/Controllers/EtlController.cs`

**Checkpoint**: User Stories 1 and 2 both work independently. Admins can trigger and monitor jobs.

---

## Phase 5: User Story 3 - Recover from Failed Pipeline Job (Priority: P3)

**Goal**: Admin can `POST /api/app/admin/etl/retry/{jobId}` on a failed job to re-run the full pipeline from scratch, clearing prior partial data and returning a new completed result.

**Independent Test**: Trigger a job against a document with an invalid/missing OriginalPdfId to force a failure. Verify `GET /api/app/admin/etl/jobs/{id}` shows `Status: 5 (Failed)` with a non-null `ErrorMessage`. Fix the document's PDF reference, then `POST /api/app/admin/etl/retry/{jobId}` and verify the job returns to `Status: 4 (Completed)`.

### Implementation for User Story 3

- [X] T019 [US3] Implement `RetryAsync(Guid jobId)` in `EtlPipelineAppService`: load job (EntityNotFoundException if missing), guard Status==Failed (UserFriendlyException if not Failed), delete all DocumentChunks for DocumentId (cascades ChunkEmbeddings), reset all job fields (Status=Queued, ErrorMessage=null, EmbeddingsGenerated=0, ChunksLoaded=0, ChunksProduced=0, CompletedAt=null, all stage timestamps=null), reset LegalDocument (IsProcessed=false, TotalChunks=0), save, then delegate to internal trigger execution - in `backend/src/backend.Application/Services/EtlPipelineService/EtlPipelineAppService.cs`
- [X] T020 [US3] Add `[HttpPost("retry/{jobId}")]` endpoint method to `EtlController` delegating to `RetryAsync` in `backend/src/backend.Web.Host/Controllers/EtlController.cs`

**Checkpoint**: All 3 user stories functional. Admins can trigger, monitor, and retry pipeline jobs.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Test coverage, code quality verification, and end-to-end validation.

- [X] T021 [P] Write unit tests for `EtlPipelineAppService.TriggerAsync`: happy path (mock PdfIngestionService returns 2 chunks, mock EmbeddingService returns vectors, verify IngestionJob ends Completed with ChunksLoaded=2 and EmbeddingsGenerated=2), empty-PDF path (mock IngestAsync returns empty list, verify Status=Completed with ChunksLoaded=0), embedding-failure path (mock GenerateEmbeddingAsync throws, verify Status=Failed with non-null ErrorMessage) in `backend/test/backend.Tests/Services/EtlPipelineServiceTests.cs`
- [X] T022 [P] Write unit tests for `ChunkEnrichmentAppService.EnrichAsync`: happy path (mock HTTP returns valid JSON, verify Keywords and TopicClassification populated), failure path (mock HTTP throws, verify fallback result returned without exception) in `backend/test/backend.Tests/Services/ChunkEnrichmentServiceTests.cs`
- [X] T023 Build the solution (`dotnet build backend/backend.sln`) and run all tests (`dotnet test backend/backend.sln`), confirm zero warnings/errors
- [ ] T024 Perform end-to-end smoke test per `specs/010-etl-ingestion-pipeline/quickstart.md`: trigger pipeline via Swagger, confirm `Status=4 (Completed)`, verify `DocumentChunks` rows include non-null `Keywords` and `TopicClassification`, verify `IngestionJobs` row has `EmbeddingsGenerated > 0` and `CompletedAt` set

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies - start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 completion - **BLOCKS all user stories**
- **Phase 3 (US1)**: Depends on Phase 2 - T012 and T013 can start in parallel; T014 depends on T012 + T013; T015 depends on T014
- **Phase 4 (US2)**: Depends on Phase 2 and the EtlPipelineAppService skeleton from T013 - T016 and T017 can run in parallel; T018 depends on both
- **Phase 5 (US3)**: Depends on Phase 2 and the EtlPipelineAppService skeleton from T013 - T019 before T020
- **Phase 6 (Polish)**: Depends on all user story phases complete

### User Story Dependencies

- **US1 (P1)**: Depends on Foundational only - no dependency on US2/US3
- **US2 (P2)**: Depends on Foundational + EtlPipelineAppService class from T013 (same file, sequential edits) - independent of US3
- **US3 (P3)**: Depends on Foundational + EtlPipelineAppService class from T013 - independent of US2

### Within Each User Story

- Interfaces and DTOs (Foundational) before services
- ChunkEnrichmentAppService (T012) before EtlPipelineAppService TriggerAsync (T014)
- Service implementation before controller endpoint

### Parallel Opportunities

- T002 and T003 (entity additions) - different files
- T007, T008, T009, T010 (DTOs and IChunkEnrichment interface) - all different files
- T012 (ChunkEnrichmentAppService) and T013 (EtlPipelineAppService skeleton) - different files
- T016 and T017 (GetJobsAsync, GetJobAsync) - sequential edits to same file but logically independent methods
- T021 and T022 (unit tests) - different test files

---

## Parallel Example: User Story 1

```bash
# These can start in parallel after Phase 2 completes:
Task T012: "Implement ChunkEnrichmentAppService in backend/src/backend.Application/Services/ChunkEnrichmentService/ChunkEnrichmentAppService.cs"
Task T013: "Create EtlPipelineAppService skeleton in backend/src/backend.Application/Services/EtlPipelineService/EtlPipelineAppService.cs"

# Then sequentially:
Task T014: "Implement TriggerAsync in EtlPipelineAppService" (needs T012 + T013)
Task T015: "Create EtlController with POST /trigger/{documentId}" (needs T014)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001)
2. Complete Phase 2: Foundational (T002-T011) - **critical blocker**
3. Complete Phase 3: User Story 1 (T012-T015)
4. **STOP and VALIDATE**: Smoke test via Swagger - trigger a real document
5. Admins can now process legislation through the full ETL pipeline

### Incremental Delivery

1. Phases 1-2 -> Foundation ready
2. Phase 3 -> Trigger works (MVP!) - admins can process documents
3. Phase 4 -> Monitoring works - admins can see job status and timings
4. Phase 5 -> Retry works - admins can recover from failures
5. Phase 6 -> Tests pass, PR-ready

### Single Developer Sequence

T001 -> T002+T003 (parallel) -> T004 -> T005 -> T006 -> T007+T008+T009+T010 (parallel) -> T011 -> T012+T013 (parallel) -> T014 -> T015 -> T016+T017 -> T018 -> T019 -> T020 -> T021+T022 (parallel) -> T023 -> T024

---

## Notes

- [P] tasks = different files, no shared-state dependencies
- [US#] label maps task to spec.md user story for traceability
- `EtlPipelineAppService` is the main implementation file - US1/US2/US3 tasks each add methods to it sequentially
- `EtlController` follows the same sequential pattern
- Classes must stay <=350 lines (RULES.md) - if EtlPipelineAppService grows large, extract private stage helpers to a sibling `EtlPipelineStageHelper.cs`
- Commit after each checkpoint to preserve stable state
