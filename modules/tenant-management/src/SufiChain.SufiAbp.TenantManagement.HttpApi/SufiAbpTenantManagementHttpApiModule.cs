using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.TenantManagement;

[DependsOn(
    typeof(SufiAbpAspNetCoreMvcModule),
    typeof(SufiAbpTenantManagementApplicationContractsModule)
)]
public class SufiAbpTenantManagementHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
        });
    }
}
