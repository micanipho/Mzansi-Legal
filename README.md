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

| Layer          | Technology                  | Purpose                                                 |
| -------------- | --------------------------- | ------------------------------------------------------- |
| Frontend       | Next.js + Ant Design        | Responsive multilingual web UI                          |
| Backend API    | .NET 8 + ABP Framework      | RESTful API with auth, modules                          |
| Database       | PostgreSQL                  | All data storage                                        |
| AI / LLM       | OpenAI GPT-4o               | Language detection, translation, Q&A, contract analysis |
| Speech-to-Text | OpenAI Whisper API          | Voice input (4 languages)                               |
| Text-to-Speech | OpenAI TTS API              | Voice output                                            |
| Embeddings     | text-embedding-ada-002      | Semantic search vectors                                 |
| Vector Search  | In-memory cosine similarity | Chunk retrieval                                         |
| PDF Processing | PdfPig                      | Legislation and contract text extraction                |
| Contract OCR   | OpenAI Vision API           | Text from photographed contracts                        |
| Auth           | ABP Identity                | Registration, login, roles                              |
| i18n           | next-intl + JSON locales    | UI in 4 languages                                       |
| Hosting        | Azure App Service / Docker  | CI/CD via Azure DevOps                                  |

---

## Supported Languages

| Language  | Code | Speakers (L1+L2) | AI Quality |
| --------- | ---- | ---------------- | ---------- |
| English   | en   | 17M              | Excellent  |
| isiZulu   | zu   | 28M              | Good       |
| Sesotho   | st   | 14M              | Good       |
| Afrikaans | af   | 16M              | Very Good  |

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

Current public sources include gov.za, justice.gov.za, sars.gov.za, and
fsca.co.za.

---

## Trust Guardrails

- **Primary law comes first**: the Constitution, statutes, regulations, and
  authoritative judgments outrank official guidance, forms, and regulator
  material in retrieval and answers.
- **Guidance is labeled**: official guides and forms may explain procedure, but
  they never replace binding law in legal conclusions.
- **High-risk matters escalate**: arrest, violence, imminent eviction, urgent
  deadlines, and similarly high-stakes matters trigger clarifying questions and
  visible guidance to seek human or emergency help.
- **POPIA is a product requirement**: retention, security, breach response, and
  cross-border vendor posture are treated as implementation requirements, not
  post-launch cleanup.
- **Corpus ingestion is official-source-first**: prefer gov.za, Government
  Gazette materials, justice.gov.za forms/guidance, apex-court repositories,
  and link-only handling for licensing-sensitive compiled editions unless
  permission is recorded.

---

## Design

## [Wireframes](https://www.figma.com/file/ATCmuf9e3eFScItIYkp2Zd/innminds-tutors?type=design&node-id=0%3A1&mode=dev&t=KOQnxVDxgaIX0gUm-1)

## [Figma Design](https://www.figma.com/design/TLWojEI22q0P38GRWogmMZ/Mzansi-Legal?node-id=0-1&t=CdRlpnxV9hpGaQqO-1)

## [Domain Model](https://lucid.app/lucidchart/3b02f4bb-af2b-4a9e-a99e-216f699c0555/edit?view_items=G0sID~H.Ll67&page=0_0&invitationId=inv_6692b21a-876f-4dbf-ad6c-b643288bdb25)

## [State diagram](https://viewer.diagrams.net/?tags=%7B%7D&highlight=0000ff&edit=_blank&layers=1&nav=1&title=Untitled%20Diagram.drawio#R7Vzfc5s4EP5r%2FJgbkMB2HlMn%2FTGTzmTGnbumb7KRDSlGnBCxub%2F%2BhBEgEKlJY3tx2zwk1krC4ttvtdpdyAjPNrsPnMT%2BZ%2BbRcIQsbzfCtyOE0NiZyD%2B5JFMS18aFZM0Dr5DZtWAe%2FEWV0FLSNPBo0hgoGAtFEDeFSxZFdCkaMsI52zaHrVjY%2FNaYrKkhmC9JaEr%2FCTzhK6ltWXXHRxqsffXVU1d1bEg5WAkSn3hsq4nw3QjPOGOi%2BLTZzWiYo1fiUsx7%2F0JvtTBOI9Fnwu3nBXn6lD18u4v87NvSuv%2F%2B5dOVusozCVN1w2qxIisRkFeRYMvGO19sQimz5Ud5M3HenwjCxVwQkfevgjCcsZDx%2FURs7X%2FywYKz71TrWa1Uj%2Fp2ygXdvXhbdgWWpBllGyp4JofsKlUUU0qGXav2tlYXViJfU1QpI4og6%2BrKNYbyg4LxFZCiHpB6kmOqybjw2ZpFJLyrpTrUOTqBJORNGKwjKVswIdhGdtDIu8kZnl8kplEhURY0%2FSHsnKWRR%2FObsPaz5G1%2F1RuPeeMvt2ze7vTO20y1itvK76WhuoSlfEkPc85QsbRswtdU%2FGAq7qYCpyERwXNzHV2KVVMfWCC%2FuaYQchsUwuMWN4p1qVktelTL%2BHnGYIMxH%2BU1y82pTR6NGls%2FEHQekz3cW7kDH8ekKtsoTWpqmlRldrpNuaeyqeufMKEmyesx94zFCr8nKkSmLIakgjUNr7aL2hQe9b7X2UUH3w%2BaitPTLsZvtIs3KadkwjC0Yw1POxNQ7dgg2tkF4qv2WfMoslWrJm9ko5P4oVNqdAqpUcdwF2QpAhYZei4Padxnm0Uql%2FEupjyQi6C8lj7Uok5%2FoimVqBPIUuK6H38EX%2BO0j28T1%2FQ1qMPXtH300eB1gXczazxp8d8%2B5HH2LU2RxzYL3NMsHEizGBtmcRPHYbYP%2Brj8PRepR9UBTtcuBOlRk%2FT2pOOAZXWQ3jkV6ScHwPsiGcuHCB2eduwXZ4WuTGj8cbA9dpJpz53ERpBbydSwhpCtA9PBAvB%2FjNqxGTj%2Fx8AOEyo4s1FfMoP6RXsyJP2cMTzrrx8XVD8wyY3L9B%2F9dQoaopXL1DyIyOI8pcdW8leaUPMwdQnR2tQZWrSGoMM1MO%2FTN1eBYL2Pma14KRCrtbKnL18q%2BG0LiOxuK0rD0FEagjlqKVdRu4dHreeQq4Bz%2F25fAwGNNcplagbSHWwP3TwcBB2JYHQpJ6lj0rxvSI3eWjx9G83NmPrG23TE1EOjuW2NhxZxlwcsDcsZp%2FkzGMi6p0nSUQoYwP4w7qoinxc30EjLesX%2BULnP6kB59kgL9Y20EGh5E%2F%2Buxef%2B%2BrFA9XMx5WcQ3YBmKcpl6j65byXZY%2Bli32tfRKLCdTqcz1kTFQ6MIcAnKlDv6jHoRuVcfBlvcs7TQW%2BlggbXzqBqU%2Bc8HfTWD2jtA5mPvv4qHsjGg8uVYwfCGo7J6r45bwya68CwadqGK0GDL%2Bnh675KfetWtZ96wznJtAFx%2FlR%2Fol259XLAdSv9M2m9ZXNg%2BFWZ4qgJVaygptcx3iCY%2FjaWDZptQGY161fxV%2B2I6aqr2nXe0q75UOLfAd1KyS1J%2FAUj3DNgh3giy2qFmi54ntPMtXP6b0qTQTz8alcsGgxg2DyGPhdMS9LFE10W%2FgEauHZC%2FcqGB84sXuavzkYe4UNAzG6%2FHHpVcQ8OMnNXU1wLB1O6Gbdhg0ft2kDtgbNVEJrvRw6BZ%2BCGWV64Y0cTZBjbmY0H93xNkaxqOc51kAhOOg95EKi1jxudaYXzotbhBMpydOeTjkMgm3NCsslm%2FW8dihCv%2Fu8Y%2BO5%2F)

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

| #   | Page               | Purpose                                         |
| --- | ------------------ | ----------------------------------------------- |
| 1   | Home Dashboard     | Stats, categories, trending questions, CTAs     |
| 2   | Contract Analysis  | Upload, health score, red flags, follow-up chat |
| 3   | Q&A Chat           | Multilingual conversation, voice I/O, citations |
| 4   | My Rights Explorer | Gamified rights browser with knowledge score    |
| 5   | Admin Analytics    | Charts, language distribution, review queue     |

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
│   │   ├── backend.Core/               # Domain Layer (Entities, Services)
│   │   │   ├── Domains/
│   │   │   │   ├── LegalDocuments/
│   │   │   │   ├── QA/
│   │   │   │   └── ContractAnalysis/
│   │   ├── backend.Application/        # Application Layer (AppServices)
│   │   ├── backend.EntityFrameworkCore/# Infrastructure Layer (DB Context)
│   │   ├── backend.Migrator/           # DB Seed & Migrations
│   │   └── backend.Web.Host/           # API Host
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── [locale]/               # I18n Grouped Pages
│   │   │   │   ├── page.tsx            # Home dashboard
│   │   │   │   ├── ask/                # Q&A chat
│   │   │   │   ├── contracts/          # Contract analysis
│   │   │   │   ├── rights/             # My Rights explorer
│   │   │   │   ├── history/            # User history
│   │   │   │   ├── admin/              # Admin dashboard
│   │   │   │   └── auth/               # Auth routes
│   │   ├── components/                 # UI Components
│   │   ├── locales/                    # Intal translations (en, zu, st, af)
│   │   └── services/                   # API clients
└── seed-data/
    ├── legislation/                    # Legislation PDFs
    └── financial/                      # Financial guides
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
# Choose backend.Web.Host as startup project in Visual Studio
# Run migrations using Migrator project
dotnet run --project src/backend.Migrator

# Or start the API
dotnet run --project src/backend.Web.Host
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
# Backend (appsettings.json)
OpenAI__ApiKey=sk-...
ConnectionStrings__Default=Host=localhost;Database=MzansiLegal;Username=postgres;Password=...

# Frontend (.env)
NEXT_PUBLIC_BASE_URL=http://localhost:5000
```

---

## Disclaimers

MzansiLegal provides legal and financial **information**, not professional **advice**.

- **Legal**: Contact a qualified attorney or Legal Aid South Africa (0800 110 110)
- **Financial**: Contact a registered financial advisor or the National Credit Regulator (0860 627 627)
- **Contracts**: Always have important contracts reviewed by a qualified attorney before signing

---

## License

This project was built as part of the Boxfusion Graduate Training Programme.
