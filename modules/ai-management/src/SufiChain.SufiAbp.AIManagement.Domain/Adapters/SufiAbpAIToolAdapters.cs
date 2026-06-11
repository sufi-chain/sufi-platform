using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AIManagement.MCP.Abstractions;

namespace SufiChain.SufiAbp.AIManagement.Adapters;

public class SufiAbpAIToolAdapter : ISufiAbpAITool
{
    public IMCPTool InnerTool { get; }

    public SufiAbpAIToolAdapter(IMCPTool innerTool)
    {
        InnerTool = innerTool;
    }

    public string Name => InnerTool.Name;

    public string Description => InnerTool.Description;

    public string ParameterSchema => InnerTool.ParameterSchema;

    public string Source => InnerTool.Source;

    public virtual async Task<SufiAbpAIToolExecutionResult> ExecuteAsync(
        SufiAbpAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var result = await InnerTool.ExecuteAsync(MapContext(context), parameters, cancellationToken);
        return MapResult(result);
    }

    public static WorkspaceContext MapContext(SufiAbpAIToolExecutionContext context)
    {
        return new WorkspaceContext
        {
            WorkspaceName = context.WorkspaceName,
            TenantId = context.TenantId,
            UserId = context.UserId,
            Metadata = context.Metadata
        };
    }

    public static SufiAbpAIToolExecutionContext MapContext(WorkspaceContext context)
    {
        return new SufiAbpAIToolExecutionContext
        {
            WorkspaceName = context.WorkspaceName,
            TenantId = context.TenantId,
            UserId = context.UserId,
            Metadata = context.Metadata
        };
    }

    public static SufiAbpAIToolExecutionResult MapResult(MCPToolExecutionResult result)
    {
        return new SufiAbpAIToolExecutionResult
        {
            Success = result.Success,
            Result = result.Result,
            ErrorMessage = result.ErrorMessage,
            ExceptionDetails = result.ExceptionDetails,
            ExecutionTimeMs = result.ExecutionTimeMs,
            ExecutedAt = result.ExecutedAt
        };
    }

    public static MCPToolExecutionResult MapResult(SufiAbpAIToolExecutionResult result)
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
    protected ISufiAbpAITool InnerTool { get; }

    public McpToolAdapter(ISufiAbpAITool innerTool)
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
        var result = await InnerTool.ExecuteAsync(SufiAbpAIToolAdapter.MapContext(context), parameters, cancellationToken);
        return SufiAbpAIToolAdapter.MapResult(result);
    }
}
