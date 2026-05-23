using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SufiChain.SufiAbp.AIManagement.AI;

/// <summary>
/// Represents a specific AI model configuration for a capability within a workspace.
/// A workspace can have multiple configurations (e.g., GPT-4 for chat, Whisper for audio).
/// </summary>
public class AIModelConfiguration : AuditedEntity<Guid>
{
    /// <summary>
    /// The workspace this configuration belongs to
    /// </summary>
    public Guid WorkspaceId { get; protected set; }
    
    /// <summary>
    /// The AI capability this configuration provides (Chat, Audio, Vision, etc.)
    /// </summary>
    public AICapabilityType CapabilityType { get; protected set; }
    
    /// <summary>
    /// Model identifier (e.g., "gpt-4", "whisper-1", "text-embedding-3-small")
    /// </summary>
    public string ModelId { get; protected set; } = string.Empty;
    
    /// <summary>
    /// API endpoint URL (optional, uses provider default if not specified)
    /// </summary>
    public string? ApiEndpoint { get; protected set; }
    
    /// <summary>
    /// API key for this specific model (optional, falls back to workspace-level key)
    /// </summary>
    public string? ApiKey { get; protected set; }
    
    /// <summary>
    /// Whether this configuration is currently enabled
    /// </summary>
    public bool IsEnabled { get; protected set; }
    
    /// <summary>
    /// Priority order when multiple configurations exist for the same capability (lower = higher priority)
    /// </summary>
    public int Priority { get; protected set; }

    public OpenAIApiMode? OpenAIApiMode { get; protected set; }

    public decimal? InputCostPer1KTokens { get; protected set; }

    public decimal? OutputCostPer1KTokens { get; protected set; }
    
    /// <summary>
    /// Additional configuration as JSON (model-specific parameters, temperature, max_tokens, etc.)
    /// </summary>
    public string? ConfigurationJson { get; protected set; }
    
    protected AIModelConfiguration() { }
    
    public AIModelConfiguration(
        Guid id,
        Guid workspaceId,
        AICapabilityType capabilityType,
        string modelId,
        int priority = 0
    ) : base(id)
    {
        WorkspaceId = workspaceId;
        CapabilityType = capabilityType;
        ModelId = Check.NotNullOrWhiteSpace(modelId, nameof(modelId));
        Priority = priority;
        IsEnabled = true;
    }
    
    public void UpdateConfiguration(
        string modelId,
        string? apiEndpoint,
        string? apiKey,
        string? configurationJson,
        int priority,
        OpenAIApiMode? openAIApiMode = null,
        decimal? inputCostPer1KTokens = null,
        decimal? outputCostPer1KTokens = null
    )
    {
        ValidatePricing(inputCostPer1KTokens, nameof(inputCostPer1KTokens));
        ValidatePricing(outputCostPer1KTokens, nameof(outputCostPer1KTokens));

        ModelId = Check.NotNullOrWhiteSpace(modelId, nameof(modelId));
        ApiEndpoint = apiEndpoint;
        ApiKey = apiKey;
        ConfigurationJson = configurationJson;
        Priority = priority;
        OpenAIApiMode = openAIApiMode;
        InputCostPer1KTokens = inputCostPer1KTokens;
        OutputCostPer1KTokens = outputCostPer1KTokens;
    }
    
    public void Enable() => IsEnabled = true;
    public void Disable() => IsEnabled = false;
    
    public void SetPriority(int priority) => Priority = priority;

    private static void ValidatePricing(decimal? value, string parameterName)
    {
        if (value < 0)
        {
            throw new BusinessException("AIManagement:InvalidTokenPricing")
                .WithData("ParameterName", parameterName);
        }
    }
}
