using SufiChain.SufiPlatform.Editions.Blazor.Menus;
using SufiChain.SufiPlatform.Features.Blazor;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Editions.Blazor;

[DependsOn(
    typeof(SufiEditionsApplicationContractsModule),
    typeof(SufiFeaturesBlazorModule)
)]
public class SufiEditionsBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<SufiRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiEditionsBlazorModule).Assembly);
        });

        Configure<SufiNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new EditionsMenuContributor());
        });
    }
}
