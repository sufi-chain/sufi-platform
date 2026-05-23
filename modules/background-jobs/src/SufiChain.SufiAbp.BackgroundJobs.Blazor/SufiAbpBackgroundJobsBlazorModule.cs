using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.BackgroundJobs.Blazor.Menus;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.UI.Routing;

namespace SufiChain.SufiAbp.BackgroundJobs.Blazor;

[DependsOn(
    typeof(SufiAbpBackgroundJobsApplicationContractsModule)
)]
public class SufiAbpBackgroundJobsBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register this assembly for Blazor routing
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpBackgroundJobsBlazorModule).Assembly);
        });

        // Register menu contributor
        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new BackgroundJobsMenuContributor());
        });
    }
}
