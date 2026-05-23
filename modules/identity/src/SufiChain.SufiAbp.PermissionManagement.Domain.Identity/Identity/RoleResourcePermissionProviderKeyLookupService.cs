using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using SufiChain.SufiAbp.Authorization.Permissions;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Identity.Localization;
using Volo.Abp.Localization;

namespace SufiChain.SufiAbp.PermissionManagement.Identity;

public class RoleResourcePermissionProviderKeyLookupService : IResourcePermissionProviderKeyLookupService, ITransientDependency
{
    public string Name => RolePermissionValueProvider.ProviderName;

    public ILocalizableString DisplayName { get; }

    protected IUserRoleFinder UserRoleFinder { get; }

    public RoleResourcePermissionProviderKeyLookupService(IUserRoleFinder userRoleFinder)
    {
        UserRoleFinder = userRoleFinder;
        DisplayName = LocalizableString.Create<SufiAbpIdentityResource>(nameof(RoleResourcePermissionProviderKeyLookupService));
    }

    public virtual Task<bool> IsAvailableAsync()
    {
        return Task.FromResult(true);
    }

    public virtual async Task<List<ResourcePermissionProviderKeyInfo>> SearchAsync(string filter = null, int page = 1, CancellationToken cancellationToken = default)
    {
        var roles = await UserRoleFinder.SearchRoleAsync(filter, page);
        return roles.Select(role => new ResourcePermissionProviderKeyInfo(role.RoleName, role.RoleName)).ToList();
    }

    public virtual Task<List<ResourcePermissionProviderKeyInfo>> SearchAsync(string[] keys, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(keys.Select(key => new ResourcePermissionProviderKeyInfo(key, key)).ToList());
    }
}
