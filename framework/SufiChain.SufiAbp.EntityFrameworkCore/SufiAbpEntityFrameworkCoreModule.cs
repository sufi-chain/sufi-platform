using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.EntityFrameworkCore;

[DependsOn(
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiAbpEntityFrameworkCoreModule : AbpModule
{
}
