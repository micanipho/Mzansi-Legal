#nullable enable
using Abp.Domain.Repositories;
using Abp.UI;
using backend.Domains.ContractAnalysis;
using backend.Domains.LegalDocuments;
using backend.Domains.QA;
using backend.Services.ContractService;
using backend.Services.EmbeddingService;
using backend.Services.RagService;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests.ContractServiceTests;

public class ContractAnalysisServiceTests
{
    [Fact]
    public async Task AnalyseAsync_ReadablePdf_UsesDirectExtractionWithoutOcr()
    {
        var context = BuildContext();
        var service = new TestableContractAnalysisService(
            context,
            new string('A', 140),
            new string('B', 180),
            ContractType.Lease,
            "{\"healthScore\":62,\"summary\":\"This lease needs review.\",\"flags\":[{\"severity\":\"red\",\"title\":\"Notice period\",\"description\":\"The notice period appears too long.\",\"clauseText\":\"The tenant must give three months notice.\",\"legislationCitation\":\"Consumer Protection Act 68 of 2008 Section 14\"}]}");

        var result = await service.AnalyseAsync(new byte[] { 1, 2, 3 }, "lease.pdf", "application/pdf", Language.English);

        result.Analysis.ContractType.ShouldBe(ContractType.Lease);
        result.Analysis.HealthScore.ShouldBe(62);
        result.ExtractedText.ShouldBe(new string('A', 140));
        result.Analysis.Flags.Count.ShouldBe(1);
        result.Analysis.Flags[0].Severity.ShouldBe(FlagSeverity.Red);
        service.OcrCallCount.ShouldBe(0);
    }

    [Fact]
    public async Task AnalyseAsync_ThinDirectText_UsesOcrFallback()
    {
        var context = BuildContext();
        var service = new TestableContractAnalysisService(
            context,
            "too short",
            new string('O', 180),
            ContractType.Credit,
            "{\"healthScore\":48,\"summary\":\"This credit agreement needs review.\",\"flags\":[]}");

        var result = await service.AnalyseAsync(new byte[] { 1, 2, 3 }, "credit.pdf", "application/pdf", Language.English);

        result.ExtractedText.ShouldBe(new string('O', 180));
        result.Analysis.ContractType.ShouldBe(ContractType.Credit);
        service.OcrCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task AnalyseAsync_WhenGroundingIsWeak_DowngradesRedFlagToAmber()
    {
        var context = new ContractLegislationContext(
            Array.Empty<RetrievedChunk>(),
            Array.Empty<RetrievedChunk>(),
            ContractCoverageState.PartialCoverage,
            "Only partial grounding is available.");
        var service = new TestableContractAnalysisService(
            context,
            new string('A', 140),
            new string('B', 180),
            ContractType.Service,
            "{\"healthScore\":55,\"summary\":\"Needs review.\",\"flags\":[{\"severity\":\"red\",\"title\":\"Liability\",\"description\":\"This clause is broad.\",\"clauseText\":\"The customer carries all risk.\",\"legislationCitation\":\"Unknown Act, Section 1\"}]}");

        var result = await service.AnalyseAsync(new byte[] { 1, 2, 3 }, "service.pdf", "application/pdf", Language.English);

        result.Analysis.Flags.Count.ShouldBe(1);
        result.Analysis.Flags[0].Severity.ShouldBe(FlagSeverity.Amber);
        result.Analysis.Flags[0].LegislationCitation.ShouldBeNull();
        result.Analysis.Flags[0].Description.ShouldContain("partially");
    }

    [Fact]
    public async Task AnalyseAsync_WhenReadableTextRemainsTooThin_ThrowsUserFriendlyException()
    {
        var service = new TestableContractAnalysisService(
            BuildContext(),
            "too short",
            "still too short",
            ContractType.Lease,
            "{\"healthScore\":62,\"summary\":\"unused\",\"flags\":[]}");

        var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
            service.AnalyseAsync(new byte[] { 1, 2, 3 }, "scan.pdf", "application/pdf", Language.English));

        exception.Message.ShouldContain("clearer PDF");
        service.OcrCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task AnalyseAsync_WhenContractTypeUnsupported_ThrowsUserFriendlyException()
    {
        var service = new TestableContractAnalysisService(
            BuildContext(),
            new string('A', 140),
            new string('B', 180),
            null,
            "{\"healthScore\":62,\"summary\":\"unused\",\"flags\":[]}");

        var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
            service.AnalyseAsync(new byte[] { 1, 2, 3 }, "unknown.pdf", "application/pdf", Language.English));

        exception.Message.ShouldContain("not currently supported");
    }

    private static ContractLegislationContext BuildContext()
    {
        return new ContractLegislationContext(
            new[]
            {
                new RetrievedChunk(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "Consumer Protection Act 68 of 2008",
                    "CPA",
                    "Consumer",
                    "Section 14",
                    "Fixed-term agreements",
                    "A consumer may cancel a fixed-term agreement on 20 business days' notice.",
                    "Contracts",
                    Array.Empty<string>(),
                    0.9f,
                    0.9f,
                    42,
                    "Consumer Protection Act 68 of 2008",
                    "Section 14",
                    RagSourceMetadata.BindingLaw,
                    RagSourceMetadata.Primary)
            },
            Array.Empty<RetrievedChunk>(),
            ContractCoverageState.PartialCoverage,
            "The current legislation corpus covers the main baseline issues.");
    }

    private static IConfiguration BuildConfig()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAI:ApiKey"] = "sk-test",
                ["OpenAI:ChatModel"] = "gpt-4o"
            })
            .Build();
    }

    private sealed class TestableContractAnalysisService : ContractAnalysisService
    {
        private readonly string _directText;
        private readonly string _ocrText;
        private readonly ContractType? _contractType;
        private readonly string _analysisResponse;

        public TestableContractAnalysisService(
            ContractLegislationContext context,
            string directText,
            string ocrText,
            ContractType? contractType,
            string analysisResponse)
            : base(
                new StubContextBuilder(context),
                Substitute.For<IHttpClientFactory>(),
                BuildConfig())
        {
            _directText = directText;
            _ocrText = ocrText;
            _contractType = contractType;
            _analysisResponse = analysisResponse;
        }

        public int OcrCallCount { get; private set; }

        protected override string ExtractDirectText(byte[] fileBytes) => _directText;

        protected override Task<string> ExtractTextWithOcrAsync(byte[] fileBytes, string fileName)
        {
            OcrCallCount++;
            return Task.FromResult(_ocrText);
        }

        protected override Task<ContractType> DetectContractTypeAsync(string extractedText)
        {
            if (!_contractType.HasValue)
            {
                throw new UserFriendlyException("This contract type is not currently supported. Please upload an employment, lease, credit, or service contract.");
            }

            return Task.FromResult(_contractType.Value);
        }

        protected override Task<string> CallChatCompletionsAsync(string systemPrompt, string userPrompt, double temperature)
        {
            return Task.FromResult(_analysisResponse);
        }
    }

    private sealed class StubContextBuilder : ContractLegislationContextBuilder
    {
        private readonly ContractLegislationContext _context;

        public StubContextBuilder(ContractLegislationContext context)
            : base(
                Substitute.For<IEmbeddingAppService>(),
                Substitute.For<IRepository<DocumentChunk, Guid>>(),
                new RagIndexStore())
        {
            _context = context;
        }

        public override Task<ContractLegislationContext> BuildAsync(ContractType contractType, string extractedText)
        {
            return Task.FromResult(_context);
        }
    }
}
