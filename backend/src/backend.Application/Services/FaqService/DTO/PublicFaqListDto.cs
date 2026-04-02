using System.Collections.Generic;

namespace backend.Services.FaqService.DTO;

/// <summary>
/// Wrapper for the public FAQ explorer feed.
/// </summary>
public class PublicFaqListDto
{
    /// <summary>Ordered FAQ cards available to public clients.</summary>
    public List<PublicFaqItemDto> Items { get; set; } = new();

    /// <summary>Total number of returned FAQ cards.</summary>
    public int TotalCount { get; set; }
}
