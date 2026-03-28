using Abp.Application.Services;
using backend.Services.PdfIngestionService.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services.PdfIngestionService;

/// <summary>
/// Extracts text from a legislation PDF stream, splits it into structured chunks aligned to
/// SA legislation section boundaries, and returns the results ready for the caller to persist.
/// The caller is responsible for creating the IngestionJob before calling IngestAsync and for
/// persisting the returned chunks and updating LegalDocument.IsProcessed after the call.
/// </summary>
public interface IPdfIngestionAppService : IApplicationService
{
    /// <summary>
    /// Ingests a legislation PDF and returns an ordered list of DocumentChunkResult objects.
    /// Updates the associated IngestionJob at each pipeline stage (Extracting, Transforming, Loading).
    /// Returns an empty list if the PDF contains no extractable text — does not throw in this case.
    /// </summary>
    Task<IReadOnlyList<DocumentChunkResult>> IngestAsync(IngestPdfRequest request);
}
