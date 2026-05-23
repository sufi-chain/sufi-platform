using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.TenantManagement;

namespace SufiChain.SufiAbp.TenantManagement;

[DependsOn(
    typeof(SufiAbpTenantManagementApplicationContractsModule),
    typeof(SufiAbpTenantManagementDomainModule)
)]
public class SufiAbpTenantManagementApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<ITenantAppService, TenantAppService>();
    }
}
