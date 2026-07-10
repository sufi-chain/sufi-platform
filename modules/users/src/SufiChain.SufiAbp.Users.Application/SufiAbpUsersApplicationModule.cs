using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Users;

[DependsOn(
    typeof(SufiAbpUsersApplicationContractsModule),
    typeof(SufiAbpDddApplicationModule)
)]
public class SufiAbpUsersApplicationModule : AbpModule
{
}
