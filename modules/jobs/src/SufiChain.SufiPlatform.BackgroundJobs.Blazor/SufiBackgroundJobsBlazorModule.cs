using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.BackgroundJobs.Blazor.Menus;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.UI.Routing;

namespace SufiChain.SufiPlatform.BackgroundJobs.Blazor;

[DependsOn(
    typeof(SufiBackgroundJobsApplicationContractsModule)
)]
public class SufiBackgroundJobsBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register this assembly for Blazor routing
        Configure<SufiRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiBackgroundJobsBlazorModule).Assembly);
        });

        // Register menu contributor
        Configure<SufiNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new BackgroundJobsMenuContributor());
        });
    }
}
