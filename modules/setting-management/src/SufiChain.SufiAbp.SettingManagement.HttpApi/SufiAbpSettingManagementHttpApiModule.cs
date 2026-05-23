using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.SettingManagement;

[DependsOn(
    typeof(SufiAbpSettingManagementApplicationContractsModule),
    typeof(SufiAbpAspNetCoreMvcModule)
)]
public class SufiAbpSettingManagementHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
        });
    }
}
