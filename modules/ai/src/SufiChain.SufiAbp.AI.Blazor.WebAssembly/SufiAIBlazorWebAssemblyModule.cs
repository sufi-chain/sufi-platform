using SufiChain.SufiAbp.AI.Blazor;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AI.Blazor.WebAssembly;

[DependsOn(
    typeof(SufiAIBlazorModule),
    typeof(SufiAIHttpApiClientModule)
)]
public class SufiAIBlazorWebAssemblyModule : AbpModule
{
}
