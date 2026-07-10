using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Users.Blazor.Public;

[DependsOn(typeof(SufiAbpUsersApplicationContractsModule))]
public class SufiAbpUsersBlazorPublicModule : AbpModule
{
}
