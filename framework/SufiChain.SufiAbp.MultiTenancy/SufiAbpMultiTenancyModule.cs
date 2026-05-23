using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.MultiTenancy;

[DependsOn(typeof(AbpMultiTenancyModule))]
public class SufiAbpMultiTenancyModule : AbpModule
{
}
