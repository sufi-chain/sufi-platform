namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class AiProviderProfileDto
{
    public AIProviderType ProviderType { get; set; }

    public AIProviderCapabilityKind CapabilityKind { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string? DefaultBaseUrl { get; set; }

    public bool RequiresExplicitBaseUrl { get; set; }

    public bool SupportsDecisions { get; set; }
}
