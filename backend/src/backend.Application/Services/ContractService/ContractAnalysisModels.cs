using backend.Domains.ContractAnalysis;
using backend.Domains.QA;
using backend.Services.RagService;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services.ContractService;

public enum ContractCoverageState
{
    InCorpusNow = 0,
    PartialCoverage = 1,
    NeedsCorpusExpansion = 2
}

public sealed record ContractExtractionResult(
    string ExtractedText,
    string ExtractionMode,
    int CharacterCount);

public sealed record ContractLegislationContext(
    IReadOnlyList<RetrievedChunk> PrimaryChunks,
    IReadOnlyList<RetrievedChunk> SupportingChunks,
    ContractCoverageState CoverageState,
    string CoverageNotes)
{
    public IReadOnlyList<RetrievedChunk> AllChunks => PrimaryChunks.Count == 0
        ? SupportingChunks
        : new List<RetrievedChunk>(PrimaryChunks).Concat(SupportingChunks).ToList();
}

public sealed record ContractAnalysisFlagDraft(
    FlagSeverity Severity,
    string Title,
    string Description,
    string ClauseText,
    string LegislationCitation,
    bool IsGrounded);

public sealed record ContractAnalysisDraft(
    ContractType ContractType,
    int HealthScore,
    string Summary,
    Language Language,
    ContractCoverageState CoverageState,
    IReadOnlyList<ContractAnalysisFlagDraft> Flags);
