using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Adapters;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ISufiAIToolExecutor))]
public class SufiAIToolExecutorAdapter : ISufiAIToolExecutor, ITransientDependency
{
    protected IMCPToolExecutor ToolExecutor { get; }

    public SufiAIToolExecutorAdapter(IMCPToolExecutor toolExecutor)
    {
        ToolExecutor = toolExecutor;
    }

    public virtual async Task<SufiAIToolExecutionResult> ExecuteAsync(
        string workspaceName,
        string toolName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var result = await ToolExecutor.ExecuteAsync(workspaceName, toolName, parameters, cancellationToken);
        return SufiAIToolAdapter.MapResult(result);
    }

    public virtual async Task<SufiAIToolExecutionResult> ExecuteAsync(
        ISufiAITool tool,
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        if (tool is SufiAIToolAdapter adapter)
        {
            var result = await ToolExecutor.ExecuteAsync(
                adapter.InnerTool,
                SufiAIToolAdapter.MapContext(context),
                parameters,
                cancellationToken);

            return SufiAIToolAdapter.MapResult(result);
        }

        return await tool.ExecuteAsync(context, parameters, cancellationToken);
    }
}
