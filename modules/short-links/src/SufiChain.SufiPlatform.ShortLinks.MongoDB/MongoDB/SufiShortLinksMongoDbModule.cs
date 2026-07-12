using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.ShortLinks.MongoDB.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.ShortLinks.MongoDB;

[DependsOn(
    typeof(SufiShortLinksDomainModule),
    typeof(AbpMongoDbModule)
    )]
public class SufiShortLinksMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<ShortLinksMongoDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });
    }
}