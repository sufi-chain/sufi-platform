using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI;

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
    /// What the tool does to data. Defaults to <see cref="AgentActionEffect.Write"/> so a tool that
    /// does not say is fail-closed (sufi-chain#312). Attribute-discovered tools resolve
    /// <see cref="SufiAiMcpToolAttribute.Effect"/> at registration.
    /// </summary>
    AgentActionEffect Effect => AgentActionEffect.Write;

    /// <summary>
    /// Executes the tool with the given parameters in a workspace context.
    /// Write-capable tools must enforce their module's permissions.
    /// </summary>
    Task<SufiAIToolExecutionResult> ExecuteAsync(
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);
}
