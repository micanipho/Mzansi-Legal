using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Ardalis.GuardClauses;
using backend.Domains.LegalDocuments;
using backend.Services.EtlPipelineService;
using Castle.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Migrator.Seed;

/// <summary>
/// Phase B seed runner: resolves the ETL pipeline service from the IoC container and
/// triggers ingestion for every unprocessed legislation document.
/// Runs after <c>InitialHostDbBuilder</c> has seeded categories and document stubs.
/// </summary>
public class LegislationIngestionRunner : ITransientDependency
{
    private readonly IIocResolver _iocResolver;
    private readonly IRepository<LegalDocument, Guid> _documentRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    /// <summary>
    /// Initialises the runner with the IoC resolver (for ETL service resolution)
    /// and the document repository (for listing unprocessed documents).
    /// </summary>
    public LegislationIngestionRunner(
        IIocResolver iocResolver,
        IRepository<LegalDocument, Guid> documentRepository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        Guard.Against.Null(iocResolver, nameof(iocResolver));
        Guard.Against.Null(documentRepository, nameof(documentRepository));
        Guard.Against.Null(unitOfWorkManager, nameof(unitOfWorkManager));

        _iocResolver = iocResolver;
        _documentRepository = documentRepository;
        _unitOfWorkManager = unitOfWorkManager;
    }

    /// <summary>
    /// Triggers the ETL pipeline for each document where <c>IsProcessed = false</c>.
    /// Documents whose PDF file is not present on disk are skipped with a warning.
    /// A failure on one document does not abort processing of the remaining documents.
    /// </summary>
    public async Task RunAsync()
    {
        var logger = GetLogger();
        List<PendingDocument> unprocessed;
        using (var uow = _unitOfWorkManager.Begin())
        {
            unprocessed = _documentRepository
                .GetAll()
                .Where(d => !d.IsProcessed)
                .Select(d => new PendingDocument(d.Id, d.Title, d.FileName))
                .ToList();
            uow.Complete();
        }

        if (unprocessed.Count == 0)
        {
            logger.Info("LegislationIngestionRunner: All documents already processed — nothing to do.");
            return;
        }

        logger.Info($"LegislationIngestionRunner: Starting ingestion for {unprocessed.Count} unprocessed document(s).");

        var succeeded = 0;
        var skipped = 0;
        var failed = 0;

        // Resolve ETL service and run as the host admin user (Id = 1).
        // IAbpSession.Use() impersonates a user without a real HTTP context,
        // satisfying the [AbpAuthorize] check on EtlPipelineAppService.
        using var abpSession = _iocResolver.ResolveAsDisposable<IAbpSession>();
        using var etlService = _iocResolver.ResolveAsDisposable<IEtlPipelineAppService>();

        using (abpSession.Object.Use(tenantId: null, userId: 1L))
        {
            foreach (var document in unprocessed)
            {
                var result = await TryIngestDocumentAsync(document, etlService.Object, logger);

                if (result == IngestResult.Succeeded) succeeded++;
                else if (result == IngestResult.Skipped) skipped++;
                else failed++;
            }
        }

        logger.Info(
            $"LegislationIngestionRunner: Seed complete — " +
            $"{succeeded} succeeded, {skipped} skipped (no file), {failed} failed.");
    }

    private async Task<IngestResult> TryIngestDocumentAsync(
        PendingDocument document,
        IEtlPipelineAppService etlService,
        ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(document.FileName))
        {
            logger.Warn(
                $"LegislationIngestionRunner: No filename set for '{document.Title}' — skipping.");
            return IngestResult.Skipped;
        }

        try
        {
            logger.Info($"LegislationIngestionRunner: Starting '{document.Title}'...");

            var job = await etlService.TriggerAsync(document.Id);

            logger.Info(
                $"LegislationIngestionRunner: '{document.Title}' — " +
                $"{job.ChunksLoaded} chunks, {job.EmbeddingsGenerated} embeddings.");

            return IngestResult.Succeeded;
        }
        catch (Exception ex)
        {
            // File-not-found from EtlPipelineAppService surfaces as UserFriendlyException
            // with message "Stored PDF file could not be found." — treat as skip.
            if (ex.Message.Contains("could not be found", StringComparison.OrdinalIgnoreCase))
            {
                logger.Warn(
                    $"LegislationIngestionRunner: PDF file not found for '{document.Title}' " +
                    $"('{document.FileName}') — skipping.");
                return IngestResult.Skipped;
            }

            logger.Warn(
                $"LegislationIngestionRunner: Failed to ingest '{document.Title}' — {ex.Message}");
            return IngestResult.Failed;
        }
    }

    private ILogger GetLogger()
    {
        // Use Castle.Core logger if available; fall back to a no-op logger.
        if (_iocResolver.IsRegistered<ILogger>())
        {
            return _iocResolver.Resolve<ILogger>();
        }

        return NullLogger.Instance;
    }

    private enum IngestResult { Succeeded, Skipped, Failed }

    private sealed record PendingDocument(Guid Id, string Title, string FileName);
}
