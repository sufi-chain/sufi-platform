using SufiChain.SufiPlatform.Authorization;
using SufiChain.SufiPlatform.Ddd;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Calendar;

[DependsOn(
    typeof(SufiCalendarDomainSharedModule),
    typeof(SufiDddApplicationContractsModule),
    typeof(SufiAuthorizationModule)
)]
public class SufiCalendarApplicationContractsModule : AbpModule
{
}