using Abp.Domain.Entities;
using backend.MzansiLegal.Categories;
using System;
using System.Collections.Generic;

namespace backend.MzansiLegal.KnowledgeBase;

public class LegalDocument : Entity<Guid>
{
    public string Title { get; set; }
    public string ShortName { get; set; }
    public string ActNumber { get; set; }
    public int Year { get; set; }
    public string FullText { get; set; }
    public string OriginalPdfPath { get; set; }
    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; }
    public bool IsProcessed { get; set; }
    public int TotalChunks { get; set; }

    public virtual ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();

    protected LegalDocument() { }

    public LegalDocument(Guid id, string title, string shortName, string actNumber, int year, Guid categoryId)
    {
        Id = id;
        Title = title;
        ShortName = shortName;
        ActNumber = actNumber;
        Year = year;
        CategoryId = categoryId;
        IsProcessed = false;
        TotalChunks = 0;
    }
}
