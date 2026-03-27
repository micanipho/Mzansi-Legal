namespace backend.MzansiLegal.KnowledgeBase;

/// <summary>
/// Intermediate DTO produced by SouthAfricanLegislationParser during PDF extraction.
/// </summary>
public class LegislationChunk
{
    public string ChapterTitle { get; set; }
    public string SectionNumber { get; set; }
    public string SectionTitle { get; set; }
    public string Content { get; set; }
    public int SortOrder { get; set; }
}
