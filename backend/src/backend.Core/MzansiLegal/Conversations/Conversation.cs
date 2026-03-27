using Abp.Domain.Entities;
using backend.Authorization.Users;
using backend.MzansiLegal.Categories;
using backend.MzansiLegal.RefLists;
using System;
using System.Collections.Generic;

namespace backend.MzansiLegal.Conversations;

public class Conversation : Entity<Guid>
{
    public long AppUserId { get; set; }
    public virtual User AppUser { get; set; }

    public Language Language { get; set; }
    public InputMethod InputMethod { get; set; }
    public DateTime StartedAt { get; set; }
    public bool IsPublicFaq { get; set; }
    public Guid? FaqCategoryId { get; set; }
    public virtual Category FaqCategory { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    protected Conversation() { }

    public Conversation(Guid id, long appUserId, Language language, InputMethod inputMethod)
    {
        Id = id;
        AppUserId = appUserId;
        Language = language;
        InputMethod = inputMethod;
        StartedAt = DateTime.UtcNow;
        IsPublicFaq = false;
    }
}
