using backend.Domains.LegalDocuments;
using System.Collections.Generic;

namespace backend.EntityFrameworkCore.Seed.Host;

/// <summary>
/// Central manifest of all pre-seeded legislation categories and documents.
/// This is the single source of truth for category names, sort orders, and document metadata.
/// All seeder classes reference these constants — do not hardcode names or filenames elsewhere.
/// </summary>
public static class LegislationManifest
{
    /// <summary>Defines a pre-seeded legislation category.</summary>
    public sealed record CategoryDefinition(
        string Name,
        DocumentDomain Domain,
        string Icon,
        int SortOrder);

    /// <summary>Defines a pre-seeded legislation document stub.</summary>
    public sealed record DocumentDefinition(
        string Title,
        string ShortName,
        string ActNumber,
        int Year,
        string FileName,
        string CategoryName);

    // ── Category name constants — used in both Categories and Documents lists ──

    private const string CatEmploymentLabour   = "Employment & Labour";
    private const string CatHousingEviction    = "Housing & Eviction";
    private const string CatConsumerRights     = "Consumer Rights";
    private const string CatDebtCredit         = "Debt & Credit";
    private const string CatPrivacyData        = "Privacy & Data";
    private const string CatSafetyHarassment   = "Safety & Harassment";
    private const string CatInsuranceRetirement = "Insurance & Retirement";
    private const string CatTax                = "Tax";
    private const string CatContractAnalysis   = "Contract Analysis";

    /// <summary>The 9 categories pre-loaded into the system, in display order.</summary>
    public static readonly IReadOnlyList<CategoryDefinition> Categories = new[]
    {
        new CategoryDefinition(CatEmploymentLabour,    DocumentDomain.Legal,     "briefcase",      1),
        new CategoryDefinition(CatHousingEviction,     DocumentDomain.Legal,     "home",           2),
        new CategoryDefinition(CatConsumerRights,      DocumentDomain.Legal,     "shield",         3),
        new CategoryDefinition(CatDebtCredit,          DocumentDomain.Legal,     "credit-card",    4),
        new CategoryDefinition(CatPrivacyData,         DocumentDomain.Legal,     "lock",           5),
        new CategoryDefinition(CatSafetyHarassment,    DocumentDomain.Legal,     "alert-triangle", 6),
        new CategoryDefinition(CatInsuranceRetirement, DocumentDomain.Financial, "umbrella",       7),
        new CategoryDefinition(CatTax,                 DocumentDomain.Financial, "file-text",      8),
        new CategoryDefinition(CatContractAnalysis,    DocumentDomain.Legal,     "clipboard",      9),
    };

    /// <summary>The 13 legislation documents pre-registered in the system.</summary>
    public static readonly IReadOnlyList<DocumentDefinition> Documents = new[]
    {
        // ── Legal domain ──────────────────────────────────────────────────────
        new DocumentDefinition(
            "Constitution of the Republic of South Africa",
            "Constitution", "108", 1996,
            "constitution-1996.pdf",
            CatContractAnalysis),

        new DocumentDefinition(
            "Basic Conditions of Employment Act",
            "BCEA", "75", 1997,
            "bcea-1997.pdf",
            CatEmploymentLabour),

        new DocumentDefinition(
            "Consumer Protection Act",
            "CPA", "68", 2008,
            "cpa-2008.pdf",
            CatConsumerRights),

        new DocumentDefinition(
            "Labour Relations Act",
            "LRA", "66", 1995,
            "lra-1995.pdf",
            CatEmploymentLabour),

        new DocumentDefinition(
            "Protection of Personal Information Act",
            "POPIA", "4", 2013,
            "popia-2013.pdf",
            CatPrivacyData),

        new DocumentDefinition(
            "Rental Housing Act",
            "RHA", "50", 1999,
            "rental-housing-act-1999.pdf",
            CatHousingEviction),

        new DocumentDefinition(
            "Protection from Harassment Act",
            "PHA", "17", 2011,
            "protection-harassment-act-2011.pdf",
            CatSafetyHarassment),

        new DocumentDefinition(
            "National Credit Act",
            "NCA", "34", 2005,
            "nca-2005.pdf",
            CatDebtCredit),

        // ── Financial domain ──────────────────────────────────────────────────
        new DocumentDefinition(
            "Financial Advisory and Intermediary Services Act",
            "FAIS", "37", 2002,
            "fais-2002.pdf",
            CatInsuranceRetirement),

        new DocumentDefinition(
            "Tax Administration Act",
            "TAA", "28", 2011,
            "tax-admin-act-2011.pdf",
            CatTax),

        new DocumentDefinition(
            "Pension Funds Act",
            "PFA", "24", 1956,
            "pension-funds-act-1956.pdf",
            CatInsuranceRetirement),

        new DocumentDefinition(
            "SARS Tax Guide",
            "SARS Guide", "N/A", 2024,
            "sars-tax-guide-2024.pdf",
            CatTax),

        new DocumentDefinition(
            "FSCA Regulatory Framework",
            "FSCA Materials", "N/A", 2024,
            "fsca-regulatory-2024.pdf",
            CatInsuranceRetirement),
    };
}
