using SufiChain.SufiPlatform.BackgroundJobs.Localization;
using SufiChain.SufiPlatform.BackgroundJobs.Permissions;
using SufiChain.SufiPlatform.UI.Navigation;

namespace SufiChain.SufiPlatform.BackgroundJobs.Blazor.Menus;

public class BackgroundJobsMenuContributor : IMenuContributor
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
        var l = context.GetLocalizer<SufiBackgroundJobsResource>();
        var administration = context.Menu.GetAdministration();

        administration.AddItem(new ApplicationMenuItem(
            BackgroundJobsMenuNames.GroupName,
            l["BackgroundJobs"],
            url: "/panel/admin/background-jobs",
            icon: "clock",
            order: 30
        ).RequirePermissions(BackgroundJobsPermissions.BackgroundJobs.Default));

        return Task.CompletedTask;
    }
}

public static class BackgroundJobsMenuNames
{
    public const string GroupName = "SufiBackgroundJobs";
}
