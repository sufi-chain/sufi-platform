using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Identity.Blazor.Public;

[DependsOn(typeof(SufiIdentityApplicationContractsModule))]
public class SufiIdentityBlazorPublicModule : AbpModule
{
}
