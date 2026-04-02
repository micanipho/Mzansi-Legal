using Abp.Authorization;
using Abp.UI;
using backend.Controllers;
using backend.Services.ContractService;
using backend.Services.ContractService.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace backend.Web.Host.Controllers
{
    /// <summary>
    /// Authenticated contract-analysis endpoints for upload, history, and detail retrieval.
    /// </summary>
    [Route("api/app/contract")]
    [AbpAuthorize]
    public class ContractController : backendControllerBase
    {
        private readonly IContractAppService _contractAppService;

        public ContractController(IContractAppService contractAppService)
        {
            _contractAppService = contractAppService;
        }

        /// <summary>
        /// Uploads and analyzes a supported PDF contract for the current user.
        /// </summary>
        [HttpPost("analyse")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<ContractAnalysisDto> Analyse([FromForm] AnalyseContractForm request)
        {
            if (request?.File == null)
            {
                throw new UserFriendlyException("Please choose a PDF contract to upload.");
            }

            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await request.File.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            var languageCode = string.IsNullOrWhiteSpace(request.ResponseLanguageCode)
                ? GetLanguageCodeFromRequest()
                : request.ResponseLanguageCode;

            return await _contractAppService.AnalyseAsync(new AnalyseContractRequest
            {
                FileName = request.File.FileName,
                ContentType = request.File.ContentType,
                FileBytes = fileBytes,
                ResponseLanguageCode = languageCode
            });
        }

        /// <summary>
        /// Returns the current user's saved analyses, newest first.
        /// </summary>
        [HttpGet("my")]
        public Task<ContractAnalysisListDto> GetMy()
        {
            return _contractAppService.GetMyAsync();
        }

        /// <summary>
        /// Returns a single saved analysis owned by the current user.
        /// </summary>
        [HttpGet("{id}")]
        public Task<ContractAnalysisDto> Get(Guid id)
        {
            return _contractAppService.GetAsync(id);
        }

        /// <summary>
        /// Asks a follow-up question about a saved contract analysis owned by the current user.
        /// </summary>
        [HttpPost("{id}/ask")]
        public Task<ContractFollowUpAnswerDto> Ask(Guid id, [FromBody] AskContractQuestionRequest request)
        {
            return _contractAppService.AskAsync(id, request);
        }

        private string GetLanguageCodeFromRequest()
        {
            var raw = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            var primary = raw.Split(',')[0].Trim();
            if (primary.Contains("-"))
            {
                primary = primary.Split('-')[0];
            }

            return primary;
        }

        public class AnalyseContractForm
        {
            public IFormFile File { get; set; }

            public string ResponseLanguageCode { get; set; }
        }
    }
}
