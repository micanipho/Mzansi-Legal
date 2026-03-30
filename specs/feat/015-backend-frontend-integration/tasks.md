# Implementation Tasks: Backend Frontend Integration

**Branch**: `feat/015-backend-frontend-integration` | **Date**: 2026-03-30
**Input**: Feature specification from `specs/feat/015-backend-frontend-integration/spec.md`

**Constraint**: Authentication is deferred. **DO NOT add [AbpAuthorize] guards** or session-based redirects in this phase.

## Phase 1: Setup (Backend Bypass)
*Goal: Ensure the backend is reachable without authentication for integration testing.*

- [x] T001 [P] Temporarily bypass authentication in `backend/src/backend.Web.Host/Controllers/QaController.cs` (comment out `[AbpAuthorize]` on the controller and `Ask` method)
- [ ] T002 [P] Verify backend is running and `POST /api/app/qa/ask` returns `400 Bad Request` (not `401 Unauthorized`) when called with an empty body via Postman/cURL

## Phase 2: Foundational (Frontend Infrastructure)
*Goal: Set up the API client and base state management.*

- [x] T003 Create frontend API service for Q&A in `frontend/src/services/qa.service.ts` using the standard `fetch` API
- [x] T004 [P] Define TypeScript interfaces for `AskQuestionRequest` and `RagAnswerResult` in `frontend/src/services/qa.service.ts` matching the backend DTOs
- [x] T005 [P] Create a custom hook `useChat` in `frontend/src/hooks/useChat.ts` to manage the message thread state (loading, error, messages array)

## Phase 3: [US1] Chat Interface (P1)
*Goal: Enable basic Q&A submission and answer display.*

- [x] T006 [P] [US1] Create `ChatInput` component in `frontend/src/components/chat/ChatInput.tsx` with validation to prevent empty submissions
- [x] T007 [P] [US1] Create `ChatMessage` component in `frontend/src/components/chat/ChatMessage.tsx` using Ant Design's `List.Item` to display user/bot text
- [x] T008 [US1] Integrate `ChatInput` and `ChatMessage` into the main Q&A page in `frontend/src/app/qa/page.tsx`
- [x] T009 [US1] Wire the `useChat` hook to the Q&A page to handle question submission and update the UI with the bot's response
- [x] T010 [US1] Implement a visible loading indicator (e.g., Ant Design `Spin`) in the chat thread while a request is in-flight in `frontend/src/app/qa/page.tsx`

## Phase 4: [US2] Conversation History (P2)
*Goal: Maintain a thread of messages within the current session.*

- [x] T011 [US2] Update `useChat` hook in `frontend/src/hooks/useChat.ts` to append new messages to the existing thread instead of replacing them
- [x] T012 [US2] Implement automatic scrolling to the bottom of the chat container when a new message arrives in `frontend/src/components/chat/ChatThread.tsx`
- [ ] T013 [US2] Verify that multiple exchanges (questions/answers) persist on screen during the session

## Phase 5: [US3] Citation Viewing (P3)
*Goal: Display structured citations alongside answers.*

- [x] T014 [P] [US3] Create `CitationList` component in `frontend/src/components/chat/CitationList.tsx` to render the Act name and section number from the backend
- [x] T015 [US3] Integrate `CitationList` into the `ChatMessage` component in `frontend/src/components/chat/ChatMessage.tsx` (only for bot messages)
- [x] T016 [US3] Add a "view source excerpt" toggle or tooltip to show the `excerpt` field for each citation in `frontend/src/components/chat/CitationList.tsx`

## Phase 6: Polish & Multilingual
*Goal: Final UI refinements and language context.*

- [x] T017 Pass the current application language (from `i18n` context) to the backend in the `AskQuestionRequest` or as an `Accept-Language` header in `frontend/src/services/qa.service.ts`
- [x] T018 Implement user-friendly error messages for network failures or backend errors in the `ChatThread` component
- [x] T019 Final styling review using Ant Design primitives to match the "Mzansi Legal" aesthetic in `frontend/src/styles/chat.css`

## Implementation Strategy
- **MVP (US1)**: Connect the frontend to the backend and get a single answer displayed.
- **Incremental**: Add history (US2) then citations (US3).
- **No-Auth**: All tests will be performed without a session token for now.

## Dependencies
- US1 (T006-T010) → Foundational (T003-T005)
- US2 (T011-T013) → US1
- US3 (T014-T016) → US1
- Phase 6 (T017-T019) → US1, US2, US3
