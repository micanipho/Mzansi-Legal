using backend.MzansiLegal.Categories;
using backend.MzansiLegal.KnowledgeBase;
using backend.MzansiLegal.RefLists;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.EntityFrameworkCore.Seed.Host;

/// <summary>
/// Seeds the 13 core SA legislation documents and their categories.
/// Runs only once — skipped if categories already exist.
/// </summary>
public class MzansiLegalDataSeeder
{
    private readonly backendDbContext _context;

    public MzansiLegalDataSeeder(backendDbContext context)
    {
        _context = context;
    }

    public void Create()
    {
        if (_context.Categories.Any()) return;

        var categories = SeedCategories();
        SeedLegalDocuments(categories);
    }

    private Dictionary<string, Guid> SeedCategories()
    {
        var cats = new[]
        {
            new Category(Guid.NewGuid(), "Employment",   "work",    LegalDomain.Legal,    1) { LocalizedLabels = """{"en":"Employment","zu":"Ukuqashwa","st":"Mosebetsi","af":"Indiensneming"}""" },
            new Category(Guid.NewGuid(), "Housing",      "home",    LegalDomain.Legal,    2) { LocalizedLabels = """{"en":"Housing","zu":"Izindawo Zokuhlala","st":"Bodulo","af":"Behuising"}""" },
            new Category(Guid.NewGuid(), "Consumer",     "shop",    LegalDomain.Legal,    3) { LocalizedLabels = """{"en":"Consumer Rights","zu":"Amalungelo Abathengi","st":"Ditokelo tsa Badudi","af":"Verbruikersregte"}""" },
            new Category(Guid.NewGuid(), "Family",       "family",  LegalDomain.Legal,    4) { LocalizedLabels = """{"en":"Family Law","zu":"Umthetho Womndeni","st":"Molao wa Lelapa","af":"Familiereg"}""" },
            new Category(Guid.NewGuid(), "Financial",    "bank",    LegalDomain.Financial,5) { LocalizedLabels = """{"en":"Financial Rights","zu":"Amalungelo Ezimali","st":"Ditokelo tsa Ditjhelete","af":"Finansiële Regte"}""" },
            new Category(Guid.NewGuid(), "Criminal",     "shield",  LegalDomain.Legal,    6) { LocalizedLabels = """{"en":"Criminal Justice","zu":"Ubulungiswa Bobugebengu","st":"Toka ya Botlokotsebe","af":"Strafreg"}""" },
        };

        _context.Categories.AddRange(cats);
        _context.SaveChanges();

        return cats.ToDictionary(c => c.Name, c => c.Id);
    }

    private void SeedLegalDocuments(Dictionary<string, Guid> categoryIds)
    {
        // 13 core SA legislation documents — PDFs to be uploaded by admin via /admin/document/upload
        var docs = new[]
        {
            new LegalDocument(Guid.NewGuid(), "Constitution of the Republic of South Africa, 1996",        "Constitution",              "Act 108",  1996, categoryIds["Criminal"])   { IsProcessed = false },
            new LegalDocument(Guid.NewGuid(), "Labour Relations Act",                                      "LRA",                       "Act 66",   1995, categoryIds["Employment"]) { IsProcessed = false },
            new LegalDocument(Guid.NewGuid(), "Basic Conditions of Employment Act",                        "BCEA",                      "Act 75",   1997, categoryIds["Employment"]) { IsProcessed = false },
            new LegalDocument(Guid.NewGuid(), "Employment Equity Act",                                     "EEA",                       "Act 55",   1998, categoryIds["Employment"]) { IsProcessed = false },
            new LegalDocument(Guid.NewGuid(), "Consumer Protection Act",                                   "CPA",                       "Act 68",   2008, categoryIds["Consumer"])   { IsProcessed = false },
            new LegalDocument(Guid.NewGuid(), "National Credit Act",                                       "NCA",                       "Act 34",   2005, categoryIds["Financial"])  { IsProcessed = false },
            new LegalDocument(Guid.NewGuid(), "Rental Housing Act",                                        "Rental Housing Act",        "Act 50",   1999, categoryIds["Housing"])    { IsProcessed = false },
            new LegalDocument(Guid.NewGuid(), "Prevention of Illegal Eviction from and Unlawful Occupation of Land Act", "PIE Act",   "Act 19",   1998, categoryIds["Housing"])    { IsProcessed = false },
            new LegalDocument(Guid.NewGuid(), "Domestic Violence Act",                                     "DVA",                       "Act 116",  1998, categoryIds["Family"])     { IsProcessed = false },
            new LegalDocument(Guid.NewGuid(), "Children's Act",                                            "Children's Act",            "Act 38",   2005, categoryIds["Family"])     { IsProcessed = false },
            new LegalDocument(Guid.NewGuid(), "Protection of Personal Information Act",                    "POPIA",                     "Act 4",    2013, categoryIds["Consumer"])   { IsProcessed = false },
            new LegalDocument(Guid.NewGuid(), "Unemployment Insurance Act",                                "UIA",                       "Act 63",   2001, categoryIds["Employment"]) { IsProcessed = false },
            new LegalDocument(Guid.NewGuid(), "Equality Act (Promotion of Equality and Prevention of Unfair Discrimination Act)", "PEPUDA", "Act 4", 2000, categoryIds["Criminal"]) { IsProcessed = false },
        };

        _context.LegalDocuments.AddRange(docs);
        _context.SaveChanges();
    }
}
