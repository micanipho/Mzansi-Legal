using Ardalis.GuardClauses;
using backend.Services.RagService.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services.RagService;

public sealed class RagConfidenceEvaluator
{
    private static readonly HashSet<string> BindingLawExpectationTerms = new(StringComparer.Ordinal)
    {
        "act", "acts", "law", "laws", "legal", "legislation", "regulation", "regulations", "section", "sections", "statute", "statutory"
    };

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
        var normalizedQuestion = RagSourceHintExtractor.Normalize(translatedQuestionText);
        var questionTerms = RagSourceHintExtractor.TokenizeNormalized(normalizedQuestion);
        var primary = retrievalDecision.RankedDocuments.FirstOrDefault();
        var runnerUp = retrievalDecision.RankedDocuments.Skip(1).FirstOrDefault();
        var primaryAuthorityType = primary?.AuthorityType ?? RagSourceMetadata.BindingLaw;
        var hasBindingLawSupport = retrievalDecision.SelectedChunks.Any(chunk =>
            chunk.AuthorityType == RagSourceMetadata.BindingLaw);
        var hasGuidanceSupport = retrievalDecision.SelectedChunks.Any(chunk =>
            chunk.AuthorityType == RagSourceMetadata.OfficialGuidance);
        var guidanceOnlySupport = hasGuidanceSupport && !hasBindingLawSupport;
        var userExplicitlyAskedForLaw = questionTerms.Any(BindingLawExpectationTerms.Contains);
        var hasCompetingControllingSource = HasCompetingControllingSource(primary, retrievalDecision.RankedDocuments);

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
        var supportsDirectLawAnswer =
            primaryAuthorityType == RagSourceMetadata.BindingLaw &&
            !hasCompetingControllingSource;
        var supportsGuidanceOnlyAnswer =
            guidanceOnlySupport &&
            primary.FinalDocumentScore >= 0.70f &&
            strongChunkCount >= 1 &&
            !userExplicitlyAskedForLaw &&
            !hasCompetingControllingSource &&
            !retrievalDecision.IsAmbiguousQuestion;

        if (primary.FinalDocumentScore >= 0.76f &&
            strongChunkCount >= 2 &&
            scoreGap >= 0.06f &&
            !retrievalDecision.IsAmbiguousQuestion &&
            supportsDirectLawAnswer)
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

        if (guidanceOnlySupport && userExplicitlyAskedForLaw)
        {
            return retrievalDecision with
            {
                AnswerMode = RagAnswerMode.Clarification,
                ConfidenceBand = RagConfidenceBand.Low,
                RequiresUrgentAttention = requiresUrgentAttention
            };
        }

        if (supportsGuidanceOnlyAnswer)
        {
            return retrievalDecision with
            {
                AnswerMode = RagAnswerMode.Cautious,
                ConfidenceBand = RagConfidenceBand.Medium,
                RequiresUrgentAttention = requiresUrgentAttention
            };
        }

        if (primary.FinalDocumentScore >= 0.56f &&
            (strongChunkCount >= 1 || hasBroadDocumentSupport) &&
            !retrievalDecision.IsAmbiguousQuestion &&
            !hasCompetingControllingSource)
        {
            return retrievalDecision with
            {
                AnswerMode = RagAnswerMode.Cautious,
                ConfidenceBand = RagConfidenceBand.Medium,
                RequiresUrgentAttention = requiresUrgentAttention
            };
        }

        if (guidanceOnlySupport || hasCompetingControllingSource || primary.FinalDocumentScore >= 0.46f || hasBroadDocumentSupport)
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

    private static bool HasCompetingControllingSource(
        DocumentCandidate primary,
        IReadOnlyList<DocumentCandidate> rankedDocuments)
    {
        if (primary is null || rankedDocuments.Count < 2)
        {
            return false;
        }

        return rankedDocuments
            .Skip(1)
            .Any(candidate =>
                !string.Equals(candidate.SourceFamily, primary.SourceFamily, StringComparison.OrdinalIgnoreCase) &&
                candidate.FinalDocumentScore >= primary.FinalDocumentScore - 0.035f &&
                candidate.DocumentSemanticScore >= 0.55f &&
                candidate.MetadataAlignmentScore >= 0.22f);
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
