using Ardalis.GuardClauses;
using backend.Services.EmbeddingService;
using backend.Services.RagService.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services.RagService;

public sealed class RagRetrievalPlanner
{
    private static readonly HashSet<string> GenericQuestionTerms = new(StringComparer.Ordinal)
    {
        "help", "issue", "issues", "law", "legal", "matter", "problem", "rights", "situation", "thing", "things"
    };

    public IReadOnlyList<SemanticChunkMatch> BuildSemanticMatches(
        float[] questionVector,
        IEnumerable<IndexedChunk> indexedChunks,
        float[] focusVector = null)
    {
        Guard.Against.Null(questionVector, nameof(questionVector));
        Guard.Against.Null(indexedChunks, nameof(indexedChunks));

        return indexedChunks
            .Select(chunk => new SemanticChunkMatch(
                chunk,
                CalculateSemanticScore(questionVector, focusVector, chunk.Vector),
                0f,
                0f))
            .OrderByDescending(match => match.SemanticScore)
            .ToList();
    }

    public RetrievalDecision BuildPlan(
        string translatedQuestionText,
        float[] questionVector,
        IEnumerable<SemanticChunkMatch> semanticMatches,
        IReadOnlyList<SourceHint> hints,
        IReadOnlyList<DocumentProfile> documentProfiles)
    {
        Guard.Against.NullOrWhiteSpace(translatedQuestionText, nameof(translatedQuestionText));
        Guard.Against.Null(questionVector, nameof(questionVector));
        Guard.Against.Null(semanticMatches, nameof(semanticMatches));
        Guard.Against.Null(hints, nameof(hints));
        Guard.Against.Null(documentProfiles, nameof(documentProfiles));

        var normalizedQuestion = RagSourceHintExtractor.Normalize(translatedQuestionText);
        var questionTerms = SplitTerms(normalizedQuestion);
        var questionTermSpecificity = BuildQuestionTermSpecificity(questionTerms, documentProfiles);

        var alignedMatches = semanticMatches
            .Select(match => match with
            {
                KeywordAlignmentScore = CalculateKeywordAlignment(match.Chunk, questionTerms, questionTermSpecificity),
                MetadataAlignmentScore = CalculateChunkMetadataAlignment(
                    match.Chunk,
                    questionTerms,
                    normalizedQuestion,
                    questionTermSpecificity)
            })
            .ToList();

        var matchesByDocument = alignedMatches
            .GroupBy(match => match.Chunk.DocumentId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<SemanticChunkMatch>)group
                    .OrderByDescending(GetChunkRankingScore)
                    .ToList());

        var rankedDocuments = documentProfiles
            .Select(profile =>
                BuildDocumentCandidate(
                    profile,
                    matchesByDocument.TryGetValue(profile.DocumentId, out var documentMatches)
                        ? documentMatches
                        : Array.Empty<SemanticChunkMatch>(),
                    hints,
                    questionTerms,
                    normalizedQuestion,
                    questionVector,
                    questionTermSpecificity))
            .Where(candidate =>
                candidate.FinalDocumentScore >= 0.24f ||
                candidate.HintBoostScore > 0f ||
                candidate.MetadataAlignmentScore >= 0.35f)
            .OrderByDescending(candidate => candidate.FinalDocumentScore)
            .ThenByDescending(candidate => candidate.DocumentSemanticScore)
            .ToList();

        if (rankedDocuments.Count == 0)
        {
            return new RetrievalDecision
            {
                ClarificationQuestion = BuildClarificationQuestion(Array.Empty<DocumentCandidate>(), questionTerms)
            };
        }

        var primaryDocument = rankedDocuments.FirstOrDefault();
        var supportingDocuments = SelectSupportingDocuments(primaryDocument, rankedDocuments.Skip(1).ToList());
        var selectedChunks = SelectChunks(primaryDocument, supportingDocuments);
        var isAmbiguousQuestion = IsAmbiguousQuestion(questionTerms, rankedDocuments);

        return new RetrievalDecision
        {
            SelectedChunks = selectedChunks,
            PrimaryDocumentId = primaryDocument?.DocumentId,
            SupportingDocumentIds = supportingDocuments.Select(candidate => candidate.DocumentId).ToList(),
            RankedDocuments = rankedDocuments,
            ClarificationQuestion = BuildClarificationQuestion(rankedDocuments, questionTerms),
            IsAmbiguousQuestion = isAmbiguousQuestion
        };
    }

    private static DocumentCandidate BuildDocumentCandidate(
        DocumentProfile profile,
        IReadOnlyList<SemanticChunkMatch> matches,
        IReadOnlyList<SourceHint> hints,
        IReadOnlyCollection<string> questionTerms,
        string normalizedQuestion,
        float[] questionVector,
        IReadOnlyDictionary<string, float> questionTermSpecificity)
    {
        var topRelevantMatches = matches
            .Where(match =>
                match.SemanticScore >= Math.Max(RagPromptBuilder.SemanticCandidateFloor - 0.10f, 0.30f) ||
                match.MetadataAlignmentScore >= 0.35f)
            .OrderByDescending(GetChunkRankingScore)
            .Take(4)
            .ToList();

        if (topRelevantMatches.Count == 0)
        {
            topRelevantMatches = matches
                .OrderByDescending(GetChunkRankingScore)
                .Take(2)
                .ToList();
        }

        var maxChunkSemantic = topRelevantMatches.Count == 0
            ? 0f
            : topRelevantMatches.Max(match => match.SemanticScore);

        var meanTop = topRelevantMatches.Count == 0
            ? 0f
            : (float)topRelevantMatches
                .Take(3)
                .Average(match => match.SemanticScore);

        var keywordAlignment = topRelevantMatches.Count == 0
            ? 0f
            : (float)topRelevantMatches
                .Take(3)
                .Average(match => match.KeywordAlignmentScore);

        var documentSemantic = profile.Vector.Length == questionVector.Length && questionVector.Length > 0
            ? EmbeddingHelper.CosineSimilarity(questionVector, profile.Vector)
            : 0f;

        var metadataAlignment = CalculateProfileAlignment(
            profile,
            questionTerms,
            normalizedQuestion,
            questionTermSpecificity);
        var semanticBreadth = CalculateSemanticBreadth(matches);
        var hintBoost = hints
            .Where(hint => hint.MatchedDocumentId == profile.DocumentId ||
                           string.Equals(hint.MatchedCategoryName, profile.CategoryName, StringComparison.OrdinalIgnoreCase))
            .Sum(hint => hint.BoostWeight);

        var finalScore = Math.Clamp(
            (documentSemantic * 0.22f) +
            (maxChunkSemantic * 0.18f) +
            (meanTop * 0.10f) +
            (metadataAlignment * 0.30f) +
            (keywordAlignment * 0.05f) +
            (semanticBreadth * 0.10f) +
            (float)Math.Min(hintBoost, 0.05d),
            0f,
            1f);

        return new DocumentCandidate(
            profile.DocumentId,
            profile.ActName,
            profile.ActShortName,
            profile.CategoryName,
            topRelevantMatches,
            maxChunkSemantic,
            meanTop,
            documentSemantic,
            metadataAlignment,
            semanticBreadth,
            (float)hintBoost,
            finalScore);
    }

    private static IReadOnlyList<DocumentCandidate> SelectSupportingDocuments(
        DocumentCandidate primaryDocument,
        IReadOnlyList<DocumentCandidate> alternatives)
    {
        if (primaryDocument is null)
        {
            return Array.Empty<DocumentCandidate>();
        }

        return alternatives
            .Where(candidate =>
                candidate.FinalDocumentScore >= primaryDocument.FinalDocumentScore - 0.16f &&
                (candidate.MaxChunkSemanticScore >= RagPromptBuilder.SupportingDocumentFloor ||
                 candidate.DocumentSemanticScore >= RagPromptBuilder.SupportingDocumentFloor - 0.05f ||
                 candidate.MetadataAlignmentScore >= 0.35f) &&
                !string.Equals(candidate.ActName, primaryDocument.ActName, StringComparison.OrdinalIgnoreCase))
            .Take(2)
            .ToList();
    }

    private static IReadOnlyList<RetrievedChunk> SelectChunks(
        DocumentCandidate primaryDocument,
        IReadOnlyList<DocumentCandidate> supportingDocuments)
    {
        if (primaryDocument is null)
        {
            return Array.Empty<RetrievedChunk>();
        }

        var chunks = new List<RetrievedChunk>();
        chunks.AddRange(primaryDocument.TopMatches
            .Take(RagPromptBuilder.MaxChunksPerDocument)
            .Select(ToRetrievedChunk));

        foreach (var supportingDocument in supportingDocuments)
        {
            var remainingSlots = RagPromptBuilder.MaxContextChunks - chunks.Count;
            if (remainingSlots <= 0)
            {
                break;
            }

            chunks.AddRange(supportingDocument.TopMatches
                .Take(Math.Min(1, remainingSlots))
                .Select(ToRetrievedChunk));
        }

        return chunks
            .Take(RagPromptBuilder.MaxContextChunks)
            .ToList();
    }

    private static RetrievedChunk ToRetrievedChunk(SemanticChunkMatch match) =>
        new(
            match.Chunk.ChunkId,
            match.Chunk.DocumentId,
            match.Chunk.ActName,
            match.Chunk.ActShortName,
            match.Chunk.CategoryName,
            match.Chunk.SectionNumber,
            match.Chunk.SectionTitle,
            match.Chunk.Excerpt,
            match.Chunk.TopicClassification,
            match.Chunk.Keywords,
            match.SemanticScore,
            Math.Clamp(
                (match.SemanticScore * 0.55f) +
                (match.MetadataAlignmentScore * 0.30f) +
                (match.KeywordAlignmentScore * 0.15f),
                0f,
                1f),
            match.Chunk.TokenCount);

    private static bool IsAmbiguousQuestion(
        IReadOnlyCollection<string> questionTerms,
        IReadOnlyList<DocumentCandidate> rankedDocuments)
    {
        if (questionTerms.Count == 0)
        {
            return true;
        }

        var primary = rankedDocuments.FirstOrDefault();
        if (primary is null)
        {
            return true;
        }

        var specificTerms = questionTerms
            .Where(term => !GenericQuestionTerms.Contains(term))
            .ToList();

        if (specificTerms.Count == 0)
        {
            return true;
        }

        if (specificTerms.Count == 1 &&
            primary.MetadataAlignmentScore < 0.35f &&
            primary.DocumentSemanticScore < 0.58f)
        {
            return true;
        }

        if (rankedDocuments.Count >= 2)
        {
            var scoreGap = rankedDocuments[0].FinalDocumentScore - rankedDocuments[1].FinalDocumentScore;
            if (scoreGap < 0.04f &&
                primary.MetadataAlignmentScore < 0.45f &&
                primary.DocumentSemanticScore < 0.60f)
            {
                return true;
            }
        }

        return false;
    }

    private static string BuildClarificationQuestion(
        IReadOnlyList<DocumentCandidate> rankedDocuments,
        IReadOnlyCollection<string> questionTerms)
    {
        if (rankedDocuments.Count == 0)
        {
            return "Can you share one more detail, such as the legal issue, the people involved, or the type of document or dispute?";
        }

        var topCategories = rankedDocuments
            .Select(candidate => candidate.CategoryName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(2)
            .ToList();

        if (questionTerms.Count <= 3 && topCategories.Count == 1)
        {
            return $"Can you share one more detail about the {topCategories[0]} issue, such as who is involved or what has already happened?";
        }

        if (topCategories.Count == 2)
        {
            return $"Can you share one more detail so I can narrow this down? It may relate to {topCategories[0]} or {topCategories[1]}.";
        }

        var actName = rankedDocuments[0].ActName;
        return $"Can you share one more detail about the situation so I can confirm whether {actName} is the right source?";
    }

    private static float CalculateKeywordAlignment(
        IndexedChunk chunk,
        IReadOnlyCollection<string> questionTerms,
        IReadOnlyDictionary<string, float> questionTermSpecificity)
    {
        if (questionTerms is null || questionTerms.Count == 0 || chunk.Keywords.Count == 0)
        {
            return 0f;
        }

        var keywordTerms = chunk.Keywords
            .SelectMany(RagSourceHintExtractor.Tokenize)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var matchedWeight = keywordTerms
            .Where(questionTerms.Contains)
            .Sum(term => GetQuestionTermWeight(questionTermSpecificity, term));

        var denominator = CalculateTopQuestionWeight(questionTerms, questionTermSpecificity, 3);
        return denominator <= 0f
            ? 0f
            : Math.Clamp(matchedWeight / denominator, 0f, 1f);
    }

    private static float CalculateChunkMetadataAlignment(
        IndexedChunk chunk,
        IReadOnlyCollection<string> questionTerms,
        string normalizedQuestion,
        IReadOnlyDictionary<string, float> questionTermSpecificity)
    {
        var terms = new HashSet<string>(StringComparer.Ordinal);
        var phrases = new HashSet<string>(StringComparer.Ordinal);

        AddMetadata(chunk.ActName, terms, phrases);
        AddMetadata(chunk.ActShortName, terms, phrases);
        AddMetadata(chunk.CategoryName, terms, phrases);
        AddMetadata(chunk.SectionTitle, terms, phrases);
        AddMetadata(RagSourceHintExtractor.ExtractLeadingHeading(chunk.Excerpt), terms, phrases);
        AddMetadata(chunk.TopicClassification, terms, phrases);

        foreach (var keyword in chunk.Keywords)
        {
            AddMetadata(keyword, terms, phrases);
        }

        return CalculateMetadataAlignment(
            questionTerms,
            normalizedQuestion,
            terms,
            phrases,
            questionTermSpecificity);
    }

    private static float CalculateProfileAlignment(
        DocumentProfile profile,
        IReadOnlyCollection<string> questionTerms,
        string normalizedQuestion,
        IReadOnlyDictionary<string, float> questionTermSpecificity) =>
        CalculateMetadataAlignment(
            questionTerms,
            normalizedQuestion,
            profile.MetadataTerms,
            profile.MetadataPhrases,
            questionTermSpecificity);

    private static float CalculateMetadataAlignment(
        IReadOnlyCollection<string> questionTerms,
        string normalizedQuestion,
        IEnumerable<string> metadataTerms,
        IEnumerable<string> metadataPhrases,
        IReadOnlyDictionary<string, float> questionTermSpecificity)
    {
        if (questionTerms.Count == 0)
        {
            return 0f;
        }

        var termSet = metadataTerms as ISet<string> ?? new HashSet<string>(metadataTerms, StringComparer.Ordinal);
        var matchedWeight = questionTerms
            .Where(termSet.Contains)
            .Sum(term => GetQuestionTermWeight(questionTermSpecificity, term));
        var overlapDenominator = CalculateTopQuestionWeight(questionTerms, questionTermSpecificity, 3);
        var overlapScore = overlapDenominator <= 0f
            ? 0f
            : Math.Clamp(matchedWeight / overlapDenominator, 0f, 1f);

        var phraseScore = metadataPhrases
            .Where(phrase =>
                phrase.Length >= 3 &&
                normalizedQuestion.Contains(phrase, StringComparison.Ordinal))
            .Select(phrase => CalculatePhraseSpecificity(phrase, questionTermSpecificity))
            .DefaultIfEmpty(0f)
            .Max();

        return Math.Clamp(Math.Max(overlapScore, phraseScore), 0f, 1f);
    }

    private static float CalculateSemanticBreadth(IReadOnlyList<SemanticChunkMatch> matches)
    {
        if (matches.Count == 0)
        {
            return 0f;
        }

        var supportingCount = matches.Count(match =>
            match.SemanticScore >= 0.52f ||
            (match.SemanticScore >= RagPromptBuilder.SemanticCandidateFloor &&
             match.MetadataAlignmentScore >= 0.35f));

        return Math.Clamp(supportingCount / 3f, 0f, 1f);
    }

    private static float GetChunkRankingScore(SemanticChunkMatch match) =>
        Math.Clamp(
            (match.SemanticScore * 0.55f) +
            (match.MetadataAlignmentScore * 0.30f) +
            (match.KeywordAlignmentScore * 0.15f),
            0f,
            1f);

    private static void AddMetadata(string rawValue, ISet<string> terms, ISet<string> phrases)
    {
        var normalized = RagSourceHintExtractor.Normalize(rawValue);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        phrases.Add(normalized);

        foreach (var term in RagSourceHintExtractor.TokenizeNormalized(normalized))
        {
            terms.Add(term);
        }

        foreach (var alias in RagSourceHintExtractor.BuildAliases(normalized))
        {
            terms.Add(alias);
            phrases.Add(alias);
        }
    }

    private static IReadOnlyCollection<string> SplitTerms(string normalizedQuestion) =>
        RagSourceHintExtractor.TokenizeNormalized(normalizedQuestion);

    private static float CalculateSemanticScore(
        float[] questionVector,
        float[] focusVector,
        float[] chunkVector)
    {
        var baseScore = EmbeddingHelper.CosineSimilarity(questionVector, chunkVector);
        if (focusVector is null || focusVector.Length == 0 || focusVector.Length != chunkVector.Length)
        {
            return baseScore;
        }

        var focusScore = EmbeddingHelper.CosineSimilarity(focusVector, chunkVector);
        return Math.Clamp(
            (baseScore * 0.65f) +
            (focusScore * 0.35f),
            0f,
            1f);
    }

    private static IReadOnlyDictionary<string, float> BuildQuestionTermSpecificity(
        IReadOnlyCollection<string> questionTerms,
        IReadOnlyList<DocumentProfile> documentProfiles)
    {
        var specificity = new Dictionary<string, float>(StringComparer.Ordinal);
        if (questionTerms.Count == 0)
        {
            return specificity;
        }

        var totalDocuments = Math.Max(documentProfiles.Count, 1);
        var logDenominator = Math.Log(totalDocuments + 1d);

        foreach (var term in questionTerms)
        {
            var documentFrequency = documentProfiles.Count(profile => profile.MetadataTerms.Contains(term));
            var inverseDocumentFrequency = logDenominator <= 0d
                ? 1d
                : Math.Log((totalDocuments + 1d) / (documentFrequency + 1d)) / logDenominator;
            var genericPenalty = GenericQuestionTerms.Contains(term) ? 0.35d : 1d;
            specificity[term] = (float)Math.Clamp(
                (0.2d + (inverseDocumentFrequency * 0.8d)) * genericPenalty,
                0.05d,
                1d);
        }

        return specificity;
    }

    private static float GetQuestionTermWeight(
        IReadOnlyDictionary<string, float> questionTermSpecificity,
        string term) =>
        questionTermSpecificity.TryGetValue(term, out var weight)
            ? weight
            : 1f;

    private static float CalculateTopQuestionWeight(
        IReadOnlyCollection<string> questionTerms,
        IReadOnlyDictionary<string, float> questionTermSpecificity,
        int maxTerms) =>
        questionTerms
            .Select(term => GetQuestionTermWeight(questionTermSpecificity, term))
            .OrderByDescending(weight => weight)
            .Take(Math.Max(maxTerms, 1))
            .Sum();

    private static float CalculatePhraseSpecificity(
        string phrase,
        IReadOnlyDictionary<string, float> questionTermSpecificity)
    {
        if (string.IsNullOrWhiteSpace(phrase))
        {
            return 0f;
        }

        if (questionTermSpecificity.TryGetValue(phrase, out var phraseWeight))
        {
            return phraseWeight;
        }

        var phraseTerms = RagSourceHintExtractor.TokenizeNormalized(phrase);
        if (phraseTerms.Count == 0)
        {
            return 0f;
        }

        return phraseTerms
            .Where(questionTermSpecificity.ContainsKey)
            .Select(term => questionTermSpecificity[term])
            .DefaultIfEmpty(0f)
            .Average();
    }
}

public sealed record IndexedChunk(
    Guid ChunkId,
    Guid DocumentId,
    string ActName,
    string ActShortName,
    string ActNumber,
    int Year,
    string CategoryName,
    string SectionNumber,
    string SectionTitle,
    string Excerpt,
    IReadOnlyList<string> Keywords,
    string TopicClassification,
    int TokenCount,
    float[] Vector);

public sealed record SemanticChunkMatch(
    IndexedChunk Chunk,
    float SemanticScore,
    float KeywordAlignmentScore,
    float MetadataAlignmentScore);

public sealed record DocumentCandidate(
    Guid DocumentId,
    string ActName,
    string ActShortName,
    string CategoryName,
    IReadOnlyList<SemanticChunkMatch> TopMatches,
    float MaxChunkSemanticScore,
    float MeanTopChunkScore,
    float DocumentSemanticScore,
    float MetadataAlignmentScore,
    float SemanticBreadthScore,
    float HintBoostScore,
    float FinalDocumentScore);

public sealed record RetrievedChunk(
    Guid ChunkId,
    Guid DocumentId,
    string ActName,
    string ActShortName,
    string CategoryName,
    string SectionNumber,
    string SectionTitle,
    string Excerpt,
    string TopicClassification,
    IReadOnlyList<string> Keywords,
    float SemanticScore,
    float RelevanceScore,
    int TokenCount);

public sealed record RetrievalDecision
{
    public IReadOnlyList<RetrievedChunk> SelectedChunks { get; init; } = Array.Empty<RetrievedChunk>();

    public Guid? PrimaryDocumentId { get; init; }

    public IReadOnlyList<Guid> SupportingDocumentIds { get; init; } = Array.Empty<Guid>();

    public IReadOnlyList<DocumentCandidate> RankedDocuments { get; init; } = Array.Empty<DocumentCandidate>();

    public RagConfidenceBand ConfidenceBand { get; init; } = RagConfidenceBand.Low;

    public RagAnswerMode AnswerMode { get; init; } = RagAnswerMode.Insufficient;

    public string ClarificationQuestion { get; init; }

    public bool IsAmbiguousQuestion { get; init; }

    public bool IsGroundedAnswer =>
        AnswerMode == RagAnswerMode.Direct || AnswerMode == RagAnswerMode.Cautious;
}
