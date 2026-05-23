using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.AIManagement.MCP.Abstractions;

/// <summary>
/// Discovers internal MCP tools by scanning ApplicationService methods
/// marked with [MCPTool] attribute.
/// </summary>
public interface IInternalToolDiscoveryService
{
    /// <summary>
    /// Scan all registered services and discover methods marked with [MCPTool].
    /// Results are cached for performance.
    /// </summary>
    Task<List<IMCPTool>> DiscoverToolsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Refresh the tool cache (re-scan all services).
    /// </summary>
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
