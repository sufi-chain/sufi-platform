using SufiChain.SufiPlatform.Ddd;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Permissions;

[DependsOn(
    typeof(SufiDddApplicationContractsModule),
    typeof(SufiPermissionsDomainSharedModule)
)]
public class SufiPermissionsApplicationContractsModule : AbpModule
{
}
