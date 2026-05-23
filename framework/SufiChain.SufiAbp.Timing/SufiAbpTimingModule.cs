using Volo.Abp.Modularity;
using Volo.Abp.Timing;

namespace SufiChain.SufiAbp.Timing;

[DependsOn(typeof(AbpTimingModule))]
public class SufiAbpTimingModule : AbpModule
{
}
