using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Represents a specific AI model configuration for a capability within a workspace.
/// A workspace can have multiple configurations (e.g., GPT-4 for chat, Whisper for audio).
/// </summary>
public class AIModelConfiguration : AuditedEntity<Guid>
{
    public const int DefaultMaxContextTokens = AIModelConfigurationConsts.DefaultMaxContextTokens;
    public const int MaxDisplayNameLength = 256;
    public const int MaxDescriptionLength = 1024;

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
    /// Label shown in the end-user selector. Falls back to <see cref="ModelId"/> when empty.
    /// </summary>
    public string? DisplayName { get; protected set; }

    /// <summary>
    /// Optional hint rendered in the selector.
    /// </summary>
    public string? Description { get; protected set; }

    /// <summary>
    /// When true, opted-in routes may appear in the end-user catalog. Default is false.
    /// </summary>
    public bool IsUserSelectable { get; protected set; }
    
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

    public OpenAIApiMode OpenAIApiMode { get; protected set; }

    /// <summary>
    /// Context window for this model route.
    /// </summary>
    public int MaxContextTokens { get; protected set; }

    public decimal? InputCostPer1MTokens { get; protected set; }

    public decimal? OutputCostPer1MTokens { get; protected set; }
   
    public int? Dimensions { get; protected set; }
    
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
        IsUserSelectable = false;
        OpenAIApiMode = OpenAIApiMode.ChatCompletions;
        MaxContextTokens = DefaultMaxContextTokens;
    }
    
    public void UpdateConfiguration(
        string modelId,
        string? apiEndpoint,
        string? apiKey,
        int priority,
        OpenAIApiMode openAIApiMode = OpenAIApiMode.ChatCompletions,
        decimal? inputCostPer1MTokens = null,
        decimal? outputCostPer1MTokens = null,
        int? dimensions = null,
        string? displayName = null,
        bool isUserSelectable = false,
        string? description = null,
        int maxContextTokens = DefaultMaxContextTokens
   )
   {
        ValidatePricing(inputCostPer1MTokens, nameof(inputCostPer1MTokens));
       ValidatePricing(outputCostPer1MTokens, nameof(outputCostPer1MTokens));
        ValidateDimensions(dimensions);

       ModelId = Check.NotNullOrWhiteSpace(modelId, nameof(modelId));
       DisplayName = NormalizeOptionalText(displayName, MaxDisplayNameLength, nameof(displayName));
       Description = NormalizeOptionalText(description, MaxDescriptionLength, nameof(description));
       IsUserSelectable = isUserSelectable;
       ApiEndpoint = apiEndpoint;
       ApiKey = apiKey;
       Priority = priority;
       OpenAIApiMode = openAIApiMode;
       MaxContextTokens = NormalizeMaxContextTokens(maxContextTokens);
       InputCostPer1MTokens = inputCostPer1MTokens;
       OutputCostPer1MTokens = outputCostPer1MTokens;
        Dimensions = dimensions;
   }

    /// <summary>
    /// Legacy rows from the selectable-route migration stored <c>0</c>. Treat that as unset.
    /// </summary>
    public static int NormalizeMaxContextTokens(int maxContextTokens)
    {
        if (maxContextTokens < 0)
        {
            throw new BusinessException(AIErrorCodes.InvalidMaxContextTokens)
                .WithData("MaxContextTokens", maxContextTokens);
        }

        return maxContextTokens < 1 ? DefaultMaxContextTokens : maxContextTokens;
    }

    public void Disable() => IsEnabled = false;

   private static void ValidatePricing(decimal? value, string parameterName)
   {
       if (value < 0)
       {
           throw new BusinessException("AI:InvalidTokenPricing")
               .WithData("ParameterName", parameterName);
       }
   }
 
    private static void ValidateDimensions(int? dimensions)
    {
        if (dimensions is <= 0)
        {
            throw new BusinessException("AI:InvalidEmbeddingDimensions");
        }
    }

    private static string? NormalizeOptionalText(string? value, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Check.Length(value.Trim(), parameterName, maxLength);
    }
}
