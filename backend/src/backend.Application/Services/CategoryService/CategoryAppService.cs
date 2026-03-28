using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using backend.Domains.LegalDocuments;
using backend.Services.CategoryService.DTO;
using System;

namespace backend.Services.CategoryService;

/// <summary>
/// Handles CRUD operations for Category entities.
/// ABP automatically exposes this as a REST API under /api/services/app/category/.
/// </summary>
[AbpAuthorize]
public class CategoryAppService
    : AsyncCrudAppService<Category, CategoryDto, Guid, PagedAndSortedResultRequestDto, CategoryDto, CategoryDto>,
      ICategoryAppService
{
    /// <summary>Initialises the service with the ABP-injected Category repository.</summary>
    public CategoryAppService(IRepository<Category, Guid> repository)
        : base(repository)
    {
    }
}
