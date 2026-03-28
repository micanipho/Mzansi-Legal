# Feature Specification: Frontend Shell Polish

**Feature Branch**: `[011-frontend-shell-polish]`  
**Created**: 2026-03-28  
**Status**: Draft  
**Input**: User description: "Rename or add /ask instead of /chat. Add /contracts/[id]. Add /admin/dashboard. Make the locale switcher actually change locale. Move design colors into CSS variables and add the paper-grain overlay. Install @ant-design/charts if you still want that dependency included."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Reach Core Journeys Through the Correct Routes (Priority: P1)

A visitor can move through the main product journeys using route names that match the product language used in demos and navigation, including the ask flow, contract list, contract detail view, rights explorer, and admin dashboard.

**Why this priority**: Demo clarity depends on the product feeling intentional rather than improvised. If the route structure, navigation labels, and available pages do not match the spoken demo story, the experience feels unfinished.

**Independent Test**: Open the localized app shell and navigate to home, ask, contracts list, contract detail, rights, and admin dashboard using visible navigation and direct URLs. Each destination must load a usable page with no broken links or dead-end routes.

**Acceptance Scenarios**:

1. **Given** a visitor is on the localized home page, **When** they choose the primary question-and-answer journey from navigation or a call to action, **Then** they are taken to the localized `/ask` route rather than a `/chat` route.
2. **Given** a visitor is on the contracts listing page, **When** they open a specific contract result, **Then** they are taken to a localized contract detail page that represents one contract analysis record.
3. **Given** an admin user opens the localized admin dashboard route directly, **When** the page loads, **Then** they see a dashboard shell rather than a missing-route or placeholder error.

---

### User Story 2 - Change Language From Anywhere (Priority: P1)

A multilingual user can switch between supported languages from the global navigation and remain inside the same part of the product without manually editing the URL.

**Why this priority**: Multilingual behavior is a constitutional requirement for this product and one of the clearest signals that the platform is more than a generic chatbot.

**Independent Test**: Open any primary localized route, switch to another supported language using the language selector, and verify the page reloads in the selected language while keeping the user in the same journey.

**Acceptance Scenarios**:

1. **Given** a user is on a supported localized page, **When** they choose a different language from the global selector, **Then** the application reloads the equivalent page in the selected language.
2. **Given** a user changes language once, **When** they continue navigating through the application, **Then** subsequent in-app navigation remains in the selected language until they change it again.

---

### User Story 3 - Experience a Cohesive Product Shell (Priority: P2)

A visitor sees a consistent visual system across the primary page families, including shared color tokens, typography, background atmosphere, and a paper-like texture treatment that makes the product feel designed rather than scaffolded.

**Why this priority**: The MD demo depends as much on product confidence as feature coverage. A cohesive shell makes every page feel part of one credible platform.

**Independent Test**: Visit the localized home, ask, contracts, contract detail, rights, and admin dashboard pages on desktop and mobile widths and verify that each page uses the same palette, typography, background treatment, and shared shell components.

**Acceptance Scenarios**:

1. **Given** a visitor navigates between primary pages, **When** each page loads, **Then** the page presents the same visual identity rather than falling back to generic or inconsistent styling.
2. **Given** a visitor views the application on a small screen, **When** the global shell renders, **Then** core layout elements remain readable and usable without horizontal scrolling.

---

### User Story 4 - Review Platform Status From an Admin Entry Point (Priority: P3)

An admin can open a dashboard route that presents a high-level operational view of the platform and supports future metrics storytelling during demos.

**Why this priority**: Even a lightweight dashboard shell expands the product story from a single-user assistant into a platform with operational value and oversight.

**Independent Test**: Open the localized admin dashboard route and verify it contains an overview structure with summary content and at least one visual insight area suitable for platform metrics.

**Acceptance Scenarios**:

1. **Given** an admin opens the dashboard route, **When** the page loads, **Then** they see summary sections that communicate platform oversight rather than a blank page.
2. **Given** the dashboard is shown during a demo, **When** the presenter references operational insights, **Then** the page contains a dedicated visual area that can represent those insights.

---

### Edge Cases

- What happens when a user switches language while viewing a route that is missing translated copy for some labels?
- How does the shell behave when a user visits an outdated `/chat` link from a bookmark or shared URL?
- What happens when a contract detail identifier is missing, invalid, or refers to a contract the user cannot access?
- How does the language selector behave when the current route has query parameters or nested navigation state?
- What does the admin dashboard show when no live metrics are available yet?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST expose the primary question-and-answer journey at a localized `/ask` route.
- **FR-002**: The system MUST ensure that users who navigate from the shell, calls to action, or bookmarked legacy `/chat` links are taken to the ask experience without confusion.
- **FR-003**: The system MUST provide a localized contracts detail route at `/contracts/[id]` that presents a single contract analysis view.
- **FR-004**: The system MUST provide a localized admin dashboard route at `/admin/dashboard`.
- **FR-005**: The system MUST include all required page families in the localized shell: home, ask, contracts list, contract detail, rights, and admin dashboard.
- **FR-006**: The global navigation MUST include a visible language selector that allows users to switch among English, isiZulu, Sesotho, and Afrikaans.
- **FR-007**: When a user changes language from the global selector, the system MUST keep the user in the equivalent product journey whenever that journey exists in the selected language.
- **FR-008**: The system MUST preserve the selected language during subsequent in-app navigation until the user chooses a different language.
- **FR-009**: All required navigation labels, page headings, and primary action labels in the shell MUST be available in all four supported languages.
- **FR-010**: The visual system MUST define shared brand color tokens at the global shell level so that primary pages use one consistent palette.
- **FR-011**: The shell MUST include a paper-like background texture treatment that is visible across the primary page families without reducing readability.
- **FR-012**: The home, ask, contracts, contract detail, rights, and admin dashboard pages MUST all use the shared typography, color tokens, and atmospheric background treatment.
- **FR-013**: The admin dashboard MUST present a structured overview suitable for platform storytelling, including summary content and a visual insight area.
- **FR-014**: When dashboard data is unavailable, the admin dashboard MUST show an intentional empty or placeholder state rather than an error-like blank surface.
- **FR-015**: Required shell routes and navigation paths MUST be usable on both desktop and mobile-width layouts.

### Key Entities *(include if feature involves data)*

- **Localized Shell Route**: A user-facing destination within the product shell identified by journey type, locale, and addressable path.
- **Language Selection State**: The currently active user language that determines visible route prefixes and shell copy.
- **Contract Detail View**: A single-record presentation of one contract analysis result, including summary content and follow-up actions.
- **Admin Dashboard View**: A high-level operational page made up of summary sections and at least one visual insight region.
- **Visual Token Set**: The shared palette and surface styling rules used to keep all primary pages visually consistent.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of required localized routes load a usable page with no missing-route errors for home, ask, contracts list, contract detail, rights, and admin dashboard.
- **SC-002**: In usability testing, a first-time tester can reach the ask flow, a contract detail page, and the admin dashboard from the home page in under 2 minutes without verbal guidance.
- **SC-003**: 100% of language changes initiated from the global selector update the visible shell to the selected supported language within the same journey when that journey exists.
- **SC-004**: Visual review confirms that all six required page families use the shared brand palette, typography, and paper-style background treatment.
- **SC-005**: On representative mobile and desktop viewport checks, all required shell pages remain readable and navigable with no horizontal scrolling in the primary content area.

## Assumptions

- Existing page content for home, rights, contracts list, and question-and-answer flows remains the baseline content unless explicitly replaced by this feature.
- The current history page may remain in the product, but it is not part of the required milestone acceptance for this feature.
- The contract detail route may initially present a representative or existing contract analysis record while deeper persistence work continues elsewhere.
- The admin dashboard in this feature is a frontend-ready shell and does not require complete live backend metrics to deliver demo value.
- Existing authentication and authorization rules for admin-only areas will be reused rather than redesigned in this feature.
