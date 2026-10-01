using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Dashboard.Dashboard;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.UI.Navigation;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Volo.Abp.Users;
using Xunit;
using IAuthorizationService = Microsoft.AspNetCore.Authorization.IAuthorizationService;

namespace SufiChain.SufiPlatform.Dashboard.Dashboard;

public class DashboardShortcutWorkflowTests
{
    [Fact]
    public async Task AddShortcut_Should_Pin_An_Authorized_Panel_Item()
    {
        var userId = Guid.NewGuid();
        var service = NewService(Menu(), userId, out var store);

        var shortcut = await service.AddShortcutAsync(new AddShortcutInput { MenuKey = "home" });

        shortcut.MenuKey.ShouldBe("home");
        shortcut.Url.ShouldBe("/panel/home");
        await store.Received().SetAsync(
            Arg.Any<string>(),
            Arg.Is<string>(json => json.Contains("home")),
            Arg.Any<string>(),
            userId.ToString());
    }

    [Fact]
    public async Task AddShortcut_Should_Reject_A_Menu_That_Is_Not_On_The_Panel()
    {
        var service = NewService(Menu(), Guid.NewGuid(), out _);
        var error = await Should.ThrowAsync<BusinessException>(() =>
            service.AddShortcutAsync(new AddShortcutInput { MenuKey = "missing" }));
        error.Code.ShouldBe("SufiDashboard:ShortcutNotAvailable");
    }

    [Fact]
    public async Task Reorder_Should_Reject_An_Anonymous_User()
    {
        var service = NewService(Menu(), null, out _);
        var error = await Should.ThrowAsync<BusinessException>(() =>
            service.ReorderShortcutsAsync(new ReorderShortcutsInput()));
        error.Code.ShouldBe("SufiDashboard:NotAuthenticated");
    }

    private static DashboardShortcutAppService NewService(
        ApplicationMenu menu,
        Guid? userId,
        out ISettingsStore store)
    {
        var menus = Substitute.For<IMenuManager>();
        menus.GetAsync(StandardMenus.Main).Returns(menu);
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        store = Substitute.For<ISettingsStore>();
        var service = new DashboardShortcutAppService(
            menus,
            settings,
            store,
            Substitute.For<IAuthorizationService>());
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(userId);
        var services = new ServiceCollection();
        services.AddSingleton(currentUser);
        service.LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());
        return service;
    }

    private static ApplicationMenu Menu()
    {
        var menu = new ApplicationMenu(StandardMenus.Main);
        menu.AddItem(new ApplicationMenuItem("home", "Home", "/panel/home"));
        return menu;
    }
}
