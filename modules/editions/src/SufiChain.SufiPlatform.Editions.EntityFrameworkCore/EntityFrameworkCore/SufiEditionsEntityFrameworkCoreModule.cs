using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Editions.Repositories;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Editions.EntityFrameworkCore;

[DependsOn(
    typeof(SufiEditionsDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiEditionsEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<EditionsDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Edition, EfCoreEditionRepository>();
        });
    }
}
