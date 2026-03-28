# Feature Specification: RAG Domain Model

**Feature Branch**: `004-rag-domain-model`
**Created**: 2026-03-28
**Status**: Draft
**Milestone**: Setup & Data Pipeline

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Store and Categorise Legal Documents (Priority: P1)

An administrator uploads a piece of legislation (e.g., the Labour Relations Act) into the system. The document is assigned to a legal category, its full text is preserved, and the record is marked as awaiting processing.

**Why this priority**: Without storable, categorised legal documents there is nothing to chunk or embed — this is the foundational data unit of the entire RAG pipeline.

**Independent Test**: Can be fully tested by inserting a Category record, then inserting a LegalDocument linked to it, and verifying both records can be read back with all properties intact.

**Acceptance Scenarios**:

1. **Given** a Category named "Labour Law" with Domain = Legal exists, **When** a LegalDocument is created with a valid CategoryId and all required properties, **Then** the document is persisted and retrievable with its Category relationship intact.
2. **Given** a LegalDocument exists, **When** the record is queried, **Then** IsProcessed defaults to false and TotalChunks defaults to 0.
3. **Given** an invalid CategoryId is supplied, **When** a LegalDocument is created, **Then** the operation is rejected with a foreign-key constraint error.

---

### User Story 2 - Break Documents into Searchable Chunks (Priority: P2)

After a document has been stored, a processing job splits its full text into structured chunks aligned to chapters and sections. Each chunk is linked back to its parent document and assigned a sort order that preserves reading sequence.

**Why this priority**: Chunks are the unit of retrieval in a RAG system; without them, similarity search cannot operate at the required granularity.

**Independent Test**: Can be fully tested by inserting a LegalDocument, then inserting multiple DocumentChunk records referencing it, and querying chunks ordered by SortOrder.

**Acceptance Scenarios**:

1. **Given** a persisted LegalDocument, **When** DocumentChunk records are inserted with the document's Id as DocumentId, **Then** each chunk is retrievable and its Content, SectionNumber, and SortOrder are correctly stored.
2. **Given** multiple chunks for the same document, **When** queried and ordered by SortOrder, **Then** chunks are returned in the correct reading sequence.
3. **Given** an invalid DocumentId is supplied, **When** a DocumentChunk is created, **Then** the operation is rejected with a foreign-key constraint error.

---

### User Story 3 - Attach Embedding Vectors to Chunks (Priority: P3)

Once chunks have been created, the system stores a 1 536-dimension float vector alongside each chunk so that a similarity search service can retrieve semantically relevant passages given a user query.

**Why this priority**: Embeddings are the numerical representation needed for RAG retrieval; they depend on chunks existing first, making this the final layer of the pipeline.

**Independent Test**: Can be fully tested by inserting a DocumentChunk and a ChunkEmbedding linked to it, then reading the embedding back and verifying the full 1 536-element vector is stored and retrieved without loss.

**Acceptance Scenarios**:

1. **Given** a persisted DocumentChunk, **When** a ChunkEmbedding is inserted with a 1 536-element float vector and the correct ChunkId, **Then** the embedding is persisted and the full vector can be read back.
2. **Given** an invalid ChunkId is supplied, **When** a ChunkEmbedding is created, **Then** the operation is rejected with a foreign-key constraint error.
3. **Given** a ChunkEmbedding exists, **When** a similarity query is issued against the vector column, **Then** results are returned ranked by vector distance.

---

### Edge Cases

- What happens when a document is uploaded but contains no parsable text (blank PDF)?
- What happens when a chunk's Content exceeds the maximum column length?
- How does the system handle a duplicate LegalDocument with the same ActNumber and Year?
- What happens if an embedding vector is inserted with fewer or more than 1 536 dimensions?
- How does the system behave when a Category is deleted but LegalDocuments referencing it still exist?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow administrators to create Category records with Name, Icon, Domain (Legal or Financial), and SortOrder.
- **FR-002**: The system MUST allow administrators to create LegalDocument records linked to a Category, capturing Title, ShortName, ActNumber, Year, FullText, FileName, and metadata flags.
- **FR-003**: The system MUST enforce referential integrity between LegalDocument and Category (a document cannot exist without a valid Category).
- **FR-004**: The system MUST allow a processing service to create DocumentChunk records linked to a LegalDocument, capturing chapter and section metadata, Content, TokenCount, and SortOrder.
- **FR-005**: The system MUST enforce referential integrity between DocumentChunk and LegalDocument.
- **FR-006**: The system MUST allow a processing service to create ChunkEmbedding records linked to a DocumentChunk, storing a 1 536-dimension float vector.
- **FR-007**: The system MUST enforce referential integrity between ChunkEmbedding and DocumentChunk.
- **FR-008**: The system MUST support querying DocumentChunks by parent LegalDocument, ordered by SortOrder.
- **FR-009**: The system MUST expose repository interfaces for each aggregate root (Category, LegalDocument) following domain-driven design conventions.
- **FR-010**: The Domain classification (Legal, Financial) MUST be a strongly-typed enumeration value used on Category.

### Key Entities

- **Category**: Classifies legal or financial documents; identified by Name, visual Icon, Domain classification (Legal or Financial), and display SortOrder. Acts as the top-level organisational unit.
- **LegalDocument**: Represents a single piece of legislation; linked to a Category and stores full text, metadata (ActNumber, Year), processing state (IsProcessed, TotalChunks), and a reference to an uploaded PDF file.
- **DocumentChunk**: A structured fragment of a LegalDocument aligned to a chapter or section boundary; carries positional metadata (SortOrder, SectionNumber) and token count for budget-aware retrieval.
- **ChunkEmbedding**: Stores the numerical vector representation of a DocumentChunk for similarity search; kept separate from the chunk itself for storage and query performance reasons.
- **Domain (enum)**: A two-value classification (Legal, Financial) used to categorise Categories.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All four entity types can be created, read, updated, and deleted through their respective repository interfaces without data loss.
- **SC-002**: Referential integrity constraints prevent orphaned records across all three parent-child relationships (Category → LegalDocument → DocumentChunk → ChunkEmbedding).
- **SC-003**: A 1 536-element float vector can be stored and retrieved for a ChunkEmbedding with no precision loss.
- **SC-004**: A DocumentChunk query for a given document returns all chunks in SortOrder sequence with 100% accuracy.
- **SC-005**: The database migration runs to completion on a clean PostgreSQL instance and all four tables are present and structurally correct.
- **SC-006**: Test data covering at least one Category, one LegalDocument, five DocumentChunks, and five ChunkEmbeddings can be inserted and queried successfully.

## Assumptions

- The system will be operated by internal administrators or automated processing services; no public-facing end-user interface is in scope for this feature.
- The PDF file reference on LegalDocument (OriginalPdf / FileName) is stored as metadata only at this stage; actual file storage and retrieval is handled by a separate file-management service.
- Similarity search queries against embedding vectors will be handled by a dedicated search component in a later feature; this feature only defines storage.
- The 1 536-dimension vector size corresponds to a standard embedding model output and is treated as a fixed constant for schema design.
- Audit logging, soft delete, and multi-tenancy conventions are applied automatically via base entity classes.
- Category and LegalDocument are aggregate roots; DocumentChunk and ChunkEmbedding are child entities within the LegalDocument aggregate.
- The Domain enumeration is integer-backed and stored as an integer column in the database.
