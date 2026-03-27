using Abp.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Embeddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.MzansiLegal.KnowledgeBase;

public class EmbeddingService
{
    private readonly IRepository<ChunkEmbedding, Guid> _embeddingRepo;
    private readonly IRepository<DocumentChunk, Guid> _chunkRepo;
    private readonly EmbeddingClient _openAiClient;
    private readonly string _model;

    public EmbeddingService(
        IRepository<ChunkEmbedding, Guid> embeddingRepo,
        IRepository<DocumentChunk, Guid> chunkRepo,
        IConfiguration configuration)
    {
        _embeddingRepo = embeddingRepo;
        _chunkRepo = chunkRepo;

        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured.");
        _model = configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-ada-002";

        var client = new OpenAIClient(apiKey);
        _openAiClient = client.GetEmbeddingClient(_model);
    }

    public async Task EmbedChunkAsync(DocumentChunk chunk)
    {
        var vector = await GetEmbeddingAsync(chunk.Content);
        var embedding = new ChunkEmbedding(Guid.NewGuid(), chunk.Id, vector);
        await _embeddingRepo.InsertAsync(embedding);
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var result = await _openAiClient.GenerateEmbeddingAsync(text);
        return result.Value.ToFloats().ToArray();
    }

    /// <summary>
    /// Returns the top-k most relevant chunks for the given query vector using cosine similarity.
    /// Loads all embeddings in-memory — appropriate for up to ~20 documents (~5k chunks).
    /// </summary>
    public async Task<List<ScoredChunk>> SearchAsync(float[] queryVector, int topK = 5)
    {
        var embeddings = await _embeddingRepo.GetAllListAsync();
        var scored = new List<ScoredChunk>(embeddings.Count);

        foreach (var embedding in embeddings)
        {
            var chunkVector = embedding.GetVector();
            if (chunkVector.Length == 0) continue;

            var score = CosineSimilarity(queryVector, chunkVector);
            scored.Add(new ScoredChunk
            {
                ChunkId = embedding.DocumentChunkId,
                Score = score
            });
        }

        // Sort descending by score, return top-k
        scored.Sort((a, b) => b.Score.CompareTo(a.Score));
        return scored.Take(topK).ToList();
    }

    // T010: Cosine similarity
    internal static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length) return 0f;

        double dot = 0, normA = 0, normB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        if (normA == 0 || normB == 0) return 0f;
        return (float)(dot / (Math.Sqrt(normA) * Math.Sqrt(normB)));
    }
}

public class ScoredChunk
{
    public Guid ChunkId { get; set; }
    public float Score { get; set; }
}
