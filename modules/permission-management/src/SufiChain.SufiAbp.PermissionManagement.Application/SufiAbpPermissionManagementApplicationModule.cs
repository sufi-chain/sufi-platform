using Volo.Abp.Modularity;
using SufiChain.SufiAbp.PermissionManagement;
using SufiChain.SufiAbp.Ddd;

namespace SufiChain.SufiAbp.PermissionManagement;

[DependsOn(
    typeof(SufiAbpDddApplicationModule),
    typeof(SufiAbpPermissionManagementDomainModule),
    typeof(SufiAbpPermissionManagementApplicationContractsModule)
)]
public class SufiAbpPermissionManagementApplicationModule : AbpModule
{
}
