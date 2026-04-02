#nullable enable
using Abp;
using Abp.Configuration;
using Abp.Runtime.Session;
using backend.Configuration;
using backend.Services.RightsExplorerService;
using backend.Services.RightsExplorerService.DTO;
using NSubstitute;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests.RightsExplorerServiceTests;

public class RightsAcademyProgressAppServiceTests
{
    [Fact]
    public async Task GetProgressAsync_ReturnsNormalizedExploredLessonIds()
    {
        var session = CreateSession();
        var settingManager = Substitute.For<ISettingManager>();
        settingManager
            .GetSettingValueForUserAsync(AppSettingNames.RightsAcademyProgress, Arg.Any<UserIdentifier>())
            .Returns("[\"academy-one\",\"academy-two\",\"academy-one\",\" \"]");

        var service = new RightsAcademyProgressAppService
        {
            AbpSession = session,
            SettingManager = settingManager
        };

        var result = await service.GetProgressAsync();

        result.ExploredLessonIds.ShouldBe(["academy-one", "academy-two"]);
    }

    [Fact]
    public async Task UpdateProgressAsync_PersistsNormalizedExploredLessonIds()
    {
        var session = CreateSession();
        var settingManager = Substitute.For<ISettingManager>();

        var service = new RightsAcademyProgressAppService
        {
            AbpSession = session,
            SettingManager = settingManager
        };

        var result = await service.UpdateProgressAsync(new UpdateRightsAcademyProgressInput
        {
            ExploredLessonIds = ["academy-one", " academy-two ", "academy-one", ""]
        });

        result.ExploredLessonIds.ShouldBe(["academy-one", "academy-two"]);
        await settingManager.Received(1).ChangeSettingForUserAsync(
            Arg.Any<UserIdentifier>(),
            AppSettingNames.RightsAcademyProgress,
            "[\"academy-one\",\"academy-two\"]");
    }

    private static IAbpSession CreateSession()
    {
        var session = Substitute.For<IAbpSession>();
        session.UserId.Returns(42);
        session.TenantId.Returns(1);
        return session;
    }
}
