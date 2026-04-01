# Feature Specification: Intent-Aware Legal Retrieval for RAG Answers

**Feature Branch**: `feat/021-intent-aware-rag`  
**Created**: 2026-04-01  
**Status**: Draft  
**Input**: User description: "Refine the RAG system so it intelligently finds relevant legal documents based on meaning, does not require users to name specific acts in their query, and adapts answer generation behavior based on query confidence and response type."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Ask in plain language without naming an Act (Priority: P1)

A person asks a legal question in everyday language such as "Can my landlord evict me without a court order?" without naming any Act, section, or formal legal term. The system understands the issue, finds the right legal sources, and returns a grounded answer with citations.

**Why this priority**: This is the main user problem. The product should work for people who know their issue but do not know the names of the laws that govern it.

**Independent Test**: Submit a set of common legal questions that do not mention Act titles and verify the system still returns grounded answers that cite the correct legal source or sources.

**Acceptance Scenarios**:

1. **Given** a user asks a legal question without naming any Act, **When** the system searches for supporting material, **Then** it returns an answer grounded in the most relevant legal source or sources.
2. **Given** a user uses plain language rather than legal terminology, **When** the question is interpreted, **Then** the same relevant legal source is found as would be found from a more formal phrasing.
3. **Given** an answer is returned, **When** the user reviews the response, **Then** every material legal claim is supported by visible citations.

---

### User Story 2 - Different phrasings reach the same legal meaning (Priority: P2)

A user phrases the same legal issue in different ways, such as everyday speech, partial facts, or informal wording. The system consistently identifies the same legal topic and directs the answer toward the same governing source material.

**Why this priority**: Real users do not search like lawyers. The system needs to recognize meaning, not just exact words, to feel reliable.

**Independent Test**: Ask several semantically equivalent versions of the same legal question and verify that the same primary legal source is cited across the variants.

**Acceptance Scenarios**:

1. **Given** two questions that describe the same legal issue using different wording, **When** both are processed, **Then** the same primary legal source is selected for both.
2. **Given** a user uses colloquial terms such as "boss," "landlord," or "debt collector," **When** the system interprets the question, **Then** it maps those expressions to the correct legal issue area.

---

### User Story 3 - Multi-source answers are assembled automatically (Priority: P3)

A user asks a question whose answer depends on more than one legal source. The system does not force the user to know which laws apply in advance; it brings together the relevant sources and cites each one clearly.

**Why this priority**: Many legal questions are not answered by a single document. Users need the system to discover related sources on their behalf.

**Independent Test**: Ask a question that requires more than one governing source and verify the answer cites the relevant source set rather than relying on a single partial match.

**Acceptance Scenarios**:

1. **Given** a question is governed by more than one legal source, **When** the system answers, **Then** it includes the relevant combination of sources needed to support the answer.
2. **Given** one source is primary and another is supplementary, **When** the answer is presented, **Then** the response makes that relationship clear instead of presenting an incomplete single-source answer.

---

### User Story 4 - The system becomes more cautious when certainty is weak (Priority: P4)

A user asks a broad, ambiguous, or weakly supported question. The system adjusts how assertive the answer is: it gives a direct grounded answer when support is strong, and becomes more cautious when support is weak by asking for clarification or clearly limiting the conclusion.

**Why this priority**: Legal assistants must avoid sounding certain when the supporting material is unclear or incomplete.

**Independent Test**: Submit broad and ambiguous questions, then verify the system either asks for clarification or gives a clearly limited response instead of a definitive answer.

**Acceptance Scenarios**:

1. **Given** the system has strong supporting legal material, **When** it answers, **Then** it provides a direct and grounded response.
2. **Given** the system has weak or conflicting support, **When** it answers, **Then** it gives a cautious response that clearly signals uncertainty or requests clarification.

---

### Edge Cases

- What happens when a very short question such as "eviction" could refer to several different legal situations?
- How does the system behave when a user mentions the wrong Act name but the facts clearly point to a different legal source?
- What happens when retrieved material shares similar vocabulary but does not actually address the user's issue?
- How does the system respond when a question spans two related legal domains, such as housing and constitutional rights?
- What happens when the question is broad enough that several legal sources might apply, but none clearly answers the exact issue?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST accept legal questions in plain language without requiring users to provide Act names, section numbers, or formal legal terminology.
- **FR-002**: The system MUST determine the likely legal issue and source area from the meaning of the user's question, not only from exact keyword matches.
- **FR-003**: The system MUST identify and rank the legal sources most likely to answer the question, even when the user does not name any source explicitly.
- **FR-004**: The system MUST prefer legal sources that directly address the user's issue over sources that merely repeat similar words.
- **FR-005**: The system MUST recognize common everyday or colloquial phrasing and map it to the appropriate legal topic.
- **FR-006**: The system MUST produce materially consistent source selection for semantically equivalent questions phrased in different ways.
- **FR-007**: When more than one legal source is needed, the system MUST combine the relevant sources in the answer rather than forcing the user to ask source-specific follow-up questions.
- **FR-008**: When the user explicitly names a legal source, the system MUST treat that as a strong signal while still considering other clearly relevant sources.
- **FR-009**: The system MUST adjust answer-generation behavior based on how strongly the retrieved material supports the conclusion and on the type of response being produced.
- **FR-010**: The system MUST provide direct, citation-grounded answers when support is strong.
- **FR-011**: The system MUST provide a more cautious response when support is weak, broad, or ambiguous, including either a clarification request or a clearly limited conclusion.
- **FR-012**: The system MUST not present low-confidence interpretations as settled legal conclusions.
- **FR-013**: The system MUST include citations for each material claim in the answer.
- **FR-014**: The system MUST clearly indicate the legal source or sources used so that users can verify where the answer came from.

### Key Entities *(include if feature involves data)*

- **User Question**: A legal question submitted in natural language, often without formal legal terminology or source names.
- **Question Meaning**: The underlying legal issue inferred from the user's wording, such as eviction, dismissal, unfair treatment, debt recovery, or privacy.
- **Candidate Legal Source**: A law, regulation, or source section that may be relevant to the user's issue and is considered during answer generation.
- **Grounding Confidence**: The degree of certainty that the selected legal material directly supports the answer the system is about to give.
- **Answer Mode**: The response posture chosen for the question, such as direct answer, cautious answer, or clarification request.
- **Citation Set**: The collection of source references shown with the answer so users can trace each claim back to the governing material.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: On a benchmark set of common legal questions phrased without Act names, at least 85% return a grounded answer that cites the correct primary legal source on the first attempt.
- **SC-002**: On a benchmark set of semantically equivalent question variants, at least 90% lead to the same primary legal source or source set.
- **SC-003**: At least 80% of first-time test users can successfully ask a supported legal question in plain language without being told to name an Act.
- **SC-004**: 100% of low-confidence responses clearly signal uncertainty, request clarification, or limit their conclusion rather than presenting a definitive legal answer.
- **SC-005**: Users receive either a grounded answer or a clarification request within 10 seconds under normal operating conditions.
- **SC-006**: 100% of answer claims remain traceable to the citations returned with the response.

## Assumptions

- The existing legislation corpus and citation-based answer flow remain in place; this feature improves how questions are interpreted and how responses are shaped.
- The feature applies to the current legal question-answering experience and does not require a new user interface or a new content ingestion workflow.
- The system continues to ground answers in available legal source material and does not replace professional legal advice.
- Existing and future planning work may reuse current document topics, keywords, and other source descriptors if they help identify the right legal material.
- Multilingual support already defined elsewhere remains compatible with this feature, but this spec focuses on better source discovery and answer behavior rather than language expansion.
