using System.Net.Http;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI;

[ExposeServices(typeof(IAiProviderProfile))]
public class OpenAIProviderProfile : IAiProviderProfile, ITransientDependency
{
    public AIProviderType ProviderType => AIProviderType.OpenAI;

    public AIProviderCapabilityKind CapabilityKind => AIProviderCapabilityKind.OpenAICompatible;

    public string DisplayName => "OpenAI";

    public string? DefaultBaseUrl => "https://api.openai.com/v1";

    public bool RequiresExplicitBaseUrl => false;

    public string? CatalogName => null;

    public string ImageGenerationPath => "images/generations";

    public bool SupportsDecisions => false;

    public bool SupportsCapability(AICapabilityType capabilityType)
    {
        return capabilityType != AICapabilityType.Decisions;
    }

    public bool SupportsApiMode(OpenAIApiMode mode)
    {
        return mode is OpenAIApiMode.ChatCompletions or OpenAIApiMode.Responses;
    }

    public void ApplyDefaultHeaders(HttpClient client)
    {
    }
}

[ExposeServices(typeof(IAiProviderProfile))]
public class OpenAICompatibleProviderProfile : IAiProviderProfile, ITransientDependency
{
    public AIProviderType ProviderType => AIProviderType.OpenAICompatible;

    public AIProviderCapabilityKind CapabilityKind => AIProviderCapabilityKind.OpenAICompatible;

    public string DisplayName => "OpenAI-compatible";

    public string? DefaultBaseUrl => null;

    public bool RequiresExplicitBaseUrl => true;

    public string? CatalogName => null;

    public string ImageGenerationPath => "images/generations";

    public bool SupportsDecisions => false;

    public bool SupportsCapability(AICapabilityType capabilityType)
    {
        return capabilityType != AICapabilityType.Decisions;
    }

    public bool SupportsApiMode(OpenAIApiMode mode)
    {
        return mode is OpenAIApiMode.ChatCompletions or OpenAIApiMode.Responses;
    }

    public void ApplyDefaultHeaders(HttpClient client)
    {
    }
}

public static class AiProviderProfiles
{
    public static IAiProviderProfile? Find(IEnumerable<IAiProviderProfile> profiles, AIProviderType providerType)
    {
        return profiles.FirstOrDefault(profile => profile.ProviderType == providerType);
    }

    public static bool IsOpenAICompatible(IEnumerable<IAiProviderProfile> profiles, AIProviderType providerType)
    {
        var profile = Find(profiles, providerType);
        return profile == null
            ? providerType == AIProviderType.OpenAI
            : profile.CapabilityKind == AIProviderCapabilityKind.OpenAICompatible;
    }
}
