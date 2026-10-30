using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

namespace SufiChain.SufiPlatform.SufiAI.Adapters;

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

    /// <summary>The inner tool's effect, or write when the inner tool does not declare one (sufi-chain#312).</summary>
    public AgentActionEffect Effect =>
        InnerTool is IMCPToolEffect declared
        && declared.Effect is AgentActionEffect.Read or AgentActionEffect.Write
            ? declared.Effect
            : AgentActionEffect.Write;

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
            Metadata = context.Metadata,
            // The bound identity rides in metadata across the abstractions boundary (sufi-chain#384).
            CallContext = context.Metadata is { } metadata
                          && metadata.TryGetValue(AgentCallContext.MetadataKey, out var call)
                ? call as AgentCallContext
                : null
        };
    }

    public static SufiAIToolExecutionContext MapContext(WorkspaceContext context)
    {
        Dictionary<string, object?>? metadata = context.Metadata;
        if (context.CallContext is not null)
        {
            metadata = metadata is null
                ? new Dictionary<string, object?>()
                : new Dictionary<string, object?>(metadata);
            metadata[AgentCallContext.MetadataKey] = context.CallContext;
        }

        return new SufiAIToolExecutionContext
        {
            WorkspaceName = context.WorkspaceName,
            TenantId = context.TenantId,
            UserId = context.UserId,
            Metadata = metadata
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

public class McpToolAdapter : IMCPTool, IMCPToolEffect
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

    /// <summary>The inner tool's effect, which defaults to write (sufi-chain#312).</summary>
    public AgentActionEffect Effect =>
        InnerTool.Effect is AgentActionEffect.Read or AgentActionEffect.Write
            ? InnerTool.Effect
            : AgentActionEffect.Write;

    /// <inheritdoc />
    public bool IsReadOnly => Effect == AgentActionEffect.Read;

    public virtual async Task<MCPToolExecutionResult> ExecuteAsync(
        WorkspaceContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var result = await InnerTool.ExecuteAsync(SufiAIToolAdapter.MapContext(context), parameters, cancellationToken);
        return SufiAIToolAdapter.MapResult(result);
    }
}
