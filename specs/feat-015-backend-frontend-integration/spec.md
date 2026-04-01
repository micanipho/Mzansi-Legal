# Feature Specification: Backend Frontend Integration

**Feature Branch**: `feat/015-backend-frontend-integration`
**Created**: 2026-03-30
**Status**: Draft
**Input**: User description: "lets integrate the backend to the frontend"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Legal Q&A via Chat Interface (Priority: P1)

A South African citizen or legal professional visits the Mzansi Legal application and asks a question in natural language (in any of the four supported languages: English, Zulu, Xhosa, Afrikaans). The frontend sends the question to the backend RAG Q&A service, which searches relevant legislation and returns an AI-generated answer with cited legal sources. The answer and citations are displayed clearly in the chat interface.

**Why this priority**: This is the core value proposition of the application — connecting a user's question to relevant South African legislation. Without this working end-to-end, the application has no user value.

**Independent Test**: Can be tested by opening the application, typing a legal question, submitting it, and verifying that a relevant answer with at least one legislative citation appears on screen.

**Acceptance Scenarios**:

1. **Given** the user is on the Q&A page, **When** they type a legal question and submit it, **Then** an answer appears within a reasonable time with at least one cited legislative source.
2. **Given** the backend is processing a question, **When** it takes more than 2 seconds, **Then** the frontend shows a visible loading indicator so the user knows the request is in progress.
3. **Given** the backend returns an error, **When** the request fails, **Then** the frontend displays a user-friendly error message and allows the user to retry.
4. **Given** a user asks a question in Zulu or another supported language, **When** the request is submitted, **Then** the answer is returned and displayed in the same language.

---

### User Story 2 - Conversation History (Priority: P2)

A returning user can view their previous questions and the answers they received during a session. The conversation thread persists on screen as the user asks follow-up questions, building a coherent dialogue history within the current session.

**Why this priority**: Providing conversational context improves the quality of follow-up interactions and is expected by users familiar with AI chat interfaces. Without conversation history, each question is isolated and the user loses context.

**Independent Test**: Can be tested by asking two consecutive questions and verifying that both questions and their answers are visible in a scrollable conversation thread.

**Acceptance Scenarios**:

1. **Given** a user has asked one question and received an answer, **When** they ask a second question, **Then** both question-answer pairs are visible in the conversation thread in chronological order.
2. **Given** a conversation has multiple exchanges, **When** the user scrolls up, **Then** earlier messages are accessible without losing the current position.
3. **Given** a user starts a new session, **When** the page loads, **Then** a fresh conversation is started.

---

### User Story 3 - Citation Viewing (Priority: P3)

When the backend returns legislative citations alongside an answer, the user can see which specific legislation or section was referenced. Citations are displayed in a readable format alongside or below the answer.

**Why this priority**: Cited sources establish trust and allow users (especially legal professionals) to verify the basis of the answer. This differentiates the application from generic AI chat tools.

**Independent Test**: Can be tested by asking a question known to match specific legislation and verifying that the returned answer includes a visible reference to the correct act or section.

**Acceptance Scenarios**:

1. **Given** the backend returns an answer with one or more citations, **When** the answer is displayed, **Then** each citation is shown with at minimum the legislation name and relevant section identifier.
2. **Given** an answer has no citations, **When** displayed, **Then** no citation section appears rather than an empty placeholder.

---

### User Story 4 - Authenticated Access (Priority: P2)

Users must log in before accessing the Q&A feature. The frontend enforces authentication by redirecting unauthenticated users to the login page. Upon successful login, users are redirected back to the application and their session token is managed securely.

**Why this priority**: The backend exposes authenticated endpoints. Without a working auth flow wired to the frontend, no API calls will succeed.

**Independent Test**: Can be tested by visiting the Q&A page while logged out and verifying the redirect to login, then logging in and verifying access is granted.

**Acceptance Scenarios**:

1. **Given** a user is not logged in, **When** they navigate to any protected page, **Then** they are redirected to the login page.
2. **Given** a user enters valid credentials, **When** they submit the login form, **Then** they are granted access and redirected to their intended destination.
3. **Given** a user's session expires, **When** they attempt an action requiring authentication, **Then** they are prompted to log in again without losing their current page context.

---

### Edge Cases

- What happens when the backend is unreachable (network error or service down)?
- How does the frontend handle very long answers that exceed typical display height?
- What happens if the user submits an empty or whitespace-only question?
- How does the frontend behave when the user submits a question while a previous one is still loading?
- What happens when the session token is invalid or expired mid-conversation?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The frontend MUST connect to the backend API and successfully send user questions to the RAG Q&A endpoint.
- **FR-002**: The frontend MUST display answers returned from the backend, including associated legislative citations.
- **FR-003**: The frontend MUST show a loading state while awaiting a backend response.
- **FR-004**: The frontend MUST display a user-friendly error message when the backend returns an error or is unreachable.
- **FR-005**: The frontend MUST prevent submission of empty or blank questions.
- **FR-006**: The frontend MUST maintain a visible conversation thread showing the current session's questions and answers in order.
- **FR-007**: The frontend MUST enforce authenticated access — unauthenticated users are redirected to login before accessing Q&A features.
- **FR-008**: The frontend MUST securely attach the authenticated user's credentials to each backend API request.
- **FR-009**: The frontend MUST support the four application languages (English, Zulu, Xhosa, Afrikaans) in the UI and pass the user's selected language context to the backend where applicable.
- **FR-010**: The frontend MUST disable or prevent duplicate question submission while a request is in-flight.

### Key Entities

- **Question**: A natural language query submitted by the user, including the selected language and conversation context.
- **Answer**: The AI-generated response returned by the backend, linked to the originating question.
- **Citation**: A reference to a specific piece of South African legislation (act name, section) that supports the answer.
- **Conversation**: A session-scoped thread of question-answer pairs displayed to the user.
- **User Session**: The authenticated state of the current user, including the credential token used for backend API calls.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can submit a legal question and receive an answer with citations displayed on screen within 15 seconds under normal conditions.
- **SC-002**: 100% of backend API calls include valid authentication credentials — no unauthenticated requests reach protected endpoints.
- **SC-003**: The application displays a loading indicator within 500 milliseconds of submitting a question, ensuring users never see a frozen or unresponsive UI.
- **SC-004**: Unauthenticated users are redirected to the login page within one navigation step — no protected data is exposed before authentication.
- **SC-005**: Empty question submissions are blocked on the client side — no empty requests reach the backend.
- **SC-006**: The conversation thread correctly displays all questions and answers from the current session without loss or reordering.

## Assumptions

- The backend API is deployed and reachable from the frontend environment (locally during development, and via Railway in production).
- The backend exposes RESTful endpoints for the RAG Q&A service built in feat/014 — no new backend work is required for this feature.
- The frontend already has a routing structure and a login page — this feature wires the auth state to API calls and protects routes.
- The frontend's language selection state is already implemented or will be implemented alongside this integration.
- Mobile support is included (responsive layout is expected) but dedicated mobile-specific UX optimisation is out of scope for this feature.
- The backend returns citations as structured data that the frontend can render without additional lookups.
- Session management uses tokens issued by the ABP Zero backend — no third-party auth provider integration is required.
