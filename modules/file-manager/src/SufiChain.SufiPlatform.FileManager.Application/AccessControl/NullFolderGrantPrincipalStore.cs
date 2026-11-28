using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.FileManager.FileFolders;

namespace SufiChain.SufiPlatform.FileManager.AccessControl;

/// <summary>
/// Default no-op principal store. Grants are accepted and shown without names until
/// Identity replaces this with <c>IdentityFolderGrantPrincipalStore</c>.
/// </summary>
public class NullFolderGrantPrincipalStore : IFolderGrantPrincipalStore
{
    public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task<bool> RoleExistsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task<bool> OrganizationUnitExistsAsync(Guid organizationUnitId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task PopulateAsync(IList<FolderPermissionDto> permissions, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
