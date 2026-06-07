using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

/// <summary>
/// Blazor WebAssembly host adapter for Short Link Generator (SufiAbp UI — no Volo Blazor theming).
/// </summary>
[DependsOn(
    typeof(SufiAbpShortLinkGeneratorBlazorModule),
    typeof(SufiAbpShortLinkGeneratorHttpApiClientModule)
)]
public class SufiAbpShortLinkGeneratorBlazorWebAssemblyModule : AbpModule
{
}
