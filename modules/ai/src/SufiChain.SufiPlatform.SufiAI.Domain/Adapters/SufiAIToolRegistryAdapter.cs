using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Adapters;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ISufiAIToolRegistry))]
public class SufiAIToolRegistryAdapter : ISufiAIToolRegistry, ISingletonDependency
{
    protected IMCPToolRegistry ToolRegistry { get; }

    public SufiAIToolRegistryAdapter(IMCPToolRegistry toolRegistry)
    {
        ToolRegistry = toolRegistry;
    }

    public virtual async Task<List<ISufiAITool>> GetToolsForWorkspaceAsync(
        string workspaceName,
        CancellationToken cancellationToken = default)
    {
        var tools = await ToolRegistry.GetToolsForWorkspaceAsync(workspaceName, cancellationToken);
        return tools.Select(tool => (ISufiAITool)new SufiAIToolAdapter(tool)).ToList();
    }

    public virtual async Task<ISufiAITool?> GetToolAsync(
        string workspaceName,
        string toolName,
        CancellationToken cancellationToken = default)
    {
        var tool = await ToolRegistry.GetToolAsync(workspaceName, toolName, cancellationToken);
        return tool == null ? null : new SufiAIToolAdapter(tool);
    }

    public virtual Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        return ToolRegistry.RefreshAsync(cancellationToken);
    }
}
