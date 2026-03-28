using Abp.Application.Services;
using Abp.Application.Services.Dto;
using backend.Services.LegalDocumentService.DTO;
using System;

namespace backend.Services.LegalDocumentService;

/// <summary>
/// Application service contract for LegalDocument CRUD operations.
/// ABP auto-exposes all methods as REST endpoints via dynamic web API.
/// </summary>
public interface ILegalDocumentAppService
    : IAsyncCrudAppService<LegalDocumentDto, Guid, PagedAndSortedResultRequestDto, LegalDocumentDto, LegalDocumentDto>
{
}
