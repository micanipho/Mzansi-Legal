using System;
using System.Collections.Generic;

namespace backend.Services.RagService.DTO;

/// <summary>
/// One stored conversation message returned for history and thread rehydration.
/// </summary>
public class ConversationHistoryMessageDto
{
    public Guid MessageId { get; set; }

    public string Type { get; set; } = "user";

    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string DetectedLanguageCode { get; set; } = "en";

    public List<RagCitationDto> Citations { get; set; } = new();
}
