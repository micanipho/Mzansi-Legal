using backend.EntityFrameworkCore.Seed.Host;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Shouldly;
using Xunit;

namespace backend.Tests.Seed;

/// <summary>
/// Verifies that LegalDocumentRegistrar produces exactly 13 document stubs,
/// initialises them with IsProcessed=false and TotalChunks=0, and is idempotent.
/// </summary>
public class LegalDocumentRegistrarTests : backendTestBase
{
    [Fact]
    public void Create_ShouldInsert_AllThirteenDocuments()
    {
        var count = UsingDbContext(context =>
            context.LegalDocuments.IgnoreQueryFilters().Count());

        count.ShouldBe(13);
    }

    [Fact]
    public void Create_AllDocuments_ShouldHave_IsProcessedFalse()
    {
        UsingDbContext(context =>
        {
            var unprocessedCount = context.LegalDocuments
                .IgnoreQueryFilters()
                .Count(d => !d.IsProcessed);

            unprocessedCount.ShouldBe(13);
        });
    }

    [Fact]
    public void Create_AllDocuments_ShouldHave_TotalChunksZero()
    {
        UsingDbContext(context =>
        {
            var allZero = context.LegalDocuments
                .IgnoreQueryFilters()
                .All(d => d.TotalChunks == 0);

            allZero.ShouldBeTrue();
        });
    }

    [Fact]
    public void Create_AllDocuments_ShouldHave_ValidCategoryAssigned()
    {
        UsingDbContext(context =>
        {
            var withoutCategory = context.LegalDocuments
                .IgnoreQueryFilters()
                .Count(d => d.CategoryId == default);

            withoutCategory.ShouldBe(0, "Every document should have a valid CategoryId");
        });
    }

    [Fact]
    public void Create_AllDocuments_ShouldHave_FileNameSet()
    {
        UsingDbContext(context =>
        {
            var withoutFileName = context.LegalDocuments
                .IgnoreQueryFilters()
                .Count(d => string.IsNullOrWhiteSpace(d.FileName));

            withoutFileName.ShouldBe(0, "Every document should have a FileName set for ETL lookup");
        });
    }

    [Fact]
    public void Create_CalledTwice_ShouldNotCreateDuplicates()
    {
        // The base constructor already called InitialHostDbBuilder (first run).
        // Calling it again simulates a second Migrator invocation.
        UsingDbContext(context =>
        {
            new DefaultCategoriesCreator(context).Create();
            new LegalDocumentRegistrar(context).Create();
        });

        var count = UsingDbContext(context =>
            context.LegalDocuments.IgnoreQueryFilters().Count());

        count.ShouldBe(13);
    }

    [Fact]
    public void Create_ShouldContain_AllExpectedShortNames()
    {
        var expectedShortNames = new[]
        {
            "Constitution", "BCEA", "CPA", "LRA", "POPIA", "RHA", "PHA", "NCA",
            "FAIS", "TAA", "PFA", "SARS Guide", "FSCA Materials"
        };

        UsingDbContext(context =>
        {
            foreach (var shortName in expectedShortNames)
            {
                var nameLower = shortName.ToLower();
                context.LegalDocuments
                    .IgnoreQueryFilters()
                    .Any(d => d.ShortName.ToLower() == nameLower)
                    .ShouldBeTrue($"Document '{shortName}' should exist");
            }
        });
    }

    [Fact]
    public void LegalDocuments_Model_ShouldUse_ShortNameAndYear_UniqueIndex()
    {
        UsingDbContext(context =>
        {
            var entityType = context.Model.FindEntityType(typeof(backend.Domains.LegalDocuments.LegalDocument));
            entityType.ShouldNotBeNull();

            var uniqueIndex = entityType!
                .GetIndexes()
                .Single(i => i.IsUnique && i.Properties.Count == 2);

            uniqueIndex.Properties.Select(p => p.Name).ShouldBe(new[] { "ShortName", "Year" });
        });
    }
}
