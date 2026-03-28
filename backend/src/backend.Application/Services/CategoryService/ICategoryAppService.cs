using Abp.Application.Services;
using Abp.Application.Services.Dto;
using backend.Services.CategoryService.DTO;
using System;

namespace backend.Services.CategoryService;

/// <summary>
/// Application service contract for Category CRUD operations.
/// ABP auto-exposes all methods as REST endpoints via dynamic web API.
/// </summary>
public interface ICategoryAppService
    : IAsyncCrudAppService<CategoryDto, Guid, PagedAndSortedResultRequestDto, CategoryDto, CategoryDto>
{
}
