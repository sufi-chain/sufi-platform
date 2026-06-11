using SufiChain.SufiAbp.Caching;
using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Calendar;

[DependsOn(
    typeof(SufiAbpDddDomainModule),
    typeof(SufiAbpCachingModule),
    typeof(SufiAbpCalendarDomainSharedModule)
)]
public class SufiAbpCalendarDomainModule : AbpModule
{
}
