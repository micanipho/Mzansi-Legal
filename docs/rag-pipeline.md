# MzansiLegal RAG Pipeline — From Seeding to Prompts

## Overview

MzansiLegal uses a **Retrieval-Augmented Generation (RAG)** architecture. Legislation PDFs are
pre-processed offline into searchable chunks with embedding vectors. When a user asks a question,
the system finds the most relevant chunks and passes them as context to GPT-4o, which generates a
grounded, cited answer.

---

## Part 1 — Seeding (Offline, run once via Migrator)

The migrator runs in three sequential phases.

### Phase 1 — Database Migrations

EF Core applies any pending migrations to the Railway / local PostgreSQL database. This creates
all tables (`LegalDocuments`, `DocumentChunks`, `ChunkEmbeddings`, `IngestionJobs`, etc.) if they
don't already exist.

### Phase 2 — Seed Stubs (`InitialHostDbBuilder`)

Runs inside the migration transaction. Two seeders fire in order:

**1. `DefaultCategoriesCreator`**
Inserts the 9 document categories if they don't exist:
- Employment & Labour, Housing & Eviction, Consumer Rights, Debt & Credit, Privacy & Data,
  Safety & Harassment, Insurance & Retirement, Tax, Contract Analysis

**2. `LegalDocumentRegistrar`**
Inserts 13 document stubs (one row per Act) with `IsProcessed = false` and `TotalChunks = 0`.
If a stub already exists it is skipped — unless its `FileName` is empty, in which case it is
patched from the manifest (handles the LRA case where an old seed ran without filenames).

At this point the DB has metadata rows but **no text chunks or vectors**.

### Phase 3 — ETL Ingestion (`LegislationIngestionRunner`)

Runs after the migration transaction, outside any long-lived unit of work. Queries all documents
where `IsProcessed = false` and processes each one through the full pipeline.

For each document:

```
PDF file on disk
      │
      ▼
┌─────────────────────────────┐
│ 1. PdfIngestionAppService   │  Extracts text with PdfPig (word-level spatial grouping
│    IngestAsync()            │  to handle government gazette glyph concatenation).
│                             │  Detects SA legislation sections (§ headings).
│                             │  Chooses strategy:
│                             │    • SectionLevel  — if ≥ threshold sections detected
│                             │    • FixedSize     — fallback sliding window
│                             │  Returns ordered list of DocumentChunkResult objects.
└────────────┬────────────────┘
             │  (list of text chunks)
             ▼
┌─────────────────────────────┐
│ 2. ChunkEnrichmentAppService│  Calls gpt-4o-mini once per chunk.
│    EnrichAsync()            │  Prompt asks for 3–5 legal keywords + topic label.
│                             │  Parses JSON response (strips markdown fences).
│                             │  Failure is non-fatal — falls back to empty metadata.
└────────────┬────────────────┘
             │  (keywords, topic)
             ▼
┌─────────────────────────────┐
│ 3. EmbeddingAppService      │  Calls text-embedding-ada-002 once per chunk.
│    GenerateEmbeddingAsync() │  Input text truncated to 30,000 chars if needed.
│                             │  Returns float[1536] vector.
└────────────┬────────────────┘
             │  (1536-dim vector)
             ▼
┌─────────────────────────────┐
│ 4. Persist to DB            │  Inserts DocumentChunk row (content, section, keywords,
│                             │  topic, sortOrder, chunkStrategy).
│                             │  Inserts ChunkEmbedding row (vector stored as real[]).
│                             │  Updates IngestionJob progress counters.
└────────────┬────────────────┘
             │  (after all chunks)
             ▼
  LegalDocument.IsProcessed = true
  LegalDocument.TotalChunks = N
  IngestionJob.Status = Completed
```

The migrator is **idempotent** — re-running it skips already-processed documents and
already-applied migrations. Failed documents (e.g. missing PDF) can be retried by re-running.

---

## Part 2 — Runtime Q&A (Per user question)

### Startup — `RagAppService.InitialiseAsync()`

Called once when the web host starts. Loads every `DocumentChunk` that has a populated
`ChunkEmbedding` from the database into an in-memory list:

```
DB: DocumentChunks JOIN ChunkEmbeddings
        │
        ▼
_loadedChunks: List<ScoredChunk>
  - ChunkId
  - ActName       (from parent LegalDocument.Title)
  - SectionNumber
  - Excerpt       (chunk text)
  - Vector        (float[1536])
  - Score         (0 at load time, assigned per query)
```

All retrieval happens against this in-memory list — no DB query per question.

### Per Question — `RagAppService.AskAsync()`

```
User types: "Can my landlord evict me without a court order?"
      │
      ▼
┌─────────────────────────────────────────────────────────────┐
│ Step 1 — Embed the question                                 │
│                                                             │
│ EmbeddingAppService.GenerateEmbeddingAsync(questionText)    │
│   → calls text-embedding-ada-002                            │
│   → returns questionVector: float[1536]                     │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ Step 2 — Score all chunks (in-memory, no DB call)           │
│                                                             │
│ For each chunk in _loadedChunks:                            │
│   score = CosineSimilarity(questionVector, chunk.Vector)    │
│                                                             │
│ Filter:  score >= 0.7  (SimilarityThreshold)                │
│ Sort:    descending by score                                 │
│ Take:    top 5  (MaxContextChunks)                          │
│                                                             │
│ If 0 chunks pass threshold → return IsInsufficientInformation│
└──────────────────────────┬──────────────────────────────────┘
                           │  (topChunks: up to 5 ScoredChunk)
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ Step 3 — Build the prompt                                   │
│                                                             │
│ SYSTEM MESSAGE (constant):                                  │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ You are a South African legal and financial assistant.  │ │
│ │ CRITICAL RULES:                                         │ │
│ │ 1. ONLY answer using the legislation context provided.  │ │
│ │ 2. ALWAYS cite every claim as [Act Name, Section X].    │ │
│ │ 3. If context is insufficient, respond with exactly:    │ │
│ │    "I don't have enough information..."                 │ │
│ │ 4. Do NOT speculate or use general knowledge.           │ │
│ │ 5. Write in plain, accessible English.                  │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ USER MESSAGE:                                               │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ Legislation context:                                    │ │
│ │                                                         │ │
│ │ [Rental Housing Act — § 4(1)]                           │ │
│ │ No person may be evicted from their home without...     │ │
│ │                                                         │ │
│ │ [Rental Housing Act — § 13(2)]                          │ │
│ │ A landlord wishing to terminate a lease must...         │ │
│ │                                                         │ │
│ │ [Constitution — § 26(3)]                                │ │
│ │ No one may be evicted from their home without...        │ │
│ │                                                         │ │
│ │ ... (up to 5 chunks)                                    │ │
│ │                                                         │ │
│ │ Question: Can my landlord evict me without a court order│ │
│ │                                                         │ │
│ │ Answer (with citations):                                │ │
│ └─────────────────────────────────────────────────────────┘ │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ Step 4 — Call GPT-4o                                        │
│                                                             │
│ POST /v1/chat/completions                                   │
│   model:       gpt-4o                                       │
│   temperature: 0.2  (low — prioritises factual accuracy)    │
│                                                             │
│ Response example:                                           │
│   "No, your landlord cannot evict you without a court       │
│    order. Section 26(3) of the Constitution of South        │
│    Africa guarantees that no one may be evicted from        │
│    their home without a court order. [Constitution,         │
│    Section 26(3)]. The Rental Housing Act further           │
│    requires... [Rental Housing Act, Section 4(1)]."         │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ Step 5 — Persist the Q&A chain (if user is logged in)       │
│                                                             │
│ Conversation  (userId, language, startedAt)                 │
│   └── Question (originalText, language)                     │
│         └── Answer (text)                                   │
│               └── AnswerCitation × N                        │
│                     (chunkId, sectionNumber, excerpt,       │
│                      relevanceScore)                        │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
              Return RagAnswerResult to frontend:
                - answerText
                - citations[]  (actName, section, excerpt, score)
                - chunkIds[]
                - answerId
```

---

## Key Numbers

| Parameter | Value |
|---|---|
| Embedding model | `text-embedding-ada-002` |
| Vector dimensions | 1,536 |
| Enrichment model | `gpt-4o-mini` |
| Chat model | `gpt-4o` |
| Similarity threshold | 0.7 (cosine) |
| Max context chunks per query | 5 |
| Chat temperature | 0.2 |
| Max chunk input chars (embedding) | 30,000 |
| Max chunk input chars (enrichment) | 3,000 |

---

## Why RAG vs Fine-Tuning

| Concern | RAG approach |
|---|---|
| Legal accuracy | Model is grounded to exact legislation text — cannot hallucinate outside provided context |
| Updatability | Add a new Act PDF, run the migrator, it's live — no retraining |
| Citations | Every answer traces back to specific sections with relevance scores |
| Cost | Embedding once at seed time; only 1 embedding call + 1 chat call per user question |
