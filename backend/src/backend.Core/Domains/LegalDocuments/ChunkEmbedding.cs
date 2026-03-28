using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Domains.LegalDocuments;

/// <summary>
/// Stores the numerical vector representation of a DocumentChunk for similarity search.
/// Kept separate from DocumentChunk for storage and query performance reasons.
/// The vector is stored as a PostgreSQL real[] column via Npgsql native array mapping.
/// </summary>
public class ChunkEmbedding : FullAuditedEntity<Guid>
{
    /// <summary>
    /// Fixed dimension of all embedding vectors in this system.
    /// Matches the output dimension of the configured embedding model (e.g., text-embedding-ada-002).
    /// </summary>
    public const int EmbeddingDimension = 1536;

    /// <summary>FK to the DocumentChunk this embedding belongs to.</summary>
    [Required]
    public Guid ChunkId { get; set; }

    /// <summary>Navigation property to the parent DocumentChunk.</summary>
    [ForeignKey(nameof(ChunkId))]
    public virtual DocumentChunk Chunk { get; set; }

    /// <summary>
    /// Embedding vector with exactly EmbeddingDimension (1536) float values.
    /// Stored as PostgreSQL real[] via Npgsql. Application code must validate
    /// Vector.Length == EmbeddingDimension before persisting.
    /// </summary>
    [Required]
    public float[] Vector { get; set; }
}
