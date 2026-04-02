using Abp.Application.Services;
using backend.Configuration;
using backend.Services.RightsExplorerService.DTO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace backend.Services.RightsExplorerService;

/// <summary>
/// Returns the seeded rights academy catalog so the public explorer loads quickly.
/// </summary>
public class RightsAcademyAppService : ApplicationService, IRightsAcademyAppService
{
    public async Task<RightsAcademyDto> GetAcademyAsync()
    {
        var json = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.RightsAcademyCatalog);
        if (string.IsNullOrWhiteSpace(json))
        {
            json = RightsAcademySeedData.GetCatalogJson();
        }

        var result = JsonSerializer.Deserialize<RightsAcademyDto>(json, RightsAcademySeedData.SerializerOptions) ?? new RightsAcademyDto();
        result.Tracks = result.Tracks
            .Where(track => track != null)
            .OrderBy(track => track.SortOrder)
            .ThenBy(track => track.CategoryName)
            .ToList();
        result.TotalLessons = result.Tracks.Sum(track => track.Lessons?.Count ?? 0);

        foreach (var track in result.Tracks)
        {
            track.Lessons = (track.Lessons ?? [])
                .Where(lesson => lesson != null && !string.IsNullOrWhiteSpace(lesson.Id))
                .ToList();
        }

        return result;
    }
}
