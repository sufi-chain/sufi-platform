using SufiChain.SufiPlatform.Menus.Blazor.Menus;
using SufiChain.SufiPlatform.Localization.Blazor.Public;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Menus.Blazor;

[DependsOn(
    typeof(SufiMenusApplicationContractsModule),
    typeof(SufiLocalizationBlazorPublicModule))]
public class SufiMenusBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<SufiRouterOptions>(options => options.AdditionalAssemblies.Add(typeof(SufiMenusBlazorModule).Assembly));

        Configure<SufiNavigationOptions>(options => options.MenuContributors.Add(new MenusMenuContributor()));
    }
}