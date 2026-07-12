using SufiChain.SufiPlatform.Ddd;
using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Users;

[DependsOn(
    typeof(SufiUsersApplicationContractsModule),
    typeof(SufiDddApplicationModule)
)]
public class SufiUsersApplicationModule : AbpModule
{
}
