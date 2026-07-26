namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceRuntimeConfiguration
{
    public required Workspace Workspace { get; init; }

    public AIModelConfiguration? ModelConfiguration { get; init; }

    public AICapabilityType CapabilityType { get; init; }

    public AIProviderType Provider { get; init; }

    public string ModelId { get; init; } = string.Empty;

    public string? ApiEndpoint { get; init; }

    public string? ApiKey { get; init; }

    public OpenAIApiMode OpenAIApiMode { get; init; }

    public decimal? InputCostPer1MTokens { get; init; }

    public decimal? OutputCostPer1MTokens { get; init; }

    public bool IsFallback { get; init; }

    public bool IsConfigured { get; init; }

    public bool IsReady { get; init; }

    public string? FailureCode { get; init; }
}
