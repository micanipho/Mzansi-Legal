using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System;
using System.Threading.Tasks;

namespace backend.MzansiLegal.QnA;

/// <summary>
/// Handles language detection and translation using GPT-4o.
/// Supports en, zu (isiZulu), st (Sesotho), af (Afrikaans).
/// </summary>
public class LanguageService
{
    private readonly ChatClient _chatClient;

    public LanguageService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured.");
        var model = configuration["OpenAI:ChatModel"] ?? "gpt-4o";

        var openAiClient = new OpenAI.OpenAIClient(apiKey);
        _chatClient = openAiClient.GetChatClient(model);
    }

    /// <summary>
    /// Detects the language of the given text.
    /// Returns an ISO code: "en", "zu", "st", or "af".
    /// </summary>
    public async Task<string> DetectLanguageAsync(string text)
    {
        var prompt = $"Detect the language of the following text. Reply with ONLY the ISO 639-1 code (en, zu, st, or af). Text: \"{text}\"";
        var result = await _chatClient.CompleteChatAsync(prompt);
        var detected = result.Value.Content[0].Text.Trim().ToLowerInvariant();
        return detected is "en" or "zu" or "st" or "af" ? detected : "en";
    }

    /// <summary>
    /// Translates text to English for use in the RAG pipeline.
    /// If already English, returns the original text unchanged.
    /// </summary>
    public async Task<string> TranslateToEnglishAsync(string text, string sourceLanguage)
    {
        if (sourceLanguage == "en") return text;

        var languageName = sourceLanguage switch
        {
            "zu" => "isiZulu",
            "st" => "Sesotho",
            "af" => "Afrikaans",
            _ => "English"
        };

        var prompt = $"Translate the following {languageName} text to English. Reply with ONLY the translated text, no explanation.\n\nText: \"{text}\"";
        var result = await _chatClient.CompleteChatAsync(prompt);
        return result.Value.Content[0].Text.Trim();
    }

    /// <summary>
    /// Translates the given English answer to the target language.
    /// If target is English, returns the original text unchanged.
    /// </summary>
    public async Task<string> TranslateFromEnglishAsync(string englishText, string targetLanguage)
    {
        if (targetLanguage == "en") return englishText;

        var languageName = targetLanguage switch
        {
            "zu" => "isiZulu",
            "st" => "Sesotho",
            "af" => "Afrikaans",
            _ => "English"
        };

        var prompt = $"Translate the following English text to {languageName}. Reply with ONLY the translated text, no explanation.\n\nText: \"{englishText}\"";
        var result = await _chatClient.CompleteChatAsync(prompt);
        return result.Value.Content[0].Text.Trim();
    }
}
