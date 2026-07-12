using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Calendar;

[DependsOn(typeof(SufiCalendarApplicationContractsModule))]
public class SufiCalendarHttpApiClientModule : AbpModule
{
}