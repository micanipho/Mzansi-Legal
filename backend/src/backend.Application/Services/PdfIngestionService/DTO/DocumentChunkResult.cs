using backend.Domains.LegalDocuments;

namespace backend.Services.PdfIngestionService.DTO;

/// <summary>
/// Represents an in-memory chunk produced by PdfIngestionAppService.
/// Not yet persisted — the caller maps each result to a DocumentChunk entity and saves it.
/// </summary>
public class DocumentChunkResult
{
    /// <summary>Verbatim copy of the ActName supplied in IngestPdfRequest.</summary>
    public string ActName { get; set; }

    /// <summary>
    /// Full chapter identifier including number (e.g., "Chapter 2 — Fundamental Rights").
    /// Null when no chapter boundary was detected before this section, or for fixed-size chunks.
    /// </summary>
    public string ChapterTitle { get; set; }

    /// <summary>
    /// Section number extracted from the section heading (e.g., "12", "12A").
    /// Null for fixed-size fallback chunks.
    /// </summary>
    public string SectionNumber { get; set; }

    /// <summary>
    /// Section heading text (e.g., "Freedom of expression").
    /// Null for fixed-size fallback chunks.
    /// </summary>
    public string SectionTitle { get; set; }

    /// <summary>Plain-text body of the chunk. Always non-empty for valid chunks.</summary>
    public string Content { get; set; }

    /// <summary>
    /// Estimated token count calculated as (Content.Length + 3) / 4.
    /// Always positive for non-empty Content.
    /// </summary>
    public int TokenCount { get; set; }

    /// <summary>0-based sequential position of this chunk within the document.</summary>
    public int SortOrder { get; set; }

    /// <summary>Strategy that produced this chunk: SectionLevel or FixedSize.</summary>
    public ChunkStrategy Strategy { get; set; }
}
