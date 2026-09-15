using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Web;

public interface IWebSearchService
{
    Task<WebSearchResponse> SearchAsync(WebSearchRequest request, WebResearchOptions options, CancellationToken cancellationToken = default);
}

public class SearXngWebSearchService : IWebSearchService, ITransientDependency
{
    public virtual async Task<WebSearchResponse> SearchAsync(WebSearchRequest request, WebResearchOptions options, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(options.Endpoint, UriKind.Absolute, out var endpoint) || endpoint.Scheme != "https" ||
            endpoint.UserInfo.Length != 0 || endpoint.Query.Length != 0 || endpoint.Fragment.Length != 0)
            throw new WebResearchException("SearchNotConfigured");
        if (string.IsNullOrWhiteSpace(request.Query)) throw new WebResearchException("QueryRequired");
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(options.SearchTimeoutSeconds));
        try
        {
            // Administrator-configured service endpoint is trusted; result-page fetching is isolated.
            using var client = CreateClient();
            var url = options.Endpoint.TrimEnd('/') + "/search?q=" + Uri.EscapeDataString(WebContentFetcher.Clean(request.Query, 2000)) +
                "&format=json&pageno=1&language=" + Uri.EscapeDataString(request.Culture ?? "auto") +
                "&safesearch=" + options.SafeSearch.ToString(CultureInfo.InvariantCulture);
            if (request.TimeRange is "day" or "month" or "year") url += "&time_range=" + request.TimeRange;
            using var message = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrWhiteSpace(options.Token)) message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Token);
            using var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, deadline.Token);
            if (!response.IsSuccessStatusCode) throw new WebResearchException("SearchUnavailable");
            var bytes = await WebContentFetcher.ReadBoundedAsync(response.Content, 1_048_576, deadline.Token);
            if (bytes.Length > 1_048_576) throw new WebResearchException("SearchResponseTooLarge");
            using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 32 });
            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty("results", out var items) || items.ValueKind != JsonValueKind.Array)
                throw new WebResearchException("InvalidSearchResponse");
            var result = new WebSearchResponse { ModelId = "searxng" };
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object) continue;
                string urlValue;
                try { urlValue = PublicWebUrl.Parse(Text(item, "url", 4096)).AbsoluteUri; }
                catch (WebResearchException) { continue; }
                if (!seen.Add(urlValue)) continue;
                result.Results.Add(new WebSearchResult
                {
                    Url = urlValue, Title = Text(item, "title", 512), Snippet = Text(item, "content", 2000),
                    PublishedAt = DateTimeOffset.TryParse(Text(item, "publishedDate", 100), CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal, out var date) ? date : null,
                    Rank = result.Results.Count + 1, Source = "searxng"
                });
                if (result.Results.Count >= Math.Min(options.MaxSearchResults, Math.Clamp(request.MaxResults, 1, 10))) break;
            }
            return result;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { throw new WebResearchException("SearchTimeout"); }
        catch (HttpRequestException) { throw new WebResearchException("SearchUnavailable"); }
        catch (IOException) { throw new WebResearchException("SearchUnavailable"); }
        catch (FormatException) { throw new WebResearchException("SearchNotConfigured"); }
        catch (JsonException) { throw new WebResearchException("InvalidSearchResponse"); }
    }

    protected virtual HttpClient CreateClient() => new(new SocketsHttpHandler
        { AllowAutoRedirect = false, UseCookies = false, UseProxy = false }) { Timeout = Timeout.InfiniteTimeSpan };

    private static string Text(JsonElement item, string key, int max) => item.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String
        ? WebContentFetcher.Clean(value.GetString(), max) : "";
}
