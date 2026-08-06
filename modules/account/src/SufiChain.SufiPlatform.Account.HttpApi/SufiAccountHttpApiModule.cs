using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Account;

[DependsOn(
    typeof(SufiAccountApplicationContractsModule),
    typeof(SufiAspNetCoreMvcModule)
)]
public class SufiAccountHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
        });
    }
}
