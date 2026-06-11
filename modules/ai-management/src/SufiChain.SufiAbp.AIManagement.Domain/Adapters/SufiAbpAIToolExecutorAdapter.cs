using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AIManagement.MCP.Abstractions;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AIManagement.Adapters;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ISufiAbpAIToolExecutor))]
public class SufiAbpAIToolExecutorAdapter : ISufiAbpAIToolExecutor, ITransientDependency
{
    protected IMCPToolExecutor ToolExecutor { get; }

    public SufiAbpAIToolExecutorAdapter(IMCPToolExecutor toolExecutor)
    {
        ToolExecutor = toolExecutor;
    }

    public virtual async Task<SufiAbpAIToolExecutionResult> ExecuteAsync(
        string workspaceName,
        string toolName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var result = await ToolExecutor.ExecuteAsync(workspaceName, toolName, parameters, cancellationToken);
        return SufiAbpAIToolAdapter.MapResult(result);
    }

    public virtual async Task<SufiAbpAIToolExecutionResult> ExecuteAsync(
        ISufiAbpAITool tool,
        SufiAbpAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        if (tool is SufiAbpAIToolAdapter adapter)
        {
            var result = await ToolExecutor.ExecuteAsync(
                adapter.InnerTool,
                SufiAbpAIToolAdapter.MapContext(context),
                parameters,
                cancellationToken);

            return SufiAbpAIToolAdapter.MapResult(result);
        }

        return await tool.ExecuteAsync(context, parameters, cancellationToken);
    }
}
