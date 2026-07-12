using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using SufiChain.SufiPlatform.Features;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Features.Blazor;

[DependsOn(typeof(SufiFeaturesApplicationContractsModule))]
public class SufiFeaturesBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register this assembly for Blazor routing
        Configure<SufiRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiFeaturesBlazorModule).Assembly);
        });
    }
}
