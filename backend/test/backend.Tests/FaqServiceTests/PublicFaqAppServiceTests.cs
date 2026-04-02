#nullable enable
using Abp.Domain.Repositories;
using backend.Domains.LegalDocuments;
using backend.Domains.QA;
using backend.Services.FaqService;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests.FaqServiceTests;

public class PublicFaqAppServiceTests
{
    [Fact]
    public async Task GetPublicFaqsAsync_ReturnsOnlyApprovedPublicFaqEntries()
    {
        var housingCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Housing & Eviction"
        };

        var publicApproved = CreateConversation(
            housingCategory,
            isPublicFaq: true,
            questionLanguage: Language.English,
            questionText: "Can my landlord evict me without a court order?",
            answerText: "No. A landlord cannot evict you without a court order. The court must consider the circumstances.",
            isAccurate: true);

        var publicUnapproved = CreateConversation(
            housingCategory,
            isPublicFaq: true,
            questionLanguage: Language.English,
            questionText: "Can my landlord lock me out?",
            answerText: "No. Lockouts are not allowed.",
            isAccurate: null);

        var privateApproved = CreateConversation(
            housingCategory,
            isPublicFaq: false,
            questionLanguage: Language.English,
            questionText: "Can a landlord keep my deposit forever?",
            answerText: "No. A landlord must account for the deposit.",
            isAccurate: true);

        var service = CreateService(publicApproved, publicUnapproved, privateApproved);

        var result = await service.GetPublicFaqsAsync();

        result.TotalCount.ShouldBe(1);
        result.Items.Count.ShouldBe(1);
        result.Items[0].Title.ShouldBe("Can my landlord evict me without a court order?");
        result.Items[0].TopicKey.ShouldBe("housing");
        result.Items[0].PrimaryCitation.ShouldContain("Constitution of the Republic of South Africa");
        result.Items[0].Summary.ShouldBe("No. A landlord cannot evict you without a court order.");
        result.Items[0].SourceQuote.ShouldContain("No one may be evicted");
    }

    [Fact]
    public async Task GetPublicFaqsAsync_WhenRequestedLanguageExists_PrefersMatchingLanguage()
    {
        var employmentCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Employment & Labour"
        };

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            IsPublicFaq = true,
            FaqCategoryId = employmentCategory.Id,
            FaqCategory = employmentCategory,
            Questions = new List<Question>()
        };

        conversation.Questions.Add(CreateQuestionWithApprovedAnswer(
            conversation.Id,
            Language.English,
            "Can I be dismissed without a hearing?",
            "No. An employer should follow a fair process before dismissal.",
            "Labour Relations Act 66 of 1995",
            "Section 188"));

        conversation.Questions.Add(CreateQuestionWithApprovedAnswer(
            conversation.Id,
            Language.Zulu,
            "Ngingaxoshwa ngaphandle kokulalelwa?",
            "Cha. Umqashi kufanele alandele inqubo enobulungisa ngaphambi kokuxoshwa.",
            "Labour Relations Act 66 of 1995",
            "Section 188"));

        var service = CreateService(conversation);

        var result = await service.GetPublicFaqsAsync(languageCode: "zu");

        result.TotalCount.ShouldBe(1);
        result.Items[0].LanguageCode.ShouldBe("zu");
        result.Items[0].Title.ShouldBe("Ngingaxoshwa ngaphandle kokulalelwa?");
        result.Items[0].Explanation.ShouldStartWith("Cha.");
        result.Items[0].TopicKey.ShouldBe("employment");
    }

    [Fact]
    public async Task GetPublicFaqsAsync_CategoryFilter_ReturnsOnlyMatchingCategory()
    {
        var housingCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Housing & Eviction"
        };

        var consumerCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Consumer Rights"
        };

        var housingFaq = CreateConversation(
            housingCategory,
            isPublicFaq: true,
            questionLanguage: Language.English,
            questionText: "Can a landlord cut my electricity?",
            answerText: "No. A landlord cannot cut your electricity without following lawful process.",
            isAccurate: true);

        var consumerFaq = CreateConversation(
            consumerCategory,
            isPublicFaq: true,
            questionLanguage: Language.English,
            questionText: "Can I return defective goods?",
            answerText: "Yes. You may return defective goods within six months in many cases.",
            isAccurate: true);

        var service = CreateService(housingFaq, consumerFaq);

        var result = await service.GetPublicFaqsAsync(housingCategory.Id);

        result.TotalCount.ShouldBe(1);
        result.Items[0].CategoryId.ShouldBe(housingCategory.Id);
        result.Items[0].TopicKey.ShouldBe("housing");
    }

    private static PublicFaqAppService CreateService(params Conversation[] conversations)
    {
        var repository = Substitute.For<IRepository<Conversation, Guid>>();
        repository.GetAll().Returns(conversations.AsQueryable());
        return new PublicFaqAppService(repository);
    }

    private static Conversation CreateConversation(
        Category category,
        bool isPublicFaq,
        Language questionLanguage,
        string questionText,
        string answerText,
        bool? isAccurate)
    {
        var conversationId = Guid.NewGuid();

        return new Conversation
        {
            Id = conversationId,
            IsPublicFaq = isPublicFaq,
            FaqCategoryId = category.Id,
            FaqCategory = category,
            Questions = new List<Question>
            {
                CreateQuestionWithAnswer(
                    conversationId,
                    questionLanguage,
                    questionText,
                    answerText,
                    isAccurate,
                    "Constitution of the Republic of South Africa",
                    "Section 26(3)")
            }
        };
    }

    private static Question CreateQuestionWithApprovedAnswer(
        Guid conversationId,
        Language language,
        string questionText,
        string answerText,
        string actName,
        string sectionNumber)
    {
        return CreateQuestionWithAnswer(
            conversationId,
            language,
            questionText,
            answerText,
            true,
            actName,
            sectionNumber);
    }

    private static Question CreateQuestionWithAnswer(
        Guid conversationId,
        Language language,
        string questionText,
        string answerText,
        bool? isAccurate,
        string actName,
        string sectionNumber)
    {
        var documentId = Guid.NewGuid();
        var chunk = new DocumentChunk
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            Document = new LegalDocument
            {
                Id = documentId,
                Title = actName,
                Year = 1996,
                CategoryId = Guid.NewGuid()
            },
            SectionNumber = sectionNumber,
            Content = "No one may be evicted from their home without an order of court.",
            SortOrder = 0
        };

        return new Question
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            OriginalText = questionText,
            TranslatedText = questionText,
            Language = language,
            InputMethod = InputMethod.Text,
            Answers = new List<Answer>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    QuestionId = Guid.NewGuid(),
                    Text = answerText,
                    Language = language,
                    IsAccurate = isAccurate,
                    CreationTime = DateTime.UtcNow,
                    Citations = new List<AnswerCitation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            ChunkId = chunk.Id,
                            Chunk = chunk,
                            SectionNumber = sectionNumber,
                            Excerpt = "No one may be evicted from their home without an order of court.",
                            RelevanceScore = 0.95m
                        }
                    }
                }
            }
        };
    }
}
