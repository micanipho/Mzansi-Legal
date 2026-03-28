using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using backend.Domains.LegalDocuments;
using backend.Services.LegalDocumentService.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services.LegalDocumentService;

/// <summary>
/// Handles CRUD operations for LegalDocument entities.
/// ABP automatically exposes this as a REST API under /api/services/app/legalDocument/.
/// List queries return LegalDocumentListDto (no FullText) to reduce payload size.
/// Single-record reads return the full LegalDocumentDto including FullText.
/// </summary>
[AbpAuthorize]
public class LegalDocumentAppService
    : AsyncCrudAppService<LegalDocument, LegalDocumentDto, Guid, PagedAndSortedResultRequestDto, LegalDocumentDto, LegalDocumentDto>,
      ILegalDocumentAppService
{
    /// <summary>Initialises the service with the ABP-injected LegalDocument repository.</summary>
    public LegalDocumentAppService(IRepository<LegalDocument, Guid> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// Returns a paged list of legal documents without FullText to keep the payload small.
    /// Consumers needing full content should call Get with the document Id.
    /// </summary>
    public override async Task<PagedResultDto<LegalDocumentDto>> GetAllAsync(PagedAndSortedResultRequestDto input)
    {
        var query = Repository.GetAll();
        var totalCount = await query.CountAsync();

        var documents = await query
            .OrderBy(d => d.Title)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .Select(d => new LegalDocumentDto
            {
                Id = d.Id,
                Title = d.Title,
                ShortName = d.ShortName,
                ActNumber = d.ActNumber,
                Year = d.Year,
                FileName = d.FileName,
                OriginalPdfId = d.OriginalPdfId,
                CategoryId = d.CategoryId,
                IsProcessed = d.IsProcessed,
                TotalChunks = d.TotalChunks
                // FullText intentionally excluded from list results
            })
            .ToListAsync();

        return new PagedResultDto<LegalDocumentDto>(totalCount, documents);
    }
}
