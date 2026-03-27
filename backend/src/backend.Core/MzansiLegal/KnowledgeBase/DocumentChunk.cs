using Abp.Domain.Entities;
using System;

namespace backend.MzansiLegal.KnowledgeBase;

public class DocumentChunk : Entity<Guid>
{
    public Guid LegalDocumentId { get; set; }
    public virtual LegalDocument LegalDocument { get; set; }

    public string ChapterTitle { get; set; }
    public string SectionNumber { get; set; }
    public string SectionTitle { get; set; }
    public string Content { get; set; }
    public int TokenCount { get; set; }
    public int SortOrder { get; set; }

    public virtual ChunkEmbedding Embedding { get; set; }

    protected DocumentChunk() { }

    public DocumentChunk(Guid id, Guid legalDocumentId, string sectionNumber, string sectionTitle, string content, int sortOrder)
    {
        Id = id;
        LegalDocumentId = legalDocumentId;
        SectionNumber = sectionNumber;
        SectionTitle = sectionTitle;
        Content = content;
        SortOrder = sortOrder;
    }
}
