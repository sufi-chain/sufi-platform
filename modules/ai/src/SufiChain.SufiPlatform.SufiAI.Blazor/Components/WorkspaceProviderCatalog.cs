using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Components;

internal static class WorkspaceProviderCatalog
{
    public static List<AiProviderProfileDto> Fallback()
    {
        return
        [
            Profile(AIProviderType.OpenAI, "OpenAI", "https://api.openai.com/v1", requiresBaseUrl: false, supportsDecisions: false),
            Profile(AIProviderType.OpenAICompatible, "OpenAI-compatible", defaultBaseUrl: null, requiresBaseUrl: true, supportsDecisions: false),
            Profile(AIProviderType.OpenRouter, "OpenRouter", "https://openrouter.ai/api/v1", requiresBaseUrl: false, supportsDecisions: true),
            Profile(AIProviderType.HuggingFace, "Hugging Face", "https://router.huggingface.co/v1", requiresBaseUrl: false, supportsDecisions: false),
            Profile(AIProviderType.AvalAI, "AvalAI", "https://api.avalai.ir/v1", requiresBaseUrl: false, supportsDecisions: false),
            Profile(AIProviderType.Liara, "Liara", defaultBaseUrl: null, requiresBaseUrl: true, supportsDecisions: false)
        ];
    }

    private static AiProviderProfileDto Profile(
        AIProviderType providerType,
        string displayName,
        string? defaultBaseUrl,
        bool requiresBaseUrl,
        bool supportsDecisions)
    {
        return new AiProviderProfileDto
        {
            ProviderType = providerType,
            CapabilityKind = AIProviderCapabilityKind.OpenAICompatible,
            DisplayName = displayName,
            DefaultBaseUrl = defaultBaseUrl,
            RequiresExplicitBaseUrl = requiresBaseUrl,
            SupportsDecisions = supportsDecisions
        };
    }
}
