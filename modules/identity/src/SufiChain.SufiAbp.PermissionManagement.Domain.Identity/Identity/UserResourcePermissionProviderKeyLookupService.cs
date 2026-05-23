using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Identity.Localization;
using Volo.Abp.Localization;
using SufiChain.SufiAbp.Authorization.Permissions;

namespace SufiChain.SufiAbp.PermissionManagement.Identity;

public class UserResourcePermissionProviderKeyLookupService : IResourcePermissionProviderKeyLookupService, ITransientDependency
{
    public string Name => UserPermissionValueProvider.ProviderName;

    public ILocalizableString DisplayName { get; }

    protected IUserRoleFinder UserRoleFinder { get; }

    public UserResourcePermissionProviderKeyLookupService(IUserRoleFinder userRoleFinder)
    {
        UserRoleFinder = userRoleFinder;
        DisplayName = LocalizableString.Create<SufiAbpIdentityResource>(nameof(UserResourcePermissionProviderKeyLookupService));
    }

    public virtual Task<bool> IsAvailableAsync()
    {
        return Task.FromResult(true);
    }

    public virtual async Task<List<ResourcePermissionProviderKeyInfo>> SearchAsync(string filter = null, int page = 1, CancellationToken cancellationToken = default)
    {
        var users = await UserRoleFinder.SearchUserAsync(filter, page);
        return users.Select(user => new ResourcePermissionProviderKeyInfo(user.Id.ToString(), user.UserName)).ToList();
    }

    public virtual async Task<List<ResourcePermissionProviderKeyInfo>> SearchAsync(string[] keys, CancellationToken cancellationToken = default)
    {
        var userIds = keys
            .Select(key => Guid.TryParse(key, out var id) ? (Guid?)id : null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        var users = await UserRoleFinder.SearchUserByIdsAsync(userIds);
        return users.Select(user => new ResourcePermissionProviderKeyInfo(user.Id.ToString(), user.UserName)).ToList();
    }
}
