using backend.Services.RagService;
using Shouldly;
using System;
using Xunit;

namespace backend.Tests.RagServiceTests;

public class RagDocumentProfileBuilderTests
{
    private readonly RagDocumentProfileBuilder _builder = new();

    [Fact]
    public void Build_LongSectionTitle_AddsSlidingWindowAcronymAliases()
    {
        var documentId = Guid.NewGuid();
        var profiles = _builder.Build(new[]
        {
            new IndexedChunk(
                Guid.NewGuid(),
                documentId,
                "Labour Relations Act",
                "LRA",
                "66",
                1995,
                "Employment & Labour",
                "112",
                "Establishment of Commission for Conciliation, Mediation and Arbitration",
                "The Commission is established to conciliate and arbitrate labour disputes.",
                new[] { "dismissal", "conciliation", "arbitration" },
                "Employment disputes",
                64,
                new float[] { 1f, 0f })
        });

        var profile = profiles.ShouldHaveSingleItem();

        profile.MetadataTerms.ShouldContain("ccma");
        profile.MetadataPhrases.ShouldContain("ccma");
        profile.MetadataTerms.ShouldContain("lra");
    }

    [Fact]
    public void Build_ExcerptHeadingWithSectionNumber_AddsCleanAcronymAlias()
    {
        var documentId = Guid.NewGuid();
        var profiles = _builder.Build(new[]
        {
            new IndexedChunk(
                Guid.NewGuid(),
                documentId,
                "Labour Relations Act",
                "LRA",
                "66",
                1995,
                "Employment & Labour",
                "112",
                string.Empty,
                "112. Establishment of Commission for Conciliation, Mediation and Arbitration\r\nThe Commission is hereby established as a juristic person.",
                new[] { "conciliation", "arbitration" },
                "Employment disputes",
                64,
                new float[] { 1f, 0f })
        });

        var profile = profiles.ShouldHaveSingleItem();

        profile.MetadataTerms.ShouldContain("ccma");
        profile.MetadataTerms.ShouldNotContain("1ccma");
    }
}
