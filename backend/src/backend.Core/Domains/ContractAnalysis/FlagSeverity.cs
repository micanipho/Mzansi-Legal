namespace backend.Domains.ContractAnalysis;

/// <summary>
/// Indicates the severity of a finding identified during contract analysis.
/// Values are stored as integers in the database for efficiency and compile-time safety.
/// </summary>
public enum FlagSeverity
{
    /// <summary>
    /// A serious concern that could significantly disadvantage or legally expose the user.
    /// The user should seek legal advice before signing.
    /// </summary>
    Red = 0,

    /// <summary>
    /// A cautionary finding that warrants attention and possible negotiation,
    /// but does not necessarily prevent signing.
    /// </summary>
    Amber = 1,

    /// <summary>
    /// A standard clause that is common and generally acceptable.
    /// Flagged for awareness rather than concern.
    /// </summary>
    Green = 2
}
