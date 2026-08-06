using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Users;

[DependsOn(
    typeof(SufiUsersDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
    )]
public class SufiUsersEntityFrameworkCoreModule : AbpModule
{

}
