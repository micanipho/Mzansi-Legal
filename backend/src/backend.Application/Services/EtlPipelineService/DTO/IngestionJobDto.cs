using backend.Domains.ETL;
using backend.Domains.LegalDocuments;
using System;

namespace backend.Services.EtlPipelineService.DTO;

/// <summary>
/// Full detail view of an IngestionJob, returned by TriggerAsync, GetJobAsync, and RetryAsync.
/// Duration properties are computed from paired stage timestamps rather than stored separately.
/// </summary>
public class IngestionJobDto
{
    /// <summary>Unique identifier of the ingestion job.</summary>
    public Guid Id { get; set; }

    /// <summary>ID of the LegalDocument being ingested.</summary>
    public Guid DocumentId { get; set; }

    /// <summary>Title of the LegalDocument — joined at query time for display.</summary>
    public string DocumentTitle { get; set; }

    /// <summary>Current stage of the pipeline.</summary>
    public IngestionStatus Status { get; set; }

    /// <summary>ID of the admin user who triggered the job. Null if triggered programmatically.</summary>
    public long? TriggeredByUserId { get; set; }

    /// <summary>UTC timestamp when the pipeline started (= ExtractStartedAt).</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>UTC timestamp when the job reached Completed or Failed status.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Human-readable error message. Null for non-failed jobs.</summary>
    public string ErrorMessage { get; set; }

    // ── Extract stage ─────────────────────────────────────────────────────────

    /// <summary>UTC timestamp when text extraction began.</summary>
    public DateTime? ExtractStartedAt { get; set; }

    /// <summary>UTC timestamp when text extraction completed.</summary>
    public DateTime? ExtractCompletedAt { get; set; }

    /// <summary>Wall-clock duration of the Extract stage. Null if stage is incomplete.</summary>
    public TimeSpan? ExtractDuration =>
        ExtractStartedAt.HasValue && ExtractCompletedAt.HasValue
            ? ExtractCompletedAt.Value - ExtractStartedAt.Value
            : (TimeSpan?)null;

    /// <summary>Total characters extracted from the PDF.</summary>
    public int ExtractedCharacterCount { get; set; }

    // ── Transform stage ───────────────────────────────────────────────────────

    /// <summary>UTC timestamp when section parsing and chunking began.</summary>
    public DateTime? TransformStartedAt { get; set; }

    /// <summary>UTC timestamp when chunking completed.</summary>
    public DateTime? TransformCompletedAt { get; set; }

    /// <summary>Wall-clock duration of the Transform stage. Null if stage is incomplete.</summary>
    public TimeSpan? TransformDuration =>
        TransformStartedAt.HasValue && TransformCompletedAt.HasValue
            ? TransformCompletedAt.Value - TransformStartedAt.Value
            : (TimeSpan?)null;

    /// <summary>Number of chunks produced by the Transform stage.</summary>
    public int ChunksProduced { get; set; }

    /// <summary>Chunking strategy used (SectionLevel or FixedSize). Null until Transform completes.</summary>
    public ChunkStrategy? Strategy { get; set; }

    // ── Load stage ────────────────────────────────────────────────────────────

    /// <summary>UTC timestamp when the Load stage began (chunk persistence started).</summary>
    public DateTime? LoadStartedAt { get; set; }

    /// <summary>UTC timestamp when all chunks were successfully persisted.</summary>
    public DateTime? LoadCompletedAt { get; set; }

    /// <summary>Wall-clock duration of the Load stage. Null if stage is incomplete.</summary>
    public TimeSpan? LoadDuration =>
        LoadStartedAt.HasValue && LoadCompletedAt.HasValue
            ? LoadCompletedAt.Value - LoadStartedAt.Value
            : (TimeSpan?)null;

    /// <summary>Number of DocumentChunk entities saved to the database.</summary>
    public int ChunksLoaded { get; set; }

    /// <summary>Number of ChunkEmbedding records generated and saved.</summary>
    public int EmbeddingsGenerated { get; set; }

    // ── Overall ───────────────────────────────────────────────────────────────

    /// <summary>
    /// End-to-end wall-clock duration from pipeline start to terminal state.
    /// Null while the job is still in progress.
    /// </summary>
    public TimeSpan? TotalDuration =>
        ExtractStartedAt.HasValue && CompletedAt.HasValue
            ? CompletedAt.Value - ExtractStartedAt.Value
            : (TimeSpan?)null;
}
