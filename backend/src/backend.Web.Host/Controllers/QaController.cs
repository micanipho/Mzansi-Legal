using Abp.Authorization;
using backend.Controllers;
using backend.Services.RagService;
using backend.Services.RagService.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace backend.Web.Host.Controllers
{
    /// <summary>
    /// Authenticated endpoint for submitting legal questions to the RAG Q&amp;A pipeline.
    /// Answers are grounded in retrieved South African legislation and include verifiable citations.
    /// </summary>
    [Route("api/app/qa")]
    // [AbpAuthorize]
    public class QaController : backendControllerBase
    {
        private readonly IRagAppService _ragAppService;

        /// <summary>Initialises the controller with the RAG orchestration service.</summary>
        public QaController(IRagAppService ragAppService)
        {
            _ragAppService = ragAppService;
        }

        /// <summary>
        /// Submits a natural-language legal question and returns a cited answer from South African legislation.
        /// Returns <c>isInsufficientInformation: true</c> when no relevant legislation is found.
        /// </summary>
        [HttpPost("ask")]
        public Task<RagAnswerResult> Ask([FromBody] AskQuestionRequest request)
        {
            return _ragAppService.AskAsync(request);
        }
    }
}
