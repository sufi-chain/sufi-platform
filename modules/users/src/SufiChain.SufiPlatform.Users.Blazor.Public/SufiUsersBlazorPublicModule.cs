using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Users.Blazor.Public;

[DependsOn(typeof(SufiUsersApplicationContractsModule))]
public class SufiUsersBlazorPublicModule : AbpModule
{
}
