using System;
using System.Reflection;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Audit;

/// <summary>
/// The request correlation id for an audit row. Optional: a host that does not run the ABP correlation
/// middleware still gets audit rows, just without the id.
/// </summary>
public interface IMcpAuditCorrelationProvider
{
    string? GetCorrelationId();
}

/// <summary>
/// Resolves <c>Volo.Abp.Tracing.ICorrelationIdProvider</c> reflectively when the host has it, so the AI
/// module carries no hard reference to the tracing package (the same approach the chat error filter uses).
/// </summary>
public class McpAuditCorrelationProvider : IMcpAuditCorrelationProvider
{
    private const string ProviderTypeName = "Volo.Abp.Tracing.ICorrelationIdProvider, Volo.Abp.Tracing";

    private readonly IServiceProvider _serviceProvider;
    private readonly MethodInfo? _get;

    public McpAuditCorrelationProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _get = Type.GetType(ProviderTypeName, throwOnError: false)?
            .GetMethod("Get", BindingFlags.Instance | BindingFlags.Public, binder: null, Type.EmptyTypes, modifiers: null);
    }

    public string? GetCorrelationId()
    {
        if (_get is null)
        {
            return null;
        }

        try
        {
            var providerType = Type.GetType(ProviderTypeName, throwOnError: false);
            var provider = providerType is null ? null : _serviceProvider.GetService(providerType);
            return provider is null ? null : _get.Invoke(provider, null) as string;
        }
        catch (Exception)
        {
            // A missing or misbehaving correlation provider must never break the audit write.
            return null;
        }
    }
}
