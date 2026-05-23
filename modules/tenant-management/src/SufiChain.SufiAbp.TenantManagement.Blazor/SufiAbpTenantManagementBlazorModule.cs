using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiAbp.FeatureManagement.Blazor;
using SufiChain.SufiAbp.TenantManagement.Blazor.Menus;
using SufiChain.SufiAbp.TenantManagement.Blazor.TenantSelector;
using SufiChain.SufiAbp.UI.MultiTenancy;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.TenantManagement;

namespace SufiChain.SufiAbp.TenantManagement.Blazor;

/// <summary>
/// ABP Module for SufiAbp TenantManagement Blazor UI.
/// Provides Tenant management pages.
/// For tiered hosts (HttpApi.Client): Add TenantManagementHttpApiClientModule — provides
/// ITenantLookupAppService via proxy and MvcRemoteTenantStore for ITenantStore.
/// For integrated hosts (Application+DB): Add TenantManagementApplicationModule — provides
/// ITenantLookupAppService implementation and TenantStore.
/// </summary>
[DependsOn(
    typeof(SufiAbpTenantManagementApplicationContractsModule),
    typeof(SufiAbpFeatureManagementBlazorModule)
)]
public class SufiAbpTenantManagementBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register this assembly for Blazor routing
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpTenantManagementBlazorModule).Assembly);
        });

        // Register menu contributor
        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new TenantManagementMenuContributor());
        });

        // Replace default tenant lookup with tenant-management implementation (for SelectFromList/Search modes)
        context.Services.Replace(ServiceDescriptor.Scoped<ITenantLookupService, TenantLookupServiceAdapter>());
    }
}
