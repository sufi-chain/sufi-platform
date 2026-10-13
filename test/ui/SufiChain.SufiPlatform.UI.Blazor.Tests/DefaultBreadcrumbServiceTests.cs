using System.Globalization;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Services.Layout;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests;

public class DefaultBreadcrumbServiceTests
{
    [Fact]
    public async Task Matched_menu_item_returns_localized_display_name()
    {
        var menuManager = Substitute.For<IMenuManager>();
        menuManager.GetMainMenuAsync().Returns(BuildMenu("Editions"));

        var service = new DefaultBreadcrumbService(menuManager);
        var crumbs = await service.GetBreadcrumbsForUrlAsync("/panel/admin/editions");

        crumbs.ShouldHaveSingleItem().Text.ShouldBe("Editions");
    }

    [Fact]
    public async Task Matched_menu_item_under_fa_returns_persian_display_name()
    {
        var menuManager = Substitute.For<IMenuManager>();
        menuManager.GetMainMenuAsync().Returns(BuildMenu("نسخه‌ها"));

        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fa");
            var service = new DefaultBreadcrumbService(menuManager);
            var crumbs = await service.GetBreadcrumbsForUrlAsync("/panel/admin/editions");
            crumbs.ShouldHaveSingleItem().Text.ShouldBe("نسخه‌ها");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public async Task Unknown_url_returns_no_editions_segment()
    {
        var menuManager = Substitute.For<IMenuManager>();
        menuManager.GetMainMenuAsync().Returns(BuildMenu("Editions"));

        var service = new DefaultBreadcrumbService(menuManager);
        var crumbs = await service.GetBreadcrumbsForUrlAsync("/panel/admin/unknown-page");

        crumbs.ShouldBeEmpty();
        crumbs.ShouldNotContain(item => string.Equals(item.Text, "editions", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Switching_culture_on_same_instance_reloads_menu_display_names()
    {
        var menuManager = Substitute.For<IMenuManager>();
        menuManager.GetMainMenuAsync()
            .Returns(
                _ => BuildMenu("Editions"),
                _ => BuildMenu("نسخه‌ها"));

        var service = new DefaultBreadcrumbService(menuManager);
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
            var en = await service.GetBreadcrumbsForUrlAsync("/panel/admin/editions");
            en.ShouldHaveSingleItem().Text.ShouldBe("Editions");

            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fa");
            var fa = await service.GetBreadcrumbsForUrlAsync("/panel/admin/editions");
            fa.ShouldHaveSingleItem().Text.ShouldBe("نسخه‌ها");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }

        await menuManager.Received(2).GetMainMenuAsync();
    }

    private static ApplicationMenu BuildMenu(string editionsDisplayName)
    {
        var menu = new ApplicationMenu("Main");
        menu.AddItem(new ApplicationMenuItem(
            "SufiEditions.Editions",
            editionsDisplayName,
            url: "/panel/admin/editions"));
        return menu;
    }
}
