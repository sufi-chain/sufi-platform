using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.FileManager.FileFolders;

/// <summary>
/// Default <see cref="IFolderAccessResolver"/>. Encapsulates the folder access policy:
/// host bypass → admin role → owner → explicit user/role/OU grant (with inheritance up the
/// parent chain). Tenant isolation is enforced first.
/// </summary>
public class FolderAccessResolver : IFolderAccessResolver, ITransientDependency
{
    private readonly IFileFolderRepository _folderRepository;

    public FolderAccessResolver(IFileFolderRepository folderRepository)
    {
        _folderRepository = folderRepository;
    }

    public async Task<bool> HasPermissionAsync(
        FileFolder folder,
        FolderAccessContext context,
        FolderPermissionLevel requiredLevel,
        CancellationToken cancellationToken = default)
    {
        if (folder == null)
        {
            return false;
        }

        var chain = await BuildChainAsync(folder, cancellationToken);
        return HasPermission(chain, context, requiredLevel);
    }

    public bool HasPermission(
        IReadOnlyList<FileFolder> folderChain,
        FolderAccessContext context,
        FolderPermissionLevel requiredLevel)
    {
        if (folderChain == null || folderChain.Count == 0)
        {
            return false;
        }

        // Anonymous callers never get folder access through this path.
        // (Public file share-tokens are validated on a separate, file-scoped path.)
        if (context.IsAnonymous)
        {
            return false;
        }

        // The target folder is the last element in the root-first chain.
        var target = folderChain[folderChain.Count - 1];

        // Tenant isolation: a tenant user cannot touch another tenant's folder.
        // Host users (no tenant) bypass tenant checks.
        if (!context.IsHost && target.TenantId != context.TenantId)
        {
            return false;
        }

        // Host users have full access across tenants.
        if (context.IsHost)
        {
            return true;
        }

        // Admin role grants full access within the caller's own tenant.
        if (context.IsAdmin)
        {
            return true;
        }

        // Owner has full access.
        if (target.CreatorId == context.UserId)
        {
            return true;
        }

        // Walk the chain from the target up to the root, evaluating grants with inheritance.
        for (var i = folderChain.Count - 1; i >= 0; i--)
        {
            var current = folderChain[i];
            var isTarget = i == folderChain.Count - 1;

            foreach (var permission in current.Permissions)
            {
                if (!permission.HasPermission(requiredLevel))
                {
                    continue;
                }

                if (!AppliesTo(permission, context))
                {
                    continue;
                }

                // On the target folder, any matching grant applies directly.
                if (isTarget)
                {
                    return true;
                }

                // On an ancestor, the grant only applies to descendants if it inherits.
                if (permission.InheritToChildren)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool AppliesTo(FolderPermission permission, FolderAccessContext context)
    {
        // User grant
        if (permission.UserId.HasValue && permission.UserId == context.UserId)
        {
            return true;
        }

        // Role grant
        if (permission.RoleId.HasValue)
        {
            for (var i = 0; i < context.RoleIds.Count; i++)
            {
                if (context.RoleIds[i] == permission.RoleId.Value)
                {
                    return true;
                }
            }
        }

        // Organization-unit grant
        if (permission.OrganizationUnitId.HasValue)
        {
            for (var i = 0; i < context.OrganizationUnitIds.Count; i++)
            {
                if (context.OrganizationUnitIds[i] == permission.OrganizationUnitId.Value)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private async Task<List<FileFolder>> BuildChainAsync(FileFolder folder, CancellationToken cancellationToken)
    {
        // Root-first chain including the target folder.
        var chain = new List<FileFolder>();
        var current = folder;

        while (current != null)
        {
            // Ensure permissions are loaded for the node we are evaluating.
            if (current.Permissions == null || current.Permissions.Count == 0)
            {
                var withPermissions = await _folderRepository.GetWithPermissionsAsync(current.Id, cancellationToken);
                if (withPermissions != null)
                {
                    current = withPermissions;
                }
            }

            chain.Insert(0, current);

            if (!current.ParentId.HasValue)
            {
                break;
            }

            current = await _folderRepository.GetAsync(current.ParentId.Value, cancellationToken: cancellationToken);
        }

        return chain;
    }
}
