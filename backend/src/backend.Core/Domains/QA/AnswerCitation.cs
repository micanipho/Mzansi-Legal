using Abp.Domain.Entities.Auditing;
using backend.Domains.LegalDocuments;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Domains.QA;

/// <summary>
/// Links an <see cref="Answer"/> to a specific <see cref="DocumentChunk"/> from the legislation knowledge base.
/// Represents a verifiable citation supporting the AI-generated answer with a section reference,
/// a text excerpt, and a relevance score from the retrieval pipeline.
///
/// <para>
/// <b>Cross-aggregate reference</b>: <see cref="Chunk"/> references <see cref="DocumentChunk"/>
/// which belongs to the LegalDocuments aggregate. This is permitted by the constitution
/// exclusively for RAG citation traceability. The FK uses <see cref="DeleteBehavior.Restrict"/>
/// (configured in DbContext) to prevent chunk deletion while citations exist.
/// </para>
/// </summary>
public class AnswerCitation : FullAuditedEntity<Guid>
{
    /// <summary>Foreign key to the parent <see cref="Answer"/> that contains this citation.</summary>
    [Required]
    public Guid AnswerId { get; set; }

    /// <summary>Navigation property to the parent Answer.</summary>
    [ForeignKey(nameof(AnswerId))]
    public virtual Answer Answer { get; set; }

    /// <summary>
    /// Foreign key to the <see cref="DocumentChunk"/> from the legislation knowledge base
    /// that supports the answer. Cross-aggregate reference — chunk deletion is restricted
    /// while this citation exists.
    /// </summary>
    [Required]
    public Guid ChunkId { get; set; }

    /// <summary>Navigation property to the referenced DocumentChunk (cross-aggregate read reference).</summary>
    [ForeignKey(nameof(ChunkId))]
    public virtual DocumentChunk Chunk { get; set; }

    /// <summary>
    /// Section identifier of the legislation this citation points to (e.g., "§ 12(3)").
    /// Stored for display purposes independent of the chunk's current content.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string SectionNumber { get; set; }

    /// <summary>
    /// Relevant text excerpt from the <see cref="Chunk"/> that directly supports the answer.
    /// Copied at citation time to preserve the grounding text even if the chunk is later updated.
    /// </summary>
    [Required]
    public string Excerpt { get; set; }

    /// <summary>
    /// Cosine similarity score produced by the retrieval pipeline, indicating how relevant
    /// this chunk was to the original question. Expected range: 0.0 (no relevance) to 1.0 (exact match).
    /// Range enforcement is the responsibility of the application layer.
    /// </summary>
    [Required]
    public decimal RelevanceScore { get; set; }
}
