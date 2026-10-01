using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Catalog;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Provider.AvalAI;

[ExposeServices(typeof(IAiProviderProfile))]
public class AvalAIProviderProfile : IAiProviderProfile, ITransientDependency
{
    public const string Catalog = "AvalAI";

    public AIProviderType ProviderType => AIProviderType.AvalAI;
    public AIProviderCapabilityKind CapabilityKind => AIProviderCapabilityKind.OpenAICompatible;
    public string DisplayName => "AvalAI";
    public string? DefaultBaseUrl => "https://api.avalai.ir/v1";
    public bool RequiresExplicitBaseUrl => false;
    public string? CatalogName => Catalog;
    public string ImageGenerationPath => "images/generations";
    public bool SupportsDecisions => false;

    public bool SupportsCapability(AICapabilityType capabilityType) => capabilityType != AICapabilityType.Decisions;

    public bool SupportsApiMode(OpenAIApiMode mode) =>
        mode is OpenAIApiMode.ChatCompletions or OpenAIApiMode.Responses;

    public void ApplyDefaultHeaders(HttpClient client)
    {
    }
}

[ExposeServices(typeof(IModelCatalogProvider))]
public class AvalAIModelCatalogSource : IModelCatalogProvider, ITransientDependency
{
    public const string HttpClientName = "SufiAI.ModelCatalog.AvalAI";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AvalAIModelCatalogSource> _logger;

    public AvalAIModelCatalogSource(IHttpClientFactory httpClientFactory, ILogger<AvalAIModelCatalogSource> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public string Name => AvalAIProviderProfile.Catalog;

    public async Task<IReadOnlyList<ModelCatalogEntry>?> TryListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClientFactory.CreateClient(HttpClientName).GetAsync("public/models", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("AvalAI catalog failed with status {StatusCode}.", (int)response.StatusCode);
                return null;
            }

            return Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "AvalAI catalog is unavailable.");
            return null;
        }
    }

    public Task<ModelCatalogLookup> TryFindAsync(string modelId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ModelCatalogLookup { Status = ModelCatalogLookupStatus.Unknown });
    }

    public static IReadOnlyList<ModelCatalogEntry> Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<ModelCatalogEntry>();
        }

        var entries = new List<ModelCatalogEntry>();
        foreach (var item in data.EnumerateArray())
        {
            var id = item.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            var mode = item.TryGetProperty("mode", out var modeElement) ? modeElement.GetString() : null;
            entries.Add(new ModelCatalogEntry
            {
                Id = id,
                Name = ReadString(item, "name"),
                IconUrl = ModelCatalogIconUrl.Read(item),
                Mode = mode,
                SupportedEndpoints = ReadStrings(item, "supported_endpoints")
            });
        }

        return entries;
    }

    private static string? ReadString(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static List<string> ReadStrings(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.Array)
        {
            return new List<string>();
        }

        return property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .ToList();
    }
}
