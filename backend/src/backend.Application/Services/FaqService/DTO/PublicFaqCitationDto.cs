using System;

namespace backend.Services.FaqService.DTO;

/// <summary>
/// Lightweight citation DTO exposed to public FAQ consumers such as the rights explorer.
/// </summary>
public class PublicFaqCitationDto
{
    /// <summary>The backing citation entity identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Full legislation title for display.</summary>
    public string ActName { get; set; }

    /// <summary>Section or locator string for the cited source.</summary>
    public string SectionNumber { get; set; }

    /// <summary>Quoted supporting excerpt from the cited source.</summary>
    public string Excerpt { get; set; }

    /// <summary>Retrieval relevance score recorded when the answer was grounded.</summary>
    public decimal RelevanceScore { get; set; }
}
