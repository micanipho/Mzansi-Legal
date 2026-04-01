using backend.Services.EmbeddingService.DTO;
using System.Threading.Tasks;

namespace backend.Services.EmbeddingService;

/// <summary>
/// Generates semantic embedding vectors for text using the configured embedding model.
/// Intended for use by the ingestion pipeline to populate ChunkEmbedding.Vector
/// during the Loading stage.
/// </summary>
public interface IEmbeddingAppService
{
    /// <summary>
    /// Generates a 1,536-dimensional embedding vector for the provided text.
    /// Text exceeding 30,000 characters is silently truncated before the API call.
    /// </summary>
    /// <param name="text">The plain-text content to embed. Must not be null or whitespace.</param>
    /// <returns>
    /// An <see cref="EmbeddingResult"/> containing the float[1536] vector and diagnostic metadata.
    /// </returns>
    /// <exception cref="System.ArgumentException">Thrown when text is null or whitespace.</exception>
    /// <exception cref="System.Net.Http.HttpRequestException">Propagated on network or API failure.</exception>
    Task<EmbeddingResult> GenerateEmbeddingAsync(string text);
}
