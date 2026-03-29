namespace backend.Services.ChunkEnrichmentService.DTO;

/// <summary>
/// Result of LLM-based keyword and topic extraction for a document chunk.
/// Returns safe fallback values when enrichment fails — never null.
/// </summary>
public class ChunkEnrichmentResult
{
    /// <summary>
    /// Comma-separated legal keywords extracted from the chunk content
    /// (e.g., "employment,termination,fair dismissal").
    /// Empty string when enrichment failed or returned no keywords.
    /// </summary>
    public string Keywords { get; init; } = string.Empty;

    /// <summary>
    /// Topic classification label assigned to the chunk
    /// (e.g., "Labour Relations", "Constitutional Rights").
    /// "Unknown" when enrichment failed or returned no topic.
    /// </summary>
    public string TopicClassification { get; init; } = "Unknown";

    /// <summary>Returns a fallback result used when enrichment fails non-fatally.</summary>
    public static ChunkEnrichmentResult Fallback() =>
        new() { Keywords = string.Empty, TopicClassification = "Unknown" };
}
