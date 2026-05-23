using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.PermissionManagement;

[DependsOn(
    typeof(SufiAbpPermissionManagementApplicationContractsModule),
    typeof(SufiAbpAspNetCoreMvcModule)
)]
public class SufiAbpPermissionManagementHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
        });
    }
}
