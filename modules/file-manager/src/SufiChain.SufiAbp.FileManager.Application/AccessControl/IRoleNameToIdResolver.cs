using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.FileManager.AccessControl;

/// <summary>
/// Resolves role ids from role names (since <c>ICurrentUser.Roles</c> carries names,
/// while <see cref="SufiChain.SufiAbp.FileManager.FileFolders.FolderPermission.RoleId"/>
/// stores a Guid). The default implementation returns an empty set; hosts integrating
/// SufiChain.SufiAbp.Identity should replace it with a real lookup.
/// </summary>
public interface IRoleNameToIdResolver
{
    Task<IReadOnlyList<Guid>> ResolveAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken = default);
}
