using backend.Services.RagService;
using Shouldly;
using System;
using Xunit;

namespace backend.Tests.RagServiceTests;

public class RagIndexStoreTests
{
    [Fact]
    public void Replace_SharedIndexData_MarksStoreReady()
    {
        var store = new RagIndexStore();
        var documentId = Guid.NewGuid();
        var chunk = new IndexedChunk(
            Guid.NewGuid(),
            documentId,
            "Labour Relations Act",
            "LRA",
            "66",
            1995,
            "Employment & Labour",
            "112",
            "Establishment of Commission for Conciliation, Mediation and Arbitration",
            "The Commission resolves labour disputes.",
            new[] { "conciliation", "arbitration" },
            "Employment disputes",
            32,
            new float[] { 1f, 0f });
        var profile = new DocumentProfile(
            documentId,
            "Labour Relations Act",
            "LRA",
            "Employment & Labour",
            new[] { "ccma", "labour" },
            new[] { "ccma" },
            new float[] { 1f, 0f });

        store.Replace(new[] { chunk }, new[] { profile });

        store.IsReady.ShouldBeTrue();
        store.LoadedChunks.ShouldHaveSingleItem().ActName.ShouldBe("Labour Relations Act");
        store.DocumentProfiles.ShouldHaveSingleItem().MetadataTerms.ShouldContain("ccma");
    }
}
