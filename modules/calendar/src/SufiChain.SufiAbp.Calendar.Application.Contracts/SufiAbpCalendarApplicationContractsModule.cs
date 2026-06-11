using SufiChain.SufiAbp.Authorization;
using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Calendar;

[DependsOn(
    typeof(SufiAbpCalendarDomainSharedModule),
    typeof(SufiAbpDddApplicationContractsModule),
    typeof(SufiAbpAuthorizationModule)
)]
public class SufiAbpCalendarApplicationContractsModule : AbpModule
{
}
