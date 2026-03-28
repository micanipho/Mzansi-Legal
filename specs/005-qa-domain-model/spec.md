# Feature Specification: Q&A Domain Model for RAG System

**Feature Branch**: `005-qa-domain-model`
**Created**: 2026-03-28
**Status**: Draft
**Milestone**: Setup & Data Pipeline

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Start a Legal Conversation (Priority: P1)

A registered user opens the legal assistant and asks their first question. The system creates a new conversation linked to their account, records the question (in the user's chosen language and input method), generates an AI answer, and attaches citations pointing to the relevant legislation sections.

**Why this priority**: This is the core end-to-end flow. Without the ability to create and persist a conversation with a question and answer, the entire RAG system cannot function. All other stories build on this foundation.

**Independent Test**: Can be fully tested by submitting a single question as an authenticated user and verifying that a Conversation, Question, Answer, and at least one AnswerCitation record are all persisted and queryable.

**Acceptance Scenarios**:

1. **Given** an authenticated user with no prior conversations, **When** they submit a legal question in English via text, **Then** a new Conversation is created with their UserId, Language=en, InputMethod=Text, and a StartedAt timestamp
2. **Given** a new Conversation exists, **When** the system processes the question, **Then** a Question record is created with OriginalText, Language, InputMethod, and the ConversationId foreign key
3. **Given** a Question record exists, **When** the AI generates a response, **Then** an Answer record is created with the response text, Language, and QuestionId foreign key
4. **Given** an Answer record exists, **When** citations are resolved from legislation chunks, **Then** one or more AnswerCitation records are created with AnswerId, ChunkId, SectionNumber, Excerpt, and RelevanceScore

---

### User Story 2 - Continue an Existing Conversation (Priority: P2)

A user returns to an ongoing legal conversation and asks a follow-up question. The system appends the new question and answer to the same conversation, preserving the full exchange history.

**Why this priority**: Conversation continuity is critical for legal assistance — users often need clarification across multiple exchanges. Without this, every interaction is isolated and context is lost.

**Independent Test**: Can be tested by creating a Conversation with one Question/Answer pair, then adding a second Question/Answer pair to the same Conversation and verifying the relationship chain is intact.

**Acceptance Scenarios**:

1. **Given** an existing Conversation, **When** a user submits a follow-up question, **Then** a new Question is added to the same ConversationId without creating a new Conversation
2. **Given** a Conversation with multiple Questions, **When** the conversation history is queried, **Then** all Questions and their Answers are returned in order

---

### User Story 3 - Mark an Answer as a Public FAQ (Priority: P3)

An admin or the system marks a conversation as a public FAQ, optionally associating it with a legal category. This enables curated answers to be surfaced to other users searching similar topics.

**Why this priority**: The public FAQ feature extends the value of individual answers to all users, but depends on the core Q&A model being stable first.

**Independent Test**: Can be tested by setting `IsPublicFaq=true` and assigning a `FaqCategory` on an existing Conversation, then querying for public FAQ conversations filtered by category.

**Acceptance Scenarios**:

1. **Given** a completed Conversation, **When** `IsPublicFaq` is set to true with a valid `FaqCategory`, **Then** the Conversation is retrievable via a public FAQ query
2. **Given** `IsPublicFaq=false`, **When** querying public FAQs, **Then** the Conversation does not appear in results

---

### User Story 4 - Validate Answer Accuracy (Priority: P4)

An admin reviews an AI-generated answer and marks it as accurate or inaccurate, adding optional notes. This supports quality control of the legal assistant's outputs.

**Why this priority**: Quality control is important for a legal assistant but does not block the core data pipeline. Administrators can perform this after the core model is live.

**Independent Test**: Can be tested by updating `IsAccurate` and `AdminNotes` on an existing Answer record and verifying the values are persisted correctly.

**Acceptance Scenarios**:

1. **Given** an Answer record with `IsAccurate=null`, **When** an admin sets `IsAccurate=true` and adds notes, **Then** both fields are persisted and queryable
2. **Given** an Answer marked inaccurate, **When** admin notes are added, **Then** `AdminNotes` is stored and associated with the correct Answer

---

### Edge Cases

- What happens when a Conversation's FaqCategory FK references a Category that is later deleted? (Category deletion must be prevented or Conversation must be nullified)
- How does the system handle an Answer with zero AnswerCitation records? (Answer must still be valid — citations are expected but not mandatory at the data model level)
- What happens when a DocumentChunk referenced by AnswerCitation is deleted? (Cross-aggregate reference must be protected — deletion of a referenced chunk must be blocked or handled gracefully)
- How does the system handle voice input when no AudioFile is provided? (AudioFile is a stored file reference — absence should be allowed for text-only interactions)
- What happens when OriginalText and TranslatedText are the same language? (TranslatedText may equal OriginalText; no constraint prevents this)
- How are RelevanceScore values bounded? (RelevanceScore is a decimal — values should fall between 0.0 and 1.0; validation enforced at application layer)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST persist every Conversation with a mandatory link to a registered user — anonymous conversations are not permitted
- **FR-002**: System MUST record the Language and InputMethod for every Conversation and every Question independently, as they may differ
- **FR-003**: System MUST allow a Conversation to be optionally associated with a public FAQ Category
- **FR-004**: System MUST persist each Question with its original text, translated text, language, input method, and optional audio file reference within a Conversation
- **FR-005**: System MUST persist each Answer with its generated text, language, optional audio file reference, and optional accuracy flag within a Question
- **FR-006**: System MUST support admin review of Answers by allowing `IsAccurate` and `AdminNotes` to be set after initial creation
- **FR-007**: System MUST persist AnswerCitations linking each Answer to a specific legislation chunk, with section number, excerpt, and relevance score
- **FR-008**: System MUST enforce that AnswerCitation references to DocumentChunk are valid and queryable across the aggregate boundary
- **FR-009**: System MUST support retrieval of a full conversation thread: Conversation → Questions → Answers → Citations
- **FR-010**: System MUST use Language and InputMethod as enumerated values restricted to defined options (Language: en, zu, st, af; InputMethod: Text, Voice)
- **FR-011**: System MUST record the timestamp when a Conversation is started
- **FR-012**: System MUST allow a Conversation to be flagged as a public FAQ, with the flag defaulting to false

### Key Entities

- **Conversation**: Represents a single legal assistance session for a registered user. Owns one or more Questions. Carries language, input method, start time, and optional public FAQ classification.
- **Question**: Represents a single user query within a Conversation. Stores both the original and translated text, the language and input method used, and an optional audio file reference.
- **Answer**: Represents the AI-generated response to a Question. Stores the response text, language, optional audio file, and admin-reviewable accuracy metadata.
- **AnswerCitation**: Represents a link between an Answer and a specific legislation chunk that supports it. Stores the section reference, a text excerpt, and a relevance score. References DocumentChunk across aggregate boundaries.
- **Language (enum)**: Enumerated list of supported languages: English (en), Zulu (zu), Sesotho (st), Afrikaans (af).
- **InputMethod (enum)**: Enumerated list of input modes: Text, Voice.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A complete conversation thread (Conversation → Question → Answer → Citation) can be created and retrieved in a single operation without data loss
- **SC-002**: All four entities and their relationships are enforced at the persistence layer — records with invalid foreign keys are rejected
- **SC-003**: 100% of Language and InputMethod fields are constrained to defined enumeration values — out-of-range values are rejected at the data boundary
- **SC-004**: Admin accuracy review fields (`IsAccurate`, `AdminNotes`) can be updated on an existing Answer without affecting other fields
- **SC-005**: Public FAQ conversations are queryable by Category, returning only Conversations where `IsPublicFaq=true`
- **SC-006**: Cross-aggregate citation queries (Answer + DocumentChunk metadata) return correct results without requiring manual joins by calling code
- **SC-007**: The data model supports at least 10 citations per Answer and at least 50 questions per Conversation without schema constraints

## Assumptions

- All users interacting with the system are authenticated — there is no guest or anonymous mode for conversations
- An AudioFile is a reference (e.g., a stored file ID or path) rather than binary data stored directly in the database; the actual file is managed by a separate storage service
- TranslatedText may equal OriginalText when no translation is needed (e.g., the user's language matches the system's default)
- DocumentChunk entities already exist in the system (from the RAG document pipeline) and are treated as a read-only cross-aggregate reference by this feature
- FaqCategory references the Category entity defined in the RAG domain model (feature 004); no new Category entity is introduced here
- RelevanceScore values are expected to fall between 0.0 and 1.0 but enforcement is at the application layer, not a database constraint
- The public FAQ feature does not include access-control logic in this iteration — visibility rules are deferred to a future feature
- Voice input (AudioFile) is optional; the model must support text-only interactions without requiring an audio file
- Conversations are not soft-deleted in this iteration — deletion behavior is deferred
- The Language enum values (en, zu, st, af) represent the four official South African languages supported by this assistant
