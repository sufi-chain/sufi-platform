using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Execution;

public interface IMcpToolDecisionGate
{
    Task<string?> GetBlockReasonAsync(
        WorkspaceContext context,
        string toolName,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);
}

public class AllowMcpToolDecisionGate : IMcpToolDecisionGate, ITransientDependency
{
    public Task<string?> GetBlockReasonAsync(
        WorkspaceContext context,
        string toolName,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(null);
    }
}
