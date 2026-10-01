using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Catalog;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Provider.HuggingFace;

[ExposeServices(typeof(IAiProviderProfile))]
public class HuggingFaceProviderProfile : IAiProviderProfile, ITransientDependency
{
    public const string Catalog = "HuggingFace";

    public AIProviderType ProviderType => AIProviderType.HuggingFace;
    public AIProviderCapabilityKind CapabilityKind => AIProviderCapabilityKind.OpenAICompatible;
    public string DisplayName => "Hugging Face";
    public string? DefaultBaseUrl => "https://router.huggingface.co/v1";
    public bool RequiresExplicitBaseUrl => false;
    public string? CatalogName => Catalog;
    public string ImageGenerationPath => "images/generations";
    public bool SupportsDecisions => false;

    public bool SupportsCapability(AICapabilityType capabilityType)
    {
        return capabilityType is AICapabilityType.ChatCompletion
            or AICapabilityType.VisionAnalysis
            or AICapabilityType.WebSearch
            or AICapabilityType.WebFetch;
    }

    public bool SupportsApiMode(OpenAIApiMode mode) => mode == OpenAIApiMode.ChatCompletions;

    public void ApplyDefaultHeaders(HttpClient client)
    {
    }
}

[ExposeServices(typeof(IModelCatalogProvider))]
public class HuggingFaceModelCatalogSource : IModelCatalogProvider, ITransientDependency
{
    public const string HttpClientName = "SufiAI.ModelCatalog.HuggingFace";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HuggingFaceModelCatalogSource> _logger;

    public HuggingFaceModelCatalogSource(IHttpClientFactory httpClientFactory, ILogger<HuggingFaceModelCatalogSource> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public string Name => HuggingFaceProviderProfile.Catalog;

    public async Task<IReadOnlyList<ModelCatalogEntry>?> TryListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClientFactory.CreateClient(HttpClientName).GetAsync("v1/models", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Hugging Face catalog failed with status {StatusCode}.", (int)response.StatusCode);
                return null;
            }

            return Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Hugging Face catalog is unavailable.");
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

            var architecture = item.TryGetProperty("architecture", out var architectureElement) ? architectureElement : default;
            entries.Add(new ModelCatalogEntry
            {
                Id = id,
                Name = ReadString(item, "name"),
                IconUrl = ModelCatalogIconUrl.Read(item),
                InputModalities = ReadStrings(architecture, "input_modalities"),
                OutputModalities = ReadStrings(architecture, "output_modalities")
            });
        }

        return entries;
    }

    private static string? ReadString(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object &&
               element.TryGetProperty(name, out var property) &&
               property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static List<string> ReadStrings(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(name, out var property) ||
            property.ValueKind != JsonValueKind.Array)
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
