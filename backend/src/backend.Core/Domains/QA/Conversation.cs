using Abp.Domain.Entities.Auditing;
using backend.Domains.LegalDocuments;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Domains.QA;

/// <summary>
/// Represents a single legal assistance session initiated by a registered user.
/// A Conversation owns one or more Questions and may optionally be published as a public FAQ.
/// Every Conversation is mandatory-linked to an AppUser — anonymous sessions are not permitted.
/// </summary>
public class Conversation : FullAuditedEntity<Guid>
{
    /// <summary>
    /// Foreign key to the ABP Zero AppUser who owns this conversation.
    /// Typed as <c>long</c> to match ABP Zero's User primary key type.
    /// </summary>
    [Required]
    public long UserId { get; set; }

    /// <summary>
    /// Preferred language of this conversation, applied to all answers unless overridden per-question.
    /// Must be one of the four constitutionally-required SA languages.
    /// </summary>
    [Required]
    public Language Language { get; set; }

    /// <summary>
    /// Primary input method for this conversation (Text or Voice).
    /// Individual questions may override this on their own <see cref="InputMethod"/> property.
    /// </summary>
    [Required]
    public InputMethod InputMethod { get; set; }

    /// <summary>UTC timestamp recording when this conversation was opened by the user.</summary>
    [Required]
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// When <c>true</c>, this conversation is published as a curated public FAQ entry
    /// visible to all users. Defaults to <c>false</c> — conversations are private by default.
    /// </summary>
    public bool IsPublicFaq { get; set; }

    /// <summary>
    /// Optional foreign key to a <see cref="Category"/> used to classify this conversation
    /// when it is published as a public FAQ. Null when <see cref="IsPublicFaq"/> is <c>false</c>.
    /// </summary>
    public Guid? FaqCategoryId { get; set; }

    /// <summary>Navigation property to the FAQ category. Null unless this is a public FAQ.</summary>
    [ForeignKey(nameof(FaqCategoryId))]
    public virtual Category FaqCategory { get; set; }

    /// <summary>
    /// Ordered collection of questions submitted within this conversation.
    /// Loaded via Include; not populated on lightweight queries.
    /// </summary>
    public virtual ICollection<Question> Questions { get; set; }
}
