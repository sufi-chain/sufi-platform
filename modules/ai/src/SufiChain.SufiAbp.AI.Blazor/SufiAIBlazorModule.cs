using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AI.Blazor.Menus;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AI.Blazor;

[DependsOn(
    typeof(SufiAIApplicationContractsModule)
)]
public class SufiAIBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register menu contributor
        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new AIMenuContributor());
        });

        // Register this assembly for Blazor routing
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAIBlazorModule).Assembly);
        });
    }
}
