using Abp.Application.Services;
using Abp.Application.Services.Dto;
using backend.Services.LegalDocumentService.DTO;
using System;
using System.IO;
using System.Threading.Tasks;

namespace backend.Services.LegalDocumentService;

/// <summary>
/// Application service contract for LegalDocument CRUD operations and PDF ingestion triggering.
/// ABP auto-exposes all methods as REST endpoints via dynamic web API.
/// </summary>
public interface ILegalDocumentAppService
    : IAsyncCrudAppService<LegalDocumentDto, Guid, PagedAndSortedResultRequestDto, LegalDocumentDto, LegalDocumentDto>
{
    /// <summary>
    /// Runs the full PDF ingestion pipeline for an existing LegalDocument:
    /// creates an IngestionJob, chunks the PDF, persists chunks, and marks the document as processed.
    /// </summary>
    Task TriggerIngestionAsync(Guid documentId, Stream pdfStream, string actName);
}
