using System;
using System.Collections.Generic;

namespace backend.Services.ContractService.DTO;

/// <summary>
/// Full saved contract-analysis result returned after upload and on detail fetches.
/// </summary>
public class ContractAnalysisDto
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

    public List<ContractFlagDto> Flags { get; set; } = new();
}
