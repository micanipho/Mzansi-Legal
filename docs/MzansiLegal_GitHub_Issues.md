
# MzansiLegal — GitHub Issues Backlog
## Organised by Epic → Individual Feature Issues
## Labels: epic, day-1, day-2, day-3, day-4, day-5, backend, frontend, ai, accessibility

---

═══════════════════════════════════════════════════════════
EPIC 1: PROJECT SETUP & DATA PIPELINE (Day 1)
═══════════════════════════════════════════════════════════

---

## Issue #1: ABP Backend Project Scaffold
**Labels:** `day-1` `backend` `setup`

### 1. Is your feature request related to a problem? Please describe.
There is no backend project yet. We need a .NET 8 + ABP Framework scaffold with PostgreSQL as the database provider to serve as the foundation for all API endpoints.

### 2. Solution/feature Description:
- Scaffold a new ABP project using `abp new MzansiLegal -t app --ui none --mobile none --db-provider ef -dbms PostgreSQL`
- Configure the connection string for PostgreSQL
- Verify the project builds and runs successfully
- Ensure Swagger UI is accessible for API testing

### 3. Alternatives:
- Could use a minimal .NET Web API without ABP, but ABP provides built-in auth, module system, and multi-tenancy which saves significant time.

### 4. Additional context, and milestones
- **Milestone:** Day 1 — Setup & Data Pipeline
- **Acceptance:** Project runs, Swagger loads, database is created
- **Blocked by:** Nothing — this is the first task

---

## Issue #2: Domain Entities — Knowledge Base Aggregate
**Labels:** `day-1` `backend` `domain`

### 1. Is your feature request related to a problem? Please describe.
We need the core domain entities to store legislation documents, their section-level chunks, and embedding vectors. Without these entities, the RAG pipeline has nowhere to store or retrieve data.

### 2. Solution/feature Description:
Create the following entities following ABP/DDD conventions with PartOf composition:
- **Category** — Name, Icon, Domain (RefList: Legal|Financial), SortOrder
- **LegalDocument** — Title, ShortName, ActNumber, Year, FullText, OriginalPdf (StoredFile), FileName, CategoryId (FK → Category), IsProcessed, TotalChunks. PartOf: Category
- **DocumentChunk** — DocumentId (FK), ChapterTitle, SectionNumber, SectionTitle, Content, TokenCount, SortOrder. PartOf: LegalDocument
- **ChunkEmbedding** — ChunkId (FK), Vector (float[1536]). PartOf: DocumentChunk

Create RefList enum for Domain (Legal, Financial).
Run EF migration and verify tables are created.

### 3. Alternatives:
- Could store embeddings as JSON strings instead of float arrays. Simpler but slower for similarity search.
- Could skip ChunkEmbedding as a separate entity and put the vector on DocumentChunk. Separate entity is cleaner because embeddings are large and not always needed when displaying chunks.

### 4. Additional context, and milestones
- **Milestone:** Day 1 — Setup & Data Pipeline
- **Acceptance:** All tables created in PostgreSQL, can insert and query test data
- **Blocked by:** Issue #1

---

## Issue #3: Domain Entities — Conversation Aggregate
**Labels:** `day-1` `backend` `domain`

### 1. Is your feature request related to a problem? Please describe.
We need entities to store user conversations, questions, AI-generated answers, and the citations that link answers back to legislation. This is the core Q&A data model.

### 2. Solution/feature Description:
Create the following entities:
- **Conversation** — UserId (FK → AppUser, mandatory), Language (RefList), InputMethod (RefList: Text|Voice), StartedAt, IsPublicFaq (bool), FaqCategory (nullable FK → Category). PartOf: AppUser
- **Question** — ConversationId (FK), OriginalText, TranslatedText, Language (RefList), InputMethod (RefList), AudioFile (StoredFile). PartOf: Conversation
- **Answer** — QuestionId (FK), Text, Language (RefList), AudioFile (StoredFile), IsAccurate (bool?), AdminNotes (string?). PartOf: Question
- **AnswerCitation** — AnswerId (FK), ChunkId (FK → DocumentChunk, cross-aggregate reference), SectionNumber, Excerpt, RelevanceScore (decimal). PartOf: Answer

Create RefList enums for Language (en, zu, st, af) and InputMethod (Text, Voice).
Run EF migration.

### 3. Alternatives:
- Could make Conversation independent (not PartOf AppUser) to allow anonymous Q&A. Decided against this because all conversations require auth for security and history tracking. Anonymous users see FAQ conversations instead.

### 4. Additional context, and milestones
- **Milestone:** Day 1 — Setup & Data Pipeline
- **Acceptance:** All tables created, AnswerCitation.ChunkId correctly references DocumentChunk across aggregates
- **Blocked by:** Issue #1, #2

---

## Issue #4: Domain Entities — Contract Analysis Aggregate
**Labels:** `day-1` `backend` `domain`

### 1. Is your feature request related to a problem? Please describe.
We need entities to store uploaded contracts, their analysis results (health score, summary), and the individual red flag/caution/standard findings.

### 2. Solution/feature Description:
Create the following entities:
- **ContractAnalysis** — UserId (FK → AppUser, mandatory), OriginalFile (StoredFile), ExtractedText, ContractType (RefList: Employment|Lease|Credit|Service), HealthScore (int 0-100), Summary, Language (RefList), AnalysedAt. PartOf: AppUser
- **ContractFlag** — ContractAnalysisId (FK), Severity (RefList: Red|Amber|Green), Title, Description, ClauseText, LegislationCitation, SortOrder. PartOf: ContractAnalysis

Create RefList enums for ContractType and FlagSeverity.
Run EF migration.

### 3. Alternatives:
- Could store flags as a JSON array on ContractAnalysis instead of a separate entity. Separate entity is better for querying (e.g. "show all red flags across all contracts") and admin review.

### 4. Additional context, and milestones
- **Milestone:** Day 1 — Setup & Data Pipeline
- **Acceptance:** Tables created, ContractAnalysis correctly linked to AppUser
- **Blocked by:** Issue #1

---

## Issue #5: Domain Entities — AppUser Extension
**Labels:** `day-1` `backend` `domain`

### 1. Is your feature request related to a problem? Please describe.
ABP's built-in IdentityUser doesn't have fields for language preference, accessibility settings, or user role (Citizen vs Admin). We need to extend it.

### 2. Solution/feature Description:
Extend ABP's IdentityUser (AppUser) with:
- PreferredLanguage: RefList (en, zu, st, af)
- DyslexiaMode: bool (default false)
- AutoPlayAudio: bool (default false)
- Role: RefList (Citizen, Admin)

Run EF migration.

### 3. Alternatives:
- Could use ABP's built-in role system instead of a RefList for Role. ABP roles are more powerful (permissions-based) but a RefList is simpler for MVP where we only have two roles.

### 4. Additional context, and milestones
- **Milestone:** Day 1 — Setup & Data Pipeline
- **Acceptance:** AppUser table has the new columns, can create users with accessibility preferences
- **Blocked by:** Issue #1

---

## Issue #6: PDF Ingestion Service — Text Extraction & Smart Chunking
**Labels:** `day-1` `backend` `ai`

### 1. Is your feature request related to a problem? Please describe.
Legislation PDFs need to be processed into section-level chunks for the RAG pipeline. Raw PDFs are useless for search — they need to be split intelligently by legal section, preserving metadata (Act name, chapter, section number).

### 2. Solution/feature Description:
Build a PdfIngestionService that:
1. Accepts a PDF file stream
2. Extracts text using PdfPig (UglyToad.PdfPig NuGet package)
3. Chunks the text by section using regex patterns for SA legislation format:
   - Detect Chapter boundaries (`Chapter X — Title`)
   - Detect Section boundaries (`Section N. Title` or `N. Title`)
   - Each chunk = one complete legal section with chapter context
4. Falls back to fixed-size chunking (500 tokens with 50-token overlap) if section detection finds < 3 sections
5. Splits large sections (>800 tokens) by subsection markers ((1), (2), (3))
6. Returns a list of DocumentChunk objects ready to be saved
7. Includes token count estimation per chunk

Install: `dotnet add package UglyToad.PdfPig`

### 3. Alternatives:
- Could use iTextSharp instead of PdfPig. PdfPig is better for text extraction from government PDFs with inconsistent formatting.
- Could chunk by fixed size only (simpler). Smart section-level chunking produces much better retrieval quality because each chunk is a complete legal thought.

### 4. Additional context, and milestones
- **Milestone:** Day 1 — Setup & Data Pipeline
- **Acceptance:** Can process the Constitution PDF and produce ~200+ meaningful chunks with correct section numbers
- **Blocked by:** Issue #2
- **Test with:** Constitution PDF from justice.gov.za/constitution/SAConstitution-web-eng.pdf

---

## Issue #7: Embedding Service — OpenAI Integration
**Labels:** `day-1` `backend` `ai`

### 1. Is your feature request related to a problem? Please describe.
Each legislation chunk needs to be converted into a 1,536-dimensional vector (embedding) for semantic search. This requires calling the OpenAI embeddings API.

### 2. Solution/feature Description:
Build an EmbeddingService that:
1. Accepts a text string
2. Calls OpenAI's `text-embedding-ada-002` model via REST API
3. Returns a float[1536] vector
4. Handles token limits (truncate text > 30,000 chars)
5. Includes a static `CosineSimilarity(float[] a, float[] b)` method for comparing vectors

Configure via `appsettings.json`:
```json
{
  "OpenAI": {
    "ApiKey": "sk-...",
    "EmbeddingModel": "text-embedding-ada-002"
  }
}
```

### 3. Alternatives:
- Could use Azure OpenAI instead of direct OpenAI API. Same models, different endpoint. Easy to swap later.
- Could use a local embedding model (e.g. sentence-transformers) to avoid API costs. Quality is significantly lower for multilingual content.

### 4. Additional context, and milestones
- **Milestone:** Day 1 — Setup & Data Pipeline
- **Acceptance:** Can generate an embedding for a test string, cosine similarity returns ~1.0 for identical texts and ~0.3 for unrelated texts
- **Blocked by:** Issue #1
- **Cost:** ~$0.0001 per chunk. 1,000 chunks ≈ $0.10

---

## Issue #8: Seed Database with Legislation
**Labels:** `day-1` `backend` `data`

### 1. Is your feature request related to a problem? Please describe.
The platform needs pre-loaded legislation to be useful. Without seed data, the RAG pipeline has nothing to search.

### 2. Solution/feature Description:
Create a seed method (in DbMigrator or a standalone console app) that:
1. Downloads or reads from local copies of 13 key documents:
   - Legal: Constitution, BCEA, CPA, LRA, POPIA, Rental Housing Act, Protection from Harassment Act
   - Financial: NCA, FAIS, Tax Administration Act, Pension Funds Act, SARS tax guide, FSCA materials
2. Creates Category records (Employment & Labour, Housing & Eviction, Consumer Rights, Debt & Credit, Tax, Privacy & Data, Safety & Harassment, Insurance & Retirement, Contract Analysis)
3. For each document: creates LegalDocument record, runs PdfIngestionService to chunk, runs EmbeddingService to embed each chunk, saves all DocumentChunk and ChunkEmbedding records
4. Marks each document as IsProcessed = true

### 3. Alternatives:
- Could provide an admin UI for uploading documents one by one. Seed method is faster for initial data load. Admin upload will be built on Day 4 for adding new documents later.

### 4. Additional context, and milestones
- **Milestone:** Day 1 — Setup & Data Pipeline
- **Acceptance:** 13 documents processed, ~500-1,000 chunks with embeddings in database, all categories created
- **Blocked by:** Issue #2, #6, #7
- **Sources:** justice.gov.za, sars.gov.za, fsca.co.za (all free/public)

---

## Issue #9: Next.js Frontend Scaffold
**Labels:** `day-1` `frontend` `setup`

### 1. Is your feature request related to a problem? Please describe.
There is no frontend project yet. We need a Next.js application with Ant Design components, the organic design system, routing for all 5 pages, and a language selector shell.

### 2. Solution/feature Description:
- Scaffold: `npx create-next-app@latest frontend --typescript --app`
- Install dependencies: `npm install antd @ant-design/icons @ant-design/charts next-intl`
- Set up the organic design system:
  - Colors as CSS variables (moss green, terracotta, rice paper, etc.)
  - Import Fraunces and Nunito from Google Fonts
  - Paper grain texture overlay component
- Create app layout with sticky floating pill navbar
- Set up routing: `/` (home), `/ask` (chat), `/contracts` and `/contracts/[id]` (contract analysis), `/rights` (explorer), `/admin/dashboard`
- Set up next-intl with 4 locale JSON files (en.json, zu.json, st.json, af.json) — start with English, translate later
- Create API service file with base URL configuration
- Language selector in navbar (functional, switches locale)

### 3. Alternatives:
- Could use Tailwind CSS instead of inline styles with Ant Design. Ant Design provides more pre-built components (tables, charts, forms) which saves time on Day 3-4.

### 4. Additional context, and milestones
- **Milestone:** Day 1 — Setup & Data Pipeline
- **Acceptance:** App runs, all 5 routes accessible, navbar with language selector works, design system visible
- **Blocked by:** Nothing — can be done in parallel with backend


═══════════════════════════════════════════════════════════
EPIC 2: RAG PIPELINE & MULTILINGUAL Q&A (Day 2)
═══════════════════════════════════════════════════════════

---

## Issue #10: RAG Service — Core Q&A Pipeline
**Labels:** `day-2` `backend` `ai`

### 1. Is your feature request related to a problem? Please describe.
The platform needs to answer user questions by retrieving relevant legislation chunks and generating AI responses with citations. This is the core intelligence of the application.

### 2. Solution/feature Description:
Build a RagService that:
1. Loads all chunk embeddings into memory on startup (fine for MVP with ~1,000 chunks)
2. Accepts a user question (in English)
3. Generates an embedding for the question
4. Performs cosine similarity search against all chunks, returns top-5 above threshold (0.7)
5. Builds an LLM prompt with:
   - System prompt establishing the AI as a SA legal/financial assistant
   - Retrieved chunks as context, labelled with Act name and section number
   - Instructions to ONLY answer from context, ALWAYS cite section numbers, say "I don't have enough information" if context is insufficient
6. Calls OpenAI GPT-4o chat completions API (temperature 0.2 for factual accuracy)
7. Returns the answer text, a list of citation objects, and the chunk IDs used

### 3. Alternatives:
- Could use pgvector for vector search instead of in-memory. Better for production scale but overkill for MVP with ~1,000 chunks.
- Could use a lower temperature (0.0) for maximum factual adherence. 0.2 gives slightly more natural language while staying accurate.

### 4. Additional context, and milestones
- **Milestone:** Day 2 — RAG + Multilingual Q&A
- **Acceptance:** Can ask "Can my landlord evict me?" and get an answer citing Section 26(3) of the Constitution
- **Blocked by:** Issue #7, #8

---

## Issue #11: Language Detection & Translation Layer
**Labels:** `day-2` `backend` `ai`

### 1. Is your feature request related to a problem? Please describe.
Users will ask questions in isiZulu, Sesotho, or Afrikaans, but the RAG pipeline works in English (embeddings are in English). We need to detect the input language, translate to English for search, and instruct the LLM to respond in the user's language.

### 2. Solution/feature Description:
Build a LanguageService that:
1. Detects input language using a lightweight GPT-4o call: "What language is this? Respond with only the ISO code: en, zu, st, or af"
2. If not English, translates the question to English using GPT-4o: "Translate to English: {question}"
3. Stores both OriginalText and TranslatedText on the Question entity
4. Adds language directive to the RAG prompt: "Respond in isiZulu. Keep all Act names and section numbers in English."

### 3. Alternatives:
- Could use a dedicated language detection library (e.g. langdetect). GPT-4o is more accurate for SA languages and we're already calling it anyway.
- Could use the browser's Web Speech API for language detection from voice. Whisper already detects language during transcription — use that instead.

### 4. Additional context, and milestones
- **Milestone:** Day 2 — RAG + Multilingual Q&A
- **Acceptance:** Can ask "Ingabe umnikazi wendlu angangixosha?" in isiZulu and get a correct answer in isiZulu with English citations
- **Blocked by:** Issue #10

---

## Issue #12: Q&A API Endpoint
**Labels:** `day-2` `backend` `api`

### 1. Is your feature request related to a problem? Please describe.
The frontend needs an API endpoint to submit questions and receive AI-generated answers with citations. This connects the RAG pipeline to the user interface.

### 2. Solution/feature Description:
Create QuestionAppService with:
- `POST /api/app/question/ask` — accepts { question: string, language?: string }
  - Runs language detection → translation → RAG pipeline
  - Saves Conversation, Question, Answer, and AnswerCitation records
  - Returns { questionId, answer, citations[], disclaimer, detectedLanguage }
- `GET /api/app/question/history` — returns current user's question history (requires auth)
- `GET /api/app/question/popular` — returns top 10 most asked questions (public)
- `GET /api/app/question/faqs?categoryId={id}` — returns public FAQ conversations for a category

### 3. Alternatives:
- Could use SignalR/WebSockets for streaming responses. Simpler REST call is fine for MVP — streaming can be added later for better UX.

### 4. Additional context, and milestones
- **Milestone:** Day 2 — RAG + Multilingual Q&A
- **Acceptance:** All 4 endpoints working via Swagger, answers include accurate citations
- **Blocked by:** Issue #3, #10, #11

---

## Issue #13: Store Q&A Data for History and Analytics
**Labels:** `day-2` `backend` `data`

### 1. Is your feature request related to a problem? Please describe.
Every question and answer needs to be persisted for user history, admin analytics, and the FAQ system. Without storage, we lose all interaction data.

### 2. Solution/feature Description:
In the QuestionAppService.AskAsync method:
1. Create a Conversation record (or reuse existing if conversationId provided)
2. Create a Question record with OriginalText, TranslatedText, Language, InputMethod
3. After RAG generates an answer, create an Answer record with the response text and language
4. Create AnswerCitation records for each citation, linking to the DocumentChunk via ChunkId
5. All records linked via PartOf relationships (Conversation → Question → Answer → AnswerCitation)

### 3. Alternatives:
- Could store analytics separately in a denormalized analytics table for faster queries. Premature optimization for MVP — query the source tables directly.

### 4. Additional context, and milestones
- **Milestone:** Day 2 — RAG + Multilingual Q&A
- **Acceptance:** After asking a question, all records exist in database with correct relationships
- **Blocked by:** Issue #3, #12


═══════════════════════════════════════════════════════════
EPIC 3: FRONTEND CORE UX + VOICE + CONTRACTS (Day 3)
═══════════════════════════════════════════════════════════

---

## Issue #14: Home Dashboard Page
**Labels:** `day-3` `frontend` `ui`

### 1. Is your feature request related to a problem? Please describe.
The app needs a home page that feels like a real product — not a blank chatbot. Users should immediately see stats, categories, and trending questions to understand what the platform offers.

### 2. Solution/feature Description:
Build the home dashboard with:
- Hero section: heading "Know your rights. In your language.", search bar with mic button, suggestion links
- Stats row: 4 cards showing questions answered, Acts indexed, languages, contracts analysed (fetch from API)
- Two CTA cards: "Analyse a contract" (prominent) and "Ask a question"
- Category grid: 9 cards with domain tags (Legal/Financial/Contracts), fetched from Category API
- Trending questions: top 5 from popular endpoint, showing multilingual questions
- All using the organic design system (Fraunces headings, pill shapes, blob backgrounds, asymmetric card radii)

### 3. Alternatives:
- Could use a simple landing page with just a search bar. Dashboard with live data makes the app feel established and feature-rich.

### 4. Additional context, and milestones
- **Milestone:** Day 3 — Frontend Core UX
- **Acceptance:** Page loads with real data from API, all sections visible, responsive
- **Blocked by:** Issue #9, #12

---

## Issue #15: Q&A Chat Interface
**Labels:** `day-3` `frontend` `ui`

### 1. Is your feature request related to a problem? Please describe.
Users need a chat-style interface to ask questions and receive answers. The chat must support multilingual display, expandable citations, and the legal disclaimer.

### 2. Solution/feature Description:
Build the chat interface at `/ask`:
- Message list with user bubbles (right, moss green) and AI response cards (left)
- AI response cards contain:
  - Voice playback bar (speaker button, waveform, "Listen in {language}")
  - Answer text
  - Expandable citations section with Act name tags and section numbers
  - Disclaimer in user's language
  - Related questions suggestions (3-5)
- Input bar at bottom: pill input, mic button, send button
- Welcome state (no messages): heading, subtitle, starter question buttons
- Loading state: "Searching legislation and generating answer..." with spinner
- Auto-scroll to latest message

### 3. Alternatives:
- Could use a simple form instead of chat interface. Chat feels more natural and supports multi-turn conversation.

### 4. Additional context, and milestones
- **Milestone:** Day 3 — Frontend Core UX
- **Acceptance:** Can type a question, see loading state, receive answer with citations and disclaimer
- **Blocked by:** Issue #9, #12

---

## Issue #16: Voice Input — Whisper API Integration
**Labels:** `day-3` `frontend` `backend` `ai` `accessibility`

### 1. Is your feature request related to a problem? Please describe.
Many South Africans are more comfortable speaking than typing. Blind users cannot type. We need voice input that works in all 4 supported languages.

### 2. Solution/feature Description:
**Frontend:**
- Mic button in chat input bar and on home search bar
- Press to start recording, press again to stop (or auto-stop after silence)
- Use browser MediaRecorder API to capture audio as webm/mp3
- Send audio blob to backend endpoint
- Show transcribed text in input before submitting (user can edit)

**Backend:**
- `POST /api/app/voice/transcribe` — accepts audio file
- Sends to OpenAI Whisper API for transcription
- Returns { text, detectedLanguage }
- Optionally save AudioFile as StoredFile on the Question entity

### 3. Alternatives:
- Could use browser Web Speech API (free, offline). Very poor support for isiZulu and Sesotho. Whisper is significantly more accurate for SA languages.

### 4. Additional context, and milestones
- **Milestone:** Day 3 — Frontend + Voice
- **Acceptance:** Can tap mic, speak in isiZulu, see transcription appear, submit as question
- **Blocked by:** Issue #15

---

## Issue #17: Voice Output — TTS API Integration
**Labels:** `day-3` `frontend` `backend` `ai` `accessibility`

### 1. Is your feature request related to a problem? Please describe.
Blind users and those with limited literacy need to hear answers read aloud. Dyslexic users also benefit from audio output.

### 2. Solution/feature Description:
**Frontend:**
- Speaker/play button on each AI answer card
- Voice playback bar with waveform visualization
- Play/pause toggle
- For users with AutoPlayAudio enabled, auto-play when answer arrives

**Backend:**
- `POST /api/app/voice/speak` — accepts { text, language }
- Sends to OpenAI TTS API (model: tts-1, voice: alloy or nova)
- Returns audio stream (mp3)
- Optionally cache audio as StoredFile on Answer entity

### 3. Alternatives:
- Could use browser SpeechSynthesis API (free). Quality for SA languages is poor and inconsistent across browsers. OpenAI TTS sounds natural.

### 4. Additional context, and milestones
- **Milestone:** Day 3 — Frontend + Voice
- **Acceptance:** Can tap speaker button on any answer, hear it read aloud in the correct language
- **Blocked by:** Issue #15

---

## Issue #18: Contract Upload & Analysis Pipeline
**Labels:** `day-3` `backend` `ai`

### 1. Is your feature request related to a problem? Please describe.
Users need to upload contracts and receive AI-powered analysis with a health score, plain-language summary, and red flag alerts citing legislation.

### 2. Solution/feature Description:
Build ContractAnalysisService and ContractAppService:

**Backend pipeline:**
1. `POST /api/app/contract/analyse` — accepts PDF file upload (requires auth)
2. Extract text using PdfPig (same library as legislation). Fallback: if text extraction yields < 100 chars, send to OpenAI Vision API for OCR
3. Auto-detect contract type by sending first 500 chars to GPT-4o: "What type of contract is this? Respond with: Employment, Lease, Credit, or Service"
4. Build analysis prompt:
   - System prompt: "You are a contract analyst for South African law"
   - Include the full contract text
   - Include relevant legislation chunks from RAG search (search for key terms from the contract)
   - Instructions: "Return a JSON response with: healthScore (0-100), summary (plain language), flags (array of {severity, title, description, clauseText, legislationCitation})"
5. Parse JSON response, create ContractAnalysis and ContractFlag records
6. Return the full analysis result

**API endpoints:**
- `POST /api/app/contract/analyse` — upload and analyse
- `GET /api/app/contract/{id}` — get analysis result
- `GET /api/app/contract/my` — list current user's analyses
- `POST /api/app/contract/{id}/ask` — ask follow-up question about a contract (uses contract text + legislation as RAG context)

### 3. Alternatives:
- Could do the analysis in multiple API calls (one for type detection, one for scoring, one for flags). Single structured JSON response is faster and cheaper.
- Could skip the RAG legislation search and rely only on GPT-4o's training knowledge. RAG ensures citations are accurate to real SA legislation sections.

### 4. Additional context, and milestones
- **Milestone:** Day 3 — Contracts
- **Acceptance:** Can upload a lease PDF, receive health score, summary, and red flags citing real legislation sections
- **Blocked by:** Issue #4, #10

---

## Issue #19: Contract Analysis Results Page
**Labels:** `day-3` `frontend` `ui`

### 1. Is your feature request related to a problem? Please describe.
After uploading a contract, users need a rich visual display of the analysis results — health score, summary, red flags, and a follow-up chat.

### 2. Solution/feature Description:
Build the contract analysis results page at `/contracts/[id]`:
- Back link to contracts list
- Score ring: animated organic blob-shaped gauge showing the health score (0-100) with terracotta border, slightly rotated for handcrafted feel
- Document info: title, upload date, analysis time, pill tags (type, pages, language)
- Plain-language summary card on sand/stone background
- Breakdown row: 3 stat cards (red flags count, caution count, standard count)
- Red flags section: cards with burnt sienna left border, title, description, legislation citation
- Caution section: cards with terracotta left border
- Standard section: single moss green card confirming all standard clauses are in order
- Follow-up chat input at bottom for asking about specific clauses

Also build contract upload page at `/contracts`:
- Upload area (drag-and-drop or click to upload)
- Supported formats note (PDF, or photo via phone camera)
- List of user's previous analyses with scores

### 3. Alternatives:
- Could display results as a simple list. The visual score ring and animated red flag cards create a strong demo moment.

### 4. Additional context, and milestones
- **Milestone:** Day 3 — Contracts
- **Acceptance:** Upload a lease, see animated score, red flags with citations, can ask follow-up questions
- **Blocked by:** Issue #9, #18


═══════════════════════════════════════════════════════════
EPIC 4: EXPLORER + ADMIN + AUTH (Day 4)
═══════════════════════════════════════════════════════════

---

## Issue #20: My Rights Explorer Page
**Labels:** `day-4` `frontend` `ui`

### 1. Is your feature request related to a problem? Please describe.
Users need a way to browse their rights interactively by topic, not just through chat. This provides a discovery experience and shows the depth of the platform's content.

### 2. Solution/feature Description:
Build the My Rights explorer at `/rights`:
- Page heading and subtitle
- Knowledge score card: progress bar showing topics explored out of total, percentage in Fraunces serif
- Category filter tabs: horizontal pill buttons (All, Employment, Housing, Consumer, etc.)
- Rights cards grid (2 columns):
  - Collapsed state: title, legislation citation in moss green, one-line summary, plus icon
  - Expanded state (spans full width): full explanation, source quote box, action buttons ("Ask a follow-up", "Listen in {language}", "Share")
- Rights data sourced from FAQ conversations (IsPublicFaq = true)
- Knowledge score tracked client-side (which cards the user has expanded)
- Cards have asymmetric border-radius cycling through 6 patterns

### 3. Alternatives:
- Could use an accordion component. Custom expandable cards with varied radii feel more organic and less like a generic FAQ page.

### 4. Additional context, and milestones
- **Milestone:** Day 4 — Explorer + Admin
- **Acceptance:** Can browse categories, expand cards to see full rights explanations with citations, knowledge score updates
- **Blocked by:** Issue #9, #12 (FAQ endpoint)

---

## Issue #21: Admin Analytics Dashboard
**Labels:** `day-4` `frontend` `backend` `ui`

### 1. Is your feature request related to a problem? Please describe.
Administrators need visibility into platform usage — what questions are being asked, in which languages, how accurate the AI is, and which answers need review.

### 2. Solution/feature Description:
**Backend:**
- `GET /api/app/admin/stats` — returns aggregate stats (total questions, contracts analysed, avg response time, citation accuracy)
- `GET /api/app/admin/questions-by-category` — returns question counts grouped by category
- `GET /api/app/admin/language-distribution` — returns language and input method breakdown
- `GET /api/app/admin/top-questions` — returns top 10 most asked questions
- `GET /api/app/admin/review-queue` — returns answers where IsAccurate is null or false
- All endpoints require Admin role

**Frontend** at `/admin/dashboard`:
- Stat cards: 4 metrics with trend indicators (+/-% vs previous period)
- Questions by category: horizontal bar chart (Ant Design Charts)
- Language distribution: bars with dots, including voice vs text breakdown
- Top questions: numbered list with multilingual entries and ask count
- Review queue: list with status dots (red=flagged, amber=pending), description, time, "Review" button

### 3. Alternatives:
- Could use a third-party analytics tool (e.g. Metabase). Custom dashboard is more impressive in a demo and fully integrated.

### 4. Additional context, and milestones
- **Milestone:** Day 4 — Admin
- **Acceptance:** Dashboard loads with real data, all charts render, review queue shows flagged items
- **Blocked by:** Issue #9, #13

---

## Issue #22: Answer Quality Review System
**Labels:** `day-4` `backend` `frontend`

### 1. Is your feature request related to a problem? Please describe.
AI-generated answers may be inaccurate — citing wrong sections, mixing up SA law with other jurisdictions, or providing misleading information. Admins need to review and flag answers.

### 2. Solution/feature Description:
**Backend:**
- `PUT /api/app/admin/answer/{id}/review` — accepts { isAccurate: bool, adminNotes: string }
- Updates the Answer entity's IsAccurate and AdminNotes fields
- Flagged answers (IsAccurate = false) are excluded from FAQ generation

**Frontend** (within admin dashboard):
- Review queue shows unreviewed and flagged answers
- Click "Review" opens the answer with: original question, AI answer, citations, and the actual legislation text for comparison
- Admin can mark as accurate (green), flag as inaccurate (red), and add notes
- Flagged answers show the admin's correction notes

### 3. Alternatives:
- Could auto-flag answers below a citation relevance threshold. Manual review is more reliable for MVP. Auto-flagging can be added later.

### 4. Additional context, and milestones
- **Milestone:** Day 4 — Admin
- **Acceptance:** Admin can review an answer, mark as accurate or flag with notes, review queue updates accordingly
- **Blocked by:** Issue #21

---

## Issue #23: User Authentication
**Labels:** `day-4` `backend` `frontend`

### 1. Is your feature request related to a problem? Please describe.
Conversations and contract analyses require authentication (mandatory UserId). Users need to register and log in to use the platform.

### 2. Solution/feature Description:
**Backend:**
- ABP Identity module handles registration, login, token generation out of the box
- Configure JWT authentication
- Seed an admin user account during DbMigrator

**Frontend:**
- Login page with pill-shaped email and password inputs
- Register page with name, email, password, preferred language selector
- Auth token storage (cookies or localStorage)
- Protected routes: `/contracts`, `/admin/dashboard` require auth
- Unprotected routes: `/` (home), `/rights` (read-only), `/ask` (redirects to login when submitting)
- User avatar/initials in navbar when logged in

### 3. Alternatives:
- Could use social login (Google, etc.) for faster onboarding. ABP Identity with email/password is simpler for MVP and doesn't require third-party OAuth setup.

### 4. Additional context, and milestones
- **Milestone:** Day 4 — Auth
- **Acceptance:** Can register, login, see user initials in navbar, protected routes redirect to login
- **Blocked by:** Issue #5, #9

---

## Issue #24: FAQ Creation System (Admin)
**Labels:** `day-4` `backend`

### 1. Is your feature request related to a problem? Please describe.
The My Rights explorer and trending questions need curated, quality-checked content. Admins need a way to create FAQ conversations that are visible to all users.

### 2. Solution/feature Description:
**Backend:**
- `POST /api/app/admin/faq/create` — accepts { question, categoryId, language }
- Creates a Conversation with IsPublicFaq = true, FaqCategory = categoryId
- Runs the Q&A pipeline to generate the answer
- Returns the conversation for admin review
- `PUT /api/app/admin/faq/{conversationId}/publish` — sets IsAccurate = true on the answer, making it publicly visible
- `GET /api/app/question/faqs?categoryId={id}` — returns published FAQs for a category (used by home page and My Rights explorer)

Admin creates FAQs by asking questions through a dedicated admin interface. The system generates answers using the same RAG pipeline. Admin reviews, corrects if needed, and publishes.

### 3. Alternatives:
- Could create a separate FAQ entity. Using the same Conversation entity with an IsPublicFaq flag is cleaner — one data model, one pipeline, no duplication.

### 4. Additional context, and milestones
- **Milestone:** Day 4 — Admin
- **Acceptance:** Admin can create a FAQ, system generates an answer, admin publishes it, FAQ appears on home page and My Rights
- **Blocked by:** Issue #3, #12, #23


═══════════════════════════════════════════════════════════
EPIC 5: ACCESSIBILITY + POLISH + DEPLOY (Day 5)
═══════════════════════════════════════════════════════════

---

## Issue #25: Accessibility — Screen Reader & Keyboard Navigation
**Labels:** `day-5` `frontend` `accessibility`

### 1. Is your feature request related to a problem? Please describe.
Blind users rely on screen readers and keyboard navigation to use web applications. Without proper ARIA labels, semantic HTML, and focus management, the app is inaccessible.

### 2. Solution/feature Description:
- Add ARIA labels to all interactive elements (buttons, inputs, nav links)
- Add ARIA roles to landmark regions (nav, main, aside, footer)
- Ensure all focusable elements have visible focus rings (moss green ring-2 ring-offset-2)
- Tab order follows visual order on all pages
- Chat messages have aria-live="polite" for screen reader announcements
- Contract score and flags are announced to screen readers
- Skip-to-content link at top of page
- All images and icons have alt text or aria-hidden

### 3. Alternatives:
- Could defer accessibility to a future sprint. Given that accessibility is a core differentiator (blind and dyslexic users), it should be in the MVP.

### 4. Additional context, and milestones
- **Milestone:** Day 5 — Accessibility
- **Target:** WCAG 2.1 AA compliance
- **Acceptance:** Can navigate entire app with keyboard only, screen reader announces all content meaningfully
- **Blocked by:** Issues #14-20

---

## Issue #26: Dyslexia-Friendly Mode
**Labels:** `day-5` `frontend` `accessibility`

### 1. Is your feature request related to a problem? Please describe.
10-15% of South Africans have dyslexia. Dense legal text is particularly difficult for dyslexic users. They need an alternative reading mode with larger text, increased spacing, and a dyslexia-friendly font.

### 2. Solution/feature Description:
- Toggle switch in settings / navbar for dyslexia mode
- When enabled:
  - Font switches from Nunito to OpenDyslexic (import from CDN)
  - Font size increases by 2px across all body text
  - Line height increases to 2.0
  - Letter spacing increases to 0.05em
  - Paragraph spacing increases
  - Maximum line width reduced for easier tracking
- Preference saved to AppUser.DyslexiaMode and persisted across sessions
- Combined with voice output for maximum accessibility

### 3. Alternatives:
- Could just increase font size. OpenDyslexic's weighted bottoms specifically help dyslexic readers distinguish letter shapes.

### 4. Additional context, and milestones
- **Milestone:** Day 5 — Accessibility
- **Acceptance:** Toggle switches to OpenDyslexic font with increased spacing, preference persists after page reload
- **Blocked by:** Issue #5, #23

---

## Issue #27: Multilingual UI Label Files
**Labels:** `day-5` `frontend` `i18n`

### 1. Is your feature request related to a problem? Please describe.
Static UI elements (navigation, buttons, headings, placeholders, disclaimers) need to display in the user's selected language. Currently only English is supported.

### 2. Solution/feature Description:
Complete the 4 locale JSON files:
- `en.json` — English (already partially done)
- `zu.json` — isiZulu translations
- `st.json` — Sesotho translations
- `af.json` — Afrikaans translations

Cover all static text: navbar links, page headings, button labels, input placeholders, disclaimer text, category names, error messages, loading states, empty states.

Use next-intl's `useTranslations()` hook throughout all components.

### 3. Alternatives:
- Could use GPT-4o to generate the translations. For static UI text, human-quality translations are important — use a native speaker to verify at minimum.

### 4. Additional context, and milestones
- **Milestone:** Day 5 — Polish
- **Acceptance:** Switching language selector changes all UI text, no English fallbacks visible in isiZulu/Sesotho/Afrikaans modes
- **Blocked by:** Issue #9

---

## Issue #28: Disclaimers Integration
**Labels:** `day-5` `frontend` `ui`

### 1. Is your feature request related to a problem? Please describe.
The platform must clearly state that it provides legal/financial information, not professional advice. This is a legal requirement — especially for financial content under the FAIS Act.

### 2. Solution/feature Description:
- Three disclaimers pre-translated in all 4 languages:
  - Legal disclaimer — mentions Legal Aid SA (0800 110 110)
  - Financial disclaimer — mentions NCR (0860 627 627) and FAIS compliance
  - Contract analysis disclaimer — reminds users to consult an attorney before signing
- Show appropriate disclaimer after every AI answer (based on detected domain)
- Show contract disclaimer on every contract analysis result
- Persistent footer disclaimer on all pages
- Disclaimers stored in locale JSON files for i18n

### 3. Alternatives:
- Could show a single generic disclaimer for all content. Separate legal and financial disclaimers are more accurate — especially the FAIS Act reference for financial content.

### 4. Additional context, and milestones
- **Milestone:** Day 5 — Polish
- **Acceptance:** Every AI answer has appropriate disclaimer, all disclaimers display in user's language
- **Blocked by:** Issue #27

---

## Issue #29: Responsive Design & Error Handling
**Labels:** `day-5` `frontend` `ui`

### 1. Is your feature request related to a problem? Please describe.
The app needs to work on mobile devices (many South Africans access the internet primarily via phone) and handle errors gracefully.

### 2. Solution/feature Description:
**Responsive:**
- All grids collapse to single column on mobile
- Navbar switches to hamburger menu on mobile
- Chat input bar is full-width on mobile
- Contract score ring scales down appropriately
- Category cards stack vertically
- Touch targets minimum 44px

**Error handling:**
- API call failures show friendly error messages (not raw errors)
- Network offline detection with retry prompt
- Empty states for: no questions yet, no contracts yet, no search results
- Loading skeletons for all data-dependent sections
- 404 page for invalid routes

### 3. Alternatives:
- Could build a separate mobile app. Responsive web app reaches more users without requiring app store distribution.

### 4. Additional context, and milestones
- **Milestone:** Day 5 — Polish
- **Acceptance:** App fully usable on iPhone SE (smallest common screen), all errors handled gracefully
- **Blocked by:** Issues #14-20

---

## Issue #30: Azure DevOps CI/CD Pipeline & Deployment
**Labels:** `day-5` `backend` `frontend` `devops`

### 1. Is your feature request related to a problem? Please describe.
The application needs to be deployed to a staging environment with automated build and deployment via Azure DevOps, following Boxfusion's standard deployment practices.

### 2. Solution/feature Description:
- Create Azure DevOps project for MzansiLegal
- Set up CI pipeline:
  - Build .NET backend
  - Run backend unit tests (if any)
  - Build Next.js frontend
  - Publish artifacts
- Set up CD pipeline:
  - Deploy backend to Azure App Service (or Docker container)
  - Deploy frontend to Azure Static Web Apps (or same App Service)
  - Configure environment variables (OpenAI keys, connection strings)
  - Configure PostgreSQL on Azure
- Create work items board matching these GitHub issues
- Write deployment documentation

### 3. Alternatives:
- Could deploy to Vercel (frontend) + Railway (backend) for faster setup. Azure matches Boxfusion's standard infrastructure.

### 4. Additional context, and milestones
- **Milestone:** Day 5 — Deploy
- **Acceptance:** App accessible via a public URL, both frontend and backend running, can complete full demo flow
- **Blocked by:** All previous issues

---

## Issue #31: Demo Preparation & Documentation
**Labels:** `day-5` `documentation`

### 1. Is your feature request related to a problem? Please describe.
The 5-day sprint needs to end with a polished demo and documentation. Without preparation, the demo will be disorganised and key features might be missed.

### 2. Solution/feature Description:
- Prepare demo script (10 steps, ~8-10 minutes):
  1. Home dashboard — live stats, trending questions
  2. Contract analysis — upload real lease PDF
  3. Score animation, red flags with legislation citations
  4. Follow-up question about the contract
  5. Q&A chat — eviction question in isiZulu
  6. Cited answer, voice playback
  7. Cross-domain question in English
  8. My Rights explorer — knowledge score, expand card, listen in Sesotho
  9. Admin dashboard — analytics, review queue
  10. Wrap up
- Prepare test data: a real lease agreement PDF for contract analysis demo
- Seed at least 5 FAQ conversations for the My Rights explorer
- Create README.md (already done)
- Record a backup video of the demo in case of technical issues
- Prepare answers for likely questions: "How accurate is it?", "What about hallucination?", "Isn't this just a chatbot?"

### 3. Alternatives:
- Could skip demo prep and wing it. Prepared demos are significantly more impressive and show professionalism.

### 4. Additional context, and milestones
- **Milestone:** Day 5 — Demo
- **Acceptance:** Complete demo rehearsal runs smoothly end-to-end
- **Blocked by:** All previous issues

