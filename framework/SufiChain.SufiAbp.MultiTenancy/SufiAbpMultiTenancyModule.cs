using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.MultiTenancy;

[DependsOn(typeof(Volo.Abp.MultiTenancy.AbpMultiTenancyModule))]
public class SufiAbpMultiTenancyModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<ICurrentTenant, SufiAbpCurrentTenant>();
    }
}
