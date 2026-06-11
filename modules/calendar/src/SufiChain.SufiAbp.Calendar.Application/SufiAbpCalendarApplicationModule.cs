using SufiChain.SufiAbp.Caching;
using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Calendar;

[DependsOn(
    typeof(SufiAbpCalendarDomainModule),
    typeof(SufiAbpCalendarApplicationContractsModule),
    typeof(SufiAbpDddApplicationModule),
    typeof(SufiAbpCachingModule)
)]
public class SufiAbpCalendarApplicationModule : AbpModule
{
}
