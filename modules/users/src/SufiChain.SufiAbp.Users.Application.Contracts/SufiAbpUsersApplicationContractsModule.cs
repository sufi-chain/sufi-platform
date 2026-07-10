using SufiChain.SufiAbp.Authorization;
using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Users;

[DependsOn(
    typeof(SufiAbpDddApplicationContractsModule),
    typeof(SufiAbpAuthorizationModule),
    typeof(SufiAbpUsersDomainSharedModule)
)]
public class SufiAbpUsersApplicationContractsModule : AbpModule
{
}
