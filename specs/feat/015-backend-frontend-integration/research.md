# Research: Backend Frontend Integration

## Decision: Frontend API Client & State Management
**Rationale**: We need a robust way to communicate with the ABP-based .NET backend. Since we are using Next.js (TypeScript), we will use `fetch` or `axios` (decided: `fetch` for simplicity and standard compliance) wrapped in a service layer. State management for the chat will use React's `useState` and `useReducer` for conversation history, as global state like Redux/Zustand is not yet justified for a single-feature MVP.

**Alternatives Considered**:
- **Axios**: Provides better error handling and interceptors, but `fetch` is native and sufficient for current needs.
- **React Query (TanStack Query)**: Excellent for server state, but since this is a chat interface with sequential messages and streaming potential (future), a custom hook/service is preferred for more control over the message thread.

## Decision: Authentication Deferral
**Rationale**: Per user instruction, authentication is not yet implemented in the workspace. We will implement the API calls without `Authorization` headers and bypass/remove `[AbpAuthorize]` guards on the backend endpoints for this phase.
**Action**: Temporarily comment out or remove `[AbpAuthorize]` from `QaController.cs` if present, to allow integration testing.

## Decision: Multilingual Support
**Rationale**: The application supports English, isiZulu, Sesotho, and Afrikaans. The frontend will use `next-intl` or a similar lightweight i18n approach. The language preference will be passed to the backend in the `AskQuestionRequest` if the DTO supports it, or via `Accept-Language` headers.

## Decision: UI Components
**Rationale**: Using Ant Design (as per Constitution) for the chat interface components: `List` for messages, `Input.Search` or `TextArea` for input, and `Spin` for loading states.

## Research Tasks (Phase 0)
- [x] Verify `QaController` endpoint path and DTO structure.
- [x] Confirm Next.js project structure for services (likely `src/services/api/`).
- [x] Identify citation rendering requirements (Act name, Section number).
