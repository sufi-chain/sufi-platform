namespace SufiChain.SufiPlatform.SufiAI.Catalog;

public sealed class OpenRouterCatalogEndpoint
{
    public string? BaseUrl { get; init; }

    public string? ApiKey { get; init; }
}

/// <summary>
/// Resolves the OpenRouter model-list host from a workspace connection.
/// Production egress blocks openrouter.ai, so the list must use the workspace base URL.
/// </summary>
public interface IOpenRouterCatalogEndpointResolver
{
    Task<OpenRouterCatalogEndpoint> ResolveAsync(CancellationToken cancellationToken = default);
}
