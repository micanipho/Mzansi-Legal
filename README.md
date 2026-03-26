# MzansiLegal

**Multilingual AI-Powered Legal & Financial Rights Assistant for South African Citizens**

MzansiLegal is a platform that helps South Africans understand their legal and financial rights by asking questions in their home language, uploading contracts for AI-powered analysis, and exploring their rights interactively. Every answer is backed by actual South African legislation with section-level citations.

---

## The Problem

- Attorney fees start at R1,500–R3,000/hour — out of reach for most South Africans
- Legislation is published only in English, excluding millions of isiZulu, Sesotho, and Afrikaans speakers
- 1.1 million South Africans are visually impaired; 10–15% have dyslexia
- Millions sign contracts (leases, employment, credit) without understanding unfair or illegal clauses
- Legal and financial problems are deeply intertwined but treated as separate domains

## The Solution

A single platform where citizens can:
- **Ask** legal and financial questions in English, isiZulu, Sesotho, or Afrikaans (text or voice)
- **Upload** contracts and get an instant health score, red flag alerts citing legislation, and a plain-language summary
- **Explore** their rights interactively with a gamified knowledge tracker
- **Listen** to answers read aloud — accessible for blind and dyslexic users

---

## Features

### AI Legal & Financial Q&A (RAG Pipeline)
- Ask a question in any supported language via text or voice
- System detects language, translates internally, searches legislation via embeddings
- Returns a cited answer in the user's language with Act name and section number
- Cross-domain questions (e.g. retrenchment rights + severance tax) answered holistically

### AI Contract Analysis
- Upload a lease, employment, credit, or service contract (PDF or photo)
- Auto-detects contract type
- Generates health score (0–100) with traffic light breakdown
- Flags red flags citing specific legislation (e.g. "Interest rate exceeds NCA maximum")
- Plain-language summary in the user's language
- Follow-up chat to ask questions about specific clauses

### Voice Support & Accessibility
- Voice input via OpenAI Whisper API (all 4 languages)
- Voice output via OpenAI TTS API (read answers aloud)
- Auto-play for blind users (configurable)
- Screen reader support with ARIA labels and keyboard navigation
- Dyslexia-friendly mode (OpenDyslexic font, increased spacing)
- High contrast mode respecting OS settings

### My Rights Explorer
- Interactive browse experience — not a static FAQ list
- Rights organised by category with expandable cards and legislation citations
- Gamified knowledge score tracking exploration progress
- "Ask a follow-up", "Listen in isiZulu", and "Share" actions per card

### FAQ System
- Admin creates conversations, system generates AI answers
- Admin reviews and publishes as public FAQs
- FAQs appear on home page, My Rights explorer, and as related suggestions
- Same Conversation entity — distinguished by `IsPublicFaq` flag

### Admin Analytics Dashboard
- Live stats: questions, contracts analysed, response time, citation accuracy
- Questions by category (bar chart)
- Language distribution with voice vs text breakdown
- Top questions (multilingual)
- Answer quality review queue with flagging tools

---

## Tech Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| Frontend | Next.js + Ant Design | Responsive multilingual web UI |
| Backend API | .NET 8 + ABP Framework | RESTful API with auth, modules |
| Database | PostgreSQL | All data storage |
| AI / LLM | OpenAI GPT-4o | Language detection, translation, Q&A, contract analysis |
| Speech-to-Text | OpenAI Whisper API | Voice input (4 languages) |
| Text-to-Speech | OpenAI TTS API | Voice output |
| Embeddings | text-embedding-ada-002 | Semantic search vectors |
| Vector Search | In-memory cosine similarity | Chunk retrieval |
| PDF Processing | PdfPig | Legislation and contract text extraction |
| Contract OCR | OpenAI Vision API | Text from photographed contracts |
| Auth | ABP Identity | Registration, login, roles |
| i18n | next-intl + JSON locales | UI in 4 languages |
| Hosting | Azure App Service / Docker | CI/CD via Azure DevOps |

---

## Supported Languages

| Language | Code | Speakers (L1+L2) | AI Quality |
|----------|------|-------------------|------------|
| English | en | 17M | Excellent |
| isiZulu | zu | 28M | Good |
| Sesotho | st | 14M | Good |
| Afrikaans | af | 16M | Very Good |

---

## Knowledge Base

### Legal Legislation
1. Constitution of the Republic of South Africa, 1996
2. Basic Conditions of Employment Act 75 of 1997 (BCEA)
3. Consumer Protection Act 68 of 2008 (CPA)
4. Labour Relations Act 66 of 1995 (LRA)
5. Protection of Personal Information Act 4 of 2013 (POPIA)
6. Rental Housing Act 50 of 1999
7. Protection from Harassment Act 17 of 2011

### Financial Legislation & Guidance
1. National Credit Act 34 of 2005 (NCA)
2. Financial Advisory and Intermediary Services Act 37 of 2002 (FAIS)
3. Tax Administration Act 28 of 2011
4. Pension Funds Act 24 of 1956
5. SARS personal income tax guides
6. FSCA consumer education materials

All sourced from justice.gov.za, sars.gov.za, and fsca.co.za.

---

## Domain Model

### Aggregates

```
STANDALONE:
  Category (Name, Icon, Domain: Legal|Financial)

KNOWLEDGE BASE:
  LegalDocument (root)
    └── DocumentChunk (PartOf)
         └── ChunkEmbedding (PartOf)

CONVERSATION:
  Conversation (root)
    └── Question (PartOf)
         └── Answer (PartOf)
              └── AnswerCitation (PartOf)

CONTRACT:
  ContractAnalysis (root)
    └── ContractFlag (PartOf)

USER:
  AppUser (extends IdentityUser)
```

### Relationships

```
PartOf (composition — filled diamond):
  AppUser       ◆───── Conversation
  AppUser       ◆───── ContractAnalysis
  LegalDocument ◆───── DocumentChunk
  DocumentChunk ◆───── ChunkEmbedding
  Conversation  ◆───── Question
  Question      ◆───── Answer
  Answer        ◆───── AnswerCitation
  ContractAnalysis ◆── ContractFlag

FK References (normal arrow):
  LegalDocument ──────> Category
  Conversation ───────> Category (nullable, for FAQs)
  AnswerCitation ─────> DocumentChunk (cross-aggregate)
```

### RefList Enumerations
- **Domain**: Legal | Financial
- **Language**: en | zu | st | af
- **InputMethod**: Text | Voice
- **UserRole**: Citizen | Admin
- **ContractType**: Employment | Lease | Credit | Service
- **FlagSeverity**: Red | Amber | Green

---

## Application Pages

| # | Page | Purpose |
|---|------|---------|
| 1 | Home Dashboard | Stats, categories, trending questions, CTAs |
| 2 | Contract Analysis | Upload, health score, red flags, follow-up chat |
| 3 | Q&A Chat | Multilingual conversation, voice I/O, citations |
| 4 | My Rights Explorer | Gamified rights browser with knowledge score |
| 5 | Admin Analytics | Charts, language distribution, review queue |

---

## How RAG Works

RAG (Retrieval-Augmented Generation) is the core of MzansiLegal:

1. **Ingest**: Legislation PDFs are uploaded, text is extracted, split into section-level chunks, and each chunk is converted to a 1,536-number embedding vector
2. **Store**: Chunks and embeddings are saved in the database
3. **Query**: When a user asks a question, the question is embedded using the same model
4. **Retrieve**: Cosine similarity finds the most relevant legislation chunks
5. **Generate**: The retrieved chunks are pasted into the LLM prompt as context, and the AI generates an answer based on the actual legislation — not from memory

This means every answer is grounded in real SA law, not AI hallucination.

---

## Multilingual Architecture

The system uses a "translate-in, process, translate-out" approach:

1. User speaks/types in isiZulu
2. Whisper transcribes (if voice)
3. System detects language, translates to English for search
4. RAG runs in English (embeddings stay in one language)
5. LLM prompt says: "Respond in isiZulu. Keep Act names and section numbers in English."
6. User gets answer in isiZulu with English citations

Adding a new language requires zero re-indexing.

---

## Project Structure

```
mzansilegal/
├── backend/
│   ├── src/
│   │   ├── MzansiLegal.Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── LegalDocument.cs
│   │   │   │   ├── DocumentChunk.cs
│   │   │   │   ├── ChunkEmbedding.cs
│   │   │   │   ├── Category.cs
│   │   │   │   ├── Conversation.cs
│   │   │   │   ├── Question.cs
│   │   │   │   ├── Answer.cs
│   │   │   │   ├── AnswerCitation.cs
│   │   │   │   ├── ContractAnalysis.cs
│   │   │   │   ├── ContractFlag.cs
│   │   │   │   └── AppUser.cs
│   │   │   └── Services/
│   │   │       ├── PdfIngestionService.cs
│   │   │       ├── EmbeddingService.cs
│   │   │       ├── RagService.cs
│   │   │       └── ContractAnalysisService.cs
│   │   ├── MzansiLegal.Application/
│   │   │   ├── QuestionAppService.cs
│   │   │   ├── ContractAppService.cs
│   │   │   └── AdminAppService.cs
│   │   ├── MzansiLegal.HttpApi/
│   │   └── MzansiLegal.DbMigrator/
│   └── test/
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── page.tsx                  # Home dashboard
│   │   │   ├── ask/page.tsx              # Q&A chat
│   │   │   ├── contracts/page.tsx        # Contract upload
│   │   │   ├── contracts/[id]/page.tsx   # Contract analysis results
│   │   │   ├── rights/page.tsx           # My Rights explorer
│   │   │   └── admin/
│   │   │       └── dashboard/page.tsx    # Admin analytics
│   │   ├── components/
│   │   │   ├── ChatInterface.tsx
│   │   │   ├── CitationCard.tsx
│   │   │   ├── ContractScoreRing.tsx
│   │   │   ├── RedFlagCard.tsx
│   │   │   ├── RightsCard.tsx
│   │   │   ├── CategoryGrid.tsx
│   │   │   ├── VoicePlayback.tsx
│   │   │   └── KnowledgeScore.tsx
│   │   ├── locales/
│   │   │   ├── en.json
│   │   │   ├── zu.json
│   │   │   ├── st.json
│   │   │   └── af.json
│   │   └── services/
│   │       └── api.ts
│   └── package.json
└── seed-data/
    ├── legislation/                # Downloaded PDFs from justice.gov.za
    └── financial/                  # NCA, SARS guides, FSCA materials
```

---

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- PostgreSQL
- OpenAI API key

### Backend Setup
```bash
# Scaffold ABP project
abp new MzansiLegal -t app --ui none --mobile none --db-provider ef -dbms PostgreSQL

# Install PdfPig for PDF processing
dotnet add package UglyToad.PdfPig

# Run migrations
dotnet run --project src/MzansiLegal.DbMigrator

# Start the API
dotnet run --project src/MzansiLegal.HttpApi.Host
```

### Frontend Setup
```bash
cd frontend
npm install
npm install antd @ant-design/icons @ant-design/charts next-intl

npm run dev
```

### Environment Variables
```env
# Backend
OpenAI__ApiKey=sk-...
OpenAI__EmbeddingModel=text-embedding-ada-002
OpenAI__ChatModel=gpt-4o
ConnectionStrings__Default=Host=localhost;Database=MzansiLegal;Username=postgres;Password=...

# Frontend
NEXT_PUBLIC_API_URL=https://localhost:44301/api
```

### Seed the Knowledge Base
1. Download legislation PDFs from justice.gov.za, sars.gov.za, fsca.co.za
2. Place them in `seed-data/legislation/` and `seed-data/financial/`
3. Use the admin upload endpoint or run the DbMigrator seed method

---

## 5-Day Implementation Plan

| Day | Focus | Key Deliverables |
|-----|-------|-----------------|
| 1 | Setup & Data Pipeline | ABP scaffold, PDF ingestion, seed 13 documents, Next.js shell |
| 2 | RAG + Multilingual Q&A | Embedding search, language detection, Q&A API |
| 3 | Frontend + Voice + Contracts | Home dashboard, chat with voice, contract analysis pipeline |
| 4 | Explorer + Admin + Auth | My Rights page, analytics dashboard, review queue, auth |
| 5 | Accessibility + Polish + Deploy | ARIA, dyslexia mode, i18n files, CI/CD, demo prep |

---

## Disclaimers

MzansiLegal provides legal and financial **information**, not professional **advice**.

- **Legal**: Contact a qualified attorney or Legal Aid South Africa (0800 110 110)
- **Financial**: Contact a registered financial advisor or the National Credit Regulator (0860 627 627)
- **Contracts**: Always have important contracts reviewed by a qualified attorney before signing

---

## License

This project was built as part of the Boxfusion Graduate Training Programme.