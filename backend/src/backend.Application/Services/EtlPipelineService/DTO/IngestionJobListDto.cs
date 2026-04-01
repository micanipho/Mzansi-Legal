using backend.Domains.ETL;
using System;

namespace backend.Services.EtlPipelineService.DTO;

/// <summary>
/// Lightweight view of an IngestionJob for the admin jobs list.
/// Omits per-stage timestamps to reduce payload size.
/// </summary>
public class IngestionJobListDto
{
    /// <summary>Unique identifier of the ingestion job.</summary>
    public Guid Id { get; set; }

    /// <summary>ID of the LegalDocument being ingested.</summary>
    public Guid DocumentId { get; set; }

    /// <summary>Title of the LegalDocument — joined at query time for display.</summary>
    public string DocumentTitle { get; set; }

    /// <summary>Current stage of the pipeline.</summary>
    public IngestionStatus Status { get; set; }

    /// <summary>UTC timestamp when the pipeline started.</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>UTC timestamp when the job reached a terminal state. Null while in progress.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// End-to-end wall-clock duration. Null while the job is still in progress.
    /// </summary>
    public TimeSpan? TotalDuration =>
        StartedAt.HasValue && CompletedAt.HasValue
            ? CompletedAt.Value - StartedAt.Value
            : (TimeSpan?)null;

    /// <summary>Number of DocumentChunk entities saved to the database.</summary>
    public int ChunksLoaded { get; set; }

    /// <summary>Number of ChunkEmbedding records generated and saved.</summary>
    public int EmbeddingsGenerated { get; set; }

    /// <summary>Human-readable error message when Status = Failed. Null otherwise.</summary>
    public string ErrorMessage { get; set; }
}
