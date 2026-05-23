using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using SufiChain.SufiAbp.BackgroundJobs;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.Ddd;
using SufiChain.SufiAbp.Mapperly;

namespace SufiChain.SufiAbp.BackgroundJobs;

[DependsOn(
    typeof(SufiAbpBackgroundJobsApplicationContractsModule),
    typeof(SufiAbpDddApplicationModule),
    typeof(SufiAbpMapperlyModule),
    typeof(SufiAbpBackgroundJobsDomainModule)
)]
public class SufiAbpBackgroundJobsApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiAbpBackgroundJobsApplicationModule>();
    }
}
