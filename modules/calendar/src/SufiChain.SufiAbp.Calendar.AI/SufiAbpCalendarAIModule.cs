using SufiChain.SufiAbp.AI;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Calendar;

/// <summary>
/// Optional Calendar AI integration module. Publishes narrow, permission-checked
/// Calendar tools through the platform AI abstraction layer without making the
/// Calendar core depend on any AI provider module.
/// </summary>
[DependsOn(
    typeof(SufiAbpCalendarApplicationModule),
    typeof(SufiAIAbstractionsModule)
)]
public class SufiAbpCalendarAIModule : AbpModule
{
}
