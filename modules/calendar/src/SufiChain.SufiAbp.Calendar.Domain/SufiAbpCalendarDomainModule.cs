using SufiChain.SufiAbp.Users;
using Volo.Abp.Modularity;
using Volo.Abp.Domain;

using Volo.Abp.Caching;
namespace SufiChain.SufiAbp.Calendar;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(AbpCachingModule),
    typeof(SufiAbpUsersAbstractionModule),
    typeof(SufiAbpCalendarDomainSharedModule)
)]
public class SufiAbpCalendarDomainModule : AbpModule
{
}
