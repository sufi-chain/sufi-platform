using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Provider-neutral description of an AI model available in a workspace.
/// </summary>
public class SufiAIModelInfo
{
    /// <summary>
    /// Model identifier (e.g. <c>gpt-4o</c>).
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// Optional human-readable display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Capabilities supported by this model.
    /// </summary>
    public List<SufiAICapability> Capabilities { get; set; } = new();
}
