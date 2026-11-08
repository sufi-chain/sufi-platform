namespace SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

/// <summary>
/// sufi-chain#557: catalog metadata that must not be confused with <see cref="IMCPToolEffect"/>.
/// </summary>
public interface IMCPToolCatalogAnnotation
{
    /// <summary>MCP <c>annotations.readOnlyHint</c>, or null when the server omitted it.</summary>
    bool? ReadOnlyHint { get; }

    /// <summary>True when an admin stored the effect. A hint alone leaves this false.</summary>
    bool EffectIsAdminSet { get; }
}
