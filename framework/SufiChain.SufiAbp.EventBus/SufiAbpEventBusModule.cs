using Volo.Abp.EventBus;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.EventBus;

[DependsOn(
    typeof(AbpEventBusModule)
)]
public class SufiAbpEventBusModule : AbpModule
{
}
