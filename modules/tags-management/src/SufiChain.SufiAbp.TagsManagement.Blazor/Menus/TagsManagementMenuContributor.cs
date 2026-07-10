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

        administration.AddItem(new ApplicationMenuItem(
            TagsManagementMenuNames.Tags,
            l["Tags"],
            url: "/panel/admin/tags",
            icon: "tag",
            order: 20
        ).RequirePermissions(TagsManagementPermissions.Tags.Default));

        return Task.CompletedTask;
    }
}

/// <summary>
/// Menu name constants for TagsManagement module.
/// </summary>
public static class TagsManagementMenuNames
{
    public const string Tags = "TagsManagement.Tags";
}
