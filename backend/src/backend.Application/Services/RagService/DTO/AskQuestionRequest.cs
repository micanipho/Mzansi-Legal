using System.ComponentModel.DataAnnotations;

namespace backend.Services.RagService.DTO;

/// <summary>
/// Input DTO for <see cref="IRagAppService.AskAsync"/>.
/// Carries the user's natural-language legal question.
/// </summary>
public class AskQuestionRequest
{
    /// <summary>
    /// The legal question submitted by the user in plain English.
    /// Must not be null or whitespace. Maximum 30,000 characters.
    /// </summary>
    [Required]
    [MaxLength(30_000)]
    public string QuestionText { get; set; }
}
