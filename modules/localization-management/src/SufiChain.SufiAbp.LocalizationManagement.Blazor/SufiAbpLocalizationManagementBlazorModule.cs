using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.LocalizationManagement.Blazor.Menus;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.LocalizationManagement.Blazor;

[DependsOn(
    typeof(SufiAbpLocalizationManagementApplicationContractsModule)
)]
public class SufiAbpLocalizationManagementBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register this assembly for Blazor routing
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpLocalizationManagementBlazorModule).Assembly);
        });

        // Register menu contributor
        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new LocalizationManagementMenuContributor());
        });
    }
}
