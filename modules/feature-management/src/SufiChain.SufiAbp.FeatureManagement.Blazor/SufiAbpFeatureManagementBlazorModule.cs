using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using SufiChain.SufiAbp.FeatureManagement;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.FeatureManagement.Blazor;

[DependsOn(typeof(SufiAbpFeatureManagementApplicationContractsModule))]
public class SufiAbpFeatureManagementBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register this assembly for Blazor routing
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpFeatureManagementBlazorModule).Assembly);
        });
    }
}
