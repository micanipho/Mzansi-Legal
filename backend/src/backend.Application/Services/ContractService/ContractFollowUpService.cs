using Abp.Application.Services;
using Ardalis.GuardClauses;
using backend.Domains.ContractAnalysis;
using backend.Domains.QA;
using backend.Services.ContractService.DTO;
using backend.Services.LanguageService;
using backend.Services.RagService;
using backend.Services.RagService.DTO;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace backend.Services.ContractService;

/// <summary>
/// Answers follow-up questions about a saved contract using stored contract text plus grounded legislation.
/// </summary>
public class ContractFollowUpService : ApplicationService
{
    private static readonly HashSet<string> UrgentTerms = new(StringComparer.Ordinal)
    {
        "asap", "deadline", "evict", "eviction", "hearing", "immediately", "lockout", "urgent"
    };

    private static readonly string[] UrgentRiskPhrases =
    {
        "changed the locks",
        "cut off my electricity",
        "cut off my power",
        "cut off my water",
        "hearing tomorrow",
        "locked me out",
        "right now"
    };

    private readonly ContractLegislationContextBuilder _contractLegislationContextBuilder;
    private readonly ILanguageAppService _languageAppService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;
    private readonly string _chatModel;

    public ContractFollowUpService(
        ContractLegislationContextBuilder contractLegislationContextBuilder,
        ILanguageAppService languageAppService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        Guard.Against.Null(contractLegislationContextBuilder, nameof(contractLegislationContextBuilder));
        Guard.Against.Null(languageAppService, nameof(languageAppService));
        Guard.Against.Null(httpClientFactory, nameof(httpClientFactory));
        Guard.Against.Null(configuration, nameof(configuration));

        _contractLegislationContextBuilder = contractLegislationContextBuilder;
        _languageAppService = languageAppService;
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

    public virtual async Task<ContractFollowUpAnswerDto> AskAsync(
        ContractAnalysis analysis,
        string questionText,
        string responseLanguageCode = null)
    {
        Guard.Against.Null(analysis, nameof(analysis));
        Guard.Against.NullOrWhiteSpace(questionText, nameof(questionText));

        var detectedLanguage = await _languageAppService.DetectLanguageAsync(questionText);
        var detectedLanguageCode = RagPromptBuilder.ToIsoCode(detectedLanguage);
        var translatedQuestionText = await _languageAppService.TranslateToEnglishAsync(questionText, detectedLanguage);
        var responseLanguage = string.IsNullOrWhiteSpace(responseLanguageCode)
            ? detectedLanguage
            : ContractAnalysisService.ParseLanguageCode(responseLanguageCode);
        var requiresUrgentAttention = RequiresUrgentAttention(translatedQuestionText);

        var contractExcerpts = SelectRelevantExcerpts(analysis.ExtractedText, translatedQuestionText);
        var context = await _contractLegislationContextBuilder.BuildForFollowUpAsync(
            analysis.ContractType,
            analysis.ExtractedText,
            translatedQuestionText);
        var (answerMode, confidenceBand) = DetermineAnswerMode(context, contractExcerpts.Count, requiresUrgentAttention);

        if (answerMode == RagAnswerMode.Insufficient)
        {
            return BuildNonGroundedResult(
                responseLanguage,
                detectedLanguageCode,
                contractExcerpts,
                requiresUrgentAttention);
        }

        var answerText = await BuildGroundedAnswerAsync(
            analysis,
            questionText,
            responseLanguage,
            answerMode,
            contractExcerpts,
            context,
            requiresUrgentAttention);

        return new ContractFollowUpAnswerDto
        {
            AnswerText = answerText,
            AnswerMode = answerMode,
            ConfidenceBand = confidenceBand,
            RequiresUrgentAttention = requiresUrgentAttention,
            DetectedLanguageCode = detectedLanguageCode,
            ContractExcerpts = contractExcerpts.ToList(),
            Citations = RagAppService.CreateCitations(context.AllChunks)
        };
    }

    protected virtual Task<string> CallChatCompletionsAsync(string systemPrompt, string userPrompt, double temperature)
    {
        return CallChatCompletionsInternalAsync(systemPrompt, userPrompt, temperature);
    }

    internal static ContractFollowUpAnswerDto BuildNonGroundedResult(
        Language responseLanguage,
        string detectedLanguageCode,
        IReadOnlyList<string> contractExcerpts,
        bool requiresUrgentAttention)
    {
        return new ContractFollowUpAnswerDto
        {
            AnswerText = RagPromptBuilder.BuildInsufficientResponse(responseLanguage, requiresUrgentAttention),
            AnswerMode = RagAnswerMode.Insufficient,
            ConfidenceBand = RagConfidenceBand.Low,
            RequiresUrgentAttention = requiresUrgentAttention,
            DetectedLanguageCode = detectedLanguageCode,
            ContractExcerpts = contractExcerpts?.ToList() ?? new List<string>(),
            Citations = new List<RagCitationDto>()
        };
    }

    public static IReadOnlyList<string> SelectRelevantExcerpts(string extractedText, string translatedQuestionText)
    {
        if (string.IsNullOrWhiteSpace(extractedText))
        {
            return Array.Empty<string>();
        }

        var questionTerms = RagSourceHintExtractor.Tokenize(translatedQuestionText);
        var candidates = extractedText
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => line.Length >= 24)
            .Take(160)
            .ToList();

        if (candidates.Count == 0)
        {
            return Array.Empty<string>();
        }

        var scored = candidates
            .Select((line, index) => new
            {
                Line = line,
                Index = index,
                Score = ScoreExcerpt(line, questionTerms)
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Index)
            .ToList();

        var matched = scored
            .Where(item => item.Score > 0)
            .Take(3)
            .Select(item => item.Line)
            .ToList();

        if (matched.Count > 0)
        {
            return matched;
        }

        return candidates.Take(2).ToList();
    }

    private async Task<string> BuildGroundedAnswerAsync(
        ContractAnalysis analysis,
        string originalQuestionText,
        Language responseLanguage,
        RagAnswerMode answerMode,
        IReadOnlyList<string> contractExcerpts,
        ContractLegislationContext context,
        bool requiresUrgentAttention)
    {
        var systemPrompt = BuildSystemPrompt(answerMode, responseLanguage, requiresUrgentAttention);
        var userPrompt = BuildUserPrompt(
            analysis,
            originalQuestionText,
            answerMode,
            contractExcerpts,
            context,
            requiresUrgentAttention);

        return await CallChatCompletionsAsync(
            systemPrompt,
            userPrompt,
            answerMode == RagAnswerMode.Direct ? 0.2d : 0.1d);
    }

    private async Task<string> CallChatCompletionsInternalAsync(
        string systemPrompt,
        string userPrompt,
        double temperature)
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

    private static (RagAnswerMode AnswerMode, RagConfidenceBand ConfidenceBand) DetermineAnswerMode(
        ContractLegislationContext context,
        int excerptCount,
        bool requiresUrgentAttention)
    {
        if (context.AllChunks.Count == 0 || excerptCount == 0)
        {
            return (RagAnswerMode.Insufficient, RagConfidenceBand.Low);
        }

        if (context.CoverageState == ContractCoverageState.NeedsCorpusExpansion)
        {
            return (RagAnswerMode.Insufficient, RagConfidenceBand.Low);
        }

        if (context.CoverageState == ContractCoverageState.InCorpusNow &&
            context.PrimaryChunks.Count > 0 &&
            !requiresUrgentAttention)
        {
            return (RagAnswerMode.Direct, RagConfidenceBand.High);
        }

        return (RagAnswerMode.Cautious, RagConfidenceBand.Medium);
    }

    private static int ScoreExcerpt(string excerpt, IReadOnlyList<string> questionTerms)
    {
        var normalizedExcerpt = RagSourceHintExtractor.Normalize(excerpt);
        var score = questionTerms.Count(term => normalizedExcerpt.Contains(term, StringComparison.Ordinal));

        if (excerpt.Any(char.IsDigit))
        {
            score += 1;
        }

        return score;
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
        return questionTerms.Any(UrgentTerms.Contains);
    }

    private static string BuildSystemPrompt(
        RagAnswerMode answerMode,
        Language responseLanguage,
        bool requiresUrgentAttention)
    {
        var prompt =
            "You are a South African contract analyst answering a follow-up question about a specific contract.\n\n" +
            "CRITICAL RULES:\n" +
            "1. Use only the supplied contract excerpts and legislation context.\n" +
            "2. Include citations for every material legal claim in the format [Act Name, Section X].\n" +
            "3. If the support is limited, say so clearly instead of overstating certainty.\n" +
            "4. Keep the answer practical, plain-language, and focused on what the clause means for the user.\n" +
            "5. Keep Act names, section numbers, and quoted clause text in English.";

        if (answerMode == RagAnswerMode.Cautious)
        {
            prompt += "\n6. Lead with what is uncertain or needs review before giving the grounded answer.";
        }

        if (requiresUrgentAttention)
        {
            prompt += "\n7. Add a short immediate-help note if the situation may involve urgent deadlines, lockout, eviction, or immediate harm.";
        }

        var languageDirective = responseLanguage switch
        {
            Language.Zulu => "Respond in isiZulu. Keep all Act names and section numbers in English.",
            Language.Sesotho => "Respond in Sesotho. Keep all Act names and section numbers in English.",
            Language.Afrikaans => "Respond in Afrikaans. Keep all Act names and section numbers in English.",
            _ => string.Empty
        };

        if (!string.IsNullOrWhiteSpace(languageDirective))
        {
            prompt += $"\n8. {languageDirective}";
        }

        return prompt;
    }

    private static string BuildUserPrompt(
        ContractAnalysis analysis,
        string originalQuestionText,
        RagAnswerMode answerMode,
        IReadOnlyList<string> contractExcerpts,
        ContractLegislationContext context,
        bool requiresUrgentAttention)
    {
        var contractTextBlock = string.Join(
            "\n\n",
            contractExcerpts.Select((excerpt, index) => $"Contract excerpt {index + 1}:\n{excerpt}"));
        var legislationContextBlock = ContractPromptBuilder.BuildContextBlock(context.AllChunks);
        var answerLead = answerMode == RagAnswerMode.Cautious
            ? "Answer carefully. Explain what is clear, what is uncertain, and what should be reviewed."
            : "Answer directly using the contract excerpts and legislation context only.";

        if (requiresUrgentAttention)
        {
            answerLead += " Add a short immediate-help note if the situation sounds urgent.";
        }

        return
            $"Contract title: {ContractPromptBuilder.BuildDisplayTitle(null, analysis.ExtractedText, analysis.ContractType)}\n" +
            $"Contract type: {ContractPromptBuilder.ToContractTypeValue(analysis.ContractType)}\n" +
            $"Coverage state: {context.CoverageState}\n" +
            $"Coverage notes: {context.CoverageNotes}\n\n" +
            $"Relevant contract excerpts:\n\n{contractTextBlock}\n\n" +
            $"Legislation context:\n\n{legislationContextBlock}\n\n" +
            $"Follow-up question: {originalQuestionText}\n\n" +
            $"{answerLead}\n\n" +
            "Answer:";
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
}
