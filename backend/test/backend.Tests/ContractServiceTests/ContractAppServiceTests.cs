#nullable enable
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using backend.Domains.ContractAnalysis;
using backend.Domains.QA;
using backend.Services.ContractService;
using backend.Services.ContractService.DTO;
using backend.Services.RagService;
using backend.Services.RagService.DTO;
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

    [Fact]
    public async Task GetAsync_SplitsStrengthsAndConcernsFromFlags()
    {
        var harness = new ContractAppServiceHarness();
        harness.Service.UseSession(42);
        var targetId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        harness.Analyses.Add(new ContractAnalysis
        {
            Id = targetId,
            UserId = 42,
            ExtractedText = "Lease agreement text",
            ContractType = ContractType.Lease,
            HealthScore = 78,
            Summary = "Balanced lease summary",
            Language = Language.English,
            AnalysedAt = DateTime.UtcNow,
            Flags = new List<ContractFlag>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ContractAnalysisId = targetId,
                    Severity = FlagSeverity.Green,
                    Title = "Clear maintenance split",
                    Description = "The maintenance duties are allocated clearly.",
                    ClauseText = "Routine repairs remain the landlord's duty.",
                    LegislationCitation = null,
                    SortOrder = 0
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ContractAnalysisId = targetId,
                    Severity = FlagSeverity.Red,
                    Title = "Broad penalty fee",
                    Description = "The penalty fee is unusually high.",
                    ClauseText = "A penalty of R15 000 applies for early exit.",
                    LegislationCitation = "Consumer Protection Act 68 of 2008 Section 14",
                    SortOrder = 1
                }
            }
        });

        var result = await harness.Service.GetAsync(targetId);

        result.Strengths.Count.ShouldBe(1);
        result.Strengths[0].Title.ShouldBe("Clear maintenance split");
        result.Concerns.Count.ShouldBe(1);
        result.Concerns[0].Title.ShouldBe("Broad penalty fee");
        result.Flags.Count.ShouldBe(2);
    }

    [Fact]
    public async Task AskAsync_OwnerScopedContract_ReturnsFollowUpAnswer()
    {
        var harness = new ContractAppServiceHarness();
        harness.Service.UseSession(42);
        var targetId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
        harness.SeedAnalysis(targetId, 42, ContractType.Lease, 68, DateTime.UtcNow, FlagSeverity.Amber);
        harness.FollowUpService.Result = new ContractFollowUpAnswerDto
        {
            AnswerText = "This clause needs review before you rely on it.",
            AnswerMode = RagAnswerMode.Cautious,
            ConfidenceBand = RagConfidenceBand.Medium,
            DetectedLanguageCode = "en",
            ContractExcerpts = new List<string> { "The tenant must give three months notice." },
            Citations = new List<RagCitationDto>()
        };

        var result = await harness.Service.AskAsync(targetId, new AskContractQuestionRequest
        {
            QuestionText = "Can they really require three months notice?",
            ResponseLanguageCode = "en"
        });

        result.AnswerMode.ShouldBe(RagAnswerMode.Cautious);
        result.ContractExcerpts.Count.ShouldBe(1);
    }

    private sealed class ContractAppServiceHarness
    {
        public Guid NextAnalysisId { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public List<ContractAnalysis> Analyses { get; } = new();
        public List<ContractFlag> Flags { get; } = new();
        public StubContractAnalysisService AnalysisService { get; }
        public StubContractFollowUpService FollowUpService { get; }
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
            FollowUpService = new StubContractFollowUpService();
            Service = new TestableContractAppService(
                contractAnalysisRepository,
                contractFlagRepository,
                AnalysisService,
                FollowUpService,
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
            ContractFollowUpService contractFollowUpService,
            List<ContractAnalysis> analyses)
            : base(contractAnalysisRepository, contractFlagRepository, contractAnalysisService, contractFollowUpService)
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

    private sealed class StubContractFollowUpService : ContractFollowUpService
    {
        public StubContractFollowUpService()
            : base(
                new StubContextBuilder(),
                Substitute.For<backend.Services.LanguageService.ILanguageAppService>(),
                Substitute.For<IHttpClientFactory>(),
                BuildConfig())
        {
        }

        public ContractFollowUpAnswerDto Result { get; set; } = new();

        public override Task<ContractFollowUpAnswerDto> AskAsync(
            ContractAnalysis analysis,
            string questionText,
            string responseLanguageCode = null)
        {
            return Task.FromResult(Result);
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
