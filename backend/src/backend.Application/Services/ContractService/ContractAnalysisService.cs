using Abp.Application.Services;
using Abp.UI;
using Ardalis.GuardClauses;
using backend.Domains.ContractAnalysis;
using backend.Domains.QA;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace backend.Services.ContractService;

/// <summary>
/// Executes contract extraction, type detection, legislation grounding, and structured analysis generation.
/// </summary>
public class ContractAnalysisService : ApplicationService
{
    internal const int MinimumReadableCharacters = 100;
    internal const int MaximumFileSizeBytes = 10 * 1024 * 1024;

    private readonly ContractLegislationContextBuilder _contractLegislationContextBuilder;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;
    private readonly string _chatModel;

    public ContractAnalysisService(
        ContractLegislationContextBuilder contractLegislationContextBuilder,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        Guard.Against.Null(contractLegislationContextBuilder, nameof(contractLegislationContextBuilder));
        Guard.Against.Null(httpClientFactory, nameof(httpClientFactory));
        Guard.Against.Null(configuration, nameof(configuration));

        _contractLegislationContextBuilder = contractLegislationContextBuilder;
        _httpClientFactory = httpClientFactory;
        _apiKey = configuration["OpenAI:ApiKey"];
        _chatModel = configuration["OpenAI:ChatModel"];

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException("OpenAI:ApiKey must be set in appsettings.json.");
        }

        if (string.IsNullOrWhiteSpace(_chatModel))
        {
            throw new InvalidOperationException("OpenAI:ChatModel must be set in appsettings.json.");
        }
    }

    public virtual async Task<(ContractAnalysisDraft Analysis, string ExtractedText)> AnalyseAsync(
        byte[] fileBytes,
        string fileName,
        string contentType,
        Language responseLanguage)
    {
        Guard.Against.Null(fileBytes, nameof(fileBytes));
        Guard.Against.NullOrWhiteSpace(fileName, nameof(fileName));

        ValidateUpload(fileBytes, fileName, contentType);

        var extraction = await ExtractTextAsync(fileBytes, fileName);
        var contractType = await DetectContractTypeAsync(extraction.ExtractedText);
        var context = await _contractLegislationContextBuilder.BuildAsync(contractType, extraction.ExtractedText);
        var draft = await BuildAnalysisDraftAsync(
            fileName,
            extraction.ExtractedText,
            contractType,
            responseLanguage,
            context);

        return (draft, extraction.ExtractedText);
    }

    internal static Language ParseLanguageCode(string languageCode) => languageCode?.Trim().ToLowerInvariant() switch
    {
        "zu" => Language.Zulu,
        "st" => Language.Sesotho,
        "af" => Language.Afrikaans,
        _ => Language.English
    };

    protected virtual async Task<ContractExtractionResult> ExtractTextAsync(byte[] fileBytes, string fileName)
    {
        string directText;

        try
        {
            directText = ExtractDirectText(fileBytes);
        }
        catch
        {
            throw new UserFriendlyException("We couldn't read this PDF. Please upload a valid, unprotected PDF file.");
        }

        var directCharacterCount = CountMeaningfulCharacters(directText);
        if (directCharacterCount >= MinimumReadableCharacters)
        {
            return new ContractExtractionResult(directText.Trim(), "directPdfText", directCharacterCount);
        }

        var ocrText = await ExtractTextWithOcrAsync(fileBytes, fileName);
        var ocrCharacterCount = CountMeaningfulCharacters(ocrText);
        if (ocrCharacterCount < MinimumReadableCharacters)
        {
            throw new UserFriendlyException("We couldn't read enough text from this contract PDF. Please upload a clearer PDF or a text-based file.");
        }

        return new ContractExtractionResult(ocrText.Trim(), "ocrFallback", ocrCharacterCount);
    }

    protected virtual string ExtractDirectText(byte[] fileBytes)
    {
        using var stream = new MemoryStream(fileBytes, writable: false);
        return PdfIngestionService.PdfIngestionAppService.ExtractTextFromStream(stream);
    }

    protected virtual async Task<string> ExtractTextWithOcrAsync(byte[] fileBytes, string fileName)
    {
        using var client = _httpClientFactory.CreateClient("OpenAI");
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "v1/responses")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _apiKey) },
            Content = JsonContent.Create(new
            {
                model = _chatModel,
                input = new object[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "input_text",
                                text = "Extract all readable text from this South African contract PDF. Return only the extracted text."
                            },
                            new
                            {
                                type = "input_file",
                                filename = fileName,
                                file_data = $"data:application/pdf;base64,{Convert.ToBase64String(fileBytes)}"
                            }
                        }
                    }
                }
            })
        };

        using var response = await client.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<OpenAiResponsesResponse>();
        return body?.Output?
            .SelectMany(item => item.Content ?? Enumerable.Empty<OpenAiResponsesContent>())
            .Where(content => string.Equals(content.Type, "output_text", StringComparison.OrdinalIgnoreCase))
            .Select(content => content.Text)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
            ?? string.Empty;
    }

    protected virtual async Task<ContractType> DetectContractTypeAsync(string extractedText)
    {
        var sample = extractedText.Length > 500 ? extractedText[..500] : extractedText;
        var raw = await CallChatCompletionsAsync(
            "You classify South African contracts. Reply with only one label: Employment, Lease, Credit, or Service.",
            $"What type of contract is this? Respond with: Employment, Lease, Credit, or Service.\n\n{sample}",
            0d);

        return raw.Trim() switch
        {
            "Employment" => ContractType.Employment,
            "Lease" => ContractType.Lease,
            "Credit" => ContractType.Credit,
            "Service" => ContractType.Service,
            _ => throw new UserFriendlyException("This contract type is not currently supported. Please upload an employment, lease, credit, or service contract.")
        };
    }

    protected virtual async Task<ContractAnalysisDraft> BuildAnalysisDraftAsync(
        string fileName,
        string extractedText,
        ContractType contractType,
        Language responseLanguage,
        ContractLegislationContext context)
    {
        var displayTitle = ContractPromptBuilder.BuildDisplayTitle(fileName, extractedText, contractType);
        var systemPrompt = ContractPromptBuilder.BuildSystemPrompt(responseLanguage);
        var userPrompt = ContractPromptBuilder.BuildUserPrompt(displayTitle, contractType, extractedText, context);
        var rawResponse = await CallChatCompletionsAsync(systemPrompt, userPrompt, 0.1d);
        var json = ContractPromptBuilder.StripMarkdownFence(rawResponse);
        var parsed = JsonSerializer.Deserialize<ContractAnalysisPayload>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (parsed == null)
        {
            throw new UserFriendlyException("The contract analysis response was invalid. Please try again.");
        }

        var flags = NormalizeFlags(parsed.Flags, context).ToList();
        return new ContractAnalysisDraft(
            contractType,
            Math.Clamp(parsed.HealthScore, 0, 100),
            string.IsNullOrWhiteSpace(parsed.Summary)
                ? BuildFallbackSummary(contractType, responseLanguage)
                : parsed.Summary.Trim(),
            responseLanguage,
            context.CoverageState,
            flags);
    }

    protected virtual async Task<string> CallChatCompletionsAsync(string systemPrompt, string userPrompt, double temperature)
    {
        using var client = _httpClientFactory.CreateClient("OpenAI");
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _apiKey) },
            Content = JsonContent.Create(new OpenAiChatRequest(
                _chatModel,
                temperature,
                new[]
                {
                    new OpenAiChatMessage("system", systemPrompt),
                    new OpenAiChatMessage("user", userPrompt)
                }))
        };

        using var response = await client.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<OpenAiChatResponse>();
        return body?.Choices?[0]?.Message?.Content
               ?? throw new InvalidOperationException("OpenAI chat response contained no content.");
    }

    private static IEnumerable<ContractAnalysisFlagDraft> NormalizeFlags(
        IEnumerable<ContractFlagPayload> rawFlags,
        ContractLegislationContext context)
    {
        var flags = rawFlags ?? Array.Empty<ContractFlagPayload>();
        var knownCitations = context.AllChunks
            .Select(chunk => $"{chunk.ActName} {chunk.SectionNumber}".ToLowerInvariant())
            .Concat(context.AllChunks.Select(chunk => $"{chunk.SourceTitle ?? chunk.ActName} {chunk.SourceLocator ?? chunk.SectionNumber}".ToLowerInvariant()))
            .ToList();

        foreach (var flag in flags)
        {
            if (string.IsNullOrWhiteSpace(flag?.Title) ||
                string.IsNullOrWhiteSpace(flag.Description) ||
                string.IsNullOrWhiteSpace(flag.ClauseText))
            {
                continue;
            }

            var severity = ParseSeverity(flag.Severity);
            var citation = string.IsNullOrWhiteSpace(flag.LegislationCitation)
                ? null
                : flag.LegislationCitation.Trim();
            var isGrounded = !string.IsNullOrWhiteSpace(citation) &&
                             knownCitations.Any(known => citation.ToLowerInvariant().Contains(known, StringComparison.Ordinal));

            if (!isGrounded && severity == FlagSeverity.Red)
            {
                severity = FlagSeverity.Amber;
                citation = null;
            }

            var description = flag.Description.Trim();
            if (!isGrounded && context.CoverageState != ContractCoverageState.InCorpusNow)
            {
                description = $"{description} The current legislation corpus supports this only partially, so treat it as a review point rather than a definitive legal conclusion.";
            }

            yield return new ContractAnalysisFlagDraft(
                severity,
                flag.Title.Trim(),
                description,
                flag.ClauseText.Trim(),
                citation,
                isGrounded);
        }
    }

    private static void ValidateUpload(byte[] fileBytes, string fileName, string contentType)
    {
        if (fileBytes.Length == 0)
        {
            throw new UserFriendlyException("Please upload a non-empty PDF file.");
        }

        if (fileBytes.Length > MaximumFileSizeBytes)
        {
            throw new UserFriendlyException("The contract PDF is too large. Please upload a PDF under 10 MB.");
        }

        var isPdfFileName = Path.GetExtension(fileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
        var isPdfContentType = string.IsNullOrWhiteSpace(contentType) ||
            contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);

        if (!isPdfFileName || !isPdfContentType)
        {
            throw new UserFriendlyException("Only PDF contract uploads are supported right now.");
        }
    }

    private static int CountMeaningfulCharacters(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? 0
            : text.Count(character => !char.IsWhiteSpace(character));

    private static FlagSeverity ParseSeverity(string rawSeverity) => rawSeverity?.Trim().ToLowerInvariant() switch
    {
        "red" => FlagSeverity.Red,
        "green" => FlagSeverity.Green,
        _ => FlagSeverity.Amber
    };

    private static string BuildFallbackSummary(ContractType contractType, Language language)
    {
        var contractLabel = ContractPromptBuilder.ToContractTypeValue(contractType);
        return language switch
        {
            Language.Zulu => $"Lokhu kubuyekezwa kwe-{contractLabel} contract kuqede ukuhlonza amaphuzu okumele abuyekezwe ngaphambi kokusayina.",
            Language.Sesotho => $"Tlhahlobo ena ya {contractLabel} contract e supile dintlha tse tshwanetseng ho shejwa pele o saena.",
            Language.Afrikaans => $"Hierdie {contractLabel} contract-ontleding wys die belangrikste punte wat voor ondertekening nagegaan moet word.",
            _ => $"This {contractLabel} contract analysis highlights the main points that should be reviewed before signing."
        };
    }

    private sealed record OpenAiChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("messages")] OpenAiChatMessage[] Messages);

    private sealed record OpenAiChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record OpenAiChatResponse(
        [property: JsonPropertyName("choices")] OpenAiChatChoice[] Choices);

    private sealed record OpenAiChatChoice(
        [property: JsonPropertyName("message")] OpenAiChatMessage Message);

    private sealed class ContractAnalysisPayload
    {
        [JsonPropertyName("healthScore")]
        public int HealthScore { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("flags")]
        public List<ContractFlagPayload> Flags { get; set; } = new();
    }

    private sealed class ContractFlagPayload
    {
        [JsonPropertyName("severity")]
        public string Severity { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("clauseText")]
        public string ClauseText { get; set; }

        [JsonPropertyName("legislationCitation")]
        public string LegislationCitation { get; set; }
    }

    private sealed class OpenAiResponsesResponse
    {
        [JsonPropertyName("output")]
        public List<OpenAiResponsesItem> Output { get; set; } = new();
    }

    private sealed class OpenAiResponsesItem
    {
        [JsonPropertyName("content")]
        public List<OpenAiResponsesContent> Content { get; set; } = new();
    }

    private sealed class OpenAiResponsesContent
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
