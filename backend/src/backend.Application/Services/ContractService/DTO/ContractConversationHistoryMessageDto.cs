namespace backend.Services.ContractService.DTO;

/// <summary>
/// One contract follow-up message used to preserve conversational context.
/// </summary>
public class ContractConversationHistoryMessageDto
{
    public string Role { get; set; }

    public string Text { get; set; }
}
