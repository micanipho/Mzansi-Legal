# Tasks: Frontend Shell Polish

**Input**: Design documents from `/specs/011-frontend-shell-polish/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/, quickstart.md

**Tests**: No automated tests were explicitly requested in the specification. Validation for this feature is based on lint/build checks and the manual route, locale, and responsive flows defined in quickstart.md.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Frontend Infrastructure)

**Purpose**: Add the shared dependency and create the basic route/component structure required by the planned shell work.

- [X] T001 Install `@ant-design/charts` and update `frontend/package.json` plus `frontend/package-lock.json`
- [X] T002 Create the planned route folders and shell component folders under `frontend/src/app/[locale]/ask/`, `frontend/src/app/[locale]/contracts/[id]/`, `frontend/src/app/[locale]/admin/dashboard/`, and `frontend/src/components/dashboard/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish the shared route, locale, and visual-shell primitives that every user story depends on.

**CRITICAL**: No user story work can begin until this phase is complete.

- [X] T003 Create a shared localized route definition for home, ask, contracts, contract detail, rights, admin dashboard, and legacy chat handling in `frontend/src/i18n/routing.ts` and `frontend/src/components/layout/AppNavbar.tsx`
- [X] T004 Create global CSS variable design tokens for the organic palette, surface colors, text colors, and texture settings in `frontend/src/styles/globals.css` and `frontend/src/app/globals.css`
- [X] T005 Create a reusable paper-grain background layer component and shell-friendly texture styling in `frontend/src/components/layout/OrganicBackground.tsx` and `frontend/src/styles/globals.css`
- [X] T006 Refactor Ant Design theme token generation to read from the shared shell token model in `frontend/src/styles/theme.ts` and `frontend/src/components/providers/AntdProvider.tsx`
- [X] T007 [P] Add missing shell copy keys for ask, contract detail, admin dashboard, and locale-switch labels in `frontend/src/messages/en.json`, `frontend/src/messages/zu.json`, `frontend/src/messages/st.json`, and `frontend/src/messages/af.json`

**Checkpoint**: Shared shell tokens, reusable background treatment, and message structure are ready for route and page implementation.

---

## Phase 3: User Story 1 - Reach Core Journeys Through the Correct Routes (Priority: P1) MVP

**Goal**: Expose the core demo journeys on intentional localized routes: `/ask`, `/contracts/[id]`, and `/admin/dashboard`, while keeping old `/chat` links usable.

**Independent Test**: Start the frontend and verify that `/{locale}/ask`, `/{locale}/contracts`, `/{locale}/contracts/{id}`, `/{locale}/rights`, and `/{locale}/admin/dashboard` all load usable pages, and that `/{locale}/chat` lands in the ask journey without behaving like a separate primary destination.

### Implementation for User Story 1

- [X] T008 [US1] Add the canonical ask page route by moving or re-exporting the current Q&A experience into `frontend/src/app/[locale]/ask/page.tsx`
- [X] T009 [US1] Add compatibility handling for legacy `/chat` URLs in `frontend/src/app/[locale]/chat/page.tsx` and `frontend/src/proxy.ts`
- [X] T010 [US1] Create the contract detail page shell for a single analysis record in `frontend/src/app/[locale]/contracts/[id]/page.tsx`
- [X] T011 [US1] Update the contracts list to link into canonical contract detail routes in `frontend/src/app/[locale]/contracts/page.tsx`
- [X] T012 [US1] Create the admin dashboard route shell in `frontend/src/app/[locale]/admin/dashboard/page.tsx`
- [X] T013 [US1] Update shell navigation and primary call-to-action links to point to canonical routes in `frontend/src/components/layout/AppNavbar.tsx`, `frontend/src/app/[locale]/page.tsx`, and any route references in `frontend/src/app/[locale]/contracts/page.tsx`

**Checkpoint**: The core product story can now be demoed through the intended route structure.

---

## Phase 4: User Story 2 - Change Language From Anywhere (Priority: P1)

**Goal**: Make the language selector functional so users can switch locale and stay in the same journey whenever possible.

**Independent Test**: Open the ask page, rights page, contract detail page, and admin dashboard, change locale from the navbar, and verify the route prefix and visible shell copy update while staying in the same journey.

### Implementation for User Story 2

- [X] T014 [US2] Create a reusable locale-switch mapping helper that preserves the current route family, route params, and safe query string in `frontend/src/i18n/routing.ts` and a new helper under `frontend/src/i18n/`
- [X] T015 [US2] Implement an interactive locale switcher UI in `frontend/src/components/layout/AppNavbar.tsx`
- [X] T016 [US2] Update layout-level shell rendering to ensure locale-switched pages resolve correctly in `frontend/src/app/[locale]/layout.tsx` and `frontend/src/app/page.tsx`
- [X] T017 [US2] Localize navbar labels, locale names, and route-facing shell copy in `frontend/src/messages/en.json`, `frontend/src/messages/zu.json`, `frontend/src/messages/st.json`, and `frontend/src/messages/af.json`

**Checkpoint**: Multilingual switching works from the shared shell without forcing users back to the home page.

---

## Phase 5: User Story 3 - Experience a Cohesive Product Shell (Priority: P2)

**Goal**: Apply one consistent visual identity across the main page families using CSS variables, shared typography, the paper-grain layer, and mobile-safe shell behavior.

**Independent Test**: Visit localized home, ask, contracts, contract detail, rights, and admin dashboard pages on desktop and mobile widths and verify they all share the same palette, typography, background treatment, and usable navigation without horizontal scrolling.

### Implementation for User Story 3

- [X] T018 [P] [US3] Refactor the home and rights pages to consume the shared shell tokens and background system in `frontend/src/app/[locale]/page.tsx` and `frontend/src/app/[locale]/rights/page.tsx`
- [X] T019 [P] [US3] Refactor the ask and contracts pages to consume the shared shell tokens and background system in `frontend/src/app/[locale]/ask/page.tsx`, `frontend/src/app/[locale]/contracts/page.tsx`, and `frontend/src/app/[locale]/contracts/[id]/page.tsx`
- [X] T020 [US3] Make the floating pill navbar responsive and keyboard-friendly for smaller viewports in `frontend/src/components/layout/AppNavbar.tsx` and `frontend/src/styles/globals.css`
- [X] T021 [US3] Apply the updated shell tokens and textured background consistently from the locale layout in `frontend/src/app/[locale]/layout.tsx` and `frontend/src/components/layout/OrganicBackground.tsx`

**Checkpoint**: The application looks and behaves like one coherent product shell across all primary journeys.

---

## Phase 6: User Story 4 - Review Platform Status From an Admin Entry Point (Priority: P3)

**Goal**: Give admins a dashboard route with summary content and a visual insight area that supports platform storytelling during demos.

**Independent Test**: Open `/{locale}/admin/dashboard` and verify the page shows a meaningful dashboard shell with stat summaries, a chart-ready insight area, and an intentional empty/fallback state when no live metrics are present.

### Implementation for User Story 4

- [X] T022 [P] [US4] Create reusable admin dashboard summary card and section components in `frontend/src/components/dashboard/`
- [X] T023 [US4] Add the chart-backed insight panel using `@ant-design/charts` in `frontend/src/components/dashboard/` and wire it into `frontend/src/app/[locale]/admin/dashboard/page.tsx`
- [X] T024 [US4] Add localized dashboard content, empty states, and supporting admin copy in `frontend/src/messages/en.json`, `frontend/src/messages/zu.json`, `frontend/src/messages/st.json`, and `frontend/src/messages/af.json`

**Checkpoint**: The admin entry point communicates platform value, not just page scaffolding.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Validate the full shell against the planned route, locale, and responsive expectations.

- [X] T025 [P] Update the frontend quickstart or README guidance for canonical ask routing and dashboard validation in `specs/011-frontend-shell-polish/quickstart.md` and `frontend/README.md`
- [X] T026 Run validation commands `npm run lint` and `npm run build` from `frontend/` and fix any issues in touched frontend files
- [X] T027 Run the manual validation flow from `specs/011-frontend-shell-polish/quickstart.md` covering canonical routes, legacy `/chat`, locale switching, dashboard rendering, and mobile-width shell behavior

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - blocks all user stories
- **User Stories (Phases 3-6)**: Depend on Foundational completion
- **Polish (Phase 7)**: Depends on the desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Foundational - establishes the canonical route structure and page shells
- **User Story 2 (P1)**: Starts after Foundational - depends on canonical route definitions from T003 and is easier to verify once US1 routes exist
- **User Story 3 (P2)**: Starts after Foundational - should layer onto the canonical routes and shell created in US1
- **User Story 4 (P3)**: Starts after Foundational - depends on the dashboard route from US1 and shared shell tokens from Phase 2

### Within Each User Story

- Route files before shell link rewiring
- Locale mapping before interactive locale-switch UI
- Shared shell token refactors before responsive polish pass
- Dashboard components before dashboard chart integration

### Parallel Opportunities

- T007 can run in parallel with the other foundational styling work once message structure is agreed
- T018 and T019 can run in parallel because they target different page groups
- T022 can run in parallel with other dashboard-copy work before T023 integrates the chart
- T025 can run in parallel with implementation wrap-up while final validation is being prepared

---

## Parallel Example: User Story 3

```bash
# These page-family refactors can run in parallel after the shared shell tokens are in place:
Task: "Refactor the home and rights pages to consume the shared shell tokens and background system in frontend/src/app/[locale]/page.tsx and frontend/src/app/[locale]/rights/page.tsx"
Task: "Refactor the ask and contracts pages to consume the shared shell tokens and background system in frontend/src/app/[locale]/ask/page.tsx, frontend/src/app/[locale]/contracts/page.tsx, and frontend/src/app/[locale]/contracts/[id]/page.tsx"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. Stop and validate the canonical route structure, including legacy `/chat` compatibility
5. Demo the cleaned-up route story if needed

### Incremental Delivery

1. Setup + Foundational -> shared shell primitives are ready
2. Add User Story 1 -> route structure and page reachability are demo-ready
3. Add User Story 2 -> multilingual shell switching becomes believable
4. Add User Story 3 -> product polish and visual coherence improve the perceived quality
5. Add User Story 4 -> admin dashboard strengthens the platform narrative

### Parallel Team Strategy

With multiple contributors:

1. One contributor completes Setup + Foundational
2. After that:
   - Contributor A handles canonical routes and contract detail shell (US1)
   - Contributor B handles locale switch behavior and translations (US2)
   - Contributor C handles shell token application and responsive styling (US3)
3. Dashboard work (US4) can proceed once the dashboard route exists

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [US#] labels map each task back to the feature specification
- The current `/chat` route is treated as compatibility behavior, not the preferred shell destination
- This feature is intentionally frontend-first and does not require backend persistence changes to deliver demo value
