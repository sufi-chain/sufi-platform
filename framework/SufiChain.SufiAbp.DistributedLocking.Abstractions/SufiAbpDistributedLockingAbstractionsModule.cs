using Volo.Abp.DistributedLocking;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.DistributedLocking.Abstractions;

[DependsOn(typeof(AbpDistributedLockingAbstractionsModule))]
public class SufiAbpDistributedLockingAbstractionsModule : AbpModule
{
}
