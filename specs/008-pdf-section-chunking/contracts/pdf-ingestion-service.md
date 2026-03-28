# Contract: IPdfIngestionAppService

**Feature**: 008-pdf-section-chunking
**Layer**: Application (internal service — no HTTP endpoint)
**Date**: 2026-03-28

---

## Overview

`PdfIngestionAppService` is an **internal application service** called by other AppServices or admin-facing controllers. It has no direct REST endpoint — the contract is the C# interface.

---

## Interface Contract

### Method: `IngestAsync`

```csharp
Task<IReadOnlyList<DocumentChunkResult>> IngestAsync(IngestPdfRequest request)
```

**Input: `IngestPdfRequest`**

| Field | Type | Required | Description |
|---|---|---|---|
| `PdfStream` | `Stream` | ✅ | Readable byte stream of the legislation PDF. Must be open and readable when the method is called. |
| `ActName` | `string` | ✅ | Official Act name (e.g., "Labour Relations Act"). Used as `ActName` on every returned chunk. |
| `DocumentId` | `Guid` | ✅ | ID of the parent `LegalDocument` entity. Set as `DocumentId` on each `DocumentChunk` by the caller. |
| `IngestionJobId` | `Guid` | ✅ | ID of the pre-created `IngestionJob` entity. The service updates this job at each pipeline stage. |

**Output: `IReadOnlyList<DocumentChunkResult>`**

Each `DocumentChunkResult` in the returned list:

| Field | Type | Nullable | Description |
|---|---|---|---|
| `ActName` | `string` | No | Verbatim copy of `request.ActName`. |
| `ChapterTitle` | `string?` | Yes | Full chapter identifier including number (e.g., "Chapter 2 — Fundamental Rights"). Null when no chapter detected. |
| `SectionNumber` | `string?` | Yes | Section number (e.g., "12", "12A"). Null for fixed-size chunks. |
| `SectionTitle` | `string?` | Yes | Section heading text. Null for fixed-size chunks. |
| `Content` | `string` | No | Plain-text body of the chunk. |
| `TokenCount` | `int` | No | Estimated token count. Always > 0. |
| `SortOrder` | `int` | No | 0-based sequential index within the document. |
| `Strategy` | `ChunkStrategy` | No | `SectionLevel` or `FixedSize`. |

---

## Behaviour Contract

### Happy path — section-level chunking

1. `IngestAsync` called with a valid stream and non-empty `ActName`.
2. Service sets `IngestionJob.Status = Extracting`, records `ExtractStartedAt`.
3. Full text extracted from PDF; `ExtractedCharacterCount` and `ExtractCompletedAt` set.
4. Service sets `Status = Transforming`, records `TransformStartedAt`.
5. Regex chapter/section detection runs. If **≥3 sections detected**:
   - Each section becomes one `DocumentChunkResult` (Strategy = `SectionLevel`).
   - Sections >800 tokens are split at subsection markers `(N)`.
6. `ChunksProduced` and `TransformCompletedAt` set.
7. Service sets `Status = Loading`, records `LoadStartedAt`.
8. Returns `IReadOnlyList<DocumentChunkResult>` to caller. Caller persists chunks.
   - Note: the service does **not** persist chunks itself; it signals readiness via the return value.
9. Caller updates `ChunksLoaded`, `LoadCompletedAt`, and `Status = Completed` on the `IngestionJob`.

### Fallback path — fixed-size chunking

Steps 1–4 same as above. If **<3 sections detected**:
- Text split into 500-token (≈2000 char) windows with 50-token (≈200 char) overlap.
- Each window becomes one `DocumentChunkResult` (Strategy = `FixedSize`).
- `ChapterTitle`, `SectionNumber`, `SectionTitle` are all null.

### Empty / unreadable PDF

- If extracted text is empty (0 characters), `ExtractedCharacterCount = 0` and service returns an empty list.
- `Status` remains at the last reached stage (caller must detect empty result and mark job accordingly).
- No exception is thrown.

### Error path

- If PdfPig throws during extraction (corrupt PDF, stream error), the service catches the exception, sets `IngestionJob.Status = Failed`, sets `ErrorMessage` to a description of the failure, and re-throws so the caller can log the error.

---

## Constants Used Internally

| Constant | Value | Meaning |
|---|---|---|
| `MinSectionsForAuto` | 3 | Minimum detected sections to use section-level strategy |
| `MaxTokensPerChunk` | 800 | Token threshold above which a section is split by subsections |
| `FixedChunkTokens` | 500 | Window size for fixed-size fallback |
| `OverlapTokens` | 50 | Overlap between adjacent fixed-size chunks |
| `CharsPerTokenEstimate` | 4 | Characters-per-token used for all token approximations |

---

## Caller Responsibilities

The caller of `IngestAsync` is responsible for:

1. Creating the `IngestionJob` entity (Status = `Queued`) before calling `IngestAsync`.
2. Creating the `LegalDocument` and obtaining its `Id` before calling `IngestAsync`.
3. Mapping each `DocumentChunkResult` to a `DocumentChunk` entity and saving to the database.
4. Updating the `IngestionJob` fields `ChunksLoaded`, `LoadCompletedAt`, and `Status = Completed` after persistence.
5. Updating `LegalDocument.IsProcessed = true` and `LegalDocument.TotalChunks` after successful persistence.
6. Handling the case of an empty result list (deciding whether to mark the job as Failed or Completed).

---

## Not Included in This Contract

- **Embedding generation**: Handled by a separate service (future feature). `ChunkEmbedding` rows are not created by `PdfIngestionAppService`.
- **PDF upload / file storage**: Handled by `LegalDocumentAppService`.
- **Admin HTTP endpoint**: If needed, an admin controller will call `IPdfIngestionAppService` — scaffolded separately via `add-endpoint` skill.
