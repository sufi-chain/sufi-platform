using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Cache;

/// <summary>
/// Read-only tool backed by a cached descriptor. Catalog/listing retrieval never needs live
/// transport I/O. Execution must go through <see cref="IMCPToolRegistry.ResolveAsync"/> to obtain
/// an executable tool instance.
/// </summary>
public class CachedMCPTool : IMCPTool
{
    public CachedMCPTool(MCPToolDescriptor descriptor)
    {
        Name = descriptor.Name;
        Description = descriptor.Description;
        ParameterSchema = descriptor.ParameterSchema;
        ToolType = descriptor.ToolType;
        Source = descriptor.Source;
    }

    public string Name { get; }
    public string Description { get; }
    public string ParameterSchema { get; }
    public MCPToolType ToolType { get; }
    public string Source { get; }

    public Task<MCPToolExecutionResult> ExecuteAsync(
        WorkspaceContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        throw new BusinessException("AI:MCPToolNotExecutableFromCache")
            .WithData("ToolName", Name);
    }
}
