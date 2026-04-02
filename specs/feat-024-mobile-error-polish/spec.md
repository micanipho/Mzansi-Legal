# Feature Specification: Mobile Error Polish

**Feature Branch**: `feat/024-mobile-error-polish`  
**Created**: 2026-04-02  
**Status**: Draft  
**Input**: User description: "The app needs to work on mobile devices (many South Africans access the internet primarily via phone) and handle errors gracefully. Responsive: all grids collapse to single column on mobile, navbar switches to hamburger menu on mobile, chat input bar is full-width on mobile, contract score ring scales down appropriately, category cards stack vertically, touch targets minimum 44px. Error handling: API call failures show friendly error messages (not raw errors), network offline detection with retry prompt, empty states for no questions yet, no contracts yet, no search results, loading skeletons for all data-dependent sections, and a 404 page for invalid routes. Alternative: could build a separate mobile app, but a responsive web app reaches more users without app store distribution. Milestone: Polish. Acceptance: app fully usable on iPhone SE (smallest common screen), all errors handled gracefully."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Mobile-First Core Journeys (Priority: P1)

As a person using the app on a small phone, I can read content, navigate between key areas, ask legal questions, and review results without horizontal scrolling, clipped controls, or touch interactions that are too small to use comfortably.

**Why this priority**: Mobile access is the primary access path for many intended users. If the app is difficult to use on a small phone, the core product is effectively unavailable to a large share of the audience.

**Independent Test**: Open the app on an iPhone SE-sized viewport and complete the primary journeys across navigation, question asking, browsing categories, and viewing contract results without layout breakage or unusable controls.

**Acceptance Scenarios**:

1. **Given** a user opens any grid-based page on a small phone, **When** the viewport matches a compact mobile width, **Then** multi-column layouts collapse into a single readable column without horizontal scrolling.
2. **Given** a user is browsing on a small phone, **When** they open the main navigation, **Then** the navigation is available through a compact menu pattern that keeps links reachable and readable.
3. **Given** a user is asking a question on a small phone, **When** the chat composer is shown, **Then** the input bar spans the available width and remains easy to tap and type into.
4. **Given** a user is viewing contract analysis results on a small phone, **When** the score visualization appears, **Then** it scales to fit the screen without clipping or overlapping nearby content.
5. **Given** a user is browsing category or topic cards on a small phone, **When** the page loads, **Then** the cards stack vertically and preserve minimum touch target size for interactive elements.

---

### User Story 2 - Friendly Recovery From Failures (Priority: P2)

As a user, I receive understandable feedback when something goes wrong so I know what happened, what I can do next, and whether retrying is possible.

**Why this priority**: A polished experience must fail safely and clearly. Raw technical errors or silent failures reduce trust, especially for users on unstable mobile connections.

**Independent Test**: Simulate offline conditions, failed requests, and empty results while navigating the app and confirm the user always sees a friendly message, an appropriate next step, and a retry path where relevant.

**Acceptance Scenarios**:

1. **Given** an action depends on data from the server, **When** the request fails, **Then** the user sees a clear, friendly error message instead of raw technical details.
2. **Given** the user loses internet connectivity, **When** they attempt a data-dependent action, **Then** the app detects the offline state, explains the problem in plain language, and offers a retry prompt once connectivity returns.
3. **Given** a section has no available content yet, **When** the user opens that section, **Then** the app shows a purpose-built empty state explaining what is missing and what to do next.
4. **Given** a data-dependent section is loading, **When** the screen first appears, **Then** the app shows a loading skeleton that matches the expected content structure instead of flashing empty or broken layouts.

---

### User Story 3 - Safe Navigation and Discoverability (Priority: P3)

As a user following links or typing a route manually, I am guided back to valid content when a page does not exist, rather than reaching a dead end.

**Why this priority**: Invalid links and mistyped URLs are common on mobile devices. A dedicated not-found experience keeps users oriented and reduces abandonment.

**Independent Test**: Visit an invalid route directly from desktop and phone-sized viewports and confirm the app shows a clear not-found experience with working navigation back to a useful destination.

**Acceptance Scenarios**:

1. **Given** a user opens an invalid route, **When** the page cannot be found, **Then** the app shows a dedicated not-found page with a clear explanation and a route back to key app areas.
2. **Given** a user searches or filters for content that does not match any results, **When** the result set is empty, **Then** the app shows a no-results state that distinguishes this from loading or failure.

---

### Edge Cases

- A user opens the app on a very small phone in portrait mode and rotates the device while a data-dependent section is still loading.
- A user loses connectivity after opening a page but before submitting a question, search, or retry action.
- A section returns an empty dataset after previously showing content in the same session.
- A user follows a deep link to a missing route from an external source or messaging app.
- A user encounters repeated intermittent failures and needs a consistent retry path without losing context.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST present all grid-based content in a single-column layout on compact mobile viewports.
- **FR-002**: The system MUST provide a compact navigation pattern on mobile that keeps primary destinations discoverable and reachable without requiring horizontal scrolling.
- **FR-003**: The system MUST ensure the question input experience remains full-width and usable on compact mobile viewports.
- **FR-004**: The system MUST adapt score-based visual summaries so they remain fully visible and legible on compact mobile viewports.
- **FR-005**: The system MUST stack category and topic cards vertically on compact mobile viewports.
- **FR-006**: The system MUST provide touch targets of at least 44 by 44 pixels for interactive controls across mobile layouts.
- **FR-007**: The system MUST prevent horizontal scrolling on core user-facing pages under the smallest supported mobile viewport.
- **FR-008**: The system MUST replace raw technical API failure text with friendly, task-specific user messages.
- **FR-009**: The system MUST detect when the user is offline during data-dependent actions and present a retry-oriented recovery message.
- **FR-010**: The system MUST provide explicit empty states for conversation history with no questions, contract history with no analyses, and searches or filters with no matching results.
- **FR-011**: The system MUST provide loading skeletons for every data-dependent section before content is available.
- **FR-012**: The system MUST provide a dedicated not-found page for invalid routes with a path back to valid parts of the app.
- **FR-013**: The system MUST distinguish between loading, empty, offline, and failed states so users can understand the difference between each condition.
- **FR-014**: The system MUST preserve the current user context where practical after a recoverable failure so users can retry without re-entering information unnecessarily.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can complete the primary journeys of navigation, question asking, category browsing, and contract result viewing on an iPhone SE-sized viewport without horizontal scrolling or clipped interactive elements.
- **SC-002**: 100% of user-visible request failures display friendly recovery messaging with no raw server or technical error text exposed to end users.
- **SC-003**: 100% of core data-dependent screens present an intentional state for loading, empty results, and failure conditions.
- **SC-004**: 100% of interactive controls in the polished mobile experience meet the minimum 44 by 44 pixel touch target requirement.
- **SC-005**: Users reaching an invalid route can return to a valid destination in one interaction from the not-found page.

## Assumptions

- The existing web app remains the only delivery channel for this milestone; no separate native mobile app is introduced.
- Existing authentication, search, chat, contract, and history flows remain functionally the same; this milestone improves usability and resilience rather than changing business workflows.
- The smallest supported viewport for acceptance is equivalent to an iPhone SE in portrait orientation.
- Friendly error copy can vary by context as long as it is understandable, non-technical, and gives the user a sensible next step.
- Existing destinations such as the home, ask, history, rights, and contracts areas remain the appropriate recovery targets from error and not-found states.
