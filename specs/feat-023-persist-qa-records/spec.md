# Feature Specification: Persist Q&A Interaction Records

**Feature Branch**: `feat/023-persist-qa-records`
**Created**: 2026-04-02
**Status**: Draft
**Input**: User description: "Every question and answer needs to be persisted for user history, admin analytics, and the FAQ system. Without storage, we lose all interaction data."

## User Scenarios & Testing _(mandatory)_

### User Story 1 - Full Interaction Record Saved on Ask (Priority: P1)

A user submits a legal question through the Q&A interface. Once the system produces an
answer, the entire interaction — the question, the generated answer, and all the document
citations that supported the answer — is saved and linked together. A subsequent inspection
of the data confirms that every piece of the conversation is stored and correctly associated.

**Why this priority**: This is the foundational requirement. Without capturing the
question-and-answer pair in storage, no other feature (history, analytics, FAQ) has data
to work from. All downstream value depends on this being reliable from day one.

**Independent Test**: Submitting any question through the Q&A interface and then
verifying via the administrative data view (or direct database inspection) that records
for the conversation, question, answer, and each citation all exist and are properly linked.

**Acceptance Scenarios**:

1. **Given** a user submits a question in English, **When** the system returns an answer, **Then** a Conversation record, a Question record (with original text, detected language, and input method), an Answer record (with response text and language), and one or more AnswerCitation records (each linked to a specific document chunk) are all saved and linked together under the same conversation.
2. **Given** a user submits a question in a language other than English (e.g., Zulu), **When** the system translates and answers it, **Then** the Question record stores both the original text and the translated text, and the Answer record stores the response in the appropriate language.
3. **Given** the system is unable to store the records after generating an answer, **When** the error occurs, **Then** the user still receives the answer but an error is logged for the failed persistence, and no partial/orphaned records are left in storage.

---

### User Story 2 - Continuing an Existing Conversation (Priority: P2)

A returning user asks a follow-up question within the same session. The system associates
the new question-and-answer with the existing session rather than creating a duplicate
conversation.

**Why this priority**: Multi-turn conversations are a key UX pattern and a prerequisite
for coherent history views. Without this, each follow-up appears as a new unrelated
session, making history unusable.

**Independent Test**: Submitting two questions in the same session, then verifying that
both question-answer pairs are stored under one Conversation record rather than two
separate Conversation records.

**Acceptance Scenarios**:

1. **Given** a user has an existing conversation session, **When** they ask a follow-up question, **Then** the new Question and Answer records are linked to the existing Conversation record rather than a new one.
2. **Given** an invalid or expired conversation ID is provided, **When** a new question is submitted, **Then** the system creates a new Conversation record and proceeds normally.

---

### User Story 3 - Admin Analytics Access to Stored Interactions (Priority: P3)

An administrator reviews interaction data to understand usage patterns, frequently asked
questions, and citation sources over time. The stored records provide sufficient structure
for querying and reporting.

**Why this priority**: Analytics access is a secondary consumer of the data. The data
must be stored correctly first (P1/P2) before analytics queries are meaningful. However,
the schema must be designed with analytics in mind from the start to avoid costly migrations.

**Independent Test**: After multiple questions are asked, an administrator can retrieve
a list of all interactions (conversations, questions, answers, and citations) filtered by
date range or language, confirming that the stored schema supports these access patterns.

**Acceptance Scenarios**:

1. **Given** questions have been recorded over multiple days, **When** an admin queries interactions by date range, **Then** all conversations, associated questions, answers, and citations within that range are retrievable.
2. **Given** questions in multiple languages have been stored, **When** an admin filters by language, **Then** only records matching that language are returned.

---

### Edge Cases

- **Storage failure after answer generation**: If persistence fails after the answer has been delivered to the user, the system must not surface a storage error to the user but must log it internally. The user experience must not be degraded.
- **Question with no citations**: Some answers may not have supporting document citations (e.g., general guidance). The system must still create valid Conversation, Question, and Answer records; zero AnswerCitation records is a valid and expected outcome.
- **Duplicate conversation collision**: If two concurrent requests provide the same conversation ID, the system must handle this gracefully without creating duplicate Conversation records or corrupting existing data.
- **Very long question text**: Questions exceeding typical character budgets must be accepted and stored in full without truncation; any storage limits must be defined and enforced with a clear user-facing error.
- **POPIA / Personal information**: Questions may contain personal information (names, ID numbers, case references). Records must be stored in a way that supports future deletion or de-identification requests in compliance with South African data protection requirements.

## Requirements _(mandatory)_

### Functional Requirements

- **FR-001**: The system MUST create and persist a Conversation record when a new question is asked without an existing conversation identifier.
- **FR-002**: The system MUST reuse an existing Conversation record when a valid conversation identifier is provided with the question.
- **FR-003**: The system MUST persist a Question record containing the original question text, the translated question text (if translation occurred), the detected language, and the input method (e.g., typed, voice).
- **FR-004**: The system MUST persist an Answer record containing the generated answer text and the response language, linked to the corresponding Question record.
- **FR-005**: The system MUST persist an AnswerCitation record for each document chunk cited in the answer, linked to both the Answer record and the specific document chunk identifier.
- **FR-006**: All persisted records (Conversation → Question → Answer → AnswerCitation) MUST be linked via explicit parent-child relationships so that the full interaction can be reconstructed from any record in the chain.
- **FR-007**: The system MUST NOT expose a storage failure to the end user; storage errors must be logged internally without degrading the answer delivery experience.
- **FR-008**: The system MUST NOT persist partial or orphaned records; if any part of the persistence chain fails, records already written in that attempt must be rolled back or flagged as incomplete.
- **FR-009**: The system MUST store personal information contained in questions in a manner that supports future deletion or de-identification requests.
- **FR-010**: Administrators MUST be able to retrieve stored interaction records, filterable by date range and language, to support analytics and FAQ curation.

### Key Entities

- **Conversation**: Groups one or more questions from the same interaction session. Identified by a unique session identifier. Attributes: session identifier, start time, user reference (if authenticated).
- **Question**: A single question asked within a Conversation. Attributes: original text, translated text, detected language, input method, timestamp, link to parent Conversation.
- **Answer**: The system-generated response to a Question. Attributes: response text, response language, timestamp, link to parent Question.
- **AnswerCitation**: A reference to a specific document chunk that contributed to an Answer. Attributes: link to parent Answer, document chunk identifier, citation order/rank.

## Success Criteria _(mandatory)_

### Measurable Outcomes

- **SC-001**: After a user asks any question, 100% of complete interactions (question + answer + citations) are retrievable from storage within 5 seconds of the answer being delivered.
- **SC-002**: A storage failure during record persistence must never prevent the user from receiving the answer; the answer delivery success rate must not decrease as a result of this feature.
- **SC-003**: Administrators can retrieve all interaction records for any given date range in under 10 seconds for datasets covering up to 12 months of history.
- **SC-004**: 100% of answers that include document citations have at least one corresponding AnswerCitation record; answers with no citations have zero AnswerCitation records (no orphaned citations).
- **SC-005**: A follow-up question within an active session is correctly associated with the existing Conversation record in 100% of cases when a valid session ID is provided.

## Assumptions

- The Q&A system already has a functional answer-generation flow; this feature adds persistence without altering the answer-generation logic or user-facing response.
- A document chunk identifier (ChunkId) is already available from the existing retrieval-augmented generation process and can be used directly to create AnswerCitation records.
- Input method classification (typed, voice, etc.) is already captured or can be passed by the calling interface; the persistence layer only stores it.
- Multi-language support (translation and language detection) is already functional upstream; this feature records the results but does not implement translation itself.
- User identity is optional — the system must support both authenticated and anonymous interactions; anonymous records are stored without a user reference.
- POPIA compliance for data deletion or de-identification is an operational requirement to be satisfied by policy and tooling; the schema must support it but a deletion workflow is out of scope for this milestone.
- Analytics queries will run against the same source tables as interaction storage (no separate analytics schema for MVP).
