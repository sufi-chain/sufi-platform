using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Localization.Blazor.Menus;
using SufiChain.SufiPlatform.Localization.Blazor.Settings;
using SufiChain.SufiPlatform.Settings.Blazor;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Localization.Blazor;

[DependsOn(
    typeof(SufiLocalizationApplicationContractsModule),
    typeof(SufiSettingsBlazorModule)
)]
public class SufiLocalizationBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register this assembly for Blazor routing
        Configure<SufiRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiLocalizationBlazorModule).Assembly);
        });

        // Register menu contributor
        Configure<SufiNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new LocalizationMenuContributor());
        });

        Configure<SettingsComponentOptions>(options =>
        {
            options.Contributors.Add(new LocalizationSettingsGroupContributor());
        });
    }
}
