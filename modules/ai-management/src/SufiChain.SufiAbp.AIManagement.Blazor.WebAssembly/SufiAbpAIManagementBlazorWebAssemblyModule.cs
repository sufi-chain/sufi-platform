using SufiChain.SufiAbp.AIManagement.Blazor;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AIManagement.Blazor.WebAssembly;

[DependsOn(
    typeof(SufiAbpAIManagementBlazorModule),
    typeof(SufiAbpAIManagementHttpApiClientModule)
)]
public class SufiAbpAIManagementBlazorWebAssemblyModule : AbpModule
{
}
