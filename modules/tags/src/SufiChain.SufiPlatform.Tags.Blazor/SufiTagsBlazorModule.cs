using SufiChain.SufiPlatform.Settings.Blazor;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.Tags.Blazor.Menus;
using SufiChain.SufiPlatform.Tags.Blazor.Settings;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Tags.Blazor;

/// <summary>
/// ABP Module for Sufi Tags Blazor UI.
/// Provides Tag management and TagLink inspector pages.
/// </summary>
[DependsOn(
    typeof(SufiTagsApplicationContractsModule),
    typeof(SufiSettingsBlazorModule)
)]
public class SufiTagsBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register this assembly for Blazor routing
        Configure<SufiRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiTagsBlazorModule).Assembly);
        });

        // Register menu contributor
        Configure<SufiNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new TagsMenuContributor());
        });

        Configure<SettingsComponentOptions>(options =>
        {
            options.Contributors.Add(new TagsSettingsGroupContributor());
        });
    }
}