using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace backend.Services.RagService;

public sealed class RagSourceHintExtractor
{
    private const double ActTitleBoost = 0.16d;
    private const double ShortNameBoost = 0.14d;
    private const double ActNumberBoost = 0.10d;
    private const double CategoryBoost = 0.08d;
    private static readonly HashSet<string> AcronymStopWords = new(StringComparer.Ordinal)
    {
        "a", "an", "and", "for", "in", "of", "on", "or", "the", "to", "with"
    };
    private static readonly HashSet<string> StopWords = new(StringComparer.Ordinal)
    {
        "a", "an", "and", "are", "as", "at", "be", "been", "being", "but", "by",
        "can", "could", "did", "do", "does", "for", "from", "had", "has", "have",
        "how", "i", "if", "in", "into", "is", "it", "its", "may", "me", "might",
        "my", "of", "on", "or", "our", "should", "that", "the", "their", "them",
        "there", "these", "they", "this", "those", "to", "under", "was", "we",
        "were", "what", "when", "where", "which", "who", "why", "will", "with",
        "without", "would", "you", "your"
    };

    public IReadOnlyList<SourceHint> Extract(
        string translatedQuestionText,
        IEnumerable<IndexedChunk> indexedChunks)
    {
        Guard.Against.NullOrWhiteSpace(translatedQuestionText, nameof(translatedQuestionText));
        Guard.Against.Null(indexedChunks, nameof(indexedChunks));

        var normalizedQuestion = Normalize(translatedQuestionText);
        var documentSamples = indexedChunks
            .GroupBy(chunk => chunk.DocumentId)
            .Select(group => group.First())
            .ToList();

        var hints = new List<SourceHint>();

        foreach (var chunk in documentSamples)
        {
            if (ContainsNormalizedPhrase(normalizedQuestion, chunk.ActName))
            {
                hints.Add(new SourceHint(
                    chunk.ActName,
                    RagSourceHintType.ActTitle,
                    chunk.DocumentId,
                    null,
                    ActTitleBoost));
            }

            if (!string.IsNullOrWhiteSpace(chunk.ActShortName) &&
                ContainsNormalizedPhrase(normalizedQuestion, chunk.ActShortName))
            {
                hints.Add(new SourceHint(
                    chunk.ActShortName,
                    RagSourceHintType.ShortName,
                    chunk.DocumentId,
                    null,
                    ShortNameBoost));
            }

            if (MatchesActNumber(normalizedQuestion, chunk.ActNumber, chunk.Year))
            {
                hints.Add(new SourceHint(
                    $"{chunk.ActNumber} of {chunk.Year}",
                    RagSourceHintType.ActNumber,
                    chunk.DocumentId,
                    null,
                    ActNumberBoost));
            }
        }

        foreach (var categoryName in documentSamples
            .Select(chunk => chunk.CategoryName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!ContainsNormalizedPhrase(normalizedQuestion, categoryName))
            {
                continue;
            }

            hints.Add(new SourceHint(
                categoryName,
                RagSourceHintType.Category,
                null,
                categoryName,
                CategoryBoost));
        }

        return hints
            .GroupBy(hint => $"{hint.HintType}:{hint.MatchedDocumentId}:{hint.MatchedCategoryName}:{Normalize(hint.HintText)}")
            .Select(group => group.OrderByDescending(hint => hint.BoostWeight).First())
            .ToList();
    }

    private static bool ContainsNormalizedPhrase(string normalizedQuestion, string candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        var normalizedCandidate = Normalize(candidate);
        if (normalizedCandidate.Length < 3)
        {
            return false;
        }

        return normalizedQuestion.Contains(normalizedCandidate, StringComparison.Ordinal);
    }

    private static bool MatchesActNumber(string normalizedQuestion, string actNumber, int year)
    {
        if (string.IsNullOrWhiteSpace(actNumber))
        {
            return false;
        }

        var compactPattern = $"act {Normalize(actNumber)} of {year}";
        var numericPattern = $"{Normalize(actNumber)} of {year}";
        return normalizedQuestion.Contains(compactPattern, StringComparison.Ordinal) ||
               normalizedQuestion.Contains(numericPattern, StringComparison.Ordinal);
    }

    internal static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var lower = value.Trim().ToLowerInvariant();
        lower = Regex.Replace(lower, @"[^\w\s]", " ");
        lower = Regex.Replace(lower, @"\s+", " ");
        return lower.Trim();
    }

    internal static IReadOnlyList<string> Tokenize(string value) =>
        TokenizeNormalized(Normalize(value));

    internal static IReadOnlyList<string> TokenizeNormalized(string normalizedValue)
    {
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            return Array.Empty<string>();
        }

        return normalizedValue
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(IsMeaningfulTerm)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    internal static bool IsMeaningfulTerm(string term) =>
        !string.IsNullOrWhiteSpace(term) &&
        term.Length > 2 &&
        !StopWords.Contains(term);

    internal static IReadOnlyCollection<string> BuildAliases(string normalizedPhrase)
    {
        if (string.IsNullOrWhiteSpace(normalizedPhrase))
        {
            return Array.Empty<string>();
        }

        var words = normalizedPhrase
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(word =>
                word.Length > 1 &&
                char.IsLetter(word[0]) &&
                !AcronymStopWords.Contains(word))
            .ToArray();

        if (words.Length < 2)
        {
            return Array.Empty<string>();
        }

        var aliases = new HashSet<string>(StringComparer.Ordinal);
        var maxWindowLength = Math.Min(words.Length, 6);

        for (var startIndex = 0; startIndex < words.Length; startIndex++)
        {
            for (var windowLength = 2; windowLength <= maxWindowLength; windowLength++)
            {
                if (startIndex + windowLength > words.Length)
                {
                    break;
                }

                var acronym = new string(words
                    .Skip(startIndex)
                    .Take(windowLength)
                    .Select(word => word[0])
                    .ToArray());

                if (acronym.Length >= 3)
                {
                    aliases.Add(acronym);
                }
            }
        }

        return aliases.ToArray();
    }

    internal static string ExtractLeadingHeading(string excerpt)
    {
        if (string.IsNullOrWhiteSpace(excerpt))
        {
            return string.Empty;
        }

        var firstLine = excerpt
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(firstLine))
        {
            return string.Empty;
        }

        return firstLine.Length > 160
            ? firstLine[..160]
            : firstLine;
    }
}

public enum RagSourceHintType
{
    ActTitle,
    ShortName,
    ActNumber,
    Category
}

public sealed record SourceHint(
    string HintText,
    RagSourceHintType HintType,
    Guid? MatchedDocumentId,
    string MatchedCategoryName,
    double BoostWeight);
