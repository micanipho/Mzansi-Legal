using backend.Domains.LegalDocuments;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.EntityFrameworkCore.Seed.Host;

/// <summary>
/// Idempotent seeder that registers all 13 pre-defined legislation document stubs.
/// Each stub is matched against the database by (ShortName, Year) — duplicates are skipped.
/// Documents are created with <c>IsProcessed = false</c> and <c>TotalChunks = 0</c>;
/// the ETL pipeline (run in Phase B via <see cref="LegislationIngestionRunner"/>) will
/// process each document and update these fields once embeddings are generated.
/// </summary>
public class LegalDocumentRegistrar
{
    private readonly backendDbContext _context;

    /// <summary>Initialises the registrar with the host DbContext.</summary>
    public LegalDocumentRegistrar(backendDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Inserts any missing document stubs from <see cref="LegislationManifest.Documents"/>.
    /// Looks up each document's category by name and skips the document with a warning log
    /// if the category is not found (prevents FK violation).
    /// </summary>
    public void Create()
    {
        var categoryIndex = BuildCategoryIndex();

        foreach (var definition in LegislationManifest.Documents)
        {
            CreateIfNotExists(definition, categoryIndex);
        }
    }

    private Dictionary<string, Guid> BuildCategoryIndex()
    {
        return _context.Categories
            .IgnoreQueryFilters()
            .Select(c => new { c.Name, c.Id })
            .ToDictionary(
                c => c.Name.ToLower(),
                c => c.Id);
    }

    private void CreateIfNotExists(
        LegislationManifest.DocumentDefinition definition,
        Dictionary<string, Guid> categoryIndex)
    {
        var shortNameLower = definition.ShortName.ToLower();
        var existing = _context.LegalDocuments
            .IgnoreQueryFilters()
            .FirstOrDefault(d => d.ShortName.ToLower() == shortNameLower && d.Year == definition.Year);

        if (existing != null)
        {
            // Patch FileName if it was seeded from an earlier manifest version that lacked it.
            if (string.IsNullOrWhiteSpace(existing.FileName) && !string.IsNullOrWhiteSpace(definition.FileName))
            {
                existing.FileName = definition.FileName;
            }
            return;
        }

        if (!categoryIndex.TryGetValue(definition.CategoryName.ToLower(), out var categoryId))
        {
            // Category not found — seeder ran out of order or category was deleted.
            // Log and skip rather than throwing, to allow the remaining documents to be registered.
            Console.WriteLine(
                $"[WARN] LegalDocumentRegistrar: Category '{definition.CategoryName}' not found " +
                $"— skipping document '{definition.ShortName}'.");
            return;
        }

        _context.LegalDocuments.Add(new LegalDocument
        {
            Id = Guid.NewGuid(),
            Title = definition.Title,
            ShortName = definition.ShortName,
            ActNumber = definition.ActNumber,
            Year = definition.Year,
            FileName = definition.FileName,
            CategoryId = categoryId,
            IsProcessed = false,
            TotalChunks = 0
        });
    }
}
