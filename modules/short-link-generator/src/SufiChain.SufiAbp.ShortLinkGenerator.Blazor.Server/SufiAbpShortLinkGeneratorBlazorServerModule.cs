using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

/// <summary>
/// Blazor Server host adapter for Short Link Generator (SufiTheme / SufiAbp UI — no Volo Blazor theming).
/// </summary>
[DependsOn(typeof(SufiAbpShortLinkGeneratorBlazorModule))]
public class SufiAbpShortLinkGeneratorBlazorServerModule : AbpModule
{
}
