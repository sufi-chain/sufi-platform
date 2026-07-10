using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI.Data;

/// <summary>
/// Enables MCP tools on the default AI workspace without exposing workspace persistence to other modules.
/// </summary>
public interface IWorkspaceMcpToolBootstrapper : ITransientDependency
{
    /// <summary>
    /// Merges the given tool names into the tenant default workspace enabled-tool list.
    /// </summary>
    Task EnableToolsOnDefaultWorkspaceAsync(
        IReadOnlyList<string> toolNames,
        CancellationToken cancellationToken = default);
}
