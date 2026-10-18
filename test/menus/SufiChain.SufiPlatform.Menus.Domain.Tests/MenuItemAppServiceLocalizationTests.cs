using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Menus;

public class MenuItemAppServiceLocalizationTests
{
    [Fact]
    public void Admin_tree_and_list_localize_after_cache()
    {
        var source = File.ReadAllText(FindRepoFile(Path.Combine(
            "modules",
            "menus",
            "src",
            "SufiChain.SufiPlatform.Menus.Application",
            "Menus",
            "MenuItemAppService.cs")));

        var getTree = source.IndexOf("GetTreeAsync(", StringComparison.Ordinal);
        var localizeTree = source.IndexOf("LocalizeTreeAsync(cached.Tree", StringComparison.Ordinal);
        getTree.ShouldBeGreaterThan(-1);
        localizeTree.ShouldBeGreaterThan(getTree);

        var getList = source.IndexOf("GetListAsync(", StringComparison.Ordinal);
        var localizeItems = source.IndexOf("LocalizeItemsAsync(page", StringComparison.Ordinal);
        getList.ShouldBeGreaterThan(-1);
        localizeItems.ShouldBeGreaterThan(getList);

        source.ShouldContain("MenuBusinessLocalizationService");
        source.ShouldContain("ResolveMenuItemDisplayName");
    }

    [Fact]
    public void Admin_localize_keeps_items_when_display_name_is_empty()
    {
        var admin = File.ReadAllText(FindRepoFile(Path.Combine(
            "modules",
            "menus",
            "src",
            "SufiChain.SufiPlatform.Menus.Application",
            "Menus",
            "MenuItemAppService.cs")));
        var publicMenus = File.ReadAllText(FindRepoFile(Path.Combine(
            "modules",
            "menus",
            "src",
            "SufiChain.SufiPlatform.Menus.Application",
            "Menus",
            "PublicMenuAppService.cs")));

        publicMenus.ShouldContain("if (string.IsNullOrWhiteSpace(copy.DisplayName))");
        publicMenus.ShouldContain("continue;");

        var localizeTree = admin.IndexOf("protected virtual List<MenuItemTreeDto> LocalizeTree(", StringComparison.Ordinal);
        localizeTree.ShouldBeGreaterThan(-1);
        var slice = admin[localizeTree..];
        var nextMethod = slice.IndexOf("protected virtual MenuItemDto LocalizeItem(", StringComparison.Ordinal);
        nextMethod.ShouldBeGreaterThan(0);
        slice[..nextMethod].ShouldNotContain("continue;");
    }

    [Fact]
    public void Category_navigation_edit_loads_stored_item_via_GetAsync()
    {
        var source = File.ReadAllText(FindRepoFile(Path.Combine(
            "pro-modules",
            "helpdesk",
            "src",
            "SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Blazor",
            "Components",
            "ProjectSettings",
            "CategoryNavigationTab.razor.cs"),
            underPlatform: false));

        source.ShouldContain("MenuItemAppService.GetAsync(item.Id)");
        source.ShouldContain("DisplayNames = stored.DisplayNames");
        source.ShouldContain("DisplayName = ResolveEditableDisplayName(stored.DisplayName");
    }

    private static string FindRepoFile(string relativePath, bool underPlatform = true)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidates = underPlatform
                ? new[]
                {
                    Path.Combine(dir.FullName, relativePath),
                    Path.Combine(dir.FullName, "sufi-platform", relativePath)
                }
                : new[]
                {
                    Path.Combine(dir.FullName, relativePath),
                    Path.Combine(dir.FullName, "..", relativePath)
                };

            foreach (var candidate in candidates)
            {
                var full = Path.GetFullPath(candidate);
                if (File.Exists(full))
                {
                    return full;
                }
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(relativePath);
    }
}
