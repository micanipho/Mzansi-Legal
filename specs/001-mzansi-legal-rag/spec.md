# Technical Specification: MzansiLegal Platform

## Document Control
- Spec ID: 001-mzansi-legal-rag
- Version: 2.0 (fresh rewrite)
- Date: 2026-03-26
- Status: Ready for implementation
- Primary objective: Recreate the current React experience in Next.js + Ant Design while keeping domain, RAG, and accessibility rules intact.

## Source Alignment
This specification is the implementation source of truth and is aligned to:
- Product specification and domain intent.
- Engineering standards and architecture guardrails.
- GitHub delivery backlog (Issues #1-#31).
- Current React reference UI for visual and interaction parity.

## Architecture Principles
1. Backend: .NET 8 + ABP modular monolith + PostgreSQL.
2. Frontend: Next.js 14 App Router + Ant Design + next-intl.
3. AI: Retrieval-Augmented Generation (RAG), not model memory.
4. Multilingual strategy: Translate-in, process, translate-out.
5. Accessibility: WCAG 2.1 AA as an MVP requirement, not optional polish.
6. Domain modeling: DDD with PartOf composition, RefList enums, StoredFile attachments.
7. Security: Role-based access (Citizen, Admin) with private user data by default.

## Epic and Issue Traceability Matrix

| Epic | Issue Range | Spec Coverage |
|---|---|---|
| Project setup and data pipeline | #1-#9 | Backend scaffold, entities, ingestion, embeddings, seed, frontend scaffold |
| RAG and multilingual Q&A | #10-#13 | RagService, LanguageService, Question API, persistence for analytics/history |
| Frontend core UX, voice, contracts | #14-#19 | Home, Chat, Whisper input, TTS output, contract analysis pipeline and pages |
| Explorer, admin, auth | #20-#24 | Rights explorer, admin dashboard, review workflow, auth, FAQ lifecycle |
| Accessibility, polish, deploy | #25-#31 | ARIA/keyboard, dyslexia mode, i18n labels, disclaimers, responsive/error handling, CI/CD, demo readiness |

## Route Architecture (Next.js App Router)

### Public routes
- / : Home dashboard.
- /rights : My Rights explorer (public read mode).
- /ask : Q&A entry route.
- /chat : Alias redirect to /ask for compatibility with existing React references.
- /auth/login : Login page.
- /auth/register : Registration page.

### Authenticated citizen routes
- /contracts : Contract upload and analysis history.
- /contracts/[id] : Contract analysis result view.
- /contracts/[id]/chat : Follow-up chat scoped to contract context.
- /history : Private conversation and contract interaction history.
- /settings : Language, dyslexia mode, auto-play audio, and contrast preferences.

### Admin routes
- /admin/dashboard : Analytics overview.
- /admin/review-queue : Answer quality review and moderation actions.
- /admin/faqs : FAQ creation, generation, review, and publish flow.

## Frontend Rebuild Blueprint (React to Next.js + AntD)

### Global shell
- Sticky pill-shaped navbar with locale selector and auth state.
- Organic background layer with soft gradient/blob atmosphere.
- Design tokens:
  - Moss green primary: #5D7052.
  - Terracotta secondary: #C18C5D.
  - Rice paper background: #FDFCF8.
  - Accent and destructive states mapped to accessible contrast pairs.
- Typography:
  - Headings: Fraunces.
  - Body/UI: Nunito.
- Interaction style:
  - Rounded pill buttons.
  - Asymmetric card radii variants.
  - Soft organic shadows.

### Page component tree

#### Home (/)
- HomePage
  - HeroSection (headline, search, mic trigger, quick suggestions)
  - LiveStatsRow (questions, acts, languages, contracts)
  - PrimaryActions (Analyze Contract, Ask Question)
  - CategoryGrid (9 cards, domain tags)
  - TrendingQuestionsList (multilingual)

#### Ask (/ask)
- ChatPage
  - ConversationHeader (language and input method status)
  - MessageList
    - UserBubble
    - AssistantCard
      - AudioPlaybackBar
      - CitationAccordion
      - Domain disclaimer block
      - RelatedQuestions list
  - MessageComposer (text input, mic button, send button)
  - EmptyState and LoadingState

#### Contracts list/upload (/contracts)
- ContractsPage
  - UploadDropzone
  - UploadConstraintsHint
  - ExistingAnalysesList
  - EmptyState

#### Contract result (/contracts/[id])
- ContractAnalysisPage
  - BackLink
  - HealthScoreGauge (animated)
  - ContractMetaPills
  - PlainLanguageSummaryCard
  - FlagBreakdownStats
  - RedFlagSection
  - AmberSection
  - GreenSection
  - FollowUpComposer

#### Rights explorer (/rights)
- RightsExplorerPage
  - IntroHeader
  - KnowledgeScoreCard
  - CategoryFilterTabs
  - RightsCardGrid
    - CollapsedRightsCard
    - ExpandedRightsCard (actions: follow-up, listen, share)

#### Admin dashboard (/admin/dashboard)
- AdminDashboardPage
  - StatCardsWithTrend
  - QuestionsByCategoryChart
  - LanguageDistributionPanel
  - TopQuestionsList
  - ReviewQueuePreview

#### Review queue (/admin/review-queue)
- ReviewQueuePage
  - QueueFilters (status, language, category)
  - QueueTable
  - ReviewDrawer
    - QuestionAnswerContext
    - CitationComparisonPanel
    - AccuracyDecisionActions
    - AdminNotesEditor

#### FAQ admin (/admin/faqs)
- FaqManagementPage
  - FaqCreateForm
  - GeneratedAnswerPreview
  - PublishWorkflowControls
  - PublishedFaqList

#### Settings (/settings)
- UserSettingsPage
  - LanguagePreferenceSelect
  - DyslexiaModeToggle
  - AutoPlayToggle
  - HighContrastToggle

#### History (/history)
- HistoryPage
  - ConversationHistoryList
  - ContractHistoryList
  - ResumeAction
  - DeleteAction

## Backend Service Architecture

### Core services
1. PdfIngestionService
   - Extract text from PDF using PdfPig.
   - Chunk legislation by chapter and section heuristics.
   - Fallback chunking when section parsing is weak.
2. EmbeddingService
   - Generate text-embedding-ada-002 vectors.
   - Provide cosine similarity function.
3. RagService
   - Load chunk vectors in memory for MVP.
   - Retrieve top-K chunks using cosine similarity.
   - Build strict context-only prompt with citations.
4. LanguageService
   - Detect en, zu, st, af.
   - Translate non-English to English for retrieval.
   - Preserve response language requirement with English citations.
5. ContractAnalysisService
   - Extract contract text (PDF and OCR fallback).
   - Detect contract type.
   - Produce health score, summary, and structured flags.
6. VoiceService
   - Whisper transcription endpoint.
   - TTS generation endpoint and optional caching.

### Interaction model
- Question flow: API -> LanguageService -> RagService -> persistence -> optional VoiceService.
- Contract flow: API upload -> extraction -> classification -> legislation retrieval -> analysis -> persistence -> optional follow-up chat.
- Admin review flow: moderation endpoints update Answer accuracy fields and FAQ publication status.

## Domain and Persistence Model

### Aggregates and entities
1. Category
   - Name, localized labels, Icon, Domain, SortOrder.
2. Knowledge base aggregate
   - LegalDocument (root): metadata, full text, file references, processed state.
   - DocumentChunk (PartOf LegalDocument): chapter/section/content/token metadata.
   - ChunkEmbedding (PartOf DocumentChunk): float[1536] vector.
3. Conversation aggregate
   - Conversation (root, PartOf AppUser): language, input method, IsPublicFaq, optional FAQ category.
   - Question (PartOf Conversation): original and translated text, language, input method, optional audio.
   - Answer (PartOf Question): response text, language, optional audio, moderation fields.
   - AnswerCitation (PartOf Answer): section reference, excerpt, relevance score, chunk reference.
4. Contract aggregate
   - ContractAnalysis (root, PartOf AppUser): original file, extracted text, contract type, score, summary, language, timestamp.
   - ContractFlag (PartOf ContractAnalysis): severity, description, clause text, citation, sort order.
5. AppUser extension
   - PreferredLanguage, DyslexiaMode, AutoPlayAudio, Role.

### Required indexes and constraints
- Unique and indexed identifiers on all aggregate roots.
- FK integrity for all PartOf relationships.
- Check constraints:
  - HealthScore between 0 and 100.
  - Allowed enum values for language, input method, role, contract type, severity.
- Indexes:
  - Conversation by UserId and StartedAt.
  - Question by ConversationId and created timestamp.
  - Answer accuracy review fields for queue filtering.
  - ContractAnalysis by UserId and AnalysedAt.
  - Chunk metadata fields used in retrieval diagnostics.

## API Endpoint Catalog

### Auth and profile
- POST /api/app/auth/register
- POST /api/app/auth/login
- POST /api/app/auth/logout
- GET /api/app/user/me
- PUT /api/app/user/preferences

### Q&A and FAQs
- POST /api/app/question/ask
- GET /api/app/question/history
- GET /api/app/question/popular
- GET /api/app/question/faqs

### Voice
- POST /api/app/voice/transcribe
- POST /api/app/voice/speak

### Contracts
- POST /api/app/contract/analyse
- GET /api/app/contract/{id}
- GET /api/app/contract/my
- POST /api/app/contract/{id}/ask

### Admin analytics and moderation
- GET /api/app/admin/stats
- GET /api/app/admin/questions-by-category
- GET /api/app/admin/language-distribution
- GET /api/app/admin/top-questions
- GET /api/app/admin/review-queue
- PUT /api/app/admin/answer/{id}/review
- POST /api/app/admin/faq/create
- PUT /api/app/admin/faq/{conversationId}/publish

### Knowledge base operations
- POST /api/app/admin/document/upload
- POST /api/app/admin/document/reindex

## Authentication and Authorization Rules
1. Anonymous users:
   - May browse Home and Rights explorer.
   - May view public FAQ and trending content.
   - On ask or contract submission attempt, redirect to login.
2. Citizen users:
   - Full access to personal Q&A, contracts, history, settings.
   - No admin analytics or moderation access.
3. Admin users:
   - Full admin dashboard and moderation access.
   - FAQ creation and publication rights.
4. Data visibility:
   - Conversations and contracts are private by default.
   - Public FAQ content is explicitly published only.

## Functional Requirements

### Q&A and multilingual pipeline
- FR-001: Users must submit text questions in en, zu, st, af.
- FR-002: Users must submit voice questions transcribed through Whisper.
- FR-003: Answers must include at least one Act and section citation when context exists.
- FR-004: Response language must match selected or detected user language.
- FR-005: Citations must remain in English even when response language is non-English.
- FR-006: Q&A answer cards must include expandable citation details.
- FR-007: Related questions must be shown after each answer.
- FR-008: Legal or financial disclaimers must appear with each answer in active locale.
- FR-009: Empty retrieval must return a safe fallback and no fabricated legal claim.

### Contracts
- FR-010: Users must upload contract files (PDF, PNG, JPG).
- FR-011: Upload view must show validation for unsupported type/size before processing.
- FR-012: System must extract contract text with OCR fallback when extraction is weak.
- FR-013: System must classify contract type as Employment, Lease, Credit, or Service.
- FR-014: Contract analysis must return health score (0-100), summary, and flags.
- FR-015: Flags must use severity Red, Amber, Green with legislation citations.
- FR-016: Contract result page must provide a follow-up chat input.
- FR-017: Follow-up answers must consider both contract text and legislation context.

### Home, rights explorer, and discovery
- FR-018: Home must display live stats from API.
- FR-019: Home must display category cards with domain tags.
- FR-020: Home must display multilingual trending/public questions.
- FR-021: Rights explorer must support category filtering.
- FR-022: Rights explorer cards must support expanded/collapsed states.
- FR-023: Expanded rights cards must include follow-up, listen, and share actions.
- FR-024: Knowledge score must update when rights content is explored.

### Admin
- FR-025: Admin dashboard must show key platform metrics and trends.
- FR-026: Admin dashboard must show question category distribution and language split.
- FR-027: Review queue must list answers with pending or flagged moderation status.
- FR-028: Admin must mark an answer as accurate or inaccurate and save notes.
- FR-029: Flagged inaccurate answers must be excluded from FAQ publication.
- FR-030: Admin must create FAQ candidates through the same RAG pipeline.
- FR-031: Admin must publish approved FAQ items to public surfaces.

### Authentication, profile, and history
- FR-032: Users must register and login with email and password.
- FR-033: Role-based access must enforce Citizen versus Admin capabilities.
- FR-034: History page must list prior conversations and contract analyses.
- FR-035: Users must resume history entries from History page.
- FR-036: Users must manage language and accessibility preferences in Settings.
- FR-037: Preferences must persist across sessions.

### Accessibility, i18n, and responsive behavior
- FR-038: All interactive controls must be keyboard accessible.
- FR-039: App must include ARIA labels and semantic landmarks.
- FR-040: Chat responses must announce updates for screen readers.
- FR-041: Dyslexia mode must switch font and increase spacing/readability metrics.
- FR-042: Auto-play audio preference must trigger answer playback when enabled.
- FR-043: High contrast mode must respect OS-level preference.
- FR-044: Locale switching must update all static UI labels through next-intl.
- FR-045: Mobile layout must support primary flows with minimum 44x44 touch targets.
- FR-046: Friendly loading, empty, and error states must be provided on all data views.

### DevOps and release readiness
- FR-047: CI pipeline must build backend and frontend and run test stages.
- FR-048: CD pipeline must deploy backend and frontend to Azure targets.
- FR-049: Environment secrets must be externalized and not hardcoded.
- FR-050: Deployment docs must support repeatable staging setup and demo execution.

## Non-Functional Requirements
1. API latency targets:
   - Text Q&A median <= 8s.
   - Voice Q&A median <= 12s.
   - Contract analysis <= 30s for typical uploads.
2. Reliability:
   - Graceful fallback for AI provider failures.
   - Retries with bounded attempts for transient provider errors.
3. Security:
   - JWT authentication for protected endpoints.
   - Private user data isolation.
4. Observability:
   - Structured logs for AI calls, retrieval quality, and moderation actions.
5. Accessibility:
   - No critical WCAG 2.1 AA violations in primary flows.

## RAG Pipeline Flow (Detailed)
1. User submits text or transcribed voice input.
2. LanguageService detects language (en, zu, st, af).
3. If non-English, text is translated to English for retrieval.
4. EmbeddingService creates query vector.
5. RagService computes cosine similarity against indexed chunk embeddings.
6. Top 5 chunks above threshold are selected.
7. Prompt is built with strict context-only and citation rules.
8. GPT-4o generates answer in user language with English citations.
9. Answer, citations, and moderation metadata are stored.
10. Disclaimer is attached and returned to client.
11. Optional TTS is generated for playback.

## Contract Analysis Pipeline Flow (Detailed)
1. Authenticated user uploads contract file.
2. File validation checks type and size.
3. Text extraction runs via PdfPig.
4. If extraction is weak, OCR fallback runs via vision model.
5. Type detection classifies Employment, Lease, Credit, or Service.
6. Relevant legal chunks are retrieved for context.
7. Analysis prompt requests strict JSON output: score, summary, flags.
8. Response is parsed and validated.
9. ContractAnalysis and ContractFlag entities are saved.
10. Structured result is returned to frontend.
11. Follow-up questions reuse contract and legislation context.

## Error Handling Strategy
1. AI provider timeout/rate limit:
   - Return user-safe message and retry option.
   - Log provider failure category for monitoring.
2. Unsupported language:
   - Return explicit unsupported-language guidance and supported list.
3. Empty retrieval set:
   - Return no-context fallback, never fabricate law.
4. Upload failures:
   - Return friendly validation errors for type, size, unreadable content.
5. Translation failure:
   - Fail safely with fallback prompt to retry in English.

## Data Seeding Strategy
1. Seed Category records first.
2. Seed 13 documents:
   - Legal: Constitution, BCEA, CPA, LRA, POPIA, Rental Housing Act, Protection from Harassment Act.
   - Financial: NCA, FAIS, Tax Administration Act, Pension Funds Act, SARS guides, FSCA materials.
3. For each document:
   - Store metadata and original file reference.
   - Chunk by section.
   - Generate embeddings.
   - Mark document processed.
4. Seed baseline public FAQ items for explorer and home trends.
5. Seed admin account for moderation workflows.

## CI/CD and Deployment Architecture
1. CI stages:
   - Restore dependencies.
   - Build backend and frontend.
   - Run tests.
   - Publish artifacts.
2. CD stages:
   - Deploy backend to Azure App Service.
   - Deploy frontend to Azure Static Web Apps or App Service.
   - Run smoke checks.
3. Required environment variables:
   - OpenAI key and model config.
   - Database connection string.
   - JWT/auth settings.
   - Storage settings for file/audio artifacts.
4. Release governance:
   - Staging verification before production promotion.
   - Rollback procedure documented.

## Success Criteria
- SC-001: 90% of in-scope questions return cited answers.
- SC-002: Text answers are returned within 10 seconds under normal load.
- SC-003: Contract analysis returns score, summary, flags within 30 seconds.
- SC-004: All four supported languages return localized answers and UI labels.
- SC-005: Keyboard-only navigation passes all primary flow tests.
- SC-006: Dyslexia mode visibly applies within 1 second and persists after reload.
- SC-007: Admin moderation action reflects in review queue and FAQ visibility within 60 seconds.
- SC-008: All 13 seed documents are indexed and retrievable before launch.
- SC-009: Mobile experience works on small viewport without layout break in primary flows.
- SC-010: Empty-context and provider-error responses never expose raw technical errors.
- SC-011: Language preference changes apply globally within one page transition.
- SC-012: Review workflow cycle (open -> decide -> note -> save) completes in under 60 seconds.
- SC-013: Upload validation feedback appears in <= 2 seconds after file selection.
- SC-014: Route protection blocks unauthorized access to admin endpoints 100% of tests.

## Assumptions
1. Anonymous users can browse public pages but cannot submit private interactions.
2. Email/password authentication is sufficient for MVP.
3. In-memory vector search is acceptable for MVP scale and migrates to pgvector for production.
4. Translations are AI-assisted and reviewed for critical legal disclaimers.
5. Legislation update automation is outside v1 scope.

## Out of Scope (v1)
1. Native mobile apps.
2. Real-time streaming response transport.
3. Multi-region high availability.
4. Social login providers.
5. Automated legal source update crawler.

## Implementation Order (Issue-Driven)
1. Foundation and data pipeline (#1-#9).
2. RAG and multilingual API core (#10-#13).
3. Core frontend and contract flows (#14-#19).
4. Explorer, admin moderation, auth, FAQ tooling (#20-#24).
5. Accessibility, localization completion, resilient UX, deployment, demo prep (#25-#31).

## Completion Checklist
- [ ] Route architecture implemented in Next.js App Router.
- [ ] API and domain entities delivered with migrations.
- [ ] RAG and contract pipelines operational with citations.
- [ ] Accessibility and i18n validated in all primary paths.
- [ ] Admin moderation and FAQ publish flow live.
- [ ] CI/CD deployment successful and documented.

## Appendix A: Issue to Requirement Traceability

| Issue | Title | FR Mapping | SC Mapping |
|---|---|---|---|
| #1 | ABP Backend Project Scaffold | FR-047, FR-048, FR-049 | SC-014 |
| #2 | Domain Entities — Knowledge Base Aggregate | FR-003, FR-009, FR-050 | SC-001, SC-008 |
| #3 | Domain Entities — Conversation Aggregate | FR-001, FR-002, FR-006, FR-034 | SC-001, SC-010 |
| #4 | Domain Entities — Contract Analysis Aggregate | FR-014, FR-015, FR-017 | SC-003 |
| #5 | Domain Entities — AppUser Extension | FR-033, FR-036, FR-037, FR-041, FR-042 | SC-006, SC-011 |
| #6 | PDF Ingestion Service — Text Extraction and Smart Chunking | FR-012, FR-050 | SC-008 |
| #7 | Embedding Service — OpenAI Integration | FR-003, FR-009, FR-050 | SC-001, SC-008 |
| #8 | Seed Database with Legislation | FR-018, FR-019, FR-050 | SC-008 |
| #9 | Next.js Frontend Scaffold | FR-018, FR-020, FR-044, FR-045 | SC-004, SC-009 |
| #10 | RAG Service — Core Q&A Pipeline | FR-003, FR-005, FR-009 | SC-001, SC-002, SC-010 |
| #11 | Language Detection and Translation Layer | FR-001, FR-004, FR-005, FR-044 | SC-004, SC-011 |
| #12 | Q&A API Endpoint | FR-001, FR-006, FR-007, FR-034 | SC-001, SC-002 |
| #13 | Store Q&A Data for History and Analytics | FR-024, FR-025, FR-034, FR-035 | SC-007, SC-012 |
| #14 | Home Dashboard Page | FR-018, FR-019, FR-020 | SC-009 |
| #15 | Q&A Chat Interface | FR-006, FR-007, FR-008, FR-046 | SC-002, SC-010 |
| #16 | Voice Input — Whisper API Integration | FR-002, FR-040, FR-045 | SC-002, SC-009 |
| #17 | Voice Output — TTS API Integration | FR-042, FR-046 | SC-002 |
| #18 | Contract Upload and Analysis Pipeline | FR-010, FR-011, FR-012, FR-013, FR-014, FR-015, FR-017 | SC-003, SC-013 |
| #19 | Contract Analysis Results Page | FR-014, FR-015, FR-016, FR-046 | SC-003, SC-009 |
| #20 | My Rights Explorer Page | FR-021, FR-022, FR-023, FR-024 | SC-009 |
| #21 | Admin Analytics Dashboard | FR-025, FR-026, FR-027 | SC-007, SC-012 |
| #22 | Answer Quality Review System | FR-027, FR-028, FR-029 | SC-007, SC-012 |
| #23 | User Authentication | FR-032, FR-033, FR-034, FR-036 | SC-014 |
| #24 | FAQ Creation System (Admin) | FR-030, FR-031 | SC-007 |
| #25 | Accessibility — Screen Reader and Keyboard Navigation | FR-038, FR-039, FR-040 | SC-005 |
| #26 | Dyslexia-Friendly Mode | FR-041, FR-036, FR-037 | SC-006 |
| #27 | Multilingual UI Label Files | FR-044 | SC-004, SC-011 |
| #28 | Disclaimers Integration | FR-008, FR-046 | SC-010 |
| #29 | Responsive Design and Error Handling | FR-045, FR-046 | SC-009, SC-010 |
| #30 | Azure DevOps CI/CD Pipeline and Deployment | FR-047, FR-048, FR-049, FR-050 | SC-014 |
| #31 | Demo Preparation and Documentation | FR-050 | SC-009 |

### Traceability Notes
1. Multiple issues map to shared FR/SC items by design because several backlog tasks contribute to the same outcome.
2. Any future issue added to the backlog must include at least one FR and one SC linkage in this appendix before implementation starts.
