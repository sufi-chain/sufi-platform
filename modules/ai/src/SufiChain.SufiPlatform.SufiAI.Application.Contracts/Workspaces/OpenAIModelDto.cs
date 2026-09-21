using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class OpenAIModelDto
{
    public string Id { get; set; } = string.Empty;

    public string? OwnedBy { get; set; }

    public long? Created { get; set; }

    /// <summary>LiteLLM-style catalog mode when the provider sends it.</summary>
    public string? Mode { get; set; }

    /// <summary>OpenRouter <c>architecture.modality</c> when present.</summary>
    public string? Modality { get; set; }

    public List<string>? InputModalities { get; set; }

    public List<string>? OutputModalities { get; set; }
}
