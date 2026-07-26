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

    public virtual async Task<List<ISufiAITool>> GetCatalogAsync(
        CancellationToken cancellationToken = default)
    {
        var tools = await ToolRegistry.GetCatalogAsync(cancellationToken);
        return tools.Select(tool => (ISufiAITool)new SufiAIToolAdapter(tool)).ToList();
    }

    public virtual async Task<List<ISufiAITool>> ResolveAsync(
        IReadOnlyCollection<string> toolNames,
        CancellationToken cancellationToken = default)
    {
        var result = await ToolRegistry.ResolveAsync(toolNames, cancellationToken);
        return result.Tools.Select(tool => (ISufiAITool)new SufiAIToolAdapter(tool)).ToList();
    }

    public virtual Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        return ToolRegistry.RefreshAsync(cancellationToken);
    }
}
