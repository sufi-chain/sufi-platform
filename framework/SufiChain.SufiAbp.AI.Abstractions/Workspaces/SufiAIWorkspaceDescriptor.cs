using System.Collections.Generic;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Credential-free description of an AI workspace exposed to product modules.
/// Intentionally carries no API keys, endpoints, or provider configuration —
/// key material stays inside the provider/implementation layer.
/// </summary>
public class SufiAIWorkspaceDescriptor
{
    /// <summary>
    /// Unique workspace name used to address the workspace in AI requests.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Default model identifier configured for the workspace, when known.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Whether the workspace is active (admin-enabled).
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Whether the workspace is fully configured and ready to serve requests
    /// (e.g. has a model and credentials), without exposing the configuration itself.
    /// </summary>
    public bool IsReady { get; set; }

    /// <summary>
    /// Capabilities enabled for this workspace.
    /// </summary>
    public List<SufiAICapability> Capabilities { get; set; } = new();
}
