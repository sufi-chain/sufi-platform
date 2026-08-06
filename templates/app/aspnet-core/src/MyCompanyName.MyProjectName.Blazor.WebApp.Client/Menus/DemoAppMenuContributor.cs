using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.UI.Navigation;
using MyCompanyName.MyProjectName.Localization;

namespace MyCompanyName.MyProjectName.Menus;

public class DemoAppMenuContributor : IMenuContributor
{
    private readonly IConfiguration _configuration;

    public DemoAppMenuContributor(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
        else if (context.Menu.Name == StandardMenus.User)
        {
            await ConfigureUserMenuAsync(context);
        }
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.ServiceProvider.GetRequiredService<IStringLocalizer<DemoAppResource>>();

        // Reorder administration sub-menus: Identity, TenantManagement, Settings,
        // AuditLogging, BackgroundJobs, LocalizationManagement
        var administration = context.Menu.GetAdministration();
        administration.DisplayName = l["Administration"];
        administration.Order = 100;
        administration.SetSubItemOrder("Identity", 1);
        administration.SetSubItemOrder("TenantManagement", 2);
        administration.SetSubItemOrder("SettingManagement", 3);
        administration.SetSubItemOrder("AuditLogging", 4);
        administration.SetSubItemOrder("BackgroundJobs", 5);
        administration.SetSubItemOrder("LocalizationManagement", 6);

        var hasDemoModules = false;
        // <TEMPLATE-REMOVE IF-NOT="module:file-manager-demo">
        hasDemoModules = true;
        // </TEMPLATE-REMOVE>
        if (hasDemoModules)
        {
            var demo = context.Menu.GetDemo();
            demo.Order = 101;
        }


        return Task.CompletedTask;
    }

    private Task ConfigureUserMenuAsync(MenuConfigurationContext context)
    {
        var l = context.ServiceProvider.GetRequiredService<IStringLocalizer<DemoAppResource>>();


        context.Menu.AddItem(new ApplicationMenuItem(
            "Account.Logout",
            l["Logout"],
            url: "~/Account/Logout",
            icon: "fa fa-power-off",
            order: int.MaxValue - 1000)
            .RequireAuthenticated());

        return Task.CompletedTask;
    }
}
