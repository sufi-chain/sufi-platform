using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.FileManager.AccessControl;

/// <summary>
/// Default no-op <see cref="IRoleNameToIdResolver"/>. Returns no role ids, so role-by-id
/// grants do not resolve until a host provides a real implementation. Admin-role detection
/// by name still works without this.
/// </summary>
public class NullRoleNameToIdResolver : IRoleNameToIdResolver, ITransientDependency
{
    public Task<IReadOnlyList<Guid>> ResolveAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Guid> empty = Array.Empty<Guid>();
        return Task.FromResult(empty);
    }
}
