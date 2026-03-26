using Abp.Application.Services;
using backend.Authorization.Accounts.Dto;
using System.Threading.Tasks;

namespace backend.Authorization.Accounts;

public interface IAccountAppService : IApplicationService
{
    Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

    Task<RegisterOutput> Register(RegisterInput input);
}


