namespace backend.Domains.LegalDocuments;

/// <summary>
/// Identifies the chunking strategy used to produce a DocumentChunk during PDF ingestion.
/// SectionLevel: SA legislation regex detected ≥3 sections; chunks align to chapter/section boundaries.
/// FixedSize: Fewer than 3 sections detected; chunks are fixed-width sliding windows with overlap.
/// </summary>
public enum ChunkStrategy
{
    SectionLevel = 0,
    FixedSize    = 1
}
