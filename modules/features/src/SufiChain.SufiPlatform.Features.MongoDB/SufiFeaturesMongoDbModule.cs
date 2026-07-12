using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Features.MongoDB;

[DependsOn(
    typeof(SufiFeaturesDomainModule),
    typeof(AbpMongoDbModule)
    )]
public class SufiFeaturesMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<FeaturesMongoDbContext>(options =>
        {
            options.AddDefaultRepositories<IFeaturesMongoDbContext>();

            options.AddRepository<FeatureGroupDefinitionRecord, MongoFeatureGroupDefinitionRecordRepository>();
            options.AddRepository<FeatureDefinitionRecord, MongoFeatureDefinitionRecordRepository>();
            options.AddRepository<FeatureValue, MongoFeatureValueRepository>();
        });
    }

}
