using backend.Services.RagService;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

namespace backend.Tests.RagServiceTests;

/// <summary>
/// Unit tests for <see cref="RagPromptBuilder"/> static helper.
/// All tests are pure — no I/O, no database, no network.
/// </summary>
public class RagPromptBuilderTests
{
    // ── BuildSystemPrompt ─────────────────────────────────────────────────────

    [Fact]
    public void BuildSystemPrompt_ReturnsNonEmptyString()
    {
        var result = RagPromptBuilder.BuildSystemPrompt();
        result.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void BuildSystemPrompt_ContainsOnlyAnswerInstruction()
    {
        // The system prompt must instruct the LLM to only use provided context.
        var result = RagPromptBuilder.BuildSystemPrompt();
        result.ShouldContain("ONLY");
        result.ShouldContain("answer");
    }

    [Fact]
    public void BuildSystemPrompt_ContainsCitationInstruction()
    {
        var result = RagPromptBuilder.BuildSystemPrompt();
        result.ShouldContain("citation");
    }

    // ── BuildContextBlock ─────────────────────────────────────────────────────

    [Fact]
    public void BuildContextBlock_SingleChunk_IncludesActNameAndSectionNumber()
    {
        var chunk = new RagPromptBuilder.ScoredChunk(
            ChunkId: Guid.NewGuid(),
            ActName: "Constitution of the Republic of South Africa",
            SectionNumber: "§ 26(3)",
            Excerpt: "No one may be evicted from their home without a court order.",
            Score: 0.91f,
            Vector: Array.Empty<float>());

        var result = RagPromptBuilder.BuildContextBlock(new[] { chunk });

        result.ShouldContain("Constitution of the Republic of South Africa");
        result.ShouldContain("§ 26(3)");
        result.ShouldContain("No one may be evicted");
    }

    [Fact]
    public void BuildContextBlock_SingleChunk_UsesExpectedLabelFormat()
    {
        // Label format must be: [ActName — SectionNumber]
        var chunk = new RagPromptBuilder.ScoredChunk(
            ChunkId: Guid.NewGuid(),
            ActName: "Labour Relations Act",
            SectionNumber: "§ 187",
            Excerpt: "Automatically unfair dismissals.",
            Score: 0.85f,
            Vector: Array.Empty<float>());

        var result = RagPromptBuilder.BuildContextBlock(new[] { chunk });

        result.ShouldContain("[Labour Relations Act — § 187]");
    }

    [Fact]
    public void BuildContextBlock_MultipleChunks_IncludesAllChunks()
    {
        var chunks = new List<RagPromptBuilder.ScoredChunk>
        {
            new(Guid.NewGuid(), "Act A", "§ 1", "Content A", 0.9f, Array.Empty<float>()),
            new(Guid.NewGuid(), "Act B", "§ 2", "Content B", 0.8f, Array.Empty<float>())
        };

        var result = RagPromptBuilder.BuildContextBlock(chunks);

        result.ShouldContain("Act A");
        result.ShouldContain("§ 1");
        result.ShouldContain("Content A");
        result.ShouldContain("Act B");
        result.ShouldContain("§ 2");
        result.ShouldContain("Content B");
    }

    // ── BuildUserPrompt ───────────────────────────────────────────────────────

    [Fact]
    public void BuildUserPrompt_IncludesQuestionText()
    {
        const string question = "Can my landlord evict me?";
        var result = RagPromptBuilder.BuildUserPrompt(question, "some context");
        result.ShouldContain(question);
    }

    [Fact]
    public void BuildUserPrompt_IncludesContextBlock()
    {
        const string context = "[Act — § 1]\nRelevant text here.";
        var result = RagPromptBuilder.BuildUserPrompt("Any question?", context);
        result.ShouldContain(context);
    }

    [Fact]
    public void BuildUserPrompt_NullOrWhitespaceQuestion_Throws()
    {
        Should.Throw<Exception>(() => RagPromptBuilder.BuildUserPrompt(null, "ctx"));
        Should.Throw<Exception>(() => RagPromptBuilder.BuildUserPrompt("   ", "ctx"));
    }

    // ── Constants ─────────────────────────────────────────────────────────────

    [Fact]
    public void SimilarityThreshold_Is0Point7()
    {
        RagPromptBuilder.SimilarityThreshold.ShouldBe(0.7f);
    }

    [Fact]
    public void MaxContextChunks_Is5()
    {
        RagPromptBuilder.MaxContextChunks.ShouldBe(5);
    }
}
