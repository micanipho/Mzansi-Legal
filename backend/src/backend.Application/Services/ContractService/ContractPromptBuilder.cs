using Ardalis.GuardClauses;
using backend.Domains.ContractAnalysis;
using backend.Domains.QA;
using backend.Services.RagService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace backend.Services.ContractService;

/// <summary>
/// Builds the structured contract-analysis prompt and normalizes model output.
/// </summary>
public static class ContractPromptBuilder
{
    public static string BuildSystemPrompt(Language language)
    {
        var prompt =
            "You are a contract analyst for South African law.\n\n" +
            "Return ONLY valid JSON with this exact shape:\n" +
            "{\"healthScore\":0,\"summary\":\"...\",\"flags\":[{\"severity\":\"red|amber|green\",\"title\":\"...\",\"description\":\"...\",\"clauseText\":\"...\",\"legislationCitation\":\"...|null\"}]}\n\n" +
            "Rules:\n" +
            "1. Base every legal claim on the supplied legislation context only.\n" +
            "2. Keep the summary and flag descriptions plain-language and practical.\n" +
            "3. Include green flags for clauses that are notably user-friendly, protective, or above the usual baseline for this contract type.\n" +
            "4. Use amber or red flags for clauses that are below standard, risky, one-sided, or need human review.\n" +
            "5. Use severity red only for serious grounded concerns.\n" +
            "6. If grounding is limited, use amber wording and explain the limit instead of overstating certainty.\n" +
            "7. Keep Act names, section numbers, and clause excerpts in English.\n" +
            "8. Do not include markdown fences or commentary.";

        var languageDirective = BuildLanguageDirective(language);
        if (!string.IsNullOrWhiteSpace(languageDirective))
        {
            prompt += $"\n9. {languageDirective}";
        }

        return prompt;
    }

    public static string BuildUserPrompt(
        string displayTitle,
        ContractType contractType,
        string contractText,
        ContractLegislationContext context)
    {
        Guard.Against.NullOrWhiteSpace(displayTitle, nameof(displayTitle));
        Guard.Against.NullOrWhiteSpace(contractText, nameof(contractText));
        Guard.Against.Null(context, nameof(context));

        var prompt = new StringBuilder();
        prompt.AppendLine($"Contract title: {displayTitle}");
        prompt.AppendLine($"Contract type: {ToContractTypeValue(contractType)}");
        prompt.AppendLine($"Coverage state: {context.CoverageState}");
        prompt.AppendLine($"Coverage notes: {context.CoverageNotes}");
        prompt.AppendLine();
        prompt.AppendLine("Legislation context:");
        prompt.AppendLine(BuildContextBlock(context.AllChunks));
        prompt.AppendLine();
        prompt.AppendLine("Full contract text:");
        prompt.AppendLine(contractText);

        return prompt.ToString().TrimEnd();
    }

    public static string BuildContextBlock(IEnumerable<RetrievedChunk> chunks)
    {
        Guard.Against.Null(chunks, nameof(chunks));

        var sb = new StringBuilder();
        var indexedChunks = chunks.ToList();

        if (indexedChunks.Count == 0)
        {
            return "No grounded legislation context was available.";
        }

        foreach (var chunk in indexedChunks)
        {
            sb.AppendLine($"[{chunk.SourceTitle ?? chunk.ActName} - {chunk.SourceLocator ?? chunk.SectionNumber}]");
            sb.AppendLine($"Source role: {chunk.SourceRole}");
            sb.AppendLine(chunk.Excerpt);
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    public static string StripMarkdownFence(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return raw;
        }

        var trimmed = raw.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var firstNewLine = trimmed.IndexOf('\n');
        if (firstNewLine < 0)
        {
            return trimmed;
        }

        var inner = trimmed[(firstNewLine + 1)..];
        if (inner.EndsWith("```", StringComparison.Ordinal))
        {
            inner = inner[..^3];
        }

        return inner.Trim();
    }

    public static string ToContractTypeValue(ContractType contractType) => contractType switch
    {
        ContractType.Employment => "employment",
        ContractType.Lease => "lease",
        ContractType.Credit => "credit",
        _ => "service"
    };

    public static string ToSeverityValue(FlagSeverity severity) => severity switch
    {
        FlagSeverity.Red => "red",
        FlagSeverity.Amber => "amber",
        _ => "green"
    };

    public static string ToLanguageCode(Language language) => RagPromptBuilder.ToIsoCode(language);

    public static string BuildDisplayTitle(string fileName, string extractedText, ContractType contractType)
    {
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            var titleFromFile = System.IO.Path.GetFileNameWithoutExtension(fileName)?.Trim();
            if (!string.IsNullOrWhiteSpace(titleFromFile))
            {
                return titleFromFile;
            }
        }

        var firstMeaningfulLine = extractedText?
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault(line => line.Length >= 8);

        if (!string.IsNullOrWhiteSpace(firstMeaningfulLine))
        {
            return firstMeaningfulLine.Length > 80
                ? firstMeaningfulLine[..80]
                : firstMeaningfulLine;
        }

        return $"{ToContractTypeValue(contractType)} contract";
    }

    private static string BuildLanguageDirective(Language language) => language switch
    {
        Language.Zulu => "Respond in isiZulu. Keep all Act names and section numbers in English.",
        Language.Sesotho => "Respond in Sesotho. Keep all Act names and section numbers in English.",
        Language.Afrikaans => "Respond in Afrikaans. Keep all Act names and section numbers in English.",
        _ => string.Empty
    };
}
