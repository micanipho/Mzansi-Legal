using System.Collections.Generic;

namespace backend.Services.ContractService.DTO;

/// <summary>
/// Follow-up question submitted against a saved contract analysis.
/// </summary>
public class AskContractQuestionRequest
{
    public string QuestionText { get; set; }

    public string ResponseLanguageCode { get; set; }

    public List<ContractConversationHistoryMessageDto> ConversationHistory { get; set; } = new();
}
