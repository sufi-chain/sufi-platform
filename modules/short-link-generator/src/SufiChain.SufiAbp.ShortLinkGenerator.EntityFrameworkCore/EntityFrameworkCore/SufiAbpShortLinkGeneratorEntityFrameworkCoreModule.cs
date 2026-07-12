using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.ShortLinkGenerator.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpShortLinkGeneratorDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiAbpShortLinkGeneratorEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<ShortLinkGeneratorDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });
    }
}


