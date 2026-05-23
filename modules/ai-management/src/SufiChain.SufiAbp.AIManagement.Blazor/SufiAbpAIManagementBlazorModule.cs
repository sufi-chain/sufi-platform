using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AIManagement;
using SufiChain.SufiAbp.AIManagement.Blazor.Menus;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AIManagement.Blazor;

[DependsOn(
    typeof(SufiAbpAIManagementApplicationContractsModule)
)]
public class SufiAbpAIManagementBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register menu contributor
        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new AIManagementMenuContributor());
        });

        // Register this assembly for Blazor routing
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpAIManagementBlazorModule).Assembly);
        });
    }
}
