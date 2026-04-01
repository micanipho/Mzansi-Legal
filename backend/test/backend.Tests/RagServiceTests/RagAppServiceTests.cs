using backend.Domains.QA;
using backend.Services.RagService;
using backend.Services.RagService.DTO;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace backend.Tests.RagServiceTests;

public class RagAppServiceTests
{
    [Fact]
    public void ShouldPersistAnswer_OnlyGroundedModesReturnTrue()
    {
        RagAppService.ShouldPersistAnswer(RagAnswerMode.Direct).ShouldBeTrue();
        RagAppService.ShouldPersistAnswer(RagAnswerMode.Cautious).ShouldBeTrue();
        RagAppService.ShouldPersistAnswer(RagAnswerMode.Clarification).ShouldBeFalse();
        RagAppService.ShouldPersistAnswer(RagAnswerMode.Insufficient).ShouldBeFalse();
    }

    [Fact]
    public void BuildNonGroundedResult_ClarificationMode_ReturnsClarificationQuestionWithoutPersistence()
    {
        const string clarificationQuestion = "Can you share whether this is about a rented home or a workplace issue?";

        var result = RagAppService.BuildNonGroundedResult(
            Language.English,
            "en",
            RagAnswerMode.Clarification,
            RagConfidenceBand.Low,
            clarificationQuestion);

        result.AnswerMode.ShouldBe(RagAnswerMode.Clarification);
        result.IsInsufficientInformation.ShouldBeTrue();
        result.AnswerId.ShouldBeNull();
        result.Citations.ShouldBeEmpty();
        result.ChunkIds.ShouldBeEmpty();
        result.ClarificationQuestion.ShouldBe(clarificationQuestion);
        result.AnswerText.ShouldContain("need one detail first");
    }

    [Fact]
    public void BuildNonGroundedResult_InsufficientMode_RemovesGeneralKnowledgeFallbackBehavior()
    {
        var result = RagAppService.BuildNonGroundedResult(
            Language.English,
            "en",
            RagAnswerMode.Insufficient,
            RagConfidenceBand.Low,
            null);

        result.AnswerMode.ShouldBe(RagAnswerMode.Insufficient);
        result.IsInsufficientInformation.ShouldBeTrue();
        result.AnswerText.ShouldContain("can't answer this responsibly");
        result.AnswerText.ShouldContain("legal grounding");
        result.AnswerText.ShouldNotContain("general AI knowledge");
        result.Citations.ShouldBeEmpty();
        result.ChunkIds.ShouldBeEmpty();
        result.AnswerId.ShouldBeNull();
        result.ClarificationQuestion.ShouldBeNull();
    }

    [Fact]
    public void CreateCitations_MultiSourceChunks_ReturnsOneCitationPerSourceChunk()
    {
        var citations = RagAppService.CreateCitations(new[]
        {
            new RetrievedChunk(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Constitution of the Republic of South Africa",
                "Constitution",
                "Housing",
                "Section 26(3)",
                "Housing",
                "No one may be evicted without a court order.",
                "Housing Rights",
                Array.Empty<string>(),
                0.92f,
                0.92f,
                30),
            new RetrievedChunk(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Rental Housing Act 50 of 1999",
                "Rental Housing Act",
                "Housing",
                "Section 4",
                "Rental agreements",
                "A landlord and tenant are bound by the rental agreement.",
                "Rental Housing",
                Array.Empty<string>(),
                0.81f,
                0.81f,
                28)
        });

        citations.Count.ShouldBe(2);
        citations.Select(citation => citation.ActName).ShouldContain("Constitution of the Republic of South Africa");
        citations.Select(citation => citation.ActName).ShouldContain("Rental Housing Act 50 of 1999");
    }
}
