using Abp.Application.Services;
using backend.Services.FaqService.DTO;
using System;
using System.Threading.Tasks;

namespace backend.Services.FaqService;

/// <summary>
/// Exposes curated public FAQ content for discovery surfaces such as the My Rights explorer.
/// </summary>
public interface IPublicFaqAppService : IApplicationService
{
    /// <summary>
    /// Returns approved public FAQ entries, optionally filtered by category and biased toward
    /// a requested language when the same FAQ exists in multiple languages.
    /// </summary>
    Task<PublicFaqListDto> GetPublicFaqsAsync(Guid? categoryId = null, string languageCode = null);
}
