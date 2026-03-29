using backend.Domains.LegalDocuments;
using backend.EntityFrameworkCore.Seed.Host;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Linq;
using Xunit;

namespace backend.Tests.Seed;

/// <summary>
/// Verifies that DefaultCategoriesCreator produces exactly 9 categories,
/// assigns correct domain and sort order values, and is idempotent on repeated runs.
/// </summary>
public class DefaultCategoriesCreatorTests : backendTestBase
{
    [Fact]
    public void Create_ShouldInsert_AllNineCategories()
    {
        var count = UsingDbContext(context =>
            context.Categories.IgnoreQueryFilters().Count());

        count.ShouldBe(9);
    }

    [Fact]
    public void Create_ShouldAssign_CorrectDomains()
    {
        UsingDbContext(context =>
        {
            var legalCount = context.Categories
                .IgnoreQueryFilters()
                .Count(c => c.Domain == DocumentDomain.Legal);

            var financialCount = context.Categories
                .IgnoreQueryFilters()
                .Count(c => c.Domain == DocumentDomain.Financial);

            // 7 legal categories, 2 financial (Tax + Insurance & Retirement)
            legalCount.ShouldBe(7);
            financialCount.ShouldBe(2);
        });
    }

    [Fact]
    public void Create_ShouldAssign_UniqueSortOrders_OneToNine()
    {
        UsingDbContext(context =>
        {
            var sortOrders = context.Categories
                .IgnoreQueryFilters()
                .Select(c => c.SortOrder)
                .OrderBy(s => s)
                .ToList();

            sortOrders.ShouldBe(Enumerable.Range(1, 9).ToList());
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
        });

        var count = UsingDbContext(context =>
            context.Categories.IgnoreQueryFilters().Count());

        count.ShouldBe(9);
    }

    [Fact]
    public void Create_ShouldContain_AllExpectedCategoryNames()
    {
        var expectedNames = new[]
        {
            "Employment & Labour", "Housing & Eviction", "Consumer Rights",
            "Debt & Credit", "Privacy & Data", "Safety & Harassment",
            "Insurance & Retirement", "Tax", "Contract Analysis"
        };

        UsingDbContext(context =>
        {
            foreach (var name in expectedNames)
            {
                var nameLower = name.ToLower();
                context.Categories
                    .IgnoreQueryFilters()
                    .Any(c => c.Name.ToLower() == nameLower)
                    .ShouldBeTrue($"Category '{name}' should exist");
            }
        });
    }
}
