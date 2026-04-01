using System;
using System.Linq;

namespace backend.Services.RagService;

public static class RagSourceMetadata
{
    public const string BindingLaw = "bindingLaw";
    public const string OfficialGuidance = "officialGuidance";
    public const string Primary = "primary";
    public const string Supporting = "supporting";

    private static readonly string[] GuidanceCues =
    {
        "faq",
        "form",
        "forms",
        "framework",
        "guide",
        "guidance",
        "manual",
        "material",
        "materials",
        "notice",
        "practice note",
        "template",
        "toolkit"
    };

    public static string DeriveAuthorityType(string title, string shortName, string actNumber)
    {
        var normalizedTitle = RagSourceHintExtractor.Normalize(title);
        var normalizedShortName = RagSourceHintExtractor.Normalize(shortName);

        if (ContainsGuidanceCue(normalizedTitle) || ContainsGuidanceCue(normalizedShortName))
        {
            return OfficialGuidance;
        }

        if (string.IsNullOrWhiteSpace(actNumber))
        {
            return OfficialGuidance;
        }

        return actNumber.Any(char.IsDigit)
            ? BindingLaw
            : OfficialGuidance;
    }

    public static string BuildSourceFamily(string title, string shortName)
    {
        var preferred = string.IsNullOrWhiteSpace(shortName) ? title : shortName;
        return RagSourceHintExtractor.Normalize(preferred);
    }

    public static string BuildSourceLocator(string sectionNumber, string sectionTitle)
    {
        if (!string.IsNullOrWhiteSpace(sectionNumber))
        {
            return sectionNumber.Trim();
        }

        return string.IsNullOrWhiteSpace(sectionTitle)
            ? string.Empty
            : sectionTitle.Trim();
    }

    private static bool ContainsGuidanceCue(string normalizedValue) =>
        !string.IsNullOrWhiteSpace(normalizedValue) &&
        GuidanceCues.Any(normalizedValue.Contains);
}
