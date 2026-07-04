using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.FileManager.AccessControl;

/// <summary>
/// Resolves the organization-unit ids that a user belongs to (optionally including
/// ancestor OUs). The file-manager module ships a default no-op implementation
/// (returns an empty set); hosts integrating SufiChain.SufiAbp.Identity should
/// replace it with a real implementation backed by <c>IOrganizationUnitAppService</c>.
/// </summary>
public interface IUserOrganizationUnitProvider
{
    /// <summary>
    /// Returns the organization-unit ids for the given user, including ancestor units
    /// when <paramref name="includeAncestors"/> is true.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetOrganizationUnitIdsAsync(
        Guid userId,
        bool includeAncestors = true,
        CancellationToken cancellationToken = default);
}
