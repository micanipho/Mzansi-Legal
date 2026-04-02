using System.Collections.Generic;

namespace backend.Services.RightsExplorerService.DTO;

public class RightsAcademyProgressDto
{
    public List<string> ExploredLessonIds { get; set; } = new();
}
