using Abp.Application.Services;
using backend.Sessions.Dto;
using System.Threading.Tasks;

namespace backend.Sessions;

public interface ISessionAppService : IApplicationService
{
    Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
}


