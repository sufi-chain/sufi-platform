using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.FileManager.AccessControl;

/// <summary>
/// Default no-op <see cref="IUserOrganizationUnitProvider"/>. Returns no OU memberships.
/// Replace in a host that integrates SufiChain.SufiAbp.Identity to enable OU-based grants.
/// </summary>
public class NullUserOrganizationUnitProvider : IUserOrganizationUnitProvider, ITransientDependency
{
    public Task<IReadOnlyList<Guid>> GetOrganizationUnitIdsAsync(
        Guid userId,
        bool includeAncestors = true,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Guid> empty = Array.Empty<Guid>();
        return Task.FromResult(empty);
    }
}
