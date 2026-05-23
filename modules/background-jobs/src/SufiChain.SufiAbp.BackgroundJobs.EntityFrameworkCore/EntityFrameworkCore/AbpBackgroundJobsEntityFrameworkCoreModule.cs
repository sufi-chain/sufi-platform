using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.BackgroundJobs.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpBackgroundJobsDomainModule),
    typeof(SufiAbpEntityFrameworkCoreModule)
)]
public class SufiAbpBackgroundJobsEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<BackgroundJobsDbContext>(options =>
        {
            options.AddRepository<BackgroundJobRecord, EfCoreBackgroundJobRepository>();
        });
    }
}
