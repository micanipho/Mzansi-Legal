using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Domains.LegalDocuments;

/// <summary>
/// Represents a structured fragment of a LegalDocument produced by the ingestion pipeline.
/// Chunks are aligned to chapter and section boundaries and ordered by SortOrder
/// to preserve the original reading sequence.
/// </summary>
public class DocumentChunk : FullAuditedEntity<Guid>
{
    /// <summary>FK to the parent LegalDocument.</summary>
    [Required]
    public Guid DocumentId { get; set; }

    /// <summary>Navigation property to the parent LegalDocument.</summary>
    [ForeignKey(nameof(DocumentId))]
    public virtual LegalDocument Document { get; set; }

    /// <summary>Title of the chapter this chunk belongs to (e.g., "Chapter 2 — Fundamental Rights").</summary>
    [MaxLength(500)]
    public string ChapterTitle { get; set; }

    /// <summary>Section identifier within the act (e.g., "§ 12(3)").</summary>
    [MaxLength(50)]
    public string SectionNumber { get; set; }

    /// <summary>Heading of the specific section.</summary>
    [MaxLength(500)]
    public string SectionTitle { get; set; }

    /// <summary>Plain-text body of this chunk, used as the retrieval unit in the RAG pipeline.</summary>
    [Required]
    public string Content { get; set; }

    /// <summary>
    /// Number of tokens in this chunk as counted by the embedding tokeniser.
    /// Used for budget-aware retrieval to stay within context window limits.
    /// </summary>
    public int TokenCount { get; set; }

    /// <summary>
    /// Position of this chunk within the parent document.
    /// Querying by (DocumentId, SortOrder) returns chunks in reading sequence.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>Embedding vector associated with this chunk. Null until the pipeline has processed the chunk.</summary>
    public virtual ChunkEmbedding Embedding { get; set; }

    /// <summary>
    /// Chunking strategy used to produce this chunk (SectionLevel or FixedSize).
    /// Null for chunks ingested before feature 008-pdf-section-chunking was introduced.
    /// </summary>
    public ChunkStrategy? ChunkStrategy { get; set; }

    // ── LLM enrichment fields (added by feature 010-etl-ingestion-pipeline) ──

    /// <summary>
    /// Comma-separated legal keywords extracted by LLM enrichment
    /// (e.g., "employment,termination,fair dismissal").
    /// Null for chunks processed before this feature or when enrichment failed.
    /// </summary>
    [MaxLength(500)]
    public string Keywords { get; set; }

    /// <summary>
    /// Topic classification label assigned by LLM enrichment
    /// (e.g., "Labour Relations"). "Unknown" when enrichment failed gracefully.
    /// Null for chunks processed before this feature.
    /// </summary>
    [MaxLength(200)]
    public string TopicClassification { get; set; }
}
