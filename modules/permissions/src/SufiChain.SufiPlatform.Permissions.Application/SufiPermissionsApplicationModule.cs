using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.Permissions;
using SufiChain.SufiPlatform.Ddd;

namespace SufiChain.SufiPlatform.Permissions;

[DependsOn(
    typeof(SufiDddApplicationModule),
    typeof(SufiPermissionsDomainModule),
    typeof(SufiPermissionsApplicationContractsModule)
)]
public class SufiPermissionsApplicationModule : AbpModule
{
}
