using SufiChain.SufiAbp.Caching;
using SufiChain.SufiAbp.Ddd;
using SufiChain.SufiAbp.Users;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Calendar;

[DependsOn(
    typeof(SufiAbpDddDomainModule),
    typeof(SufiAbpCachingModule),
    typeof(SufiAbpUsersAbstractionModule),
    typeof(SufiAbpCalendarDomainSharedModule)
)]
public class SufiAbpCalendarDomainModule : AbpModule
{
}
