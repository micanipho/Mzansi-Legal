using System;
using System.Collections.Generic;

namespace backend.Services.RagService.DTO;

/// <summary>Summary of a single conversation for display in the history list.</summary>
public class ConversationSummaryDto
{
    public Guid ConversationId { get; set; }
    public string FirstQuestion { get; set; }
    public int QuestionCount { get; set; }
    public DateTime StartedAt { get; set; }
    public string Language { get; set; }
}

/// <summary>Paged result returned by <c>GET /api/app/qa/conversations</c>.</summary>
public class ConversationsListDto
{
    public List<ConversationSummaryDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
