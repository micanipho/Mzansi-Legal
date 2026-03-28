namespace backend.Domains.LegalDocuments;

/// <summary>
/// Classifies a category as belonging to the legal or financial domain.
/// Used as a RefList enum stored as an integer column in the database.
/// </summary>
public enum DocumentDomain
{
    /// <summary>Legal legislation domain (e.g., labour law, family law).</summary>
    Legal = 1,

    /// <summary>Financial regulation domain (e.g., tax, banking).</summary>
    Financial = 2
}
