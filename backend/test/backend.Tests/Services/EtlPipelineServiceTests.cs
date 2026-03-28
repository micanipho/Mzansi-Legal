#nullable enable
using Abp.Domain.Repositories;
using backend.Domains.ETL;
using backend.Domains.LegalDocuments;
using backend.Services.ChunkEnrichmentService;
using backend.Services.ChunkEnrichmentService.DTO;
using backend.Services.EmbeddingService;
using backend.Services.EmbeddingService.DTO;
using backend.Services.EtlPipelineService;
using backend.Services.PdfIngestionService;
using backend.Services.PdfIngestionService.DTO;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests.Services;

/// <summary>
/// Unit tests for ETL orchestration success, empty-result completion, and failure handling.
/// </summary>
public class EtlPipelineServiceTests
{
    [Fact]
    public async Task TriggerAsync_HappyPath_CompletesAndCountsChunksAndEmbeddings()
    {
        using var context = new EtlPipelineTestContext();
        var documentId = context.SeedDocument();

        context.PdfIngestionAppService.IngestAsync(Arg.Any<IngestPdfRequest>())
            .Returns(Task.FromResult<IReadOnlyList<DocumentChunkResult>>(
                new[]
                {
                    BuildChunkResult("First chunk", 0),
                    BuildChunkResult("Second chunk", 1)
                }));

        context.ChunkEnrichmentAppService.EnrichAsync(Arg.Any<string>())
            .Returns(Task.FromResult(new ChunkEnrichmentResult
            {
                Keywords = "law,rights",
                TopicClassification = "Constitutional Rights"
            }));

        context.EmbeddingAppService.GenerateEmbeddingAsync(Arg.Any<string>())
            .Returns(Task.FromResult(new EmbeddingResult
            {
                Vector = new float[ChunkEmbedding.EmbeddingDimension],
                Model = "text-embedding-ada-002",
                InputCharacterCount = 20
            }));

        var result = await context.Service.TriggerAsync(documentId);

        result.Status.ShouldBe(IngestionStatus.Completed);
        result.ChunksLoaded.ShouldBe(2);
        result.EmbeddingsGenerated.ShouldBe(2);
        context.Jobs.Single().Status.ShouldBe(IngestionStatus.Completed);
        context.Chunks.Count.ShouldBe(2);
        context.Embeddings.Count.ShouldBe(2);
    }

    [Fact]
    public async Task TriggerAsync_EmptyChunkSet_CompletesWithoutSavingChunks()
    {
        using var context = new EtlPipelineTestContext();
        var documentId = context.SeedDocument();

        context.PdfIngestionAppService.IngestAsync(Arg.Any<IngestPdfRequest>())
            .Returns(Task.FromResult<IReadOnlyList<DocumentChunkResult>>(Array.Empty<DocumentChunkResult>()));

        var result = await context.Service.TriggerAsync(documentId);

        result.Status.ShouldBe(IngestionStatus.Completed);
        result.ChunksLoaded.ShouldBe(0);
        result.EmbeddingsGenerated.ShouldBe(0);
        context.Chunks.ShouldBeEmpty();
        context.Embeddings.ShouldBeEmpty();
    }

    [Fact]
    public async Task TriggerAsync_EmbeddingFailure_MarksJobFailedAndPropagates()
    {
        using var context = new EtlPipelineTestContext();
        var documentId = context.SeedDocument();

        context.PdfIngestionAppService.IngestAsync(Arg.Any<IngestPdfRequest>())
            .Returns(Task.FromResult<IReadOnlyList<DocumentChunkResult>>(
                new[] { BuildChunkResult("Broken chunk", 0) }));

        context.ChunkEnrichmentAppService.EnrichAsync(Arg.Any<string>())
            .Returns(Task.FromResult(ChunkEnrichmentResult.Fallback()));

        context.EmbeddingAppService.GenerateEmbeddingAsync(Arg.Any<string>())
            .Returns<Task<EmbeddingResult>>(_ => throw new InvalidOperationException("Embedding service unavailable."));

        var exception = await Should.ThrowAsync<InvalidOperationException>(() => context.Service.TriggerAsync(documentId));

        exception.Message.ShouldContain("Embedding service unavailable");
        context.Jobs.Single().Status.ShouldBe(IngestionStatus.Failed);
        context.Jobs.Single().ErrorMessage.ShouldContain("Embedding service unavailable");
        context.Chunks.ShouldBeEmpty();
        context.Embeddings.ShouldBeEmpty();
    }

    private static DocumentChunkResult BuildChunkResult(string content, int sortOrder)
    {
        return new DocumentChunkResult
        {
            ActName = "Constitution of the Republic of South Africa",
            ChapterTitle = "Chapter 2",
            SectionNumber = $"1{sortOrder}",
            SectionTitle = "Rights",
            Content = content,
            TokenCount = 10,
            SortOrder = sortOrder,
            Strategy = ChunkStrategy.SectionLevel
        };
    }

    private sealed class EtlPipelineTestContext : IDisposable
    {
        private readonly string _seedFilePath;

        public List<IngestionJob> Jobs { get; } = new List<IngestionJob>();
        public List<LegalDocument> Documents { get; } = new List<LegalDocument>();
        public List<DocumentChunk> Chunks { get; } = new List<DocumentChunk>();
        public List<ChunkEmbedding> Embeddings { get; } = new List<ChunkEmbedding>();

        public IRepository<IngestionJob, Guid> JobRepository { get; } = Substitute.For<IRepository<IngestionJob, Guid>>();
        public IRepository<LegalDocument, Guid> DocumentRepository { get; } = Substitute.For<IRepository<LegalDocument, Guid>>();
        public IRepository<DocumentChunk, Guid> ChunkRepository { get; } = Substitute.For<IRepository<DocumentChunk, Guid>>();
        public IRepository<ChunkEmbedding, Guid> EmbeddingRepository { get; } = Substitute.For<IRepository<ChunkEmbedding, Guid>>();
        public IPdfIngestionAppService PdfIngestionAppService { get; } = Substitute.For<IPdfIngestionAppService>();
        public IEmbeddingAppService EmbeddingAppService { get; } = Substitute.For<IEmbeddingAppService>();
        public IChunkEnrichmentAppService ChunkEnrichmentAppService { get; } = Substitute.For<IChunkEnrichmentAppService>();

        public EtlPipelineAppService Service { get; }

        public EtlPipelineTestContext()
        {
            _seedFilePath = CreateSeedPdf();
            ConfigureRepositories();
            Service = new EtlPipelineAppService(
                JobRepository,
                DocumentRepository,
                ChunkRepository,
                EmbeddingRepository,
                PdfIngestionAppService,
                EmbeddingAppService,
                ChunkEnrichmentAppService);
        }

        public Guid SeedDocument()
        {
            var documentId = Guid.NewGuid();
            Documents.Add(new LegalDocument
            {
                Id = documentId,
                Title = "Employment Equity Act",
                Year = 1998,
                CategoryId = Guid.NewGuid(),
                FileName = Path.GetFileName(_seedFilePath),
                OriginalPdfId = Guid.NewGuid()
            });

            return documentId;
        }

        public void Dispose()
        {
            if (File.Exists(_seedFilePath))
            {
                File.Delete(_seedFilePath);
            }
        }

        private void ConfigureRepositories()
        {
            DocumentRepository.GetAll().Returns(_ => Documents.AsQueryable());
            JobRepository.GetAll().Returns(_ => Jobs.AsQueryable());
            ChunkRepository.GetAll().Returns(_ => Chunks.AsQueryable());
            EmbeddingRepository.GetAll().Returns(_ => Embeddings.AsQueryable());

            DocumentRepository.FirstOrDefaultAsync(Arg.Any<Guid>())
                .Returns(call => Task.FromResult(Documents.FirstOrDefault(x => x.Id == call.Arg<Guid>())!));

            DocumentRepository.GetAsync(Arg.Any<Guid>())
                .Returns(call => Task.FromResult(Documents.Single(x => x.Id == call.Arg<Guid>())));

            JobRepository.FirstOrDefaultAsync(Arg.Any<Guid>())
                .Returns(call => Task.FromResult(Jobs.FirstOrDefault(x => x.Id == call.Arg<Guid>())!));

            JobRepository.InsertAsync(Arg.Any<IngestionJob>())
                .Returns(call =>
                {
                    Jobs.Add(call.Arg<IngestionJob>());
                    return Task.FromResult(call.Arg<IngestionJob>());
                });

            JobRepository.UpdateAsync(Arg.Any<IngestionJob>())
                .Returns(call => Task.FromResult(call.Arg<IngestionJob>()));

            DocumentRepository.UpdateAsync(Arg.Any<LegalDocument>())
                .Returns(call => Task.FromResult(call.Arg<LegalDocument>()));

            ChunkRepository.InsertAsync(Arg.Any<DocumentChunk>())
                .Returns(call =>
                {
                    Chunks.Add(call.Arg<DocumentChunk>());
                    return Task.FromResult(call.Arg<DocumentChunk>());
                });

            ChunkRepository.DeleteAsync(Arg.Any<DocumentChunk>())
                .Returns(call =>
                {
                    Chunks.Remove(call.Arg<DocumentChunk>());
                    return Task.CompletedTask;
                });

            EmbeddingRepository.InsertAsync(Arg.Any<ChunkEmbedding>())
                .Returns(call =>
                {
                    Embeddings.Add(call.Arg<ChunkEmbedding>());
                    return Task.FromResult(call.Arg<ChunkEmbedding>());
                });
        }

        private static string CreateSeedPdf()
        {
            var root = FindRepoRoot();
            var directory = Path.Combine(root, "seed-data", "legislation");
            Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, $"etl-test-{Guid.NewGuid():N}.pdf");
            File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4 });
            return path;
        }

        private static string FindRepoRoot()
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current != null)
            {
                if (File.Exists(Path.Combine(current.FullName, "backend.sln")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            throw new InvalidOperationException("Could not locate backend root containing backend.sln.");
        }
    }
}
