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
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests.RagServiceTests;

public class RagAppServiceTests
{
    [Fact]
    public async Task AskAsync_AuthenticatedGroundedAnswer_AnswerAndCitationsArePersisted()
    {
        var harness = CreateHarness(
            CreateGroundedHousingChunks(),
            new float[] { 1f, 0f },
            Language.Zulu,
            "Can my landlord evict me without a court order?",
            "Cha. Umnikazi wendlu akakwazi ukukuxosha ngaphandle kwenqubo yasenkantolo [Constitution of the Republic of South Africa, Section 26(3)].");
        harness.Service.UseSession(42);

        var result = await harness.Service.AskAsync(new AskQuestionRequest
        {
            QuestionText = "Ingabe umnikazi wendlu angangixosha?"
        });

        result.AnswerText.ShouldContain("Constitution of the Republic of South Africa");
        result.DetectedLanguageCode.ShouldBe("zu");
        result.AnswerMode.ShouldNotBe(RagAnswerMode.Clarification);
        result.AnswerMode.ShouldNotBe(RagAnswerMode.Insufficient);
        result.AnswerId.ShouldBe(harness.Service.NextAnswerId);
        result.AnswerId.ShouldNotBeNull();
        result.Citations.ShouldNotBeEmpty();
        result.PrimarySourceTitle.ShouldBe("Constitution of the Republic of South Africa");
        result.PrimarySourceLocator.ShouldBe("Section 26(3)");
        result.PrimaryAuthorityType.ShouldBe(RagSourceMetadata.BindingLaw);
        result.HasSupportingSources.ShouldBeFalse();

        harness.Service.PersistedQuestionCount.ShouldBe(1);
        harness.Service.PersistedAnswerCount.ShouldBe(1);
        harness.Service.LastOriginalText.ShouldBe("Ingabe umnikazi wendlu angangixosha?");
        harness.Service.LastTranslatedText.ShouldBe("Can my landlord evict me without a court order?");
        harness.Service.LastQuestionLanguage.ShouldBe(Language.Zulu);
        harness.Service.LastAnswerQuestionId.ShouldBe(harness.Service.NextQuestionId);
        harness.Service.LastPersistedChunkIds.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task AskAsync_AuthenticatedInsufficientResponse_PersistsQuestionWithoutAnswer()
    {
        var harness = CreateHarness(
            CreateEmploymentChunks(),
            new float[] { 0f, 1f },
            Language.Afrikaans,
            "Can my landlord evict me without a court order?",
            "unused");
        harness.Service.UseSession(42);
        var suppliedConversationId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var result = await harness.Service.AskAsync(new AskQuestionRequest
        {
            QuestionText = "Kan my verhuurder my uitsit?",
            ConversationId = suppliedConversationId
        });

        result.AnswerMode.ShouldBe(RagAnswerMode.Insufficient);
        result.AnswerId.ShouldBe(harness.Service.NextAnswerId);
        result.Citations.ShouldBeEmpty();
        result.ChunkIds.ShouldBeEmpty();
        result.DetectedLanguageCode.ShouldBe("af");
        result.PrimarySourceTitle.ShouldBeNull();
        result.PrimarySourceLocator.ShouldBeNull();
        result.PrimaryAuthorityType.ShouldBeNull();
        result.HasSupportingSources.ShouldBeFalse();

        harness.Service.PersistedQuestionCount.ShouldBe(1);
        harness.Service.PersistedAnswerCount.ShouldBe(1);
        harness.Service.LastConversationId.ShouldBeNull();
        harness.Service.LastOriginalText.ShouldBe("Kan my verhuurder my uitsit?");
        harness.Service.LastTranslatedText.ShouldBe("Can my landlord evict me without a court order?");
        harness.Service.LastQuestionLanguage.ShouldBe(Language.Afrikaans);
    }

    [Fact]
    public async Task AskAsync_AnonymousUser_NoPersistenceOccurs()
    {
        var harness = CreateHarness(
            CreateGroundedHousingChunks(),
            new float[] { 1f, 0f },
            Language.English,
            "Can my landlord evict me without a court order?",
            "No. A landlord cannot evict you without a court order.");
        harness.Service.UseSession(null);

        var result = await harness.Service.AskAsync(new AskQuestionRequest
        {
            QuestionText = "Can my landlord evict me without a court order?"
        });

        result.AnswerId.ShouldBeNull();
        harness.Service.PersistedQuestionCount.ShouldBe(0);
        harness.Service.PersistedAnswerCount.ShouldBe(0);
    }

    [Fact]
    public async Task AskAsync_WithValidConversationId_ReusesThatConversation()
    {
        var harness = CreateHarness(
            CreateGroundedHousingChunks(),
            new float[] { 1f, 0f },
            Language.English,
            "Can my landlord evict me without a court order?",
            "No. A landlord cannot evict you without a court order.");
        var suppliedConversationId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        harness.SeedConversation(
            suppliedConversationId,
            42,
            DateTime.UtcNow.AddMinutes(-10),
            "Can my landlord evict me without a court order?");
        harness.Service.UseSession(42);
        harness.Service.UseBaseQuestionPersistence = true;

        var result = await harness.Service.AskAsync(new AskQuestionRequest
        {
            QuestionText = "What if they change the locks?",
            ConversationId = suppliedConversationId
        });

        harness.Service.LastConversationId.ShouldBe(suppliedConversationId);
        harness.Conversations.Count.ShouldBe(1);
        harness.Questions.Count.ShouldBe(1);
        harness.Questions[0].ConversationId.ShouldBe(suppliedConversationId);
        result.ConversationId.ShouldBe(suppliedConversationId);
    }

    [Fact]
    public async Task AskAsync_WithConversationIdBelongingToAnotherUser_CreatesNewConversation()
    {
        var harness = CreateHarness(
            CreateGroundedHousingChunks(),
            new float[] { 1f, 0f },
            Language.English,
            "Can my landlord evict me without a court order?",
            "No. A landlord cannot evict you without a court order.");
        var foreignConversationId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        harness.SeedConversation(
            foreignConversationId,
            7,
            DateTime.UtcNow.AddMinutes(-5),
            "Can my landlord evict me without a court order?");
        harness.Service.UseSession(42);
        harness.Service.UseBaseQuestionPersistence = true;

        var result = await harness.Service.AskAsync(new AskQuestionRequest
        {
            QuestionText = "What if they disconnect the water?",
            ConversationId = foreignConversationId
        });

        harness.Conversations.Count.ShouldBe(2);
        harness.Questions.Count.ShouldBe(1);
        harness.Questions[0].ConversationId.ShouldBe(harness.NextConversationId);
        result.ConversationId.ShouldBe(harness.NextConversationId);
        result.ConversationId.ShouldNotBe(foreignConversationId);
    }

    [Fact]
    public async Task AskAsync_WithNullConversationId_CreatesNewConversation()
    {
        var harness = CreateHarness(
            CreateGroundedHousingChunks(),
            new float[] { 1f, 0f },
            Language.English,
            "Can my landlord evict me without a court order?",
            "No. A landlord cannot evict you without a court order.");
        harness.Service.UseSession(42);
        harness.Service.UseBaseQuestionPersistence = true;

        var result = await harness.Service.AskAsync(new AskQuestionRequest
        {
            QuestionText = "What if my landlord threatens me?"
        });

        harness.Service.LastConversationId.ShouldBeNull();
        harness.Conversations.Count.ShouldBe(1);
        harness.Questions.Count.ShouldBe(1);
        harness.Questions[0].ConversationId.ShouldBe(harness.NextConversationId);
        result.ConversationId.ShouldBe(harness.NextConversationId);
    }

    [Fact]
    public async Task GetConversationsAsync_ReturnsOnlyCurrentUserConversations_OrderedByStartedAtDescending()
    {
        var harness = CreateHarness(
            CreateGroundedHousingChunks(),
            new float[] { 1f, 0f },
            Language.English,
            "Can my landlord evict me without a court order?",
            "unused");
        harness.Service.UseSession(42);
        harness.SeedConversation(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            42,
            DateTime.UtcNow.AddHours(-2),
            "Older question");
        harness.SeedConversation(
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            7,
            DateTime.UtcNow.AddHours(-1),
            "Other user's question");
        harness.SeedConversation(
            Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            42,
            DateTime.UtcNow,
            "Newest question");

        var result = await harness.Service.GetConversationsAsync();

        result.TotalCount.ShouldBe(2);
        result.Items.Select(item => item.ConversationId).ShouldBe(new[]
        {
            Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
        });
    }

    [Fact]
    public async Task GetConversationsAsync_IncludesFirstQuestionAndCount()
    {
        var harness = CreateHarness(
            CreateGroundedHousingChunks(),
            new float[] { 1f, 0f },
            Language.English,
            "Can my landlord evict me without a court order?",
            "unused");
        harness.Service.UseSession(42);
        var conversation = harness.SeedConversation(
            Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            42,
            DateTime.UtcNow,
            "First question",
            "Second question");

        var result = await harness.Service.GetConversationsAsync();

        result.TotalCount.ShouldBe(1);
        result.Items[0].ConversationId.ShouldBe(conversation.Id);
        result.Items[0].FirstQuestion.ShouldBe("First question");
        result.Items[0].QuestionCount.ShouldBe(2);
        result.Items[0].Language.ShouldBe("english");
    }

    [Fact]
    public void ShouldPersistAnswer_OnlyGroundedModesReturnTrue()
    {
        RagAppService.ShouldPersistAnswer(RagAnswerMode.Direct).ShouldBeTrue();
        RagAppService.ShouldPersistAnswer(RagAnswerMode.Cautious).ShouldBeTrue();
        RagAppService.ShouldPersistAnswer(RagAnswerMode.Clarification).ShouldBeTrue();
        RagAppService.ShouldPersistAnswer(RagAnswerMode.Insufficient).ShouldBeTrue();
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
        result.PrimarySourceTitle.ShouldBeNull();
        result.PrimarySourceLocator.ShouldBeNull();
        result.PrimaryAuthorityType.ShouldBeNull();
        result.HasSupportingSources.ShouldBeFalse();
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
        result.PrimarySourceTitle.ShouldBeNull();
        result.PrimarySourceLocator.ShouldBeNull();
        result.PrimaryAuthorityType.ShouldBeNull();
        result.HasSupportingSources.ShouldBeFalse();
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

    [Fact]
    public async Task AskAsync_WithExistingConversation_UsesStoredTurnsAsPromptContext()
    {
        var harness = CreateHarness(
            CreateGroundedHousingChunks(),
            new float[] { 1f, 0f },
            Language.English,
            "What if they change the locks?",
            "A landlord still cannot evict you without a court order.");
        var suppliedConversationId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        harness.SeedConversationThread(
            suppliedConversationId,
            42,
            DateTime.UtcNow.AddMinutes(-10),
            ("Can my landlord evict me without a court order?", "No. A court order is required."));
        harness.Service.UseSession(42);
        harness.Service.UseBaseQuestionPersistence = true;

        await harness.Service.AskAsync(new AskQuestionRequest
        {
            QuestionText = "What if they change the locks?",
            ConversationId = suppliedConversationId
        });

        harness.LastChatRequestBody.ShouldContain("Conversation history for continuity only");
        harness.LastChatRequestBody.ShouldContain("User: Can my landlord evict me without a court order?");
        harness.LastChatRequestBody.ShouldContain("Assistant: No. A court order is required.");
        harness.EmbeddingRequests.ShouldContain(request =>
            request.Contains("Current question: What if they change the locks?", StringComparison.Ordinal));
        harness.EmbeddingRequests.ShouldContain(request =>
            request.Contains("User: Can my landlord evict me without a court order?", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GetConversationAsync_ReturnsOrderedUserAndAssistantMessages()
    {
        var harness = CreateHarness(
            CreateGroundedHousingChunks(),
            new float[] { 1f, 0f },
            Language.English,
            "unused",
            "unused");
        var conversationId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
        harness.SeedConversationThread(
            conversationId,
            42,
            DateTime.UtcNow.AddMinutes(-10),
            ("Can my landlord evict me without a court order?", "No. A court order is required."),
            ("What if they change the locks?", "That still needs a court process."));
        harness.Service.UseSession(42);

        var result = await harness.Service.GetConversationAsync(conversationId);

        result.ConversationId.ShouldBe(conversationId);
        result.QuestionCount.ShouldBe(2);
        result.Messages.Select(message => message.Type).ShouldBe(new[] { "user", "bot", "user", "bot" });
        result.Messages[0].Text.ShouldBe("Can my landlord evict me without a court order?");
        result.Messages[1].Text.ShouldBe("No. A court order is required.");
        result.Messages[2].Text.ShouldBe("What if they change the locks?");
    }

    private static RagAppServiceHarness CreateHarness(
        IReadOnlyList<IndexedChunk> loadedChunks,
        float[] questionVector,
        Language detectedLanguage,
        string translatedText,
        string chatResponse)
    {
        var store = new RagIndexStore();
        store.Replace(loadedChunks, new RagDocumentProfileBuilder().Build(loadedChunks).ToList());

        var embeddingService = Substitute.For<IEmbeddingAppService>();
        var embeddingRequests = new List<string>();
        embeddingService.GenerateEmbeddingAsync(Arg.Any<string>()).Returns(call =>
        {
            var text = call.Arg<string>();
            embeddingRequests.Add(text);
            return new EmbeddingResult
            {
                Vector = questionVector,
                Model = "text-embedding-3-small",
                InputCharacterCount = text.Length
            };
        });

        var languageService = Substitute.For<ILanguageAppService>();
        languageService.DetectLanguageAsync(Arg.Any<string>()).Returns(detectedLanguage);
        languageService.TranslateToEnglishAsync(Arg.Any<string>(), detectedLanguage).Returns(translatedText);

        var chatHandler = new StubChatHandler(chatResponse);
        var httpFactory = Substitute.For<IHttpClientFactory>();
        httpFactory.CreateClient("OpenAI").Returns(_ => new HttpClient(chatHandler)
        {
            BaseAddress = new Uri("https://api.openai.com/")
        });

        return new RagAppServiceHarness(
            embeddingService,
            languageService,
            store,
            httpFactory,
            BuildConfig(),
            chatHandler,
            embeddingRequests);
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
        public string LastRequestBody { get; private set; } = string.Empty;

        public StubChatHandler(string content)
        {
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequestBody = request.Content is null
                ? string.Empty
                : request.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();

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

    private sealed class RagAppServiceHarness
    {
        public Guid NextConversationId { get; } = Guid.Parse("33333333-3333-3333-3333-333333333333");
        public Guid NextQuestionId { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public Guid NextAnswerId { get; } = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public List<Conversation> Conversations { get; } = new();
        public List<Question> Questions { get; } = new();
        public List<Answer> Answers { get; } = new();
        public List<AnswerCitation> Citations { get; } = new();
        public TestableRagAppService Service { get; }
        public string LastChatRequestBody => _chatHandler.LastRequestBody;
        public IReadOnlyList<string> EmbeddingRequests => _embeddingRequests;
        private readonly StubChatHandler _chatHandler;
        private readonly List<string> _embeddingRequests;

        public RagAppServiceHarness(
            IEmbeddingAppService embeddingService,
            ILanguageAppService languageService,
            RagIndexStore ragIndexStore,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            StubChatHandler chatHandler,
            List<string> embeddingRequests)
        {
            _chatHandler = chatHandler;
            _embeddingRequests = embeddingRequests;
            var conversationRepository = Substitute.For<IRepository<Conversation, Guid>>();
            var questionRepository = Substitute.For<IRepository<Question, Guid>>();
            var answerRepository = Substitute.For<IRepository<Answer, Guid>>();
            var citationRepository = Substitute.For<IRepository<AnswerCitation, Guid>>();

            conversationRepository.GetAll().Returns(_ => Conversations.AsQueryable());
            conversationRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Conversation, bool>>>())
                .Returns(call =>
                {
                    var predicate = call.Arg<Expression<Func<Conversation, bool>>>().Compile();
                    return Task.FromResult(Conversations.FirstOrDefault(predicate) ?? null!);
                });
            conversationRepository.InsertAndGetIdAsync(Arg.Any<Conversation>())
                .Returns(call =>
                {
                    var entity = call.Arg<Conversation>();
                    entity.Id = NextConversationId;
                    entity.Questions ??= new List<Question>();
                    Conversations.Add(entity);
                    return Task.FromResult(entity.Id);
                });

            questionRepository.InsertAndGetIdAsync(Arg.Any<Question>())
                .Returns(call =>
                {
                    var entity = call.Arg<Question>();
                    entity.Id = NextQuestionId;
                    Questions.Add(entity);

                    var conversation = Conversations.FirstOrDefault(item => item.Id == entity.ConversationId);
                    if (conversation != null)
                    {
                        conversation.Questions ??= new List<Question>();
                        conversation.Questions.Add(entity);
                    }

                    return Task.FromResult(entity.Id);
                });

            answerRepository.InsertAndGetIdAsync(Arg.Any<Answer>())
                .Returns(call =>
                {
                    var entity = call.Arg<Answer>();
                    entity.Id = NextAnswerId;
                    Answers.Add(entity);
                    return Task.FromResult(entity.Id);
                });

            citationRepository.InsertAsync(Arg.Any<AnswerCitation>())
                .Returns(call =>
                {
                    var entity = call.Arg<AnswerCitation>();
                    Citations.Add(entity);
                    return Task.FromResult(entity);
                });

            Service = new TestableRagAppService(
                embeddingService,
                languageService,
                Substitute.For<IRepository<DocumentChunk, Guid>>(),
                conversationRepository,
                questionRepository,
                answerRepository,
                citationRepository,
                ragIndexStore,
                httpClientFactory,
                configuration,
                Conversations);
        }

        public Conversation SeedConversation(
            Guid conversationId,
            long userId,
            DateTime startedAt,
            params string[] questionTexts)
        {
            var conversation = new Conversation
            {
                Id = conversationId,
                UserId = userId,
                Language = Language.English,
                InputMethod = InputMethod.Text,
                StartedAt = startedAt,
                Questions = questionTexts.Select((text, index) => new Question
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    OriginalText = text,
                    TranslatedText = text,
                    Language = Language.English,
                    InputMethod = InputMethod.Text,
                    CreationTime = startedAt.AddMinutes(index)
                }).ToList()
            };

            Conversations.Add(conversation);
            return conversation;
        }

        public Conversation SeedConversationThread(
            Guid conversationId,
            long userId,
            DateTime startedAt,
            params (string Question, string Answer)[] turns)
        {
            var conversation = new Conversation
            {
                Id = conversationId,
                UserId = userId,
                Language = Language.English,
                InputMethod = InputMethod.Text,
                StartedAt = startedAt,
                Questions = turns.Select((turn, index) =>
                {
                    var questionTime = startedAt.AddMinutes(index * 2);
                    var questionId = Guid.NewGuid();
                    return new Question
                    {
                        Id = questionId,
                        ConversationId = conversationId,
                        OriginalText = turn.Question,
                        TranslatedText = turn.Question,
                        Language = Language.English,
                        InputMethod = InputMethod.Text,
                        CreationTime = questionTime,
                        Answers = new List<Answer>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                QuestionId = questionId,
                                Text = turn.Answer,
                                Language = Language.English,
                                CreationTime = questionTime.AddSeconds(30),
                                Citations = new List<AnswerCitation>()
                            }
                        }
                    };
                }).ToList()
            };

            Conversations.Add(conversation);
            return conversation;
        }
    }

    private sealed class TestableRagAppService : RagAppService
    {
        private readonly List<Conversation> _conversations;

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
            IConfiguration configuration,
            List<Conversation> conversations)
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
            _conversations = conversations;
        }

        public Guid NextQuestionId { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public Guid NextAnswerId { get; } = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public bool UseBaseQuestionPersistence { get; set; }
        public int PersistedQuestionCount { get; private set; }
        public int PersistedAnswerCount { get; private set; }
        public Guid? LastConversationId { get; private set; }
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

        protected override async Task<PersistedQuestionResult> PersistQuestionAsync(
            long userId,
            Guid? conversationId,
            string originalText,
            string translatedText,
            Language language)
        {
            PersistedQuestionCount++;
            LastConversationId = conversationId;
            LastOriginalText = originalText;
            LastTranslatedText = translatedText;
            LastQuestionLanguage = language;

            if (UseBaseQuestionPersistence)
            {
                return await base.PersistQuestionAsync(
                    userId,
                    conversationId,
                    originalText,
                    translatedText,
                    language);
            }

            return new PersistedQuestionResult(NextQuestionId, conversationId ?? Guid.Parse("33333333-3333-3333-3333-333333333333"));
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

        protected override Task<List<Conversation>> ListConversationsAsync(IOrderedQueryable<Conversation> query)
        {
            return Task.FromResult(query.ToList());
        }

        protected override Task<Conversation> LoadConversationThreadAsync(long userId, Guid conversationId)
        {
            return Task.FromResult(
                _conversations.FirstOrDefault(conversation =>
                    conversation.Id == conversationId &&
                    conversation.UserId == userId))!;
        }
    }
}
