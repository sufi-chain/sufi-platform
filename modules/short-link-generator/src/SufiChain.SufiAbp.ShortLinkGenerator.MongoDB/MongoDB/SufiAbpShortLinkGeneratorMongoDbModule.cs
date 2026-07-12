using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.ShortLinkGenerator.MongoDB.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.ShortLinkGenerator.MongoDB;

[DependsOn(
    typeof(SufiAbpShortLinkGeneratorDomainModule),
    typeof(AbpMongoDbModule)
    )]
public class SufiAbpShortLinkGeneratorMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<ShortLinkGeneratorMongoDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });
    }
}


