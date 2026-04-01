using backend.Domains.QA;
using System.Threading.Tasks;

namespace backend.Services.LanguageService;

/// <summary>
/// Detects the language of user input and translates non-English text to English
/// for use in the English-language RAG knowledge-base search pipeline.
/// </summary>
public interface ILanguageAppService
{
    /// <summary>
    /// Detects the language of the input text and returns the corresponding
    /// <see cref="Language"/> enum value.
    /// Returns <see cref="Language.English"/> when the language is unrecognised, unsupported,
    /// or the input is null or whitespace.
    /// </summary>
    /// <param name="text">The user's raw question text.</param>
    Task<Language> DetectLanguageAsync(string text);

    /// <summary>
    /// Translates the given text to English.
    /// Returns the original text unchanged when <paramref name="sourceLanguage"/> is
    /// <see cref="Language.English"/> — no API call is made in that case.
    /// Returns an empty string when the input is null or whitespace.
    /// </summary>
    /// <param name="text">Text to translate.</param>
    /// <param name="sourceLanguage">The detected source language of the text.</param>
    Task<string> TranslateToEnglishAsync(string text, Language sourceLanguage);
}
