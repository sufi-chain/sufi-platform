using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Tenants.Localization;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.Tenants;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Tenants.Blazor.Menus;

/// <summary>
/// Menu contributor for Tenants pages (host-only).
/// </summary>
public class TenantsMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var currentTenant = context.ServiceProvider.GetRequiredService<ICurrentTenant>();
        if (currentTenant.Id != null)
        {
            return Task.CompletedTask;
        }

        var l = context.GetLocalizer<SufiTenantsResource>();
        var administration = context.Menu.GetAdministration();

        administration.AddItem(new ApplicationMenuItem(
            TenantsMenuNames.GroupName,
            l["Menu:Tenants"],
            url: "/panel/admin/tenant-management/tenants",
            icon: "building",
            order: 10
        ).RequirePermissions(TenantsPermissions.Tenants.Default));

        return Task.CompletedTask;
    }
}

/// <summary>
/// Menu name constants for Tenants module.
/// </summary>
public static class TenantsMenuNames
{
    public const string GroupName = "SufiTenants";
    public const string Tenants = GroupName + ".Tenants";
}
