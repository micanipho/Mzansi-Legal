# Implementation Plan: Backend Frontend Integration

**Branch**: `feat/015-backend-frontend-integration` | **Date**: 2026-03-30 | **Spec**: `specs/feat/015-backend-frontend-integration/spec.md`
**Input**: Feature specification from `/specs/feat/015-backend-frontend-integration/spec.md`

**Note**: This plan focuses on connecting the frontend Next.js application to the backend RAG Q&A service. **Authentication is currently out of scope per user instruction (deferred until implementation in the workspace).**

## Summary

Integrate the Next.js frontend with the ABP-based .NET backend Q&A service. This involves implementing a frontend API service to communicate with the `QaController` (`POST /api/app/qa/ask`), managing the chat interface state (message thread), and rendering AI-generated answers with structured legislative citations.

## Technical Context

**Language/Version**: TypeScript (Next.js), C# / .NET 9 (ABP Framework)
**Primary Dependencies**: Next.js, Ant Design, Fetch API (Frontend); ABP Framework, OpenAI (Backend)
**Storage**: N/A (Frontend state); PostgreSQL (Backend persistence)
**Testing**: Jest, React Testing Library
**Target Platform**: Web
**Project Type**: web-service/web-app integration
**Performance Goals**: Latency ≤ 8s for text responses (per Constitution)
**Constraints**: Support for English, isiZulu, Sesotho, Afrikaans; **Auth bypassed**
**Scale/Scope**: Integration of core Q&A chat interface

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify all gates from the constitution (`/.specify/memory/constitution.md`):

- [x] **Layer Gate**: New frontend services placed in `frontend/src/services/`.
- [x] **Naming Gate**: `qa.service.ts` follows naming conventions.
- [x] **Coding Standards Gate**: Planned approach complies with `docs/BP.md`.
- [x] **Skill Gate**: Identified `add-service` and `add-styling` skills.
- [x] **Multilingual Gate**: Q&A UI planned for all 4 languages; language context passed to backend.
- [x] **Citation Gate**: `RagAnswerResult` includes structured citations; frontend renders Act + Section.
- [x] **Accessibility Gate**: Keyboard navigation and screen reader semantics planned for chat interface.
- [x] **ETL/Ingestion Gate**: N/A (Consumes existing data).

## Project Structure

### Documentation (this feature)

```text
specs/feat/015-backend-frontend-integration/
├── plan.md              # This file
├── research.md          # Research findings and decisions
├── data-model.md        # Frontend interfaces and UI state
├── quickstart.md        # Implementation guide
├── contracts/           # API contract for QaController
│   └── qa-api.md
└── tasks.md             # To be generated
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── backend.Application/ (RagAppService)
│   └── backend.Web.Host/ (QaController)

frontend/
├── src/
│   ├── app/ (Q&A Page)
│   ├── components/ (Chat components)
│   ├── services/ (qa.service.ts)
│   └── i18n/ (Language support)
└── tests/
```

**Structure Decision**: Option 2: Web application (frontend + backend). The frontend will live in `frontend/` and communicate with the backend in `backend/`.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| **Auth Bypassed** | User instruction: "auth is not yet implemented" | Implementing auth first would delay core feature integration. |

## Phase 0: Outline & Research
Completed in `research.md`. Key decision: Use `fetch` API, `useState` for state, and bypass `[AbpAuthorize]`.

## Phase 1: Design & Contracts
Completed in `data-model.md`, `contracts/qa-api.md`, and `quickstart.md`.
- **Entities**: `Message`, `Citation`, `ConversationContext` defined for frontend.
- **Contracts**: `POST /api/app/qa/ask` defined with DTO mapping.
- **Quickstart**: Steps for manual verification.

## Phase 2: Tasks
The plan is ready for task generation. Use `/speckit.tasks` to generate `tasks.md`.
