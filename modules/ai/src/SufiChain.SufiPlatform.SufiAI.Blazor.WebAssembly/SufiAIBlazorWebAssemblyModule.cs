using SufiChain.SufiPlatform.SufiAI.Blazor;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.WebAssembly;

[DependsOn(
    typeof(SufiAIBlazorModule),
    typeof(SufiAIHttpApiClientModule)
)]
public class SufiAIBlazorWebAssemblyModule : AbpModule
{
}
