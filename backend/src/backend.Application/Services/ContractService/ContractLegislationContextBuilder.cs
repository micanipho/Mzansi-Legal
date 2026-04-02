using Abp.Application.Services;
using Abp.Domain.Repositories;
using Ardalis.GuardClauses;
using backend.Domains.ContractAnalysis;
using backend.Domains.LegalDocuments;
using backend.Services.EmbeddingService;
using backend.Services.RagService;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services.ContractService;

/// <summary>
/// Builds grounded legislation context for contract analysis by reusing the current RAG helpers.
/// </summary>
public class ContractLegislationContextBuilder : ApplicationService
{
    private readonly IEmbeddingAppService _embeddingAppService;
    private readonly IRepository<DocumentChunk, Guid> _chunkRepository;
    private readonly RagIndexStore _ragIndexStore;
    private readonly RagSourceHintExtractor _sourceHintExtractor = new();
    private readonly RagDocumentProfileBuilder _documentProfileBuilder = new();
    private readonly RagRetrievalPlanner _retrievalPlanner = new();

    public ContractLegislationContextBuilder(
        IEmbeddingAppService embeddingAppService,
        IRepository<DocumentChunk, Guid> chunkRepository,
        RagIndexStore ragIndexStore)
    {
        _embeddingAppService = embeddingAppService;
        _chunkRepository = chunkRepository;
        _ragIndexStore = ragIndexStore;
    }

    public virtual async Task<ContractLegislationContext> BuildAsync(ContractType contractType, string extractedText)
    {
        Guard.Against.NullOrWhiteSpace(extractedText, nameof(extractedText));

        await EnsureIndexLoadedAsync();

        var queryText = BuildQueryText(contractType, extractedText);
        var embedding = await _embeddingAppService.GenerateEmbeddingAsync(queryText);
        var focusQueryText = RagQueryFocusBuilder.Build(queryText);
        float[] focusVector = null;

        if (!string.IsNullOrWhiteSpace(focusQueryText) &&
            !string.Equals(focusQueryText, queryText, StringComparison.OrdinalIgnoreCase))
        {
            focusVector = (await _embeddingAppService.GenerateEmbeddingAsync(focusQueryText)).Vector;
        }

        var loadedChunks = _ragIndexStore.LoadedChunks;
        var documentProfiles = _ragIndexStore.DocumentProfiles;
        var semanticMatches = _retrievalPlanner.BuildSemanticMatches(
            embedding.Vector,
            loadedChunks,
            focusVector);
        var sourceHints = _sourceHintExtractor.Extract(queryText, loadedChunks);
        var plan = _retrievalPlanner.BuildPlan(
            queryText,
            embedding.Vector,
            semanticMatches,
            sourceHints,
            documentProfiles);

        var selectedChunks = ApplyContractFilters(contractType, plan.SelectedChunks, queryText);
        var primaryChunks = selectedChunks
            .Where(chunk => string.Equals(chunk.SourceRole, RagSourceMetadata.Primary, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var supportingChunks = selectedChunks
            .Where(chunk => !string.Equals(chunk.SourceRole, RagSourceMetadata.Primary, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var (coverageState, coverageNotes) = DetermineCoverage(contractType, queryText, selectedChunks.Count);

        return new ContractLegislationContext(primaryChunks, supportingChunks, coverageState, coverageNotes);
    }

    private async Task EnsureIndexLoadedAsync()
    {
        if (_ragIndexStore.IsReady)
        {
            return;
        }

        var chunks = await _chunkRepository
            .GetAll()
            .Include(chunk => chunk.Embedding)
            .Include(chunk => chunk.Document)
            .ThenInclude(document => document.Category)
            .Where(chunk => chunk.Embedding != null)
            .ToListAsync();

        var indexedChunks = chunks
            .Select(chunk => new IndexedChunk(
                chunk.Id,
                chunk.DocumentId,
                chunk.Document?.Title ?? "Unknown Act",
                chunk.Document?.ShortName ?? string.Empty,
                chunk.Document?.ActNumber ?? string.Empty,
                chunk.Document?.Year ?? 0,
                chunk.Document?.Category?.Name ?? string.Empty,
                chunk.SectionNumber ?? string.Empty,
                chunk.SectionTitle ?? string.Empty,
                chunk.Content ?? string.Empty,
                ParseKeywords(chunk.Keywords),
                chunk.TopicClassification ?? string.Empty,
                chunk.TokenCount,
                chunk.Embedding.Vector))
            .ToList();

        var profiles = _documentProfileBuilder.Build(indexedChunks).ToList();
        _ragIndexStore.Replace(indexedChunks, profiles);
    }

    private static IReadOnlyList<RetrievedChunk> ApplyContractFilters(
        ContractType contractType,
        IReadOnlyList<RetrievedChunk> selectedChunks,
        string queryText)
    {
        var expectedActs = contractType switch
        {
            ContractType.Employment => new[] { "Basic Conditions of Employment", "Labour Relations", "Employment Equity", "BCEA", "LRA" },
            ContractType.Lease => new[] { "Rental Housing", "Constitution" },
            ContractType.Credit => new[] { "National Credit", "Consumer Protection", "NCA", "CPA" },
            _ => new[] { "Consumer Protection", "Constitution", "CPA" }
        };

        var filtered = selectedChunks
            .Where(chunk => expectedActs.Any(act =>
                chunk.ActName.Contains(act, StringComparison.OrdinalIgnoreCase) ||
                chunk.ActShortName.Contains(act, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (filtered.Count > 0)
        {
            return filtered;
        }

        if (contractType == ContractType.Lease &&
            (queryText.Contains("evict", StringComparison.OrdinalIgnoreCase) ||
             queryText.Contains("eviction", StringComparison.OrdinalIgnoreCase)))
        {
            return selectedChunks
                .Where(chunk => chunk.ActName.Contains("Constitution", StringComparison.OrdinalIgnoreCase) ||
                                chunk.ActName.Contains("Rental Housing", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return selectedChunks;
    }

    private static (ContractCoverageState CoverageState, string CoverageNotes) DetermineCoverage(
        ContractType contractType,
        string queryText,
        int selectedChunkCount)
    {
        if (selectedChunkCount == 0)
        {
            return (
                ContractCoverageState.NeedsCorpusExpansion,
                "The current legislation corpus did not return grounded support for this contract issue.");
        }

        if (contractType == ContractType.Lease &&
            (queryText.Contains("evict", StringComparison.OrdinalIgnoreCase) ||
             queryText.Contains("eviction", StringComparison.OrdinalIgnoreCase)))
        {
            return (
                ContractCoverageState.PartialCoverage,
                "Lease analysis is partially grounded here. Basic tenancy rights are covered, but eviction procedure remains incomplete until PIE is ingested.");
        }

        if (contractType == ContractType.Lease || contractType == ContractType.Service)
        {
            return (
                ContractCoverageState.PartialCoverage,
                "The current legislation corpus covers the main baseline issues, but some contract-specific questions still need cautious phrasing.");
        }

        return (
            ContractCoverageState.InCorpusNow,
            "The current legislation corpus contains grounded baseline authority for this contract type.");
    }

    private static string BuildQueryText(ContractType contractType, string extractedText)
    {
        var contractLead = ContractPromptBuilder.ToContractTypeValue(contractType);
        var lines = extractedText
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(20);

        return $"{contractLead} contract {string.Join(' ', lines)}";
    }

    private static List<string> ParseKeywords(string rawKeywords)
    {
        if (string.IsNullOrWhiteSpace(rawKeywords))
        {
            return new List<string>();
        }

        return rawKeywords
            .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(RagSourceHintExtractor.Normalize)
            .Where(keyword => !string.IsNullOrWhiteSpace(keyword))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }
}
