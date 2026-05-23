using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;
using SufiChain.SufiAbp.PermissionManagement.MongoDB;
using SufiChain.SufiAbp.MongoDB;

namespace SufiChain.SufiAbp.PermissionManagement.MongoDB;

[DependsOn(
    typeof(SufiAbpPermissionManagementDomainModule),
    typeof(SufiAbpMongoDbModule),
)]
public class SufiAbpPermissionManagementMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Use ABP's default PermissionManagementMongoDbContext and repositories
        // This is a simple wrapper that ensures SufiAbp module dependencies are correct
    }
}
