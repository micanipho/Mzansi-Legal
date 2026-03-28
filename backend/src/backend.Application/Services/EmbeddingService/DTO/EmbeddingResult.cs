namespace backend.Services.EmbeddingService.DTO;

/// <summary>
/// Represents the output of a single embedding API call.
/// Contains the vector, the model that produced it, and diagnostic metadata.
/// </summary>
public class EmbeddingResult
{
    /// <summary>The 1,536-element embedding vector returned by the model.</summary>
    public float[] Vector { get; init; }

    /// <summary>Model name echoed from the API response (e.g., "text-embedding-ada-002").</summary>
    public string Model { get; init; }

    /// <summary>Character count of the text after truncation, for diagnostics and billing estimates.</summary>
    public int InputCharacterCount { get; init; }
}
