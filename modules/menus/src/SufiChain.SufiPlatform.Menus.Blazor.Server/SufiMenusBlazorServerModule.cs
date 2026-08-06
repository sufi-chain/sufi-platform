using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Menus.Blazor.Server;

[DependsOn(typeof(SufiMenusBlazorModule))]
public class SufiMenusBlazorServerModule : AbpModule
{
}