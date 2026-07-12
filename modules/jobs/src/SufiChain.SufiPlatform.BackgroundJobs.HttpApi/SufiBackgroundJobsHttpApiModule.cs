using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.BackgroundJobs;

[DependsOn(
    typeof(SufiBackgroundJobsApplicationContractsModule),
    typeof(SufiAspNetCoreMvcModule)
)]
public class SufiBackgroundJobsHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
        });
    }
}
