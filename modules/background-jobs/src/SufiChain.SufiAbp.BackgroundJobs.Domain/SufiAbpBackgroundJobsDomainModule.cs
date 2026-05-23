using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Mapperly;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.BackgroundJobs;

[DependsOn(
    typeof(SufiAbpBackgroundJobsDomainSharedModule),
    typeof(SufiAbpBackgroundJobsModule),
    typeof(SufiAbpMapperlyModule)
    )]
public class SufiAbpBackgroundJobsDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiAbpBackgroundJobsDomainModule>();
     
    }
}
