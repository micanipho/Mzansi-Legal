# Tasks: Full UI Pages — MzansiLegal Design System

**Input**: Design documents from `specs/feat/019-full-ui-pages/`
**Prerequisites**: plan.md ✅ | spec.md ✅ | research.md ✅ | data-model.md ✅ | contracts/ ✅

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US7)
- All paths relative to repository root

---

## Phase 1: Setup

**Purpose**: Restore the working tree and verify the dev environment is ready.

- [x] T001 Restore all frontend files from git: `git checkout HEAD -- frontend/`
- [x] T002 Install dependencies: `cd frontend && npm install`
- [x] T003 [P] Create `frontend/.env.local` with `NEXT_PUBLIC_API_BASE=http://localhost:21021`
- [x] T004 Verify dev server starts: `cd frontend && npm run dev` → `http://localhost:3000/en`

**Checkpoint**: Dev server running, all routes accessible (may show partial data)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Verify and harden the design system, shared layout, and auth infrastructure before working on individual pages.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T005 Verify CSS variables in `frontend/src/styles/globals.css` match design tokens (colours, fonts, shadows) from the design reference PDFs
- [x] T006 [P] Verify TypeScript token exports in `frontend/src/styles/theme.ts` — ensure `C`, `R`, `fontSerif`, `fontSans`, `shadowOrganic` are complete and correctly typed
- [x] T007 Verify `AuthProvider` in `frontend/src/components/providers/AuthProvider.tsx` correctly reads `ml_token` and `ml_user` cookies on mount and exposes `{ user, isLoading, signIn, signOut }` via context
- [x] T008 Verify `useAuth` hook in `frontend/src/hooks/useAuth.ts` re-exports the auth context correctly
- [x] T009 [P] Verify `appRoutes` and `createLocalizedPath` in `frontend/src/i18n/routing.ts` contain all 8 routes: home, ask, contracts, rights, history, auth, adminDashboard, and the `[id]` contract detail helper
- [x] T010 Verify root layout `frontend/src/app/[locale]/layout.tsx` wraps all pages with `AntdProvider`, `AuthProvider`, and `AppNavbar`
- [x] T011 Verify `AppNavbar` in `frontend/src/components/layout/AppNavbar.tsx`:
  - Active link indicator matches current pathname
  - Language selector switches locale correctly using `buildLocaleSwitchHref`
  - Shows "Get started" when unauthenticated; shows user initials + dropdown when authenticated
  - Admin users see the Dashboard link in the nav links
- [x] T012 [P] Verify `AntdProvider` in `frontend/src/components/providers/AntdProvider.tsx` applies the `antdTheme` config from `frontend/src/styles/theme.ts`

**Checkpoint**: Design system tokens confirmed, auth context working, navbar functional across all locales

---

## Phase 3: User Story 6 — Sign In / Register (Priority: P1) 🔐

**Goal**: Users can authenticate via the Sign In and Register forms, with role-based redirect on success.

**Independent Test**: Navigate to `/en/auth`, complete the Register form with a test email and password, verify redirect to `/en/`. Then sign in with admin credentials and verify redirect to `/en/admin/dashboard`.

### Implementation

- [ ] T013 [US6] Verify auth page layout `frontend/src/app/[locale]/auth/layout.tsx` — ensures no AppNavbar renders on auth pages
- [ ] T014 [US6] Verify auth page `frontend/src/app/[locale]/auth/page.tsx`:
  - Tab switching between Sign In and Register via URL hash (`#register` / `#sign-in`)
  - Fraunces serif logo/brand treatment visible
  - "← Back to home" link at top using `createLocalizedPath`
- [ ] T015 [US6] Verify `SignInForm` in `frontend/src/components/auth/SignInForm.tsx`:
  - Email + password fields with Ant Design `Form` and `Form.Item`
  - Inline validation: both fields required, email format checked
  - On success: call `useAuth().signIn()` → redirect based on `user.isAdmin`
  - On error: display inline error without clearing email field
- [ ] T016 [US6] Verify `RegisterForm` in `frontend/src/components/auth/RegisterForm.tsx`:
  - Name, email, password fields with matching validation
  - Password minimum length enforced client-side
  - On success: redirect to home page
- [ ] T017 [US6] Audit `frontend/src/services/authService.ts` — ensure `signIn()` and `register()` call the correct backend endpoints and set `ml_token` / `ml_user` cookies on success
- [ ] T018 [P] [US6] Add/verify i18n keys for auth namespace in all four language files:
  - `frontend/src/messages/en.json` → `auth.*` keys
  - `frontend/src/messages/zu.json` → `auth.*` keys
  - `frontend/src/messages/st.json` → `auth.*` keys
  - `frontend/src/messages/af.json` → `auth.*` keys

**Checkpoint**: Sign in and register work end-to-end; role-based redirect verified; all language strings present

---

## Phase 4: User Story 1 — Landing Page (Priority: P1) 🏠

**Goal**: Unauthenticated visitors see the complete landing page with hero, stats, feature cards, category grid, and trending questions.

**Independent Test**: Open `http://localhost:3000/en` while unauthenticated, scroll through all sections, verify all content renders, and switch to each of the 4 locales via the language selector.

### Implementation

- [ ] T019 [US1] Verify landing page `frontend/src/app/[locale]/page.tsx`:
  - Hero section: tagline "Know your rights. In your language." in Fraunces serif + sub-tagline + search bar + suggestion chips
  - Stats row: 4 cards (Questions answered, Acts indexed, Languages, Contracts analysed) with correct values and organic border radii
  - Feature cards: "Analyse a contract" and "Ask a question" — correct icons and descriptions
  - Category grid: 9 cards with correct icons, names, descriptions, and type badges (Legal / Financial / Contracts)
  - Trending questions: 5 items with numbered ranks and category tags
- [ ] T020 [US1] Verify `AskExperience` component in `frontend/src/components/chat/AskExperience.tsx`:
  - Input field with mic icon and "Ask now" / submit button
  - 3 suggestion chip links below the input
  - Clicking a chip pre-fills the input and focuses it
  - Submit navigates to the Ask page with the question as a query param
- [ ] T021 [US1] Verify `OrganicBackground` in `frontend/src/components/layout/OrganicBackground.tsx`:
  - Fixed-position decorative SVG/CSS blobs behind the hero section
  - `pointer-events: none` and low `z-index` so it does not block interaction
- [ ] T022 [P] [US1] Add/verify i18n keys for home namespace in all four language files:
  - Hero tagline, sub-tagline, stats labels, feature card texts, category names + descriptions, trending question text, all suggestion chips
  - `frontend/src/messages/en.json`, `zu.json`, `st.json`, `af.json` → `home.*` keys

**Checkpoint**: Landing page fully renders in all 4 locales; hero search bar navigates to Ask page; "Get started" button navigates to Auth page

---

## Phase 5: User Story 2 — Ask / Chat Page (Priority: P1) 💬

**Goal**: Users can submit legal questions in any supported language and receive AI-generated answers with citation sources, related questions, voice output, and voice input.

**Independent Test**: Navigate to `/en/ask`, submit "Can my landlord evict me?", verify an answer streams in, citations are expandable, "Listen in English" button triggers TTS, and a related question chip is clickable.

### Implementation

- [ ] T023 [US2] Verify `frontend/src/app/[locale]/ask/page.tsx` renders `QaChatPage` with correct locale prop
- [ ] T024 [US2] Verify `QaChatPage` in `frontend/src/components/chat/QaChatPage.tsx`:
  - Maintains `messages: ChatMessage[]` state for the conversation thread
  - Calls `qaService.ask()` on submit and appends streamed response to messages
  - Shows legal disclaimer banner below the chat thread
- [ ] T025 [US2] Verify `ChatThread` in `frontend/src/components/chat/ChatThread.tsx`:
  - Renders user messages (right-aligned, primary background) and assistant messages (left-aligned, card background)
  - Auto-scrolls to the latest message
- [ ] T026 [US2] Verify `ChatMessage` in `frontend/src/components/chat/ChatMessage.tsx`:
  - User message bubble: correct styling with organic border radius
  - Assistant message: body text + `CitationList` accordion (hidden when `citations.length === 0`) + "Listen in [language]" button + `VoiceOutput` + related question chips
- [ ] T027 [US2] Verify `CitationList` in `frontend/src/components/chat/CitationList.tsx`:
  - Toggle: "Sources (N sections cited)" with expand/collapse
  - Each citation: act name (bold) + section number + optional excerpt
  - Uses `aria-expanded` for accessibility
- [ ] T028 [US2] Verify `VoiceOutput` in `frontend/src/components/chat/VoiceOutput.tsx`:
  - Uses `window.speechSynthesis` with locale-appropriate `lang` tag (en-ZA, zu-ZA, st-ZA, af-ZA)
  - Button hidden when `speechSynthesis` is not supported
- [ ] T029 [US2] Verify `VoiceInput` in `frontend/src/components/chat/VoiceInput.tsx`:
  - Activates `SpeechRecognition` API on mic button click
  - Transcribed text populates the `ChatInput` field
  - Handles permission-denied gracefully (shows message, does not crash)
- [ ] T030 [US2] Verify `ChatInput` in `frontend/src/components/chat/ChatInput.tsx`:
  - Multi-line textarea growing to max 4 lines
  - Submit on Enter (not Shift+Enter)
  - Send button disabled when input is empty
  - Both mic and send buttons are keyboard-accessible (tab + Enter/Space)
- [ ] T031 [US2] Verify `useChat` hook in `frontend/src/hooks/useChat.ts` manages `conversationId` and the message array across follow-up questions
- [ ] T032 [US2] Verify `qaService.ts` in `frontend/src/services/qaService.ts`:
  - POST to `/api/qa/ask` with `{ question, conversationId?, locale }`
  - Handles streamed responses or JSON response shape `QaResponse` from data-model.md
  - Returns `{ answer, citations, relatedQuestions, conversationId, questionId }`
- [ ] T033 [P] [US2] Add/verify i18n keys for ask namespace in all four language files:
  - Input placeholder, disclaimer text (with Legal Aid SA number), "Listen in [language]" labels, related questions label, sources label, error messages
  - `frontend/src/messages/en.json`, `zu.json`, `st.json`, `af.json` → `ask.*` keys

**Checkpoint**: Full ask/chat flow works end-to-end including citations, TTS, STT, and related questions; no console errors on streaming

---

## Phase 6: User Story 3 — Contracts (Priority: P2) 📄

**Goal**: Authenticated users can upload a contract, see analysis status, and view a detailed analysis with risk score, red flags, caution items, and standard clauses.

**Independent Test**: Sign in, navigate to `/en/contracts`, verify auth guard redirects unauthenticated users; when signed in, upload a PDF and verify it appears in the list with "Analysing" status; navigate to a contract detail page and verify score, summary, red flags, and caution sections.

### Implementation

- [ ] T034 [US3] Implement Contracts list page `frontend/src/app/[locale]/contracts/page.tsx`:
  - Auth guard: redirect to auth if `user` is null after loading
  - Upload button triggering a file input (PDF only, `accept=".pdf"`)
  - List of uploaded contracts: each item shows name, type badge, upload date, status badge ("Analysing" / "Complete"), score (when complete), and a link to the detail page
  - Empty state when no contracts uploaded: icon + prompt to upload first contract
- [ ] T035 [US3] Verify `contractData.ts` in `frontend/src/components/contracts/contractData.ts`:
  - Static mock data for at least 2 contracts matching the `ContractAnalysis` shape from data-model.md
  - `getContractById(id)` helper returns `ContractAnalysis | undefined`
- [ ] T036 [US3] Verify Contract Detail page `frontend/src/app/[locale]/contracts/[id]/page.tsx`:
  - "← Back to contracts" link at the top using `createLocalizedPath`
  - Circular score badge (120×120px) with colour based on score range (0–39 red, 40–69 amber, 70–100 green)
  - Document metadata row: upload date, analysis time, pages, clauses, language
  - Plain-language summary card with body text + inline `ChatInput` for follow-up questions
  - Red Flags section with count badge: each flag — red left-border card with title, description, legislation chip
  - Caution section with count badge: each item — amber warning icon, title, description
  - Standard Clauses section: green check icon + "All standard clauses are in order" message (when clean)
  - `notFound()` call when `getContractById(id)` returns undefined
- [ ] T037 [P] [US3] Add/verify i18n keys for contracts namespace in all four language files:
  - `upload`, `uploadHint`, `statusAnalysing`, `statusComplete`, `statusFailed`, `empty`, `backToContracts`, `redFlags`, `caution`, `standardClauses`, `plainSummary`, `askAboutContract`
  - `frontend/src/messages/en.json`, `zu.json`, `st.json`, `af.json` → `contracts.*` keys

**Checkpoint**: Auth guard verified; contract list and detail pages render correctly with mock data in all 4 locales

---

## Phase 7: User Story 4 — My Rights (Priority: P2) ⚖️

**Goal**: Users explore legal rights by category, track learning progress, and expand individual rights cards to read full explanations with action buttons.

**Independent Test**: Navigate to `/en/rights`, verify category filter tabs work, progress bar renders, a card can be expanded to show body text + pull-quote + action buttons, and clicking "Ask a follow-up" navigates to the Ask page.

### Implementation

- [ ] T038 [US4] Verify My Rights page `frontend/src/app/[locale]/rights/page.tsx`:
  - Page title "Know your rights" in Fraunces serif + subtitle
  - Progress bar section: "Your knowledge score — you've explored X of 20 rights topics" + percentage + styled progress bar
  - Category filter tabs: All, Employment, Housing, Consumer, Debt & Credit, Tax, Privacy — active tab highlighted with primary colour
  - Grid of `RightCard` entries filtered by selected category
- [ ] T039 [US4] Verify `RightCard` render logic in the rights page:
  - Collapsed state: title (bold Fraunces) + legislation citation (muted) + summary line + "+" toggle button
  - Expanded state: full explanation paragraph + pull-quote block (left border, italic, muted) + "Ask a follow-up", "Listen in [language]", "Share" buttons + "–" collapse button
  - `aria-expanded` on the toggle button
  - "Ask a follow-up" → `router.push(createLocalizedPath(locale, appRoutes.ask) + '?q=' + encodeURIComponent(title))`
  - "Listen in [language]" → `VoiceOutput` component reads the body text
  - "Share" → `navigator.share()` if available, else `navigator.clipboard.writeText(window.location.href)`
- [ ] T040 [US4] Implement progress tracking in the rights page:
  - Track expanded card IDs in `useState` (or `localStorage` under key `ml_rights_progress`)
  - `explored` count increments the first time a card is expanded
  - Progress bar percentage = `(explored / 20) * 100`
- [ ] T041 [P] [US4] Add/verify i18n keys for rights namespace in all four language files:
  - `pageTitle`, `subtitle`, `knowledgeScore`, `explored`, `of`, `topics`, `allFilter`, filter labels, all card title/legislation/summary/body/quote keys, `askFollowUp`, `share`
  - `frontend/src/messages/en.json`, `zu.json`, `st.json`, `af.json` → `rights.*` keys

**Checkpoint**: Category filtering works; card expand/collapse with aria-expanded verified; progress bar increments on first expand; all 4 locales render without missing keys

---

## Phase 8: User Story 5 — Conversation History (Priority: P3) 📋

**Goal**: Authenticated users see a list of their past conversations and can navigate to any thread.

**Independent Test**: Sign in, navigate to `/en/history`, verify auth guard redirects when unauthenticated; with a user who has prior conversations, verify the list renders with timestamps and question previews; clicking a conversation navigates to the Ask page with that thread.

### Implementation

- [x] T042 [US5] Implement History page `frontend/src/app/[locale]/history/page.tsx`:
  - Auth guard: redirect to auth if unauthenticated
  - On mount: fetch conversation list from `GET /api/conversations` via `qaService` (or `historyService`)
  - Loading state: skeleton or spinner while fetching
  - Empty state (no conversations): icon + "You haven't asked any questions yet" + "Ask your first question" CTA button
  - Conversation list: each item shows first question (truncated at 120 chars), date formatted in locale, question count badge, and a chevron link to the Ask page with `?conversationId=<id>`
- [ ] T043 [US5] Add `getConversations()` method to `frontend/src/services/qaService.ts`:
  - GET `/api/conversations` with auth header from cookie
  - Returns `ConversationsListResponse` shape from data-model.md
  - On auth error (401): triggers `signOut()` and redirects to auth
- [ ] T044 [P] [US5] Add/verify i18n keys for history namespace in all four language files:
  - `title`, `empty`, `signInPrompt`, `askFirst`, `conversation`, `questionCount`, `viewThread`
  - `frontend/src/messages/en.json`, `zu.json`, `st.json`, `af.json` → `history.*` keys

**Checkpoint**: Auth guard confirmed; empty state and list state both render correctly; clicking a conversation opens the thread in the Ask page

---

## Phase 9: User Story 7 — Admin Dashboard (Priority: P3) 📊

**Goal**: Admin users see aggregate platform stats, a usage chart, and recent activity. Non-admin users are redirected to home.

**Independent Test**: Sign in as admin, navigate to `/en/admin/dashboard`, verify summary cards, chart, and recent activity list render. Sign out, sign in as regular user, attempt to navigate to dashboard, verify redirect to home.

### Implementation

- [ ] T045 [US7] Verify Admin Dashboard page `frontend/src/app/[locale]/admin/dashboard/page.tsx`:
  - Auth guard: redirect to auth if unauthenticated; redirect to home if `!user.isAdmin`
  - Loading spinner while `isLoading` is true
  - Summary cards row: Total Questions, Active Users, Contracts Analysed, Documents Indexed (using `SummaryCard` component)
  - `InsightChart` section with chart title and data
  - Recent activity section (last 10 items) — can use static mock for MVP if backend endpoint unavailable
- [ ] T046 [US7] Verify `SummaryCard` in `frontend/src/components/dashboard/SummaryCard.tsx`:
  - Accepts `{ icon, label, value, tone }` props per data-model.md contract
  - Card with `C.card` background, `16px` radius, `1px solid C.border` border
  - Large value in bold, small muted label beneath, icon top-left
- [ ] T047 [US7] Verify `InsightChart` in `frontend/src/components/dashboard/InsightChart.tsx`:
  - Renders a bar chart using `InsightDataPoint[]` props
  - Bar colours: `tone === "primary"` → `C.primary`; `"secondary"` → `C.secondary`; `"danger"` → `C.destructive`
  - Chart is accessible (bars have aria labels with value)
- [ ] T048 [US7] Verify `SectionCard` in `frontend/src/components/dashboard/SectionCard.tsx`:
  - Card wrapper with title slot and content slot
  - Used to group summary cards and chart sections on the dashboard
- [ ] T049 [P] [US7] Add/verify i18n keys for admin namespace in all four language files:
  - `dashboardTitle`, `totalQuestions`, `activeUsers`, `contractsAnalysed`, `documentsIndexed`, `insightTitle`, `recentActivity`, `noActivity`
  - `frontend/src/messages/en.json`, `zu.json`, `st.json`, `af.json` → `admin.*` keys

**Checkpoint**: Admin role guard confirmed; all dashboard sections render; non-admin redirect verified; all 4 locales render without missing keys

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Final quality checks across all pages before PR.

- [ ] T050 [P] Run i18n key audit: start dev server and open each of the 8 routes in all 4 locales — verify no `[MISSING]` keys appear in the UI or browser console
- [x] T051 [P] Run TypeScript build check: `cd frontend && npm run build` — zero type errors required
- [ ] T052 [P] Accessibility spot-check: tab through each page's interactive elements (nav links, language selector, form inputs, card expand toggles, chat input, voice buttons) — verify focus ring is visible and `aria-*` attributes are present
- [ ] T053 Verify all 8 routes are reachable at desktop width (1280px) with no horizontal scroll or layout overflow
- [ ] T054 Verify all 8 routes are usable at tablet width (768px) — content visible, nav accessible, no clipping
- [ ] T055 [P] Verify organic border radii are consistently applied across all stat cards, feature cards, category cards, and rights cards per research.md token table
- [ ] T056 Copy final `tasks.md` to `specs/feat-019-full-ui-pages/tasks.md` so the canonical spec directory has the full task list

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 — BLOCKS all user story phases
- **User Story 6 — Auth (Phase 3)**: Depends on Phase 2 — do Auth first since all protected pages depend on `useAuth`
- **User Story 1 — Landing (Phase 4)**: Depends on Phase 2 — public page, can start in parallel with Phase 3
- **User Story 2 — Ask (Phase 5)**: Depends on Phase 2 — can start in parallel with Phases 3 & 4
- **User Story 3 — Contracts (Phase 6)**: Depends on Phase 2 + Phase 3 (auth guard uses `useAuth`)
- **User Story 4 — My Rights (Phase 7)**: Depends on Phase 2 — public-facing, no auth dependency for core features
- **User Story 5 — History (Phase 8)**: Depends on Phase 2 + Phase 3 + Phase 5 (uses `qaService.getConversations`)
- **User Story 7 — Dashboard (Phase 9)**: Depends on Phase 2 + Phase 3 (admin auth guard)
- **Polish (Phase 10)**: Depends on all phases complete

### Within Each Phase

- Tasks marked [P] can run in parallel (different files, no intra-phase dependencies)
- i18n key tasks [P] can always run in parallel with implementation tasks for the same story
- Verify tasks (foundational phase) should run sequentially to catch issues early

### Parallel Opportunities Per Story

```text
Phase 3 (Auth):
  Parallel: T015 (SignInForm) + T016 (RegisterForm) + T018 (i18n keys)
  Sequential: T013 → T014 → T017

Phase 4 (Landing):
  Parallel: T020 (AskExperience) + T021 (OrganicBackground) + T022 (i18n keys)
  Sequential: T019

Phase 5 (Ask/Chat):
  Parallel group A: T025 (ChatThread) + T026 (ChatMessage) + T027 (CitationList)
  Parallel group B: T028 (VoiceOutput) + T029 (VoiceInput) + T033 (i18n keys)
  Sequential: T023 → T024 → T030 → T031 → T032

Phase 6 (Contracts):
  Parallel: T035 (contractData) + T037 (i18n keys)
  Sequential: T034 → T036

Phase 7 (Rights):
  Parallel: T039 (RightCard logic) + T040 (progress tracking) + T041 (i18n keys)
  Sequential: T038

Phase 8 (History):
  Parallel: T043 (qaService extension) + T044 (i18n keys)
  Sequential: T042

Phase 9 (Dashboard):
  Parallel: T046 (SummaryCard) + T047 (InsightChart) + T048 (SectionCard) + T049 (i18n keys)
  Sequential: T045

Phase 10 (Polish):
  All [P] tasks (T050, T051, T052, T055) can run in parallel
  Sequential: T053 → T054 → T056
```

---

## Implementation Strategy

### MVP First (P1 Stories Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: Auth (US6)
4. Complete Phase 4: Landing Page (US1)
5. Complete Phase 5: Ask/Chat (US2)
6. **STOP and VALIDATE**: All three P1 stories functional end-to-end
7. Deploy/demo if ready

### Incremental Delivery

- Foundation + Auth → users can authenticate ✅
- + Landing Page → public entry point ✅
- + Ask/Chat → core Q&A value proposition ✅ **(MVP)**
- + Contracts → contract analysis flow ✅
- + My Rights → rights explorer ✅
- + History → personalised history ✅
- + Admin Dashboard → platform monitoring ✅

### Parallel Team Strategy

With 3 developers after Phase 2 is complete:
- Developer A: Phase 3 (Auth) → Phase 6 (Contracts) → Phase 8 (History)
- Developer B: Phase 4 (Landing) → Phase 7 (My Rights)
- Developer C: Phase 5 (Ask/Chat) → Phase 9 (Dashboard)

---

## Notes

- All tasks are verify-or-implement tasks because the existing code base has foundational pages that need to be restored from git before enhancement
- `T001` (git checkout) is the single most critical task — nothing else can proceed until it's done
- The `[P]` i18n tasks in each phase can always be done in parallel by a separate team member
- Commit after each phase checkpoint for clean rollback points
- Phase 10 Polish tasks must all pass before raising a PR to `main`
