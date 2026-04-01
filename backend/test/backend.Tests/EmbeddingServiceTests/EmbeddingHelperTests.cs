using backend.Services.EmbeddingService;
using Shouldly;
using System;
using Xunit;

namespace backend.Tests.EmbeddingServiceTests;

/// <summary>
/// Unit tests for EmbeddingHelper static methods.
/// No external dependencies — all tests are pure computation.
/// </summary>
public class EmbeddingHelperTests
{
    // ── TruncateToLimit ───────────────────────────────────────────────────────

    [Fact]
    public void TruncateToLimit_TextUnderLimit_ReturnsUnchanged()
    {
        const string text = "Short legislation text";
        EmbeddingHelper.TruncateToLimit(text).ShouldBe(text);
    }

    [Fact]
    public void TruncateToLimit_TextAtExactLimit_ReturnsUnchanged()
    {
        var text = new string('a', 30_000);
        var result = EmbeddingHelper.TruncateToLimit(text);
        result.Length.ShouldBe(30_000);
        result.ShouldBe(text);
    }

    [Fact]
    public void TruncateToLimit_TextOverLimit_ReturnsTruncatedTo30000Chars()
    {
        var text = new string('a', 30_001);
        var result = EmbeddingHelper.TruncateToLimit(text);
        result.Length.ShouldBe(30_000);
    }

    [Fact]
    public void TruncateToLimit_TextOverLimit_PreservesLeadingContent()
    {
        // The first 30,000 characters must be intact after truncation.
        var prefix = new string('x', 30_000);
        var text = prefix + "OVERFLOW";
        EmbeddingHelper.TruncateToLimit(text).ShouldBe(prefix);
    }

    // ── CosineSimilarity ──────────────────────────────────────────────────────

    [Fact]
    public void CosineSimilarity_IdenticalUnitVectors_ReturnsOne()
    {
        var v = new float[] { 1f, 0f, 0f };
        var result = EmbeddingHelper.CosineSimilarity(v, v);
        Math.Abs(result - 1f).ShouldBeLessThan(0.001f);
    }

    [Fact]
    public void CosineSimilarity_IdenticalNonUnitVectors_ReturnsOne()
    {
        var v = new float[] { 3f, 4f, 0f };
        var result = EmbeddingHelper.CosineSimilarity(v, v);
        Math.Abs(result - 1f).ShouldBeLessThan(0.001f);
    }

    [Fact]
    public void CosineSimilarity_OrthogonalVectors_ReturnsZero()
    {
        var a = new float[] { 1f, 0f };
        var b = new float[] { 0f, 1f };
        var result = EmbeddingHelper.CosineSimilarity(a, b);
        Math.Abs(result).ShouldBeLessThan(0.001f);
    }

    [Fact]
    public void CosineSimilarity_OppositeVectors_ReturnsNegativeOne()
    {
        var a = new float[] { 1f, 0f };
        var b = new float[] { -1f, 0f };
        var result = EmbeddingHelper.CosineSimilarity(a, b);
        Math.Abs(result - (-1f)).ShouldBeLessThan(0.001f);
    }

    [Fact]
    public void CosineSimilarity_DifferentLengths_ThrowsArgumentException()
    {
        var a = new float[] { 1f, 2f };
        var b = new float[] { 1f };
        Should.Throw<ArgumentException>(() => EmbeddingHelper.CosineSimilarity(a, b));
    }

    [Fact]
    public void CosineSimilarity_NullFirstVector_Throws()
    {
        var b = new float[] { 1f };
        Should.Throw<Exception>(() => EmbeddingHelper.CosineSimilarity(null, b));
    }

    [Fact]
    public void CosineSimilarity_NullSecondVector_Throws()
    {
        var a = new float[] { 1f };
        Should.Throw<Exception>(() => EmbeddingHelper.CosineSimilarity(a, null));
    }

    [Fact]
    public void CosineSimilarity_ZeroVector_ReturnsZeroWithoutNaN()
    {
        // A zero magnitude vector must return 0, not NaN.
        var zero = new float[] { 0f, 0f, 0f };
        var v = new float[] { 1f, 0f, 0f };
        var result = EmbeddingHelper.CosineSimilarity(zero, v);
        result.ShouldBe(0f);
        float.IsNaN(result).ShouldBeFalse();
    }
}
