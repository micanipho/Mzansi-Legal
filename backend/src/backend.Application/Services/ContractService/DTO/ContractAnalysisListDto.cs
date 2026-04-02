using System.Collections.Generic;

namespace backend.Services.ContractService.DTO;

/// <summary>
/// Owner-scoped collection of saved contract analyses.
/// </summary>
public class ContractAnalysisListDto
{
    public List<ContractAnalysisListItemDto> Items { get; set; } = new();

    public int TotalCount { get; set; }
}
