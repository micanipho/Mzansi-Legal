using backend.Services.RagService;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace backend.Tests.RagServiceTests;

public class RagRetrievalPlannerTests
{
    private readonly RagRetrievalPlanner _planner = new();
    private readonly RagDocumentProfileBuilder _documentProfileBuilder = new();
    private readonly RagSourceHintExtractor _hintExtractor = new();

    [Fact]
    public void BuildPlan_PlainLanguageEvictionQuestion_SelectsHousingSourceWithoutActName()
    {
        const string question = "Can my landlord evict me without a court order?";
        var chunks = new[]
        {
            CreateChunk("Constitution of the Republic of South Africa", "Constitution", "Housing", "court order eviction housing", "Housing Rights", new[] { "landlord", "eviction", "court", "home" }, new float[] { 1f, 0f }),
            CreateChunk("Labour Relations Act", "LRA", "Employment", "dismissal hearing worker", "Employment", new[] { "dismissal", "employee" }, new float[] { 0f, 1f })
        };

        var semanticMatches = _planner.BuildSemanticMatches(new float[] { 1f, 0f }, chunks);
        var hints = _hintExtractor.Extract(question, chunks);
        var plan = _planner.BuildPlan(
            question,
            new float[] { 1f, 0f },
            semanticMatches,
            hints,
            _documentProfileBuilder.Build(chunks));

        plan.RankedDocuments[0].ActName.ShouldBe("Constitution of the Republic of South Africa");
        plan.SelectedChunks.Select(chunk => chunk.ActName).ShouldContain("Constitution of the Republic of South Africa");
        plan.SelectedChunks.Select(chunk => chunk.ActName).ShouldNotContain("Labour Relations Act");
    }

    [Fact]
    public void BuildPlan_SemanticallyEquivalentVariants_KeepSamePrimarySource()
    {
        var chunks = new[]
        {
            CreateChunk("Constitution of the Republic of South Africa", "Constitution", "Housing", "court order eviction housing", "Housing Rights", new[] { "landlord", "eviction", "court", "home" }, new float[] { 1f, 0f }),
            CreateChunk("Labour Relations Act", "LRA", "Employment", "dismissal hearing worker", "Employment", new[] { "dismissal", "employee" }, new float[] { 0f, 1f })
        };

        var firstPlan = BuildPlan(
            "Can my landlord evict me without a court order?",
            new float[] { 1f, 0f },
            chunks);

        var secondPlan = BuildPlan(
            "Can a property owner throw me out if I rent from them?",
            new float[] { 0.98f, 0.05f },
            chunks);

        firstPlan.PrimaryDocumentId.ShouldBe(secondPlan.PrimaryDocumentId);
    }

    [Fact]
    public void BuildPlan_WrongActHint_DoesNotOutrankStrongerFactualMatch()
    {
        const string question = "Under the Labour Relations Act, can my landlord evict me without a court order?";
        var chunks = new[]
        {
            CreateChunk("Constitution of the Republic of South Africa", "Constitution", "Housing", "court order eviction housing", "Housing Rights", new[] { "landlord", "eviction", "court", "home" }, new float[] { 1f, 0f }),
            CreateChunk("Labour Relations Act", "LRA", "Employment", "dismissal hearing worker", "Employment", new[] { "dismissal", "employee", "labour" }, new float[] { 0.35f, 0.95f })
        };

        var semanticMatches = _planner.BuildSemanticMatches(new float[] { 1f, 0f }, chunks);
        var hints = _hintExtractor.Extract(question, chunks);
        var plan = _planner.BuildPlan(
            question,
            new float[] { 1f, 0f },
            semanticMatches,
            hints,
            _documentProfileBuilder.Build(chunks));

        plan.RankedDocuments[0].ActName.ShouldBe("Constitution of the Republic of South Africa");
    }

    [Fact]
    public void BuildPlan_MultiSourceQuestion_SelectsSupportingDocument()
    {
        const string question = "Can a landlord evict me and ignore the rental protections in my lease?";
        var constitutionId = Guid.NewGuid();
        var rentalId = Guid.NewGuid();

        var chunks = new[]
        {
            CreateChunk("Constitution of the Republic of South Africa", "Constitution", "Housing", "court order eviction housing", "Housing Rights", new[] { "landlord", "eviction", "court", "home" }, new float[] { 1f, 0f }, constitutionId),
            CreateChunk("Rental Housing Act 50 of 1999", "Rental Housing Act", "Housing", "rental tenant lease landlord", "Rental Housing", new[] { "rental", "tenant", "lease", "landlord" }, new float[] { 0.92f, 0.08f }, rentalId),
            CreateChunk("Labour Relations Act", "LRA", "Employment", "dismissal hearing worker", "Employment", new[] { "dismissal", "employee" }, new float[] { 0f, 1f })
        };

        var plan = BuildPlan(question, new float[] { 1f, 0f }, chunks);

        var selectedDocumentIds = plan.SelectedChunks
            .Select(chunk => chunk.DocumentId)
            .Distinct()
            .ToList();

        selectedDocumentIds.ShouldContain(constitutionId);
        selectedDocumentIds.ShouldContain(rentalId);
        plan.SupportingDocumentIds.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void BuildPlan_BroadCcmaRightsQuestion_UsesDocumentMeaningAndSectionAliases()
    {
        const string question = "What are my CCMA rights?";
        var chunks = new[]
        {
            CreateChunk(
                "Labour Relations Act",
                "LRA",
                "Employment & Labour",
                "The Commission handles conciliation, arbitration, and unfair dismissal disputes.",
                "Employment disputes",
                new[] { "dismissal", "conciliation", "arbitration", "employee" },
                new float[] { 0.88f, 0.12f },
                sectionTitle: "Establishment of Commission for Conciliation, Mediation and Arbitration"),
            CreateChunk(
                "Labour Relations Act",
                "LRA",
                "Employment & Labour",
                "Every employee has the right not to be unfairly dismissed.",
                "Employment disputes",
                new[] { "employee", "rights", "dismissal", "unfair labour practice" },
                new float[] { 0.84f, 0.16f },
                sectionNumber: "191",
                sectionTitle: "Disputes about unfair dismissals and unfair labour practices"),
            CreateChunk(
                "Consumer Protection Act",
                "CPA",
                "Consumer Rights",
                "Consumers may return defective goods within six months.",
                "Consumer rights",
                new[] { "consumer", "refund", "goods" },
                new float[] { 0.08f, 0.92f })
        };

        var plan = BuildPlan(question, new float[] { 0.86f, 0.14f }, chunks);

        plan.RankedDocuments[0].ActName.ShouldBe("Labour Relations Act");
        plan.IsAmbiguousQuestion.ShouldBeFalse();
        plan.SelectedChunks.ShouldContain(chunk => chunk.ActName == "Labour Relations Act");
    }

    [Fact]
    public void BuildPlan_DistinctiveAcronymOutweighsGenericRightsLanguage()
    {
        const string question = "What are my CCMA rights?";
        var labourRelationsActId = Guid.NewGuid();
        var employmentActId = Guid.NewGuid();
        var consumerActId = Guid.NewGuid();

        var chunks = new[]
        {
            CreateChunk(
                "Labour Relations Act",
                "LRA",
                "Employment & Labour",
                "The Commission resolves labour disputes through conciliation and arbitration.",
                "Employment disputes",
                new[] { "dismissal", "conciliation", "arbitration", "employee" },
                new float[] { 0.60f, 0.40f },
                labourRelationsActId,
                sectionNumber: "112",
                sectionTitle: "Establishment of Commission for Conciliation, Mediation and Arbitration"),
            CreateChunk(
                "Basic Conditions of Employment Act",
                "BCEA",
                "Employment & Labour",
                "Employees have rights to fair working hours and leave.",
                "Employment rights",
                new[] { "employee", "rights", "leave", "hours" },
                new float[] { 0.68f, 0.32f },
                employmentActId),
            CreateChunk(
                "Consumer Protection Act",
                "CPA",
                "Consumer Rights",
                "Consumers have rights to fair value and safe goods.",
                "Consumer rights",
                new[] { "consumer", "rights", "goods" },
                new float[] { 0.66f, 0.34f },
                consumerActId)
        };

        var plan = BuildPlan(question, new float[] { 0.67f, 0.33f }, chunks);

        plan.RankedDocuments[0].ActName.ShouldBe("Labour Relations Act");
        plan.RankedDocuments[0].MetadataAlignmentScore.ShouldBeGreaterThan(
            plan.RankedDocuments[1].MetadataAlignmentScore);
    }

    private RetrievalDecision BuildPlan(string questionText, float[] questionVector, IReadOnlyList<IndexedChunk> chunks)
    {
        var semanticMatches = _planner.BuildSemanticMatches(questionVector, chunks);
        var hints = _hintExtractor.Extract(questionText, chunks);
        var documentProfiles = _documentProfileBuilder.Build(chunks);
        return _planner.BuildPlan(questionText, questionVector, semanticMatches, hints, documentProfiles);
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
}
