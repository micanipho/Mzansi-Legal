using Abp.Application.Services;
using backend.Services.EtlPipelineService.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services.EtlPipelineService;

/// <summary>
/// Orchestrates the full ETL pipeline for legislation PDF ingestion:
/// Extract (PdfPig text extraction) → Transform (section chunking) →
/// Enrich (LLM keywords/topic) → Load (OpenAI embeddings + persistence).
/// Tracks every stage via IngestionJob with per-stage timing and counts.
/// </summary>
public interface IEtlPipelineAppService : IApplicationService
{
    /// <summary>
    /// Triggers a full ETL pipeline run for the specified legal document.
    /// Creates an IngestionJob, runs all pipeline stages, and returns the job record
    /// in its terminal state (Completed or Failed).
    /// </summary>
    /// <param name="documentId">ID of the LegalDocument to process.</param>
    /// <returns>The IngestionJobDto reflecting the final pipeline state.</returns>
    /// <exception cref="Abp.Domain.Entities.EntityNotFoundException">
    /// Thrown when the document does not exist.
    /// </exception>
    /// <exception cref="Abp.UI.UserFriendlyException">
    /// Thrown when an active job already exists for this document,
    /// or when the document has no uploaded PDF file.
    /// </exception>
    Task<IngestionJobDto> TriggerAsync(Guid documentId);

    /// <summary>
    /// Retries a failed ingestion job from the beginning (full pipeline re-run).
    /// Deletes any partial chunks from the previous run before re-running.
    /// </summary>
    /// <param name="jobId">ID of the IngestionJob to retry. Must be in Failed status.</param>
    /// <returns>The IngestionJobDto reflecting the final pipeline state after retry.</returns>
    /// <exception cref="Abp.Domain.Entities.EntityNotFoundException">
    /// Thrown when the job does not exist.
    /// </exception>
    /// <exception cref="Abp.UI.UserFriendlyException">
    /// Thrown when the job status is not Failed.
    /// </exception>
    Task<IngestionJobDto> RetryAsync(Guid jobId);

    /// <summary>
    /// Returns all ingestion jobs ordered by creation time descending.
    /// Each entry includes document title, current status, and duration summary.
    /// </summary>
    Task<List<IngestionJobListDto>> GetJobsAsync();

    /// <summary>
    /// Returns the full detail of a single ingestion job including per-stage durations,
    /// chunk counts, embeddings generated, and error information.
    /// </summary>
    /// <param name="id">ID of the IngestionJob to retrieve.</param>
    /// <exception cref="Abp.Domain.Entities.EntityNotFoundException">
    /// Thrown when the job does not exist.
    /// </exception>
    Task<IngestionJobDto> GetJobAsync(Guid id);
}
