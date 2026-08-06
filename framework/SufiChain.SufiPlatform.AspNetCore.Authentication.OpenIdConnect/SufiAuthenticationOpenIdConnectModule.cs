using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.Account;
using SufiChain.SufiPlatform.Identity.AspNetCore;
using SufiChain.SufiPlatform.OpenIddict;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.AspNetCore.Authentication.OpenIdConnect;

/// <summary>
/// ABP Module for Sufi Authentication with OpenID Connect.
/// Provides OpenIddict authorization endpoint that redirects to Blazor login pages.
/// This module replaces ABP's default AuthorizeController with one that works with Blazor UI.
/// </summary>
[DependsOn(
    typeof(SufiOpenIddictAspNetCoreModule),
    typeof(SufiAccountApplicationContractsModule),
    typeof(SufiIdentityAspNetCoreModule)
)]
public class SufiAuthenticationOpenIdConnectModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Controllers are auto-discovered by ASP.NET Core MVC
    }

    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        // Remove ABP's OpenIddict AuthorizeController to avoid route conflicts
        // with our custom AuthorizationController that redirects to Blazor login pages.
        var partManager = context.Services
            .FirstOrDefault(s => s.ServiceType == typeof(ApplicationPartManager))
            ?.ImplementationInstance as ApplicationPartManager;

        partManager?.FeatureProviders.Add(new ExcludeAbpOpenIddictControllerFeatureProvider());
    }
}

/// <summary>
/// Feature provider that excludes ABP's OpenIddict AuthorizeController
/// to allow our custom implementation to handle /connect/authorize.
/// </summary>
internal class ExcludeAbpOpenIddictControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        var abpAuthorizeController = feature.Controllers
            .FirstOrDefault(t => t.FullName == "Volo.Abp.OpenIddict.Controllers.AuthorizeController");

        if (abpAuthorizeController != null)
        {
            feature.Controllers.Remove(abpAuthorizeController);
        }
    }
}
