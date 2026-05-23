using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AspNetCore.Authentication;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.UI.Abstractions.Account;
using SufiChain.SufiAbp.UI.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AspNetCore.Authentication.Server;

/// <summary>
/// ABP Module for SufiAbp server-side authentication.
/// Provides MVC controllers for OIDC (Login/Logout), cookie complete-login for Blazor Interactive Server,
/// and SwitchTenant (HTTP-based tenant cookie) for Blazor UI.
/// </summary>
[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(SufiAbpIdentityDomainModule) // IdentityUserManager for account flows
)]
public class SufiAbpAuthenticationServerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register authentication options
        context.Services.AddOptions<SufiAbpAuthenticationOptions>();

        // Ensure TenantSwitchOptions is available for SwitchTenant action
        context.Services.Configure<TenantSwitchOptions>(_ => { });

        // Default in-memory login completion token store (Blazor Interactive Server -> cookie flow)
        context.Services.AddMemoryCache();
        context.Services.AddSingleton<ILoginCompletionTokenStore, LoginCompletionTokenStore>();

        // SufiAbpAccountController is automatically discovered by MVC
        // Account/{action} routes are available before Blazor fallback
    }
}
