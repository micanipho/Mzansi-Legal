using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services.RagService;

public static class RagQueryFocusBuilder
{
    private static readonly HashSet<string> GenericIntentTerms = new(StringComparer.Ordinal)
    {
        "help", "issue", "issues", "law", "laws", "legal", "matter", "problem", "problems",
        "right", "rights", "rule", "rules", "situation", "thing", "things"
    };

    public static string Build(string translatedQuestionText)
    {
        var focusTerms = RagSourceHintExtractor.Tokenize(translatedQuestionText)
            .Where(term => !GenericIntentTerms.Contains(term))
            .ToArray();

        if (focusTerms.Length == 0)
        {
            return string.Empty;
        }

        return string.Join(' ', focusTerms);
    }
}
