using Abp.Application.Services;
using Abp.Domain.Repositories;
using backend.Domains.QA;
using backend.Services.FaqService.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services.FaqService;

/// <summary>
/// Reads curated FAQ conversations and projects them into a public-safe explorer feed.
/// Only conversations explicitly marked as public FAQ and backed by an admin-approved answer
/// are exposed to public clients.
/// </summary>
public class PublicFaqAppService : ApplicationService, IPublicFaqAppService
{
    private readonly IRepository<Conversation, Guid> _conversationRepository;

    public PublicFaqAppService(IRepository<Conversation, Guid> conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    /// <summary>
    /// Returns the public FAQ feed used by discovery surfaces such as the rights explorer.
    /// </summary>
    public Task<PublicFaqListDto> GetPublicFaqsAsync(Guid? categoryId = null, string languageCode = null)
    {
        var requestedLanguage = ParseLanguage(languageCode);
        var conversations = LoadFaqConversations(categoryId);

        var items = conversations
            .Select(conversation => MapToItem(conversation, requestedLanguage))
            .Where(item => item != null)
            .Cast<PublicFaqItemDto>()
            .OrderBy(item => GetTopicSortOrder(item.TopicKey))
            .ThenBy(item => item.Title)
            .ToList();

        return Task.FromResult(new PublicFaqListDto
        {
            Items = items,
            TotalCount = items.Count
        });
    }

    private List<Conversation> LoadFaqConversations(Guid? categoryId)
    {
        var query = _conversationRepository
            .GetAll()
            .Include(conversation => conversation.FaqCategory)
            .Include(conversation => conversation.Questions)
                .ThenInclude(question => question.Answers)
                    .ThenInclude(answer => answer.Citations)
                        .ThenInclude(citation => citation.Chunk)
                            .ThenInclude(chunk => chunk.Document)
            .Where(conversation => conversation.IsPublicFaq);

        if (categoryId.HasValue)
        {
            query = query.Where(conversation => conversation.FaqCategoryId == categoryId.Value);
        }

        return query.ToList();
    }

    private static PublicFaqItemDto MapToItem(Conversation conversation, Language? requestedLanguage)
    {
        var selection = SelectApprovedAnswer(conversation, requestedLanguage);
        if (selection == null)
        {
            return null;
        }

        var citations = selection.Answer.Citations?
            .OrderByDescending(citation => citation.RelevanceScore)
            .ThenBy(citation => citation.SectionNumber)
            .Select(citation => new PublicFaqCitationDto
            {
                Id = citation.Id,
                ActName = citation.Chunk?.Document?.Title ?? "Legislation source",
                SectionNumber = citation.SectionNumber ?? string.Empty,
                Excerpt = citation.Excerpt ?? string.Empty,
                RelevanceScore = citation.RelevanceScore
            })
            .ToList() ?? new List<PublicFaqCitationDto>();

        var strongestCitation = citations.FirstOrDefault();

        return new PublicFaqItemDto
        {
            Id = conversation.Id,
            ConversationId = conversation.Id,
            QuestionId = selection.Question.Id,
            AnswerId = selection.Answer.Id,
            CategoryId = conversation.FaqCategoryId,
            CategoryName = conversation.FaqCategory?.Name ?? "General rights",
            TopicKey = ToTopicKey(conversation.FaqCategory?.Name),
            Title = selection.Question.OriginalText?.Trim() ?? string.Empty,
            Summary = BuildSummary(selection.Answer.Text),
            Explanation = selection.Answer.Text?.Trim() ?? string.Empty,
            SourceQuote = strongestCitation?.Excerpt,
            PrimaryCitation = strongestCitation == null
                ? string.Empty
                : BuildCitationLabel(strongestCitation.ActName, strongestCitation.SectionNumber),
            LanguageCode = ToIsoCode(selection.Question.Language),
            PublishedAt = selection.Answer.CreationTime,
            Citations = citations
        };
    }

    private static ApprovedFaqSelection SelectApprovedAnswer(Conversation conversation, Language? requestedLanguage)
    {
        var approvedPairs = conversation.Questions?
            .SelectMany(question => (question.Answers ?? Array.Empty<Answer>())
                .Where(answer => answer.IsAccurate == true)
                .Select(answer => new ApprovedFaqSelection(question, answer)))
            .ToList();

        if (approvedPairs == null || approvedPairs.Count == 0)
        {
            return null;
        }

        if (requestedLanguage.HasValue)
        {
            var matchingLanguage = approvedPairs
                .Where(pair => pair.Question.Language == requestedLanguage.Value)
                .OrderByDescending(pair => pair.Answer.CreationTime)
                .FirstOrDefault();

            if (matchingLanguage != null)
            {
                return matchingLanguage;
            }
        }

        var englishFallback = approvedPairs
            .Where(pair => pair.Question.Language == Language.English)
            .OrderByDescending(pair => pair.Answer.CreationTime)
            .FirstOrDefault();

        return englishFallback
            ?? approvedPairs.OrderByDescending(pair => pair.Answer.CreationTime).FirstOrDefault();
    }

    private static string BuildSummary(string answerText)
    {
        if (string.IsNullOrWhiteSpace(answerText))
        {
            return string.Empty;
        }

        var normalized = answerText
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Trim();

        var sentenceBreakIndex = normalized.IndexOfAny(new[] { '.', '!', '?' });
        if (sentenceBreakIndex >= 0 && sentenceBreakIndex < 160)
        {
            var firstSentence = normalized[..(sentenceBreakIndex + 1)].Trim();
            if (firstSentence.Length >= 40)
            {
                return firstSentence;
            }

            var nextSentenceStart = sentenceBreakIndex + 1;
            while (nextSentenceStart < normalized.Length && normalized[nextSentenceStart] == ' ')
            {
                nextSentenceStart++;
            }

            var nextSentenceEnd = normalized.IndexOfAny(new[] { '.', '!', '?' }, nextSentenceStart);
            if (nextSentenceEnd > nextSentenceStart && nextSentenceEnd < 160)
            {
                return normalized[..(nextSentenceEnd + 1)].Trim();
            }
        }

        if (normalized.Length <= 160)
        {
            return normalized;
        }

        return $"{normalized[..157].TrimEnd()}...";
    }

    private static string BuildCitationLabel(string actName, string sectionNumber)
    {
        if (string.IsNullOrWhiteSpace(actName))
        {
            return sectionNumber ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(sectionNumber))
        {
            return actName;
        }

        return $"{actName}, {sectionNumber}";
    }

    private static Language? ParseLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return null;
        }

        return languageCode.Trim().ToLowerInvariant() switch
        {
            "en" => Language.English,
            "zu" => Language.Zulu,
            "st" => Language.Sesotho,
            "af" => Language.Afrikaans,
            _ => null
        };
    }

    private static string ToIsoCode(Language language)
    {
        return language switch
        {
            Language.Zulu => "zu",
            Language.Sesotho => "st",
            Language.Afrikaans => "af",
            _ => "en"
        };
    }

    private static string ToTopicKey(string categoryName)
    {
        var normalized = categoryName?.Trim().ToLowerInvariant() ?? string.Empty;

        return normalized switch
        {
            "employment & labour" => "employment",
            "housing & eviction" => "housing",
            "consumer rights" => "consumer",
            "debt & credit" => "debtCredit",
            "privacy & data" => "privacy",
            "safety & harassment" => "safety",
            "insurance & retirement" => "insurance",
            "contract analysis" => "contracts",
            "tax" => "tax",
            _ when normalized.Contains("employment") || normalized.Contains("labour") => "employment",
            _ when normalized.Contains("housing") || normalized.Contains("eviction") => "housing",
            _ when normalized.Contains("consumer") => "consumer",
            _ when normalized.Contains("debt") || normalized.Contains("credit") => "debtCredit",
            _ when normalized.Contains("privacy") || normalized.Contains("data") => "privacy",
            _ when normalized.Contains("safety") || normalized.Contains("harassment") => "safety",
            _ when normalized.Contains("insurance") || normalized.Contains("retirement") => "insurance",
            _ when normalized.Contains("tax") => "tax",
            _ when normalized.Contains("contract") => "contracts",
            _ => "legal"
        };
    }

    private static int GetTopicSortOrder(string topicKey)
    {
        return topicKey switch
        {
            "employment" => 1,
            "housing" => 2,
            "consumer" => 3,
            "debtCredit" => 4,
            "tax" => 5,
            "privacy" => 6,
            "safety" => 7,
            "insurance" => 8,
            "contracts" => 9,
            _ => 99
        };
    }

    private sealed record ApprovedFaqSelection(Question Question, Answer Answer);
}
