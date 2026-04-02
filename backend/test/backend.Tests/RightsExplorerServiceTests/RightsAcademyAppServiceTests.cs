#nullable enable
using Abp.Configuration;
using backend.Configuration;
using backend.Services.RightsExplorerService;
using NSubstitute;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests.RightsExplorerServiceTests;

public class RightsAcademyAppServiceTests
{
    [Fact]
    public async Task GetAcademyAsync_ReturnsSeededCatalogFromApplicationSetting()
    {
        var json = """
        {
          "tracks": [
            {
              "topicKey": "housing",
              "categoryName": "Housing & Eviction",
              "sortOrder": 2,
              "lessons": [
                {
                  "id": "academy-housing-lease",
                  "documentId": "33333333-3333-3333-3333-333333333332",
                  "topicKey": "housing",
                  "categoryName": "Housing & Eviction",
                  "title": "Lease basics",
                  "lawShortName": "RHA",
                  "lawTitle": "Rental Housing Act",
                  "summary": "Lease protections",
                  "explanation": "Lease protections explained",
                  "sourceQuote": "Quote",
                  "primaryCitation": "Rental Housing Act, section 5",
                  "askQuery": "Explain lease basics",
                  "citations": []
                }
              ]
            },
            {
              "topicKey": "employment",
              "categoryName": "Employment & Labour",
              "sortOrder": 1,
              "lessons": [
                {
                  "id": "academy-employment-terms",
                  "documentId": "22222222-2222-2222-2222-222222222221",
                  "topicKey": "employment",
                  "categoryName": "Employment & Labour",
                  "title": "Written terms",
                  "lawShortName": "BCEA",
                  "lawTitle": "Basic Conditions of Employment Act",
                  "summary": "Written terms matter",
                  "explanation": "Written terms explained",
                  "sourceQuote": "Quote",
                  "primaryCitation": "Basic Conditions of Employment Act, sections 29 and 33",
                  "askQuery": "Explain written terms",
                  "citations": []
                }
              ]
            }
          ],
          "totalLessons": 99
        }
        """;

        var settingManager = Substitute.For<ISettingManager>();
        settingManager.GetSettingValueForApplicationAsync(AppSettingNames.RightsAcademyCatalog).Returns(json);

        var service = new RightsAcademyAppService
        {
            SettingManager = settingManager
        };

        var result = await service.GetAcademyAsync();

        result.TotalLessons.ShouldBe(2);
        result.Tracks.Count.ShouldBe(2);
        result.Tracks[0].TopicKey.ShouldBe("employment");
        result.Tracks[1].TopicKey.ShouldBe("housing");
    }

    [Fact]
    public async Task GetAcademyAsync_FallsBackToGeneratedSeedCatalogWhenSettingIsEmpty()
    {
        var settingManager = Substitute.For<ISettingManager>();
        settingManager.GetSettingValueForApplicationAsync(AppSettingNames.RightsAcademyCatalog).Returns(string.Empty);

        var service = new RightsAcademyAppService
        {
            SettingManager = settingManager
        };

        var result = await service.GetAcademyAsync();

        result.TotalLessons.ShouldBeGreaterThan(0);
        result.Tracks.ShouldNotBeEmpty();
        result.Tracks.Select(track => track.TopicKey).ShouldContain("employment");
        result.Tracks.Select(track => track.TopicKey).ShouldContain("housing");
    }
}
