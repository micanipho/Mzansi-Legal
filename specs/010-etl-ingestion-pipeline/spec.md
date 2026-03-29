# Feature Specification: ETL Ingestion Pipeline with Job Tracking

**Feature Branch**: `010-etl-ingestion-pipeline`
**Created**: 2026-03-28
**Status**: Draft
**Input**: User description: "Create IngestionJob entity and a staged ETL pipeline service with Extract, Transform, Load stages, status tracking, error handling, and retry support for legislation document processing."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Trigger ETL Pipeline for Uploaded Document (Priority: P1)

An administrator has uploaded a legislation PDF and wants to process it through the ETL pipeline to make it searchable. They trigger processing for a specific document and can immediately see the job was accepted and is now queued.

**Why this priority**: This is the primary entry point. Without the ability to trigger a job, no other functionality is accessible. This is the minimal action needed to get value from the system.

**Independent Test**: Can be fully tested by triggering a job for an existing unprocessed document and verifying a job record exists with status "Queued", delivering visibility into whether processing has started.

**Acceptance Scenarios**:

1. **Given** a legal document exists and has not been processed, **When** the admin triggers ETL processing for that document, **Then** an ingestion job is created with status "Queued", recording the triggering user and the start timestamp
2. **Given** a document is already being processed, **When** the admin attempts to trigger another ETL job for the same document, **Then** the system rejects the duplicate request and informs the admin the document is already being processed
3. **Given** a document ID that does not exist, **When** the admin triggers ETL processing, **Then** the system returns a clear "document not found" error

---

### User Story 2 - Monitor Pipeline Progress in Real-Time (Priority: P2)

An administrator has triggered an ETL job and wants to know how far along it is — which stage is currently executing, how long each stage took, and whether it has completed successfully.

**Why this priority**: Without visibility into progress, the admin cannot distinguish between a slow job and a stuck or failed one. This is the core observability problem the feature is solving.

**Independent Test**: Can be fully tested by reading job status after triggering and verifying stage-level status transitions (Extracting → Transforming → Loading → Completed) with duration values populated.

**Acceptance Scenarios**:

1. **Given** an ingestion job is in progress, **When** the admin fetches the job details, **Then** the response shows the current stage status, durations for completed stages, and the number of chunks and embeddings produced so far
2. **Given** an ingestion job has completed successfully, **When** the admin views the job, **Then** status is "Completed", all stage durations are recorded, and chunk/embedding counts are non-zero
3. **Given** the admin requests the list of all jobs, **When** they view the jobs list, **Then** each job entry shows document name, current status, start time, and total duration

---

### User Story 3 - Recover from Failed Pipeline Job (Priority: P3)

An administrator sees a job has failed mid-pipeline. They want to understand what went wrong and retry the job without losing progress from stages that already succeeded.

**Why this priority**: Without retry support, any pipeline failure requires a manual workaround. Retry from the failed stage avoids re-doing expensive completed work.

**Independent Test**: Can be tested by simulating a failure at any stage and verifying the retry endpoint resumes from the correct stage, preserving previously recorded data.

**Acceptance Scenarios**:

1. **Given** an ingestion job has status "Failed", **When** the admin retries the job, **Then** the pipeline resumes from the stage that failed, stages that had already completed are not re-run, and the job status returns to an active state
2. **Given** an ingestion job has failed with an error message, **When** the admin views the job detail, **Then** the error message is human-readable and identifies which stage failed and the cause
3. **Given** a job that is not in "Failed" status, **When** the admin attempts to retry it, **Then** the system rejects the request with an appropriate error indicating the job is not in a retryable state

---

### Edge Cases

- What happens when a PDF has no extractable text (scanned image PDF with fewer than 100 characters of extracted content)?
- How does the system handle a PDF that partially fails mid-transform (e.g., some chunks succeed, then LLM classification fails)?
- What if the embedding service is unavailable during the Load stage?
- What if the same document is triggered for ETL while a previous job for it is still in progress?
- How are very large documents handled — are there limits on the number of chunks or embedding batches?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow an administrator to trigger the ETL pipeline for any uploaded legal document that has not yet been successfully processed
- **FR-002**: The system MUST create an ingestion job record when the ETL pipeline is triggered, capturing the document reference, triggering user, and start time
- **FR-003**: The system MUST update the ingestion job status as the pipeline progresses through each stage: Queued → Extracting → Transforming → Loading → Completed
- **FR-004**: The system MUST record the duration for each stage (Extract, Transform, Load) and the total end-to-end duration upon completion
- **FR-005**: The system MUST detect scanned PDFs (less than 100 characters of extracted text) and log a warning without failing the entire job
- **FR-006**: The extraction stage MUST strip page numbers, headers, and footers from extracted text before passing it to the transform stage
- **FR-007**: The transform stage MUST parse the legal document structure into Chapters, Sections, and Subsections, and calculate token counts for each chunk
- **FR-008**: The transform stage MUST enrich each chunk with keywords and a topic classification derived from the chunk content
- **FR-009**: The load stage MUST generate a vector embedding for each transformed chunk and persist both the chunk and its embedding
- **FR-010**: Upon successful completion, the system MUST mark the source legal document as processed and record completion time and counts on the ingestion job
- **FR-011**: When any stage fails, the system MUST capture the error message, mark the job as "Failed", and halt further processing
- **FR-012**: The system MUST allow an administrator to retry a failed ingestion job from the stage where it failed, skipping already-completed stages
- **FR-013**: The system MUST expose an endpoint to list all ingestion jobs, showing document reference, status, start time, and completion time
- **FR-014**: The system MUST expose an endpoint to retrieve the full detail of a single ingestion job, including per-stage durations, error message, and output counts
- **FR-015**: The system MUST prevent duplicate active jobs for the same document (only one job in a non-terminal state at a time per document)

### Key Entities

- **IngestionJob**: Represents a single end-to-end ETL run for a legal document. Tracks lifecycle status (Queued, Extracting, Transforming, Loading, Completed, Failed), per-stage durations, output counts (chunks created, embeddings generated), error information, and the user who triggered it. Linked to a legal document.
- **LegalDocument**: The source legislation PDF being processed. Has a flag indicating whether it has been successfully processed through the pipeline.
- **DocumentChunk**: A structured fragment of the legal document created during the Transform stage. Contains parsed structure metadata (chapter/section/subsection), token count, keywords, and topic classification.
- **ChunkEmbedding**: A vector representation of a document chunk generated during the Load stage, used for semantic search.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Administrators can see the processing status of any document within 2 seconds of triggering the pipeline
- **SC-002**: A failed pipeline job provides a clear, human-readable error message identifying the stage and cause, visible in the job detail view
- **SC-003**: Retrying a failed job successfully resumes from the correct stage in 100% of cases where the underlying failure condition has been resolved
- **SC-004**: Administrators can identify which pipeline stage is the slowest for any given document by reviewing per-stage durations on the job detail
- **SC-005**: A stage failure does not result in partial or corrupted data — either a stage completes fully or its output is not persisted
- **SC-006**: The jobs list returns all ingestion jobs within 3 seconds regardless of the total number of job records

## Assumptions

- Processing is synchronous for MVP — the admin triggers a job and the pipeline runs immediately in the request thread; background queue support is out of scope for this iteration
- Only administrators can trigger and monitor ETL jobs; regular end-users have no access to ingestion job data
- The lightweight LLM call for keyword and topic classification in the Transform stage uses the same OpenAI integration already present in the project
- A "retry" resumes from the failed stage; it does not re-run already-completed stages — the system stores intermediate results (e.g., extracted text) between stage transitions to enable this
- The system does not support cancelling an in-progress job; only jobs in "Failed" status can be retried
- Per-stage durations are wall-clock times measured within the pipeline service, not via external monitoring infrastructure
- Documents that are scanned-only (no extractable text) proceed through the pipeline but produce zero chunks; the admin is warned via the error message on the job, not blocked from triggering
- The ETL trigger endpoint is admin-only and protected by the existing role-based access control in the application
