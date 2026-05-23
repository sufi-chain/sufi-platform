using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Identity;

[DependsOn(
    typeof(global::SufiChain.SufiAbp.Identity.SufiAbpIdentityApplicationContractsModule),
    typeof(SufiAbpAspNetCoreMvcModule)
)]
public class SufiAbpIdentityHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
        });
    }
}
