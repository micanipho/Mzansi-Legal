using Abp.Domain.Entities.Auditing;
using backend.Domains.LegalDocuments;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Domains.ETL;

/// <summary>
/// Tracks a single PDF ingestion pipeline run for a LegalDocument.
/// Records the status and timing of each stage: Extracting, Transforming, and Loading.
/// Required by the ETL/Ingestion Gate — every ingestion operation must have a corresponding job.
/// </summary>
public class IngestionJob : FullAuditedEntity<Guid>
{
    /// <summary>FK to the LegalDocument being ingested.</summary>
    [Required]
    public Guid DocumentId { get; set; }

    /// <summary>Current stage of the ingestion pipeline.</summary>
    [Required]
    public IngestionStatus Status { get; set; } = IngestionStatus.Queued;

    // ── Extract stage ────────────────────────────────────────────────────────

    /// <summary>UTC timestamp when text extraction from the PDF began.</summary>
    public DateTime? ExtractStartedAt { get; set; }

    /// <summary>UTC timestamp when text extraction completed successfully.</summary>
    public DateTime? ExtractCompletedAt { get; set; }

    /// <summary>Total number of characters extracted from the PDF. Zero until extraction completes.</summary>
    public int ExtractedCharacterCount { get; set; } = 0;

    // ── Transform stage ──────────────────────────────────────────────────────

    /// <summary>UTC timestamp when section parsing and chunking began.</summary>
    public DateTime? TransformStartedAt { get; set; }

    /// <summary>UTC timestamp when chunking completed and DocumentChunkResults were produced.</summary>
    public DateTime? TransformCompletedAt { get; set; }

    /// <summary>Number of DocumentChunkResults returned by the Transform stage. Zero until transform completes.</summary>
    public int ChunksProduced { get; set; } = 0;

    // ── Load stage ───────────────────────────────────────────────────────────

    /// <summary>UTC timestamp when the caller began persisting chunks to the database.</summary>
    public DateTime? LoadStartedAt { get; set; }

    /// <summary>UTC timestamp when all chunks were successfully persisted.</summary>
    public DateTime? LoadCompletedAt { get; set; }

    /// <summary>Number of DocumentChunk entities successfully saved to the database. Zero until load completes.</summary>
    public int ChunksLoaded { get; set; } = 0;

    // ── Strategy and error ───────────────────────────────────────────────────

    /// <summary>
    /// The chunking strategy detected during the Transform stage.
    /// Null until the Transform stage completes.
    /// </summary>
    public ChunkStrategy? Strategy { get; set; }

    /// <summary>
    /// Human-readable error description when Status = Failed.
    /// Null for non-failed jobs.
    /// </summary>
    [MaxLength(2000)]
    public string ErrorMessage { get; set; }

    // ── Orchestrator fields (added by feature 010-etl-ingestion-pipeline) ────

    /// <summary>
    /// ABP user ID of the admin who triggered this ingestion job.
    /// Null when triggered programmatically or when the triggering user has been removed.
    /// </summary>
    public long? TriggeredByUserId { get; set; }

    /// <summary>
    /// Number of ChunkEmbedding records successfully generated and saved during the Load stage.
    /// Zero until the Load stage completes.
    /// </summary>
    public int EmbeddingsGenerated { get; set; } = 0;

    /// <summary>
    /// UTC timestamp when the job reached a terminal state (Completed or Failed).
    /// Null while the job is still in progress.
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}
