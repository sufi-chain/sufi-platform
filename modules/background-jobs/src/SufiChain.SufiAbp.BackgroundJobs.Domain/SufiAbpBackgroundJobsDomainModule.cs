using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

using Volo.Abp.Mapperly;
using Volo.Abp.BackgroundJobs;
namespace SufiChain.SufiAbp.BackgroundJobs;

[DependsOn(
    typeof(SufiAbpBackgroundJobsDomainSharedModule),
    typeof(AbpBackgroundJobsModule),
    typeof(AbpMapperlyModule)
    )]
public class SufiAbpBackgroundJobsDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiAbpBackgroundJobsDomainModule>();
     
    }
}
