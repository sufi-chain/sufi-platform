using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Web;

public interface IWebContentFetcher
{
    Task<WebFetchResponse> FetchAsync(WebFetchRequest request, CancellationToken cancellationToken = default);
}

public class WebContentFetcher : IWebContentFetcher, ITransientDependency
{
    public virtual async Task<WebFetchResponse> FetchAsync(WebFetchRequest request, CancellationToken cancellationToken = default)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(request.TimeoutSeconds, 2, 30)));
        var ct = deadline.Token;
        try
        {
            var uri = PublicWebUrl.Parse(request.Url);
            using var client = CreateClient();
            for (var redirects = 0; redirects <= 3; redirects++)
            {
                using var message = new HttpRequestMessage(HttpMethod.Get, uri);
                message.Headers.UserAgent.ParseAdd("SufiAI/1.0 (+web-research)");
                using var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, ct);
                if ((int)response.StatusCode is 301 or 302 or 303 or 307 or 308)
                {
                    if (redirects == 3 || response.Headers.Location == null)
                        throw new WebResearchException("RedirectLimit");
                    uri = PublicWebUrl.Parse(new Uri(uri, response.Headers.Location).AbsoluteUri);
                    continue;
                }
                if (!response.IsSuccessStatusCode) throw new WebResearchException("FetchFailed");
                var type = response.Content.Headers.ContentType?.MediaType;
                if (type is not ("text/html" or "text/plain" or "application/xhtml+xml") ||
                    response.Content.Headers.ContentEncoding.Count != 0 ||
                    string.Equals(response.Content.Headers.ContentDisposition?.DispositionType, "attachment", StringComparison.OrdinalIgnoreCase))
                    throw new WebResearchException("ContentTypeNotSupported");
                var maxBytes = Math.Clamp(request.MaxBytes, 1024, 5_242_880);
                var bytes = await ReadBoundedAsync(response.Content, maxBytes, ct);
                var encoding = Encoding.UTF8;
                var charset = response.Content.Headers.ContentType?.CharSet?.Trim('"');
                if (!string.IsNullOrWhiteSpace(charset))
                {
                    try { encoding = Encoding.GetEncoding(charset); }
                    catch (ArgumentException) { /* UTF-8 fallback for unsupported encodings. */ }
                    catch (NotSupportedException) { /* Optional code-page provider is not installed. */ }
                }
                var content = encoding.GetString(bytes.AsSpan(0, Math.Min(bytes.Length, maxBytes)));
                string? title = null;
                if (type != "text/plain")
                {
                    var document = await new HtmlParser().ParseDocumentAsync(content, ct);
                    title = Clean(document.Title, 512);
                    foreach (var element in document.QuerySelectorAll("script,style,noscript,nav,header,footer,form,iframe,svg,template,[hidden],[aria-hidden=true],input,button"))
                        element.Remove();
                    foreach (var element in document.QuerySelectorAll("[style]"))
                    {
                        var style = element.GetAttribute("style")?.Replace(" ", "").ToLowerInvariant() ?? "";
                        if (style.Contains("display:none") || style.Contains("visibility:hidden")) element.Remove();
                    }
                    foreach (var element in document.QuerySelectorAll("p,div,section,h1,h2,h3,h4,h5,h6,li,br,tr"))
                        element.Parent?.InsertBefore(document.CreateTextNode("\n"), element);
                    content = (document.QuerySelector("article,main") ?? document.Body ?? document.DocumentElement).TextContent;
                }
                if (string.IsNullOrWhiteSpace(content)) throw new WebResearchException("EmptyPage");
                return new WebFetchResponse
                {
                    Url = request.Url, CanonicalUrl = uri.AbsoluteUri, Title = title,
                    Content = Clean(content, 24_000), ContentType = type,
                    Truncated = bytes.Length > maxBytes || content.Length > 24_000,
                    RetrievedAt = DateTime.UtcNow, StatusCode = (int)response.StatusCode
                };
            }
            throw new WebResearchException("RedirectLimit");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        { throw new WebResearchException("FetchTimeout"); }
        catch (HttpRequestException) { throw new WebResearchException("FetchFailed"); }
        catch (SocketException) { throw new WebResearchException("FetchFailed"); }
        catch (IOException) { throw new WebResearchException("FetchFailed"); }
    }

    protected virtual HttpClient CreateClient() => new(new SocketsHttpHandler
    {
        AllowAutoRedirect = false, UseProxy = false, UseCookies = false,
        AutomaticDecompression = DecompressionMethods.None,
        ConnectCallback = ConnectPublicAsync, MaxResponseHeadersLength = 32
    }) { Timeout = Timeout.InfiniteTimeSpan };

    protected virtual Task<IPAddress[]> ResolveAddressesAsync(string host, CancellationToken ct) => Dns.GetHostAddressesAsync(host, ct);

    private async ValueTask<Stream> ConnectPublicAsync(SocketsHttpConnectionContext context, CancellationToken ct)
    {
        var addresses = await ResolveAddressesAsync(context.DnsEndPoint.Host, ct);
        if (addresses.Length == 0 || addresses.Any(address => !PublicWebUrl.IsPublic(address)))
            throw new WebResearchException("UrlNotAllowed");
        // Connect directly to the already validated address. TLS still validates the original hostname.
        var socket = new Socket(addresses[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            await socket.ConnectAsync(new IPEndPoint(addresses[0], context.DnsEndPoint.Port), ct);
            return new NetworkStream(socket, ownsSocket: true);
        }
        catch { socket.Dispose(); throw; }
    }

    internal static async Task<byte[]> ReadBoundedAsync(HttpContent content, int maxBytes, CancellationToken ct)
    {
        await using var source = await content.ReadAsStreamAsync(ct);
        using var result = new MemoryStream();
        var buffer = new byte[8192];
        while (result.Length <= maxBytes)
        {
            var count = await source.ReadAsync(buffer.AsMemory(0, Math.Min(buffer.Length, maxBytes + 1 - (int)result.Length)), ct);
            if (count == 0) break;
            await result.WriteAsync(buffer.AsMemory(0, count), ct);
        }
        return result.ToArray();
    }

    public static string Clean(string? value, int limit) => new((value ?? "").Take(limit)
        .Where(c => !char.IsControl(c) || c is '\n' or '\t').ToArray());
}
