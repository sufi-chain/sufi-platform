using SufiChain.SufiAbp.AspNetCore.Components.WebAssembly.Theming;
using Volo.Abp.AspNetCore.Components.WebAssembly.Theming;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

[DependsOn(
    typeof(SufiAbpShortLinkGeneratorBlazorModule),
    typeof(SufiAbpShortLinkGeneratorHttpApiClientModule),
    typeof(SufiAbpAspNetCoreComponentsWebAssemblyThemingModule)
)]
public class SufiAbpShortLinkGeneratorBlazorWebAssemblyModule : AbpModule
{
}
