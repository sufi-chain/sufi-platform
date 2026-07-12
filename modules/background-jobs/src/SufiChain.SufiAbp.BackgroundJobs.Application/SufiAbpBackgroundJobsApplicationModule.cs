using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.Ddd;

namespace SufiChain.SufiAbp.BackgroundJobs;

[DependsOn(
    typeof(SufiAbpBackgroundJobsApplicationContractsModule),
    typeof(SufiAbpDddApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(SufiAbpBackgroundJobsDomainModule)
)]
public class SufiAbpBackgroundJobsApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiAbpBackgroundJobsApplicationModule>();
    }
}
