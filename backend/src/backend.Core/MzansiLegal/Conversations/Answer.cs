using Abp.Domain.Entities;
using backend.MzansiLegal.RefLists;
using System;
using System.Collections.Generic;

namespace backend.MzansiLegal.Conversations;

public class Answer : Entity<Guid>
{
    public Guid QuestionId { get; set; }
    public virtual Question Question { get; set; }

    public string Text { get; set; }
    public Language Language { get; set; }
    public string AudioFilePath { get; set; }
    public bool? IsAccurate { get; set; }
    public string AdminNotes { get; set; }

    public virtual ICollection<AnswerCitation> Citations { get; set; } = new List<AnswerCitation>();

    protected Answer() { }

    public Answer(Guid id, Guid questionId, string text, Language language)
    {
        Id = id;
        QuestionId = questionId;
        Text = text;
        Language = language;
    }
}
