using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using SufiChain.SufiAbp.AuditLogging.Blazor.Menus;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AuditLogging.Blazor;

[DependsOn(
    typeof(SufiAbpAuditLoggingApplicationContractsModule)
)]
public class SufiAbpAuditLoggingBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register this assembly for Blazor routing
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpAuditLoggingBlazorModule).Assembly);
        });

        // Register menu contributor
        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new AuditLoggingMenuContributor());
        });
    }
}
