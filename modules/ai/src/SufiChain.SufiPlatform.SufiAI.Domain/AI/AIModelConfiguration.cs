using System;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>Route price in <see cref="InputPriceUnit"/>. Null inherits the workspace token price only when the unit is per million tokens.</summary>
    public decimal? InputPrice { get; protected set; }

    public AIPriceUnit InputPriceUnit { get; protected set; }

    /// <summary>Route price in <see cref="OutputPriceUnit"/>.</summary>
    public decimal? OutputPrice { get; protected set; }

    public AIPriceUnit OutputPriceUnit { get; protected set; }

    public bool? AcceptsImageInput { get; protected set; }

    public bool? AcceptsFileInput { get; protected set; }

    public bool? SupportsReasoning { get; protected set; }

    public string? ReasoningEfforts { get; protected set; }

    public string? DefaultReasoningEffort { get; protected set; }

    public ModelCapabilitySource? CapabilitySource { get; protected set; }

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
        decimal? inputPrice = null,
        decimal? outputPrice = null,
        int? dimensions = null,
        string? displayName = null,
        bool isUserSelectable = false,
        string? description = null,
        int maxContextTokens = DefaultMaxContextTokens,
        AIPriceUnit inputPriceUnit = AIPriceUnit.PerMillionTokens,
        AIPriceUnit outputPriceUnit = AIPriceUnit.PerMillionTokens
   )
   {
        ValidatePricing(inputPrice, nameof(inputPrice));
        ValidatePricing(outputPrice, nameof(outputPrice));
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
        InputPrice = inputPrice;
        OutputPrice = outputPrice;
        InputPriceUnit = inputPriceUnit;
        OutputPriceUnit = outputPriceUnit;
        Dimensions = dimensions;
   }

    public void SetChatCapabilities(
        bool? acceptsImageInput,
        bool? acceptsFileInput,
        bool? supportsReasoning,
        string? reasoningEfforts,
        string? defaultReasoningEffort,
        ModelCapabilitySource? capabilitySource)
    {
        if (CapabilityType != AICapabilityType.ChatCompletion)
        {
            AcceptsImageInput = null;
            AcceptsFileInput = null;
            SupportsReasoning = null;
            ReasoningEfforts = null;
            DefaultReasoningEffort = null;
            CapabilitySource = null;
            return;
        }

        AcceptsImageInput = acceptsImageInput;
        AcceptsFileInput = acceptsFileInput;
        SupportsReasoning = supportsReasoning;
        ReasoningEfforts = NormalizeEffortList(reasoningEfforts);
        DefaultReasoningEffort = NormalizeEffort(defaultReasoningEffort);
        if (!string.IsNullOrEmpty(DefaultReasoningEffort) &&
            !GetReasoningEffortList().Contains(DefaultReasoningEffort, StringComparer.OrdinalIgnoreCase))
        {
            throw new BusinessException(AIErrorCodes.ReasoningEffortNotAllowed)
                .WithData("ReasoningEffort", DefaultReasoningEffort);
        }

        CapabilitySource = capabilitySource;
    }

    public void CopyChatCapabilitiesFrom(AIModelConfiguration? source)
    {
        if (source == null)
        {
            return;
        }

        AcceptsImageInput = source.AcceptsImageInput;
        AcceptsFileInput = source.AcceptsFileInput;
        SupportsReasoning = source.SupportsReasoning;
        ReasoningEfforts = source.ReasoningEfforts;
        DefaultReasoningEffort = source.DefaultReasoningEffort;
        CapabilitySource = source.CapabilitySource;
    }

    public void ReplaceCatalogPrices(CatalogPriceQuote quote)
    {
        ValidatePricing(quote.InputPrice, nameof(quote.InputPrice));
        ValidatePricing(quote.OutputPrice, nameof(quote.OutputPrice));
        InputPrice = quote.InputPrice;
        OutputPrice = quote.OutputPrice;
        InputPriceUnit = quote.InputUnit;
        OutputPriceUnit = quote.OutputUnit;
    }

    public IReadOnlyList<string> GetReasoningEffortList()
    {
        if (string.IsNullOrWhiteSpace(ReasoningEfforts))
        {
            return Array.Empty<string>();
        }

        return ReasoningEfforts.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;

    public void SetPriority(int priority) => Priority = priority;

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

    private static string? NormalizeEffortList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var efforts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return efforts.Length == 0 ? null : Check.Length(string.Join(",", efforts), nameof(ReasoningEfforts), 512);
    }

    private static string? NormalizeEffort(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Check.Length(value.Trim(), nameof(DefaultReasoningEffort), 64);
    }
}
