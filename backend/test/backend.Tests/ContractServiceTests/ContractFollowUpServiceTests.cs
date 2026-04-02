#nullable enable
using backend.Domains.ContractAnalysis;
using backend.Domains.QA;
using backend.Services.ContractService;
using backend.Services.ContractService.DTO;
using backend.Services.EmbeddingService;
using backend.Services.LanguageService;
using backend.Services.RagService;
using backend.Services.RagService.DTO;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests.ContractServiceTests;

public class ContractFollowUpServiceTests
{
    [Fact]
    public async Task AskAsync_WithGroundedSupport_ReturnsCautiousOrDirectAnswer()
    {
        var service = new TestableContractFollowUpService(
            BuildContext(ContractCoverageState.InCorpusNow),
            "The tenant must give three calendar months written notice.\nRoutine repairs remain the landlord's duty.",
            "This clause needs review before you rely on it. [Consumer Protection Act 68 of 2008, Section 14]");

        var result = await service.AskAsync(
            BuildAnalysis(),
            "Can the landlord require three months notice?",
            "en");

        result.AnswerMode.ShouldBe(RagAnswerMode.Direct);
        result.Citations.Count.ShouldBe(1);
        result.ContractExcerpts.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task AskAsync_WithNoGrounding_ReturnsInsufficientAnswer()
    {
        var service = new TestableContractFollowUpService(
            new ContractLegislationContext(
                Array.Empty<RetrievedChunk>(),
                Array.Empty<RetrievedChunk>(),
                ContractCoverageState.NeedsCorpusExpansion,
                "No support."),
            "The tenant must give three calendar months written notice.",
            "unused");

        var result = await service.AskAsync(
            BuildAnalysis(),
            "Can the landlord require three months notice?",
            "en");

        result.AnswerMode.ShouldBe(RagAnswerMode.Insufficient);
        result.Citations.Count.ShouldBe(0);
    }

    [Fact]
    public void SelectRelevantExcerpts_PrefersQuestionAlignedLines()
    {
        var excerpts = ContractFollowUpService.SelectRelevantExcerpts(
            "The tenant must give three calendar months written notice.\nRoutine repairs remain the landlord's duty.\nParking is available in bay 4.",
            "Can they require three months notice?");

        excerpts.Count.ShouldBeGreaterThan(0);
        excerpts[0].ShouldContain("three calendar months");
    }

    private static ContractAnalysis BuildAnalysis() => new()
    {
        Id = Guid.NewGuid(),
        UserId = 42,
        ExtractedText = "The tenant must give three calendar months written notice.\nRoutine repairs remain the landlord's duty.",
        ContractType = ContractType.Lease,
        HealthScore = 61,
        Summary = "Lease needs review.",
        Language = Language.English,
        AnalysedAt = DateTime.UtcNow,
        Flags = new List<ContractFlag>()
    };

    private static ContractLegislationContext BuildContext(ContractCoverageState coverageState)
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
            coverageState,
            "Coverage notes.");
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

    private sealed class TestableContractFollowUpService : ContractFollowUpService
    {
        private readonly ContractLegislationContext _context;
        private readonly string _analysisResponse;

        public TestableContractFollowUpService(
            ContractLegislationContext context,
            string translatedQuestionText,
            string analysisResponse)
            : base(
                new StubContextBuilder(context),
                new StubLanguageAppService(translatedQuestionText),
                Substitute.For<IHttpClientFactory>(),
                BuildConfig())
        {
            _context = context;
            _analysisResponse = analysisResponse;
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
                Substitute.For<Abp.Domain.Repositories.IRepository<backend.Domains.LegalDocuments.DocumentChunk, Guid>>(),
                new RagIndexStore())
        {
            _context = context;
        }

        public override Task<ContractLegislationContext> BuildForFollowUpAsync(
            ContractType contractType,
            string extractedText,
            string followUpQuestion)
        {
            return Task.FromResult(_context);
        }
    }

    private sealed class StubLanguageAppService : ILanguageAppService
    {
        private readonly string _translatedQuestionText;

        public StubLanguageAppService(string translatedQuestionText)
        {
            _translatedQuestionText = translatedQuestionText;
        }

        public Task<Language> DetectLanguageAsync(string text) => Task.FromResult(Language.English);

        public Task<string> TranslateToEnglishAsync(string text, Language sourceLanguage) =>
            Task.FromResult(_translatedQuestionText);
    }
}
