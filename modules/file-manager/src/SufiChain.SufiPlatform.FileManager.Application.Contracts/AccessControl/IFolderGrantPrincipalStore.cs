using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.FileManager.FileFolders;

namespace SufiChain.SufiPlatform.FileManager.AccessControl;

/// <summary>
/// Checks that a folder grant's user, role, or organization unit exists in the
/// current tenant, and fills display names for the Access table.
/// The file-manager module ships a no-op; Identity replaces it.
/// </summary>
public interface IFolderGrantPrincipalStore
{
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> RoleExistsAsync(Guid roleId, CancellationToken cancellationToken = default);

    Task<bool> OrganizationUnitExistsAsync(Guid organizationUnitId, CancellationToken cancellationToken = default);

    Task PopulateAsync(IList<FolderPermissionDto> permissions, CancellationToken cancellationToken = default);
}
