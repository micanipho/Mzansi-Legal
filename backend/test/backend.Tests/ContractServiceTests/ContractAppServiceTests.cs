#nullable enable
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using backend.Domains.ContractAnalysis;
using backend.Domains.QA;
using backend.Services.ContractService;
using backend.Services.ContractService.DTO;
using backend.Services.RagService;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests.ContractServiceTests;

public class ContractAppServiceTests
{
    [Fact]
    public async Task AnalyseAsync_AuthenticatedUser_PersistsAnalysisAndFlags()
    {
        var harness = new ContractAppServiceHarness();
        harness.Service.UseSession(42);
        harness.AnalysisService.Result = (
            new ContractAnalysisDraft(
                ContractType.Lease,
                62,
                "This lease needs review.",
                Language.English,
                ContractCoverageState.PartialCoverage,
                new[]
                {
                    new ContractAnalysisFlagDraft(
                        FlagSeverity.Red,
                        "Notice period",
                        "The notice period appears too long.",
                        "The tenant must give three months notice.",
                        "Consumer Protection Act 68 of 2008 Section 14",
                        true)
                }),
            "Lease agreement text");

        var result = await harness.Service.AnalyseAsync(new AnalyseContractRequest
        {
            FileBytes = new byte[] { 1, 2, 3 },
            FileName = "lease.pdf",
            ContentType = "application/pdf",
            ResponseLanguageCode = "en"
        });

        result.Id.ShouldBe(harness.NextAnalysisId);
        result.RedFlagCount.ShouldBe(1);
        harness.Analyses.Count.ShouldBe(1);
        harness.Flags.Count.ShouldBe(1);
        harness.Analyses[0].UserId.ShouldBe(42);
        harness.Analyses[0].ExtractedText.ShouldBe("Lease agreement text");
        harness.Flags[0].ContractAnalysisId.ShouldBe(harness.NextAnalysisId);
    }

    [Fact]
    public async Task GetMyAsync_ReturnsOnlyOwnerRecordsNewestFirst()
    {
        var harness = new ContractAppServiceHarness();
        harness.Service.UseSession(42);
        harness.SeedAnalysis(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 42, ContractType.Lease, 62, DateTime.UtcNow.AddHours(-2), FlagSeverity.Red);
        harness.SeedAnalysis(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 7, ContractType.Credit, 48, DateTime.UtcNow.AddHours(-1), FlagSeverity.Red);
        harness.SeedAnalysis(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), 42, ContractType.Employment, 78, DateTime.UtcNow, FlagSeverity.Green);

        var result = await harness.Service.GetMyAsync();

        result.TotalCount.ShouldBe(2);
        result.Items.Select(item => item.Id).ShouldBe(new[]
        {
            Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
        });
    }

    [Fact]
    public async Task GetAsync_OtherUsersAnalysis_ThrowsUserFriendlyException()
    {
        var harness = new ContractAppServiceHarness();
        harness.Service.UseSession(42);
        var targetId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        harness.SeedAnalysis(targetId, 7, ContractType.Credit, 48, DateTime.UtcNow, FlagSeverity.Red);

        await Should.ThrowAsync<UserFriendlyException>(() => harness.Service.GetAsync(targetId));
    }

    private sealed class ContractAppServiceHarness
    {
        public Guid NextAnalysisId { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public List<ContractAnalysis> Analyses { get; } = new();
        public List<ContractFlag> Flags { get; } = new();
        public StubContractAnalysisService AnalysisService { get; }
        public TestableContractAppService Service { get; }

        public ContractAppServiceHarness()
        {
            var contractAnalysisRepository = Substitute.For<IRepository<ContractAnalysis, Guid>>();
            var contractFlagRepository = Substitute.For<IRepository<ContractFlag, Guid>>();

            contractAnalysisRepository.GetAll().Returns(_ => Analyses.AsQueryable());
            contractAnalysisRepository.InsertAndGetIdAsync(Arg.Any<ContractAnalysis>())
                .Returns(call =>
                {
                    var entity = call.Arg<ContractAnalysis>();
                    entity.Id = NextAnalysisId;
                    Analyses.Add(entity);
                    return Task.FromResult(NextAnalysisId);
                });

            contractFlagRepository.InsertAsync(Arg.Any<ContractFlag>())
                .Returns(call =>
                {
                    var entity = call.Arg<ContractFlag>();
                    Flags.Add(entity);
                    return Task.FromResult(entity);
                });

            AnalysisService = new StubContractAnalysisService();
            Service = new TestableContractAppService(
                contractAnalysisRepository,
                contractFlagRepository,
                AnalysisService,
                Analyses);
        }

        public void SeedAnalysis(Guid id, long userId, ContractType contractType, int healthScore, DateTime analysedAt, FlagSeverity severity)
        {
            Analyses.Add(new ContractAnalysis
            {
                Id = id,
                UserId = userId,
                ExtractedText = $"{contractType} agreement text",
                ContractType = contractType,
                HealthScore = healthScore,
                Summary = $"{contractType} summary",
                Language = Language.English,
                AnalysedAt = analysedAt,
                Flags = new List<ContractFlag>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ContractAnalysisId = id,
                        Severity = severity,
                        Title = "Flag title",
                        Description = "Flag description",
                        ClauseText = "Clause text",
                        LegislationCitation = severity == FlagSeverity.Green ? null : "Consumer Protection Act 68 of 2008 Section 14",
                        SortOrder = 0
                    }
                }
            });
        }
    }

    private sealed class StubContractAnalysisService : ContractAnalysisService
    {
        public StubContractAnalysisService()
            : base(
                new StubContextBuilder(),
                Substitute.For<IHttpClientFactory>(),
                BuildConfig())
        {
        }

        public (ContractAnalysisDraft Analysis, string ExtractedText) Result { get; set; }

        public override Task<(ContractAnalysisDraft Analysis, string ExtractedText)> AnalyseAsync(
            byte[] fileBytes,
            string fileName,
            string contentType,
            Language responseLanguage)
        {
            return Task.FromResult(Result);
        }
    }

    private sealed class StubContextBuilder : ContractLegislationContextBuilder
    {
        public StubContextBuilder()
            : base(
                Substitute.For<backend.Services.EmbeddingService.IEmbeddingAppService>(),
                Substitute.For<IRepository<backend.Domains.LegalDocuments.DocumentChunk, Guid>>(),
                new RagIndexStore())
        {
        }

        public override Task<ContractLegislationContext> BuildAsync(ContractType contractType, string extractedText)
        {
            return Task.FromResult(new ContractLegislationContext(
                Array.Empty<RetrievedChunk>(),
                Array.Empty<RetrievedChunk>(),
                ContractCoverageState.PartialCoverage,
                "stub"));
        }
    }

    private sealed class TestableContractAppService : ContractAppService
    {
        public TestableContractAppService(
            IRepository<ContractAnalysis, Guid> contractAnalysisRepository,
            IRepository<ContractFlag, Guid> contractFlagRepository,
            ContractAnalysisService contractAnalysisService,
            List<ContractAnalysis> analyses)
            : base(contractAnalysisRepository, contractFlagRepository, contractAnalysisService)
        {
            _ = analyses;
        }

        public void UseSession(long? userId)
        {
            var session = Substitute.For<IAbpSession>();
            session.UserId.Returns(userId);
            AbpSession = session;
        }

        protected override Task<List<ContractAnalysis>> ListAnalysesAsync(IQueryable<ContractAnalysis> query)
        {
            return Task.FromResult(query.ToList());
        }

        protected override Task<ContractAnalysis?> FirstOrDefaultOwnedAnalysisAsync(IQueryable<ContractAnalysis> query)
        {
            return Task.FromResult(query.FirstOrDefault());
        }
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
}
