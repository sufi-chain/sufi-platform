using Volo.Abp;
using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

/// <summary>
/// Classifies provider HTTP failures that mean the configured model is gone.
/// The raw provider body never becomes a user-facing message.
/// </summary>
public static class EmbeddingProviderFailures
{
    public const string CapabilityEmbedding = "Embedding";
    public const string CapabilityChat = "Chat";
    public const string CapabilityDecision = "Decision";

    public static bool IsRetired(int statusCode, string? body)
    {
        if (statusCode == 410)
        {
            return true;
        }

        if (statusCode is not (400 or 404))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return false;
        }

        return body.Contains("end of life", StringComparison.OrdinalIgnoreCase)
            || body.Contains("retired", StringComparison.OrdinalIgnoreCase)
            || body.Contains("deprecated", StringComparison.OrdinalIgnoreCase)
            || body.Contains("model not found", StringComparison.OrdinalIgnoreCase)
            || body.Contains("no endpoints found", StringComparison.OrdinalIgnoreCase)
            || body.Contains("no provider serves", StringComparison.OrdinalIgnoreCase);
    }

    public static string SafeMessage(string model, string capability) =>
        $"The {capability} model '{model}' was retired by the provider. Choose a new model in Workspace model configurations, then re-index.";

    public static int ResolveDimensions(string? modelId, int? configuredDimensions)
    {
        if (configuredDimensions is > 0)
        {
            return configuredDimensions.Value;
        }

        if (EmbeddingModelDefaults.IsKnownModel(modelId))
        {
            return EmbeddingModelDefaults.GetDimensions(modelId);
        }

        throw new BusinessException(AIErrorCodes.EmbeddingDimensionsRequired)
            .WithData("Model", modelId ?? string.Empty);
    }
}

public sealed class EmbeddingModelRetiredException : Exception
{
    public EmbeddingModelRetiredException(string model, string capability)
        : base(EmbeddingProviderFailures.SafeMessage(model, capability))
    {
        Model = model;
        Capability = capability;
    }

    public string Model { get; }

    public string Capability { get; }
}
