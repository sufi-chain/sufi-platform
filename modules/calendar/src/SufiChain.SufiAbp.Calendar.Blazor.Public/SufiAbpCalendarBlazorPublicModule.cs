using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Calendar.Blazor.Public;

[DependsOn(typeof(SufiAbpCalendarApplicationContractsModule))]
public class SufiAbpCalendarBlazorPublicModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpCalendarBlazorPublicModule).Assembly);
        });
    }
}
