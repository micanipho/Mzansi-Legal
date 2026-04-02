# MzansiLegal — RAG System

## Overview

MzansiLegal uses a **Retrieval-Augmented Generation (RAG)** architecture to answer legal and financial questions grounded in real South African legislation. The system retrieves the most relevant legislation chunks for a given question and passes them as context to GPT-4o, which generates a cited, plain-language answer.

The model is instructed to answer **only** from retrieved context — it cannot speculate or draw on general knowledge. If no relevant chunks are found, it says so.

---

## Architecture

```
PDF legislation
      │
      ▼
┌─────────────────────────┐
│ PdfIngestionService     │  Extracts text with PdfPig.
│                         │  Detects SA legislation section headings.
│                         │  Strategy: SectionLevel (≥ threshold sections)
│                         │            FixedSize fallback (500 tokens, 50 overlap)
│                         │  Splits large sections (>800 tokens) by subsection.
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ ChunkEnrichmentService  │  Calls gpt-4o-mini once per chunk.
│                         │  Returns 3–5 legal keywords + topic label (JSON).
│                         │  Non-fatal — falls back to empty metadata on failure.
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ EmbeddingService        │  Calls text-embedding-ada-002.
│                         │  Input truncated to 30,000 chars if needed.
│                         │  Returns float[1536] vector.
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ PostgreSQL              │  DocumentChunk row (content, section, keywords, topic)
│                         │  ChunkEmbedding row (vector stored as real[])
└─────────────────────────┘
```

At query time:

```
User question
      │
      ▼
EmbeddingService          — embed the question → float[1536]
      │
      ▼
Cosine similarity (in-memory) — score all chunks, filter ≥ 0.7, take top 5
      │
      ▼
Build prompt              — system rules + retrieved chunks as context + question
      │
      ▼
GPT-4o                    — temperature 0.2, answer grounded to context only
      │
      ▼
Answer + citations        — Act name, section number, relevance score
      │
      ▼
Persist                   — Conversation → Question → Answer → AnswerCitation
```

---

## Key Parameters

| Parameter | Value |
|---|---|
| Embedding model | `text-embedding-ada-002` |
| Vector dimensions | 1,536 |
| Enrichment model | `gpt-4o-mini` |
| Chat model | `gpt-4o` |
| Chat temperature | 0.2 |
| Similarity threshold | 0.7 (cosine) |
| Max context chunks | 5 per query |
| Max chunk input (embedding) | 30,000 chars |
| Max chunk input (enrichment) | 3,000 chars |

---

## Seeding (Offline — runs once via Migrator)

The migrator runs three sequential phases:

**Phase 1 — Database migrations**
EF Core applies pending migrations, creating all tables (`LegalDocuments`, `DocumentChunks`, `ChunkEmbeddings`, `IngestionJobs`, etc.).

**Phase 2 — Seed stubs**
`DefaultCategoriesCreator` inserts the 9 document categories if they don't exist:
Employment & Labour, Housing & Eviction, Consumer Rights, Debt & Credit, Privacy & Data, Safety & Harassment, Insurance & Retirement, Tax, Contract Analysis.

`LegalDocumentRegistrar` inserts 13 document stubs with `IsProcessed = false` and `TotalChunks = 0`. Already-existing stubs are skipped (idempotent).

**Phase 3 — ETL ingestion**
Runs after the migration transaction. Queries all documents where `IsProcessed = false` and runs each through the full pipeline (extract → enrich → embed → persist). On completion, marks `IsProcessed = true` and sets `TotalChunks = N`.

The migrator is fully idempotent — re-running skips already-processed documents.

---

## Legal Corpus

13 Acts across two domains, producing ~500–1,000 chunks total.

**Legal**
- Constitution of the Republic of South Africa, 1996
- Basic Conditions of Employment Act 75 of 1997 (BCEA)
- Consumer Protection Act 68 of 2008 (CPA)
- Labour Relations Act 66 of 1995 (LRA)
- Protection of Personal Information Act 4 of 2013 (POPIA)
- Rental Housing Act 50 of 1999
- Protection from Harassment Act 17 of 2011

**Financial**
- National Credit Act 34 of 2005 (NCA)
- Financial Advisory and Intermediary Services Act 37 of 2002 (FAIS)
- Tax Administration Act 28 of 2011
- Pension Funds Act 24 of 1956
- SARS tax guide
- FSCA materials

**Sources:** justice.gov.za, gov.za, sars.gov.za, fsca.co.za (all free/public)

---

## Data Model

| Entity | Purpose |
|---|---|
| `LegalDocument` | Act metadata — title, number, year, category, `IsProcessed`, `TotalChunks` |
| `DocumentChunk` | Section-level text — content, section number, chapter title, keywords, topic, sort order |
| `ChunkEmbedding` | `float[1536]` vector for a chunk (1-to-1 with `DocumentChunk`) |
| `IngestionJob` | Tracks progress and status of each ETL run |
| `Conversation` | Session record — session identifier, `startedAt`, `userId` (null for anonymous) |
| `Question` | Single question in a session — `originalText`, `translatedText`, `detectedLanguage`, `inputMethod`, `timestamp` |
| `Answer` | AI-generated response — `responseText`, `responseLanguage`, `timestamp` |
| `AnswerCitation` | Links an answer to a specific chunk — `citationOrder`, section number, excerpt, relevance score |

---

## Prompt Design

**System message (constant):**
```
You are a South African legal and financial assistant.
CRITICAL RULES:
1. ONLY answer using the legislation context provided.
2. ALWAYS cite every claim as [Act Name, Section X].
3. If context is insufficient, respond with exactly:
   "I don't have enough information..."
4. Do NOT speculate or use general knowledge.
5. Write in plain, accessible English.
```

**User message structure:**
```
Legislation context:

[Rental Housing Act — § 4(1)]
<chunk text>

[Constitution — § 26(3)]
<chunk text>

... (up to 5 chunks)

Question: <user question>

Answer (with citations):
```

---

## Multilingual Support

Questions can arrive in English, isiZulu, Sesotho, or Afrikaans. The `LanguageService` handles translation before retrieval:

1. Detect input language via GPT-4o (`"Respond with only the ISO code: en, zu, st, or af"`)
2. If not English, translate the question to English for embedding and retrieval
3. Store both `OriginalText` and `TranslatedText` on the `Question` entity
4. Add a language directive to the RAG prompt: `"Respond in isiZulu. Keep all Act names and section numbers in English."`

This means the knowledge base only needs English embeddings while still supporting native-language answers.

---

## Chunking Strategy

Chunks follow natural SA legislation structure where possible:

- **SectionLevel** (preferred) — one chunk per legal section, preserving chapter heading and section number. Used when ≥ threshold sections are detected in the PDF.
- **FixedSize** (fallback) — 500-token sliding window with 50-token overlap, used when section detection finds fewer than 3 sections.
- Sections larger than 800 tokens are split further by subsection markers — `(1)`, `(2)`, `(3)`, etc.

Each chunk stores: `SectionNumber`, `ChapterTitle`, `SortOrder`, `ChunkStrategy`, `TokenCount`.

---

## Persistence — Q&A Chain

After the answer is returned to the caller, the full interaction is persisted asynchronously (fire-and-forget). A storage failure is logged internally and never surfaced to the user. If any step in the chain fails, a full rollback is performed — no partial or orphaned records are left in the database.

```
Conversation  — session identifier, startedAt, userId (null for anonymous)
  └── Question — originalText, translatedText, detectedLanguage, inputMethod, timestamp
        └── Answer — responseText, responseLanguage, timestamp
              └── AnswerCitation × N — answerId, chunkId, citationOrder,
                                       sectionNumber, excerpt, relevanceScore
```

**Session rules:**
- Valid `conversationId` supplied → reuse existing `Conversation` record
- No / expired `conversationId` → create new `Conversation`
- Duplicate concurrent ID → idempotent upsert guard

**POPIA note:** Question text may contain personal information (names, ID numbers, case references). The schema is designed to support future deletion or de-identification requests in compliance with South African data-protection requirements.

---

## Why RAG over Fine-Tuning

| Concern | RAG approach |
|---|---|
| Legal accuracy | Model is grounded to exact legislation text — cannot hallucinate outside provided context |
| Updatability | Add a new Act PDF, run the migrator, it is live — no retraining required |
| Citations | Every answer traces back to specific sections with relevance scores |
| Cost | One embedding call at seed time; one embedding + one chat call per user question |
| Transparency | Retrieved chunks are stored as `AnswerCitation` records — every answer is auditable |

---

## Limitations and Known Constraints

- **In-memory retrieval** — all chunk vectors are loaded into memory on startup. Suitable for ~1,000 chunks (MVP). At larger scale, replace with pgvector or a managed vector store.
- **English embeddings only** — retrieval quality degrades if the translated question loses nuance. Monitor non-English query satisfaction separately.
- **No re-ranking** — chunks are ranked by cosine similarity only. A cross-encoder re-ranker would improve precision for ambiguous queries.
- **Static corpus** — the knowledge base reflects the Acts at seed time. Amendments require re-ingestion of the affected document.
- **LegalBERT not used** — embeddings use `text-embedding-ada-002` (general purpose). LegalBERT or a fine-tuned SA legal embedder could improve retrieval for technical legal terminology, but requires self-hosting.
