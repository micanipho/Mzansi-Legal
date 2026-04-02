using Abp.Application.Services;
using Ardalis.GuardClauses;
using backend.Domains.QA;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace backend.Services.LanguageService;

/// <summary>
/// Uses GPT-4o to detect the language of user input and translate non-English questions to English
/// for the RAG search pipeline. All failures fall back to Language.English so the Q&amp;A flow
/// is never blocked by a language service error.
/// </summary>
public class LanguageAppService : ApplicationService, ILanguageAppService
{
    // System prompts kept as constants to avoid magic strings scattered through the class.
    private const string DetectionSystemPrompt =
        "You are a language identification assistant. " +
        "Reply only with a single ISO 639-1 code.";

    private const string TranslationSystemPrompt =
        "You are a professional translator. " +
        "Translate text faithfully, preserving meaning and tone. " +
        "Do not add commentary or explanations.";

    /// <summary>Temperature of 0.0 used for deterministic language detection and translation.</summary>
    private const double DetectionTemperature = 0.0;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;
    private readonly string _chatModel;

    /// <summary>
    /// Initialises the service and validates required OpenAI configuration keys.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <c>OpenAI:ApiKey</c> or <c>OpenAI:ChatModel</c> is missing or empty.
    /// </exception>
    public LanguageAppService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        Guard.Against.Null(httpClientFactory, nameof(httpClientFactory));
        Guard.Against.Null(configuration, nameof(configuration));

        _httpClientFactory = httpClientFactory;

        _apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException(
                "OpenAI:ApiKey must be set in appsettings.json. " +
                "Store your real key in appsettings.Development.json (gitignored).");

        _chatModel = configuration["OpenAI:ChatModel"];
        if (string.IsNullOrWhiteSpace(_chatModel))
            throw new InvalidOperationException(
                "OpenAI:ChatModel must be set in appsettings.json (e.g., \"gpt-4o\").");
    }

    /// <inheritdoc />
    public async Task<Language> DetectLanguageAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Language.English;

        try
        {
            var userMessage =
                "Identify the language of the following text. " +
                "Reply with exactly one of: en, zu, st, af. " +
                "If the language is not in this list or you are unsure, reply: en\n\n" +
                $"Text: {text}";

            var raw = await CallChatCompletionsAsync(DetectionSystemPrompt, userMessage);
            return ParseLanguageCode(raw.Trim().ToLowerInvariant());
        }
        catch (Exception ex)
        {
            Logger.Warn($"Language detection failed; defaulting to English. Error: {ex.Message}");
            return Language.English;
        }
    }

    /// <inheritdoc />
    public async Task<string> TranslateToEnglishAsync(string text, Language sourceLanguage)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // No API call needed for English — return immediately.
        if (sourceLanguage == Language.English)
            return text;

        try
        {
            var languageName = GetLanguageDisplayName(sourceLanguage);
            var userMessage =
                $"Translate the following {languageName} text to English. " +
                $"Return only the translation.\n\nText: {text}";

            var translated = await CallChatCompletionsAsync(TranslationSystemPrompt, userMessage);
            return translated.Trim();
        }
        catch (Exception ex)
        {
            Logger.Warn(
                $"Translation from {sourceLanguage} to English failed; using original text. " +
                $"Error: {ex.Message}");
            return text;
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Maps a trimmed, lowercase ISO 639-1 code to the corresponding <see cref="Language"/> enum value.
    /// Returns <see cref="Language.English"/> for any unrecognised code.
    /// </summary>
    private static Language ParseLanguageCode(string code) => code switch
    {
        "zu" => Language.Zulu,
        "st" => Language.Sesotho,
        "af" => Language.Afrikaans,
        _    => Language.English
    };

    /// <summary>
    /// Returns the full display name for a <see cref="Language"/>, used as the source language
    /// label in translation prompts (e.g. "isiZulu", "Sesotho", "Afrikaans").
    /// </summary>
    private static string GetLanguageDisplayName(Language language) => language switch
    {
        Language.Zulu      => "isiZulu",
        Language.Sesotho   => "Sesotho",
        Language.Afrikaans => "Afrikaans",
        _                  => "English"
    };

    /// <summary>
    /// Sends a single-turn chat completions request via the named "OpenAI" HttpClient
    /// and returns the assistant's reply text.
    /// </summary>
    private async Task<string> CallChatCompletionsAsync(string systemPrompt, string userPrompt)
    {
        using var client = _httpClientFactory.CreateClient("OpenAI");
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _apiKey) },
            Content = JsonContent.Create(new OpenAiChatRequest(
                Model: _chatModel,
                Temperature: DetectionTemperature,
                Messages: new[]
                {
                    new OpenAiChatMessage(Role: "system", Content: systemPrompt),
                    new OpenAiChatMessage(Role: "user",   Content: userPrompt)
                }))
        };

        using var response = await client.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<OpenAiChatResponse>();
        return body?.Choices?[0]?.Message?.Content
               ?? throw new InvalidOperationException("OpenAI chat response contained no content.");
    }

    // ── Private types for OpenAI chat REST serialisation ─────────────────────

    /// <summary>Request body for POST /v1/chat/completions.</summary>
    private sealed record OpenAiChatRequest(
        [property: JsonPropertyName("model")]       string Model,
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("messages")]    OpenAiChatMessage[] Messages);

    /// <summary>A single message in the chat messages array.</summary>
    private sealed record OpenAiChatMessage(
        [property: JsonPropertyName("role")]    string Role,
        [property: JsonPropertyName("content")] string Content);

    /// <summary>Top-level response from POST /v1/chat/completions.</summary>
    private sealed record OpenAiChatResponse(
        [property: JsonPropertyName("choices")] OpenAiChatChoice[] Choices);

    /// <summary>A single choice in the chat response.</summary>
    private sealed record OpenAiChatChoice(
        [property: JsonPropertyName("message")] OpenAiChatMessage Message);
}
