# Feature Specification: Full UI Pages — MzansiLegal Design System Implementation

**Feature Branch**: `feat/019-full-ui-pages`
**Created**: 2026-03-31
**Status**: Draft
**Input**: User description: "create all pages following the design system — landing, ask/chat, contracts, my rights, history, auth pages, and dashboard"

---

## Overview

Implement all primary user-facing pages of the MzansiLegal application in accordance with the approved visual design system. Pages include: Landing (Home), Ask/Chat, Contracts List & Detail, My Rights, History, Authentication (Sign In & Register), and Admin Dashboard. All pages share a consistent design language — typography, colour palette, spacing, card styles, navigation bar, and interactive states — as shown in the provided design references.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Browse the Landing Page (Priority: P1)

A first-time visitor arrives at the home page and immediately understands what MzansiLegal does, sees social-proof statistics, browses categories, and is prompted to ask a question or register.

**Why this priority**: The landing page is the entry point for all users. Without it no other feature is discoverable.

**Independent Test**: Open the root URL while unauthenticated, verify hero content, stats row, feature cards, category grid, and trending questions render correctly.

**Acceptance Scenarios**:

1. **Given** an unauthenticated visitor, **When** they load the home page, **Then** they see the hero section with the tagline "Know your rights. In your language.", a search/ask bar, and quick-action suggestion chips.
2. **Given** a visitor scrolling down, **When** they pass the hero, **Then** four statistics cards are visible (Questions answered, Acts indexed, Languages, Contracts analysed).
3. **Given** a visitor on the home page, **When** they view the feature action cards, **Then** two cards appear: "Analyse a contract" and "Ask a question".
4. **Given** a visitor on the home page, **When** they view the category grid, **Then** nine category cards display with icons, names, descriptions, and type badges (Legal / Financial / Contracts).
5. **Given** a visitor on the home page, **When** they view "What South Africans are asking", **Then** five trending questions display with numbered ranks and category tags.
6. **Given** a visitor, **When** they click "Get started" in the navbar, **Then** they are navigated to the Register page.

---

### User Story 2 — Ask a Legal Question (Priority: P1)

An authenticated user types or speaks a question in their preferred language and receives a cited, plain-language answer with related questions.

**Why this priority**: Ask/Chat is the core value proposition of the application.

**Independent Test**: Navigate to /ask, submit a question in any supported language, verify an answer with citations and related questions is rendered.

**Acceptance Scenarios**:

1. **Given** a user on the Ask page, **When** they type a question and submit, **Then** the question appears in the chat thread and an AI answer streams below it.
2. **Given** an answer rendered, **When** the user expands "Sources (N sections cited)", **Then** cited legislation sections are listed with act name and section number.
3. **Given** an answer rendered, **When** the user clicks "Listen in [language]", **Then** the answer is read aloud in the selected language.
4. **Given** an answer rendered, **When** the user views the disclaimer banner, **Then** it states MzansiLegal provides legal information not legal advice, and includes a Legal Aid SA contact number.
5. **Given** a user, **When** they click a related question chip below an answer, **Then** that question is submitted as a new message in the same thread.
6. **Given** a user who prefers voice input, **When** they tap the microphone icon in the input bar, **Then** speech is captured and transcribed into the input field.

---

### User Story 3 — Analyse a Contract (Priority: P2)

A user uploads a contract document, receives a plain-language summary and a risk score, and can explore red flags, caution items, and standard clauses.

**Why this priority**: Contract analysis is a key differentiator; it builds on the Ask experience.

**Independent Test**: Navigate to /contracts, upload a PDF lease agreement, verify score, summary, red flags, and caution sections appear on the detail page.

**Acceptance Scenarios**:

1. **Given** a user on the Contracts list page, **When** they upload a document, **Then** it appears in the list with an "Analysing" status indicator.
2. **Given** analysis is complete, **When** the user opens the contract detail page, **Then** they see a circular risk score (out of 100), upload date, analysis duration, and document metadata (pages, clauses, language).
3. **Given** a contract detail page, **When** viewing the Plain-language summary card, **Then** a concise paragraph describes the contract in plain language and an inline chat input is available for follow-up questions.
4. **Given** red flags exist, **When** the user views the Red Flags section, **Then** each flag shows a title, explanation, and the specific legislation violated (act name and section).
5. **Given** caution items exist, **When** the user views the Caution section, **Then** each item shows a title and explanation with a warning icon.
6. **Given** standard clauses are clean, **When** the user views the Standard Clauses section, **Then** a single "All standard clauses are in order" summary is displayed.

---

### User Story 4 — Explore My Rights (Priority: P2)

A user browses legal rights by category, tracks their learning progress, and expands individual rights cards to read detailed explanations.

**Why this priority**: Rights explorer drives repeated engagement and user education.

**Independent Test**: Navigate to /rights, verify category filter tabs, progress bar, and expandable rights cards all function.

**Acceptance Scenarios**:

1. **Given** a user on the My Rights page, **When** the page loads, **Then** they see a knowledge progress bar showing how many of 20 topics they have explored (e.g., "7 of 20 explored — 35%").
2. **Given** a user, **When** they select a category tab (All / Employment / Housing / Consumer / Debt & Credit / Tax / Privacy), **Then** only rights cards matching that category are shown.
3. **Given** a collapsed rights card, **When** the user taps the expand (+) button, **Then** the card expands to show a full plain-language explanation, a pull-quote from the legislation, and action buttons.
4. **Given** an expanded rights card, **When** the user taps "Ask a follow-up", **Then** they are navigated to the Ask page with the right title pre-filled as the question.
5. **Given** an expanded rights card, **When** the user taps "Listen in [language]", **Then** the explanation is read aloud in the selected language.
6. **Given** an expanded rights card, **When** the user taps "Share", **Then** a shareable link or share sheet is triggered.

---

### User Story 5 — View Conversation History (Priority: P3)

A user reviews their past questions and answers in a chronological history list and can return to any thread.

**Why this priority**: History enables users to revisit previous advice without re-asking.

**Independent Test**: Navigate to /history, verify a list of past conversations with timestamps and question previews; clicking one opens the thread.

**Acceptance Scenarios**:

1. **Given** a user on the History page with prior conversations, **When** the page loads, **Then** each conversation is listed with the first question, date, and language used.
2. **Given** a history item, **When** the user clicks it, **Then** they are navigated to the Ask page showing the full conversation thread.
3. **Given** a brand-new account with no history, **When** the user visits the History page, **Then** an empty state prompts them to ask their first question.

---

### User Story 6 — Sign In / Register (Priority: P1)

A new or returning user authenticates via the Sign In or Register pages to access personalised features.

**Why this priority**: Authentication gates all personalised features (history, rights progress, contract uploads).

**Independent Test**: Navigate to /auth, complete the register form, verify redirect to home page as regular user; log in as admin, verify redirect to dashboard.

**Acceptance Scenarios**:

1. **Given** an unauthenticated visitor, **When** they click "Get started", **Then** they are directed to the Register page.
2. **Given** the Register page, **When** the user fills in name, email, and password and submits, **Then** an account is created and they are redirected to the Home page.
3. **Given** the Sign In page, **When** a user enters valid credentials, **Then** they are authenticated and redirected based on role (admin → dashboard, user → home).
4. **Given** the Sign In page, **When** a user enters invalid credentials, **Then** a clear inline error message is shown without clearing the email field.
5. **Given** a signed-in user, **When** they click the language selector in the navbar, **Then** the UI switches to the selected language (English, isiZulu, Sesotho, Afrikaans) immediately.

---

### User Story 7 — Admin Dashboard (Priority: P3)

An admin user views aggregate platform statistics, usage trends, and recent activity.

**Why this priority**: Dashboard enables platform monitoring without direct database access.

**Independent Test**: Log in as admin, navigate to /admin/dashboard, verify summary cards, chart, and recent activity list render.

**Acceptance Scenarios**:

1. **Given** an admin on the Dashboard, **When** the page loads, **Then** summary cards show total questions answered, active users, contracts analysed, and documents indexed.
2. **Given** a dashboard, **When** the admin views the insight chart section, **Then** a chart displays question volume over the past 30 days.
3. **Given** a dashboard, **When** the admin views recent activity, **Then** a list shows the last 10 user questions or contract uploads with timestamps and user identifiers.
4. **Given** a non-admin user, **When** they attempt to access /admin/dashboard, **Then** they are redirected to the home page.

---

### Edge Cases

- What happens when a user submits an empty question in the Ask page?
- How does the contract detail page look when there are zero red flags or zero caution items?
- What happens if a user uploads a non-PDF file or a corrupted document?
- How does the rights progress bar behave when all 20 topics have been explored (100%)?
- What is shown in the History page for a brand-new account with zero conversations?
- How does the language selector behave if a translation key is missing for a particular string?
- What happens if the AI answer stream fails or times out mid-response?
- How does the navbar display on tablet viewports where the full link list may overflow?

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST render a public landing page accessible without authentication, displaying hero, stats, feature cards, category grid, and trending questions sections.
- **FR-002**: System MUST provide a shared navigation bar across all pages with links to Home, Ask, Contracts, My Rights, History, a language selector, and a context-sensitive CTA ("Get started" when unauthenticated, user menu when authenticated).
- **FR-003**: System MUST provide an Ask/Chat page where users submit text questions and receive AI-generated answers with citation references.
- **FR-004**: System MUST display answers with an expandable sources section listing legislation act names and section numbers.
- **FR-005**: System MUST offer a voice input option on the Ask page to capture spoken questions.
- **FR-006**: System MUST offer a "Listen in [language]" option on answers and expanded rights cards.
- **FR-007**: System MUST provide a Contracts list page where authenticated users upload documents and view analysis status.
- **FR-008**: System MUST provide a Contract Detail page showing risk score, plain-language summary, red flags, caution items, and standard clauses.
- **FR-009**: System MUST provide a My Rights page with category filter tabs, a knowledge progress bar, and expandable rights cards.
- **FR-010**: System MUST provide a History page listing past conversations for the authenticated user with navigation to individual threads.
- **FR-011**: System MUST provide Sign In and Register pages with form validation and role-based redirect on success.
- **FR-012**: System MUST provide an Admin Dashboard accessible only to admin users, with aggregate statistics, a usage chart, and recent activity.
- **FR-013**: System MUST apply a consistent design system across all pages: dark olive-green primary colour, warm off-white background, serif headlines, rounded cards, and consistent spacing scale.
- **FR-014**: System MUST support four languages (English, isiZulu, Sesotho, Afrikaans) across all translatable UI strings.
- **FR-015**: System MUST redirect unauthenticated users attempting to access protected pages to the Sign In page.
- **FR-016**: System MUST redirect non-admin users attempting to access admin-only pages to the home page.

### Key Entities

- **Page**: A distinct route within the application (Home, Ask, Contracts, Contracts Detail, My Rights, History, Auth, Admin Dashboard).
- **NavigationBar**: Shared layout component present on all pages; adapts its CTA and active link indicator based on authentication state and current route.
- **RightsCard**: Collapsible component displaying a single legal right with title, legislation citation, body text, pull-quote, and action buttons.
- **ContractAnalysis**: Structured analysis result containing a risk score, summary text, a list of red flags (with legislation references), a list of caution items, and standard clause status.
- **ConversationThread**: A sequence of user questions and AI answers belonging to one session; stored per user in history.
- **DesignToken**: Named values from the design system (colour, spacing, typography, border-radius) applied consistently across all components via the theming layer.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All eight routes (Home, Ask, Contracts List, Contract Detail, My Rights, History, Auth, Admin Dashboard) are accessible and render without errors.
- **SC-002**: Every page renders correctly at desktop viewport (1280px+) and is usable at tablet viewport (768px) without horizontal scroll.
- **SC-003**: The shared navigation bar, colour palette, typography scale, and card styles are visually consistent across 100% of pages.
- **SC-004**: Language switching updates all UI strings on the current page within 1 second without a full page reload.
- **SC-005**: Unauthenticated users attempting to access any protected route are redirected to the auth page in 100% of attempts.
- **SC-006**: Admin-only routes reject non-admin users 100% of the time.
- **SC-007**: All interactive elements (buttons, tabs, expand toggles, language selector) respond to user interaction without visible delay.
- **SC-008**: The Ask page renders the first AI answer content within 3 seconds of question submission under normal conditions.

---

## Assumptions

- The existing authentication backend (ABP Zero JWT) and RAG Q&A backend are operational and accessible via the configured API proxy.
- JWT tokens are stored in cookies (`ml_token`, `ml_user`) as established by feat/018-auth-landing-page.
- The design system colour palette (dark olive green primary, warm off-white background, terracotta/tan accents) and typography (serif headlines, sans-serif body text) are derived from the provided design reference PDFs.
- The nine category cards on the landing page are static content; dynamic category management is out of scope for this feature.
- The five trending questions on the landing page are static placeholder data initially; a dynamic trending endpoint is out of scope.
- The Admin Dashboard uses existing backend aggregate endpoints or representative mock data if endpoints are not yet available.
- Voice input uses the browser's native Web Speech API; no additional third-party speech service integration is required.
- Voice output (TTS) uses the browser's native SpeechSynthesis API.
- The History page reads from the existing Conversations and Questions backend tables.
- Mobile-first responsive layout (below 768px) is out of scope for this iteration; desktop and tablet are the primary targets.
- No new npm packages are required beyond those already installed: Ant Design 6.x, next-intl 4.x, lucide-react, antd-style createStyles.
