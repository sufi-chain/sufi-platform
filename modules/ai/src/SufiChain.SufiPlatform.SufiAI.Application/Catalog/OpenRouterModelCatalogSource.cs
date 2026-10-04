using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Catalog;

[ExposeServices(typeof(IModelCatalogProvider))]
public class OpenRouterModelCatalogSource : IModelCatalogProvider, ITransientDependency
{
    public const string NameValue = "OpenRouter";
    public const string HttpClientName = "SufiAI.ModelCatalog.OpenRouter";
    public const string DefaultCatalogBaseUrl = "https://openrouter.ai/api/";
    public const string ListPath = "v1/models?output_modalities=all";

    /// <summary>
    /// Decision models such as Jev are omitted from the default text list.
    /// <c>all</c> is documented to include them, and this filter is the explicit list.
    /// </summary>
    public const string DecisionsListPath = "v1/models?output_modalities=decisions";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOpenRouterCatalogEndpointResolver _endpoints;
    private readonly ILogger<OpenRouterModelCatalogSource> _logger;

    public OpenRouterModelCatalogSource(
        IHttpClientFactory httpClientFactory,
        IOpenRouterCatalogEndpointResolver endpoints,
        ILogger<OpenRouterModelCatalogSource> logger)
    {
        _httpClientFactory = httpClientFactory;
        _endpoints = endpoints;
        _logger = logger;
    }

    public string Name => NameValue;

    public async Task<IReadOnlyList<ModelCatalogEntry>?> TryListAsync(CancellationToken cancellationToken = default)
    {
        var endpoint = await _endpoints.ResolveAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(endpoint.BaseUrl))
        {
            _logger.LogWarning(
                "OpenRouter model list has no workspace API base URL. Falling back to the catalog client. Set the workspace base URL, such as https://or-gateway.sufichain.com/v1, so production does not call openrouter.ai directly.");
        }

        var listedTask = TryGetListAsync(endpoint, "output_modalities=all", cancellationToken);
        var decisionsTask = TryGetListAsync(endpoint, "output_modalities=decisions", cancellationToken);
        await Task.WhenAll(listedTask, decisionsTask);
        return Merge(await listedTask, await decisionsTask);
    }

    private async Task<IReadOnlyList<ModelCatalogEntry>?> TryGetListAsync(
        OpenRouterCatalogEndpoint endpoint,
        string query,
        CancellationToken cancellationToken)
    {
        var requestUri = DescribeListTarget(endpoint.BaseUrl, query);
        try
        {
            using var request = CreateListRequest(endpoint, query);
            using var response = await CreateClient().SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Model catalog list failed. Path={Path}, Url={Url}, Status={StatusCode}.",
                    query,
                    requestUri,
                    (int)response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseList(json);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Model catalog list is unavailable. Path={Path}, Url={Url}.", query, requestUri);
            return null;
        }
    }

    private static HttpRequestMessage CreateListRequest(OpenRouterCatalogEndpoint endpoint, string query)
    {
        var request = string.IsNullOrWhiteSpace(endpoint.BaseUrl)
            ? new HttpRequestMessage(HttpMethod.Get, "v1/models?" + query)
            : new HttpRequestMessage(HttpMethod.Get, ModelsUri(endpoint.BaseUrl, query));
        if (!string.IsNullOrWhiteSpace(endpoint.ApiKey))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", endpoint.ApiKey);
        }

        return request;
    }

    private static string DescribeListTarget(string? workspaceBaseUrl, string query)
    {
        return string.IsNullOrWhiteSpace(workspaceBaseUrl)
            ? "v1/models?" + query
            : ModelsUri(workspaceBaseUrl, query);
    }

    /// <summary>
    /// Public catalog host. A trailing slash is required so <c>v1/models</c> is appended.
    /// Empty configuration stays on OpenRouter. A gateway root such as
    /// <c>https://or-gateway.sufichain.com/</c> rewrites <c>/v1/*</c> to <c>/api/v1/*</c>.
    /// </summary>
    public static Uri CatalogBaseAddress(string? catalogBaseUrl)
    {
        var root = string.IsNullOrWhiteSpace(catalogBaseUrl) ? DefaultCatalogBaseUrl : catalogBaseUrl.Trim();
        if (!root.EndsWith('/'))
        {
            root += "/";
        }

        return new Uri(root);
    }

    /// <summary>
    /// Models URL under a connection base such as <c>https://or-gateway.example/v1</c>.
    /// Workspace connections already include <c>/v1</c>.
    /// </summary>
    public static string ModelsUri(string baseUrl, string? query = null)
    {
        var root = baseUrl.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(query))
        {
            return root + "/models";
        }

        return root + "/models?" + query.Trim().TrimStart('?');
    }

    public static IReadOnlyList<ModelCatalogEntry>? Merge(
        IReadOnlyList<ModelCatalogEntry>? listed,
        IReadOnlyList<ModelCatalogEntry>? decisions)
    {
        if (listed == null && decisions == null)
        {
            return null;
        }

        var merged = new Dictionary<string, ModelCatalogEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in listed ?? Array.Empty<ModelCatalogEntry>())
        {
            merged[entry.Id] = entry;
        }

        foreach (var entry in decisions ?? Array.Empty<ModelCatalogEntry>())
        {
            merged[entry.Id] = entry;
        }

        return merged.Values.ToList();
    }

    public async Task<ModelCatalogLookup> TryFindAsync(string modelId, CancellationToken cancellationToken = default)
    {
        var id = ModelCatalogMatcher.StripNitro(modelId.Trim()).Trim('/');
        var endpoint = await _endpoints.ResolveAsync(cancellationToken);
        var path = string.IsNullOrWhiteSpace(endpoint.BaseUrl)
            ? "v1/model/" + id
            : endpoint.BaseUrl.Trim().TrimEnd('/') + "/model/" + id;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            if (!string.IsNullOrWhiteSpace(endpoint.ApiKey))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", endpoint.ApiKey);
            }

            using var response = await CreateClient().SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new ModelCatalogLookup { Status = ModelCatalogLookupStatus.Unknown };
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Model catalog lookup failed with status {StatusCode}.", (int)response.StatusCode);
                return new ModelCatalogLookup { Status = ModelCatalogLookupStatus.Unavailable };
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var entry = ParseSingle(json);
            return entry == null
                ? new ModelCatalogLookup { Status = ModelCatalogLookupStatus.Unknown }
                : new ModelCatalogLookup { Status = ModelCatalogLookupStatus.Found, Entry = entry };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Model catalog lookup is unavailable.");
            return new ModelCatalogLookup { Status = ModelCatalogLookupStatus.Unavailable };
        }
    }

    public static IReadOnlyList<ModelCatalogEntry> ParseList(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<ModelCatalogEntry>();
        }

        return data.EnumerateArray()
            .Select(ParseEntry)
            .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.Id))
            .Cast<ModelCatalogEntry>()
            .ToList();
    }

    public static ModelCatalogEntry? ParseSingle(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("data", out var data))
        {
            return null;
        }

        var entryElement = data.ValueKind == JsonValueKind.Object
            ? data
            : data.ValueKind == JsonValueKind.Array && data.GetArrayLength() > 0
                ? data[0]
                : default;
        return entryElement.ValueKind == JsonValueKind.Object ? ParseEntry(entryElement) : null;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);
        if (client.BaseAddress == null)
        {
            client.BaseAddress = CatalogBaseAddress(null);
        }

        return client;
    }

    private static ModelCatalogEntry? ParseEntry(JsonElement item)
    {
        var id = ReadString(item, "id");
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        JsonElement? architecture = item.TryGetProperty("architecture", out var architectureElement) &&
                                    architectureElement.ValueKind == JsonValueKind.Object
            ? architectureElement
            : null;
        JsonElement? pricing = item.TryGetProperty("pricing", out var pricingElement) &&
                               pricingElement.ValueKind == JsonValueKind.Object
            ? pricingElement
            : null;

        var outputModalities = architecture.HasValue
            ? ReadStrings(architecture.Value, "output_modalities")
            : new List<string>();
        var modality = architecture.HasValue ? ReadString(architecture.Value, "modality") : null;
        var reasoning = ReadReasoning(item);
        return new ModelCatalogEntry
        {
            Id = id,
            Name = ReadString(item, "name"),
            IconUrl = ModelCatalogIconUrl.Read(item),
            Mode = IsDecisions(outputModalities, modality) ? "decisions" : ReadString(item, "mode"),
            InputModalities = architecture.HasValue ? ReadStrings(architecture.Value, "input_modalities") : new List<string>(),
            OutputModalities = outputModalities,
            SupportedParameters = ReadStrings(item, "supported_parameters"),
            ContextLength = ReadContextLength(item),
            ReasoningEfforts = reasoning.Efforts,
            DefaultReasoningEffort = reasoning.DefaultEffort,
            PromptPricePerToken = pricing.HasValue ? ReadDecimal(pricing.Value, "prompt") : null,
            CompletionPricePerToken = pricing.HasValue ? ReadDecimal(pricing.Value, "completion") : null,
            ImagePrice = pricing.HasValue ? ReadDecimal(pricing.Value, "image") : null,
            ImageOutputPrice = pricing.HasValue ? ReadDecimal(pricing.Value, "image_output") : null,
            ImageTokenPrice = pricing.HasValue ? ReadDecimal(pricing.Value, "image_token") : null,
            RequestPrice = pricing.HasValue ? ReadDecimal(pricing.Value, "request") : null,
            WebSearchPrice = pricing.HasValue ? ReadDecimal(pricing.Value, "web_search") : null
        };
    }

    public static int? ReadContextLength(JsonElement item)
    {
        var length = ReadPositiveInt(item, "context_length");
        if (length != null)
        {
            return length;
        }

        return item.TryGetProperty("top_provider", out var top) && top.ValueKind == JsonValueKind.Object
            ? ReadPositiveInt(top, "context_length")
            : null;
    }

    public static (List<string> Efforts, string? DefaultEffort) ReadReasoning(JsonElement item)
    {
        if (!item.TryGetProperty("reasoning", out var reasoning) || reasoning.ValueKind != JsonValueKind.Object)
        {
            return (new List<string>(), null);
        }

        return (ReadStrings(reasoning, "supported_efforts"), ReadString(reasoning, "default_effort"));
    }

    private static bool IsDecisions(IReadOnlyList<string> outputModalities, string? modality)
    {
        if (outputModalities.Any(value => string.Equals(value, "decisions", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(modality) &&
               modality.Contains("decisions", StringComparison.OrdinalIgnoreCase);
    }

    private static List<string> ReadStrings(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
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

    private static decimal? ReadDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
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

    private static int? ReadPositiveInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var number) && number > 0)
        {
            return number;
        }

        if (property.ValueKind == JsonValueKind.String &&
            int.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) &&
            parsed > 0)
        {
            return parsed;
        }

        return null;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }
}
