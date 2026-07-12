using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using SufiChain.SufiPlatform.AuditLogging.Blazor.Menus;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.AuditLogging.Blazor;

[DependsOn(
    typeof(SufiAuditLoggingApplicationContractsModule)
)]
public class SufiAuditLoggingBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register this assembly for Blazor routing
        Configure<SufiRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAuditLoggingBlazorModule).Assembly);
        });

        // Register menu contributor
        Configure<SufiNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new AuditLoggingMenuContributor());
        });
    }
}
