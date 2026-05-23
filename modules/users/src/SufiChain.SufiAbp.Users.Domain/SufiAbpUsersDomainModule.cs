using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Users;

[DependsOn(
    typeof(SufiAbpUsersDomainSharedModule),
    typeof(SufiAbpUsersAbstractionModule),
    typeof(AbpDddDomainModule)
    )]
public class SufiAbpUsersDomainModule : AbpModule
{

}
