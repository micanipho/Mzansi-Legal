using backend.Services.RagService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Web.Host.Startup
{
    /// <summary>
    /// Hosted service that pre-loads all chunk embeddings into the RAG service's in-memory store
    /// at application startup. Running before the first request is served ensures that
    /// the first call to <see cref="IRagAppService.AskAsync"/> has no cold-start loading delay.
    /// </summary>
    public class RagStartupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RagStartupService> _logger;
        private readonly IHostApplicationLifetime _appLifetime;

        /// <summary>Initialises the startup service with the DI service provider and logger.</summary>
        public RagStartupService(
            IServiceProvider serviceProvider,
            ILogger<RagStartupService> logger,
            IHostApplicationLifetime appLifetime)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _appLifetime = appLifetime;
        }

        /// <summary>
        /// Waits for the application to fully start (ensuring ABP/Windsor has completed its
        /// assembly scanning and component registration via <c>app.UseAbp()</c>) before
        /// resolving <see cref="IRagAppService"/> and calling
        /// <see cref="IRagAppService.InitialiseAsync"/> to populate the in-memory embedding store.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // ABP registers Windsor components during app.UseAbp() in Configure(), which runs
            // as part of GenericWebHostService startup. Waiting for ApplicationStarted guarantees
            // Windsor is fully initialised before we try to resolve IRagAppService.
            var appStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _appLifetime.ApplicationStarted.Register(() => appStarted.TrySetResult());
            await appStarted.Task.WaitAsync(stoppingToken);

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var ragService = scope.ServiceProvider.GetRequiredService<IRagAppService>();
                    await ragService.InitialiseAsync(stoppingToken);
                }

                _logger.LogInformation("[RagService] Chunk embeddings loaded into memory. RAG service is ready.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "[RagService] WARNING: Failed to load chunk embeddings at startup. " +
                    "Ensure the ETL pipeline has been run to populate ChunkEmbeddings. " +
                    "RAG Q&A will return insufficient-information responses until embeddings are available.");
            }
        }
    }
}
