using System;

namespace backend.Services.RagService.DTO;

/// <summary>
/// Represents a single verified legislation citation returned with a RAG answer.
/// Each citation links a specific Act section to a claim in the generated answer text.
/// </summary>
public class RagCitationDto
{
    /// <summary>ID of the <see cref="backend.Domains.LegalDocuments.DocumentChunk"/> used for this citation.</summary>
    public Guid ChunkId { get; set; }

    /// <summary>
    /// Full name of the legislation Act (e.g., "Constitution of the Republic of South Africa").
    /// Sourced from <c>LegalDocument.Title</c>.
    /// </summary>
    public string ActName { get; set; }

    /// <summary>
    /// Section identifier within the Act (e.g., "§ 26(3)").
    /// Sourced from <c>DocumentChunk.SectionNumber</c>.
    /// </summary>
    public string SectionNumber { get; set; }

    /// <summary>
    /// Preferred generic source title for any cited source, including official guidance.
    /// </summary>
    public string SourceTitle { get; set; }

    /// <summary>
    /// Preferred generic locator for the cited source such as a section, rule, or heading.
    /// </summary>
    public string SourceLocator { get; set; }

    /// <summary>
    /// Distinguishes binding law from supporting official guidance.
    /// </summary>
    public string AuthorityType { get; set; }

    /// <summary>
    /// Distinguishes the primary source from supporting sources in a multi-source answer.
    /// </summary>
    public string SourceRole { get; set; }

    /// <summary>
    /// Relevant text excerpt from the chunk that supports the answer.
    /// Truncated to 500 characters if the full chunk content is longer.
    /// </summary>
    public string Excerpt { get; set; }

    /// <summary>
    /// Cosine similarity score between the question embedding and this chunk's embedding.
    /// Only chunks scoring ≥ 0.7 are included. Values are in the range [0.7, 1.0].
    /// </summary>
    public float RelevanceScore { get; set; }
}
