using System.Net.Http;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.SufiAI;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Provider.OpenRouter;

public class OpenRouterOptions
{
    public string? HttpReferer { get; set; }

    public string? Title { get; set; }

    /// <summary>
    /// OpenAI-style site used by the public model catalog. Empty uses
    /// <c>https://openrouter.ai/api/</c>. A gateway such as
    /// <c>https://or-gateway.sufichain.com/</c> is used when direct OpenRouter is blocked.
    /// The catalog client appends <c>v1/models</c>.
    /// </summary>
    public string? CatalogBaseUrl { get; set; }
}

[ExposeServices(typeof(IAiProviderProfile))]
public class OpenRouterProviderProfile : IAiProviderProfile, ITransientDependency
{
    private readonly OpenRouterOptions _options;

    public OpenRouterProviderProfile(IOptions<OpenRouterOptions> options)
    {
        _options = options.Value;
    }

    public AIProviderType ProviderType => AIProviderType.OpenRouter;

    public AIProviderCapabilityKind CapabilityKind => AIProviderCapabilityKind.OpenAICompatible;

    public string DisplayName => "OpenRouter";

    public string? DefaultBaseUrl => "https://openrouter.ai/api/v1";

    public bool RequiresExplicitBaseUrl => false;

    public string? CatalogName => "OpenRouter";

    public string ImageGenerationPath => "images";

    public bool SupportsDecisions => true;

    public bool SupportsCapability(AICapabilityType capabilityType)
    {
        return true;
    }

    public bool SupportsApiMode(OpenAIApiMode mode)
    {
        return mode is OpenAIApiMode.ChatCompletions or OpenAIApiMode.Responses;
    }

    public void ApplyDefaultHeaders(HttpClient client)
    {
        if (!string.IsNullOrWhiteSpace(_options.HttpReferer))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("HTTP-Referer", _options.HttpReferer);
        }

        if (!string.IsNullOrWhiteSpace(_options.Title))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-OpenRouter-Title", _options.Title);
        }
    }
}
