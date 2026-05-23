using Volo.Abp;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Core;

[DependsOn(
    typeof(AbpModule)
)]
public class SufiAbpModule : AbpModule
{
}
