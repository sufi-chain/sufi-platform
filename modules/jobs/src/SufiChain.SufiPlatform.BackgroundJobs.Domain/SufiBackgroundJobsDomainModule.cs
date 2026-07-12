using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

using Volo.Abp.Mapperly;
using Volo.Abp.BackgroundJobs;
namespace SufiChain.SufiPlatform.BackgroundJobs;

[DependsOn(
    typeof(SufiBackgroundJobsDomainSharedModule),
    typeof(AbpBackgroundJobsModule),
    typeof(AbpMapperlyModule)
    )]
public class SufiBackgroundJobsDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiBackgroundJobsDomainModule>();
     
    }
}
