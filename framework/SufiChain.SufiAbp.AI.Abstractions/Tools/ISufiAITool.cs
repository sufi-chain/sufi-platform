using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// An LLM-callable tool that product modules can publish without referencing
/// a provider module. Providers (e.g. AI MCP) discover and expose
/// registered tools to AI models.
/// </summary>
public interface ISufiAITool
{
    /// <summary>
    /// Unique tool name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Human-readable description of what the tool does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// JSON schema describing the tool's parameters.
    /// </summary>
    string ParameterSchema { get; }

    /// <summary>
    /// Source identifier (e.g. publishing module or service name).
    /// </summary>
    string Source { get; }

    /// <summary>
    /// Executes the tool with the given parameters in a workspace context.
    /// Write-capable tools must enforce their module's permissions.
    /// </summary>
    Task<SufiAIToolExecutionResult> ExecuteAsync(
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);
}
