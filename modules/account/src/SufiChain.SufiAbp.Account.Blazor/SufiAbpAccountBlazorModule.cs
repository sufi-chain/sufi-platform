using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiAbp.Account.Blazor.Menus;
using SufiChain.SufiAbp.Account.Blazor.Services;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Identity.AspNetCore;
using SufiChain.SufiAbp.UI.Abstractions.Account;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using SufiChain.SufiBlazor;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Account.Blazor;

/// <summary>
/// ABP Module for SufiAbp Account Blazor UI.
/// Provides Login, Register, Profile, and Password management pages using Static SSR.
/// Uses SignInManager and UserManager for direct cookie-based authentication.
/// </summary>
[DependsOn(
    typeof(SufiAbpIdentityAspNetCoreModule),
    typeof(SufiAbpIdentityApplicationContractsModule),
    typeof(SufiAbpAccountApplicationContractsModule)
)]
public class SufiAbpAccountBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // SufiBlazor required for Sb components used in AccountLayout and account pages
        context.Services.AddSufiBlazor();

        // Register this assembly for Blazor routing
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpAccountBlazorModule).Assembly);
        });

        // Register menu contributor for user menu
        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new AccountMenuContributor());
        });

        // Default no-op token store; hosts using Interactive Server with cookie auth register
        // a real implementation (e.g. AuthenticationServerModule) which we must not override.
        context.Services.TryAddScoped<ILoginCompletionTokenStore, NullLoginCompletionTokenStore>();
        context.Services.TryAddScoped<ITwoFactorPendingLoginStore, NullTwoFactorPendingLoginStore>();
    }
}
