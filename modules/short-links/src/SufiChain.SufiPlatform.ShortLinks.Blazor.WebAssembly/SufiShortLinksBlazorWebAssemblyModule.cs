using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.ShortLinks;

/// <summary>
/// Blazor WebAssembly host adapter for Short Link Generator (Sufi UI — no Volo Blazor theming).
/// </summary>
[DependsOn(
    typeof(SufiShortLinksBlazorModule),
    typeof(SufiShortLinksHttpApiClientModule)
)]
public class SufiShortLinksBlazorWebAssemblyModule : AbpModule
{
}