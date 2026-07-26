using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    private static readonly TimeSpan DisconnectGracePeriod = TimeSpan.FromSeconds(2);

    private readonly string _command;
    private readonly string[] _arguments;
    private readonly ILogger<StdioTransportClient> _logger;
    private Process? _process;
    private StreamWriter? _stdin;
    private StreamReader? _stdout;
    private Task? _stderrDrainTask;
    private int _requestId;
    private readonly SemaphoreSlim _requestLock = new(1, 1);

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
        using var cancelRegistration = cancellationToken.Register(TerminateProcess);

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
            _stderrDrainTask = DrainStandardErrorAsync(_process.StandardError);

            await SendJsonRpcRequestAsync("initialize", new { protocolVersion = "1.0" }, cancellationToken);

            _logger.LogInformation("Connected to MCP server via STDIO: {Command}", _command);
        }
        catch (Exception ex) when (ex is not BusinessException)
        {
            TerminateProcess();
            _logger.LogError(ex, "Failed to connect to MCP server: {Command}", _command);
            throw new BusinessException(AIErrorCodes.MCPServerConnectionFailed)
                .WithData("Command", _command)
                .WithData("Error", ex.Message);
        }
        catch
        {
            TerminateProcess();
            throw;
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_process == null)
        {
            return;
        }

        try
        {
            try
            {
                _stdin?.Close();
            }
            catch
            {
                // ignored
            }

            if (!_process.HasExited)
            {
                try
                {
                    await _process.WaitForExitAsync(cancellationToken)
                        .WaitAsync(DisconnectGracePeriod, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    TerminateProcess();
                    return;
                }
                catch (TimeoutException)
                {
                    TerminateProcess();
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during MCP server disconnect");
            TerminateProcess();
            return;
        }

        TerminateProcess();
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
        await _requestLock.WaitAsync(cancellationToken);
        try
        {
            if (_stdin == null || _stdout == null)
            {
                throw new InvalidOperationException("Not connected to MCP server");
            }

            // Kill the child process on cancel so ReadLineAsync cannot block a thread forever.
            using var cancelRegistration = cancellationToken.Register(TerminateProcess);

            var requestId = Interlocked.Increment(ref _requestId);

            var request = new
            {
                jsonrpc = "2.0",
                id = requestId,
                method,
                @params = parameters
            };

            var json = JsonSerializer.Serialize(request);
            await _stdin.WriteLineAsync(json.AsMemory(), cancellationToken);
            await _stdin.FlushAsync(cancellationToken);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var responseLine = await _stdout.ReadLineAsync(cancellationToken);

                if (string.IsNullOrEmpty(responseLine))
                {
                    throw new BusinessException(AIErrorCodes.MCPServerConnectionFailed)
                        .WithData("Reason", "Empty response from server");
                }

                using var responseDoc = JsonDocument.Parse(responseLine);
                var root = responseDoc.RootElement;

                if (!root.TryGetProperty("id", out var responseId) ||
                    responseId.ValueKind != JsonValueKind.Number ||
                    responseId.GetInt32() != requestId)
                {
                    continue;
                }

                if (root.TryGetProperty("error", out var error))
                {
                    throw new BusinessException(AIErrorCodes.MCPToolExecutionFailed)
                        .WithData("Error", error.GetRawText());
                }

                if (root.TryGetProperty("result", out var result))
                {
                    return result.Clone();
                }

                throw new BusinessException(AIErrorCodes.MCPServerConnectionFailed)
                    .WithData("Reason", "Invalid response format");
            }
        }
        finally
        {
            _requestLock.Release();
        }
    }

    private async Task DrainStandardErrorAsync(StreamReader standardError)
    {
        try
        {
            while (await standardError.ReadLineAsync() is { } line)
            {
                _logger.LogDebug("MCP server {Command} stderr: {Line}", _command, line);
            }
        }
        catch (ObjectDisposedException)
        {
            // Process torn down.
        }
    }

    private void TerminateProcess()
    {
        try
        {
            if (_process is { HasExited: false })
            {
                _process.Kill(entireProcessTree: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to terminate MCP server process: {Command}", _command);
        }

        try { _stdin?.Dispose(); } catch { /* ignored */ }
        try { _stdout?.Dispose(); } catch { /* ignored */ }
        try { _process?.Dispose(); } catch { /* ignored */ }

        _stdin = null;
        _stdout = null;
        _process = null;
        _stderrDrainTask = null;
    }

    public void Dispose()
    {
        TerminateProcess();
        _requestLock.Dispose();
    }
}
