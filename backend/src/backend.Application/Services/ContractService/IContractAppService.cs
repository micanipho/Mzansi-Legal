using Abp.Application.Services;
using backend.Services.ContractService.DTO;
using System;
using System.Threading.Tasks;

namespace backend.Services.ContractService;

/// <summary>
/// Orchestrates authenticated contract upload, analysis persistence, and owner-scoped retrieval.
/// </summary>
public interface IContractAppService : IApplicationService
{
    Task<ContractAnalysisDto> AnalyseAsync(AnalyseContractRequest request);

    Task<ContractAnalysisListDto> GetMyAsync();

    Task<ContractAnalysisDto> GetAsync(Guid id);
}
