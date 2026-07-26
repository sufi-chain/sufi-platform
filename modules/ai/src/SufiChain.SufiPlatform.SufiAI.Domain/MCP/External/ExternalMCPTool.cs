using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

namespace SufiChain.SufiPlatform.SufiAI.MCP.External;

/// <summary>
/// Wrapper for external MCP server tools.
/// </summary>
public class ExternalMCPTool : IMCPTool
{
    private static readonly TimeSpan ExecutionTimeout = TimeSpan.FromSeconds(30);
    private readonly IMCPTransportClient _client;
    private readonly Guid _serverId;
    private readonly string _remoteName;
    
    public string Name { get; }
    public string Description { get; }
    public string ParameterSchema { get; }
    public MCPToolType ToolType => MCPToolType.External;
    public string Source { get; }
    
    public ExternalMCPTool(
        string name,
        string description,
        string parameterSchema,
        Guid serverId,
        string serverName,
        string remoteName,
        IMCPTransportClient client)
    {
        Name = name;
        Description = description;
        ParameterSchema = parameterSchema;
        _serverId = serverId;
        _remoteName = remoteName;
        Source = serverName;
        _client = client;
    }
    
    public async Task<MCPToolExecutionResult> ExecuteAsync(
        WorkspaceContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(ExecutionTimeout);
            var result = await _client.CallToolAsync(_remoteName, parameters, timeoutSource.Token);
            
            stopwatch.Stop();
            
            if (result.Success)
            {
                return MCPToolExecutionResult.CreateSuccess(result.Result, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                return MCPToolExecutionResult.CreateFailure(result.ErrorMessage ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            return MCPToolExecutionResult.CreateFailure(ex.Message, ex.ToString());
        }
    }
}
