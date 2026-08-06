using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Editions.MongoDB.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Editions.MongoDB;

[DependsOn(
    typeof(SufiEditionsDomainModule),
    typeof(AbpMongoDbModule)
)]
public class SufiEditionsMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<EditionsMongoDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Edition, MongoEditionRepository>();
        });
    }
}
