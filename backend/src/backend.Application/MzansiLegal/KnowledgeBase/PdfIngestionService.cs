using Abp.Domain.Repositories;
using backend.MzansiLegal.Categories;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace backend.MzansiLegal.KnowledgeBase;

public class PdfIngestionService
{
    private readonly IRepository<LegalDocument, Guid> _documentRepo;
    private readonly IRepository<DocumentChunk, Guid> _chunkRepo;
    private readonly EmbeddingService _embeddingService;

    public PdfIngestionService(
        IRepository<LegalDocument, Guid> documentRepo,
        IRepository<DocumentChunk, Guid> chunkRepo,
        EmbeddingService embeddingService)
    {
        _documentRepo = documentRepo;
        _chunkRepo = chunkRepo;
        _embeddingService = embeddingService;
    }

    public async Task<LegalDocument> IngestAsync(
        Stream pdfStream,
        string title,
        string shortName,
        string actNumber,
        int year,
        Guid categoryId,
        string pdfPath = null)
    {
        // Parse PDF into structured chunks
        var chunks = SouthAfricanLegislationParser.Parse(pdfStream);

        // Create document record
        var document = new LegalDocument(
            Guid.NewGuid(), title, shortName, actNumber, year, categoryId);
        document.OriginalPdfPath = pdfPath;

        // Concatenate full text from chunks for search fallback
        var fullText = new StringBuilder();
        foreach (var chunk in chunks)
            fullText.AppendLine(chunk.Content);
        document.FullText = fullText.ToString();

        await _documentRepo.InsertAsync(document);

        // Create chunk + embedding records
        foreach (var parsed in chunks)
        {
            var chunk = new DocumentChunk(
                Guid.NewGuid(),
                document.Id,
                parsed.SectionNumber,
                parsed.SectionTitle,
                parsed.Content,
                parsed.SortOrder)
            {
                ChapterTitle = parsed.ChapterTitle,
                TokenCount = EstimateTokens(parsed.Content)
            };

            await _chunkRepo.InsertAsync(chunk);

            // Generate and store embedding
            await _embeddingService.EmbedChunkAsync(chunk);
        }

        document.TotalChunks = chunks.Count;
        document.IsProcessed = true;
        await _documentRepo.UpdateAsync(document);

        return document;
    }

    private static int EstimateTokens(string text) =>
        (int)Math.Ceiling(text.Length / 4.0); // ~4 chars per token
}
