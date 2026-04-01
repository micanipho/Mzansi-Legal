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
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Services.RagService;

/// <summary>
/// Core RAG orchestration service for South African legal Q&amp;A.
/// Loads all chunk embeddings into memory at startup, scores them per question
/// via cosine similarity, calls GPT-4o with a grounded prompt, and persists
/// the full Conversation → Question → Answer → AnswerCitation chain.
/// </summary>
public class RagAppService : ApplicationService, IRagAppService
{
    private readonly IEmbeddingAppService _embeddingService;

    /// <summary>Detects question language and translates non-English questions for knowledge-base search.</summary>
    private readonly ILanguageAppService _languageService;

    private readonly IRepository<DocumentChunk, Guid> _chunkRepository;
    private readonly IRepository<Conversation, Guid> _conversationRepository;
    private readonly IRepository<Question, Guid> _questionRepository;
    private readonly IRepository<Answer, Guid> _answerRepository;
    private readonly IRepository<AnswerCitation, Guid> _citationRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;
    private readonly string _chatModel;
    private readonly string _baseUrl;

    // In-memory store populated by InitialiseAsync; read-only during AskAsync.
    private List<RagPromptBuilder.ScoredChunk> _loadedChunks = new();

    /// <summary>
    /// Initialises the service and validates required OpenAI configuration keys.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when any required OpenAI config key is missing.</exception>
    public RagAppService(
        IEmbeddingAppService embeddingService,
        ILanguageAppService languageService,
        IRepository<DocumentChunk, Guid> chunkRepository,
        IRepository<Conversation, Guid> conversationRepository,
        IRepository<Question, Guid> questionRepository,
        IRepository<Answer, Guid> answerRepository,
        IRepository<AnswerCitation, Guid> citationRepository,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        Guard.Against.Null(configuration, nameof(configuration));

        _embeddingService = embeddingService;
        _languageService = languageService;
        _chunkRepository = chunkRepository;
        _conversationRepository = conversationRepository;
        _questionRepository = questionRepository;
        _answerRepository = answerRepository;
        _citationRepository = citationRepository;
        _httpClientFactory = httpClientFactory;

        _apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException(
                "OpenAI:ApiKey must be set in appsettings.json. " +
                "Store your real key in appsettings.Development.json (gitignored).");

        _chatModel = configuration["OpenAI:ChatModel"];
        if (string.IsNullOrWhiteSpace(_chatModel))
            throw new InvalidOperationException(
                "OpenAI:ChatModel must be set in appsettings.json (e.g., \"gpt-4o\").");

        _baseUrl = configuration["OpenAI:BaseUrl"];
        if (string.IsNullOrWhiteSpace(_baseUrl))
            throw new InvalidOperationException(
                "OpenAI:BaseUrl must be set in appsettings.json.");
    }

    /// <summary>
    /// Loads all DocumentChunk embeddings from the database into the in-memory store.
    /// Only chunks with a populated embedding vector are loaded. Existing in-memory
    /// data is replaced atomically on each call.
    /// </summary>
    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        // Load all chunks that have an embedding, including the parent document for the Act name.
        var chunks = await _chunkRepository
            .GetAll()
            .Include(c => c.Embedding)
            .Include(c => c.Document)
            .Where(c => c.Embedding != null)
            .ToListAsync(cancellationToken);

        _loadedChunks = chunks
            .Select(c => new RagPromptBuilder.ScoredChunk(
                ChunkId: c.Id,
                ActName: c.Document?.Title ?? "Unknown Act",
                SectionNumber: c.SectionNumber ?? string.Empty,
                Excerpt: c.Content ?? string.Empty,
                Score: 0f,    // score is assigned per-query, not at load time
                Vector: c.Embedding.Vector))
            .ToList();
    }

    /// <summary>
    /// Returns conversations for the current user ordered newest-first,
    /// each decorated with the first question text and total question count.
    /// </summary>
    public async Task<ConversationsListDto> GetConversationsAsync()
    {
        var userId = AbpSession.UserId
            ?? throw new Abp.Authorization.AbpAuthorizationException("You must be signed in to view conversation history.");

        var conversations = await _conversationRepository
            .GetAll()
            .Include(c => c.Questions)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.StartedAt)
            .ToListAsync();

        var items = conversations.Select(c => new ConversationSummaryDto
        {
            ConversationId = c.Id,
            FirstQuestion = c.Questions
                .OrderBy(q => q.CreationTime)
                .Select(q => q.OriginalText)
                .FirstOrDefault() ?? string.Empty,
            QuestionCount = c.Questions.Count,
            StartedAt = c.StartedAt,
            Language = c.Language.ToString().ToLowerInvariant()
        }).ToList();

        return new ConversationsListDto { Items = items, TotalCount = items.Count };
    }

    /// <summary>
    /// Embeds the user's question, scores all loaded chunks via cosine similarity,
    /// and — when relevant chunks exist — calls GPT-4o and persists the Q&amp;A chain.
    /// Returns <see cref="RagAnswerResult.IsInsufficientInformation"/> = <c>true</c>
    /// when no chunk scores ≥ <see cref="RagPromptBuilder.SimilarityThreshold"/>.
    /// </summary>
    public async Task<RagAnswerResult> AskAsync(AskQuestionRequest request)
    {
        Guard.Against.Null(request, nameof(request));
        Guard.Against.NullOrWhiteSpace(request.QuestionText, nameof(request.QuestionText));

        // Multilingual: detect input language and translate to English for knowledge-base search.
        var detectedLanguage = await _languageService.DetectLanguageAsync(request.QuestionText);
        var translatedText = await _languageService.TranslateToEnglishAsync(request.QuestionText, detectedLanguage);
        var detectedLanguageCode = RagPromptBuilder.ToIsoCode(detectedLanguage);
        // Embed the translated question for semantic search against the legislation corpus.
        var embeddingResult = await _embeddingService.GenerateEmbeddingAsync(translatedText);
        var questionVector = embeddingResult.Vector;

        // Score all loaded chunks and filter to those above the similarity threshold.
        var topChunks = _loadedChunks
            .Select(c => c with { Score = EmbeddingHelper.CosineSimilarity(questionVector, c.Vector) })
            .Where(c => c.Score >= RagPromptBuilder.SimilarityThreshold)
            .OrderByDescending(c => c.Score)
            .Take(RagPromptBuilder.MaxContextChunks)
            .ToList();

        const string disclaimer =
            "⚠️ *No matching legislation was found in the database for this question. " +
            "The following answer is based on general AI knowledge and is not legally authoritative. " +
            "Please consult a qualified South African attorney for advice specific to your situation.*\n\n";

        string answerText;

        // Fallback: no chunks met the threshold — query the AI without legislation context.
        if (topChunks.Count == 0)
        {
            var fallbackAnswer = await CallChatCompletionsAsync(
                RagPromptBuilder.BuildFallbackSystemPrompt(detectedLanguage),
                request.QuestionText);

            return new RagAnswerResult
            {
                AnswerText = disclaimer + fallbackAnswer,
                IsInsufficientInformation = true,
                Citations = new List<RagCitationDto>(),
                ChunkIds = new List<Guid>(),
                AnswerId = null,
                DetectedLanguageCode = detectedLanguageCode
            };
        }

        // Build prompt and call GPT-4o with legislation context.
        var systemPrompt = RagPromptBuilder.BuildSystemPrompt(detectedLanguage);
        var contextBlock = RagPromptBuilder.BuildContextBlock(topChunks);
        var userPrompt = RagPromptBuilder.BuildUserPrompt(request.QuestionText, contextBlock);
        answerText = await CallChatCompletionsAsync(systemPrompt, userPrompt);

        // Persist the Q&A chain only when a user session exists (auth is deferred in this phase).
        Guid? answerId = null;
        if (AbpSession.UserId.HasValue)
        {
            answerId = await PersistQaAsync(
                AbpSession.UserId.Value,
                request.QuestionText,
                translatedText,
                detectedLanguage,
                answerText,
                topChunks);
        }

        // Build and return the result.
        var citations = topChunks.Select(c => new RagCitationDto
        {
            ChunkId = c.ChunkId,
            ActName = c.ActName,
            SectionNumber = c.SectionNumber,
            Excerpt = c.Excerpt.Length > 500 ? c.Excerpt[..500] : c.Excerpt,
            RelevanceScore = c.Score
        }).ToList();

        return new RagAnswerResult
        {
            AnswerText = answerText,
            IsInsufficientInformation = false,
            Citations = citations,
            ChunkIds = topChunks.Select(c => c.ChunkId).ToList(),
            AnswerId = answerId,
            DetectedLanguageCode = detectedLanguageCode
        };
    }

    /// <summary>
    /// Sends a chat completions request to the OpenAI API and returns the assistant's reply text.
    /// Uses the named "OpenAI" HttpClient configured in Startup.cs.
    /// </summary>
    private async Task<string> CallChatCompletionsAsync(string systemPrompt, string userPrompt)
    {
        using var client = _httpClientFactory.CreateClient("OpenAI");
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _apiKey) },
            Content = JsonContent.Create(new OpenAiChatRequest(
                Model: _chatModel,
                Temperature: RagPromptBuilder.ChatTemperature,
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

    /// <summary>
    /// Persists the Q&amp;A chain (Conversation → Question → Answer → AnswerCitations) for the current user.
    /// Stores both the original question text and its English translation; records the detected language
    /// on the Conversation, Question, and Answer entities.
    /// </summary>
    /// <returns>The ID of the persisted <see cref="Answer"/> entity.</returns>
    private async Task<Guid> PersistQaAsync(
        long userId,
        string originalText,
        string translatedText,
        Language language,
        string answerText,
        IEnumerable<RagPromptBuilder.ScoredChunk> usedChunks)
    {
        // Create Conversation.
        var conversation = new Conversation
        {
            UserId = userId,
            Language = language,
            InputMethod = InputMethod.Text,
            StartedAt = DateTime.UtcNow,
            IsPublicFaq = false
        };
        var conversationId = await _conversationRepository.InsertAndGetIdAsync(conversation);

        // Create Question — store both the original text and the English translation.
        var question = new Question
        {
            ConversationId = conversationId,
            OriginalText = originalText,
            TranslatedText = translatedText,
            Language = language,
            InputMethod = InputMethod.Text
        };
        var questionId = await _questionRepository.InsertAndGetIdAsync(question);

        // Create Answer — response language matches the detected input language.
        var answer = new Answer
        {
            QuestionId = questionId,
            Text = answerText,
            Language = language
        };
        var answerId = await _answerRepository.InsertAndGetIdAsync(answer);

        // Create one AnswerCitation per retrieved chunk.
        foreach (var chunk in usedChunks)
        {
            await _citationRepository.InsertAsync(new AnswerCitation
            {
                AnswerId = answerId,
                ChunkId = chunk.ChunkId,
                SectionNumber = chunk.SectionNumber,
                Excerpt = chunk.Excerpt.Length > 500 ? chunk.Excerpt[..500] : chunk.Excerpt,
                RelevanceScore = (decimal)chunk.Score
            });
        }

        return answerId;
    }

    // ── Private types for OpenAI chat REST serialisation ─────────────────────

    /// <summary>Request body for POST /v1/chat/completions.</summary>
    private sealed record OpenAiChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("messages")] OpenAiChatMessage[] Messages);

    /// <summary>A single message in the chat messages array.</summary>
    private sealed record OpenAiChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    /// <summary>Top-level response from POST /v1/chat/completions.</summary>
    private sealed record OpenAiChatResponse(
        [property: JsonPropertyName("choices")] OpenAiChatChoice[] Choices);

    /// <summary>A single choice in the chat response.</summary>
    private sealed record OpenAiChatChoice(
        [property: JsonPropertyName("message")] OpenAiChatMessage Message);
}
