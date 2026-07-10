using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Identity.Blazor.Public;

[DependsOn(typeof(SufiAbpIdentityApplicationContractsModule))]
public class SufiAbpIdentityBlazorPublicModule : AbpModule
{
}
