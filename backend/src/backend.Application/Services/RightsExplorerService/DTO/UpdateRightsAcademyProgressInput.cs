using System.Collections.Generic;

namespace backend.Services.RightsExplorerService.DTO;

public class UpdateRightsAcademyProgressInput
{
    public List<string> ExploredLessonIds { get; set; } = new();
}
