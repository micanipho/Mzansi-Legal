# Research: PDF Section Chunking Ingestion Service

**Feature**: 008-pdf-section-chunking
**Phase**: 0 — Research & Unknowns Resolution
**Date**: 2026-03-28

---

## 1. PDF Text Extraction — PdfPig API

**Decision**: Use `UglyToad.PdfPig` via `document.GetPage(i).Text` for full-page text concatenation.

**Rationale**: PdfPig is open-source (Apache 2.0), actively maintained, and produces superior text output from government-formatted PDFs compared to iTextSharp. The `page.Text` property returns words in reading order (left-to-right, top-to-bottom) which is correct for continuous legislation text. For SA government PDFs that use multi-column layouts in exceptional cases, `page.GetWords()` with positional filtering is available as a future refinement.

**Key API**:
```csharp
using UglyToad.PdfPig;

using var document = PdfDocument.Open(stream);
var sb = new StringBuilder();
foreach (var page in document.GetPages())
{
    sb.AppendLine(page.Text);
}
var fullText = sb.ToString();
```

**Alternatives considered**:
- iTextSharp (LGPL/commercial): More widely known but produces noisier output from government PDF formatting; license complications for open-source use.
- PdfSharpCore: Limited text extraction capabilities for complex PDFs.
- Tika.NET: Heavy dependency, requires Java runtime.

---

## 2. SA Legislation Regex Patterns

**Decision**: Three-tier regex hierarchy: Chapter → Section → Subsection.

**Rationale**: SA legislation follows a consistent drafting style defined by the SA Law Reform Commission. Chapter and section headers appear on their own lines. Section numbering uses Arabic numerals with optional letter suffixes (e.g., `12A`).

**Patterns**:

```csharp
// Chapter boundary — matches "Chapter 2 — Title" or "CHAPTER 2 - Title" or "Chapter II — Title"
private static readonly Regex ChapterPattern = new(
    @"^Chapter\s+(\d+|[IVX]+)\s*[\u2014\-\u2013]+\s*(.+)$",
    RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

// Section boundary — matches "12. Title" or "12A. Title" or "Section 12. Title"
private static readonly Regex SectionPattern = new(
    @"^(?:Section\s+)?(\d+[A-Z]?)\.\s+(.+)$",
    RegexOptions.Multiline | RegexOptions.Compiled);

// Subsection marker — matches "(1)" or "(2)" at the start of a line/paragraph
private static readonly Regex SubsectionPattern = new(
    @"(?<=\n|^)\((\d+)\)\s+",
    RegexOptions.Multiline | RegexOptions.Compiled);
```

**Fallback threshold**: If fewer than 3 section matches are found across the entire text, use fixed-size chunking. This avoids treating a table of contents (which may match section patterns) as sufficient evidence of parseable structure.

**Alternatives considered**:
- Purely position-based splitting: Ignores semantic boundaries, produces poor retrieval quality.
- External NLP library for boundary detection: Adds heavy dependency; regex is sufficient for structured legislation.

---

## 3. Token Count Estimation

**Decision**: Character-division approximation: `tokenCount = (text.Length + 3) / 4`

**Rationale**: OpenAI's `cl100k_base` tokenizer (used by GPT-3.5/4 and Ada-002 embeddings) averages approximately 4 characters per token for English legal prose. The `+3` ensures rounding up rather than truncation. This approach requires no NuGet dependency and produces estimates accurate to within ±15% for typical legal text.

**Named constant**:
```csharp
private const int CharsPerTokenEstimate = 4;
```

**Used for**:
- Deciding whether a section exceeds 800 tokens → `(content.Length + 3) / 4 > MaxTokensPerChunk`
- Sliding window position in fixed-size chunking: `windowChars = MaxTokensPerChunk * CharsPerTokenEstimate`
- Reporting `TokenCount` on each returned `DocumentChunkResult`

**Alternatives considered**:
- SharpToken (C# port of tiktoken): More accurate, but adds a dependency and is unnecessary given the ±15% tolerance is sufficient for chunking decisions.
- Word-count method (`words * 1.33`): Slightly less accurate than char-based for legal text with long Latin terms.

---

## 4. Fixed-Size Chunking — Sliding Window

**Decision**: 500-token window with 50-token overlap, computed in character space.

**Parameters as named constants**:
```csharp
private const int FixedChunkTokens   = 500;
private const int OverlapTokens      = 50;
private const int MaxSectionTokens   = 800;
private const int MinSectionsForAuto = 3;
```

**Character equivalents**:
- Window size: `500 × 4 = 2000` characters
- Step size: `(500 - 50) × 4 = 1800` characters

**Algorithm**: Slide through `fullText` in steps of `stepChars`, taking `windowChars` characters per chunk. Each chunk carries the document's `ActName` and a sequential `SortOrder`.

**Alternatives considered**:
- Sentence-boundary splitting within windows: Better semantic coherence but requires NLP dependency.
- Paragraph splitting: Some legislation PDFs lack clean paragraph breaks; regex is more reliable.

---

## 5. IngestionJob State Machine

**Decision**: Linear pipeline with terminal failure at any stage.

**States**:
```
Queued → Extracting → Transforming → Loading → Completed
               ↓             ↓           ↓
            Failed         Failed      Failed
```

**Per-stage fields tracked on `IngestionJob`**:

| Field | Purpose |
|---|---|
| `Status` (IngestionStatus enum) | Current pipeline stage |
| `ExtractStartedAt` / `ExtractCompletedAt` | Text extraction timing |
| `TransformStartedAt` / `TransformCompletedAt` | Section parsing + chunking timing |
| `LoadStartedAt` / `LoadCompletedAt` | Chunk persistence timing |
| `ExtractedCharacterCount` | Volume signal after extraction |
| `ChunksProduced` | Count of chunks returned by Transform |
| `ChunksLoaded` | Count of chunks successfully persisted |
| `ErrorMessage` | Non-null when Status = Failed; describes failure stage |

**Rationale**: Constitution Principle V mandates IngestionJob tracks each stage (Queued → Extracting → Transforming → Loading → Completed/Failed) with duration, chunk counts, and error details. The admin dashboard needs live stage status. Each of the three timed stages maps directly to one method call in `PdfIngestionAppService`.

**Alternatives considered**:
- Single status + single timestamp: Insufficient for admin observability (no per-stage timing).
- Event sourcing per stage: Adds domain complexity beyond current requirements.

---

## 6. Service Architecture Decision

**Decision**: Single `PdfIngestionAppService` in `backend.Application` with four focused private methods.

**Method decomposition**:
1. `ExtractTextAsync(Stream pdfStream)` → `string fullText`
2. `DetectSections(string fullText)` → `IReadOnlyList<DetectedSection>`
3. `BuildSectionChunks(IReadOnlyList<DetectedSection> sections, string actName)` → `List<DocumentChunkResult>`
4. `BuildFixedSizeChunks(string fullText, string actName)` → `List<DocumentChunkResult>`
5. `SplitLargeSectionBySubsections(DetectedSection section, string actName, int sortOrderBase)` → `List<DocumentChunkResult>` (called from #3 when token count > 800)

**Rationale**: Keeps the class well under 350 lines; each method has a single clear responsibility; no layer violations (PdfPig is only referenced in `ExtractTextAsync`). Domain logic (chunking decisions) stays in the Application service since there is no reuse requirement in the domain layer for this feature.

**Interface**: `IPdfIngestionAppService` exposes one public method: `IngestAsync(IngestPdfRequest request)`.

---

## 7. DocumentChunk Entity Extension

**Decision**: Add `ChunkStrategy` (enum) property to the existing `DocumentChunk` entity. No separate `ChapterNumber` field — the existing `ChapterTitle` property already stores the full chapter identifier (e.g., "Chapter 2 — Fundamental Rights") as required.

**Migration**: `AddPdfIngestionEntities` adds:
- `ChunkStrategy` column on `DocumentChunks` (nullable, default `SectionLevel`)
- New `IngestionJobs` table
- Index on `IngestionJobs(DocumentId)` for admin dashboard queries
- Index on `IngestionJobs(Status)` for filtering active jobs

**Alternatives considered**:
- Adding `ChapterNumber` as a separate column: The existing `ChapterTitle` already contains the number; splitting would require a data migration for any future existing data.
