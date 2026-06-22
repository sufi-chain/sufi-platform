using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.AI.MCP.Abstractions;

namespace SufiChain.SufiAbp.AI.MCP.External;

/// <summary>
/// Wrapper for external MCP server tools.
/// </summary>
public class ExternalMCPTool : IMCPTool
{
    private readonly IMCPTransportClient _client;
    private readonly Guid _serverId;
    
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
        IMCPTransportClient client)
    {
        Name = name;
        Description = description;
        ParameterSchema = parameterSchema;
        _serverId = serverId;
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
            var result = await _client.CallToolAsync(Name, parameters, cancellationToken);
            
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
