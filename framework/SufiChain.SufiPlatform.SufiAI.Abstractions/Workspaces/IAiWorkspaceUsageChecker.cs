using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Reports live bindings that should block deletion of an AI workspace.
/// Historical logs and owned child rows are not usages.
/// </summary>
public interface IAiWorkspaceUsageChecker
{
    Task<IReadOnlyList<AiWorkspaceUsage>> FindActiveUsagesAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);
}

public static class AiWorkspaceUsageRelations
{
    public const string Hooshvare = "WorkspaceUsage:Hooshvare";

    public const string Assignment = "WorkspaceUsage:Assignment";

    public const string InheritedWorkspace = "WorkspaceUsage:InheritedWorkspace";
}

public class AiWorkspaceUsage
{
    public string RelationKey { get; set; } = string.Empty;

    public int ActiveCount { get; set; }
}
