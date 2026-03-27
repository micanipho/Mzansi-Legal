using Abp.AspNetCore.Mvc.Authorization;
using backend.MzansiLegal.QnA;
using backend.MzansiLegal.QnA.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace backend.Controllers.MzansiLegal
{
    [Route("api/app/question")]
    [ApiController]
    public class QuestionController : backendControllerBase
    {
        private readonly RagService _ragService;

        public QuestionController(RagService ragService)
        {
            _ragService = ragService;
        }

        /// <summary>
        /// POST /api/app/question/ask
        /// Submit a question in any supported language and receive a cited answer.
        /// </summary>
        [HttpPost("ask")]
        [AbpMvcAuthorize]
        public async Task<QuestionWithAnswerDto> AskAsync([FromBody] AskQuestionInput input)
        {
            var userId = AbpSession.UserId
                ?? throw new UnauthorizedAccessException("User must be logged in to ask questions.");

            return await _ragService.AskAsync(userId, input);
        }
    }
}
