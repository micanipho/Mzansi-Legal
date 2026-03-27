using Abp.Domain.Repositories;
using backend.MzansiLegal.Conversations;
using backend.MzansiLegal.KnowledgeBase;
using backend.MzansiLegal.QnA.Dto;
using backend.MzansiLegal.RefLists;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend.MzansiLegal.QnA;

/// <summary>
/// Core RAG pipeline: embed question → vector search → GPT-4o answer with citations.
/// T014
/// </summary>
public class RagService
{
    private readonly EmbeddingService _embeddingService;
    private readonly LanguageService _languageService;
    private readonly IRepository<Conversation, Guid> _conversationRepo;
    private readonly IRepository<Question, Guid> _questionRepo;
    private readonly IRepository<Answer, Guid> _answerRepo;
    private readonly IRepository<AnswerCitation, Guid> _citationRepo;
    private readonly IRepository<DocumentChunk, Guid> _chunkRepo;
    private readonly IRepository<LegalDocument, Guid> _documentRepo;
    private readonly ChatClient _chatClient;
    private const int TopK = 5;

    public RagService(
        EmbeddingService embeddingService,
        LanguageService languageService,
        IRepository<Conversation, Guid> conversationRepo,
        IRepository<Question, Guid> questionRepo,
        IRepository<Answer, Guid> answerRepo,
        IRepository<AnswerCitation, Guid> citationRepo,
        IRepository<DocumentChunk, Guid> chunkRepo,
        IRepository<LegalDocument, Guid> documentRepo,
        IConfiguration configuration)
    {
        _embeddingService = embeddingService;
        _languageService = languageService;
        _conversationRepo = conversationRepo;
        _questionRepo = questionRepo;
        _answerRepo = answerRepo;
        _citationRepo = citationRepo;
        _chunkRepo = chunkRepo;
        _documentRepo = documentRepo;

        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured.");
        var model = configuration["OpenAI:ChatModel"] ?? "gpt-4o";
        var openAiClient = new OpenAI.OpenAIClient(apiKey);
        _chatClient = openAiClient.GetChatClient(model);
    }

    public async Task<QuestionWithAnswerDto> AskAsync(
        long userId,
        AskQuestionInput input)
    {
        var languageEnum = ParseLanguage(input.Language);

        // 1. Get or create conversation
        Conversation conversation;
        if (input.ConversationId.HasValue)
        {
            conversation = await _conversationRepo.GetAsync(input.ConversationId.Value);
        }
        else
        {
            conversation = new Conversation(Guid.NewGuid(), userId, languageEnum, InputMethod.Text);
            await _conversationRepo.InsertAsync(conversation);
        }

        // 2. Detect language and translate to English for RAG
        var detectedLanguage = await _languageService.DetectLanguageAsync(input.Text);
        var englishQuestion = await _languageService.TranslateToEnglishAsync(input.Text, detectedLanguage);

        // 3. Save question
        var question = new Question(Guid.NewGuid(), conversation.Id, input.Text, languageEnum, InputMethod.Text)
        {
            TranslatedText = englishQuestion
        };
        await _questionRepo.InsertAsync(question);

        // 4. Embed the English question and search
        var queryVector = await _embeddingService.GetEmbeddingAsync(englishQuestion);
        var scored = await _embeddingService.SearchAsync(queryVector, TopK);

        // 5. Load the top-k chunks with their documents
        var chunkIds = scored.Select(s => s.ChunkId).ToList();
        var chunks = await _chunkRepo.GetAllListAsync(c => chunkIds.Contains(c.Id));

        // Load documents for citations
        var docIds = chunks.Select(c => c.LegalDocumentId).Distinct().ToList();
        var documents = await _documentRepo.GetAllListAsync(d => docIds.Contains(d.Id));
        var docMap = documents.ToDictionary(d => d.Id);

        // 6. Build context for GPT-4o
        var contextBuilder = new StringBuilder();
        foreach (var sc in scored)
        {
            var chunk = chunks.FirstOrDefault(c => c.Id == sc.ChunkId);
            if (chunk == null) continue;
            docMap.TryGetValue(chunk.LegalDocumentId, out var doc);

            contextBuilder.AppendLine($"[Source: {doc?.ShortName ?? "Unknown"}, Section {chunk.SectionNumber}]");
            contextBuilder.AppendLine(chunk.Content);
            contextBuilder.AppendLine();
        }

        // 7. Generate answer with GPT-4o
        var systemPrompt =
            "You are a South African legal rights assistant. Answer questions using ONLY the provided legal context. " +
            "Always cite the Act name and section number. If the context does not contain an answer, say so clearly. " +
            "Do not fabricate legal information. Keep answers concise and accessible to ordinary citizens.";

        var userPrompt =
            $"Context:\n{contextBuilder}\n\nQuestion: {englishQuestion}\n\nProvide a clear, cited answer.";

        var chatResult = await _chatClient.CompleteChatAsync(
            new ChatMessage[]
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            });

        var englishAnswer = chatResult.Value.Content[0].Text;

        // 8. Translate answer back to user's language
        var localizedAnswer = await _languageService.TranslateFromEnglishAsync(englishAnswer, detectedLanguage);

        // 9. Save answer
        var answer = new Answer(Guid.NewGuid(), question.Id, localizedAnswer, languageEnum);
        await _answerRepo.InsertAsync(answer);

        // 10. Save citations
        var citations = new List<CitationDto>();
        foreach (var sc in scored)
        {
            var chunk = chunks.FirstOrDefault(c => c.Id == sc.ChunkId);
            if (chunk == null) continue;
            docMap.TryGetValue(chunk.LegalDocumentId, out var doc);

            var citation = new AnswerCitation(
                Guid.NewGuid(),
                answer.Id,
                chunk.Id,
                chunk.SectionNumber,
                chunk.Content.Length > 300 ? chunk.Content[..300] + "..." : chunk.Content,
                (decimal)sc.Score);
            await _citationRepo.InsertAsync(citation);

            citations.Add(new CitationDto
            {
                ActName = doc?.Title ?? "Unknown Act",
                Section = chunk.SectionNumber,
                Excerpt = citation.Excerpt,
                Relevance = Math.Round(sc.Score, 3)
            });
        }

        return new QuestionWithAnswerDto
        {
            Id = question.Id,
            ConversationId = conversation.Id,
            Text = input.Text,
            Language = detectedLanguage,
            Answer = new AnswerDto
            {
                Id = answer.Id,
                Text = localizedAnswer,
                Citations = citations
            }
        };
    }

    private static Language ParseLanguage(string lang) => lang?.ToLowerInvariant() switch
    {
        "zu" => Language.Zu,
        "st" => Language.St,
        "af" => Language.Af,
        _ => Language.En
    };
}
