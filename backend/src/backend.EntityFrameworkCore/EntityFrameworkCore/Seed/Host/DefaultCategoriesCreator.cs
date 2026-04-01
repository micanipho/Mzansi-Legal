using backend.Domains.LegalDocuments;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace backend.EntityFrameworkCore.Seed.Host;

/// <summary>
/// Idempotent seeder that ensures all 9 pre-defined legislation categories exist in the database.
/// Checks for an existing category by name (case-insensitive) before inserting, so it is safe
/// to run on every Migrator invocation without creating duplicates.
/// </summary>
public class DefaultCategoriesCreator
{
    private readonly backendDbContext _context;

    /// <summary>Initialises the seeder with the host DbContext.</summary>
    public DefaultCategoriesCreator(backendDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Inserts any missing categories from <see cref="LegislationManifest.Categories"/>
    /// and saves them before returning so subsequent seeders can query them.
    /// Existing categories (matched by name, case-insensitive) are left unchanged.
    /// </summary>
    public void Create()
    {
        foreach (var definition in LegislationManifest.Categories)
        {
            CreateIfNotExists(definition);
        }

        _context.SaveChanges();
    }

    private void CreateIfNotExists(LegislationManifest.CategoryDefinition definition)
    {
        var nameLower = definition.Name.ToLower();
        var exists = _context.Categories
            .IgnoreQueryFilters()
            .Any(c => c.Name.ToLower() == nameLower);

        if (exists)
        {
            return;
        }

        _context.Categories.Add(new Category
        {
            Id = Guid.NewGuid(),
            Name = definition.Name,
            Domain = definition.Domain,
            Icon = definition.Icon,
            SortOrder = definition.SortOrder
        });
    }
}
