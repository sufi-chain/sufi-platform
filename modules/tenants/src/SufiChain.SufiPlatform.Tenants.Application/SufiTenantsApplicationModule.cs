using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.Tenants;

namespace SufiChain.SufiPlatform.Tenants;

[DependsOn(
    typeof(SufiTenantsApplicationContractsModule),
    typeof(SufiTenantsDomainModule)
)]
public class SufiTenantsApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<ITenantAppService, TenantAppService>();
    }
}
