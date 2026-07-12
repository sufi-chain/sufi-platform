using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.SettingManagement.MongoDB;

[DependsOn(
    typeof(SufiAbpSettingManagementDomainModule),
    typeof(AbpMongoDbModule)
    )]
public class SufiAbpSettingManagementMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<SettingManagementMongoDbContext>(options =>
        {
            options.AddDefaultRepositories<ISettingManagementMongoDbContext>();

            options.AddRepository<Setting, MongoSettingRepository>();
            options.AddRepository<SettingDefinitionRecord, MongoSettingDefinitionRecordRepository>();
        });
    }
}
