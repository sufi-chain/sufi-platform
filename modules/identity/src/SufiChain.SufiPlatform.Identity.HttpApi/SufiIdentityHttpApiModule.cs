using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Identity;

[DependsOn(
    typeof(global::SufiChain.SufiPlatform.Identity.SufiIdentityApplicationContractsModule),
    typeof(SufiAspNetCoreMvcModule)
)]
public class SufiIdentityHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
        });
    }
}
