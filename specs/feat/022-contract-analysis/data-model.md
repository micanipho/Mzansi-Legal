# Data Model: Contract Analysis

**Feature**: `feat/022-contract-analysis` | **Date**: 2026-04-02

## Overview

This feature is built mostly on top of existing persisted entities. The main additions are application-layer DTOs and computed runtime models that manage upload extraction, structured analysis generation, legislation retrieval, and contract-specific follow-up answers.

MVP does not require a new persisted follow-up chat model. Saved history is the contract analysis itself.

## Persisted Domain Entities

### ContractAnalysis

**Backed by**: `backend.Core/Domains/ContractAnalysis/ContractAnalysis.cs`

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `Guid` | Aggregate identifier |
| `UserId` | `long` | Owning ABP user |
| `OriginalFileId` | `Guid?` | Stored uploaded file reference |
| `ExtractedText` | `string` | Contract text produced by PdfPig or OCR |
| `ContractType` | `ContractType` | `Employment`, `Lease`, `Credit`, `Service` |
| `HealthScore` | `int` | 0-100 inclusive |
| `Summary` | `string` | Plain-language analysis summary |
| `Language` | `Language` | Output language for summary and findings |
| `AnalysedAt` | `DateTime` | UTC analysis completion timestamp |
| `Flags` | `ICollection<ContractFlag>` | Child findings |

**Role in this feature**:
- stores the canonical saved result for each uploaded contract
- anchors ownership and private history lookup
- provides the contract text used for follow-up questions

**Notes**:
- MVP assumes history and detail pages can be driven from this aggregate plus derived DTO fields.
- Stable display metadata such as file name should be reused from the stored-file path when possible; if implementation proves that impossible, a narrow additive schema change may be needed later.

### ContractFlag

**Backed by**: `backend.Core/Domains/ContractAnalysis/ContractFlag.cs`

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `Guid` | Flag identifier |
| `ContractAnalysisId` | `Guid` | Parent analysis |
| `Severity` | `FlagSeverity` | `Red`, `Amber`, or `Green` |
| `Title` | `string` | Short display heading |
| `Description` | `string` | Plain-language explanation |
| `ClauseText` | `string` | Exact clause excerpt from contract |
| `LegislationCitation` | `string` | Relevant statute citation when grounded |
| `SortOrder` | `int` | Display ordering within the analysis |

**Role in this feature**:
- stores the individual findings returned by the structured analysis prompt
- drives red-flag, caution, and standard-clause display on the detail view

### LegalDocument

**Backed by**: `backend.Core/Domains/LegalDocuments/LegalDocument.cs`

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `Guid` | Document identifier |
| `Title` | `string` | Full legislation title |
| `ShortName` | `string` | Citation-friendly alias |
| `ActNumber` | `string` | Act number or source id |
| `Year` | `int` | Publication year |
| `CategoryId` | `Guid` | Source category |
| `IsProcessed` | `bool` | Corpus ingestion status |
| `TotalChunks` | `int` | Indexed chunk count |

**Role in this feature**:
- provides the current legal corpus used to ground contract analysis and follow-up questions
- determines whether a contract issue is covered by current seeded authority

### DocumentChunk

**Backed by**: `backend.Core/Domains/LegalDocuments/DocumentChunk.cs`

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `Guid` | Chunk identifier |
| `DocumentId` | `Guid` | Parent legal document |
| `SectionNumber` | `string` | Citation locator |
| `SectionTitle` | `string` | Section heading |
| `Content` | `string` | Retrieved legal text |
| `Keywords` | `string` | Enriched keywords |
| `TopicClassification` | `string` | Topic metadata |
| `TokenCount` | `int` | Context-budget helper |

**Role in this feature**:
- supplies the legislation context for analysis flags and follow-up answers
- provides the legal excerpts used in citations

## Computed Runtime Models

### ContractUploadDraft

**Kind**: Upload-processing model

| Field | Purpose |
|-------|---------|
| `FileName` | Original uploaded file name |
| `ContentType` | Request validation |
| `ByteLength` | Upload size checks and logging minimization |
| `RequestedLanguage` | Target response language |
| `StoredFileId` | Saved binary-object reference |

### ContractExtractionResult

**Kind**: Extraction model

| Field | Purpose |
|-------|---------|
| `ExtractedText` | Readable contract text |
| `ExtractionMode` | `directPdfText` or `ocrFallback` |
| `CharacterCount` | Threshold and validation signal |
| `IsReadable` | Safe success/failure gate |
| `FailureReason` | User-facing unreadable guidance |

### ContractClassificationResult

**Kind**: Type-detection model

| Field | Purpose |
|-------|---------|
| `ContractType` | Supported family classification |
| `IsSupported` | Whether normal analysis should continue |
| `RawSignal` | Internal prompt output for diagnostics |
| `FailureReason` | User-facing unsupported guidance |

### ContractLegislationContext

**Kind**: Retrieval model

| Field | Purpose |
|-------|---------|
| `PrimaryChunks` | Main governing source chunks |
| `SupportingChunks` | Helpful supporting chunks when appropriate |
| `CoverageState` | `InCorpusNow`, `PartialCoverage`, or `NeedsCorpusExpansion` |
| `CoverageNotes` | Why the issue is well-covered or limited |

### ContractAnalysisDraft

**Kind**: Pre-persistence analysis model

| Field | Purpose |
|-------|---------|
| `HealthScore` | Model-generated score before validation |
| `Summary` | Plain-language user summary |
| `Flags` | Parsed findings before persistence |
| `CoverageState` | Carries retrieval limits into validation |
| `NeedsHumanReview` | Safety signal when support is weak |

### ContractAnalysisFlagDraft

**Kind**: Parsed structured finding

| Field | Purpose |
|-------|---------|
| `Severity` | Intended display severity |
| `Title` | Short finding title |
| `Description` | Plain-language explanation |
| `ClauseText` | Supporting contract excerpt |
| `LegislationCitation` | Required for definitive legal claims |
| `IsGrounded` | Result of citation validation |

### ContractHistoryItem

**Kind**: API projection

| Field | Purpose |
|-------|---------|
| `Id` | Analysis identifier |
| `DisplayTitle` | User-facing contract title |
| `ContractType` | Category display |
| `HealthScore` | Card score |
| `Summary` | Short list preview |
| `Language` | Output language |
| `AnalysedAt` | Sorting and display |
| `RedFlagCount` | Quick severity signal |
| `AmberFlagCount` | Quick caution signal |
| `GreenFlagCount` | Standard-clause count |

### ContractFollowUpRequest

**Kind**: Runtime request model

| Field | Purpose |
|-------|---------|
| `ContractAnalysisId` | Target saved analysis |
| `QuestionText` | User follow-up question |
| `RequestedLanguage` | Output language |

### ContractFollowUpAnswer

**Kind**: Runtime response model

| Field | Purpose |
|-------|---------|
| `AnswerText` | Contract-aware answer |
| `AnswerMode` | `direct`, `cautious`, or `insufficient` |
| `ConfidenceBand` | Retrieval confidence |
| `RequiresUrgentAttention` | Safety escalation signal |
| `LegislationCitations` | Grounding citations |
| `ContractExcerpts` | Relevant contract snippets used in reasoning |

## Validation Rules

### Upload validation

- Authentication is required.
- Only PDF uploads are accepted in MVP.
- Empty files must fail immediately.
- If direct extraction yields fewer than 100 meaningful characters, OCR fallback must run before unreadable failure is returned.
- Contracts that cannot be classified into the four supported types must not proceed to normal analysis.

### Analysis validation

- `HealthScore` must be between 0 and 100 inclusive.
- Each persisted flag must have `Title`, `Description`, `ClauseText`, and a valid `Severity`.
- A finding that states a legal right, obligation, prohibition, or statutory unfairness must include a grounded legislation citation.
- Weakly grounded findings must be downgraded to cautionary wording or omitted.

### Ownership and privacy validation

- List, detail, and follow-up requests must all scope by the authenticated user's `UserId`.
- Raw contract text must not be surfaced in list responses.
- Contract content must not be written to application logs in plaintext.

### Follow-up validation

- The target contract analysis must exist and belong to the requesting user.
- Follow-up answers must combine contract text with legislation context.
- If legal grounding is weak, the follow-up must return limitation language instead of uncited legal advice.

## State Transitions

### Analysis lifecycle

```text
Upload received
-> file stored
-> direct PDF text extraction
-> optional OCR fallback
-> supported contract classification
-> legislation retrieval
-> structured JSON analysis
-> validation and grounding checks
-> persisted ContractAnalysis + ContractFlags
```

### Failure path

```text
Upload received
-> extraction too thin or contract unsupported
-> safe user-facing failure
-> no completed ContractAnalysis persisted
```

### Follow-up lifecycle

```text
Saved analysis selected
-> follow-up question submitted
-> contract excerpts + legislation context assembled
-> conservative answer generation
-> one of:
   - Direct
   - Cautious
   - Insufficient
```
