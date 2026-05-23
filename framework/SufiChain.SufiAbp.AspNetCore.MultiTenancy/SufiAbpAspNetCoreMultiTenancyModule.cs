using Volo.Abp.Modularity;
using Volo.Abp.AspNetCore.MultiTenancy;

namespace SufiChain.SufiAbp.AspNetCore.MultiTenancy;

[DependsOn(typeof(AbpAspNetCoreMultiTenancyModule))]
public class SufiAbpAspNetCoreMultiTenancyModule : AbpModule
{
}
