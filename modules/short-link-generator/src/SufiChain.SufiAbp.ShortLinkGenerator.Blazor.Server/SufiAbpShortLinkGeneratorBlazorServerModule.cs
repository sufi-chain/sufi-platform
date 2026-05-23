using SufiChain.SufiAbp.AspNetCore.Components.Server.Theming;
using SufiChain.SufiAbp.ShortLinkGenerator;
using Volo.Abp.AspNetCore.Components.Server.Theming;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

[DependsOn(
    typeof(SufiAbpAspNetCoreComponentsServerThemingModule),
    typeof(SufiAbpShortLinkGeneratorBlazorModule)
)]
public class SufiAbpShortLinkGeneratorBlazorServerModule : AbpModule
{
}
