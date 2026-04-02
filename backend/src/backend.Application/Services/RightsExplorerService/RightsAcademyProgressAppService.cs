using Abp;
using Abp.Authorization;
using Abp.Runtime.Session;
using backend.Configuration;
using backend.Services.RightsExplorerService.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace backend.Services.RightsExplorerService;

[AbpAuthorize]
public class RightsAcademyProgressAppService : backendAppServiceBase, IRightsAcademyProgressAppService
{
    public async Task<RightsAcademyProgressDto> GetProgressAsync()
    {
        var user = GetCurrentUserIdentifier();
        var json = await SettingManager.GetSettingValueForUserAsync(AppSettingNames.RightsAcademyProgress, user);
        return DeserializeProgress(json);
    }

    public async Task<RightsAcademyProgressDto> UpdateProgressAsync(UpdateRightsAcademyProgressInput input)
    {
        var result = new RightsAcademyProgressDto
        {
            ExploredLessonIds = NormalizeIds(input?.ExploredLessonIds)
        };

        var json = JsonSerializer.Serialize(result.ExploredLessonIds, RightsAcademySeedData.SerializerOptions);
        await SettingManager.ChangeSettingForUserAsync(GetCurrentUserIdentifier(), AppSettingNames.RightsAcademyProgress, json);

        return result;
    }

    private UserIdentifier GetCurrentUserIdentifier()
    {
        return new UserIdentifier(AbpSession.TenantId, AbpSession.GetUserId());
    }

    private static RightsAcademyProgressDto DeserializeProgress(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new RightsAcademyProgressDto();
        }

        var ids = JsonSerializer.Deserialize<List<string>>(json, RightsAcademySeedData.SerializerOptions) ?? new List<string>();
        return new RightsAcademyProgressDto
        {
            ExploredLessonIds = NormalizeIds(ids)
        };
    }

    private static List<string> NormalizeIds(IEnumerable<string> exploredLessonIds)
    {
        return (exploredLessonIds ?? [])
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct()
            .ToList();
    }
}
