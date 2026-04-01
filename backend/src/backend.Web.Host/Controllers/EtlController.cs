using Abp.Authorization;
using backend.Controllers;
using backend.Services.EtlPipelineService;
using backend.Services.EtlPipelineService.DTO;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Web.Host.Controllers
{
    /// <summary>
    /// Admin-only endpoints for triggering, monitoring, and retrying ETL ingestion jobs.
    /// </summary>
    [Route("api/app/admin/etl")]
    [AbpAuthorize]
    public class EtlController : backendControllerBase
    {
        private readonly IEtlPipelineAppService _etlPipelineAppService;

        /// <summary>Initialises the controller with the ETL orchestration application service.</summary>
        public EtlController(IEtlPipelineAppService etlPipelineAppService)
        {
            _etlPipelineAppService = etlPipelineAppService;
        }

        /// <summary>Triggers ETL processing for the specified legal document.</summary>
        [HttpPost("trigger/{documentId}")]
        public Task<IngestionJobDto> Trigger(Guid documentId)
        {
            return _etlPipelineAppService.TriggerAsync(documentId);
        }

        /// <summary>Returns all ingestion jobs ordered by newest first.</summary>
        [HttpGet("jobs")]
        public Task<List<IngestionJobListDto>> GetJobs()
        {
            return _etlPipelineAppService.GetJobsAsync();
        }

        /// <summary>Returns the full detail of a single ingestion job.</summary>
        [HttpGet("jobs/{id}")]
        public Task<IngestionJobDto> GetJob(Guid id)
        {
            return _etlPipelineAppService.GetJobAsync(id);
        }

        /// <summary>Retries a previously failed ingestion job from scratch.</summary>
        [HttpPost("retry/{jobId}")]
        public Task<IngestionJobDto> Retry(Guid jobId)
        {
            return _etlPipelineAppService.RetryAsync(jobId);
        }
    }
}
