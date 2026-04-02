using Abp.Application.Services;
using backend.Services.RightsExplorerService.DTO;
using System.Threading.Tasks;

namespace backend.Services.RightsExplorerService;

public interface IRightsAcademyProgressAppService : IApplicationService
{
    Task<RightsAcademyProgressDto> GetProgressAsync();

    Task<RightsAcademyProgressDto> UpdateProgressAsync(UpdateRightsAcademyProgressInput input);
}
