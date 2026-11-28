using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace SufiChain.SufiPlatform.FileManager.AccessControl;

/// <summary>
/// Default no-op <see cref="IRoleNameToIdResolver"/>. Returns no role ids, so role-by-id
/// grants do not resolve until Identity replaces it. Admin-role detection by name still
/// works without this. Registered with TryAdd so the Identity resolver wins whichever
/// module loads first.
/// </summary>
public class NullRoleNameToIdResolver : IRoleNameToIdResolver
{
    public Task<IReadOnlyList<Guid>> ResolveAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Guid> empty = Array.Empty<Guid>();
        return Task.FromResult(empty);
    }
}
