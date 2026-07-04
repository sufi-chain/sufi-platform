using SufiChain.SufiAbp.TagsManagement.Blazor.Menus;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.TagsManagement.Blazor;

/// <summary>
/// ABP Module for SufiAbp TagsManagement Blazor UI.
/// Provides Tag management and TagLink inspector pages.
/// </summary>
[DependsOn(
    typeof(SufiAbpTagsManagementApplicationContractsModule)
)]
public class SufiAbpTagsManagementBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register this assembly for Blazor routing
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpTagsManagementBlazorModule).Assembly);
        });

        // Register menu contributor
        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new TagsManagementMenuContributor());
        });
    }
}
