using Abp.Application.Services;
using backend.MultiTenancy.Dto;

namespace backend.MultiTenancy;

public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
{
}



