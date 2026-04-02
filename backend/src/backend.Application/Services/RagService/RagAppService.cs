using Abp.Application.Services;
using Abp.Domain.Repositories;
using Ardalis.GuardClauses;
using backend.Domains.LegalDocuments;
using backend.Domains.QA;
using backend.Services.EmbeddingService;
using backend.Services.LanguageService;
using backend.Services.RagService.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Services.RagService;

/// <summary>
/// Core RAG orchestration service for South African legal Q&amp;A.
/// Loads chunk embeddings and source metadata into memory, plans document-aware retrieval,
/// calls the chat model only when grounding is adequate, persists conversation history for
/// authenticated users, and can rehydrate stored threads back into the client.
/// </summary>
public class RagAppService : ApplicationService, IRagAppService
{
    private const int MaxConversationHistoryMessages = 6;
    private readonly IEmbeddingAppService _embeddingService;
    private readonly ILanguageAppService _languageService;
    private readonly IRepository<DocumentChunk, Guid> _chunkRepository;
    private readonly IRepository<Conversation, Guid> _conversationRepository;
    private readonly IRepository<Question, Guid> _questionRepository;
    private readonly IRepository<Answer, Guid> _answerRepository;
    private readonly IRepository<AnswerCitation, Guid> _citationRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RagIndexStore _ragIndexStore;
    private readonly RagSourceHintExtractor _sourceHintExtractor = new();
    private readonly RagDocumentProfileBuilder _documentProfileBuilder = new();
    private readonly RagRetrievalPlanner _retrievalPlanner = new();
    private readonly RagConfidenceEvaluator _confidenceEvaluator = new();
    private readonly string _apiKey;
    private readonly string _chatModel;

    public RagAppService(
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
    {
        Guard.Against.Null(configuration, nameof(configuration));
        Guard.Against.Null(ragIndexStore, nameof(ragIndexStore));

        _embeddingService = embeddingService;
        _languageService = languageService;
        _chunkRepository = chunkRepository;
        _conversationRepository = conversationRepository;
        _questionRepository = questionRepository;
        _answerRepository = answerRepository;
        _citationRepository = citationRepository;
        _ragIndexStore = ragIndexStore;
        _httpClientFactory = httpClientFactory;

        _apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException(
                "OpenAI:ApiKey must be set in appsettings.json. " +
                "Store your real key in appsettings.Development.json (gitignored).");
        }

        _chatModel = configuration["OpenAI:ChatModel"];
        if (string.IsNullOrWhiteSpace(_chatModel))
        {
            throw new InvalidOperationException(
                "OpenAI:ChatModel must be set in appsettings.json (e.g., \"gpt-4o\").");
        }

        var baseUrl = configuration["OpenAI:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("OpenAI:BaseUrl must be set in appsettings.json.");
        }
    }

    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        using var uow = UnitOfWorkManager.Begin();

        var chunks = await _chunkRepository
            .GetAll()
            .Include(c => c.Embedding)
            .Include(c => c.Document)
            .ThenInclude(d => d.Category)
            .Where(c => c.Embedding != null)
            .ToListAsync(cancellationToken);

        var loadedChunks = chunks
            .Select(chunk => new IndexedChunk(
                ChunkId: chunk.Id,
                DocumentId: chunk.DocumentId,
                ActName: chunk.Document?.Title ?? "Unknown Act",
                ActShortName: chunk.Document?.ShortName ?? string.Empty,
                ActNumber: chunk.Document?.ActNumber ?? string.Empty,
                Year: chunk.Document?.Year ?? 0,
                CategoryName: chunk.Document?.Category?.Name ?? string.Empty,
                SectionNumber: chunk.SectionNumber ?? string.Empty,
                SectionTitle: chunk.SectionTitle ?? string.Empty,
                Excerpt: chunk.Content ?? string.Empty,
                Keywords: ParseKeywords(chunk.Keywords),
                TopicClassification: chunk.TopicClassification ?? string.Empty,
                TokenCount: chunk.TokenCount,
                Vector: chunk.Embedding.Vector))
            .ToList();

        var documentProfiles = _documentProfileBuilder
            .Build(loadedChunks)
            .ToList();

        _ragIndexStore.Replace(loadedChunks, documentProfiles);

        await uow.CompleteAsync();
    }

    public async Task<ConversationsListDto> GetConversationsAsync()
    {
        var userId = AbpSession.UserId
            ?? throw new Abp.Authorization.AbpAuthorizationException("You must be signed in to view conversation history.");

        var conversations = await ListConversationsAsync(
            _conversationRepository
                .GetAll()
                .Include(c => c.Questions)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.StartedAt));

        var items = conversations.Select(conversation => new ConversationSummaryDto
        {
            ConversationId = conversation.Id,
            FirstQuestion = conversation.Questions
                .OrderBy(question => question.CreationTime)
                .Select(question => question.OriginalText)
                .FirstOrDefault() ?? string.Empty,
            QuestionCount = conversation.Questions.Count,
            StartedAt = conversation.StartedAt,
            Language = conversation.Language.ToString().ToLowerInvariant()
        }).ToList();

        return new ConversationsListDto { Items = items, TotalCount = items.Count };
    }

    public async Task<ConversationDetailDto> GetConversationAsync(Guid conversationId)
    {
        var userId = AbpSession.UserId
            ?? throw new Abp.Authorization.AbpAuthorizationException("You must be signed in to view conversation history.");

        var conversation = await LoadConversationThreadAsync(userId, conversationId);
        if (conversation is null)
        {
            throw new Abp.Authorization.AbpAuthorizationException("Conversation not found.");
        }

        return MapConversationDetail(conversation);
    }

    public async Task<RagAnswerResult> AskAsync(AskQuestionRequest request)
    {
        Guard.Against.Null(request, nameof(request));
        Guard.Against.NullOrWhiteSpace(request.QuestionText, nameof(request.QuestionText));

        await EnsureIndexLoadedAsync();

        var userId = AbpSession.UserId;
        var existingConversation = userId.HasValue && request.ConversationId.HasValue
            ? await LoadConversationThreadAsync(userId.Value, request.ConversationId.Value)
            : null;
        var detectedLanguage = await _languageService.DetectLanguageAsync(request.QuestionText);
        var translatedText = await _languageService.TranslateToEnglishAsync(request.QuestionText, detectedLanguage);
        var detectedLanguageCode = RagPromptBuilder.ToIsoCode(detectedLanguage);
        var conversationHistoryBlock = BuildConversationHistoryBlock(existingConversation);
        var retrievalQuestionText = BuildRetrievalQuestionText(translatedText, conversationHistoryBlock);
        var embeddingResult = await _embeddingService.GenerateEmbeddingAsync(retrievalQuestionText);
        var focusQueryText = RagQueryFocusBuilder.Build(retrievalQuestionText);
        float[] focusVector = null;
        if (!string.IsNullOrWhiteSpace(focusQueryText) &&
            !string.Equals(focusQueryText, retrievalQuestionText, StringComparison.OrdinalIgnoreCase))
        {
            focusVector = (await _embeddingService.GenerateEmbeddingAsync(focusQueryText)).Vector;
        }

        var loadedChunks = _ragIndexStore.LoadedChunks;
        var documentProfiles = _ragIndexStore.DocumentProfiles;

        var semanticMatches = _retrievalPlanner.BuildSemanticMatches(
            embeddingResult.Vector,
            loadedChunks,
            focusVector);
        var sourceHints = _sourceHintExtractor.Extract(retrievalQuestionText, loadedChunks);
        var retrievalPlan = _retrievalPlanner.BuildPlan(
            retrievalQuestionText,
            embeddingResult.Vector,
            semanticMatches,
            sourceHints,
            documentProfiles);
        var retrievalDecision = _confidenceEvaluator.Evaluate(retrievalQuestionText, retrievalPlan);
        Guid? answerId = null;
        Guid? conversationId = null;

        if (retrievalDecision.AnswerMode == RagAnswerMode.Insufficient)
        {
            var result = BuildNonGroundedResult(
                detectedLanguage,
                detectedLanguageCode,
                RagAnswerMode.Insufficient,
                retrievalDecision.ConfidenceBand,
                null,
                retrievalDecision.RequiresUrgentAttention);

            if (userId.HasValue)
            {
                var persistedQuestion = await PersistQuestionAsync(
                    userId.Value,
                    existingConversation?.Id,
                    request.QuestionText,
                    translatedText,
                    detectedLanguage);
                conversationId = persistedQuestion.ConversationId;
                answerId = await PersistAnswerAsync(
                    persistedQuestion.QuestionId,
                    result.AnswerText,
                    detectedLanguage,
                    Array.Empty<RetrievedChunk>());
                result.ConversationId = conversationId;
                result.AnswerId = answerId;
            }

            return result;
        }

        if (retrievalDecision.AnswerMode == RagAnswerMode.Clarification)
        {
            var clarificationQuestion = await BuildClarificationQuestionAsync(
                request.QuestionText,
                detectedLanguage,
                retrievalDecision,
                conversationHistoryBlock);

            var result = BuildNonGroundedResult(
                detectedLanguage,
                detectedLanguageCode,
                RagAnswerMode.Clarification,
                retrievalDecision.ConfidenceBand,
                clarificationQuestion,
                retrievalDecision.RequiresUrgentAttention);

            if (userId.HasValue)
            {
                var persistedQuestion = await PersistQuestionAsync(
                    userId.Value,
                    existingConversation?.Id,
                    request.QuestionText,
                    translatedText,
                    detectedLanguage);
                conversationId = persistedQuestion.ConversationId;
                answerId = await PersistAnswerAsync(
                    persistedQuestion.QuestionId,
                    BuildStoredClarificationAnswer(result.AnswerText, clarificationQuestion),
                    detectedLanguage,
                    Array.Empty<RetrievedChunk>());
                result.ConversationId = conversationId;
                result.AnswerId = answerId;
            }

            return result;
        }

        var answerText = await BuildGroundedAnswerAsync(
            request.QuestionText,
            detectedLanguage,
            retrievalDecision,
            conversationHistoryBlock);
        if (userId.HasValue)
        {
            var persistedQuestion = await PersistQuestionAsync(
                userId.Value,
                existingConversation?.Id,
                request.QuestionText,
                translatedText,
                detectedLanguage);
            conversationId = persistedQuestion.ConversationId;

            if (ShouldPersistAnswer(retrievalDecision.AnswerMode))
            {
                answerId = await PersistAnswerAsync(
                    persistedQuestion.QuestionId,
                    answerText,
                    detectedLanguage,
                    retrievalDecision.SelectedChunks);
            }
        }

        return new RagAnswerResult
        {
            AnswerText = answerText,
            IsInsufficientInformation = false,
            Citations = CreateCitations(retrievalDecision.SelectedChunks),
            ChunkIds = retrievalDecision.SelectedChunks.Select(chunk => chunk.ChunkId).ToList(),
            AnswerId = answerId,
            ConversationId = conversationId,
            DetectedLanguageCode = detectedLanguageCode,
            AnswerMode = retrievalDecision.AnswerMode,
            ConfidenceBand = retrievalDecision.ConfidenceBand,
            PrimarySourceTitle = GetPrimarySourceTitle(retrievalDecision.SelectedChunks),
            PrimarySourceLocator = GetPrimarySourceLocator(retrievalDecision.SelectedChunks),
            PrimaryAuthorityType = GetPrimaryAuthorityType(retrievalDecision.SelectedChunks),
            HasSupportingSources = retrievalDecision.SelectedChunks.Any(chunk =>
                string.Equals(chunk.SourceRole, RagSourceMetadata.Supporting, StringComparison.Ordinal)),
            RequiresUrgentAttention = retrievalDecision.RequiresUrgentAttention
        };
    }

    public static bool ShouldPersistAnswer(RagAnswerMode answerMode) =>
        answerMode == RagAnswerMode.Direct ||
        answerMode == RagAnswerMode.Cautious ||
        answerMode == RagAnswerMode.Clarification ||
        answerMode == RagAnswerMode.Insufficient;

    private async Task EnsureIndexLoadedAsync()
    {
        if (_ragIndexStore.IsReady)
        {
            return;
        }

        await InitialiseAsync();
    }

    public static RagAnswerResult BuildNonGroundedResult(
        Language language,
        string detectedLanguageCode,
        RagAnswerMode answerMode,
        RagConfidenceBand confidenceBand,
        string clarificationQuestion,
        bool requiresUrgentAttention = false)
    {
        var answerText = answerMode == RagAnswerMode.Clarification
            ? RagPromptBuilder.BuildClarificationLead(language, requiresUrgentAttention)
            : RagPromptBuilder.BuildInsufficientResponse(language, requiresUrgentAttention);

        return new RagAnswerResult
        {
            AnswerText = answerText,
            IsInsufficientInformation = true,
            Citations = new List<RagCitationDto>(),
            ChunkIds = new List<Guid>(),
            AnswerId = null,
            DetectedLanguageCode = detectedLanguageCode,
            AnswerMode = answerMode,
            ConfidenceBand = confidenceBand,
            PrimarySourceTitle = null,
            PrimarySourceLocator = null,
            PrimaryAuthorityType = null,
            HasSupportingSources = false,
            ClarificationQuestion = answerMode == RagAnswerMode.Clarification ? clarificationQuestion : null,
            RequiresUrgentAttention = requiresUrgentAttention
        };
    }

    private async Task<string> BuildGroundedAnswerAsync(
        string originalQuestionText,
        Language detectedLanguage,
        RetrievalDecision retrievalDecision,
        string conversationHistoryBlock)
    {
        var systemPrompt = RagPromptBuilder.BuildSystemPrompt(
            retrievalDecision.AnswerMode,
            detectedLanguage,
            retrievalDecision.RequiresUrgentAttention);
        var contextBlock = RagPromptBuilder.BuildContextBlock(retrievalDecision.SelectedChunks);
        var userPrompt = RagPromptBuilder.BuildUserPrompt(
            originalQuestionText,
            contextBlock,
            retrievalDecision.AnswerMode,
            conversationHistoryBlock: conversationHistoryBlock,
            requiresUrgentAttention: retrievalDecision.RequiresUrgentAttention);

        return await CallChatCompletionsAsync(
            systemPrompt,
            userPrompt,
            RagPromptBuilder.GetChatTemperature(retrievalDecision.AnswerMode));
    }

    private async Task<string> BuildClarificationQuestionAsync(
        string originalQuestionText,
        Language detectedLanguage,
        RetrievalDecision retrievalDecision,
        string conversationHistoryBlock)
    {
        if (retrievalDecision.SelectedChunks.Count == 0)
        {
            return retrievalDecision.ClarificationQuestion;
        }

        try
        {
            var systemPrompt = RagPromptBuilder.BuildSystemPrompt(
                RagAnswerMode.Clarification,
                detectedLanguage,
                retrievalDecision.RequiresUrgentAttention);
            var contextBlock = RagPromptBuilder.BuildContextBlock(retrievalDecision.SelectedChunks);
            var userPrompt = RagPromptBuilder.BuildUserPrompt(
                originalQuestionText,
                contextBlock,
                RagAnswerMode.Clarification,
                retrievalDecision.ClarificationQuestion,
                conversationHistoryBlock,
                retrievalDecision.RequiresUrgentAttention);

            var response = await CallChatCompletionsAsync(
                systemPrompt,
                userPrompt,
                RagPromptBuilder.GetChatTemperature(RagAnswerMode.Clarification));

            return SanitizeClarificationQuestion(response, retrievalDecision.ClarificationQuestion);
        }
        catch
        {
            return retrievalDecision.ClarificationQuestion;
        }
    }

    private async Task<string> CallChatCompletionsAsync(
        string systemPrompt,
        string userPrompt,
        double temperature)
    {
        using var client = _httpClientFactory.CreateClient("OpenAI");
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _apiKey) },
            Content = JsonContent.Create(new OpenAiChatRequest(
                Model: _chatModel,
                Temperature: temperature,
                Messages: new[]
                {
                    new OpenAiChatMessage(Role: "system", Content: systemPrompt),
                    new OpenAiChatMessage(Role: "user", Content: userPrompt)
                }))
        };

        using var response = await client.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<OpenAiChatResponse>();
        return body?.Choices?[0]?.Message?.Content
               ?? throw new InvalidOperationException("OpenAI chat response contained no content.");
    }

    protected virtual async Task<PersistedQuestionResult> PersistQuestionAsync(
        long userId,
        Guid? conversationId,
        string originalText,
        string translatedText,
        Language language)
    {
        var resolvedConversationId = conversationId.HasValue
            ? (await _conversationRepository.FirstOrDefaultAsync(
                conversation => conversation.Id == conversationId.Value && conversation.UserId == userId))?.Id
            : null;

        if (!resolvedConversationId.HasValue)
        {
            resolvedConversationId = await CreateConversationAsync(userId, language);
        }

        var question = new Question
        {
            ConversationId = resolvedConversationId.Value,
            OriginalText = originalText,
            TranslatedText = translatedText,
            Language = language,
            InputMethod = InputMethod.Text,
        };
        var questionId = await _questionRepository.InsertAndGetIdAsync(question);
        return new PersistedQuestionResult(questionId, resolvedConversationId.Value);
    }

    protected virtual Task<List<Conversation>> ListConversationsAsync(IOrderedQueryable<Conversation> query)
    {
        return query.ToListAsync();
    }

    protected virtual async Task<Guid> PersistAnswerAsync(
        Guid questionId,
        string answerText,
        Language language,
        IEnumerable<RetrievedChunk> usedChunks)
    {
        var answer = new Answer
        {
            QuestionId = questionId,
            Text = answerText,
            Language = language
        };
        var answerId = await _answerRepository.InsertAndGetIdAsync(answer);

        foreach (var chunk in usedChunks)
        {
            await _citationRepository.InsertAsync(new AnswerCitation
            {
                AnswerId = answerId,
                ChunkId = chunk.ChunkId,
                SectionNumber = chunk.SectionNumber,
                Excerpt = chunk.Excerpt.Length > 500 ? chunk.Excerpt[..500] : chunk.Excerpt,
                RelevanceScore = (decimal)chunk.RelevanceScore
            });
        }

        return answerId;
    }

    private async Task<Guid> CreateConversationAsync(long userId, Language language)
    {
        var conversation = new Conversation
        {
            UserId = userId,
            Language = language,
            InputMethod = InputMethod.Text,
            StartedAt = DateTime.UtcNow,
            IsPublicFaq = false
        };

        return await _conversationRepository.InsertAndGetIdAsync(conversation);
    }

    private static string SanitizeClarificationQuestion(string generatedQuestion, string fallbackQuestion)
    {
        var candidate = string.IsNullOrWhiteSpace(generatedQuestion)
            ? fallbackQuestion
            : generatedQuestion.Trim();

        var firstLine = candidate
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()?
            .Trim();

        if (string.IsNullOrWhiteSpace(firstLine))
        {
            return fallbackQuestion;
        }

        return firstLine.EndsWith("?", StringComparison.Ordinal) ? firstLine : $"{firstLine}?";
    }

    public static List<RagCitationDto> CreateCitations(IEnumerable<RetrievedChunk> usedChunks) =>
        usedChunks
            .Select(chunk => new RagCitationDto
            {
                ChunkId = chunk.ChunkId,
                ActName = chunk.ActName,
                SectionNumber = chunk.SectionNumber,
                SourceTitle = chunk.SourceTitle,
                SourceLocator = chunk.SourceLocator,
                AuthorityType = chunk.AuthorityType,
                SourceRole = chunk.SourceRole,
                Excerpt = chunk.Excerpt.Length > 500 ? chunk.Excerpt[..500] : chunk.Excerpt,
                RelevanceScore = chunk.RelevanceScore
            })
            .ToList();

    private static RetrievedChunk GetPrimarySourceChunk(IEnumerable<RetrievedChunk> usedChunks) =>
        usedChunks?
            .FirstOrDefault(chunk =>
                string.Equals(chunk.SourceRole, RagSourceMetadata.Primary, StringComparison.Ordinal)) ??
        usedChunks?
            .FirstOrDefault();

    private static string BuildStoredClarificationAnswer(string answerLead, string clarificationQuestion)
    {
        if (string.IsNullOrWhiteSpace(clarificationQuestion))
        {
            return answerLead;
        }

        return $"{answerLead}\n\n{clarificationQuestion.Trim()}";
    }

    private static string BuildRetrievalQuestionText(string translatedQuestionText, string conversationHistoryBlock)
    {
        if (string.IsNullOrWhiteSpace(conversationHistoryBlock))
        {
            return translatedQuestionText;
        }

        return
            $"Current question: {translatedQuestionText.Trim()}\n\n" +
            $"Recent conversation context:\n{conversationHistoryBlock}";
    }

    private static string BuildConversationHistoryBlock(Conversation conversation)
    {
        var messageEntries = conversation?.Questions?
            .OrderBy(question => question.CreationTime)
            .SelectMany(question =>
            {
                var entries = new List<(DateTime CreatedAt, string Role, string Text)>
                {
                    (question.CreationTime, "User", question.OriginalText ?? string.Empty)
                };

                if (question.Answers != null)
                {
                    entries.AddRange(question.Answers
                        .OrderBy(answer => answer.CreationTime)
                        .Select(answer => (answer.CreationTime, "Assistant", answer.Text ?? string.Empty)));
                }

                return entries;
            })
            .OrderBy(entry => entry.CreatedAt)
            .TakeLast(MaxConversationHistoryMessages)
            .ToList();

        if (messageEntries is null || messageEntries.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (var entry in messageEntries)
        {
            var text = entry.Text.Trim();
            if (text.Length > 500)
            {
                text = $"{text[..500]}...";
            }

            sb.AppendLine($"{entry.Role}: {text}");
        }

        return sb.ToString().TrimEnd();
    }

    private static string GetPrimarySourceTitle(IEnumerable<RetrievedChunk> usedChunks) =>
        GetPrimarySourceChunk(usedChunks)?.SourceTitle;

    private static string GetPrimarySourceLocator(IEnumerable<RetrievedChunk> usedChunks) =>
        GetPrimarySourceChunk(usedChunks)?.SourceLocator;

    private static string GetPrimaryAuthorityType(IEnumerable<RetrievedChunk> usedChunks) =>
        GetPrimarySourceChunk(usedChunks)?.AuthorityType;

    protected virtual Task<Conversation> LoadConversationThreadAsync(long userId, Guid conversationId)
    {
        return _conversationRepository
            .GetAll()
            .Include(conversation => conversation.Questions)
                .ThenInclude(question => question.Answers)
                    .ThenInclude(answer => answer.Citations)
                        .ThenInclude(citation => citation.Chunk)
                            .ThenInclude(chunk => chunk.Document)
            .FirstOrDefaultAsync(conversation =>
                conversation.Id == conversationId &&
                conversation.UserId == userId);
    }

    private static ConversationDetailDto MapConversationDetail(Conversation conversation)
    {
        var messages = new List<ConversationHistoryMessageDto>();

        foreach (var question in conversation.Questions.OrderBy(item => item.CreationTime))
        {
            messages.Add(new ConversationHistoryMessageDto
            {
                MessageId = question.Id,
                Type = "user",
                Text = question.OriginalText ?? string.Empty,
                CreatedAt = question.CreationTime,
                DetectedLanguageCode = RagPromptBuilder.ToIsoCode(question.Language),
                Citations = new List<RagCitationDto>()
            });

            foreach (var answer in (question.Answers ?? Array.Empty<Answer>()).OrderBy(item => item.CreationTime))
            {
                messages.Add(new ConversationHistoryMessageDto
                {
                    MessageId = answer.Id,
                    Type = "bot",
                    Text = answer.Text ?? string.Empty,
                    CreatedAt = answer.CreationTime,
                    DetectedLanguageCode = RagPromptBuilder.ToIsoCode(answer.Language),
                    Citations = (answer.Citations ?? Array.Empty<AnswerCitation>())
                        .OrderByDescending(citation => citation.RelevanceScore)
                        .Select(citation => new RagCitationDto
                        {
                            ChunkId = citation.ChunkId,
                            ActName = citation.Chunk?.Document?.Title ?? "Unknown Act",
                            SectionNumber = citation.SectionNumber,
                            SourceTitle = citation.Chunk?.Document?.Title ?? "Unknown Act",
                            SourceLocator = citation.SectionNumber,
                            AuthorityType = citation.Chunk?.Document is null
                                ? null
                                : RagSourceMetadata.DeriveAuthorityType(
                                    citation.Chunk.Document.Title,
                                    citation.Chunk.Document.ShortName,
                                    citation.Chunk.Document.ActNumber),
                            SourceRole = null,
                            Excerpt = citation.Excerpt,
                            RelevanceScore = (float)citation.RelevanceScore
                        })
                        .ToList()
                });
            }
        }

        return new ConversationDetailDto
        {
            ConversationId = conversation.Id,
            StartedAt = conversation.StartedAt,
            Language = conversation.Language.ToString().ToLowerInvariant(),
            QuestionCount = conversation.Questions?.Count ?? 0,
            Messages = messages
        };
    }

    private static List<string> ParseKeywords(string rawKeywords)
    {
        if (string.IsNullOrWhiteSpace(rawKeywords))
        {
            return new List<string>();
        }

        return rawKeywords
            .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(RagSourceHintExtractor.Normalize)
            .Where(keyword => !string.IsNullOrWhiteSpace(keyword))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private sealed record OpenAiChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("messages")] OpenAiChatMessage[] Messages);

    private sealed record OpenAiChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record OpenAiChatResponse(
        [property: JsonPropertyName("choices")] OpenAiChatChoice[] Choices);

    private sealed record OpenAiChatChoice(
        [property: JsonPropertyName("message")] OpenAiChatMessage Message);

    protected sealed record PersistedQuestionResult(Guid QuestionId, Guid ConversationId);
}
