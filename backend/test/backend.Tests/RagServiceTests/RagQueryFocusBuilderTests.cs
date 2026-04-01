using backend.Services.RagService;
using Shouldly;
using Xunit;

namespace backend.Tests.RagServiceTests;

public class RagQueryFocusBuilderTests
{
    [Fact]
    public void Build_RemovesGenericIntentTerms_AndKeepsDistinctiveConcepts()
    {
        var focusQuery = RagQueryFocusBuilder.Build("What are my CCMA rights?");

        focusQuery.ShouldBe("ccma rights");
    }

    [Fact]
    public void Build_WhenOnlyOneDistinctiveTermExists_PreservesHelpfulRightsSignal()
    {
        var focusQuery = RagQueryFocusBuilder.Build("Please help, what are my SARS rights?");

        focusQuery.ShouldBe("sars rights");
    }
}
