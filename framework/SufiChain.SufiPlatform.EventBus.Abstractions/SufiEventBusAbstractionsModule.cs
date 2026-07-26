using Volo.Abp.Caching;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.EventBus;

/// <summary>
/// Registers Sufi Platform distributed-event abstractions (ETO base + idempotency store).
/// </summary>
[DependsOn(typeof(AbpCachingModule))]
public class SufiEventBusAbstractionsModule : AbpModule
{
}
