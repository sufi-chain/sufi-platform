using SufiChain.SufiPlatform.Tags.Localization;
using SufiChain.SufiPlatform.Tags.Permissions;
using SufiChain.SufiPlatform.UI.Navigation;

namespace SufiChain.SufiPlatform.Tags.Blazor.Menus;

/// <summary>
/// Menu contributor for Tags admin pages.
/// </summary>
public class TagsMenuContributor : IMenuContributor
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
        var l = context.GetLocalizer<SufiTagsResource>();
        var administration = context.Menu.GetAdministration();

        administration.AddItem(new ApplicationMenuItem(
            TagsMenuNames.Tags,
            l["Menu:SufiTags"],
            url: "/panel/admin/tags",
            icon: "tag",
            order: 20
        ).RequirePermissions(TagsPermissions.Tags.Default));

        return Task.CompletedTask;
    }
}

/// <summary>
/// Menu name constants for Tags module.
/// </summary>
public static class TagsMenuNames
{
    public const string Tags = "SufiTags.Tags";
}