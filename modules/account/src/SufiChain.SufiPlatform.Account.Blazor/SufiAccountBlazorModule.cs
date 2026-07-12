using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.Account.Blazor.Menus;
using SufiChain.SufiPlatform.Account.Blazor.Services;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.AspNetCore;
using SufiChain.SufiPlatform.UI.Abstractions.Account;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using SufiChain.SufiBlazor;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Account.Blazor;

/// <summary>
/// ABP Module for Sufi Account Blazor UI.
/// Provides Login, Register, Profile, and Password management pages using Static SSR.
/// Uses SignInManager and UserManager for direct cookie-based authentication.
/// </summary>
[DependsOn(
    typeof(SufiIdentityAspNetCoreModule),
    typeof(SufiIdentityApplicationContractsModule),
    typeof(SufiAccountApplicationContractsModule)
)]
public class SufiAccountBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // SufiBlazor required for Sb components used in AccountLayout and account pages
        context.Services.AddSufiBlazor();

        // Register this assembly for Blazor routing
        Configure<SufiRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAccountBlazorModule).Assembly);
        });

        // Register menu contributor for user menu
        Configure<SufiNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new AccountMenuContributor());
        });

        // Default no-op token store; hosts using Interactive Server with cookie auth register
        // a real implementation (e.g. AuthenticationServerModule) which we must not override.
        context.Services.TryAddScoped<ILoginCompletionTokenStore, NullLoginCompletionTokenStore>();
        context.Services.TryAddScoped<ITwoFactorPendingLoginStore, NullTwoFactorPendingLoginStore>();
    }
}
