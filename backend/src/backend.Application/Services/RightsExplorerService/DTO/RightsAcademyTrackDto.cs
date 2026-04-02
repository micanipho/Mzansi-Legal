using System.Collections.Generic;

namespace backend.Services.RightsExplorerService.DTO;

/// <summary>
/// Groups academy lessons under one public-facing rights topic.
/// </summary>
public class RightsAcademyTrackDto
{
    public string TopicKey { get; set; }

    public string CategoryName { get; set; }

    public int SortOrder { get; set; }

    public List<RightsAcademyLessonDto> Lessons { get; set; } = new();
}
