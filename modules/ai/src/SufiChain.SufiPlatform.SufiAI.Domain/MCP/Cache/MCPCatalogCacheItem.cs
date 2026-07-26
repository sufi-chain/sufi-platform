using System;
using System.Collections.Generic;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Cache;

[CacheName("SufiAI-MCPCatalog")]
public class MCPCatalogCacheItem
{
    public List<MCPToolDescriptor> InternalTools { get; set; } = new();
    public List<MCPToolDescriptor> ExternalTools { get; set; } = new();
    public List<MCPServerSnapshot> Servers { get; set; } = new();
    public DateTime BuiltAtUtc { get; set; }
}

public class MCPToolDescriptor
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ParameterSchema { get; set; } = "{}";
    public MCPToolType ToolType { get; set; }
    public string Source { get; set; } = string.Empty;
}

public class MCPServerSnapshot
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TransportType { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}
