using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Users;

[DependsOn(
    typeof(SufiUsersDomainSharedModule),
    typeof(SufiUsersAbstractionModule),
    typeof(AbpDddDomainModule)
    )]
public class SufiUsersDomainModule : AbpModule
{

}
