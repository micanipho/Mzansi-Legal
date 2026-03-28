using backend.Services.ChunkEnrichmentService.DTO;
using System.Threading.Tasks;

namespace backend.Services.ChunkEnrichmentService;

/// <summary>
/// Extracts semantic metadata (keywords and topic classification) from a document chunk
/// using a lightweight LLM call. Failures are non-fatal — the service returns safe fallback
/// values rather than throwing, so the ingestion pipeline is never blocked by enrichment errors.
/// </summary>
public interface IChunkEnrichmentAppService
{
    /// <summary>
    /// Calls OpenAI chat completions to extract 3–5 keywords and a topic classification
    /// from the provided chunk content. Content exceeding 3,000 characters is truncated.
    /// </summary>
    /// <param name="content">Plain-text body of the document chunk. Must not be null or whitespace.</param>
    /// <returns>
    /// A <see cref="ChunkEnrichmentResult"/> with Keywords and TopicClassification populated.
    /// Returns fallback values (empty Keywords, "Unknown" topic) on any failure.
    /// Never throws — all exceptions are caught and logged internally.
    /// </returns>
    Task<ChunkEnrichmentResult> EnrichAsync(string content);
}
