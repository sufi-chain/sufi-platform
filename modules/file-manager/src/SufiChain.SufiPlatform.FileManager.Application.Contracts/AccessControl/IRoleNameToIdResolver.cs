using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.FileManager.AccessControl;

/// <summary>
/// Resolves role ids from role names (since <c>ICurrentUser.Roles</c> carries names,
/// while folder grants store a role id). The default implementation returns an empty set;
/// hosts integrating Identity replace it with a real lookup.
/// </summary>
public interface IRoleNameToIdResolver
{
    Task<IReadOnlyList<Guid>> ResolveAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken = default);
}
