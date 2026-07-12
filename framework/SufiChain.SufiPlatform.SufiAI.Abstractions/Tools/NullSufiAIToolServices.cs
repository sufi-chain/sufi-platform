using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Null fallback used when no tool provider module is installed: no tools exist.
/// </summary>
[Dependency(TryRegister = true)]
[ExposeServices(typeof(ISufiAIToolRegistry))]
public class NullSufiAIToolRegistry : ISufiAIToolRegistry, ITransientDependency
{
    /// <inheritdoc />
    public virtual Task<List<ISufiAITool>> GetToolsForWorkspaceAsync(
        string workspaceName,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<ISufiAITool>());
    }

    /// <inheritdoc />
    public virtual Task<ISufiAITool?> GetToolAsync(
        string workspaceName,
        string toolName,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ISufiAITool?>(null);
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
[ExposeServices(typeof(ISufiAIToolExecutor))]
public class NullSufiAIToolExecutor : ISufiAIToolExecutor, ITransientDependency
{
    /// <inheritdoc />
    public virtual Task<SufiAIToolExecutionResult> ExecuteAsync(
        string workspaceName,
        string toolName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            SufiAIToolExecutionResult.CreateFailure(SufiAIErrorCodes.ProviderNotAvailable));
    }

    /// <inheritdoc />
    public virtual Task<SufiAIToolExecutionResult> ExecuteAsync(
        ISufiAITool tool,
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            SufiAIToolExecutionResult.CreateFailure(SufiAIErrorCodes.ProviderNotAvailable));
    }
}
