using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Tags.MongoDB;

[DependsOn(
    typeof(SufiTagsDomainModule),
    typeof(AbpMongoDbModule)
)]
public class SufiTagsMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<TagsMongoDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Tags.Tag, Repositories.MongoTagRepository>();
            options.AddRepository<Tags.TagLink, Repositories.MongoTagLinkRepository>();
        });
    }
}