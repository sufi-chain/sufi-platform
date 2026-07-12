using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.MCP.External;

/// <summary>
/// STDIO transport client for MCP servers (process-based communication).
/// </summary>
public class StdioTransportClient : IMCPTransportClient
{
    private readonly string _command;
    private readonly string[] _arguments;
    private readonly ILogger<StdioTransportClient> _logger;
    private Process? _process;
    private StreamWriter? _stdin;
    private StreamReader? _stdout;
    private int _requestId;
    
    public MCPTransportType TransportType => MCPTransportType.STDIO;
    public bool IsConnected => _process != null && !_process.HasExited;
    
    public StdioTransportClient(
        string command,
        string[] arguments,
        ILogger<StdioTransportClient> logger)
    {
        _command = command;
        _arguments = arguments;
        _logger = logger;
    }
    
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _command,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            foreach (var arg in _arguments)
            {
                startInfo.ArgumentList.Add(arg);
            }
            
            _process = Process.Start(startInfo);
            
            if (_process == null)
            {
                throw new BusinessException(AIErrorCodes.MCPServerConnectionFailed)
                    .WithData("Command", _command)
                    .WithData("Reason", "Failed to start process");
            }
            
            _stdin = _process.StandardInput;
            _stdout = _process.StandardOutput;
            
            // Send initialize request
            await SendJsonRpcRequestAsync("initialize", new { protocolVersion = "1.0" }, cancellationToken);
            
            _logger.LogInformation("Connected to MCP server via STDIO: {Command}", _command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MCP server: {Command}", _command);
            throw new BusinessException(AIErrorCodes.MCPServerConnectionFailed)
                .WithData("Command", _command)
                .WithData("Error", ex.Message);
        }
    }
    
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_process != null && !_process.HasExited)
        {
            try
            {
                _stdin?.Close();
                _stdout?.Close();
                
                if (!_process.WaitForExit(5000))
                {
                    _process.Kill();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during MCP server disconnect");
            }
            finally
            {
                _process?.Dispose();
                _process = null;
            }
        }
    }
    
    public async Task<List<MCPServerToolDefinition>> ListToolsAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendJsonRpcRequestAsync("tools/list", new { }, cancellationToken);
        
        var tools = new List<MCPServerToolDefinition>();
        
        if (response.TryGetProperty("tools", out var toolsArray))
        {
            foreach (var tool in toolsArray.EnumerateArray())
            {
                tools.Add(new MCPServerToolDefinition
                {
                    Name = tool.GetProperty("name").GetString() ?? "",
                    Description = tool.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                    ParameterSchema = tool.TryGetProperty("inputSchema", out var schema) 
                        ? schema.GetRawText() 
                        : "{}"
                });
            }
        }
        
        return tools;
    }
    
    public async Task<MCPServerToolResult> CallToolAsync(
        string toolName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            name = toolName,
            arguments = parameters
        };
        
        try
        {
            var response = await SendJsonRpcRequestAsync("tools/call", request, cancellationToken);
            
            return new MCPServerToolResult
            {
                Success = true,
                Result = JsonSerializer.Deserialize<object>(response.GetRawText())
            };
        }
        catch (Exception ex)
        {
            return new MCPServerToolResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
    
    private async Task<JsonElement> SendJsonRpcRequestAsync(
        string method,
        object parameters,
        CancellationToken cancellationToken)
    {
        if (_stdin == null || _stdout == null)
        {
            throw new InvalidOperationException("Not connected to MCP server");
        }
        
        var requestId = Interlocked.Increment(ref _requestId);
        
        var request = new
        {
            jsonrpc = "2.0",
            id = requestId,
            method,
            @params = parameters
        };
        
        var json = JsonSerializer.Serialize(request);
        await _stdin.WriteLineAsync(json);
        await _stdin.FlushAsync();
        
        // Read response
        var responseLine = await _stdout.ReadLineAsync(cancellationToken);
        
        if (string.IsNullOrEmpty(responseLine))
        {
            throw new BusinessException(AIErrorCodes.MCPServerConnectionFailed)
                .WithData("Reason", "Empty response from server");
        }
        
        var responseDoc = JsonDocument.Parse(responseLine);
        var root = responseDoc.RootElement;
        
        if (root.TryGetProperty("error", out var error))
        {
            throw new BusinessException(AIErrorCodes.MCPToolExecutionFailed)
                .WithData("Error", error.GetRawText());
        }
        
        if (root.TryGetProperty("result", out var result))
        {
            return result;
        }
        
        throw new BusinessException(AIErrorCodes.MCPServerConnectionFailed)
            .WithData("Reason", "Invalid response format");
    }
    
    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
    }
}
