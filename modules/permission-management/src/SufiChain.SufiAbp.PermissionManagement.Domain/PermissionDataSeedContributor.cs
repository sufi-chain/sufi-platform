using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Roles;

namespace SufiChain.SufiAbp.PermissionManagement;

public class PermissionDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    protected ICurrentTenant CurrentTenant { get; }
    protected IPermissionDefinitionManager PermissionDefinitionManager { get; }
    protected IPermissionDataSeeder PermissionDataSeeder { get; }
    protected IStaticPermissionSaver StaticPermissionSaver { get; }

    public PermissionDataSeedContributor(
        IPermissionDefinitionManager permissionDefinitionManager,
        IPermissionDataSeeder permissionDataSeeder,
        IStaticPermissionSaver staticPermissionSaver,
        ICurrentTenant currentTenant)
    {
        PermissionDefinitionManager = permissionDefinitionManager;
        PermissionDataSeeder = permissionDataSeeder;
        StaticPermissionSaver = staticPermissionSaver;
        CurrentTenant = currentTenant;
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        await StaticPermissionSaver.SaveAsync();

        var multiTenancySide = CurrentTenant.GetMultiTenancySide();
        var permissionNames = (await PermissionDefinitionManager.GetPermissionsAsync())
            .Where(p => p.MultiTenancySide.HasFlag(multiTenancySide))
            .Where(p => !p.Providers.Any() || p.Providers.Contains(RolePermissionValueProvider.ProviderName))
            .Select(p => p.Name)
            .ToArray();

        await PermissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            AbpRoleConsts.AdminRoleName,
            permissionNames,
            context?.TenantId
        );
    }
}
