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

    public int MaxContextTokens { get; init; }

    public decimal? InputCostPer1MTokens { get; init; }

    public decimal? OutputCostPer1MTokens { get; init; }

    public bool IsFallback { get; init; }

    public Guid? ModelConfigurationId { get; init; }

    public bool IsExplicitSelection { get; init; }

    public bool IsConfigured { get; init; }

    public bool IsReady { get; init; }

    public string? FailureCode { get; init; }

    public AIModelConfiguration ToRequestModelConfiguration()
    {
        var configuration = new AIModelConfiguration(
            ModelConfiguration?.Id ?? Guid.NewGuid(),
            Workspace.Id,
            CapabilityType,
            string.IsNullOrWhiteSpace(ModelId) ? "unconfigured" : ModelId,
            ModelConfiguration?.Priority ?? 999);
        configuration.UpdateConfiguration(
            string.IsNullOrWhiteSpace(ModelId) ? "unconfigured" : ModelId,
            ApiEndpoint,
            ApiKey,
            ModelConfiguration?.Priority ?? 999,
            OpenAIApiMode,
            InputCostPer1MTokens,
            OutputCostPer1MTokens,
            ModelConfiguration?.Dimensions,
            ModelConfiguration?.DisplayName,
            ModelConfiguration?.IsUserSelectable ?? false,
            ModelConfiguration?.Description,
            MaxContextTokens > 0 ? MaxContextTokens : AIModelConfiguration.DefaultMaxContextTokens);
        return configuration;
    }
}
