using Abp.Application.Services;
using backend.Services.RightsExplorerService.DTO;
using System.Threading.Tasks;

namespace backend.Services.RightsExplorerService;

/// <summary>
/// Provides legislation-backed learning content for the rights academy.
/// </summary>
public interface IRightsAcademyAppService : IApplicationService
{
    Task<RightsAcademyDto> GetAcademyAsync();
}
