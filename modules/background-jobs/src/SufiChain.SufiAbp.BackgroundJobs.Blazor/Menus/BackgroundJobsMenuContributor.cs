using SufiChain.SufiAbp.BackgroundJobs.Localization;
using SufiChain.SufiAbp.BackgroundJobs.Permissions;
using SufiChain.SufiAbp.UI.Navigation;

namespace SufiChain.SufiAbp.BackgroundJobs.Blazor.Menus;

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
        var l = context.GetLocalizer<SufiAbpBackgroundJobsResource>();
        var administration = context.Menu.GetAdministration();

        administration.AddItem(new ApplicationMenuItem(
            BackgroundJobsMenuNames.GroupName,
            l["BackgroundJobs"],
            url: "/admin/background-jobs",
            icon: "clock",
            order: 30
        ).RequirePermissions(BackgroundJobsPermissions.BackgroundJobs.Default));

        return Task.CompletedTask;
    }
}

public static class BackgroundJobsMenuNames
{
    public const string GroupName = "BackgroundJobs";
}
