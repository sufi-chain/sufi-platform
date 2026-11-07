using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.MCP.External;

/// <summary>
/// STDIO transport client for MCP servers (process-based communication).
/// sufi-chain#557: initialize carries protocolVersion, capabilities, and clientInfo.
/// Configured environment values are injected into the child process and redacted from logs.
/// They are never placed in the argument list. A user-scoped bearer token is not an environment
/// variable; STDIO stays tenant-level.
/// </summary>
public class StdioTransportClient : IMCPTransportClient
{
    private static readonly TimeSpan DisconnectGracePeriod = TimeSpan.FromSeconds(2);
    private static readonly Regex SensitiveEnvironmentName = new(
        "(?i)(SECRET|PASSWORD|TOKEN|CREDENTIAL|CONNECTION|API[_-]?KEY|PWD)",
        RegexOptions.Compiled);

    private readonly string _command;
    private readonly string[] _arguments;
    private readonly IReadOnlyDictionary<string, string> _environment;
    private readonly TimeSpan _requestTimeout;
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
        ILogger<StdioTransportClient> logger,
        IReadOnlyDictionary<string, string>? environment = null,
        TimeSpan? requestTimeout = null)
    {
        _command = command;
        _arguments = arguments;
        _environment = environment ?? new Dictionary<string, string>();
        var timeout = requestTimeout.GetValueOrDefault(Timeout.InfiniteTimeSpan);
        _requestTimeout = timeout <= TimeSpan.Zero ? Timeout.InfiniteTimeSpan : timeout;
        _logger = logger;
    }

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        return WithTimeoutAsync(cancellationToken, ConnectCoreAsync);
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

    /// <summary>
    /// sufi-chain#384 slice 6b: STDIO has no per-user credential channel. A bearer token is ignored.
    /// Tenant secrets belong in the process environment, which is configured separately and never logged.
    /// </summary>
    public Task<List<MCPServerToolDefinition>> ListToolsAsync(
        CancellationToken cancellationToken = default,
        string? bearerToken = null)
    {
        return WithTimeoutAsync(cancellationToken, async token =>
        {
            var tools = new List<MCPServerToolDefinition>();
            string? cursor = null;
            for (var page = 0; page < 20; page++)
            {
                object? parameters = string.IsNullOrEmpty(cursor)
                    ? null
                    : new Dictionary<string, object?> { ["cursor"] = cursor };

                var response = await SendJsonRpcRequestAsync("tools/list", parameters, token);
                if (response.ValueKind == JsonValueKind.Object
                    && response.TryGetProperty("tools", out var listed))
                {
                    tools.AddRange(McpJsonRpc.ReadTools(listed));
                }

                if (response.ValueKind != JsonValueKind.Object
                    || !response.TryGetProperty("nextCursor", out var next)
                    || next.ValueKind != JsonValueKind.String)
                {
                    break;
                }

                cursor = next.GetString();
                if (string.IsNullOrEmpty(cursor))
                {
                    break;
                }
            }

            return tools;
        });
    }

    public Task<MCPServerToolResult> CallToolAsync(
        string toolName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default,
        string? bearerToken = null)
    {
        return WithTimeoutAsync(cancellationToken, async token =>
        {
            var request = new
            {
                name = toolName,
                arguments = parameters
            };

            try
            {
                var response = await SendJsonRpcRequestAsync("tools/call", request, token);
                return new MCPServerToolResult
                {
                    Success = true,
                    Result = JsonSerializer.Deserialize<object>(response.GetRawText())
                };
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var requestId = _requestId > 0 ? _requestId.ToString() : null;
                return new MCPServerToolResult
                {
                    Success = false,
                    ErrorMessage = McpSanitizedError.FromException(new InvalidOperationException(Redact(ex.Message), ex), requestId).ToClientMessage()
                };
            }
        });
    }

    private async Task ConnectCoreAsync(CancellationToken cancellationToken)
    {
        using var cancelRegistration = cancellationToken.Register(TerminateProcess);

        try
        {
            RefuseSecretsInArguments();

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

            ApplyEnvironment(startInfo);

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

            var result = await SendJsonRpcRequestAsync("initialize", McpJsonRpc.InitializeParams(), cancellationToken);
            await SendNotificationAsync("notifications/initialized", cancellationToken);

            var version = result.ValueKind == JsonValueKind.Object
                && result.TryGetProperty("protocolVersion", out var versionElement)
                && versionElement.ValueKind == JsonValueKind.String
                ? versionElement.GetString()
                : McpJsonRpc.RequestedProtocolVersion;

            _logger.LogInformation(
                "Connected to MCP server via STDIO: {Command} protocol {ProtocolVersion}",
                _command,
                version);
        }
        catch (Exception ex) when (ex is not BusinessException and not OperationCanceledException)
        {
            TerminateProcess();
            _logger.LogError(ex, "Failed to connect to MCP server: {Command}", _command);
            throw new BusinessException(AIErrorCodes.MCPServerConnectionFailed)
                .WithData("Command", _command)
                .WithData("Error", Redact(ex.Message));
        }
        catch
        {
            TerminateProcess();
            throw;
        }
    }

    private async Task<JsonElement> SendJsonRpcRequestAsync(
        string method,
        object? parameters,
        CancellationToken cancellationToken)
    {
        await _requestLock.WaitAsync(cancellationToken);
        try
        {
            if (_stdin == null || _stdout == null)
            {
                throw new InvalidOperationException("Not connected to MCP server");
            }

            using var cancelRegistration = cancellationToken.Register(TerminateProcess);

            var requestId = Interlocked.Increment(ref _requestId);
            var json = McpJsonRpc.CreateRequest(requestId, method, parameters);
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
                    var message = error.ValueKind == JsonValueKind.Object
                        && error.TryGetProperty("message", out var errorMessage)
                        && errorMessage.ValueKind == JsonValueKind.String
                        ? errorMessage.GetString()
                        : error.GetRawText();
                    throw new BusinessException(AIErrorCodes.MCPServerConnectionFailed)
                        .WithData("Error", Redact(string.IsNullOrWhiteSpace(message) ? "MCP server returned an error." : message));
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

    private async Task SendNotificationAsync(string method, CancellationToken cancellationToken)
    {
        if (_stdin == null)
        {
            throw new InvalidOperationException("Not connected to MCP server");
        }

        var json = McpJsonRpc.CreateNotification(method);
        await _stdin.WriteLineAsync(json.AsMemory(), cancellationToken);
        await _stdin.FlushAsync(cancellationToken);
    }

    private void RefuseSecretsInArguments()
    {
        foreach (var argument in _arguments)
        {
            foreach (var value in _environment.Values)
            {
                if (value.Length >= 8 && argument.Contains(value, StringComparison.Ordinal))
                {
                    throw new BusinessException(AIErrorCodes.InvalidProviderConfiguration)
                        .WithData("Error", "Refusing to start the MCP process because a secret was placed in its arguments.");
                }
            }
        }
    }

    private void ApplyEnvironment(ProcessStartInfo startInfo)
    {
        foreach (var key in startInfo.Environment.Keys.ToList())
        {
            if (SensitiveEnvironmentName.IsMatch(key))
            {
                startInfo.Environment.Remove(key);
            }
        }

        foreach (var pair in _environment)
        {
            startInfo.Environment[pair.Key] = pair.Value;
        }

        if (_environment.Count > 0)
        {
            _logger.LogDebug(
                "MCP stdio {Command} environment key count {Count}",
                _command,
                _environment.Count);
        }
    }

    private async Task DrainStandardErrorAsync(StreamReader standardError)
    {
        try
        {
            while (await standardError.ReadLineAsync() is { } line)
            {
                _logger.LogDebug("MCP server {Command} stderr: {Line}", _command, Redact(line));
            }
        }
        catch (ObjectDisposedException)
        {
            // Process torn down.
        }
    }

    private string Redact(string? text)
    {
        var value = text ?? string.Empty;
        foreach (var secret in _environment.Values)
        {
            if (secret.Length >= 4)
            {
                value = value.Replace(secret, "[redacted]", StringComparison.Ordinal);
            }
        }

        return McpSanitizedError.Sanitize(value);
    }

    private async Task WithTimeoutAsync(CancellationToken cancellationToken, Func<CancellationToken, Task> action)
    {
        await WithTimeoutAsync(cancellationToken, async token =>
        {
            await action(token);
            return true;
        });
    }

    private async Task<T> WithTimeoutAsync<T>(CancellationToken cancellationToken, Func<CancellationToken, Task<T>> action)
    {
        if (_requestTimeout == Timeout.InfiniteTimeSpan)
        {
            return await action(cancellationToken);
        }

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linked.CancelAfter(_requestTimeout);
        try
        {
            return await action(linked.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new BusinessException(AIErrorCodes.MCPServerConnectionFailed)
                .WithData("Error", "Timed out waiting for the MCP stdio server.");
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
