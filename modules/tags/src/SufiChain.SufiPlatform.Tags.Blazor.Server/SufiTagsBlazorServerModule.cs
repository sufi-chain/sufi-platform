using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Tags.Blazor.Server;

[DependsOn(typeof(SufiTagsBlazorModule))]
public class SufiTagsBlazorServerModule : AbpModule
{
}