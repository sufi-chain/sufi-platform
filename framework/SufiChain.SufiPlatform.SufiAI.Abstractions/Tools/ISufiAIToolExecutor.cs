using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Executes AI tools with proper context, validation, and auditing.
/// </summary>
public interface ISufiAIToolExecutor
{
    /// <summary>
    /// Executes a tool by name with parameters in a workspace context.
    /// </summary>
    Task<SufiAIToolExecutionResult> ExecuteAsync(
        string workspaceName,
        string toolName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a tool instance directly.
    /// </summary>
    Task<SufiAIToolExecutionResult> ExecuteAsync(
        ISufiAITool tool,
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);
}
