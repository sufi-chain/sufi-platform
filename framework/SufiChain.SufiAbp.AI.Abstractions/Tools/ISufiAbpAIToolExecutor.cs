using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Executes AI tools with proper context, validation, and auditing.
/// </summary>
public interface ISufiAbpAIToolExecutor
{
    /// <summary>
    /// Executes a tool by name with parameters in a workspace context.
    /// </summary>
    Task<SufiAbpAIToolExecutionResult> ExecuteAsync(
        string workspaceName,
        string toolName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a tool instance directly.
    /// </summary>
    Task<SufiAbpAIToolExecutionResult> ExecuteAsync(
        ISufiAbpAITool tool,
        SufiAbpAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);
}
