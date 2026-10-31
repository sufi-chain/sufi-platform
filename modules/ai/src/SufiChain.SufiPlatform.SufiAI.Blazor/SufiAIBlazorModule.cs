using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.Settings.Blazor;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.SufiAI.Blazor.Menus;
using SufiChain.SufiPlatform.SufiAI.Blazor.Settings;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiAI.Blazor;

[DependsOn(
    typeof(SufiAIApplicationContractsModule),
    typeof(SufiSettingsBlazorModule)
)]
public class SufiAIBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register menu contributor
        Configure<SufiNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new AIMenuContributor());
        });

        // Register this assembly for Blazor routing
        Configure<SufiRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAIBlazorModule).Assembly);
        });

        Configure<SettingsComponentOptions>(options =>
        {
            options.Contributors.Add(new WebResearchSettingsGroupContributor());
        });
    }
}
