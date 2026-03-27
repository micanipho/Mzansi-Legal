using Abp.Domain.Entities;
using backend.MzansiLegal.RefLists;
using System;

namespace backend.MzansiLegal.Conversations;

public class Question : Entity<Guid>
{
    public Guid ConversationId { get; set; }
    public virtual Conversation Conversation { get; set; }

    public string OriginalText { get; set; }
    public string TranslatedText { get; set; }
    public Language Language { get; set; }
    public InputMethod InputMethod { get; set; }
    public string AudioFilePath { get; set; }

    public virtual Answer Answer { get; set; }

    protected Question() { }

    public Question(Guid id, Guid conversationId, string originalText, Language language, InputMethod inputMethod)
    {
        Id = id;
        ConversationId = conversationId;
        OriginalText = originalText;
        Language = language;
        InputMethod = inputMethod;
    }
}
