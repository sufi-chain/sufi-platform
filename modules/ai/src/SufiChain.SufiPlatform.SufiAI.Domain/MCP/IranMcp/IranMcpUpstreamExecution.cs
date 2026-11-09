using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI.MCP;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.SufiAI.MCP.IranMcp;

/// <summary>
/// Gates iran-mcp upstream tools/call. Default off. Known-not-ready servers
/// return a clean unavailable result instead of throwing.
/// </summary>
public static class IranMcpUnavailableServers
{
    public static readonly HashSet<string> Keys = new(StringComparer.OrdinalIgnoreCase)
    {
        "okala-mcp",
        "divar-mcp",
        "payping-mcp",
        "bale-mcp",
        "pasavard-mcp",
        "swapwallet-mcp"
    };

    public static bool IsUnavailable(string? serverKey)
        => !string.IsNullOrWhiteSpace(serverKey) && Keys.Contains(serverKey.Trim());
}

public interface IIranMcpUpstreamExecutionGate
{
    Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default);

    bool IsUnavailable(string? serverKey);

    string UnavailableMessage(string serverKey);
}

public class IranMcpUpstreamExecutionGate : IIranMcpUpstreamExecutionGate, ITransientDependency
{
    private readonly ISettingProvider _settings;
    private readonly ILogger<IranMcpUpstreamExecutionGate> _logger;

    public IranMcpUpstreamExecutionGate(
        ISettingProvider settings,
        ILogger<IranMcpUpstreamExecutionGate> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public virtual async Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default)
    {
        var raw = await _settings.GetOrNullAsync(IranMcpSettings.AllowUpstreamExecution);
        var enabled = string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "1", StringComparison.OrdinalIgnoreCase);
        if (!enabled)
        {
            _logger.LogDebug("Iran MCP upstream execution is disabled by setting.");
        }

        return enabled;
    }

    public virtual bool IsUnavailable(string? serverKey)
        => IranMcpUnavailableServers.IsUnavailable(serverKey);

    public virtual string UnavailableMessage(string serverKey)
        => $"service unavailable: {serverKey}";
}
