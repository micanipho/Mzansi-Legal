# Data Model: Intent-Aware Legal Retrieval for RAG Answers

**Feature**: `feat/021-intent-aware-rag` | **Date**: 2026-04-01

## Overview

This feature refines answer planning on top of the existing legal corpus and Q&A persistence model. Most of the data already exists in the domain model. The new planning emphasis is on computed runtime models that help the system:

- infer the supported legal issue from plain-language facts
- distinguish binding law from official guidance
- separate primary and supporting sources
- route ambiguous or urgent prompts into clarification or escalation-safe outcomes

No new database tables are required for the current slice. Where new concepts are needed, they are derived from existing entities or carried in append-only response DTO fields.

## Persisted Domain Entities

### Category

**Backed by**: `backend.Core/Domains/LegalDocuments/Category.cs`

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `Guid` | Aggregate identifier |
| `Name` | `string` | User-facing category name |
| `Icon` | `string` | Frontend display helper |
| `Domain` | `DocumentDomain` | Legal or financial grouping |
| `SortOrder` | `int` | Display order |

**Role in this feature**:
- groups source families for retrieval weighting
- provides a fallback signal when the user uses broad topic language
- supports coverage-state grouping in benchmarks

### LegalDocument

**Backed by**: `backend.Core/Domains/LegalDocuments/LegalDocument.cs`

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `Guid` | Aggregate identifier |
| `Title` | `string` | Full document title |
| `ShortName` | `string` | Citation-friendly alias |
| `ActNumber` | `string` | Act or source identifier |
| `Year` | `int` | Publication year |
| `FullText` | `string` | Full source text |
| `FileName` | `string` | Original file name |
| `OriginalPdfId` | `Guid?` | Linked stored file |
| `CategoryId` | `Guid` | Parent category |
| `IsProcessed` | `bool` | ETL completion status |
| `TotalChunks` | `int` | Number of persisted chunks |

**Role in this feature**:
- acts as the document-level retrieval unit after chunk candidate pooling
- anchors source-family grouping
- provides the raw basis for derived authority labels such as law vs guidance

**Derived runtime metadata for this feature**:

| Derived field | Source | Purpose |
|---------------|--------|---------|
| `SourceFamily` | `Title`, `ShortName`, `Category` | Groups semantically related sources for stable ranking |
| `AuthorityType` | Curated document rules | Distinguishes `bindingLaw` from `officialGuidance` |
| `CoverageState` | Benchmark map | Marks `InCorpusNow`, `NeedsCorpusExpansion`, `GuidanceOnly`, or `Escalate` |

### DocumentChunk

**Backed by**: `backend.Core/Domains/LegalDocuments/DocumentChunk.cs`

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `Guid` | Chunk identifier |
| `DocumentId` | `Guid` | Parent document |
| `ChapterTitle` | `string` | Chapter heading |
| `SectionNumber` | `string` | Section or locator |
| `SectionTitle` | `string` | Section heading |
| `Content` | `string` | Retrieval text |
| `TokenCount` | `int` | Context-budget helper |
| `SortOrder` | `int` | Reading order |
| `ChunkStrategy` | `ChunkStrategy?` | ETL strategy |
| `Keywords` | `string` | Enrichment keywords |
| `TopicClassification` | `string` | Enrichment topic label |

**Role in this feature**:
- remains the semantic retrieval unit
- carries the keyword and topic signals used in document reranking
- feeds citation locators and source excerpts back to the response contract

### Conversation

**Backed by**: `backend.Core/Domains/QA/Conversation.cs`

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `Guid` | Conversation identifier |
| `UserId` | `long` | Owning user |
| `Language` | `Language` | Preferred output language |
| `InputMethod` | `InputMethod` | Text or voice |
| `StartedAt` | `DateTime` | Session start |
| `IsPublicFaq` | `bool` | FAQ publication flag |
| `FaqCategoryId` | `Guid?` | FAQ grouping when public |

**Role in this feature**:
- provides multilingual continuity for ask responses
- keeps clarification and direct answers inside the current chat experience

### Question

**Backed by**: `backend.Core/Domains/QA/Question.cs`

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `Guid` | Question identifier |
| `ConversationId` | `Guid` | Parent conversation |
| `OriginalText` | `string` | User-submitted wording |
| `TranslatedText` | `string` | Search-language text |
| `Language` | `Language` | Submitted language |
| `InputMethod` | `InputMethod` | Text or voice |
| `AudioFile` | `string` | Voice artifact when present |

**Role in this feature**:
- stores the original wording used to assess ambiguity and risk triggers
- preserves translation context for retrieval and user-visible language return

### Answer

**Backed by**: `backend.Core/Domains/QA/Answer.cs`

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `Guid` | Answer identifier |
| `QuestionId` | `Guid` | Parent question |
| `Text` | `string` | Returned answer text |
| `Language` | `Language` | Output language |
| `AudioFile` | `string` | Voice artifact when present |
| `IsAccurate` | `bool?` | Admin review flag |
| `AdminNotes` | `string` | Review notes |

**Role in this feature**:
- persists grounded `Direct` and `Cautious` answers only
- does not persist clarification or insufficient outcomes in the current design

### AnswerCitation

**Backed by**: `backend.Core/Domains/QA/AnswerCitation.cs`

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `Guid` | Citation identifier |
| `AnswerId` | `Guid` | Parent answer |
| `ChunkId` | `Guid` | Referenced chunk |
| `SectionNumber` | `string` | Stored locator |
| `Excerpt` | `string` | Supporting text |
| `RelevanceScore` | `decimal` | Retrieval relevance |

**Role in this feature**:
- keeps persisted traceability between grounded answers and supporting source chunks
- remains the source for user-visible citations and later review

### IngestionJob

**Backed by**: `backend.Core/Domains/ETL/IngestionJob.cs`

| Field group | Notes |
|-------------|-------|
| Extract stage | Extraction timing and character counts |
| Transform stage | Chunking timing and produced chunk counts |
| Load stage | Persistence timing and loaded chunk counts |
| Error and orchestration | Status, strategy, user trigger, completion, error message |

**Role in this feature**:
- not changed by the retrieval-hardening slice
- remains the required tracking model if the team later expands the corpus bundles identified in legislation research

## Computed Runtime Models

### SupportedLegalIssue

**Kind**: Computed retrieval model

| Field | Purpose |
|-------|---------|
| `IssueLabel` | Human-readable legal issue inferred from user wording |
| `SourceFamily` | Stable grouping for ranking equivalent sources |
| `Confidence` | Strength of issue inference before answer-mode decision |
| `CoverageState` | Whether the issue is supported by the current corpus |

### SourcePresentation

**Kind**: Computed citation and UI model

| Field | Purpose |
|-------|---------|
| `SourceTitle` | Generic display title for any cited source |
| `SourceLocator` | Section, rule, form number, or other locator |
| `AuthorityType` | `bindingLaw` or `officialGuidance` |
| `SourceRole` | `primary` or `supporting` |
| `Excerpt` | Supporting text shown to the user |

**Notes**:
- planned as an additive evolution of the current citation DTO
- allows the frontend to explain when the system is citing law vs guidance

### ClarificationAssessment

**Kind**: Computed answer-planning model

| Field | Purpose |
|-------|---------|
| `HasMaterialFactGap` | Whether missing facts block a safe answer |
| `GapReason` | Short description of the ambiguity |
| `ClarificationQuestion` | Single focused follow-up question |
| `RiskTrigger` | Urgency or high-stakes signal attached to the prompt |

### RagAnswerOutcome

**Kind**: Computed API result model

| Field | Purpose |
|-------|---------|
| `AnswerMode` | `direct`, `cautious`, `clarification`, or `insufficient` |
| `ConfidenceBand` | `high`, `medium`, or `low` |
| `AnswerText` | User-visible answer or limitation text |
| `ClarificationQuestion` | Follow-up prompt when needed |
| `Citations` | SourcePresentation collection |
| `PersistedAnswerId` | Present only for grounded persisted answers |

## Validation Rules

### Ask request

- `QuestionText` is required and must not be blank.
- Extremely short prompts are allowed but should bias toward clarification or escalation rather than a confident direct answer.

### Source labeling

- A source labeled `officialGuidance` must not be presented as the controlling legal source when a binding legal source is present.
- A response may include both `bindingLaw` and `officialGuidance`, but one or more `bindingLaw` citations must anchor grounded legal claims.
- Guidance-only responses are allowed only when the system clearly limits what the guidance means and does not overstate legal certainty.

### Persistence

- `Direct` and `Cautious` grounded answers may persist `Answer` and `AnswerCitation` records.
- `Clarification` and `Insufficient` outcomes should not create persisted answer records in the current slice.

## State Transitions

### Answer Mode Transition

```text
Question submitted
-> retrieval and issue inference
-> coverage and authority assessment
-> confidence and risk evaluation
-> one of:
   - Direct         (strong support, safe to answer)
   - Cautious       (grounded but qualified)
   - Clarification  (material fact gap)
   - Insufficient   (weak support, unsupported area, or escalation-safe limitation)
```

### Coverage State Transition

```text
Scenario identified
-> source family mapped
-> one of:
   - InCorpusNow
   - GuidanceOnly
   - NeedsCorpusExpansion
   - Escalate
```

This second state is mainly a planning and verification tool. It keeps the benchmark matrix honest about what the current seed corpus can actually support.
