using SufiChain.SufiAbp.TenantManagement.Localization;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.TenantManagement;

namespace SufiChain.SufiAbp.TenantManagement.Blazor.Menus;

/// <summary>
/// Menu contributor for TenantManagement pages.
/// </summary>
public class TenantManagementMenuContributor : IMenuContributor
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
        var l = context.GetLocalizer<SufiAbpTenantManagementResource>();
        var administration = context.Menu.GetAdministration();

        administration.AddItem(new ApplicationMenuItem(
            TenantManagementMenuNames.GroupName,
            l["Menu:TenantManagement"],
            url: "/admin/tenant-management/tenants",
            icon: "building",
            order: 10
        ).RequirePermissions(TenantManagementPermissions.Tenants.Default));

        return Task.CompletedTask;
    }
}

/// <summary>
/// Menu name constants for TenantManagement module.
/// </summary>
public static class TenantManagementMenuNames
{
    public const string GroupName = "TenantManagement";
    public const string Tenants = GroupName + ".Tenants";
}
