using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.BackgroundWorkers;

[DependsOn(
    typeof(AbpBackgroundWorkersModule)
)]
public class SufiAbpBackgroundWorkersModule : AbpModule
{
}
