using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.BackgroundJobs.EntityFrameworkCore;

[DependsOn(
    typeof(SufiBackgroundJobsDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiBackgroundJobsEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<BackgroundJobsDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            
            options.AddRepository<BackgroundJobRecord, EfCoreBackgroundJobRepository>();
        });

        context.Services.TryAddScoped<ISufiBackgroundJobsDbContext>(serviceProvider =>
            serviceProvider.GetRequiredService<BackgroundJobsDbContext>());
    }
}
