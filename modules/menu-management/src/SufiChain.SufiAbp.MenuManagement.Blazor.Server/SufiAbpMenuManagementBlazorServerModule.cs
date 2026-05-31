using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Server;

[DependsOn(typeof(SufiAbpMenuManagementBlazorModule))]
public class SufiAbpMenuManagementBlazorServerModule : AbpModule
{
}
