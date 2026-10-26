using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Audit;

namespace SufiChain.SufiPlatform.SufiAI.MCP.External;

/// <summary>
/// Wrapper for external MCP server tools.
/// 9.4 (sufi-chain#340): the old fixed 30s execution cutoff is gone. A tool call inside a turn is cancelled
/// only through the turn's own token (user Stop, session/circuit close, or shutdown).
/// 9.5 (sufi-chain#384 slice 6a): every call writes one audit row through <see cref="IMcpToolCallAuditor"/>.
/// </summary>
public class ExternalMCPTool : IMCPTool, IMCPToolEffect
{
    private readonly IMCPTransportClient _client;
    private readonly Guid _serverId;
    private readonly string _remoteName;
    private readonly IMcpToolCallAuditor? _auditor;

    public string Name { get; }
    public string Description { get; }
    public string ParameterSchema { get; }
    public MCPToolType ToolType => MCPToolType.External;
    public string Source { get; }

    /// <summary>
    /// External tools carry no declared effect yet (sufi-chain#384 slice 7 / #138), so under the interim
    /// rule they count as write.
    /// </summary>
    public bool IsReadOnly => false;

    public ExternalMCPTool(
        string name,
        string description,
        string parameterSchema,
        Guid serverId,
        string serverName,
        string remoteName,
        IMCPTransportClient client,
        IMcpToolCallAuditor? auditor = null)
    {
        Name = name;
        Description = description;
        ParameterSchema = parameterSchema;
        _serverId = serverId;
        _remoteName = remoteName;
        Source = serverName;
        _client = client;
        _auditor = auditor;
    }

    public async Task<MCPToolExecutionResult> ExecuteAsync(
        WorkspaceContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await _client.CallToolAsync(_remoteName, parameters, cancellationToken);

            stopwatch.Stop();

            if (result.Success)
            {
                var success = MCPToolExecutionResult.CreateSuccess(result.Result, stopwatch.ElapsedMilliseconds);
                await McpToolCallAudit.RecordDispatchedAsync(_auditor, this, context, parameters, success, success.ExecutionTimeMs);
                return success;
            }
            else
            {
                var failure = MCPToolExecutionResult.CreateFailure(result.ErrorMessage ?? "Unknown error");
                await McpToolCallAudit.RecordDispatchedAsync(_auditor, this, context, parameters, failure, stopwatch.ElapsedMilliseconds);
                return failure;
            }
        }
        catch (OperationCanceledException)
        {
            // Preserve cancellation semantics (ABP use-cancellation-tokens skill): the turn's token is the
            // only cancellation source here, so rethrow instead of hiding it as a tool failure.
            await McpToolCallAudit.RecordCancelledAsync(_auditor, this, context, parameters, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var failure = MCPToolExecutionResult.CreateFailure(ex.Message, ex.ToString());
            await McpToolCallAudit.RecordDispatchedAsync(_auditor, this, context, parameters, failure, stopwatch.ElapsedMilliseconds);
            return failure;
        }
    }
}
