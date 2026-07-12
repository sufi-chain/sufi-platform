using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Permissions.MongoDB;

[DependsOn(
    typeof(SufiPermissionsDomainModule),
    typeof(AbpMongoDbModule)
    )]
public class SufiPermissionsMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<PermissionsMongoDbContext>(options =>
        {
            options.AddDefaultRepositories<IPermissionsMongoDbContext>();

            options.AddRepository<PermissionGroupDefinitionRecord, MongoPermissionGroupDefinitionRecordRepository>();
            options.AddRepository<PermissionDefinitionRecord, MongoPermissionDefinitionRecordRepository>();
            options.AddRepository<PermissionGrant, MongoPermissionGrantRepository>();
            options.AddRepository<ResourcePermissionGrant, MongoResourcePermissionGrantRepository>();
        });
    }
}
