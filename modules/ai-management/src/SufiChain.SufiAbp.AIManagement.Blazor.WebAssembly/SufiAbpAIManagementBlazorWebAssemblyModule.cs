using SufiChain.SufiAbp.AIManagement.Blazor;
using Volo.Abp.AspNetCore.Components.WebAssembly;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AIManagement.Blazor.WebAssembly;

[DependsOn(
    typeof(SufiAbpAIManagementBlazorModule),
    typeof(SufiAbpAIManagementHttpApiClientModule),
    typeof(AbpAspNetCoreComponentsWebAssemblyModule)
)]
public class SufiAbpAIManagementBlazorWebAssemblyModule : AbpModule
{
}
