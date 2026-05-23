using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.ShortLinkGenerator.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpShortLinkGeneratorDomainModule),
    typeof(SufiAbpEntityFrameworkCoreModule)
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


