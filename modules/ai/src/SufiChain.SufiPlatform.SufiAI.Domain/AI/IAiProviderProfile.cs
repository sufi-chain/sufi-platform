using System.Net.Http;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Product profile for a workspace provider. The capability kind selects the executor.
/// </summary>
public interface IAiProviderProfile
{
    AIProviderType ProviderType { get; }

    AIProviderCapabilityKind CapabilityKind { get; }

    string DisplayName { get; }

    string? DefaultBaseUrl { get; }

    bool RequiresExplicitBaseUrl { get; }

    /// <summary>Name of the <see cref="Catalog.IModelCatalogProvider"/>, or null when this profile has no public catalog.</summary>
    string? CatalogName { get; }

    /// <summary>Path appended to the base URL. OpenAI uses images/generations. OpenRouter uses images.</summary>
    string ImageGenerationPath { get; }

    bool SupportsDecisions { get; }

    bool SupportsCapability(AICapabilityType capabilityType);

    bool SupportsApiMode(OpenAIApiMode mode);

    void ApplyDefaultHeaders(HttpClient client);
}
