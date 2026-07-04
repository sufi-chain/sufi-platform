using SufiChain.SufiAbp.TagsManagement.Localization;
using SufiChain.SufiAbp.TagsManagement.Permissions;
using SufiChain.SufiAbp.UI.Navigation;

namespace SufiChain.SufiAbp.TagsManagement.Blazor.Menus;

/// <summary>
/// Menu contributor for TagsManagement admin pages.
/// </summary>
public class TagsManagementMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            return ConfigureMainMenuAsync(context);
        }

        return Task.CompletedTask;
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<SufiAbpTagsManagementResource>();
        var administration = context.Menu.GetAdministration();

        var moduleMenu = new ApplicationMenuItem(
            TagsManagementMenuNames.GroupName,
            l["Menu:TagsManagement"],
            icon: "tag",
            order: 20
        );

        administration.AddItem(moduleMenu);

        moduleMenu.AddItem(new ApplicationMenuItem(
            TagsManagementMenuNames.Tags,
            l["Menu:TagsManagement.Tags"],
            url: "/panel/admin/tags-management/tags",
            icon: "tag"
        ).RequirePermissions(TagsManagementPermissions.Tags.Default));

        moduleMenu.AddItem(new ApplicationMenuItem(
            TagsManagementMenuNames.TagLinks,
            l["Menu:TagsManagement.TagLinks"],
            url: "/panel/admin/tags-management/tag-links",
            icon: "link"
        ).RequirePermissions(TagsManagementPermissions.TagLinks.Default));

        return Task.CompletedTask;
    }
}

/// <summary>
/// Menu name constants for TagsManagement module.
/// </summary>
public static class TagsManagementMenuNames
{
    public const string GroupName = "TagsManagement";
    public const string Tags = GroupName + ".Tags";
    public const string TagLinks = GroupName + ".TagLinks";
}
