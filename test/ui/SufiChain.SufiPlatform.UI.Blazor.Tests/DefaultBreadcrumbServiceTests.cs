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

    [Fact]
    public async Task DefaultBreadcrumbService_GroupAndChildSameName_CollapsedToOne()
    {
        var menu = new ApplicationMenu("Main");
        var group = new ApplicationMenuItem("Hooshvares", "هوشواره‌ها", url: "/panel/admin/ai");
        group.AddItem(new ApplicationMenuItem("HooshvareList", "هوشواره‌ها", url: "/panel/admin/ai/hooshvare"));
        menu.AddItem(group);

        var service = new DefaultBreadcrumbService(Menu(menu));
        var crumbs = await service.GetBreadcrumbsForUrlAsync("/panel/admin/ai/hooshvare");
        crumbs.Count.ShouldBe(1);
        crumbs[0].Text.ShouldBe("هوشواره‌ها");
    }

    [Fact]
    public async Task DefaultBreadcrumbService_DetailUrl_ListCrumbIsLinked()
    {
        var menu = new ApplicationMenu("Main");
        menu.AddItem(new ApplicationMenuItem("Tickets", "تیکت‌ها", url: "/panel/admin/helpdesk/tickets"));
        var service = new DefaultBreadcrumbService(Menu(menu));
        var crumbs = await service.GetBreadcrumbsForUrlAsync("/panel/admin/helpdesk/tickets/hd-1");
        crumbs.ShouldHaveSingleItem().Url.ShouldBe("/panel/admin/helpdesk/tickets");
    }

    [Fact]
    public void Layout_NavigateToUnmatchedUrl_ClearsOldTrail()
    {
        var layout = File.ReadAllText(Find("independent-projects/sufi-theme/src/SufiChain.SufiTheme.Blazor/Layouts/SufiLayoutBase.cs"));
        var start = layout.IndexOf("private void OnLocationChanged", StringComparison.Ordinal);
        var block = layout[start..(start + 500)];
        block.ShouldContain("BreadcrumbItems.Clear()");
    }

    [Fact]
    public void TicketDetail_SetsEntityCrumbAndTitle_NumberAndSubject()
    {
        var source = File.ReadAllText(Find("pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.Ticketing.Blazor/Pages/Tickets/TicketDetail.razor.cs"));
        source.ShouldContain("EntityBreadcrumbTracker");
        source.ShouldContain("—");
    }

    [Fact]
    public void HooshvareEditor_SetsEntityCrumbAndTitle()
    {
        var source = File.ReadAllText(Find("pro-modules/ai-hooshvare/src/SufiChain.SufiPlatform.SufiAI.Hooshvare.Blazor/Pages/AI/Hooshvare/HooshvareEditorPage.razor.cs"));
        source.ShouldContain("EntityBreadcrumbTracker");
        source.ShouldContain("_editDisplayName.Trim()");
    }

    [Fact]
    public void ProjectSettings_SetsEntityCrumb()
    {
        var source = File.ReadAllText(Find("pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.Blazor/Pages/Projects/ProjectSettings.razor.cs"));
        source.ShouldContain("EntityBreadcrumbTracker");
        source.ShouldContain("_project?.Name");
    }

    private static IMenuManager Menu(ApplicationMenu menu)
    {
        var manager = Substitute.For<IMenuManager>();
        manager.GetMainMenuAsync().Returns(menu);
        return manager;
    }

    private static string Find(string relative)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, relative)))
        {
            dir = dir.Parent;
        }

        dir.ShouldNotBeNull();
        return Path.Combine(dir!.FullName, relative);
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
