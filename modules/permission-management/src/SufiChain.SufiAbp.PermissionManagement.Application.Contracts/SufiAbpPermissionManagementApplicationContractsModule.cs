using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.PermissionManagement;

[DependsOn(
    typeof(SufiAbpDddApplicationContractsModule),
    typeof(SufiAbpPermissionManagementDomainSharedModule)
)]
public class SufiAbpPermissionManagementApplicationContractsModule : AbpModule
{
}
