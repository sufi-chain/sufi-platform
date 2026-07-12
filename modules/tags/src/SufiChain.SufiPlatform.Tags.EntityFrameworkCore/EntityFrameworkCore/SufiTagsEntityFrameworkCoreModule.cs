using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Tags.EntityFrameworkCore;

[DependsOn(
    typeof(SufiTagsDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiTagsEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<TagsDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Tags.Tag, Repositories.EfCoreTagRepository>();
            options.AddRepository<Tags.TagLink, Repositories.EfCoreTagLinkRepository>();
        });
    }
}