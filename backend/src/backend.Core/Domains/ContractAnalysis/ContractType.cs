namespace backend.Domains.ContractAnalysis;

/// <summary>
/// Classifies the type of legal contract being analysed.
/// Values are stored as integers in the database for efficiency and compile-time safety.
/// </summary>
public enum ContractType
{
    /// <summary>An employment agreement between an employer and an employee.</summary>
    Employment = 0,

    /// <summary>A lease agreement for property rental (residential or commercial).</summary>
    Lease = 1,

    /// <summary>A credit or loan agreement between a lender and a borrower.</summary>
    Credit = 2,

    /// <summary>A service agreement between a service provider and a client.</summary>
    Service = 3
}
