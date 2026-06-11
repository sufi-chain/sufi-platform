using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Calendar;

[DependsOn(typeof(SufiAbpCalendarApplicationContractsModule))]
public class SufiAbpCalendarHttpApiClientModule : AbpModule
{
}
