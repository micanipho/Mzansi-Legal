using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Domains.LegalDocuments;

/// <summary>
/// Represents a single piece of legislation stored in the system.
/// Acts as an aggregate root that owns DocumentChunks and their embeddings.
/// </summary>
public class LegalDocument : FullAuditedEntity<Guid>
{
    /// <summary>Full official title of the legislation (e.g., "Labour Relations Act").</summary>
    [Required]
    [MaxLength(500)]
    public string Title { get; set; }

    /// <summary>Abbreviated name used in citations (e.g., "LRA").</summary>
    [MaxLength(100)]
    public string ShortName { get; set; }

    /// <summary>Official act number (e.g., "66").</summary>
    [MaxLength(50)]
    public string ActNumber { get; set; }

    /// <summary>Year in which the act was enacted.</summary>
    [Required]
    public int Year { get; set; }

    /// <summary>Complete plain-text content of the document. Unbounded; excluded from list queries.</summary>
    public string FullText { get; set; }

    /// <summary>Original filename of the uploaded PDF (e.g., "lra-1995.pdf").</summary>
    [MaxLength(300)]
    public string FileName { get; set; }

    /// <summary>
    /// FK to the ABP BinaryObject (StoredFile) representing the original PDF.
    /// Null until the file has been uploaded.
    /// </summary>
    public Guid? OriginalPdfId { get; set; }

    /// <summary>FK to the category that classifies this document.</summary>
    [Required]
    public Guid CategoryId { get; set; }

    /// <summary>Navigation property to the parent Category.</summary>
    [ForeignKey(nameof(CategoryId))]
    public virtual Category Category { get; set; }

    /// <summary>
    /// Whether the document has been chunked and embedded by the ingestion pipeline.
    /// Defaults to false on creation.
    /// </summary>
    public bool IsProcessed { get; set; } = false;

    /// <summary>
    /// Total number of chunks generated from this document.
    /// Updated by the ingestion pipeline after processing.
    /// </summary>
    public int TotalChunks { get; set; } = 0;

    /// <summary>Ordered collection of text chunks produced from this document.</summary>
    public virtual ICollection<DocumentChunk> Chunks { get; set; }
}
