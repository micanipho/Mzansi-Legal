#nullable enable
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using backend.Domains.LegalDocuments;
using backend.Domains.QA;
using backend.Services.EmbeddingService;
using backend.Services.EmbeddingService.DTO;
using backend.Services.LanguageService;
using backend.Services.RagService;
using backend.Services.RagService.DTO;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests.RagServiceTests;

public class RagAppServiceTests
{
    [Fact]
    public async Task AskAsync_AuthenticatedGroundedAnswer_PersistsQuestionAndAnswer()
    {
        var service = CreateService(
            CreateGroundedHousingChunks(),
            new float[] { 1f, 0f },
            Language.Zulu,
            "Can my landlord evict me without a court order?",
            "Cha. Umnikazi wendlu akakwazi ukukuxosha ngaphandle kwenqubo yasenkantolo [Constitution of the Republic of South Africa, Section 26(3)].");
        service.UseSession(42);

        var result = await service.AskAsync(new AskQuestionRequest
        {
            QuestionText = "Ingabe umnikazi wendlu angangixosha?"
        });

        result.AnswerText.ShouldContain("Constitution of the Republic of South Africa");
        result.DetectedLanguageCode.ShouldBe("zu");
        result.AnswerMode.ShouldNotBe(RagAnswerMode.Clarification);
        result.AnswerMode.ShouldNotBe(RagAnswerMode.Insufficient);
        result.AnswerId.ShouldBe(service.NextAnswerId);
        result.Citations.ShouldNotBeEmpty();

        service.PersistedQuestionCount.ShouldBe(1);
        service.PersistedAnswerCount.ShouldBe(1);
        service.LastOriginalText.ShouldBe("Ingabe umnikazi wendlu angangixosha?");
        service.LastTranslatedText.ShouldBe("Can my landlord evict me without a court order?");
        service.LastQuestionLanguage.ShouldBe(Language.Zulu);
        service.LastAnswerQuestionId.ShouldBe(service.NextQuestionId);
        service.LastPersistedChunkIds.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task AskAsync_AuthenticatedInsufficientResponse_PersistsQuestionWithoutAnswer()
    {
        var service = CreateService(
            CreateEmploymentChunks(),
            new float[] { 0f, 1f },
            Language.Afrikaans,
            "Can my landlord evict me without a court order?",
            "unused");
        service.UseSession(42);

        var result = await service.AskAsync(new AskQuestionRequest
        {
            QuestionText = "Kan my verhuurder my uitsit?"
        });

        result.AnswerMode.ShouldBe(RagAnswerMode.Insufficient);
        result.AnswerId.ShouldBeNull();
        result.Citations.ShouldBeEmpty();
        result.ChunkIds.ShouldBeEmpty();
        result.DetectedLanguageCode.ShouldBe("af");

        service.PersistedQuestionCount.ShouldBe(1);
        service.PersistedAnswerCount.ShouldBe(0);
        service.LastOriginalText.ShouldBe("Kan my verhuurder my uitsit?");
        service.LastTranslatedText.ShouldBe("Can my landlord evict me without a court order?");
        service.LastQuestionLanguage.ShouldBe(Language.Afrikaans);
    }

    [Fact]
    public void ShouldPersistAnswer_OnlyGroundedModesReturnTrue()
    {
        RagAppService.ShouldPersistAnswer(RagAnswerMode.Direct).ShouldBeTrue();
        RagAppService.ShouldPersistAnswer(RagAnswerMode.Cautious).ShouldBeTrue();
        RagAppService.ShouldPersistAnswer(RagAnswerMode.Clarification).ShouldBeFalse();
        RagAppService.ShouldPersistAnswer(RagAnswerMode.Insufficient).ShouldBeFalse();
    }

    [Fact]
    public void BuildNonGroundedResult_ClarificationMode_ReturnsClarificationQuestionWithoutPersistence()
    {
        const string clarificationQuestion = "Can you share whether this is about a rented home or a workplace issue?";

        var result = RagAppService.BuildNonGroundedResult(
            Language.English,
            "en",
            RagAnswerMode.Clarification,
            RagConfidenceBand.Low,
            clarificationQuestion);

        result.AnswerMode.ShouldBe(RagAnswerMode.Clarification);
        result.IsInsufficientInformation.ShouldBeTrue();
        result.AnswerId.ShouldBeNull();
        result.Citations.ShouldBeEmpty();
        result.ChunkIds.ShouldBeEmpty();
        result.ClarificationQuestion.ShouldBe(clarificationQuestion);
        result.AnswerText.ShouldContain("need one detail first");
    }

    [Fact]
    public void BuildNonGroundedResult_UrgentInsufficientMode_FlagsUrgentAttention()
    {
        var result = RagAppService.BuildNonGroundedResult(
            Language.English,
            "en",
            RagAnswerMode.Insufficient,
            RagConfidenceBand.Low,
            null,
            requiresUrgentAttention: true);

        result.AnswerMode.ShouldBe(RagAnswerMode.Insufficient);
        result.RequiresUrgentAttention.ShouldBeTrue();
        result.AnswerText.ShouldContain("seek official or legal help right away");
    }

    [Fact]
    public void BuildNonGroundedResult_InsufficientMode_RemovesGeneralKnowledgeFallbackBehavior()
    {
        var result = RagAppService.BuildNonGroundedResult(
            Language.English,
            "en",
            RagAnswerMode.Insufficient,
            RagConfidenceBand.Low,
            null);

        result.AnswerMode.ShouldBe(RagAnswerMode.Insufficient);
        result.IsInsufficientInformation.ShouldBeTrue();
        result.AnswerText.ShouldContain("can't answer this responsibly");
        result.AnswerText.ShouldContain("legal grounding");
        result.AnswerText.ShouldNotContain("general AI knowledge");
        result.Citations.ShouldBeEmpty();
        result.ChunkIds.ShouldBeEmpty();
        result.AnswerId.ShouldBeNull();
        result.ClarificationQuestion.ShouldBeNull();
    }

    [Fact]
    public void CreateCitations_MultiSourceChunks_ReturnsOneCitationPerSourceChunk()
    {
        var citations = RagAppService.CreateCitations(new[]
        {
            new RetrievedChunk(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Constitution of the Republic of South Africa",
                "Constitution",
                "Housing",
                "Section 26(3)",
                "Housing",
                "No one may be evicted without a court order.",
                "Housing Rights",
                Array.Empty<string>(),
                0.92f,
                0.92f,
                30,
                "Constitution of the Republic of South Africa",
                "Section 26(3)",
                RagSourceMetadata.BindingLaw,
                RagSourceMetadata.Primary),
            new RetrievedChunk(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Rental Housing Act 50 of 1999",
                "Rental Housing Act",
                "Housing",
                "Section 4",
                "Rental agreements",
                "A landlord and tenant are bound by the rental agreement.",
                "Rental Housing",
                Array.Empty<string>(),
                0.81f,
                0.81f,
                28,
                "Rental Housing Act 50 of 1999",
                "Section 4",
                RagSourceMetadata.BindingLaw,
                RagSourceMetadata.Supporting)
        });

        citations.Count.ShouldBe(2);
        citations.Select(citation => citation.ActName).ShouldContain("Constitution of the Republic of South Africa");
        citations.Select(citation => citation.ActName).ShouldContain("Rental Housing Act 50 of 1999");
        citations.ShouldContain(citation =>
            citation.AuthorityType == RagSourceMetadata.BindingLaw &&
            citation.SourceRole == RagSourceMetadata.Primary);
        citations.ShouldContain(citation =>
            citation.AuthorityType == RagSourceMetadata.BindingLaw &&
            citation.SourceRole == RagSourceMetadata.Supporting);
    }

    private static TestableRagAppService CreateService(
        IReadOnlyList<IndexedChunk> loadedChunks,
        float[] questionVector,
        Language detectedLanguage,
        string translatedText,
        string chatResponse)
    {
        var store = new RagIndexStore();
        store.Replace(loadedChunks, new RagDocumentProfileBuilder().Build(loadedChunks).ToList());

        var embeddingService = Substitute.For<IEmbeddingAppService>();
        embeddingService.GenerateEmbeddingAsync(Arg.Any<string>()).Returns(_ => new EmbeddingResult
        {
            Vector = questionVector,
            Model = "text-embedding-3-small",
            InputCharacterCount = translatedText.Length
        });

        var languageService = Substitute.For<ILanguageAppService>();
        languageService.DetectLanguageAsync(Arg.Any<string>()).Returns(detectedLanguage);
        languageService.TranslateToEnglishAsync(Arg.Any<string>(), detectedLanguage).Returns(translatedText);

        var httpFactory = Substitute.For<IHttpClientFactory>();
        httpFactory.CreateClient("OpenAI").Returns(_ => new HttpClient(new StubChatHandler(chatResponse))
        {
            BaseAddress = new Uri("https://api.openai.com/")
        });

        return new TestableRagAppService(
            embeddingService,
            languageService,
            Substitute.For<IRepository<DocumentChunk, Guid>>(),
            Substitute.For<IRepository<Conversation, Guid>>(),
            Substitute.For<IRepository<Question, Guid>>(),
            Substitute.For<IRepository<Answer, Guid>>(),
            Substitute.For<IRepository<AnswerCitation, Guid>>(),
            store,
            httpFactory,
            BuildConfig());
    }

    private static IConfiguration BuildConfig()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAI:ApiKey"] = "sk-test",
                ["OpenAI:ChatModel"] = "gpt-4o",
                ["OpenAI:BaseUrl"] = "https://api.openai.com/"
            })
            .Build();
    }

    private static IReadOnlyList<IndexedChunk> CreateGroundedHousingChunks()
    {
        var constitutionId = Guid.NewGuid();

        return new[]
        {
            CreateChunk(
                "Constitution of the Republic of South Africa",
                "Constitution",
                "Housing",
                "No one may be evicted from their home without a court order.",
                "Housing rights",
                new[] { "landlord", "evict", "court", "home" },
                new float[] { 1f, 0f },
                constitutionId,
                "Section 26(3)",
                "Housing"),
            CreateChunk(
                "Constitution of the Republic of South Africa",
                "Constitution",
                "Housing",
                "A court must consider all the relevant circumstances before granting an eviction order.",
                "Housing rights",
                new[] { "court", "eviction", "order", "housing" },
                new float[] { 0.98f, 0.02f },
                constitutionId,
                "Section 26(3)",
                "Housing"),
            CreateChunk(
                "Labour Relations Act",
                "LRA",
                "Employment",
                "Employees have the right not to be unfairly dismissed.",
                "Employment rights",
                new[] { "dismissal", "employee" },
                new float[] { 0f, 1f })
        };
    }

    private static IReadOnlyList<IndexedChunk> CreateEmploymentChunks()
    {
        return new[]
        {
            CreateChunk(
                "Labour Relations Act",
                "LRA",
                "Employment",
                "Employees have the right not to be unfairly dismissed.",
                "Employment rights",
                new[] { "dismissal", "employee" },
                new float[] { 1f, 0f })
        };
    }

    private static IndexedChunk CreateChunk(
        string actName,
        string shortName,
        string categoryName,
        string excerpt,
        string topicClassification,
        IReadOnlyList<string> keywords,
        float[] vector,
        Guid? documentId = null,
        string sectionNumber = "Section 1",
        string sectionTitle = "General")
    {
        return new IndexedChunk(
            Guid.NewGuid(),
            documentId ?? Guid.NewGuid(),
            actName,
            shortName,
            "1",
            1996,
            categoryName,
            sectionNumber,
            sectionTitle,
            excerpt,
            keywords,
            topicClassification,
            50,
            vector);
    }

    private sealed class StubChatHandler : HttpMessageHandler
    {
        private readonly string _content;

        public StubChatHandler(string content)
        {
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Serialize(new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            content = _content
                        }
                    }
                }
            });

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class TestableRagAppService : RagAppService
    {
        public TestableRagAppService(
            IEmbeddingAppService embeddingService,
            ILanguageAppService languageService,
            IRepository<DocumentChunk, Guid> chunkRepository,
            IRepository<Conversation, Guid> conversationRepository,
            IRepository<Question, Guid> questionRepository,
            IRepository<Answer, Guid> answerRepository,
            IRepository<AnswerCitation, Guid> citationRepository,
            RagIndexStore ragIndexStore,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
            : base(
                embeddingService,
                languageService,
                chunkRepository,
                conversationRepository,
                questionRepository,
                answerRepository,
                citationRepository,
                ragIndexStore,
                httpClientFactory,
                configuration)
        {
        }

        public Guid NextQuestionId { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public Guid NextAnswerId { get; } = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public int PersistedQuestionCount { get; private set; }
        public int PersistedAnswerCount { get; private set; }
        public string? LastOriginalText { get; private set; }
        public string? LastTranslatedText { get; private set; }
        public Language? LastQuestionLanguage { get; private set; }
        public Guid? LastAnswerQuestionId { get; private set; }
        public IReadOnlyList<Guid> LastPersistedChunkIds { get; private set; } = Array.Empty<Guid>();

        public void UseSession(long? userId)
        {
            var session = Substitute.For<IAbpSession>();
            session.UserId.Returns(userId);
            AbpSession = session;
        }

        protected override Task<Guid> PersistQuestionAsync(
            long userId,
            string originalText,
            string translatedText,
            Language language)
        {
            PersistedQuestionCount++;
            LastOriginalText = originalText;
            LastTranslatedText = translatedText;
            LastQuestionLanguage = language;
            return Task.FromResult(NextQuestionId);
        }

        protected override Task<Guid> PersistAnswerAsync(
            Guid questionId,
            string answerText,
            Language language,
            IEnumerable<RetrievedChunk> usedChunks)
        {
            PersistedAnswerCount++;
            LastAnswerQuestionId = questionId;
            LastPersistedChunkIds = usedChunks.Select(chunk => chunk.ChunkId).ToList();
            return Task.FromResult(NextAnswerId);
        }
    }
}
