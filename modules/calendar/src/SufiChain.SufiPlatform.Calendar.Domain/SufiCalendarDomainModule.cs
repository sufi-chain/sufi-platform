using SufiChain.SufiPlatform.Users;
using Volo.Abp.Modularity;
using Volo.Abp.Domain;

using Volo.Abp.Caching;
namespace SufiChain.SufiPlatform.Calendar;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(AbpCachingModule),
    typeof(SufiUsersAbstractionModule),
    typeof(SufiCalendarDomainSharedModule)
)]
public class SufiCalendarDomainModule : AbpModule
{
}