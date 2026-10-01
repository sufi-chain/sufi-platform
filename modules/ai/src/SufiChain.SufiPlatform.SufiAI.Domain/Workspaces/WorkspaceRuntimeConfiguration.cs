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

    public decimal? InputPrice { get; init; }

    public AIPriceUnit InputPriceUnit { get; init; }

    public decimal? OutputPrice { get; init; }

    public AIPriceUnit OutputPriceUnit { get; init; }

    public bool IsFallback { get; init; }

    public Guid? ModelConfigurationId { get; init; }

    public bool IsExplicitSelection { get; init; }

    public bool IsConfigured { get; init; }

    public bool IsReady { get; init; }

    public string? FailureCode { get; init; }

    public WorkspaceRuntimeConfiguration WithOutputPrice(decimal? outputPrice)
    {
        if (OutputPrice == outputPrice)
        {
            return this;
        }

        return Copy(OpenAIApiMode, outputPrice);
    }

    public WorkspaceRuntimeConfiguration WithOpenAIApiMode(OpenAIApiMode mode)
    {
        if (OpenAIApiMode == mode)
        {
            return this;
        }

        return Copy(mode, OutputPrice);
    }

    private WorkspaceRuntimeConfiguration Copy(OpenAIApiMode mode, decimal? outputPrice)
    {
        return new WorkspaceRuntimeConfiguration
        {
            Workspace = Workspace,
            ModelConfiguration = ModelConfiguration,
            CapabilityType = CapabilityType,
            Provider = Provider,
            ModelId = ModelId,
            ApiEndpoint = ApiEndpoint,
            ApiKey = ApiKey,
            OpenAIApiMode = mode,
            MaxContextTokens = MaxContextTokens,
            InputPrice = InputPrice,
            InputPriceUnit = InputPriceUnit,
            OutputPrice = outputPrice,
            OutputPriceUnit = OutputPriceUnit,
            IsFallback = IsFallback,
            ModelConfigurationId = ModelConfigurationId,
            IsExplicitSelection = IsExplicitSelection,
            IsConfigured = IsConfigured,
            IsReady = IsReady,
            FailureCode = FailureCode
        };
    }

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
            InputPrice,
            OutputPrice,
            ModelConfiguration?.Dimensions,
            ModelConfiguration?.DisplayName,
            ModelConfiguration?.IsUserSelectable ?? false,
            ModelConfiguration?.Description,
            MaxContextTokens > 0 ? MaxContextTokens : AIModelConfiguration.DefaultMaxContextTokens,
            InputPriceUnit,
            OutputPriceUnit);
        configuration.CopyChatCapabilitiesFrom(ModelConfiguration);
        return configuration;
    }
}
