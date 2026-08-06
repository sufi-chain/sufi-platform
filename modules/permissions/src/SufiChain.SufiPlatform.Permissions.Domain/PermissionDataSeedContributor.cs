using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.SimpleStateChecking;

namespace SufiChain.SufiPlatform.Permissions;

public class PermissionDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private const string RolePermissionProviderName =
        SufiChain.SufiPlatform.Authorization.Permissions.RolePermissionValueProvider.ProviderName;

    private const string AdminRoleName = "admin";

    protected ICurrentTenant CurrentTenant { get; }
    protected IPermissionDefinitionManager PermissionDefinitionManager { get; }
    protected IPermissionManager PermissionManager { get; }
    protected ISimpleStateCheckerManager<PermissionDefinition> SimpleStateCheckerManager { get; }

    public PermissionDataSeedContributor(
        IPermissionDefinitionManager permissionDefinitionManager,
        IPermissionManager permissionManager,
        ISimpleStateCheckerManager<PermissionDefinition> simpleStateCheckerManager,
        ICurrentTenant currentTenant)
    {
        PermissionDefinitionManager = permissionDefinitionManager;
        PermissionManager = permissionManager;
        SimpleStateCheckerManager = simpleStateCheckerManager;
        CurrentTenant = currentTenant;
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        var multiTenancySide = CurrentTenant.GetMultiTenancySide();
        var permissions = (await PermissionDefinitionManager.GetPermissionsAsync())
            .Where(p => p.MultiTenancySide.HasFlag(multiTenancySide))
            .Where(p => !p.Providers.Any() || p.Providers.Contains(RolePermissionProviderName))
            .ToList();

        foreach (var permission in permissions)
        {
            if (!permission.IsEnabled)
            {
                continue;
            }

            if (!await SimpleStateCheckerManager.IsEnabledAsync(permission))
            {
                continue;
            }

            await PermissionManager.SetAsync(
                permission.Name,
                RolePermissionProviderName,
                AdminRoleName,
                isGranted: true);
        }
    }
}
