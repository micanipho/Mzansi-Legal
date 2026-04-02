using System;
using System.Collections.Generic;

namespace backend.Services.RagService.DTO;

/// <summary>
/// Full stored conversation returned to history and chat rehydration consumers.
/// </summary>
public class ConversationDetailDto
{
    public Guid ConversationId { get; set; }

    public DateTime StartedAt { get; set; }

    public string Language { get; set; } = "en";

    public int QuestionCount { get; set; }

    public List<ConversationHistoryMessageDto> Messages { get; set; } = new();
}
