using backend.Services.EmbeddingService;
using backend.Services.RagService;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace backend.Tests.RagServiceTests;

/// <summary>
/// Unit tests for RAG retrieval logic — similarity scoring, threshold filtering,
/// top-K selection, and the insufficient-information short-circuit.
/// These tests exercise the pure computation paths without hitting the database or network.
/// </summary>
public class RagAppServiceTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a scored chunk with a pre-assigned vector for use in scoring tests.
    /// The Score field is set to 0 here; tests compute real scores via EmbeddingHelper.
    /// </summary>
    private static RagPromptBuilder.ScoredChunk MakeChunk(float[] vector, string actName = "Test Act", string section = "§ 1")
        => new(Guid.NewGuid(), actName, section, "Test content.", 0f, vector);

    /// <summary>Scores a list of template chunks against a question vector, mimicking AskAsync logic.</summary>
    private static List<RagPromptBuilder.ScoredChunk> ScoreAndFilter(
        IEnumerable<RagPromptBuilder.ScoredChunk> loaded,
        float[] questionVector)
    {
        return loaded
            .Select(c => c with { Score = EmbeddingHelper.CosineSimilarity(questionVector, c.Vector) })
            .Where(c => c.Score >= RagPromptBuilder.SimilarityThreshold)
            .OrderByDescending(c => c.Score)
            .Take(RagPromptBuilder.MaxContextChunks)
            .ToList();
    }

    // ── Threshold filtering ───────────────────────────────────────────────────

    [Fact]
    public void ScoreAndFilter_ChunkBelowThreshold_IsExcluded()
    {
        // Orthogonal vectors → cosine similarity = 0 (below 0.7 threshold).
        var questionVector = new float[] { 1f, 0f, 0f };
        var chunkVector = new float[] { 0f, 1f, 0f };

        var loaded = new[] { MakeChunk(chunkVector) };
        var results = ScoreAndFilter(loaded, questionVector);

        results.ShouldBeEmpty();
    }

    [Fact]
    public void ScoreAndFilter_IdenticalVectors_ScoresOneAndIsIncluded()
    {
        // Identical vectors → cosine similarity = 1.0 (above threshold).
        var vector = new float[] { 1f, 0f, 0f };
        var loaded = new[] { MakeChunk(vector) };
        var results = ScoreAndFilter(loaded, vector);

        results.Count.ShouldBe(1);
        Math.Abs(results[0].Score - 1f).ShouldBeLessThan(0.001f);
    }

    [Fact]
    public void ScoreAndFilter_MixedChunks_OnlyAboveThresholdReturned()
    {
        var questionVector = new float[] { 1f, 0f };

        // This chunk is identical → score 1.0 (above threshold).
        var highChunk = MakeChunk(new float[] { 1f, 0f }, "High Act", "§ H");

        // This chunk is orthogonal → score 0.0 (below threshold).
        var lowChunk = MakeChunk(new float[] { 0f, 1f }, "Low Act", "§ L");

        var results = ScoreAndFilter(new[] { highChunk, lowChunk }, questionVector);

        results.Count.ShouldBe(1);
        results[0].ActName.ShouldBe("High Act");
    }

    // ── Top-K selection ───────────────────────────────────────────────────────

    [Fact]
    public void ScoreAndFilter_MoreThanMaxChunksQualify_ReturnsExactlyMaxContextChunks()
    {
        // All 7 chunks are identical to the question → all score 1.0.
        var vector = new float[] { 1f, 0f };
        var questionVector = new float[] { 1f, 0f };

        var loaded = Enumerable.Range(0, 7).Select(_ => MakeChunk(vector)).ToList();
        var results = ScoreAndFilter(loaded, questionVector);

        results.Count.ShouldBe(RagPromptBuilder.MaxContextChunks);
    }

    [Fact]
    public void ScoreAndFilter_ResultsOrderedByScoreDescending()
    {
        // High chunk scores closer to question than low chunk (both above threshold).
        var questionVector = new float[] { 1f, 0f };

        // Slightly off-axis but still above threshold (~0.89 cosine similarity).
        var mediumChunk = MakeChunk(new float[] { 2f, 1f }, "Medium Act", "§ M");

        // Identical → score 1.0.
        var highChunk = MakeChunk(new float[] { 1f, 0f }, "High Act", "§ H");

        var results = ScoreAndFilter(new[] { mediumChunk, highChunk }, questionVector);

        results.Count.ShouldBe(2);
        results[0].ActName.ShouldBe("High Act");    // highest score first
        results[1].ActName.ShouldBe("Medium Act");
    }

    // ── Insufficient information short-circuit ────────────────────────────────

    [Fact]
    public void ScoreAndFilter_NoChunksAboveThreshold_ReturnsEmptyList()
    {
        // Orthogonal → score 0.0; no chunk qualifies.
        var questionVector = new float[] { 1f, 0f, 0f };
        var loaded = new[]
        {
            MakeChunk(new float[] { 0f, 1f, 0f }),
            MakeChunk(new float[] { 0f, 0f, 1f })
        };

        var results = ScoreAndFilter(loaded, questionVector);

        // Empty result maps to IsInsufficientInformation = true in AskAsync.
        results.ShouldBeEmpty();
    }

    [Fact]
    public void ScoreAndFilter_EmptyLoadedChunks_ReturnsEmptyList()
    {
        var results = ScoreAndFilter(Array.Empty<RagPromptBuilder.ScoredChunk>(), new float[] { 1f });
        results.ShouldBeEmpty();
    }
}
