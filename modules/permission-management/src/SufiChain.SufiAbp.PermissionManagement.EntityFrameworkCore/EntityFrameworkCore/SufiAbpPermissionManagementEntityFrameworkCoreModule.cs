using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

using Volo.Abp.EntityFrameworkCore;
namespace SufiChain.SufiAbp.PermissionManagement.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpPermissionManagementDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiAbpPermissionManagementEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<PermissionManagementDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);

            options.AddRepository<PermissionGroupDefinitionRecord, EfCorePermissionGroupDefinitionRecordRepository>();
            options.AddRepository<PermissionDefinitionRecord, EfCorePermissionDefinitionRecordRepository>();
            options.AddRepository<PermissionGrant, EfCorePermissionGrantRepository>();
            options.AddRepository<ResourcePermissionGrant, EfCoreResourcePermissionGrantRepository>();
        });
    }
}
