using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.Features.Blazor;
using SufiChain.SufiPlatform.Tenants.Blazor.Menus;
using SufiChain.SufiPlatform.Tenants.Blazor.TenantSelector;
using SufiChain.SufiPlatform.UI.MultiTenancy;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.Tenants;

namespace SufiChain.SufiPlatform.Tenants.Blazor;

/// <summary>
/// ABP Module for Sufi Tenants Blazor UI.
/// Provides Tenant management pages.
/// For tiered hosts (HttpApi.Client): Add TenantsHttpApiClientModule — provides
/// ITenantLookupAppService via proxy and MvcRemoteTenantStore for ITenantStore.
/// For integrated hosts (Application+DB): Add TenantsApplicationModule — provides
/// ITenantLookupAppService implementation and TenantStore.
/// </summary>
[DependsOn(
    typeof(SufiTenantsApplicationContractsModule),
    typeof(SufiFeaturesBlazorModule)
)]
public class SufiTenantsBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register this assembly for Blazor routing
        Configure<SufiRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiTenantsBlazorModule).Assembly);
        });

        // Register menu contributor
        Configure<SufiNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new TenantsMenuContributor());
        });

        // Replace default tenant lookup with tenant-management implementation (for SelectFromList/Search modes)
        context.Services.Replace(ServiceDescriptor.Scoped<ITenantLookupService, TenantLookupServiceAdapter>());
    }
}
