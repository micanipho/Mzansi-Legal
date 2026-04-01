# Data Model: Intent-Aware Legal Retrieval for RAG Answers

**Feature**: `feat/021-intent-aware-rag` | **Date**: 2026-04-01  
**Status**: No new migrations planned

## Overview

This feature does not introduce new EF Core entities or schema changes. It refines the RAG service by:

- loading richer document and chunk metadata into memory at startup
- building document-level profiles for reranking and specificity scoring
- selecting answer modes from deterministic retrieval signals
- extending the ask-response DTO with structured response-state fields
- adding benchmark-style calibration artifacts without persisting new database records

## Existing Persistent Entities Reused

| Entity | Layer | Role in This Feature | Schema Change |
|--------|-------|----------------------|---------------|
| `LegalDocument` | Core | Supplies Act title, short name, act number, year, and category relationship | No |
| `Category` | Core | Supplies document category/domain label | No |
| `DocumentChunk` | Core | Supplies chunk text, section labels, keywords, and topic classification | No |
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
| `Year` | `int` | `LegalDocument.Year` | Act-number and year matching |
| `CategoryName` | `string` | `Category.Name` | Topic/category alignment |
| `SectionNumber` | `string` | `DocumentChunk.SectionNumber` | Citation label |
| `SectionTitle` | `string` | `DocumentChunk.SectionTitle` | Metadata relevance signal |
| `Excerpt` | `string` | `DocumentChunk.Content` | Prompt context |
| `Keywords` | `string[]` | Parsed from `DocumentChunk.Keywords` | Metadata and keyword alignment |
| `TopicClassification` | `string` | `DocumentChunk.TopicClassification` | Topic alignment |
| `TokenCount` | `int` | `DocumentChunk.TokenCount` | Context budgeting |
| `Vector` | `float[]` | `ChunkEmbedding.Vector` | Semantic search |

**Validation / rules**:
- `Excerpt` remains non-null in memory, even if normalized to empty string for safety.
- `Keywords` are parsed once and normalized at load time to avoid repeated string splitting on each request.
- `IndexedChunk` is runtime-only and never persisted back to the database.

---

### 2. `DocumentProfile` (document-level reranking model)

Built once from the loaded chunk set and stored in memory alongside `IndexedChunk`.

| Property | Type | Purpose |
|----------|------|---------|
| `DocumentId` | `Guid` | Document identity |
| `ActName` | `string` | Human-readable source label |
| `ActShortName` | `string` | Alias / short-title matching |
| `CategoryName` | `string` | Domain grouping |
| `MetadataTerms` | `string[]` | Normalized one-word and alias terms used for specificity scoring |
| `MetadataPhrases` | `string[]` | Normalized multi-word phrases used for phrase alignment |
| `Vector` | `float[]` | Document centroid embedding used for document-level semantic scoring |

**Validation / rules**:
- One `DocumentProfile` exists per document in the in-memory index.
- `MetadataTerms` and `MetadataPhrases` are derived from document labels, section titles, topic classifications, and keywords.
- `Vector` is computed as the centroid of the document's chunk embeddings and is not persisted.

---

### 3. `SourceHint`

Represents a signal extracted from the question that should influence document ranking.

| Property | Type | Purpose |
|----------|------|---------|
| `HintText` | `string` | Raw matched phrase from the question |
| `HintType` | `enum` | `ActTitle`, `ShortName`, `ActNumber`, `Category` |
| `MatchedDocumentId` | `Guid?` | Direct document hint when available |
| `MatchedCategoryName` | `string?` | Category-level hint when no document match exists |
| `BoostWeight` | `double` | Additive ranking signal |

**Rule**: `SourceHint` may boost a source but may not filter all non-matching sources out.

---

### 4. `DocumentCandidate`

Aggregated ranking view of a possible source document for the current question.

| Property | Type | Purpose |
|----------|------|---------|
| `DocumentId` | `Guid` | Candidate document |
| `ActName` | `string` | Display/citation label |
| `ActShortName` | `string` | Alias label |
| `CategoryName` | `string` | Topic grouping |
| `TopMatches` | `List<SemanticChunkMatch>` | Best matching chunk candidates for the document |
| `MaxChunkSemanticScore` | `float` | Strongest chunk similarity |
| `MeanTopChunkScore` | `float` | Stability of the strongest chunk set |
| `DocumentSemanticScore` | `float` | Similarity between question vector and document centroid |
| `MetadataAlignmentScore` | `float` | Match strength between question terms and document metadata |
| `SemanticBreadthScore` | `float` | Breadth of supporting chunks within the document |
| `HintBoostScore` | `float` | Explicit source-hint contribution |
| `FinalDocumentScore` | `float` | Combined ranking score |

**Validation / rules**:
- A `DocumentCandidate` originates from semantic matches but can be promoted by metadata alignment and source hints.
- `FinalDocumentScore` is derived from multiple signals and is never persisted.
- Supporting documents are selected only when they add distinct legal coverage and stay close enough to the primary document's score.

---

### 5. `RetrievedChunk`

Final prompt-ready chunk selected from the ranked document candidates.

| Property | Type | Purpose |
|----------|------|---------|
| `ChunkId` | `Guid` | Citation traceability |
| `DocumentId` | `Guid` | Source document grouping |
| `ActName` | `string` | Citation label |
| `ActShortName` | `string` | Alias context |
| `CategoryName` | `string` | Topic grouping |
| `SectionNumber` | `string` | Citation label |
| `SectionTitle` | `string` | Display and context aid |
| `Excerpt` | `string` | Prompt context excerpt |
| `TopicClassification` | `string` | Topic context |
| `Keywords` | `string[]` | Prompt/debug context and ranking input |
| `SemanticScore` | `float` | Raw semantic similarity |
| `RelevanceScore` | `float` | Blended prompt/citation relevance score |
| `TokenCount` | `int` | Context budgeting |

**Validation / rules**:
- `RetrievedChunk` is a reduced, prompt-ready projection of `SemanticChunkMatch`.
- Chunks are capped per document to prevent one long Act from crowding out supporting sources.

---

### 6. `RetrievalDecision`

Service-layer output of the retrieval planner and confidence evaluator.

| Property | Type | Purpose |
|----------|------|---------|
| `SelectedChunks` | `List<RetrievedChunk>` | Final context chunks passed to the prompt |
| `PrimaryDocumentId` | `Guid?` | Dominant governing source |
| `SupportingDocumentIds` | `List<Guid>` | Additional sources used in the answer |
| `RankedDocuments` | `List<DocumentCandidate>` | Ordered candidate set used for confidence evaluation |
| `ConfidenceBand` | `RagConfidenceBand` | `High`, `Medium`, `Low` |
| `AnswerMode` | `RagAnswerMode` | `Direct`, `Cautious`, `Clarification`, `Insufficient` |
| `ClarificationQuestion` | `string?` | Follow-up question when needed |
| `IsAmbiguousQuestion` | `bool` | Explicit ambiguity flag used by the confidence evaluator |
| `IsGroundedAnswer` | `bool` | True only for direct/cautious grounded answers |

**State transitions**:

```text
Strong aligned support       -> Direct
Grounded but diffuse support -> Cautious
Likely domain, missing facts -> Clarification
No responsible grounding     -> Insufficient
```

## Runtime Store

### `RagIndexStore`

Singleton application service that owns the in-memory retrieval state.

| Stored Collection | Type | Purpose |
|-------------------|------|---------|
| `LoadedChunks` | `IReadOnlyList<IndexedChunk>` | Semantic retrieval input |
| `DocumentProfiles` | `IReadOnlyList<DocumentProfile>` | Document reranking input |

**Rules**:
- `Replace()` updates both collections atomically after startup initialization.
- `IsReady` is true only when both chunks and document profiles are loaded.
- No request-time database reads are required once the store is ready.

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

## Quality Calibration Artifacts

### `RetrievalBenchmarkCase` (test/spec artifact, not persisted)

Used in spec docs and regression tests to calibrate retrieval and safety behavior.

| Property | Type | Purpose |
|----------|------|---------|
| `QuestionText` | `string` | Prompt variant to test |
| `ExpectedPrimarySource` | `string` | Governing Act or source family expected to rank first |
| `ExpectedSupportingSources` | `string[]` | Optional supplementary sources |
| `ExpectedAnswerMode` | `RagAnswerMode` | Safety posture expected for the prompt |
| `RiskTag` | `string` | Example: `plain-language`, `wrong-hint`, `multi-source`, `ambiguous`, `unsupported` |

**Rule**: `RetrievalBenchmarkCase` is kept in docs/tests only and introduces no schema change.

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
Startup load
  -> IndexedChunk[*]
  -> DocumentProfile[*]
  -> RagIndexStore

Question text
  -> SourceHint[*]
  -> IndexedChunk[*] semantic matches
  -> DocumentCandidate[*]
  -> RetrievedChunk[*]
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

No new persistence in this milestone. This preserves current `AnswerId` expectations, avoids schema churn, and matches the safety decision that non-grounded responses are not saved as legal answers.

## No Migration Required

All required source metadata already exists:

- `DocumentChunk.Keywords`
- `DocumentChunk.TopicClassification`
- `LegalDocument.ShortName`
- `LegalDocument.ActNumber`
- `LegalDocument.Year`
- `Category.Name`
- `Question.OriginalText`
- `Question.TranslatedText`
- `Conversation.Language`
- `Answer.Language`

No database migration is planned for this feature.
