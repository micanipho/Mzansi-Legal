namespace backend.Services.ContractService.DTO;

/// <summary>
/// User-facing representation of a contract finding.
/// </summary>
public class ContractFlagDto
{
    public string Severity { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string ClauseText { get; set; }

    public string LegislationCitation { get; set; }
}
