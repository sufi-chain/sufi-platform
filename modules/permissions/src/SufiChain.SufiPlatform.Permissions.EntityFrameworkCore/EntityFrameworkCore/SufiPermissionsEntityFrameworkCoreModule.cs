using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

using Volo.Abp.EntityFrameworkCore;
namespace SufiChain.SufiPlatform.Permissions.EntityFrameworkCore;

[DependsOn(
    typeof(SufiPermissionsDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiPermissionsEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<PermissionsDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);

            options.AddRepository<PermissionGroupDefinitionRecord, EfCorePermissionGroupDefinitionRecordRepository>();
            options.AddRepository<PermissionDefinitionRecord, EfCorePermissionDefinitionRecordRepository>();
            options.AddRepository<PermissionGrant, EfCorePermissionGrantRepository>();
            options.AddRepository<ResourcePermissionGrant, EfCoreResourcePermissionGrantRepository>();
        });
    }
}
