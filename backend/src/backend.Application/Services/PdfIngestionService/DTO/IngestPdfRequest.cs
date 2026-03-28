using System;
using System.IO;

namespace backend.Services.PdfIngestionService.DTO;

/// <summary>
/// Input model for a single PDF ingestion request.
/// The caller must have pre-created the LegalDocument and IngestionJob before calling IngestAsync.
/// </summary>
public class IngestPdfRequest
{
    /// <summary>Readable byte stream of the legislation PDF. Must be open and positioned at the start.</summary>
    public Stream PdfStream { get; set; }

    /// <summary>
    /// Official Act name (e.g., "Labour Relations Act").
    /// Used verbatim as ActName on every returned DocumentChunkResult.
    /// </summary>
    public string ActName { get; set; }

    /// <summary>Id of the parent LegalDocument entity. Each returned chunk is linked to this document.</summary>
    public Guid DocumentId { get; set; }

    /// <summary>Id of the pre-created IngestionJob entity. The service updates this job at each pipeline stage.</summary>
    public Guid IngestionJobId { get; set; }
}
