using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Identity.Blazor.Menus;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Blazor.Public;
using SufiChain.SufiPlatform.Users.Blazor.Public;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.Permissions;

namespace SufiChain.SufiPlatform.Identity.Blazor;

/// <summary>
/// ABP Module for Sufi Identity Blazor admin UI.
/// Provides User and Role management pages with permission assignment.
/// </summary>
[DependsOn(
    typeof(SufiIdentityApplicationContractsModule),
    typeof(SufiPermissionsApplicationContractsModule),
    typeof(SufiIdentityBlazorPublicModule),
    typeof(SufiUsersBlazorPublicModule)
)]
public class SufiIdentityBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register this assembly for Blazor routing
        Configure<SufiRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiIdentityBlazorModule).Assembly);
        });

        // Register menu contributor
        Configure<SufiNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new IdentityMenuContributor());
        });
    }
}
