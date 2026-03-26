# Feature Specification: MzansiLegal — Multilingual AI-Powered Legal & Financial Rights Assistant

**Feature Branch**: `001-mzansi-legal-rag`
**Created**: 2026-03-26
**Status**: Draft
**Input**: User description: "MzansiLegal — a multilingual AI-powered legal and financial rights assistant for South African citizens"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Citizen Asks a Legal Question (Priority: P1)

A South African citizen visits MzansiLegal and types or speaks a question about their legal rights in their preferred language (English, isiZulu, Sesotho, or Afrikaans). The system responds with a plain-language answer that cites the specific Act name and section number from South African legislation, and optionally reads the answer aloud.

**Why this priority**: This is the core value proposition of the platform — enabling any South African citizen to understand their legal rights without needing a lawyer, in their own language.

**Independent Test**: Can be fully tested by submitting a question in any supported language and verifying that a response is returned with at least one legislation citation, with the response in the same language as the question.

**Acceptance Scenarios**:

1. **Given** a citizen is on the Q&A chat page, **When** they type "What are my rights as a tenant?" in English and submit, **Then** they receive a plain-language answer citing the Rental Housing Act with specific section numbers, and a disclaimer about seeking legal advice appears.
2. **Given** a citizen selects isiZulu as their language, **When** they type or speak a question about unfair dismissal, **Then** the answer is returned in isiZulu citing the Labour Relations Act (section numbers in English), with a voice playback option available.
3. **Given** a citizen asks a question, **When** the system cannot find relevant legislation, **Then** the citizen sees a friendly message stating that no relevant information was found and is advised to consult a legal professional.
4. **Given** a citizen is using voice input, **When** they speak their question, **Then** the spoken audio is transcribed, processed, and the answer is displayed with an option to play it back as audio.

---

### User Story 2 - Citizen Uploads a Contract for Analysis (Priority: P2)

A citizen uploads a PDF or photo of a contract (employment, lease, credit, or service agreement). The system analyses the document, assigns a health score out of 100, identifies red flag clauses that may violate South African legislation, and provides a plain-language summary. The citizen can then ask follow-up questions about their specific contract.

**Why this priority**: Contract analysis is the second most impactful feature — many citizens sign contracts without understanding their rights or identifying potentially illegal clauses.

**Independent Test**: Can be fully tested by uploading a sample lease agreement PDF and verifying that a health score, at least one red/amber/green flag, and a plain-language summary are returned.

**Acceptance Scenarios**:

1. **Given** a citizen is on the contract analysis page, **When** they upload a PDF employment contract, **Then** the system displays a health score (0–100), a colour-coded breakdown of flags (red/amber/green), and a plain-language summary within a reasonable wait time.
2. **Given** the contract contains a clause that violates the Basic Conditions of Employment Act, **Then** a red flag card appears citing the specific Act and section that is being violated, with the problematic clause text highlighted.
3. **Given** the system successfully analyses the contract, **When** the citizen types a follow-up question about a specific clause, **Then** a relevant answer is provided using the contract's content and applicable legislation.
4. **Given** the uploaded file is not a readable PDF or photo, **Then** the citizen receives a clear error message prompting them to upload a supported file format.

---

### User Story 3 - Citizen Explores Their Rights by Category (Priority: P3)

A citizen uses the "My Rights" explorer to browse their legal and financial rights by category (e.g., Employment, Housing, Consumer Protection). They can expand rights cards to read the full legislation text, listen to content in their preferred language, and see their gamified knowledge score increase as they engage.

**Why this priority**: This is the discovery and education feature — it allows passive learning and builds civic awareness even for citizens who don't have a specific question.

**Independent Test**: Can be fully tested by navigating to the My Rights explorer, selecting a category, expanding a rights card, and verifying that legislation content and action options appear.

**Acceptance Scenarios**:

1. **Given** a citizen is on the My Rights explorer, **When** they click a category filter tab (e.g., "Employment"), **Then** only rights cards in that category are displayed, each showing a title and summary.
2. **Given** a citizen expands a rights card, **Then** they see the full plain-language explanation, relevant legislation citation, and action buttons to ask a follow-up question, listen to the content, or share it.
3. **Given** a citizen has engaged with at least one rights card, **Then** their gamified knowledge score increases and is visually displayed in a progress bar.

---

### User Story 4 - Admin Reviews Answer Quality (Priority: P4)

An admin user logs into the analytics dashboard, reviews the answer quality queue (questions flagged for review), marks individual answers as accurate or inaccurate, adds notes, and promotes curated Q&A pairs to the public FAQ section.

**Why this priority**: Trust and accuracy are critical for a legal information service — admin oversight ensures that citizens receive verified, high-quality answers.

**Independent Test**: Can be fully tested by logging in as an admin, viewing the review queue, marking an answer as accurate, and verifying that the conversation is promoted to the public FAQ.

**Acceptance Scenarios**:

1. **Given** an admin is on the analytics dashboard, **When** they open the answer quality review queue, **Then** they see a list of recent answers with the question, the AI-generated answer, and options to mark it as accurate or inaccurate.
2. **Given** an admin marks a conversation as accurate and sets IsPublicFaq to true, **Then** that Q&A pair appears on the home dashboard's trending questions section and in the My Rights explorer as a related question.
3. **Given** an admin adds notes to an answer, **Then** those notes are saved and visible on subsequent review of the same answer.

---

### User Story 5 - Citizen Uses Accessibility Features (Priority: P5)

A citizen with a visual impairment or dyslexia uses the platform with screen reader support, dyslexia-friendly font mode, auto-play audio, or high contrast mode to access legal information without barriers.

**Why this priority**: South Africa has a significant population with literacy and accessibility challenges — the platform must serve all citizens equitably.

**Independent Test**: Can be fully tested by enabling dyslexia mode and verifying that font, spacing, and size change, then enabling auto-play audio and verifying answers are read aloud automatically.

**Acceptance Scenarios**:

1. **Given** a citizen enables dyslexia mode, **Then** the interface switches to a dyslexia-friendly font with increased letter spacing and font size.
2. **Given** a citizen enables auto-play audio, **Then** every AI answer is automatically read aloud when it arrives without requiring the citizen to click a play button.
3. **Given** a citizen is using a keyboard only, **Then** all interactive elements are reachable and operable via keyboard navigation, meeting WCAG 2.1 AA standards.
4. **Given** a citizen's OS is set to high contrast mode, **Then** the application respects the OS preference and renders in high contrast without additional configuration.

---

### Edge Cases

- What happens when a user submits a question in a language not supported (e.g., isiXhosa)? → System detects the language, informs the user it is not yet supported, and suggests English as an alternative.
- What happens when no legislation chunk is sufficiently relevant to the question? → System responds that no relevant legislation was found and advises consulting a legal professional; no answer is fabricated.
- What happens when the AI service is unavailable? → User sees a friendly error message indicating the service is temporarily unavailable and is prompted to try again later; no partial or corrupted answers are displayed.
- What happens when an uploaded contract exceeds the maximum file size? → User sees an immediate validation message specifying the maximum allowed file size before any processing begins.
- What happens when a contract is uploaded in a language other than English? → System detects the language, translates the content for analysis, and returns results in the user's preferred language.
- What happens when a citizen with auto-play audio enabled navigates between pages? → Audio playback stops when they leave a page; it does not continue playing in the background unexpectedly.
- What happens when an admin tries to access the analytics dashboard without admin role? → Access is denied and the user is redirected to the citizen home page.

## Requirements *(mandatory)*

### Functional Requirements

**Q&A Chat:**
- **FR-001**: Citizens MUST be able to submit legal or financial questions via text input in any of the 4 supported languages (English, isiZulu, Sesotho, Afrikaans).
- **FR-002**: Citizens MUST be able to submit questions via voice input, which is transcribed to text before processing.
- **FR-003**: Every AI-generated answer MUST cite at least one specific Act name and section number from the indexed South African legislation.
- **FR-004**: Answers MUST be returned in the same language as the question (or the citizen's selected language preference).
- **FR-005**: Every answer MUST include a legal disclaimer in the citizen's language stating the answer is informational and not legal advice.
- **FR-006**: Citizens MUST be able to play answers as audio via a voice playback control.
- **FR-007**: The system MUST display related questions alongside each answer.
- **FR-008**: Citizens MUST be able to expand citation cards to view the specific legislation excerpt and section reference.

**Contract Analysis:**
- **FR-009**: Citizens MUST be able to upload a contract as a PDF or image (JPEG/PNG).
- **FR-010**: The system MUST extract and analyse the text of the uploaded contract.
- **FR-011**: The system MUST assign a health score from 0 to 100, where higher scores indicate fewer concerning clauses.
- **FR-012**: The system MUST generate a plain-language summary of the contract.
- **FR-013**: The system MUST identify and categorise flag items as Red (serious violation), Amber (concern), or Green (compliant/positive clause), each citing relevant legislation.
- **FR-014**: Citizens MUST be able to ask follow-up questions about the analysed contract.

**My Rights Explorer:**
- **FR-015**: Citizens MUST be able to browse rights cards organised by category.
- **FR-016**: Citizens MUST be able to filter rights cards by category using filter tabs.
- **FR-017**: Citizens MUST be able to expand rights cards to view full legislation text.
- **FR-018**: Citizens MUST be able to trigger audio playback of rights card content.
- **FR-019**: The system MUST track and display each citizen's knowledge score, which increases as they engage with rights cards.

**Home Dashboard:**
- **FR-020**: The home page MUST display live aggregate statistics: total questions answered, contracts analysed, supported languages, and Acts indexed.
- **FR-021**: The home page MUST display category cards with domain tags (Legal/Financial).
- **FR-022**: The home page MUST display trending/public FAQ questions sourced from admin-curated conversations.
- **FR-023**: The home page MUST provide a search bar with a microphone button to initiate a Q&A session.

**Admin Analytics:**
- **FR-024**: Admin users MUST be able to view a dashboard showing question trends, language distribution (with voice/text split), and top questions.
- **FR-025**: Admin users MUST be able to review AI-generated answers in a quality review queue.
- **FR-026**: Admin users MUST be able to mark answers as accurate or inaccurate and add review notes.
- **FR-027**: Admin users MUST be able to promote a conversation to public FAQ status, making it visible on the home page and My Rights explorer.

**Knowledge Base:**
- **FR-028**: The system MUST support ingestion of South African legislation PDFs, extracting text and indexing it for retrieval.
- **FR-029**: The system MUST store 13 pre-seeded legislation documents (7 Legal, 6 Financial) as the initial knowledge base.

**Accessibility:**
- **FR-030**: The system MUST provide a dyslexia-friendly mode that changes to an accessible font with increased spacing and font size.
- **FR-031**: The system MUST provide an auto-play audio option that reads answers aloud automatically.
- **FR-032**: All interactive elements MUST be operable via keyboard navigation.
- **FR-033**: The system MUST comply with WCAG 2.1 AA accessibility standards, including full ARIA labels and roles.
- **FR-034**: All touch/click targets MUST be a minimum of 44×44 pixels.
- **FR-035**: The system MUST respect the user's OS-level high contrast setting.

**Authentication:**
- **FR-036**: Citizens MUST be able to register and log in with an email address and password.
- **FR-037**: The system MUST support two roles: Citizen and Admin, with role-based access control.
- **FR-038**: Citizens' conversation histories and contract analyses MUST be private and only visible to the individual citizen and Admin users.

### Key Entities

- **Category**: Represents a thematic grouping of legal or financial rights (e.g., Employment, Housing, Consumer Protection). Has a domain (Legal or Financial) and a display order.
- **Legal Document**: Represents a piece of South African legislation (e.g., the Basic Conditions of Employment Act). Contains the full text and metadata (Act number, year, category). Has a processing status to indicate whether it has been indexed.
- **Document Chunk**: A section-level fragment of a Legal Document. Contains the chapter, section number, section title, and text content. Used as the retrieval unit in the RAG pipeline.
- **Conversation**: A session of questions and answers between a citizen and the system, in a specific language. Can be flagged as a public FAQ by an admin.
- **Question**: A single question within a conversation, with the original text (in the citizen's language) and the translated text (in English for retrieval).
- **Answer**: The AI-generated response to a question, stored with its language, citations, and an optional audio file. Admins can mark it as accurate and add notes.
- **Answer Citation**: A reference linking an answer to a specific Document Chunk, including the relevance score and an excerpt of the legislation used.
- **Contract Analysis**: The result of analysing an uploaded contract. Contains the extracted text, a health score, a summary, and a list of flags.
- **Contract Flag**: An individual finding from a contract analysis (Red/Amber/Green severity), with a description and the specific legislation that was applied.
- **App User**: A citizen or admin who uses the platform, with preferences for language, dyslexia mode, and audio auto-play.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Citizens receive a complete answer with at least one legislation citation for 90% of questions asked about topics covered by the 13 indexed Acts.
- **SC-002**: Citizens receive an answer within 10 seconds of submitting a text question under normal system load.
- **SC-003**: Contract analysis results (health score, flags, summary) are returned within 30 seconds of a valid contract upload.
- **SC-004**: Citizens using any of the 4 supported languages receive answers in that language, verified for 100% of test cases across all 4 languages.
- **SC-005**: The platform is usable by keyboard-only users for all primary flows, with zero critical WCAG 2.1 AA violations.
- **SC-006**: Enabling dyslexia mode visibly changes the font and spacing within 1 second of toggling the setting.
- **SC-007**: Admin review actions (mark accurate, promote to FAQ) are reflected on the home page within 60 seconds.
- **SC-008**: All 13 legislation documents are fully indexed and retrievable before the platform launches.
- **SC-009**: The platform works correctly on modern mobile and desktop browsers without layout breakage, tested across at least 3 browser/device combinations.
- **SC-010**: Citizens who cannot be helped (no relevant legislation found) receive a graceful fallback message in 100% of such cases, with no blank screens or technical error messages shown.

## Assumptions

- Citizens are assumed to have access to a modern web browser on either a mobile phone or a desktop/laptop device.
- Voice input assumes the device has a microphone and the browser supports audio capture.
- All 13 legislation documents are publicly available from South African government sources and can be legally indexed and used for informational purposes.
- The knowledge base is seeded with legislation as of the project start date; keeping legislation up to date is out of scope for v1.
- Anonymous browsing of the home page, My Rights explorer, and public FAQs is supported; account creation is required only for conversation history and contract analysis.
- The admin role is assigned manually by a system administrator; self-service admin registration is out of scope.
- The gamified knowledge score is tracked per user session; cross-session persistence of the knowledge score requires a logged-in account.
- Email/password authentication is the only login method for v1; social login (e.g., Google) is out of scope.
- The platform is deployed to a single Azure region for v1; multi-region redundancy is out of scope.
- The Afrikaans and Sesotho translations produced by the AI are assumed to be accurate enough for informational purposes; professional translation review is out of scope for v1.