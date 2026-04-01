using backend.Services.RagService;
using backend.Services.RagService.DTO;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace backend.Tests.RagServiceTests;

public class RagConfidenceEvaluatorTests
{
    private readonly RagConfidenceEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_HighConfidenceDocumentSet_ReturnsDirect()
    {
        var decision = CreateDecision(
            primaryScore: 0.84f,
            runnerUpScore: 0.70f,
            chunkScores: new[] { 0.89f, 0.82f },
            isAmbiguous: false);

        var result = _evaluator.Evaluate("Can my landlord evict me without a court order?", decision);

        result.AnswerMode.ShouldBe(RagAnswerMode.Direct);
        result.ConfidenceBand.ShouldBe(RagConfidenceBand.High);
    }

    [Fact]
    public void Evaluate_MixedButGroundedSupport_ReturnsCautious()
    {
        var decision = CreateDecision(
            primaryScore: 0.69f,
            runnerUpScore: 0.60f,
            chunkScores: new[] { 0.76f, 0.64f },
            isAmbiguous: false,
            documentSemanticScore: 0.72f,
            metadataAlignmentScore: 0.44f,
            semanticBreadthScore: 0.66f);

        var result = _evaluator.Evaluate("What can my landlord do if I am behind on rent?", decision);

        result.AnswerMode.ShouldBe(RagAnswerMode.Cautious);
        result.ConfidenceBand.ShouldBe(RagConfidenceBand.Medium);
    }

    [Fact]
    public void Evaluate_AmbiguousRequest_ReturnsClarification()
    {
        var decision = CreateDecision(
            primaryScore: 0.58f,
            runnerUpScore: 0.55f,
            chunkScores: new[] { 0.68f },
            isAmbiguous: true,
            documentSemanticScore: 0.54f,
            metadataAlignmentScore: 0.22f,
            semanticBreadthScore: 0.33f);

        var result = _evaluator.Evaluate("Can they evict me?", decision);

        result.AnswerMode.ShouldBe(RagAnswerMode.Clarification);
        result.ConfidenceBand.ShouldBe(RagConfidenceBand.Low);
    }

    [Fact]
    public void Evaluate_NoResponsibleGrounding_ReturnsInsufficient()
    {
        var decision = new RetrievalDecision
        {
            SelectedChunks = Array.Empty<RetrievedChunk>(),
            RankedDocuments = Array.Empty<DocumentCandidate>(),
            ClarificationQuestion = "Can you share more detail?"
        };

        var result = _evaluator.Evaluate("What are the rules for commercial drone corridors?", decision);

        result.AnswerMode.ShouldBe(RagAnswerMode.Insufficient);
        result.ConfidenceBand.ShouldBe(RagConfidenceBand.Low);
    }

    [Fact]
    public void Evaluate_BroadButDocumentGroundedSupport_ReturnsCautious()
    {
        var decision = CreateDecision(
            primaryScore: 0.58f,
            runnerUpScore: 0.41f,
            chunkScores: new[] { 0.63f, 0.57f, 0.54f },
            isAmbiguous: false,
            documentSemanticScore: 0.71f,
            metadataAlignmentScore: 0.52f,
            semanticBreadthScore: 0.80f);

        var result = _evaluator.Evaluate("What are my CCMA rights?", decision);

        result.AnswerMode.ShouldBe(RagAnswerMode.Cautious);
        result.ConfidenceBand.ShouldBe(RagConfidenceBand.Medium);
    }

    private static RetrievalDecision CreateDecision(
        float primaryScore,
        float runnerUpScore,
        IReadOnlyList<float> chunkScores,
        bool isAmbiguous,
        float documentSemanticScore = 0.70f,
        float metadataAlignmentScore = 0.40f,
        float semanticBreadthScore = 0.50f)
    {
        var primaryDocId = Guid.NewGuid();
        var runnerUpDocId = Guid.NewGuid();

        return new RetrievalDecision
        {
            SelectedChunks = chunkScores
                .Select(score => new RetrievedChunk(
                    Guid.NewGuid(),
                    primaryDocId,
                    "Constitution of the Republic of South Africa",
                    "Constitution",
                    "Housing",
                    "Section 26(3)",
                    "Housing",
                    "No one may be evicted without a court order.",
                    "Housing Rights",
                    Array.Empty<string>(),
                    score,
                    score,
                    32))
                .ToList(),
            RankedDocuments = new[]
            {
                new DocumentCandidate(
                    primaryDocId,
                    "Constitution of the Republic of South Africa",
                    "Constitution",
                    "Housing",
                    new List<SemanticChunkMatch>(),
                    primaryScore,
                    primaryScore,
                    documentSemanticScore,
                    metadataAlignmentScore,
                    semanticBreadthScore,
                    0f,
                    primaryScore),
                new DocumentCandidate(
                    runnerUpDocId,
                    "Rental Housing Act 50 of 1999",
                    "Rental Housing Act",
                    "Housing",
                    new List<SemanticChunkMatch>(),
                    runnerUpScore,
                    runnerUpScore,
                    0.55f,
                    0.25f,
                    0.25f,
                    0f,
                    runnerUpScore)
            },
            ClarificationQuestion = "Can you share whether this is about a rented home?",
            IsAmbiguousQuestion = isAmbiguous
        };
    }
}
