using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Cache;

/// <summary>
/// Long-lived distributed cache of the MCP tool/server catalog.
/// Warm at module initialization; invalidated only when servers are added/updated/deleted.
/// </summary>
public interface IMCPCatalogCache
{
    Task<MCPCatalogCacheItem> GetCatalogAsync(CancellationToken cancellationToken = default);
    Task RebuildAsync(CancellationToken cancellationToken = default);
    Task InvalidateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the cached tools of one server. Other servers stay as they are.
    /// </summary>
    Task ReplaceServerToolsAsync(
        string serverKey,
        IReadOnlyList<MCPToolDescriptor> tools,
        CancellationToken cancellationToken = default);
}
