using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Ardalis.GuardClauses;
using backend.Domains.ETL;
using backend.Services.PdfIngestionService.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace backend.Services.PdfIngestionService;

/// <summary>
/// Implements the PDF ingestion pipeline for SA legislation documents.
/// Extracts text with PdfPig, delegates chunking to PdfChunkingHelper (section-level or fixed-size),
/// and tracks each pipeline stage on the associated IngestionJob.
///
/// Caller responsibilities (per contracts/pdf-ingestion-service.md):
///   - Create IngestionJob (Status=Queued) before calling IngestAsync.
///   - Map returned DocumentChunkResults to DocumentChunk entities and persist them.
///   - Set LegalDocument.IsProcessed = true and TotalChunks after successful persistence.
///   - Update IngestionJob.ChunksLoaded, LoadCompletedAt, and Status = Completed after loading.
/// </summary>
[AbpAuthorize]
public class PdfIngestionAppService : ApplicationService, IPdfIngestionAppService
{
    private readonly IRepository<IngestionJob, Guid> _ingestionJobRepository;

    /// <summary>Initialises the service with the ABP-injected IngestionJob repository.</summary>
    public PdfIngestionAppService(IRepository<IngestionJob, Guid> ingestionJobRepository)
    {
        _ingestionJobRepository = ingestionJobRepository;
    }

    /// <summary>
    /// Ingests a legislation PDF and returns an ordered list of DocumentChunkResult objects.
    /// Transitions the IngestionJob through Extracting → Transforming → Loading stages.
    /// Returns an empty list without throwing when the PDF yields no extractable text.
    /// </summary>
    public async Task<IReadOnlyList<DocumentChunkResult>> IngestAsync(IngestPdfRequest request)
    {
        Guard.Against.Null(request, nameof(request));
        Guard.Against.Null(request.PdfStream, nameof(request.PdfStream));
        Guard.Against.NullOrWhiteSpace(request.ActName, nameof(request.ActName));

        var job = await _ingestionJobRepository.GetAsync(request.IngestionJobId);

        var fullText = await ExtractStageAsync(job, request.PdfStream);
        if (string.IsNullOrWhiteSpace(fullText))
        {
            // No extractable text (e.g., scanned image PDF) — return empty list without failing.
            return Array.Empty<DocumentChunkResult>();
        }

        var chunks = await TransformStageAsync(job, fullText, request.ActName);

        await SignalLoadingStageAsync(job);

        return chunks;
    }

    // ── Stage 1: Extract ─────────────────────────────────────────────────────

    /// <summary>
    /// Transitions the IngestionJob to Extracting, opens the PDF stream with PdfPig,
    /// concatenates all page text, and records the character count on the job.
    /// On failure, marks the job as Failed and re-throws so the caller can log the error.
    /// </summary>
    private async Task<string> ExtractStageAsync(IngestionJob job, Stream pdfStream)
    {
        job.Status = IngestionStatus.Extracting;
        job.ExtractStartedAt = DateTime.UtcNow;
        await _ingestionJobRepository.UpdateAsync(job);

        string fullText;
        try
        {
            fullText = ExtractText(pdfStream);
        }
        catch (Exception ex)
        {
            job.Status = IngestionStatus.Failed;
            job.ErrorMessage = $"Text extraction failed: {ex.Message}";
            await _ingestionJobRepository.UpdateAsync(job);
            throw;
        }

        job.ExtractedCharacterCount = fullText.Length;
        job.ExtractCompletedAt = DateTime.UtcNow;
        await _ingestionJobRepository.UpdateAsync(job);

        return fullText;
    }

    // ── Stage 2: Transform ───────────────────────────────────────────────────

    /// <summary>
    /// Transitions the IngestionJob to Transforming, detects SA legislation sections,
    /// delegates to section-level or fixed-size chunking based on detection count,
    /// and records the strategy and chunk count on the job.
    /// </summary>
    private async Task<List<DocumentChunkResult>> TransformStageAsync(
        IngestionJob job,
        string fullText,
        string actName)
    {
        job.Status = IngestionStatus.Transforming;
        job.TransformStartedAt = DateTime.UtcNow;
        await _ingestionJobRepository.UpdateAsync(job);

        var sections = PdfChunkingHelper.DetectSections(fullText);
        List<DocumentChunkResult> chunks;
        Domains.LegalDocuments.ChunkStrategy strategy;

        if (sections.Count < PdfChunkingHelper.MinSectionsForAuto)
        {
            // Fewer than MinSectionsForAuto sections detected — fall back to fixed-size windowing.
            strategy = Domains.LegalDocuments.ChunkStrategy.FixedSize;
            chunks   = PdfChunkingHelper.BuildFixedSizeChunks(fullText, actName);
        }
        else
        {
            strategy = Domains.LegalDocuments.ChunkStrategy.SectionLevel;
            chunks   = PdfChunkingHelper.BuildSectionChunks(sections, actName, fullText);
        }

        job.Strategy             = strategy;
        job.ChunksProduced       = chunks.Count;
        job.TransformCompletedAt = DateTime.UtcNow;
        await _ingestionJobRepository.UpdateAsync(job);

        return chunks;
    }

    // ── Stage 3: Loading (signalled) ─────────────────────────────────────────

    /// <summary>
    /// Transitions the IngestionJob to Loading and records the start timestamp.
    /// The caller is responsible for persisting chunks and finalising the job to Completed.
    /// </summary>
    private async Task SignalLoadingStageAsync(IngestionJob job)
    {
        job.Status        = IngestionStatus.Loading;
        job.LoadStartedAt = DateTime.UtcNow;
        await _ingestionJobRepository.UpdateAsync(job);
    }

    // ── Private: PDF text extraction ─────────────────────────────────────────

    /// <summary>
    /// Opens the PDF stream with PdfPig and reconstructs page text with proper word spacing
    /// and line breaks by grouping words using their Y-coordinate positions.
    /// Falls back to page.Text if no words are detected (e.g. image-only pages).
    /// Government gazette PDFs often concatenate glyphs without spaces in page.Text,
    /// so word-level extraction with spatial line grouping is required.
    /// </summary>
    private static string ExtractText(Stream pdfStream)
    {
        var sb = new StringBuilder();
        using var document = PdfDocument.Open(pdfStream);

        foreach (var page in document.GetPages())
        {
            var words = page.GetWords().ToList();
            if (words.Count == 0)
            {
                sb.AppendLine(page.Text);
                continue;
            }

            // Group words into lines by rounding Y centroid to the nearest point.
            // PDF Y-axis increases upward, so order descending to read top-to-bottom.
            var lines = words
                .GroupBy(w => Math.Round(w.BoundingBox.Centroid.Y, 0))
                .OrderByDescending(g => g.Key)
                .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)));

            foreach (var line in lines)
                sb.AppendLine(line);
        }

        return sb.ToString();
    }
}
