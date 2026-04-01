# Data Model: Intent-Aware Legal Retrieval for RAG Answers

**Feature**: `feat/021-intent-aware-rag` | **Date**: 2026-04-01  
**Status**: No new migrations planned

## Overview

This feature does not introduce new EF Core entities or schema changes. It refines the RAG service by:

- loading richer metadata from existing legal-document entities into memory
- introducing internal application-layer retrieval models
- extending the ask-response DTO with structured response-state fields

## Existing Persistent Entities Reused

| Entity | Layer | Role in This Feature | Schema Change |
|--------|-------|----------------------|---------------|
| `LegalDocument` | Core | Supplies Act title, short name, act number, year | No |
| `Category` | Core | Supplies document category/domain label | No |
| `DocumentChunk` | Core | Supplies chunk text, section labels, keywords, topic classification | No |
| `ChunkEmbedding` | Core/EF | Supplies chunk vectors for semantic retrieval | No |
| `Conversation` | Core | Persists grounded user Q&A history | No |
| `Question` | Core | Persists original and translated user text | No |
| `Answer` | Core | Persists grounded answer text | No |
| `AnswerCitation` | Core | Persists citation traceability to selected chunks | No |

## New Application-Layer Models

### 1. `IndexedChunk` (internal retrieval cache model)

Loaded at startup and stored in memory for the lifetime of the app instance.

| Property | Type | Source | Purpose |
|----------|------|--------|---------|
| `ChunkId` | `Guid` | `DocumentChunk.Id` | Traceability |
| `DocumentId` | `Guid` | `DocumentChunk.DocumentId` | Document grouping |
| `ActName` | `string` | `LegalDocument.Title` | Citation label |
| `ActShortName` | `string` | `LegalDocument.ShortName` | Explicit source hint matching |
| `ActNumber` | `string` | `LegalDocument.ActNumber` | Explicit source hint matching |
| `CategoryName` | `string` | `Category.Name` | Topic/category alignment |
| `SectionNumber` | `string` | `DocumentChunk.SectionNumber` | Citation label |
| `SectionTitle` | `string` | `DocumentChunk.SectionTitle` | Secondary relevance signal |
| `Excerpt` | `string` | `DocumentChunk.Content` | Prompt context |
| `Keywords` | `string[]` | Parsed from `DocumentChunk.Keywords` | Metadata alignment |
| `TopicClassification` | `string` | `DocumentChunk.TopicClassification` | Metadata alignment |
| `TokenCount` | `int` | `DocumentChunk.TokenCount` | Context budgeting |
| `Vector` | `float[]` | `ChunkEmbedding.Vector` | Semantic search |

**Validation / rules**:
- `Excerpt` must remain non-null in memory, even if normalized to empty string for safety.
- `Keywords` are parsed once and normalized to lower case to avoid repeated string splitting on every request.

---

### 2. `SourceHint`

Represents a signal extracted from the question that should influence document ranking.

| Property | Type | Purpose |
|----------|------|---------|
| `HintText` | `string` | Raw matched phrase from the question |
| `HintType` | `enum` | `ActTitle`, `ShortName`, `ActNumber`, `Category`, `None` |
| `MatchedDocumentId` | `Guid?` | Direct document hint when available |
| `MatchedCategoryName` | `string?` | Category-level hint when no document match exists |
| `BoostWeight` | `decimal` or `double` | Additive ranking signal |

**Rule**: `SourceHint` may boost a source but may not filter all non-matching sources out.

---

### 3. `DocumentCandidate`

Aggregated ranking view of a possible source document for the current question.

| Property | Type | Purpose |
|----------|------|---------|
| `DocumentId` | `Guid` | Candidate document |
| `ActName` | `string` | Display/citation label |
| `CategoryName` | `string` | Topic grouping |
| `TopChunkIds` | `List<Guid>` | Candidate supporting chunks |
| `MaxSemanticScore` | `float` | Best matching chunk score |
| `MeanTopChunkScore` | `float` | Stability of support within the document |
| `TopicAlignmentScore` | `float` | How well topic metadata aligns |
| `KeywordAlignmentScore` | `float` | How well keyword metadata aligns |
| `HintBoostScore` | `float` | Explicit source-hint contribution |
| `FinalDocumentScore` | `float` | Combined ranking score |

**Validation / rules**:
- A document candidate must originate from at least one semantic chunk match.
- `FinalDocumentScore` is derived from multiple signals and is never a persisted field.

---

### 4. `RetrievalDecision`

Service-layer output of the retrieval planner and confidence evaluator.

| Property | Type | Purpose |
|----------|------|---------|
| `SelectedChunks` | `List<IndexedChunk>` | Final context chunks passed to the prompt |
| `PrimaryDocumentId` | `Guid?` | Dominant governing source |
| `SupportingDocumentIds` | `List<Guid>` | Additional sources used in the answer |
| `ConfidenceBand` | `RagConfidenceBand` | `High`, `Medium`, `Low` |
| `AnswerMode` | `RagAnswerMode` | `Direct`, `Cautious`, `Clarification`, `Insufficient` |
| `ClarificationQuestion` | `string?` | Follow-up question when needed |
| `IsGroundedAnswer` | `bool` | True only for direct/cautious grounded answers |

**State transitions**:

```text
Strong aligned support       -> Direct
Grounded but diffuse support -> Cautious
Likely domain, missing facts -> Clarification
No responsible grounding     -> Insufficient
```

---

## New DTO / Enum Changes

### `RagAnswerMode`

Application-layer enum serialized as a lower-case string in API responses.

| Value | Meaning |
|-------|---------|
| `Direct` | High-confidence grounded answer |
| `Cautious` | Grounded answer with explicit limitations |
| `Clarification` | Follow-up question required before a reliable answer |
| `Insufficient` | Corpus cannot responsibly answer the question |

### `RagConfidenceBand`

| Value | Meaning |
|-------|---------|
| `High` | Strongly aligned retrieval evidence |
| `Medium` | Grounded but not decisive enough for a fully direct posture |
| `Low` | Ambiguous, weak, or insufficient support |

### `RagAnswerResult` (modified)

Existing DTO extended with new fields:

| Property | Type | Notes |
|----------|------|-------|
| `AnswerText` | `string?` | Existing |
| `IsInsufficientInformation` | `bool` | Existing; now means "not enough grounded support to provide a definitive answer" |
| `Citations` | `List<RagCitationDto>` | Existing |
| `ChunkIds` | `List<Guid>` | Existing |
| `AnswerId` | `Guid?` | Existing; remains null when no grounded answer is persisted |
| `DetectedLanguageCode` | `string` | Existing |
| `AnswerMode` | `RagAnswerMode` | New |
| `ConfidenceBand` | `RagConfidenceBand` | New |
| `ClarificationQuestion` | `string?` | New |

**Rules**:
- `ClarificationQuestion` is only populated when `AnswerMode == Clarification`.
- `Citations` are required for material claims in `Direct` and `Cautious` responses.
- `AnswerId` remains null for `Clarification` and `Insufficient` modes in this milestone.

## Entity Relationships

### Persistent relationships (unchanged)

```text
Category (1) -------- (*) LegalDocument (1) -------- (*) DocumentChunk (1) -------- (0..1) ChunkEmbedding

Conversation (1) -------- (*) Question (1) -------- (0..1) Answer (1) -------- (*) AnswerCitation
                                                                              |
                                                                              `---- DocumentChunk
```

### Runtime relationships (new in-memory models)

```text
Question text
  -> SourceHint[*]
  -> IndexedChunk[*] semantic matches
  -> DocumentCandidate[*]
  -> RetrievalDecision
  -> RagAnswerResult
```

## Persistence Strategy

### Grounded answers

Persist exactly as today through the existing chain:

```text
Conversation -> Question -> Answer -> AnswerCitation[*]
```

### Clarification / insufficient responses

No new persistence in this milestone. This preserves the current `AnswerId` expectations and avoids schema or contract churn.

## No Migration Required

All required source metadata already exists:

- `DocumentChunk.Keywords`
- `DocumentChunk.TopicClassification`
- `LegalDocument.ShortName`
- `LegalDocument.ActNumber`
- `Category.Name`
- `Question.OriginalText`
- `Question.TranslatedText`
- `Conversation.Language`
- `Answer.Language`

No database migration is planned for this feature.
