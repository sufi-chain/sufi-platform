using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.ShortLinks;

/// <summary>
/// Blazor Server host adapter for Short Link Generator (SufiTheme / Sufi UI — no Volo Blazor theming).
/// </summary>
[DependsOn(typeof(SufiShortLinksBlazorModule))]
public class SufiShortLinksBlazorServerModule : AbpModule
{
}