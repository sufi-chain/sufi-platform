using System;
using System.Collections.Generic;
using System.Linq;
using SufiChain.SufiPlatform.SufiAI;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class Workspace : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    
    public string Name { get; protected set; } = string.Empty;
    public AIProviderType Provider { get; protected set; }
    
    /// <summary>
    /// Default/fallback model ID for this workspace. Used when no specific model configuration exists for a capability.
    /// </summary>
    public string DefaultModel { get; protected set; } = string.Empty;
    
    public string? ApiKey { get; protected set; }
    public string? ApiBaseUrl { get; protected set; }
    public string? SystemPrompt { get; protected set; }
    public float Temperature { get; protected set; }
    public int MaxContextTokens { get; protected set; }
    public OpenAIApiMode OpenAIApiMode { get; protected set; }
    public decimal? InputCostPer1MTokens { get; protected set; }
    public decimal? OutputCostPer1MTokens { get; protected set; }
    public bool IsActive { get; protected set; }
    
    public string? EmbedderConfigJson { get; protected set; }
    public string? VectorStoreConfigJson { get; protected set; }
    public string? EnabledMCPToolsJson { get; protected set; }
    
    /// <summary>
    /// Collection of AI model configurations for different capabilities.
    /// Replaces the single Model property to support multi-modal AI.
    /// </summary>
    private readonly List<AIModelConfiguration> _modelConfigurations = new();
    public IReadOnlyList<AIModelConfiguration> ModelConfigurations => _modelConfigurations.AsReadOnly();
    
    /// <summary>
    /// Gets the model ID - returns primary chat completion model or the workspace's default model
    /// </summary>
    public string Model => GetPrimaryConfiguration(AICapabilityType.ChatCompletion)?.ModelId ?? DefaultModel;
    
    protected Workspace() { }
    
    public Workspace(
        Guid id,
        string name,
        AIProviderType provider,
        string model,
        Guid? tenantId = null
    ) : base(id)
    {
        SetName(name);
        Provider = provider;
        DefaultModel = model;
        TenantId = tenantId;
        IsActive = true;
        Temperature = 0.7f;
        MaxContextTokens = 200000;
        OpenAIApiMode = OpenAIApiMode.ChatCompletions;
    }
    
    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
    }
    
    public void UpdateConfiguration(
        string model,
        string? apiKey,
        string? apiBaseUrl,
        string? systemPrompt,
        float temperature,
        int maxContextTokens,
        OpenAIApiMode openAIApiMode = OpenAIApiMode.ChatCompletions,
        decimal? inputCostPer1MTokens = null,
        decimal? outputCostPer1MTokens = null
    )
    {
        ValidatePricing(inputCostPer1MTokens, nameof(inputCostPer1MTokens));
        ValidatePricing(outputCostPer1MTokens, nameof(outputCostPer1MTokens));

        DefaultModel = model;
        ApiKey = apiKey;
        ApiBaseUrl = apiBaseUrl;
        SystemPrompt = systemPrompt;
        Temperature = temperature;
        MaxContextTokens = maxContextTokens;
        OpenAIApiMode = openAIApiMode;
        InputCostPer1MTokens = inputCostPer1MTokens;
        OutputCostPer1MTokens = outputCostPer1MTokens;
    }
    
    /// <summary>
    /// Add a model configuration for a specific capability
    /// </summary>
    public AIModelConfiguration AddModelConfiguration(
        AICapabilityType capabilityType,
        string modelId,
        string? apiEndpoint = null,
        string? apiKey = null,
        string? configurationJson = null,
        int priority = 0,
        OpenAIApiMode? openAIApiMode = null,
        decimal? inputCostPer1MTokens = null,
        decimal? outputCostPer1MTokens = null
    )
    {
        var config = new AIModelConfiguration(
            Guid.NewGuid(),
            Id,
            capabilityType,
            modelId,
            priority
        );
        
        config.UpdateConfiguration(
            modelId,
            apiEndpoint,
            apiKey,
            configurationJson,
            priority,
            openAIApiMode,
            inputCostPer1MTokens,
            outputCostPer1MTokens);
        
        _modelConfigurations.Add(config);
        return config;
    }
    
    /// <summary>
    /// Remove a model configuration
    /// </summary>
    public void RemoveModelConfiguration(AIModelConfiguration configuration)
    {
        _modelConfigurations.Remove(configuration);
    }
    
    /// <summary>
    /// Get the primary (highest priority) configuration for a capability
    /// </summary>
    public AIModelConfiguration? GetPrimaryConfiguration(AICapabilityType capabilityType)
    {
        return _modelConfigurations
            .Where(c => c.CapabilityType == capabilityType && c.IsEnabled)
            .OrderBy(c => c.Priority)
            .FirstOrDefault();
    }
    
    /// <summary>
    /// Check if a capability is configured and enabled
    /// </summary>
    public bool HasCapability(AICapabilityType capabilityType)
    {
        return _modelConfigurations.Any(c => c.CapabilityType == capabilityType && c.IsEnabled);
    }
    
    public void SetEmbedderConfig(string? configJson)
    {
        EmbedderConfigJson = configJson;
    }
    
    public void SetVectorStoreConfig(string? configJson)
    {
        VectorStoreConfigJson = configJson;
    }

    public void SetEnabledMCPTools(string? toolsJson)
    {
        EnabledMCPToolsJson = toolsJson;
    }
    
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    private static void ValidatePricing(decimal? value, string parameterName)
    {
        if (value < 0)
        {
            throw new BusinessException("AI:InvalidTokenPricing")
                .WithData("ParameterName", parameterName);
        }
    }
}
