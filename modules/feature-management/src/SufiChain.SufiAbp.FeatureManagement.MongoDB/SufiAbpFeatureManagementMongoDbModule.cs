using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.FeatureManagement.MongoDB;

[DependsOn(
    typeof(SufiAbpFeatureManagementDomainModule),
    typeof(SufiAbpMongoDbModule)
    )]
public class SufiAbpFeatureManagementMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<FeatureManagementMongoDbContext>(options =>
        {
            options.AddDefaultRepositories<IFeatureManagementMongoDbContext>();

            options.AddRepository<FeatureGroupDefinitionRecord, MongoFeatureGroupDefinitionRecordRepository>();
            options.AddRepository<FeatureDefinitionRecord, MongoFeatureDefinitionRecordRepository>();
            options.AddRepository<FeatureValue, MongoFeatureValueRepository>();
        });
    }

}
