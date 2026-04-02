using backend.Services.FaqService.DTO;
using System;
using System.Collections.Generic;

namespace backend.Services.RightsExplorerService.DTO;

/// <summary>
/// One legislation-backed learning card in the rights academy.
/// </summary>
public class RightsAcademyLessonDto
{
    public string Id { get; set; }

    public Guid DocumentId { get; set; }

    public string TopicKey { get; set; }

    public string CategoryName { get; set; }

    public string Title { get; set; }

    public string LawShortName { get; set; }

    public string LawTitle { get; set; }

    public string Summary { get; set; }

    public string Explanation { get; set; }

    public string SourceQuote { get; set; }

    public string PrimaryCitation { get; set; }

    public string AskQuery { get; set; }

    public List<PublicFaqCitationDto> Citations { get; set; } = new();
}
