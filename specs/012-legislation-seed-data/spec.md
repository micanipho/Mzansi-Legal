# Feature Specification: Legislation Seed Data Pipeline

**Feature Branch**: `012-legislation-seed-data`
**Created**: 2026-03-28
**Status**: Draft
**Input**: User description: "The platform needs pre-loaded legislation to be useful. Without seed data, the RAG pipeline has nothing to search. Create a seed method in DbMigrator or a standalone console app that downloads or reads from local copies of 13 key documents, creates Category records, runs PdfIngestionService to chunk, runs EmbeddingService to embed each chunk, saves all DocumentChunk and ChunkEmbedding records, marks each document as IsProcessed = true."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Categories and Documents are Pre-loaded on First Run (Priority: P1)

A system operator runs the seed process for the first time on a fresh environment. After completion, all 9 legal categories and 13 legislation documents are registered in the system, fully processed and searchable.

**Why this priority**: Without this, no user can search for any legal information — the RAG pipeline has no data to work with. This is the foundational requirement for the platform to be usable.

**Independent Test**: Run the seed process against an empty database. Verify that all 9 categories exist, 13 legal documents are registered with `IsProcessed = true`, and approximately 500–1,000 chunks with associated embeddings are present in the database.

**Acceptance Scenarios**:

1. **Given** an empty database, **When** the seed process is executed, **Then** 9 categories are created (Employment & Labour, Housing & Eviction, Consumer Rights, Debt & Credit, Tax, Privacy & Data, Safety & Harassment, Insurance & Retirement, Contract Analysis)
2. **Given** an empty database, **When** the seed process completes, **Then** 13 legal documents are registered and each has `IsProcessed = true`
3. **Given** an empty database, **When** the seed process completes, **Then** between 500 and 1,000 document chunks exist, each with a corresponding embedding vector

---

### User Story 2 - Documents are Chunked and Embedded Correctly (Priority: P1)

For each legislation document ingested, the seed process extracts text, splits it into meaningful chunks, generates an embedding vector for each chunk, and persists all chunks and embeddings.

**Why this priority**: Chunking and embedding are prerequisites for semantic search. If this step fails for any document, that document is unsearchable.

**Independent Test**: After seeding, query the database for chunks and embeddings belonging to a specific document (e.g., the Constitution). Confirm that multiple chunks exist and each chunk has a non-null embedding.

**Acceptance Scenarios**:

1. **Given** a valid PDF source document, **When** the seed process ingests it, **Then** the document is split into at least one chunk and each chunk has an associated embedding vector stored
2. **Given** a document already marked `IsProcessed = true`, **When** the seed process is run again, **Then** the document is skipped and no duplicate chunks or embeddings are created
3. **Given** a document source that is unavailable or unreadable, **When** the seed process encounters it, **Then** the failure is logged with the document name and the process continues with remaining documents

---

### User Story 3 - Seed Process is Idempotent (Priority: P2)

A system operator runs the seed process more than once (e.g., after a deployment or re-run). The process detects already-seeded data and does not create duplicates.

**Why this priority**: Environments may restart or re-run migrations. Duplicate categories and documents would break search and inflate the database.

**Independent Test**: Run the seed process twice in succession. Confirm the category count remains 9 and the document count remains 13 after the second run.

**Acceptance Scenarios**:

1. **Given** a fully seeded database, **When** the seed process is executed again, **Then** no new categories, documents, chunks, or embeddings are created
2. **Given** a partially seeded database (some documents processed, some not), **When** the seed process is executed, **Then** only the unprocessed documents are ingested

---

### User Story 4 - Seed Process Provides Progress Feedback (Priority: P3)

During the seed process, an operator can observe which documents are being processed and whether each step (chunking, embedding) succeeded or failed.

**Why this priority**: With 13 documents each requiring network calls for embeddings, the process may take several minutes. Operators need visibility to diagnose failures.

**Independent Test**: Run the seed process and observe console/log output. Each document should produce a log entry indicating start, chunk count, embedding count, and completion or failure.

**Acceptance Scenarios**:

1. **Given** the seed process is running, **When** a document is successfully processed, **Then** a success message including the document name and chunk count is logged
2. **Given** the seed process is running, **When** a document fails to process, **Then** an error message including the document name and reason is logged, and the process continues

---

### Edge Cases

- What happens when a PDF file is missing from the local documents folder?
- What happens when the embedding service is unavailable or returns an error mid-batch?
- How does the system handle a PDF that produces zero extractable text (scanned image)?
- What happens if a category name already exists with a different casing?
- How does the process behave if the database connection is lost mid-seed?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The seed process MUST create all 9 categories if they do not already exist: Employment & Labour, Housing & Eviction, Consumer Rights, Debt & Credit, Tax, Privacy & Data, Safety & Harassment, Insurance & Retirement, Contract Analysis
- **FR-002**: The seed process MUST register all 13 legislation documents as `LegalDocument` records, each associated with the correct category
- **FR-003**: For each document, the seed process MUST invoke the PDF ingestion service to extract and chunk the document text
- **FR-004**: For each chunk produced, the seed process MUST invoke the embedding service to generate and persist an embedding vector
- **FR-005**: Upon successful processing of a document, the seed process MUST mark it `IsProcessed = true`
- **FR-006**: The seed process MUST be idempotent — re-running it must not create duplicate categories, documents, chunks, or embeddings
- **FR-007**: The seed process MUST skip documents already marked `IsProcessed = true`
- **FR-008**: The seed process MUST log progress per document (start, chunk count, embedding count, success or failure)
- **FR-009**: A single document failure MUST NOT abort the entire seed process — remaining documents must continue to be processed
- **FR-010**: The seed process MUST read documents from a configurable local folder path, with fallback to a conventional default location
- **FR-011**: The seed process MUST be executable as part of the DbMigrator project or as a standalone invocation without manual database setup steps

### Key Entities

- **Category**: Represents a legal topic area. Key attributes: name (unique), display order. 9 categories are pre-defined.
- **LegalDocument**: Represents a single piece of legislation or guidance material. Key attributes: title, category, source reference, `IsProcessed` flag, jurisdiction, document date.
- **DocumentChunk**: A section of text extracted from a `LegalDocument`. Key attributes: content text, chunk index, parent document reference.
- **ChunkEmbedding**: A vector representation of a `DocumentChunk` used for semantic search. Key attributes: embedding vector, chunk reference.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 9 categories are present in the database after the first successful seed run
- **SC-002**: All 13 legislation documents are registered with `IsProcessed = true` after the first successful seed run
- **SC-003**: Between 500 and 1,000 document chunks are stored, each with a corresponding embedding vector
- **SC-004**: A second run of the seed process produces zero new records (full idempotency verified by record count comparison)
- **SC-005**: Any single document failure results in a logged error but does not prevent the remaining documents from being processed
- **SC-006**: Operators can determine the outcome of each document's processing (success or failure) solely from log output

## Assumptions

- Document source files (PDFs) for the 13 documents are available as local files in a designated folder at time of seeding; the seed process does not need to download them automatically
- The PDF ingestion service and embedding service are already implemented and functional (this feature is blocked by: Domain Entities #2, PDF Ingestion Service #6, Embedding Service #7)
- The database schema for `LegalDocument`, `DocumentChunk`, `ChunkEmbedding`, and `Category` already exists from prior migrations
- The seed process is a developer/operator-facing tool, not an end-user-facing feature — no UI is required
- Admin UI for uploading additional documents post-seed is out of scope for this feature; it will be addressed separately
- Documents are categorised as follows:
  - Employment & Labour: Basic Conditions of Employment Act (BCEA), Labour Relations Act (LRA)
  - Consumer Rights: Consumer Protection Act (CPA)
  - Privacy & Data: Protection of Personal Information Act (POPIA)
  - Housing & Eviction: Rental Housing Act
  - Safety & Harassment: Protection from Harassment Act
  - Debt & Credit: National Credit Act (NCA)
  - Insurance & Retirement: Financial Advisory and Intermediary Services Act (FAIS), Pension Funds Act, FSCA materials
  - Tax: Tax Administration Act, SARS tax guide
  - Contract Analysis: Constitution of South Africa
- Local document files will be stored in a `docs/legislation/` folder at the project root by default, configurable via environment variable or configuration file
- The seed process runs in a trusted environment with access to the database and embedding API credentials
