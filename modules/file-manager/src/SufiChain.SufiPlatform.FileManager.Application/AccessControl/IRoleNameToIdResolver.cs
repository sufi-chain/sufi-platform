using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.FileManager.AccessControl;

/// <summary>
/// Resolves role ids from role names (since <c>ICurrentUser.Roles</c> carries names,
/// while <see cref="SufiChain.SufiPlatform.FileManager.FileFolders.FolderPermission.RoleId"/>
/// stores a Guid). The default implementation returns an empty set; hosts integrating
/// SufiChain.SufiPlatform.Identity should replace it with a real lookup.
/// </summary>
public interface IRoleNameToIdResolver
{
    Task<IReadOnlyList<Guid>> ResolveAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken = default);
}
