using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Null fallback used when no tool provider module is installed: no tools exist.
/// </summary>
[Dependency(TryRegister = true)]
[ExposeServices(typeof(ISufiAbpAIToolRegistry))]
public class NullSufiAbpAIToolRegistry : ISufiAbpAIToolRegistry, ITransientDependency
{
    /// <inheritdoc />
    public virtual Task<List<ISufiAbpAITool>> GetToolsForWorkspaceAsync(
        string workspaceName,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<ISufiAbpAITool>());
    }

    /// <inheritdoc />
    public virtual Task<ISufiAbpAITool?> GetToolAsync(
        string workspaceName,
        string toolName,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ISufiAbpAITool?>(null);
    }

    /// <inheritdoc />
    public virtual Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

/// <summary>
/// Null fallback used when no tool provider module is installed: execution fails
/// with a tool-not-found result instead of throwing.
/// </summary>
[Dependency(TryRegister = true)]
[ExposeServices(typeof(ISufiAbpAIToolExecutor))]
public class NullSufiAbpAIToolExecutor : ISufiAbpAIToolExecutor, ITransientDependency
{
    /// <inheritdoc />
    public virtual Task<SufiAbpAIToolExecutionResult> ExecuteAsync(
        string workspaceName,
        string toolName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            SufiAbpAIToolExecutionResult.CreateFailure(SufiAbpAIErrorCodes.ProviderNotAvailable));
    }

    /// <inheritdoc />
    public virtual Task<SufiAbpAIToolExecutionResult> ExecuteAsync(
        ISufiAbpAITool tool,
        SufiAbpAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            SufiAbpAIToolExecutionResult.CreateFailure(SufiAbpAIErrorCodes.ProviderNotAvailable));
    }
}
