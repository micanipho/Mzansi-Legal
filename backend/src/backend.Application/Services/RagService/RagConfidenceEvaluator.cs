using Ardalis.GuardClauses;
using backend.Services.RagService.DTO;
using System.Linq;

namespace backend.Services.RagService;

public sealed class RagConfidenceEvaluator
{
    public RetrievalDecision Evaluate(string translatedQuestionText, RetrievalDecision retrievalDecision)
    {
        Guard.Against.NullOrWhiteSpace(translatedQuestionText, nameof(translatedQuestionText));
        Guard.Against.Null(retrievalDecision, nameof(retrievalDecision));

        var primary = retrievalDecision.RankedDocuments.FirstOrDefault();
        var runnerUp = retrievalDecision.RankedDocuments.Skip(1).FirstOrDefault();

        if (primary is null || retrievalDecision.SelectedChunks.Count == 0)
        {
            return retrievalDecision with
            {
                AnswerMode = RagAnswerMode.Insufficient,
                ConfidenceBand = RagConfidenceBand.Low
            };
        }

        var scoreGap = runnerUp is null
            ? primary.FinalDocumentScore
            : primary.FinalDocumentScore - runnerUp.FinalDocumentScore;

        var strongChunkCount = retrievalDecision.SelectedChunks.Count(
            chunk => chunk.SemanticScore >= RagPromptBuilder.SimilarityThreshold);
        var hasBroadDocumentSupport =
            primary.DocumentSemanticScore >= 0.60f &&
            primary.MetadataAlignmentScore >= 0.35f &&
            primary.SemanticBreadthScore >= 0.34f;

        if (primary.FinalDocumentScore >= 0.76f &&
            strongChunkCount >= 2 &&
            scoreGap >= 0.06f &&
            !retrievalDecision.IsAmbiguousQuestion)
        {
            return retrievalDecision with
            {
                AnswerMode = RagAnswerMode.Direct,
                ConfidenceBand = RagConfidenceBand.High
            };
        }

        if (primary.FinalDocumentScore >= 0.56f &&
            (strongChunkCount >= 1 || hasBroadDocumentSupport) &&
            !retrievalDecision.IsAmbiguousQuestion)
        {
            return retrievalDecision with
            {
                AnswerMode = RagAnswerMode.Cautious,
                ConfidenceBand = RagConfidenceBand.Medium
            };
        }

        if (primary.FinalDocumentScore >= 0.46f || hasBroadDocumentSupport)
        {
            return retrievalDecision with
            {
                AnswerMode = RagAnswerMode.Clarification,
                ConfidenceBand = RagConfidenceBand.Low
            };
        }

        return retrievalDecision with
        {
            AnswerMode = RagAnswerMode.Insufficient,
            ConfidenceBand = RagConfidenceBand.Low
        };
    }
}
