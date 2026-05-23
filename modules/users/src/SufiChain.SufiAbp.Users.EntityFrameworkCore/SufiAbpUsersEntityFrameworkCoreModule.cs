using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Users;

[DependsOn(
    typeof(SufiAbpUsersDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
    )]
public class SufiAbpUsersEntityFrameworkCoreModule : AbpModule
{

}
