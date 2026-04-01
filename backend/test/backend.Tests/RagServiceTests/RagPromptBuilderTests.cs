using backend.Domains.QA;
using backend.Services.RagService;
using backend.Services.RagService.DTO;
using Shouldly;
using System;
using Xunit;

namespace backend.Tests.RagServiceTests;

public class RagPromptBuilderTests
{
    [Fact]
    public void BuildSystemPrompt_ForDirectMode_ContainsCitationInstruction()
    {
        var result = RagPromptBuilder.BuildSystemPrompt(RagAnswerMode.Direct);

        result.ShouldContain("ONLY answer using information from the legislation context");
        result.ShouldContain("citation");
    }

    [Fact]
    public void BuildSystemPrompt_ForClarificationMode_AsksForSingleFollowUpQuestion()
    {
        var result = RagPromptBuilder.BuildSystemPrompt(RagAnswerMode.Clarification, Language.English);

        result.ShouldContain("Ask exactly one focused follow-up question");
        result.ShouldContain("do NOT provide a legal conclusion");
    }

    [Fact]
    public void BuildSystemPrompt_ForUrgentCautiousMode_ExplainsUrgentHelpAndGuidanceRules()
    {
        var result = RagPromptBuilder.BuildSystemPrompt(
            RagAnswerMode.Cautious,
            Language.English,
            requiresUrgentAttention: true);

        result.ShouldContain("binding law as controlling");
        result.ShouldContain("immediate-help note");
    }

    [Fact]
    public void BuildContextBlock_SingleChunk_IncludesActNameSectionAndSectionTitle()
    {
        var chunk = new RetrievedChunk(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Constitution of the Republic of South Africa",
            "Constitution",
            "Housing",
            "Section 26(3)",
            "Housing",
            "No one may be evicted from their home without a court order.",
            "Housing Rights",
            Array.Empty<string>(),
            0.91f,
            0.91f,
            42,
            "Constitution of the Republic of South Africa",
            "Section 26(3)",
            RagSourceMetadata.BindingLaw,
            RagSourceMetadata.Primary);

        var result = RagPromptBuilder.BuildContextBlock(new[] { chunk });

        result.ShouldContain("Constitution of the Republic of South Africa");
        result.ShouldContain("Section 26(3)");
        result.ShouldContain("Section title: Housing");
        result.ShouldContain("Authority: binding law");
        result.ShouldContain("Source role: primary");
        result.ShouldContain("No one may be evicted");
    }

    [Fact]
    public void BuildUserPrompt_ForClarificationMode_OnlyRequestsFollowUpQuestion()
    {
        var result = RagPromptBuilder.BuildUserPrompt(
            "Can they evict me?",
            "[Act - Section]\nContext",
            RagAnswerMode.Clarification,
            "Can you share whether this is about a rented home?");

        result.ShouldContain("Return only the follow-up question.");
        result.ShouldContain("Can you share whether this is about a rented home?");
    }

    [Fact]
    public void BuildInsufficientResponse_ForUrgentQuestion_IncludesImmediateHelpSentence()
    {
        var result = RagPromptBuilder.BuildInsufficientResponse(
            Language.English,
            requiresUrgentAttention: true);

        result.ShouldContain("legal grounding is too weak");
        result.ShouldContain("seek official or legal help right away");
    }

    [Fact]
    public void GetChatTemperature_UsesModeSpecificValues()
    {
        RagPromptBuilder.GetChatTemperature(RagAnswerMode.Direct).ShouldBe(0.2d);
        RagPromptBuilder.GetChatTemperature(RagAnswerMode.Cautious).ShouldBe(0.1d);
        RagPromptBuilder.GetChatTemperature(RagAnswerMode.Clarification).ShouldBe(0.0d);
        RagPromptBuilder.GetChatTemperature(RagAnswerMode.Insufficient).ShouldBe(0.0d);
    }

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
