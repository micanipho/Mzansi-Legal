using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services.RagService;

public static class RagQueryFocusBuilder
{
    private static readonly HashSet<string> CourtesyTerms = new(StringComparer.Ordinal)
    {
        "please"
    };

    private static readonly HashSet<string> GenericIntentTerms = new(StringComparer.Ordinal)
    {
        "help", "issue", "issues", "law", "laws", "legal", "matter", "problem", "problems",
        "right", "rights", "rule", "rules", "situation", "thing", "things"
    };

    private static readonly HashSet<string> RightsSignalTerms = new(StringComparer.Ordinal)
    {
        "right", "rights", "entitlement", "entitlements"
    };

    public static string Build(string translatedQuestionText)
    {
        var allTerms = RagSourceHintExtractor.Tokenize(translatedQuestionText)
            .Where(term => !CourtesyTerms.Contains(term))
            .ToArray();
        var focusTerms = allTerms
            .Where(term => !GenericIntentTerms.Contains(term))
            .ToArray();

        if (focusTerms.Length == 0)
        {
            return string.Empty;
        }

        if (focusTerms.Length == 1 &&
            allTerms.Any(term => RightsSignalTerms.Contains(term)))
        {
            return $"{focusTerms[0]} rights";
        }

        return string.Join(' ', focusTerms);
    }
}
