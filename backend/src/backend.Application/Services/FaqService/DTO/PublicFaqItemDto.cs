using System;
using System.Collections.Generic;

namespace backend.Services.FaqService.DTO;

/// <summary>
/// Represents one curated public FAQ entry rendered as a rights explorer card.
/// </summary>
public class PublicFaqItemDto
{
    /// <summary>Stable card identifier. Uses the conversation ID so progress survives answer refreshes.</summary>
    public Guid Id { get; set; }

    /// <summary>Conversation identifier for the underlying curated FAQ thread.</summary>
    public Guid ConversationId { get; set; }

    /// <summary>Question identifier paired with the published answer.</summary>
    public Guid QuestionId { get; set; }

    /// <summary>Answer identifier for the published FAQ answer.</summary>
    public Guid AnswerId { get; set; }

    /// <summary>Optional category identifier used for filtering.</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Stored FAQ category display name.</summary>
    public string CategoryName { get; set; }

    /// <summary>Frontend-friendly category key such as <c>employment</c> or <c>housing</c>.</summary>
    public string TopicKey { get; set; }

    /// <summary>Question text displayed as the card title.</summary>
    public string Title { get; set; }

    /// <summary>Short one-line summary derived from the approved answer.</summary>
    public string Summary { get; set; }

    /// <summary>Full approved answer text shown when the card expands.</summary>
    public string Explanation { get; set; }

    /// <summary>Highlighted quote box text, usually sourced from the strongest citation excerpt.</summary>
    public string SourceQuote { get; set; }

    /// <summary>Primary display citation combining act name and section.</summary>
    public string PrimaryCitation { get; set; }

    /// <summary>Language code of the stored FAQ content.</summary>
    public string LanguageCode { get; set; }

    /// <summary>Timestamp of the approved answer that powers this FAQ card.</summary>
    public DateTime PublishedAt { get; set; }

    /// <summary>All citations attached to the approved answer, ordered by relevance.</summary>
    public List<PublicFaqCitationDto> Citations { get; set; } = new();
}
