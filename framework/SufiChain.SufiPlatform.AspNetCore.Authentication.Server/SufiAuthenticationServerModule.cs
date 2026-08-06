using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.AspNetCore.Authentication;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.UI.Abstractions.Account;
using SufiChain.SufiPlatform.UI.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.AspNetCore.Authentication.Server;

/// <summary>
/// ABP Module for Sufi server-side authentication.
/// Provides MVC controllers for OIDC (Login/Logout), cookie complete-login for Blazor Interactive Server,
/// and SwitchTenant (HTTP-based tenant cookie) for Blazor UI.
/// </summary>
[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(SufiIdentityDomainModule) // IdentityUserManager for account flows
)]
public class SufiAuthenticationServerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register authentication options
        context.Services.AddOptions<SufiAuthenticationOptions>();

        // Ensure TenantSwitchOptions is available for SwitchTenant action
        context.Services.Configure<TenantSwitchOptions>(_ => { });

        // Default in-memory login completion token store (Blazor Interactive Server -> cookie flow)
        context.Services.AddMemoryCache();
        context.Services.AddSingleton<ILoginCompletionTokenStore, LoginCompletionTokenStore>();
        context.Services.AddSingleton<ITwoFactorPendingLoginStore, TwoFactorPendingLoginStore>();

        // SufiAccountController is automatically discovered by MVC
        // Account/{action} routes are available before Blazor fallback
    }
}
