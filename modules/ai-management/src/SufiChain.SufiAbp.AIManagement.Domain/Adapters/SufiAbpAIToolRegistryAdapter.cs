using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AIManagement.MCP.Abstractions;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AIManagement.Adapters;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ISufiAbpAIToolRegistry))]
public class SufiAbpAIToolRegistryAdapter : ISufiAbpAIToolRegistry, ISingletonDependency
{
    protected IMCPToolRegistry ToolRegistry { get; }

    public SufiAbpAIToolRegistryAdapter(IMCPToolRegistry toolRegistry)
    {
        ToolRegistry = toolRegistry;
    }

    public virtual async Task<List<ISufiAbpAITool>> GetToolsForWorkspaceAsync(
        string workspaceName,
        CancellationToken cancellationToken = default)
    {
        var tools = await ToolRegistry.GetToolsForWorkspaceAsync(workspaceName, cancellationToken);
        return tools.Select(tool => (ISufiAbpAITool)new SufiAbpAIToolAdapter(tool)).ToList();
    }

    public virtual async Task<ISufiAbpAITool?> GetToolAsync(
        string workspaceName,
        string toolName,
        CancellationToken cancellationToken = default)
    {
        var tool = await ToolRegistry.GetToolAsync(workspaceName, toolName, cancellationToken);
        return tool == null ? null : new SufiAbpAIToolAdapter(tool);
    }

    public virtual Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        return ToolRegistry.RefreshAsync(cancellationToken);
    }
}
