# Feature Specification: PDF Section Chunking Ingestion Service

**Feature Branch**: `008-pdf-section-chunking`
**Created**: 2026-03-28
**Status**: Draft
**Input**: User description: "Legislation PDFs need to be processed into section-level chunks for the RAG pipeline. Build a PdfIngestionService that accepts a PDF file stream, extracts text using PdfPig, chunks by SA legislation sections, falls back to fixed-size chunking, splits large sections by subsection markers, and returns DocumentChunk objects."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Ingest Legislation PDF into Section Chunks (Priority: P1)

A developer or system process submits a South African legislation PDF (e.g., a Labour Relations Act PDF) to the ingestion service. The service reads the PDF, identifies chapter and section boundaries using SA legislation formatting patterns, and returns a list of section-level document chunks — each carrying its Act name, chapter, and section metadata — ready to be stored in the RAG document store.

**Why this priority**: This is the core capability. Without it, no legislation content can be searched or retrieved through the RAG pipeline. Every downstream feature depends on ingested, structured chunks.

**Independent Test**: Can be fully tested by calling the service with a real or synthetic SA legislation PDF and asserting that the returned chunks each contain the correct section number, chapter reference, and text content.

**Acceptance Scenarios**:

1. **Given** a valid SA legislation PDF with identifiable chapters and sections, **When** the PDF is submitted to the ingestion service, **Then** the service returns one `DocumentChunk` per legal section, each containing the section number, chapter number/title, Act name, full section text, and an estimated token count.
2. **Given** a legislation PDF where fewer than 3 sections are detected using the SA legislation regex patterns, **When** the PDF is processed, **Then** the service falls back to fixed-size chunking (500-token chunks with 50-token overlap) and each returned chunk carries the Act name and an estimated token count.
3. **Given** a legislation PDF containing a section whose content exceeds 800 tokens, **When** the service processes that section, **Then** the section is split at subsection markers (e.g., `(1)`, `(2)`, `(3)`), producing multiple smaller chunks that together represent the full section, each labelled with parent section metadata.

---

### User Story 2 - Preserve Legal Metadata per Chunk (Priority: P2)

Each chunk produced by the service carries enough metadata for downstream retrieval to surface accurate, attributable legal content. A consumer of the chunk knows exactly which Act, chapter, and section the text originates from without needing to re-parse the original PDF.

**Why this priority**: Retrieval quality depends on metadata. Without section-level attribution, search results cannot link users back to authoritative legal source material.

**Independent Test**: Can be tested independently by asserting on the metadata fields of each returned chunk: Act name is non-empty, chapter reference is present where a chapter was detected, section number matches the source document.

**Acceptance Scenarios**:

1. **Given** a PDF with clearly labelled chapters and sections, **When** chunks are returned, **Then** every chunk includes: `ActName`, `ChapterNumber`, `ChapterTitle` (if present), `SectionNumber`, `SectionTitle` (if present), and `TokenCount`.
2. **Given** a PDF where chapter headers are absent but sections are present, **When** chunks are returned, **Then** `ChapterNumber` and `ChapterTitle` are empty/null for all chunks, and the service does not fail.

---

### User Story 3 - Token Count Estimation per Chunk (Priority: P3)

Each returned chunk includes an estimated token count so that the downstream embedding and storage layer can make informed decisions about splitting, batching, or discarding oversized chunks without re-processing the text.

**Why this priority**: Token counts are needed to enforce embedding model input limits. This is a supporting capability that avoids redundant processing downstream.

**Independent Test**: Can be tested by comparing the estimated token count on a chunk against an independently computed token estimate for the same text (allowing a small margin for approximation method differences).

**Acceptance Scenarios**:

1. **Given** a chunk containing typical legal section text, **When** the token count is read from the chunk, **Then** the value is a positive integer consistent with a reasonable approximation (e.g., 1 token per ~4 characters).
2. **Given** a chunk that was produced by subsection splitting of an 800+ token section, **When** the token count is read, **Then** each sub-chunk's token count is less than the original section's estimated token count.

---

### Edge Cases

- What happens when the PDF stream is empty or zero bytes? → Service returns an empty list or raises a descriptive validation error before processing.
- What happens when text extraction produces no readable text (e.g., scanned image PDF)? → Service returns an empty chunk list without throwing; the caller can decide to log or escalate.
- What happens when a section marker appears in a table of contents rather than the body? → Section detection may produce false positives; the fallback threshold (< 3 sections) provides partial mitigation.
- What happens when a section contains no subsection markers but exceeds 800 tokens? → The section is returned as a single oversized chunk; no forced mid-sentence splitting is applied.
- What happens when the Act name is not derivable from the PDF content? → The caller-supplied document name is used as the Act name.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The service MUST accept a PDF file as a stream (not a file path) so it can operate in memory-only and cloud-hosted environments.
- **FR-002**: The service MUST extract all readable text from the submitted PDF before applying any chunking strategy.
- **FR-003**: The service MUST attempt section-level chunking using regex patterns that detect SA legislation chapter boundaries (e.g., `Chapter X — Title`) and section boundaries (e.g., `Section N. Title` or `N. Title`).
- **FR-004**: Each section-level chunk MUST carry the following metadata: Act name, chapter number, chapter title (nullable), section number, section title (nullable), chunk index, chunking strategy used, and estimated token count.
- **FR-005**: If section-level detection yields fewer than 3 sections, the service MUST fall back to fixed-size chunking with a 500-token window and 50-token overlap.
- **FR-006**: Any individual section chunk exceeding 800 tokens MUST be further split at subsection markers `(N)` at line/paragraph starts, producing sub-chunks that each inherit the parent section's metadata.
- **FR-007**: The service MUST return a typed list of `DocumentChunk` objects and MUST NOT persist data directly — persistence is the caller's responsibility.
- **FR-008**: The service MUST include a token count estimate on every returned chunk, computed without requiring an external API call.
- **FR-009**: The service MUST handle an empty or unreadable PDF gracefully by returning an empty list rather than throwing an unhandled exception.
- **FR-010**: The caller MUST be able to supply an Act name as an input parameter; it is used as the primary source of the `ActName` field on all returned chunks.

### Key Entities

- **DocumentChunk**: Represents one retrievable unit of legal text. Key attributes: unique identifier, Act name, chapter number, chapter title, section number, section title, text content, estimated token count, chunk index, and chunking strategy (section-level or fixed-size).
- **ChunkStrategy**: Classification of the chunking approach applied — section-level (SA legislation regex detected sufficient sections) or fixed-size (fallback).
- **Ingestion Request**: The input to the service — a readable byte stream of the PDF plus a caller-supplied Act/document name.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A standard SA legislation PDF of 50–200 pages is fully processed and all chunks returned in under 10 seconds.
- **SC-002**: For a well-formatted SA legislation PDF, at least 90% of returned section-level chunks correspond to a single complete legal section (verifiable by spot-check against source PDF).
- **SC-003**: Zero unhandled exceptions are produced for any valid or empty PDF input; all error states result in either an empty chunk list or a descriptive catchable error.
- **SC-004**: Every chunk in the returned list has a non-null, non-empty `ActName` and a positive `TokenCount`.
- **SC-005**: Fixed-size fallback chunking activates in 100% of cases where fewer than 3 sections are detected, as verified by unit tests with synthetic inputs.
- **SC-006**: Subsection splitting is applied in 100% of cases where a detected section exceeds 800 tokens and subsection markers are present.

## Assumptions

- The service operates as a pure in-process library; it has no HTTP endpoint and is called directly by other application services.
- SA legislation PDFs are text-based (not scanned images); OCR support is out of scope for this version.
- Token count estimation uses a simple character- or word-count approximation and does not call an external tokenisation API.
- The caller supplies the Act name; the service uses it verbatim as the `ActName` on all chunks.
- Chapter-level grouping is optional — documents without explicit chapter headers are handled without error.
- Subsection markers follow the pattern `(N)` at the start of a line or paragraph, consistent with standard SA legislative drafting style.
- The `DocumentChunk` type will be defined or extended as part of this feature if it does not already exist in the RAG domain.
- Batch ingestion of multiple PDFs concurrently is out of scope; this service processes one document per call.
