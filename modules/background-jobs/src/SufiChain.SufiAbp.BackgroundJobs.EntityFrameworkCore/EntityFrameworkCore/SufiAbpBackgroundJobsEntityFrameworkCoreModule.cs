using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.BackgroundJobs.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpBackgroundJobsDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiAbpBackgroundJobsEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<BackgroundJobsDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            
            options.AddRepository<BackgroundJobRecord, EfCoreBackgroundJobRepository>();
        });
    }
}
