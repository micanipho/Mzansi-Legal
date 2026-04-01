using Ardalis.GuardClauses;
using System;

namespace backend.Services.EmbeddingService;

/// <summary>
/// Static helper utilities for the embedding pipeline.
/// Provides text truncation to respect model token limits and cosine similarity
/// calculation for semantic retrieval scoring.
/// </summary>
public static class EmbeddingHelper
{
    /// <summary>
    /// Truncates text to the specified character limit when it exceeds that limit.
    /// Returns the original string unchanged when it is within the limit.
    /// </summary>
    /// <param name="text">The input text to truncate. Must not be null or whitespace.</param>
    /// <param name="maxCharacters">Maximum character count to allow. Defaults to 30,000.</param>
    /// <returns>The original string, or the first <paramref name="maxCharacters"/> characters.</returns>
    public static string TruncateToLimit(string text, int maxCharacters = 30_000)
    {
        Guard.Against.NullOrWhiteSpace(text, nameof(text));
        Guard.Against.NegativeOrZero(maxCharacters, nameof(maxCharacters));

        if (text.Length <= maxCharacters)
            return text;

        return text[..maxCharacters];
    }

    /// <summary>
    /// Computes the cosine similarity between two float vectors of equal length.
    /// Returns a value in the range [-1.0, 1.0] where 1.0 indicates identical direction.
    /// Returns 0.0 when either vector has zero magnitude to avoid returning NaN.
    /// </summary>
    /// <param name="a">First vector. Must not be null and must have the same length as <paramref name="b"/>.</param>
    /// <param name="b">Second vector. Must not be null and must have the same length as <paramref name="a"/>.</param>
    /// <returns>Cosine similarity scalar in [-1.0, 1.0].</returns>
    /// <exception cref="ArgumentException">Thrown when vectors have different lengths.</exception>
    public static float CosineSimilarity(float[] a, float[] b)
    {
        Guard.Against.Null(a, nameof(a));
        Guard.Against.Null(b, nameof(b));

        if (a.Length != b.Length)
            throw new ArgumentException(
                $"Vectors must have equal length. Got {a.Length} and {b.Length}.",
                nameof(b));

        float dotProduct = 0f;
        float magnitudeA = 0f;
        float magnitudeB = 0f;

        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            magnitudeA += a[i] * a[i];
            magnitudeB += b[i] * b[i];
        }

        magnitudeA = MathF.Sqrt(magnitudeA);
        magnitudeB = MathF.Sqrt(magnitudeB);

        // Return 0 when either vector is the zero vector to avoid NaN in the result.
        if (magnitudeA == 0f || magnitudeB == 0f)
            return 0f;

        return dotProduct / (magnitudeA * magnitudeB);
    }
}
