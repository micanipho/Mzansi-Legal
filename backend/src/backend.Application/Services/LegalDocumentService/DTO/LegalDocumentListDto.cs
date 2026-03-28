using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using backend.Domains.LegalDocuments;
using System;

namespace backend.Services.LegalDocumentService.DTO;

/// <summary>
/// Lightweight DTO for LegalDocument list queries. Excludes FullText to reduce
/// payload size when returning multiple records. Use LegalDocumentDto for single-record reads.
/// </summary>
[AutoMap(typeof(LegalDocument))]
public class LegalDocumentListDto : EntityDto<Guid>
{
    /// <summary>Full official title of the legislation.</summary>
    public string Title { get; set; }

    /// <summary>Abbreviated name used in citations.</summary>
    public string ShortName { get; set; }

    /// <summary>Official act number.</summary>
    public string ActNumber { get; set; }

    /// <summary>Year in which the act was enacted.</summary>
    public int Year { get; set; }

    /// <summary>Original filename of the uploaded PDF.</summary>
    public string FileName { get; set; }

    /// <summary>FK to the category that classifies this document.</summary>
    public Guid CategoryId { get; set; }

    /// <summary>Whether the ingestion pipeline has processed this document.</summary>
    public bool IsProcessed { get; set; }

    /// <summary>Number of chunks generated from this document.</summary>
    public int TotalChunks { get; set; }
}
