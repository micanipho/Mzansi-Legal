# Data Model: RAG Question-Answering Service

**Feature**: `feat/014-rag-qa-service` | **Date**: 2026-03-30

## Overview

No new domain entities or migrations are required. This feature is a read-and-write consumer of entities created in prior features:

| Entity | From Feature | Role in This Feature |
|--------|-------------|----------------------|
| `DocumentChunk` | 004-rag-domain-model | Source of context text, Act name, section number |
| `ChunkEmbedding` | 009-openai-embedding-service | Pre-computed vectors loaded at startup for similarity search |
| `LegalDocument` | 004-rag-domain-model | Provides `Title` (Act name) via `DocumentChunk.Document` |
| `Conversation` | 005-qa-domain-model | Created per user Q&A session |
| `Question` | 005-qa-domain-model | Stores the user's question text |
| `Answer` | 005-qa-domain-model | Stores the generated answer text |
| `AnswerCitation` | 005-qa-domain-model | Links each answer to the specific chunks used |

---

## New Application-Layer Artifacts

### `AskQuestionRequest` (Input DTO)

| Property | Type | Constraint | Description |
|----------|------|------------|-------------|
| `QuestionText` | `string` | Required, MaxLength 30,000 | The user's natural-language legal question |

---

### `RagCitationDto` (Output DTO)

Represents a single source citation returned with the answer.

| Property | Type | Description |
|----------|------|-------------|
| `ChunkId` | `Guid` | ID of the `DocumentChunk` used |
| `ActName` | `string` | Name of the legislation Act (from `LegalDocument.Title`) |
| `SectionNumber` | `string` | Section identifier (from `DocumentChunk.SectionNumber`, e.g., "§ 26(3)") |
| `Excerpt` | `string` | Relevant passage from the chunk's `Content` field (first 500 chars) |
| `RelevanceScore` | `float` | Cosine similarity score; values in [0.7, 1.0] (only above-threshold chunks included) |

---

### `RagAnswerResult` (Output DTO)

Returned by `IRagAppService.AskAsync` and by `QaController`.

| Property | Type | Description |
|----------|------|-------------|
| `AnswerText` | `string?` | The LLM-generated answer; `null` when `IsInsufficientInformation` is `true` |
| `IsInsufficientInformation` | `bool` | `true` when no chunk exceeded the 0.7 similarity threshold |
| `Citations` | `List<RagCitationDto>` | Ordered by `RelevanceScore` descending; empty list when insufficient information |
| `ChunkIds` | `List<Guid>` | IDs of all chunks passed to the LLM as context; empty when insufficient information |
| `AnswerId` | `Guid?` | ID of the persisted `Answer` entity; `null` when insufficient information |

---

### `RagPromptBuilder.ScoredChunk` (Internal Record)

Internal to `RagPromptBuilder`; not exposed via the HTTP contract.

| Property | Type | Description |
|----------|------|-------------|
| `ChunkId` | `Guid` | ID of the `DocumentChunk` |
| `ActName` | `string` | Act name for the prompt context label |
| `SectionNumber` | `string` | Section number for the prompt context label |
| `Excerpt` | `string` | Chunk content (truncated as needed) |
| `Score` | `float` | Cosine similarity score assigned during retrieval |
| `Vector` | `float[]` | The embedding vector (used during scoring; not included in output) |

---

## Entity Relationships (Existing — No Changes)

```text
LegalDocument (1) ──── (*) DocumentChunk (1) ──── (0..1) ChunkEmbedding
                                                         Vector: float[]

Conversation (1) ──── (*) Question (1) ──── (*) Answer (1) ──── (*) AnswerCitation
                                                                        │
                                                                        └─── DocumentChunk (cross-aggregate FK)
```

**Write path** (this feature adds rows to):
- `Conversations` — one per ask (or reused for session; MVP creates a new one per call for simplicity)
- `Questions` — one per `AskAsync` call
- `Answers` — one per successful LLM response
- `AnswerCitations` — one per retrieved chunk (up to 5 per answer)

**Read path** (this feature reads from):
- `DocumentChunks` with `Include(Embedding)` and `Include(Document)` — at startup only
- All subsequent reads use the in-memory `_loadedChunks` list

---

## No Migration Required

All tables and columns required by this feature already exist in the database schema. The `Language.English = 0` enum value will be used for all `Answer.Language` and `Conversation.Language` fields in this feature.
