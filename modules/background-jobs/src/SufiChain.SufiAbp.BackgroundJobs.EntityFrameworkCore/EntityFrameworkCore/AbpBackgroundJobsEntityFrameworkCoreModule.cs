using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

using Volo.Abp.EntityFrameworkCore;
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
            options.AddRepository<BackgroundJobRecord, EfCoreBackgroundJobRepository>();
        });
    }
}
