using SufiChain.SufiPlatform.Ddd;
using SufiChain.SufiPlatform.Licensing;
using SufiChain.SufiPlatform.SufiAI.Hooshvare;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Calendar;

[DependsOn(
    typeof(SufiCalendarAIModule),
    typeof(SufiCalendarDomainSharedModule),
    typeof(SufiAIHooshvareApplicationModule),
    typeof(SufiDddApplicationModule),
    typeof(SufiLicensingRuntimeModule))]
public class SufiCalendarAssistantModule : AbpModule
{
}
