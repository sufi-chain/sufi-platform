using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class GetOpenAIModelsInput
{
    public Guid? WorkspaceId { get; set; }

    /// <summary>
    /// When set, empty ApiKey/ApiBaseUrl fall back to this model configuration before the workspace.
    /// </summary>
    public Guid? ModelConfigurationId { get; set; }

    [StringLength(512)]
    public string? ApiKey { get; set; }

    [StringLength(512)]
    public string? ApiBaseUrl { get; set; }

    /// <summary>
    /// Filters the provider catalog to models that match this route capability.
    /// Defaults to chat so workspace default-model pickers stay LLM-only.
    /// </summary>
    public AICapabilityType CapabilityType { get; set; } = AICapabilityType.ChatCompletion;
}
