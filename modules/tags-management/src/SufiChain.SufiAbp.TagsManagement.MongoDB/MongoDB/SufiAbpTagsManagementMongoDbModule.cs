using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.TagsManagement.MongoDB;

[DependsOn(
    typeof(SufiAbpTagsManagementDomainModule),
    typeof(SufiAbpMongoDbModule)
)]
public class SufiAbpTagsManagementMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<TagsManagementMongoDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Tags.Tag, Repositories.MongoTagRepository>();
            options.AddRepository<Tags.TagLink, Repositories.MongoTagLinkRepository>();
        });
    }
}

