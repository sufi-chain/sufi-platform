using SufiChain.SufiPlatform.Authorization;
using SufiChain.SufiPlatform.Ddd;
using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Users;

[DependsOn(
    typeof(SufiDddApplicationContractsModule),
    typeof(SufiAuthorizationModule),
    typeof(SufiUsersDomainSharedModule)
)]
public class SufiUsersApplicationContractsModule : AbpModule
{
}
