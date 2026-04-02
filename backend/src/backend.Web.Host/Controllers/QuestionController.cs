using backend.Controllers;
using backend.Services.FaqService;
using backend.Services.FaqService.DTO;
using backend.Services.RightsExplorerService;
using backend.Services.RightsExplorerService.DTO;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace backend.Web.Host.Controllers
{
    /// <summary>
    /// Public question-discovery endpoints backed by curated FAQ content.
    /// </summary>
    [Route("api/app/question")]
    public class QuestionController : backendControllerBase
    {
        private readonly IPublicFaqAppService _publicFaqAppService;
        private readonly IRightsAcademyAppService _rightsAcademyAppService;
        private readonly IRightsAcademyProgressAppService _rightsAcademyProgressAppService;

        public QuestionController(
            IPublicFaqAppService publicFaqAppService,
            IRightsAcademyAppService rightsAcademyAppService,
            IRightsAcademyProgressAppService rightsAcademyProgressAppService)
        {
            _publicFaqAppService = publicFaqAppService;
            _rightsAcademyAppService = rightsAcademyAppService;
            _rightsAcademyProgressAppService = rightsAcademyProgressAppService;
        }

        /// <summary>
        /// Returns curated public FAQ entries for discovery surfaces such as the rights explorer.
        /// </summary>
        [HttpGet("faqs")]
        public Task<PublicFaqListDto> GetFaqs(Guid? categoryId = null, string languageCode = null)
        {
            return _publicFaqAppService.GetPublicFaqsAsync(categoryId, languageCode);
        }

        /// <summary>
        /// Returns the legislation-backed rights academy used by the learning view of the rights explorer.
        /// </summary>
        [HttpGet("academy")]
        public Task<RightsAcademyDto> GetAcademy()
        {
            return _rightsAcademyAppService.GetAcademyAsync();
        }

        /// <summary>
        /// Returns the signed-in user's academy lesson progress.
        /// </summary>
        [AbpAuthorize]
        [HttpGet("academy-progress")]
        public Task<RightsAcademyProgressDto> GetAcademyProgress()
        {
            return _rightsAcademyProgressAppService.GetProgressAsync();
        }

        /// <summary>
        /// Persists the signed-in user's academy lesson progress.
        /// </summary>
        [AbpAuthorize]
        [HttpPut("academy-progress")]
        public Task<RightsAcademyProgressDto> UpdateAcademyProgress([FromBody] UpdateRightsAcademyProgressInput input)
        {
            return _rightsAcademyProgressAppService.UpdateProgressAsync(input);
        }
    }
}
