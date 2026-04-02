using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Ardalis.GuardClauses;
using backend.Domains.ContractAnalysis;
using backend.Services.ContractService.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services.ContractService;

/// <summary>
/// Authenticated application-service surface for contract analysis and owner-scoped retrieval.
/// </summary>
[AbpAuthorize]
public class ContractAppService : ApplicationService, IContractAppService
{
    private readonly IRepository<ContractAnalysis, Guid> _contractAnalysisRepository;
    private readonly IRepository<ContractFlag, Guid> _contractFlagRepository;
    private readonly ContractAnalysisService _contractAnalysisService;

    public ContractAppService(
        IRepository<ContractAnalysis, Guid> contractAnalysisRepository,
        IRepository<ContractFlag, Guid> contractFlagRepository,
        ContractAnalysisService contractAnalysisService)
    {
        _contractAnalysisRepository = contractAnalysisRepository;
        _contractFlagRepository = contractFlagRepository;
        _contractAnalysisService = contractAnalysisService;
    }

    public async Task<ContractAnalysisDto> AnalyseAsync(AnalyseContractRequest request)
    {
        Guard.Against.Null(request, nameof(request));

        var userId = AbpSession.UserId
            ?? throw new AbpAuthorizationException("You must be signed in to analyse a contract.");
        var responseLanguage = ContractAnalysisService.ParseLanguageCode(request.ResponseLanguageCode);

        var (analysisDraft, extractedText) = await _contractAnalysisService.AnalyseAsync(
            request.FileBytes,
            request.FileName,
            request.ContentType,
            responseLanguage);

        var entity = new ContractAnalysis
        {
            UserId = userId,
            OriginalFileId = null,
            ExtractedText = extractedText,
            ContractType = analysisDraft.ContractType,
            HealthScore = analysisDraft.HealthScore,
            Summary = analysisDraft.Summary,
            Language = analysisDraft.Language,
            AnalysedAt = DateTime.UtcNow,
            Flags = new List<ContractFlag>()
        };

        var analysisId = await _contractAnalysisRepository.InsertAndGetIdAsync(entity);

        for (var index = 0; index < analysisDraft.Flags.Count; index++)
        {
            var flag = analysisDraft.Flags[index];
            var persistedFlag = new ContractFlag
            {
                ContractAnalysisId = analysisId,
                Severity = flag.Severity,
                Title = flag.Title,
                Description = flag.Description,
                ClauseText = flag.ClauseText,
                LegislationCitation = flag.LegislationCitation,
                SortOrder = index
            };

            await _contractFlagRepository.InsertAsync(persistedFlag);
            entity.Flags.Add(persistedFlag);
        }

        entity.Id = analysisId;
        return MapToDetailDto(entity, request.FileName);
    }

    public async Task<ContractAnalysisListDto> GetMyAsync()
    {
        var userId = AbpSession.UserId
            ?? throw new AbpAuthorizationException("You must be signed in to view contract analyses.");

        var analyses = _contractAnalysisRepository
            .GetAll()
            .Include(analysis => analysis.Flags)
            .Where(analysis => analysis.UserId == userId)
            .OrderByDescending(analysis => analysis.AnalysedAt);

        var materializedAnalyses = await ListAnalysesAsync(analyses);

        var items = materializedAnalyses
            .Select(MapToListItemDto)
            .ToList();

        return new ContractAnalysisListDto
        {
            Items = items,
            TotalCount = items.Count
        };
    }

    public async Task<ContractAnalysisDto> GetAsync(Guid id)
    {
        var analysis = await GetOwnedAnalysisAsync(id);
        return MapToDetailDto(analysis);
    }

    protected virtual async Task<ContractAnalysis> GetOwnedAnalysisAsync(Guid id)
    {
        var userId = AbpSession.UserId
            ?? throw new AbpAuthorizationException("You must be signed in to view contract analyses.");

        var analysis = _contractAnalysisRepository
            .GetAll()
            .Include(item => item.Flags)
            .Where(item => item.Id == id && item.UserId == userId);

        var ownedAnalysis = await FirstOrDefaultOwnedAnalysisAsync(analysis);
        return ownedAnalysis ?? throw new UserFriendlyException("Contract analysis not found.");
    }

    protected virtual Task<List<ContractAnalysis>> ListAnalysesAsync(IQueryable<ContractAnalysis> query)
    {
        return query.ToListAsync();
    }

    protected virtual Task<ContractAnalysis> FirstOrDefaultOwnedAnalysisAsync(IQueryable<ContractAnalysis> query)
    {
        return query.FirstOrDefaultAsync();
    }

    internal static ContractAnalysisDto MapToDetailDto(ContractAnalysis analysis, string fileName = null)
    {
        Guard.Against.Null(analysis, nameof(analysis));

        var orderedFlags = (analysis.Flags ?? Array.Empty<ContractFlag>())
            .OrderBy(flag => flag.SortOrder)
            .ThenBy(flag => flag.CreationTime)
            .ToList();

        return new ContractAnalysisDto
        {
            Id = analysis.Id,
            DisplayTitle = ContractPromptBuilder.BuildDisplayTitle(fileName, analysis.ExtractedText, analysis.ContractType),
            ContractType = ContractPromptBuilder.ToContractTypeValue(analysis.ContractType),
            HealthScore = analysis.HealthScore,
            Summary = analysis.Summary,
            Language = ContractPromptBuilder.ToLanguageCode(analysis.Language),
            AnalysedAt = analysis.AnalysedAt,
            RedFlagCount = orderedFlags.Count(flag => flag.Severity == FlagSeverity.Red),
            AmberFlagCount = orderedFlags.Count(flag => flag.Severity == FlagSeverity.Amber),
            GreenFlagCount = orderedFlags.Count(flag => flag.Severity == FlagSeverity.Green),
            Flags = orderedFlags
                .Select(flag => new ContractFlagDto
                {
                    Severity = ContractPromptBuilder.ToSeverityValue(flag.Severity),
                    Title = flag.Title,
                    Description = flag.Description,
                    ClauseText = flag.ClauseText,
                    LegislationCitation = flag.LegislationCitation
                })
                .ToList()
        };
    }

    internal static ContractAnalysisListItemDto MapToListItemDto(ContractAnalysis analysis)
    {
        Guard.Against.Null(analysis, nameof(analysis));

        var flags = analysis.Flags ?? Array.Empty<ContractFlag>();
        return new ContractAnalysisListItemDto
        {
            Id = analysis.Id,
            DisplayTitle = ContractPromptBuilder.BuildDisplayTitle(null, analysis.ExtractedText, analysis.ContractType),
            ContractType = ContractPromptBuilder.ToContractTypeValue(analysis.ContractType),
            HealthScore = analysis.HealthScore,
            Summary = analysis.Summary,
            Language = ContractPromptBuilder.ToLanguageCode(analysis.Language),
            AnalysedAt = analysis.AnalysedAt,
            RedFlagCount = flags.Count(flag => flag.Severity == FlagSeverity.Red),
            AmberFlagCount = flags.Count(flag => flag.Severity == FlagSeverity.Amber),
            GreenFlagCount = flags.Count(flag => flag.Severity == FlagSeverity.Green)
        };
    }
}
