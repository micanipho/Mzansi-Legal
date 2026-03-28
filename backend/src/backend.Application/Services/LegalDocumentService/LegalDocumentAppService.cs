using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using backend.Domains.ETL;
using backend.Domains.LegalDocuments;
using backend.Services.LegalDocumentService.DTO;
using backend.Services.PdfIngestionService;
using backend.Services.PdfIngestionService.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services.LegalDocumentService;

/// <summary>
/// Handles CRUD operations for LegalDocument entities and orchestrates the PDF ingestion pipeline.
/// ABP automatically exposes CRUD operations as a REST API under /api/services/app/legalDocument/.
/// List queries return LegalDocumentListDto (no FullText) to reduce payload size.
/// Single-record reads return the full LegalDocumentDto including FullText.
/// </summary>
[AbpAuthorize]
public class LegalDocumentAppService
    : AsyncCrudAppService<LegalDocument, LegalDocumentDto, Guid, PagedAndSortedResultRequestDto, LegalDocumentDto, LegalDocumentDto>,
      ILegalDocumentAppService
{
    private readonly IPdfIngestionAppService _pdfIngestionAppService;
    private readonly IRepository<IngestionJob, Guid> _ingestionJobRepository;
    private readonly IRepository<DocumentChunk, Guid> _chunkRepository;

    /// <summary>Initialises the service with the required repositories and the PDF ingestion service.</summary>
    public LegalDocumentAppService(
        IRepository<LegalDocument, Guid> repository,
        IPdfIngestionAppService pdfIngestionAppService,
        IRepository<IngestionJob, Guid> ingestionJobRepository,
        IRepository<DocumentChunk, Guid> chunkRepository)
        : base(repository)
    {
        _pdfIngestionAppService  = pdfIngestionAppService;
        _ingestionJobRepository  = ingestionJobRepository;
        _chunkRepository         = chunkRepository;
    }

    /// <summary>
    /// Orchestrates the full PDF ingestion pipeline for a LegalDocument:
    ///   1. Creates an IngestionJob (Status = Queued).
    ///   2. Calls PdfIngestionAppService.IngestAsync, which transitions the job through
    ///      Extracting → Transforming → Loading and returns DocumentChunkResults.
    ///   3. Persists each result as a DocumentChunk entity.
    ///   4. Updates LegalDocument.IsProcessed and TotalChunks.
    ///   5. Marks the IngestionJob as Completed.
    /// On an empty result (no extractable text) the document is left with IsProcessed = false.
    /// </summary>
    public async Task TriggerIngestionAsync(Guid documentId, Stream pdfStream, string actName)
    {
        var job = new IngestionJob { DocumentId = documentId, Status = IngestionStatus.Queued };
        await _ingestionJobRepository.InsertAsync(job);
        await CurrentUnitOfWork.SaveChangesAsync();

        var request = new IngestPdfRequest
        {
            PdfStream      = pdfStream,
            ActName        = actName,
            DocumentId     = documentId,
            IngestionJobId = job.Id
        };

        var chunks = await _pdfIngestionAppService.IngestAsync(request);

        foreach (var chunk in chunks)
        {
            await _chunkRepository.InsertAsync(new DocumentChunk
            {
                DocumentId    = documentId,
                ChapterTitle  = chunk.ChapterTitle,
                SectionNumber = chunk.SectionNumber,
                SectionTitle  = chunk.SectionTitle,
                Content       = chunk.Content,
                TokenCount    = chunk.TokenCount,
                SortOrder     = chunk.SortOrder,
                ChunkStrategy = chunk.Strategy
            });
        }

        if (chunks.Count > 0)
        {
            var document = await Repository.GetAsync(documentId);
            document.IsProcessed = true;
            document.TotalChunks = chunks.Count;
            await Repository.UpdateAsync(document);

            job.ChunksLoaded    = chunks.Count;
            job.LoadCompletedAt = DateTime.UtcNow;
            job.Status          = IngestionStatus.Completed;
            await _ingestionJobRepository.UpdateAsync(job);
        }
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
