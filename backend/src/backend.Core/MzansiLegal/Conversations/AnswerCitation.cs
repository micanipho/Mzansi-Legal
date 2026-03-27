using Abp.Domain.Entities;
using backend.MzansiLegal.KnowledgeBase;
using System;

namespace backend.MzansiLegal.Conversations;

public class AnswerCitation : Entity<Guid>
{
    public Guid AnswerId { get; set; }
    public virtual Answer Answer { get; set; }

    public Guid ChunkId { get; set; }
    public virtual DocumentChunk Chunk { get; set; }

    public string SectionNumber { get; set; }
    public string Excerpt { get; set; }
    public decimal RelevanceScore { get; set; }

    protected AnswerCitation() { }

    public AnswerCitation(Guid id, Guid answerId, Guid chunkId, string sectionNumber, string excerpt, decimal relevanceScore)
    {
        Id = id;
        AnswerId = answerId;
        ChunkId = chunkId;
        SectionNumber = sectionNumber;
        Excerpt = excerpt;
        RelevanceScore = relevanceScore;
    }
}
