using Ardalis.GuardClauses;
using backend.Services.RagService.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services.RagService;

public sealed class RagConfidenceEvaluator
{
    private static readonly HashSet<string> TemporalUrgencyTerms = new(StringComparer.Ordinal)
    {
        "asap", "deadline", "immediately", "now", "today", "tomorrow", "tonight", "urgent"
    };

    private static readonly HashSet<string> EnforcementTerms = new(StringComparer.Ordinal)
    {
        "arrest", "arrested", "court", "deadline", "evict", "eviction", "hearing", "lockout", "police", "sheriff"
    };

    private static readonly string[] UrgentRiskPhrases =
    {
        "changed the locks",
        "cut off my electricity",
        "cut off my power",
        "cut off my water",
        "hearing tomorrow",
        "immediate danger",
        "locked me out",
        "locked out",
        "right now"
    };

    private static readonly HashSet<string> HarmTerms = new(StringComparer.Ordinal)
    {
        "abuse", "danger", "dangerous", "harassment", "police", "threat", "threatened", "unsafe", "violence", "violent"
    };

    public RetrievalDecision Evaluate(string translatedQuestionText, RetrievalDecision retrievalDecision)
    {
        Guard.Against.NullOrWhiteSpace(translatedQuestionText, nameof(translatedQuestionText));
        Guard.Against.Null(retrievalDecision, nameof(retrievalDecision));

        var requiresUrgentAttention = RequiresUrgentAttention(translatedQuestionText);
        var primary = retrievalDecision.RankedDocuments.FirstOrDefault();
        var runnerUp = retrievalDecision.RankedDocuments.Skip(1).FirstOrDefault();

        if (primary is null || retrievalDecision.SelectedChunks.Count == 0)
        {
            return retrievalDecision with
            {
                AnswerMode = RagAnswerMode.Insufficient,
                ConfidenceBand = RagConfidenceBand.Low,
                RequiresUrgentAttention = requiresUrgentAttention
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
            var directDecision = retrievalDecision with
            {
                AnswerMode = RagAnswerMode.Direct,
                ConfidenceBand = RagConfidenceBand.High,
                RequiresUrgentAttention = requiresUrgentAttention
            };

            return requiresUrgentAttention
                ? directDecision with
                {
                    AnswerMode = RagAnswerMode.Cautious,
                    ConfidenceBand = RagConfidenceBand.Medium
                }
                : directDecision;
        }

        if (primary.FinalDocumentScore >= 0.56f &&
            (strongChunkCount >= 1 || hasBroadDocumentSupport) &&
            !retrievalDecision.IsAmbiguousQuestion)
        {
            return retrievalDecision with
            {
                AnswerMode = RagAnswerMode.Cautious,
                ConfidenceBand = RagConfidenceBand.Medium,
                RequiresUrgentAttention = requiresUrgentAttention
            };
        }

        if (primary.FinalDocumentScore >= 0.46f || hasBroadDocumentSupport)
        {
            return retrievalDecision with
            {
                AnswerMode = RagAnswerMode.Clarification,
                ConfidenceBand = RagConfidenceBand.Low,
                RequiresUrgentAttention = requiresUrgentAttention
            };
        }

        return retrievalDecision with
        {
            AnswerMode = RagAnswerMode.Insufficient,
            ConfidenceBand = RagConfidenceBand.Low,
            RequiresUrgentAttention = requiresUrgentAttention
        };
    }

    private static bool RequiresUrgentAttention(string translatedQuestionText)
    {
        var normalizedQuestion = RagSourceHintExtractor.Normalize(translatedQuestionText);
        if (string.IsNullOrWhiteSpace(normalizedQuestion))
        {
            return false;
        }

        if (UrgentRiskPhrases.Any(normalizedQuestion.Contains))
        {
            return true;
        }

        var questionTerms = RagSourceHintExtractor.TokenizeNormalized(normalizedQuestion);
        var hasTemporalUrgency = questionTerms.Any(TemporalUrgencyTerms.Contains);
        var hasEnforcementSignal = questionTerms.Any(EnforcementTerms.Contains);
        var hasHarmSignal = questionTerms.Any(HarmTerms.Contains);

        return hasHarmSignal || (hasTemporalUrgency && hasEnforcementSignal);
    }
}
