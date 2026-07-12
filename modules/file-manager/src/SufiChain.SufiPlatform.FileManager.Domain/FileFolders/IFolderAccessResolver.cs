using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.FileManager.FileFolders;

/// <summary>
/// Resolves whether the current user has a given permission level on a folder,
/// applying owner/admin/host bypass rules, explicit grants (user/role/OU),
/// tenant isolation, and inherited grants up the parent chain.
/// </summary>
public interface IFolderAccessResolver
{
    /// <summary>
    /// Evaluates access for a single folder, walking the parent chain for
    /// inherited grants where needed.
    /// </summary>
    /// <param name="folder">The folder with its <see cref="FileFolder.Permissions"/> loaded.</param>
    /// <param name="context">The current-user access context.</param>
    /// <param name="requiredLevel">The level being requested.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> HasPermissionAsync(
        FileFolder folder,
        FolderAccessContext context,
        FolderPermissionLevel requiredLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates access for a preloaded folder chain (root → ... → target folder),
    /// avoiding extra DB round-trips when the chain is already available (e.g. tree builds).
    /// The last element is the target folder; ancestors precede it in root-first order.
    /// </summary>
    bool HasPermission(
        IReadOnlyList<FileFolder> folderChain,
        FolderAccessContext context,
        FolderPermissionLevel requiredLevel);
}
