using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Identity.Blazor.Menus;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Identity.Blazor.Public;
using SufiChain.SufiAbp.Users.Blazor.Public;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.PermissionManagement;

namespace SufiChain.SufiAbp.Identity.Blazor;

/// <summary>
/// ABP Module for SufiAbp Identity Blazor admin UI.
/// Provides User and Role management pages with permission assignment.
/// </summary>
[DependsOn(
    typeof(SufiAbpIdentityApplicationContractsModule),
    typeof(SufiAbpPermissionManagementApplicationContractsModule),
    typeof(SufiAbpIdentityBlazorPublicModule),
    typeof(SufiAbpUsersBlazorPublicModule)
)]
public class SufiAbpIdentityBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register this assembly for Blazor routing
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpIdentityBlazorModule).Assembly);
        });

        // Register menu contributor
        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new IdentityMenuContributor());
        });
    }
}
