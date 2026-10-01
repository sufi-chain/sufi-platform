using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Catalog;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Provider.Liara;

[ExposeServices(typeof(IAiProviderProfile))]
public class LiaraProviderProfile : IAiProviderProfile, ITransientDependency
{
    public const string Catalog = "Liara";

    public AIProviderType ProviderType => AIProviderType.Liara;
    public AIProviderCapabilityKind CapabilityKind => AIProviderCapabilityKind.OpenAICompatible;
    public string DisplayName => "Liara";
    public string? DefaultBaseUrl => null;
    public bool RequiresExplicitBaseUrl => true;
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
public class LiaraModelCatalogSource : IModelCatalogProvider, ITransientDependency
{
    public const string HttpClientName = "SufiAI.ModelCatalog.Liara";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LiaraModelCatalogSource> _logger;

    public LiaraModelCatalogSource(IHttpClientFactory httpClientFactory, ILogger<LiaraModelCatalogSource> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public string Name => LiaraProviderProfile.Catalog;

    public async Task<IReadOnlyList<ModelCatalogEntry>?> TryListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClientFactory.CreateClient(HttpClientName).GetAsync("v1/models", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Liara catalog failed with status {StatusCode}.", (int)response.StatusCode);
                return null;
            }

            return Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Liara catalog is unavailable.");
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
        if (!document.RootElement.TryGetProperty("models", out var models) || models.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<ModelCatalogEntry>();
        }

        var entries = new List<ModelCatalogEntry>();
        foreach (var item in models.EnumerateArray())
        {
            var id = item.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            var architecture = item.TryGetProperty("architecture", out var architectureElement) ? architectureElement : default;
            var pricing = item.TryGetProperty("pricing", out var pricingElement) ? pricingElement : default;
            entries.Add(new ModelCatalogEntry
            {
                Id = id,
                Name = ReadString(item, "name"),
                IconUrl = ModelCatalogIconUrl.Read(item),
                InputModalities = ReadStrings(architecture, "input_modalities"),
                OutputModalities = ReadStrings(architecture, "output_modalities"),
                SupportedParameters = ReadStrings(item, "supported_parameters"),
                PromptPricePerToken = ReadDecimal(pricing, "prompt"),
                CompletionPricePerToken = ReadDecimal(pricing, "completion")
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

    private static decimal? ReadDecimal(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var number))
        {
            return number;
        }

        if (property.ValueKind == JsonValueKind.String &&
            decimal.TryParse(property.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return null;
    }
}
