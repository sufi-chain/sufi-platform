using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

/// <summary>
/// Performs the runtime, fail-closed check for already consumed priced usage.
/// Unknown/unpriced usage is intentionally excluded by the usage repository query.
/// </summary>
public interface IWorkspaceGuardrailService
{
    Task EnsureCanExecuteAsync(Guid workspaceId, CancellationToken cancellationToken = default);

    Task<List<WorkspaceGuardrailStatusDto>> GetStatusAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);
}
