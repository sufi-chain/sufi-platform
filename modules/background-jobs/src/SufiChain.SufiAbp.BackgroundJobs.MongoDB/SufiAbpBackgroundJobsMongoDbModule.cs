using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.BackgroundJobs.MongoDB;

[DependsOn(
    typeof(SufiAbpBackgroundJobsDomainModule),
    typeof(SufiAbpMongoDbModule)
    )]
public class SufiAbpBackgroundJobsMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<BackgroundJobsMongoDbContext>(options =>
        {
            options.AddRepository<BackgroundJobRecord, MongoBackgroundJobRepository>();
        });
    }
}
