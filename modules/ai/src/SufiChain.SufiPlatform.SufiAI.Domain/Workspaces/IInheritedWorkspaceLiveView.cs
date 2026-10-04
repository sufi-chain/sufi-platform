using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

/// <summary>
/// Replaces an inherited projection's model routes with the host source at read time.
/// </summary>
public interface IInheritedWorkspaceLiveView
{
    IDisposable Suppress();

    Task<Workspace> ApplyAsync(
        Workspace workspace,
        bool includeDetails,
        CancellationToken cancellationToken = default);

    Task<List<Workspace>> ApplyAsync(
        IReadOnlyList<Workspace> workspaces,
        CancellationToken cancellationToken = default);
}
