using Abp.Authorization;
using backend.Controllers;
using backend.Services.RagService;
using backend.Services.RagService.DTO;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace backend.Web.Host.Controllers
{
    /// <summary>
    /// Endpoints for the RAG Q&amp;A pipeline and conversation history.
    /// </summary>
    [Route("api/app/qa")]
    public class QaController : backendControllerBase
    {
        private readonly IRagAppService _ragAppService;

        /// <summary>Initialises the controller with the RAG orchestration service.</summary>
        public QaController(IRagAppService ragAppService)
        {
            _ragAppService = ragAppService;
        }

        /// <summary>
        /// Submits a natural-language legal question in English, isiZulu, Sesotho, or Afrikaans
        /// and returns one of four structured outcomes: a direct grounded answer, a cautious
        /// grounded answer, a clarification request, or an insufficient-grounding response.
        /// Non-English input is translated to English for retrieval, while the answer is directed
        /// back into the user's language. Responses may also label supporting guidance separately
        /// from binding law and flag urgent-attention scenarios for the client.
        /// </summary>
        [HttpPost("ask")]
        public Task<RagAnswerResult> Ask([FromBody] AskQuestionRequest request)
        {
            return _ragAppService.AskAsync(request);
        }

        /// <summary>
        /// Returns the authenticated user's conversation history, newest first.
        /// Each item includes the conversation ID that clients can send back on follow-up questions,
        /// along with the first question and total question count.
        /// </summary>
        [HttpGet("conversations")]
        [AbpAuthorize]
        public Task<ConversationsListDto> GetConversations()
        {
            return _ragAppService.GetConversationsAsync();
        }

        /// <summary>
        /// Returns one authenticated user's full stored conversation thread, including user messages
        /// and persisted assistant replies, so clients can render or resume the conversation.
        /// </summary>
        [HttpGet("conversations/{conversationId:guid}")]
        [AbpAuthorize]
        public Task<ConversationDetailDto> GetConversation(Guid conversationId)
        {
            return _ragAppService.GetConversationAsync(conversationId);
        }
    }
}
