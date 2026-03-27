using Abp.Domain.Entities;
using System;

namespace backend.MzansiLegal.KnowledgeBase;

public class ChunkEmbedding : Entity<Guid>
{
    public Guid DocumentChunkId { get; set; }
    public virtual DocumentChunk DocumentChunk { get; set; }

    /// <summary>
    /// Stored as a JSON array of 1536 floats (text-embedding-ada-002 dimensions).
    /// </summary>
    public string VectorJson { get; set; }

    protected ChunkEmbedding() { }

    public ChunkEmbedding(Guid id, Guid documentChunkId, float[] vector)
    {
        Id = id;
        DocumentChunkId = documentChunkId;
        VectorJson = System.Text.Json.JsonSerializer.Serialize(vector);
    }

    public float[] GetVector()
    {
        return System.Text.Json.JsonSerializer.Deserialize<float[]>(VectorJson) ?? Array.Empty<float>();
    }
}
