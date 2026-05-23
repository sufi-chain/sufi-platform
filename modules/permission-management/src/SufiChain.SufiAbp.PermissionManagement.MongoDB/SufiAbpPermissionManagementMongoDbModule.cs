using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.PermissionManagement.MongoDB;

[DependsOn(
    typeof(SufiAbpPermissionManagementDomainModule),
    typeof(SufiAbpMongoDbModule)
    )]
public class SufiAbpPermissionManagementMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<PermissionManagementMongoDbContext>(options =>
        {
            options.AddDefaultRepositories<IPermissionManagementMongoDbContext>();

            options.AddRepository<PermissionGroupDefinitionRecord, MongoPermissionGroupDefinitionRecordRepository>();
            options.AddRepository<PermissionDefinitionRecord, MongoPermissionDefinitionRecordRepository>();
            options.AddRepository<PermissionGrant, MongoPermissionGrantRepository>();
            options.AddRepository<ResourcePermissionGrant, MongoResourcePermissionGrantRepository>();
        });
    }
}
