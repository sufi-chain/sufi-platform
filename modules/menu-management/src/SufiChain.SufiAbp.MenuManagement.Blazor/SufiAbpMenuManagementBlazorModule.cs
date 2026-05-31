using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.MenuManagement.Blazor;

[DependsOn(typeof(SufiAbpMenuManagementApplicationContractsModule))]
public class SufiAbpMenuManagementBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<SufiAbpRouterOptions>(options => options.AdditionalAssemblies.Add(typeof(SufiAbpMenuManagementBlazorModule).Assembly));
    }
}
