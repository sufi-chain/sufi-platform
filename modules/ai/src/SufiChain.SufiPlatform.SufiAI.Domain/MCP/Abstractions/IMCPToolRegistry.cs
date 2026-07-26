using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

/// <summary>
/// Tenant-aware registry for internal tools and tenant-owned external MCP servers.
/// </summary>
public interface IMCPToolRegistry
{
    /// <summary>
    /// Get catalog descriptors for attribute-registered internal tools as non-executable
    /// <see cref="CachedMCPTool"/> stubs. Use <see cref="ResolveAsync"/> for execution.
    /// </summary>
    Task<List<IMCPTool>> GetInternalToolsAsync(
        CancellationToken cancellationToken = default);

    Task<List<IMCPTool>> GetCatalogAsync(CancellationToken cancellationToken = default);
    
    Task<MCPToolResolutionResult> ResolveAsync(
        IReadOnlyCollection<string> toolNames,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Refresh the tool registry (re-scan internal tools, reconnect external servers).
    /// </summary>
    Task RefreshAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts a lightweight connection test for an MCP server without persisting the client in the registry cache.
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> TestServerConnectionAsync(
        MCPServer server,
        CancellationToken cancellationToken = default);
}

public class MCPToolResolutionResult
{
    public List<IMCPTool> Tools { get; set; } = new();
    public List<MCPToolResolutionDiagnostic> Diagnostics { get; set; } = new();
}

public class MCPToolResolutionDiagnostic
{
    public string ToolName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
