using System.Collections.Generic;

namespace backend.Services.RightsExplorerService.DTO;

/// <summary>
/// Public response for the rights academy explorer feed.
/// </summary>
public class RightsAcademyDto
{
    public List<RightsAcademyTrackDto> Tracks { get; set; } = new();

    public int TotalLessons { get; set; }
}
