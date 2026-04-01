# Feature Specification: RAG Question-Answering with Legal Citations

**Feature Branch**: `014-rag-qa-service`
**Created**: 2026-03-30
**Status**: Draft
**Input**: User description: "Build a RagService that loads chunk embeddings, performs cosine similarity search, and generates cited answers using an LLM"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Legal Question Answered with Citations (Priority: P1)

A South African resident asks a legal question in plain English (e.g., "Can my landlord evict me?"). The platform retrieves the most relevant sections of legislation, generates a clear answer in natural language, and cites the exact Act name and section number for every claim made.

**Why this priority**: This is the core intelligence of the application. Without cited answers, the platform provides no legal value and cannot be trusted.

**Independent Test**: Submit the question "Can my landlord evict me?" and verify the response references Section 26(3) of the Constitution with accurate, cited text.

**Acceptance Scenarios**:

1. **Given** a user submits "Can my landlord evict me?", **When** the platform processes the question, **Then** it returns an answer that explicitly cites the relevant Act and section number (e.g., "Constitution of the Republic of South Africa, Section 26(3)").
2. **Given** a user asks about tenant rights, **When** relevant legislation exists in the system, **Then** the answer contains only information found in that legislation — no fabricated or uncited claims.
3. **Given** the platform returns an answer, **When** the user reads the response, **Then** they can see a structured list of cited sources (Act name + section number) alongside the answer.

---

### User Story 2 - Insufficient Information Response (Priority: P2)

A user asks a legal question on a topic not covered by the legislation loaded into the platform. Instead of hallucinating an answer, the platform clearly states that it does not have sufficient information to answer.

**Why this priority**: Providing incorrect legal information is worse than no information. The "I don't know" path protects users from acting on fabricated legal advice.

**Independent Test**: Submit a question on a topic not covered by any loaded legislation (e.g., aviation law if only housing and labour law is loaded). Verify the platform responds with a clear "insufficient information" message rather than a speculative answer.

**Acceptance Scenarios**:

1. **Given** a user submits a question on an uncovered legal topic, **When** no sufficiently relevant legislation is found, **Then** the platform responds with a clear, friendly statement that it does not have enough information to answer.
2. **Given** retrieved legislation is only marginally related to the question, **When** relevance falls below the required threshold, **Then** the system does not attempt to answer from irrelevant content.

---

### User Story 3 - Answer Available Immediately on First Query (Priority: P3)

A user submits their first question immediately after the application starts. The platform answers without any per-query cold-start delay, because all legislation content is pre-loaded at startup.

**Why this priority**: Perceived performance on the first query matters for user trust. A long initial delay creates a poor first impression for a legal assistant.

**Independent Test**: Restart the application, then immediately submit a question. Verify the response time is within the standard target without a warm-up penalty.

**Acceptance Scenarios**:

1. **Given** the application has just started, **When** a user immediately submits a question, **Then** the response time is within the standard target — no extra delay compared to subsequent queries.
2. **Given** the application is running, **When** multiple users submit questions concurrently, **Then** each receives a cited response without degraded response times.

---

### Edge Cases

- What happens when the user submits an empty or whitespace-only question?
- How does the system handle questions that are ambiguous or very broad (e.g., "What are my rights?")?
- What if no legislation chunk in the entire corpus meets the minimum relevance threshold for any question?
- What if the AI text generation service is temporarily unavailable during a query?
- How does the system handle duplicate citations when the same section of legislation is retrieved multiple times?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST accept a natural language legal question from a user and return a generated answer.
- **FR-002**: System MUST identify the most relevant sections of legislation for each question, returning up to 5 sections.
- **FR-003**: System MUST only include legislation sections that meet a minimum relevance threshold when generating answers — sections below this threshold MUST be excluded.
- **FR-004**: System MUST generate answers grounded exclusively in the retrieved legislation content — no claims may be made without a corresponding cited source.
- **FR-005**: System MUST cite the specific Act name and section number for every piece of information included in the answer.
- **FR-006**: System MUST respond with a clear "insufficient information" message when no sufficiently relevant legislation is found.
- **FR-007**: System MUST return, alongside the answer, a structured list of citation objects identifying the sources used (Act name and section number).
- **FR-008**: System MUST return the identifiers of the specific legislation chunks used to generate the answer, for traceability and auditability.
- **FR-009**: System MUST have all legislation content ready for retrieval at application startup, without requiring manual triggering or a per-query loading step.
- **FR-010**: System MUST prioritise factual accuracy over conversational fluency when generating answers.

### Key Entities

- **Question**: The natural language query submitted by the user, expressed in English.
- **LegislationChunk**: A discrete section of a piece of legislation, associated with an Act name, section number, and text content. Chunks are pre-computed and stored by the ingestion pipeline.
- **RelevanceScore**: A numerical measure of how closely a legislation chunk matches a given question. Only chunks above a defined minimum threshold are considered.
- **Answer**: The generated natural-language response to the user's question, composed exclusively from retrieved legislation content.
- **Citation**: A reference identifying the Act name and section number of a legislation chunk used to support a claim in the answer.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users receive a cited answer to a legal question within 10 seconds of submission under normal operating conditions.
- **SC-002**: 100% of generated answers include at least one citation when relevant legislation is found; no answer is provided without a corresponding cited source.
- **SC-003**: The system correctly declines to answer questions where no relevant legislation meets the relevance threshold, with a clear and friendly message (verifiable by testing with out-of-scope topics).
- **SC-004**: The platform correctly answers "Can my landlord evict me?" with a response citing Section 26(3) of the Constitution of the Republic of South Africa.
- **SC-005**: Zero hallucinations: no answer contains claims that are not traceable to a specific returned citation, verifiable by cross-checking answer content against cited sections.
- **SC-006**: The system correctly handles at least 95% of submitted questions (returns an answer or a clean decline) without errors or system failures.

## Assumptions

- Questions are submitted in English; multilingual question support (Zulu, Sotho, Afrikaans) is a future milestone and is out of scope for this feature.
- The legislation corpus (~1,000 chunks) has already been ingested and stored by the prior ETL ingestion pipeline; this feature consumes that data without modifying it.
- The corpus is small enough to be held entirely in memory for fast retrieval, without needing a dedicated vector database (appropriate for MVP scale).
- An external AI service is available for both question embedding and answer generation; API credentials and connectivity are managed separately as infrastructure configuration.
- The system operates in a read-only capacity with respect to legislation data — it does not ingest, update, or delete chunks.
- The feature covers the retrieval-and-generation pipeline only; the user interface for submitting questions and displaying answers is a separate concern.
- Users are assumed to have a stable internet connection sufficient for the platform to communicate with external AI services.
