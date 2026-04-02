using System.ComponentModel.DataAnnotations;

namespace backend.Services.RagService.DTO;

/// <summary>
/// Input DTO for <see cref="IRagAppService.AskAsync"/>.
/// Carries the user's natural-language legal question in English, isiZulu, Sesotho, or Afrikaans.
/// </summary>
public class AskQuestionRequest
{
    /// <summary>
    /// The legal question submitted by the user in a supported language.
    /// Non-English questions are translated to English internally for retrieval.
    /// Must not be null or whitespace. Maximum 30,000 characters.
    /// </summary>
    [Required]
    [MaxLength(30_000)]
    public string QuestionText { get; set; }
}
