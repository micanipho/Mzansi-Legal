using System;

namespace backend.Services.ContractService.DTO;

/// <summary>
/// Summary projection for the contracts history list.
/// </summary>
public class ContractAnalysisListItemDto
{
    public Guid Id { get; set; }

    public string DisplayTitle { get; set; }

    public string ContractType { get; set; }

    public int HealthScore { get; set; }

    public string Summary { get; set; }

    public string Language { get; set; }

    public DateTime AnalysedAt { get; set; }

    public int RedFlagCount { get; set; }

    public int AmberFlagCount { get; set; }

    public int GreenFlagCount { get; set; }
}
