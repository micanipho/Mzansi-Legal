using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.UI;
using Ardalis.GuardClauses;
using backend.Domains.ETL;
using backend.Domains.LegalDocuments;
using backend.Services.ChunkEnrichmentService;
using backend.Services.EmbeddingService;
using backend.Services.EtlPipelineService.DTO;
using backend.Services.PdfIngestionService;
using backend.Services.PdfIngestionService.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services.EtlPipelineService;

/// <summary>
/// Orchestrates the full ETL ingestion flow for legislation PDFs:
/// trigger, enrich, embed, persist, inspect, and retry pipeline jobs.
/// </summary>
[AbpAuthorize]
public class EtlPipelineAppService : ApplicationService, IEtlPipelineAppService
{
    private readonly IRepository<IngestionJob, Guid> _jobRepository;
    private readonly IRepository<LegalDocument, Guid> _documentRepository;
    private readonly IRepository<DocumentChunk, Guid> _chunkRepository;
    private readonly IRepository<ChunkEmbedding, Guid> _embeddingRepository;
    private readonly IPdfIngestionAppService _pdfIngestionAppService;
    private readonly IEmbeddingAppService _embeddingAppService;
    private readonly IChunkEnrichmentAppService _chunkEnrichmentAppService;

    /// <summary>
    /// Initialises the service with all ETL orchestration dependencies.
    /// </summary>
    public EtlPipelineAppService(
        IRepository<IngestionJob, Guid> jobRepository,
        IRepository<LegalDocument, Guid> documentRepository,
        IRepository<DocumentChunk, Guid> chunkRepository,
        IRepository<ChunkEmbedding, Guid> embeddingRepository,
        IPdfIngestionAppService pdfIngestionAppService,
        IEmbeddingAppService embeddingAppService,
        IChunkEnrichmentAppService chunkEnrichmentAppService)
    {
        _jobRepository = jobRepository;
        _documentRepository = documentRepository;
        _chunkRepository = chunkRepository;
        _embeddingRepository = embeddingRepository;
        _pdfIngestionAppService = pdfIngestionAppService;
        _embeddingAppService = embeddingAppService;
        _chunkEnrichmentAppService = chunkEnrichmentAppService;
    }

    /// <summary>
    /// Triggers a new ETL ingestion job for the specified document.
    /// Rejects duplicate active jobs and documents without an uploaded PDF.
    /// </summary>
    public async Task<IngestionJobDto> TriggerAsync(Guid documentId)
    {
        Guard.Against.Default(documentId, nameof(documentId));

        var document = await _documentRepository.FirstOrDefaultAsync(documentId);
        if (document == null)
        {
            throw new EntityNotFoundException(typeof(LegalDocument), documentId);
        }

        if (document.OriginalPdfId == null && string.IsNullOrWhiteSpace(document.FileName))
        {
            throw new UserFriendlyException("Document has no uploaded PDF file.");
        }

        var hasActiveJob = _jobRepository.GetAll()
            .Any(job =>
                job.DocumentId == documentId &&
                job.Status != IngestionStatus.Completed &&
                job.Status != IngestionStatus.Failed);

        if (hasActiveJob)
        {
            throw new UserFriendlyException("A pipeline job is already active for this document.");
        }

        var job = new IngestionJob
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            Status = IngestionStatus.Queued,
            TriggeredByUserId = AbpSession.UserId
        };

        await _jobRepository.InsertAsync(job);
        // Flush the insert to the DB so PdfIngestionAppService.IngestAsync can find
        // the job via GetAsync. EF Core change-tracker "Added" state is not visible
        // to a SQL query issued by a nested ApplicationService call.
        await UnitOfWorkManager.Current.SaveChangesAsync();
        return await ExecutePipelineAsync(document, job);
    }

    /// <summary>
    /// Retries a failed ETL job from scratch after clearing any partial chunk state.
    /// </summary>
    public async Task<IngestionJobDto> RetryAsync(Guid jobId)
    {
        Guard.Against.Default(jobId, nameof(jobId));

        var job = await _jobRepository.FirstOrDefaultAsync(jobId);
        if (job == null)
        {
            throw new EntityNotFoundException(typeof(IngestionJob), jobId);
        }

        if (job.Status != IngestionStatus.Failed)
        {
            throw new UserFriendlyException("Only failed ingestion jobs can be retried.");
        }

        var document = await _documentRepository.GetAsync(job.DocumentId);
        var chunks = _chunkRepository.GetAll()
            .Where(chunk => chunk.DocumentId == job.DocumentId)
            .ToList();

        foreach (var chunk in chunks)
        {
            await _chunkRepository.DeleteAsync(chunk);
        }

        ResetJobForRetry(job);
        document.IsProcessed = false;
        document.TotalChunks = 0;

        await _documentRepository.UpdateAsync(document);
        await _jobRepository.UpdateAsync(job);

        return await ExecutePipelineAsync(document, job);
    }

    /// <summary>
    /// Returns all ingestion jobs ordered by newest first with lightweight summary data.
    /// </summary>
    public Task<System.Collections.Generic.List<IngestionJobListDto>> GetJobsAsync()
    {
        var items = (
            from job in _jobRepository.GetAll()
            join document in _documentRepository.GetAll() on job.DocumentId equals document.Id
            orderby job.CreationTime descending
            select new { job, document.Title }
        ).ToList();

        var result = items.Select(item => new IngestionJobListDto
        {
            Id = item.job.Id,
            DocumentId = item.job.DocumentId,
            DocumentTitle = item.Title,
            Status = item.job.Status,
            StartedAt = item.job.ExtractStartedAt ?? item.job.CreationTime,
            CompletedAt = item.job.CompletedAt,
            ChunksLoaded = item.job.ChunksLoaded,
            EmbeddingsGenerated = item.job.EmbeddingsGenerated,
            ErrorMessage = item.job.ErrorMessage
        }).ToList();

        return Task.FromResult(result);
    }

    /// <summary>
    /// Returns the full detail of a single ingestion job.
    /// </summary>
    public Task<IngestionJobDto> GetJobAsync(Guid id)
    {
        Guard.Against.Default(id, nameof(id));

        var item = (
            from job in _jobRepository.GetAll().Where(x => x.Id == id)
            join document in _documentRepository.GetAll() on job.DocumentId equals document.Id
            select new { job, document.Title }
        ).FirstOrDefault();

        if (item == null)
        {
            throw new EntityNotFoundException(typeof(IngestionJob), id);
        }

        return Task.FromResult(MapToDetailDto(item.job, item.Title));
    }

    private async Task<IngestionJobDto> ExecutePipelineAsync(LegalDocument document, IngestionJob job)
    {
        try
        {
            using var pdfStream = await GetPdfStreamAsync(document);
            var chunks = await _pdfIngestionAppService.IngestAsync(new IngestPdfRequest
            {
                PdfStream = pdfStream,
                ActName = document.Title,
                DocumentId = document.Id,
                IngestionJobId = job.Id
            });

            if (chunks.Count == 0)
            {
                document.IsProcessed = false;
                document.TotalChunks = 0;
                job.Status = IngestionStatus.Completed;
                job.CompletedAt = DateTime.UtcNow;
                job.LoadCompletedAt ??= job.CompletedAt;
                job.ErrorMessage = "Warning: PDF contained no extractable text; no chunks were produced.";
                await _documentRepository.UpdateAsync(document);
                await _jobRepository.UpdateAsync(job);
                return MapToDetailDto(job, document.Title);
            }

            foreach (var chunkResult in chunks)
            {
                await PersistChunkAsync(document.Id, chunkResult, job);
            }

            document.IsProcessed = true;
            document.TotalChunks = job.ChunksLoaded;
            job.Status = IngestionStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
            job.LoadCompletedAt = job.CompletedAt;
            job.ErrorMessage = null;

            await _documentRepository.UpdateAsync(document);
            await _jobRepository.UpdateAsync(job);

            return MapToDetailDto(job, document.Title);
        }
        catch (Exception ex)
        {
            await MarkJobFailedAsync(job, ex);
            throw;
        }
    }

    private Task<Stream> GetPdfStreamAsync(LegalDocument document)
    {
        var filePath = FindSeedDataFile(document.FileName);
        if (filePath == null)
        {
            throw new UserFriendlyException("Stored PDF file could not be found.");
        }

        return Task.FromResult<Stream>(new MemoryStream(File.ReadAllBytes(filePath), writable: false));
    }

    private async Task PersistChunkAsync(Guid documentId, DocumentChunkResult chunkResult, IngestionJob job)
    {
        var enrichment = await _chunkEnrichmentAppService.EnrichAsync(chunkResult.Content);
        var embedding = await _embeddingAppService.GenerateEmbeddingAsync(chunkResult.Content);

        if (embedding.Vector == null || embedding.Vector.Length != ChunkEmbedding.EmbeddingDimension)
        {
            throw new UserFriendlyException(
                $"Embedding vector dimension mismatch. Expected {ChunkEmbedding.EmbeddingDimension} values.");
        }

        var chunk = new DocumentChunk
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            ChapterTitle = chunkResult.ChapterTitle,
            SectionNumber = chunkResult.SectionNumber,
            SectionTitle = chunkResult.SectionTitle,
            Content = chunkResult.Content,
            TokenCount = chunkResult.TokenCount,
            SortOrder = chunkResult.SortOrder,
            ChunkStrategy = chunkResult.Strategy,
            Keywords = TrimToLength(enrichment.Keywords, 500),
            TopicClassification = TrimToLength(enrichment.TopicClassification, 200)
        };

        await _chunkRepository.InsertAsync(chunk);
        await _embeddingRepository.InsertAsync(new ChunkEmbedding
        {
            Id = Guid.NewGuid(),
            ChunkId = chunk.Id,
            Vector = embedding.Vector
        });

        job.ChunksLoaded += 1;
        job.EmbeddingsGenerated += 1;
        await _jobRepository.UpdateAsync(job);
    }

    private async Task MarkJobFailedAsync(IngestionJob job, Exception ex)
    {
        job.Status = IngestionStatus.Failed;
        job.CompletedAt = DateTime.UtcNow;
        job.ErrorMessage ??= ex.Message;
        await _jobRepository.UpdateAsync(job);
    }

    private static void ResetJobForRetry(IngestionJob job)
    {
        job.Status = IngestionStatus.Queued;
        job.ErrorMessage = null;
        job.ExtractStartedAt = null;
        job.ExtractCompletedAt = null;
        job.ExtractedCharacterCount = 0;
        job.TransformStartedAt = null;
        job.TransformCompletedAt = null;
        job.ChunksProduced = 0;
        job.LoadStartedAt = null;
        job.LoadCompletedAt = null;
        job.ChunksLoaded = 0;
        job.EmbeddingsGenerated = 0;
        job.CompletedAt = null;
        job.Strategy = null;
    }

    private static string TrimToLength(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

    private static string FindSeedDataFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            var candidates = new List<string>
            {
                Path.Combine(current.FullName, "seed-data", "legislation", fileName),
                Path.Combine(current.FullName, "seed-data", "financial", fileName)
            };

            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            current = current.Parent;
        }

        return null;
    }

    private static IngestionJobDto MapToDetailDto(IngestionJob job, string documentTitle)
    {
        return new IngestionJobDto
        {
            Id = job.Id,
            DocumentId = job.DocumentId,
            DocumentTitle = documentTitle,
            Status = job.Status,
            TriggeredByUserId = job.TriggeredByUserId,
            StartedAt = job.ExtractStartedAt ?? job.CreationTime,
            CompletedAt = job.CompletedAt,
            ErrorMessage = job.ErrorMessage,
            ExtractStartedAt = job.ExtractStartedAt,
            ExtractCompletedAt = job.ExtractCompletedAt,
            ExtractedCharacterCount = job.ExtractedCharacterCount,
            TransformStartedAt = job.TransformStartedAt,
            TransformCompletedAt = job.TransformCompletedAt,
            ChunksProduced = job.ChunksProduced,
            Strategy = job.Strategy,
            LoadStartedAt = job.LoadStartedAt,
            LoadCompletedAt = job.LoadCompletedAt,
            ChunksLoaded = job.ChunksLoaded,
            EmbeddingsGenerated = job.EmbeddingsGenerated
        };
    }
}
