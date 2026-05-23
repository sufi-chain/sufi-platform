using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace SufiChain.SufiAbp.Threading;

[DependsOn(typeof(AbpThreadingModule))]
public class SufiAbpThreadingModule : AbpModule
{
}
