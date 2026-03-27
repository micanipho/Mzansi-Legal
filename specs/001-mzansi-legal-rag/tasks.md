# Tasks: MzansiLegal Platform

**Input**: Design documents from /specs/001-mzansi-legal-rag/
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are OPTIONAL for this project. Implementation will focus on functional requirements first.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: [ID] [P?] [Story] Description

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

- [X] T002 [P] Scaffold Next.js Frontend project in frontend/ (#9)
- [X] T003 [P] Configure next-intl for multilingual support in frontend/src/messages/ (#27)
- [X] T004 [P] Configure Ant Design theme and design tokens in frontend/src/styles/ (#9)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

- [X] T005 Setup PostgreSQL database and migrations in backend/src/MzansiLegal.EntityFrameworkCore/ (#2)
- [X] T006 Implement Category entity and RefLists in backend/src/MzansiLegal.Domain/ (#2)
- [X] T007 Implement Knowledge Base entities (LegalDocument, DocumentChunk, ChunkEmbedding) in backend/src/MzansiLegal.Domain/ (#2)
- [X] T008 Implement PdfIngestionService for structured text extraction in backend/src/MzansiLegal.Application/ (#6)
- [X] T009 Implement EmbeddingService with OpenAI integration in backend/src/MzansiLegal.Application/ (#7)
- [X] T010 Implement cosine similarity logic for vector search in backend/src/MzansiLegal.Application/ (#7)
- [X] T011 Seed database with 13 legislation documents in backend/src/MzansiLegal.DbMigrator/ (#8)

**Checkpoint**: Foundation ready - knowledge base is indexed and searchable.

---

## Phase 3: User Story 1 - Multilingual Q&A (Priority: P1) 🎯 MVP

**Goal**: Users can ask questions in multiple languages and get cited answers.

**Independent Test**: Submit a question in isiZulu via the chat UI and verify the answer is in isiZulu with English citations.

- [ ] T012 [P] [US1] Create Conversation and Question entities in backend/src/MzansiLegal.Domain/ (#3)
- [ ] T013 [P] [US1] Create Answer and AnswerCitation entities in backend/src/MzansiLegal.Domain/ (#3)
- [ ] T014 [US1] Implement RagService core Q&A pipeline in backend/src/MzansiLegal.Application/ (#10)
- [ ] T015 [US1] Implement LanguageService for detection and translation in backend/src/MzansiLegal.Application/ (#11)
- [ ] T016 [US1] Implement Q&A API Endpoint in backend/src/MzansiLegal.HttpApi/ (#12)
- [ ] T017 [P] [US1] Create Home Dashboard UI in frontend/src/app/page.tsx (#14)
- [ ] T018 [P] [US1] Create Q&A Chat Interface in frontend/src/app/chat/page.tsx (#15)
- [ ] T019 [US1] Integrate Whisper API for voice input in frontend/src/components/chat/VoiceInput.tsx (#16)
- [ ] T020 [US1] Integrate TTS API for voice output in frontend/src/components/chat/VoiceOutput.tsx (#17)

**Checkpoint**: User Story 1 (MVP) is fully functional.

---

## Phase 4: User Story 2 - Contract Analysis (Priority: P2)

**Goal**: Users can upload contracts and get health scores and red flags.

**Independent Test**: Upload a sample lease PDF and verify the health score gauge and red flag list appear.

- [ ] T021 [P] [US2] Create ContractAnalysis and ContractFlag entities in backend/src/MzansiLegal.Domain/ (#4)
- [ ] T022 [US2] Implement ContractAnalysisService with OCR fallback in backend/src/MzansiLegal.Application/ (#18)
- [ ] T023 [US2] Implement Contract Analysis API Endpoints in backend/src/MzansiLegal.HttpApi/ (#18)
- [ ] T024 [P] [US2] Create Contract Upload UI in frontend/src/app/contracts/page.tsx (#18)
- [ ] T025 [P] [US2] Create Contract Analysis Results Page in frontend/src/app/contracts/[id]/page.tsx (#19)
- [ ] T026 [US2] Implement Follow-up chat scoped to contract in frontend/src/components/contracts/FollowUpChat.tsx (#18)

**Checkpoint**: Contract analysis flow is fully functional.

---

## Phase 5: User Story 3 - Rights Explorer & Discovery (Priority: P3)

**Goal**: Users can browse rights by category and see trending questions.

**Independent Test**: Filter rights by 'Employment' category and verify correct cards are shown.

- [ ] T027 [US3] Create My Rights Explorer Page in frontend/src/app/rights/page.tsx (#20)
- [ ] T028 [P] [US3] Implement Category Filter tabs in frontend/src/components/rights/CategoryFilters.tsx (#20)
- [ ] T029 [P] [US3] Implement Expandable Rights Cards in frontend/src/components/rights/RightsCard.tsx (#20)
- [ ] T030 [US3] Implement Trending Questions list on Home Page in frontend/src/components/home/TrendingQuestions.tsx (#14)

---

## Phase 6: User Story 4 - Admin Analytics & Moderation (Priority: P4)

**Goal**: Admins can monitor the platform and moderate answers.

**Independent Test**: Mark an answer as accurate in the review queue and verify it becomes eligible for FAQ.

- [ ] T031 [US4] Implement Admin Stats API in backend/src/MzansiLegal.Application/ (#21)
- [ ] T032 [US4] Create Admin Dashboard Page in frontend/src/app/admin/dashboard/page.tsx (#21)
- [ ] T033 [US4] Create Review Queue UI in frontend/src/app/admin/review-queue/page.tsx (#22)
- [ ] T034 [US4] Implement Moderation API Endpoints in backend/src/MzansiLegal.HttpApi/ (#22)
- [ ] T035 [US4] Implement FAQ Publishing workflow in frontend/src/app/admin/faqs/page.tsx (#24)

---

## Phase 7: User Story 5 - Authentication & History (Priority: P5)

**Goal**: Users can register, login, and view their history.

**Independent Test**: Login as a citizen and verify previous conversations are listed on the history page.

- [ ] T036 [US5] Implement User Registration and Login UI in frontend/src/app/auth/ (#23)
- [ ] T037 [US5] Implement User History Page in frontend/src/app/history/page.tsx (#34)
- [ ] T038 [US5] Implement User Settings and Preferences UI in frontend/src/app/settings/page.tsx (#36)
- [ ] T039 [US5] Enforce Role-Based Access Control on Admin routes in frontend/src/middleware.ts (#23)

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Accessibility, error handling, and deployment.

- [ ] T040 [P] Implement Dyslexia-Friendly Mode toggle and styles in frontend/src/components/settings/Accessibility.tsx (#26)
- [ ] T041 [P] Perform ARIA and Keyboard Navigation audit across all pages (#25)
- [ ] T042 [P] Implement global Error and Loading states in frontend/src/app/ (#29)
- [ ] T043 [P] Configure Azure DevOps CI/CD Pipelines in .github/workflows/ (#30)
- [ ] T044 Final run of quickstart.md validation (#31)

---

## Dependencies & Execution Order

### Phase Dependencies
- **Phase 1 & 2** are strictly required before any user story work begins.
- **Phase 3 (US1)** is the MVP and should be completed first.
- **Phase 4 (US2)** and **Phase 5 (US3)** can proceed in parallel once Phase 3 is stable.
- **Phase 6 (US4)** and **Phase 7 (US5)** can proceed in parallel.
- **Phase 8** is the final wrap-up phase.

### Implementation Strategy
- **MVP First**: Focus on completing US1 (Multilingual Q&A) to provide immediate value.
- **Incremental Delivery**: Deploy US1, then US2, then the remaining stories.
- **Parallelization**: Setup and Foundational tasks marked [P] can be split across developers.

---

## Notes
- All tasks follow the sequential T### ID format.
- exact file paths are provided for clarity.
- Issue references (#) map back to the GitHub backlog issues identified in spec.md.
