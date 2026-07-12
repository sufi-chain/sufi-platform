using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Settings.MongoDB;

[DependsOn(
    typeof(SufiSettingsDomainModule),
    typeof(AbpMongoDbModule)
    )]
public class SufiSettingsMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<SettingsMongoDbContext>(options =>
        {
            options.AddDefaultRepositories<ISettingsMongoDbContext>();

            options.AddRepository<Setting, MongoSettingRepository>();
            options.AddRepository<SettingDefinitionRecord, MongoSettingDefinitionRecordRepository>();
        });
    }
}
