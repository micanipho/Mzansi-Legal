using System;
using System.ComponentModel.DataAnnotations;

namespace backend.MzansiLegal.QnA.Dto;

public class AskQuestionInput
{
    public Guid? ConversationId { get; set; }

    [Required]
    [MaxLength(8)]
    public string Language { get; set; } = "en";

    [Required]
    [MaxLength(2000)]
    public string Text { get; set; }
}
