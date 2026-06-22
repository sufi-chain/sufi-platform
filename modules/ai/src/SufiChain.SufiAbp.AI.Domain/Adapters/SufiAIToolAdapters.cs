using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AI.MCP.Abstractions;

namespace SufiChain.SufiAbp.AI.Adapters;

public class SufiAIToolAdapter : ISufiAITool
{
    public IMCPTool InnerTool { get; }

    public SufiAIToolAdapter(IMCPTool innerTool)
    {
        InnerTool = innerTool;
    }

    public string Name => InnerTool.Name;

    public string Description => InnerTool.Description;

    public string ParameterSchema => InnerTool.ParameterSchema;

    public string Source => InnerTool.Source;

    public virtual async Task<SufiAIToolExecutionResult> ExecuteAsync(
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var result = await InnerTool.ExecuteAsync(MapContext(context), parameters, cancellationToken);
        return MapResult(result);
    }

    public static WorkspaceContext MapContext(SufiAIToolExecutionContext context)
    {
        return new WorkspaceContext
        {
            WorkspaceName = context.WorkspaceName,
            TenantId = context.TenantId,
            UserId = context.UserId,
            Metadata = context.Metadata
        };
    }

    public static SufiAIToolExecutionContext MapContext(WorkspaceContext context)
    {
        return new SufiAIToolExecutionContext
        {
            WorkspaceName = context.WorkspaceName,
            TenantId = context.TenantId,
            UserId = context.UserId,
            Metadata = context.Metadata
        };
    }

    public static SufiAIToolExecutionResult MapResult(MCPToolExecutionResult result)
    {
        return new SufiAIToolExecutionResult
        {
            Success = result.Success,
            Result = result.Result,
            ErrorMessage = result.ErrorMessage,
            ExceptionDetails = result.ExceptionDetails,
            ExecutionTimeMs = result.ExecutionTimeMs,
            ExecutedAt = result.ExecutedAt
        };
    }

    public static MCPToolExecutionResult MapResult(SufiAIToolExecutionResult result)
    {
        return new MCPToolExecutionResult
        {
            Success = result.Success,
            Result = result.Result,
            ErrorMessage = result.ErrorMessage,
            ExceptionDetails = result.ExceptionDetails,
            ExecutionTimeMs = result.ExecutionTimeMs,
            ExecutedAt = result.ExecutedAt
        };
    }
}

public class McpToolAdapter : IMCPTool
{
    protected ISufiAITool InnerTool { get; }

    public McpToolAdapter(ISufiAITool innerTool)
    {
        InnerTool = innerTool;
    }

    public string Name => InnerTool.Name;

    public string Description => InnerTool.Description;

    public string ParameterSchema => InnerTool.ParameterSchema;

    public MCPToolType ToolType => MCPToolType.Internal;

    public string Source => InnerTool.Source;

    public virtual async Task<MCPToolExecutionResult> ExecuteAsync(
        WorkspaceContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var result = await InnerTool.ExecuteAsync(SufiAIToolAdapter.MapContext(context), parameters, cancellationToken);
        return SufiAIToolAdapter.MapResult(result);
    }
}
