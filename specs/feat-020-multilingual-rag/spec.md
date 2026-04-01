# Feature Specification: Multilingual RAG Q&A (isiZulu, Sesotho, Afrikaans)

**Feature Branch**: `feat/020-multilingual-rag`  
**Created**: 2026-04-01  
**Status**: Draft  
**Input**: User description: "Build a LanguageService that detects input language, translates to English for RAG search, stores both original and translated question text, and instructs the system to respond in the user's language."

## User Scenarios & Testing *(mandatory)*

### User Story 1 — isiZulu Question, isiZulu Answer (Priority: P1)

A user who speaks isiZulu types their legal question in isiZulu. The system understands the question, searches the legislation knowledge base, and returns a correct, relevant answer in isiZulu. Act names and section references appear in English within the isiZulu response.

**Why this priority**: isiZulu is South Africa's most widely spoken home language. This story validates the full end-to-end multilingual flow and directly addresses the stated acceptance criterion ("Ingabe umnikazi wendlu angangixosha?").

**Independent Test**: Submit "Ingabe umnikazi wendlu angangixosha?" and verify the response is in isiZulu and contains accurate references to the relevant legislation with Act names in English.

**Acceptance Scenarios**:

1. **Given** a user submits a question in isiZulu, **When** the system processes the question, **Then** it returns an answer written in isiZulu with legal citations (Act name and section number) preserved in English.
2. **Given** an isiZulu question that maps to known legislation, **When** the answer is returned, **Then** the answer is factually accurate and cites the correct section of the relevant Act.
3. **Given** an isiZulu question is submitted, **When** it is stored, **Then** both the original isiZulu text and the English translation are persisted alongside the question record.

---

### User Story 2 — Sesotho Question, Sesotho Answer (Priority: P2)

A user who speaks Sesotho submits a legal question in Sesotho. The system detects the language, searches for relevant legislation using the English meaning, and replies in Sesotho with English citations.

**Why this priority**: Sesotho is one of South Africa's official languages and a target language for this platform. This story confirms the multilingual pipeline generalises beyond a single language.

**Independent Test**: Submit a Sesotho legal question and verify the response is in Sesotho with English Act citations.

**Acceptance Scenarios**:

1. **Given** a question submitted in Sesotho, **When** the system responds, **Then** the answer is written in Sesotho.
2. **Given** a Sesotho question is submitted, **When** stored, **Then** both original Sesotho text and English translation are saved.

---

### User Story 3 — Afrikaans Question, Afrikaans Answer (Priority: P3)

A user submits a legal question in Afrikaans. The system replies in Afrikaans with English legal citations.

**Why this priority**: Afrikaans is widely spoken and legally recognised in South Africa. Supporting it completes the four-language requirement.

**Independent Test**: Submit an Afrikaans legal question and verify the response language is Afrikaans.

**Acceptance Scenarios**:

1. **Given** a question submitted in Afrikaans, **When** the system responds, **Then** the answer is written in Afrikaans.
2. **Given** an Afrikaans question is submitted, **When** stored, **Then** both original Afrikaans text and English translation are saved.

---

### User Story 4 — English Question, No Regression (Priority: P4)

A user submits a question in English. The system behaves exactly as before — no translation step is applied, and the answer is returned in English.

**Why this priority**: Ensures the feature does not degrade existing English Q&A behaviour.

**Independent Test**: Submit an English legal question and confirm the response is in English with no change in answer quality.

**Acceptance Scenarios**:

1. **Given** a question submitted in English, **When** the system processes it, **Then** the answer is returned in English and no unnecessary translation step is performed.

---

### Edge Cases

- What happens when the input language cannot be determined (e.g., very short or ambiguous text)?
- How does the system handle code-switching (mixing two languages in one question), which is common in informal South African speech?
- What happens when no relevant legislation is found in the knowledge base for a non-English question?
- How does the system respond if the user submits a question in a language outside the supported set (e.g., isiXhosa, Tshivenda)?
- What happens if the English translation of the question yields zero search results in the knowledge base?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST detect the language of every incoming question before searching the knowledge base.
- **FR-002**: The system MUST support four languages: English (en), isiZulu (zu), Sesotho (st), and Afrikaans (af).
- **FR-003**: When a question is in a non-English language, the system MUST produce an English translation for use in knowledge-base search.
- **FR-004**: The system MUST use the English version of the question (original or translated) as the search query against the legislation knowledge base.
- **FR-005**: The system MUST respond to the user in the same language in which the question was originally submitted.
- **FR-006**: Responses in non-English languages MUST preserve Act names, section numbers, and other legal identifiers in English.
- **FR-007**: The system MUST store both the original question text and the English translation alongside the question record.
- **FR-008**: When the detected language is English, the system MUST skip the translation step entirely and use the original text for search.
- **FR-009**: The system MUST handle unrecognised or unsupported input languages gracefully — defaulting to English processing and returning an answer in English without surfacing an error to the user.

### Key Entities

- **Question**: A user's legal query. Gains two new data points: the text as originally submitted (in any language) and the English equivalent used for knowledge-base search. Also gains a detected language code (en, zu, st, or af).
- **Supported Language**: The enumerated set of four language codes (en, zu, st, af) that drive language detection, translation routing, and response generation.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users asking legal questions in isiZulu, Sesotho, or Afrikaans receive relevant, accurate answers in their own language 100% of the time (given clearly written input in a supported language).
- **SC-002**: Legal citations — Act names and section numbers — appear in English in every non-English response, with no translation of those identifiers.
- **SC-003**: Both the original question text and its English translation are retrievable from the question record after submission.
- **SC-004**: English Q&A quality and behaviour are unchanged compared to the pre-feature baseline.
- **SC-005**: The system correctly identifies the input language for at least 95% of clearly written isiZulu, Sesotho, and Afrikaans questions.
- **SC-006**: Unsupported or undetectable input languages fall back to English processing without returning an error to the user.

## Assumptions

- The four supported languages (en, zu, st, af) are sufficient for the initial release; additional languages (e.g., isiXhosa, Tshivenda) are out of scope.
- Legislation source documents in the knowledge base are in English; the feature does not require non-English source material.
- Language detection accuracy is considered acceptable when the input is at least one full sentence; very short inputs (a single word) are treated as best-effort.
- Whisper-based voice transcription (if used upstream) already provides language detection during audio processing; this feature handles text-level detection for typed input only.
- Legal citation formatting (Act name + section number in English) is the required citation style regardless of response language — no requirement exists to translate citation text.
- The platform already has a working English RAG Q&A pipeline; this feature layers on top of that pipeline without replacing it.
- Response time expectations for multilingual questions are the same as for English questions — no stricter latency requirement applies.
