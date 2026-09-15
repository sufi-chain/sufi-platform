using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.Web;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

public class WebResearchTests
{
    [Theory]
    [InlineData("http://127.0.0.1")]
    [InlineData("http://2130706433")]
    [InlineData("http://169.254.169.254/latest/meta-data")]
    [InlineData("http://10.0.0.30")]
    [InlineData("http://[::1]")]
    [InlineData("http://[::ffff:127.0.0.1]")]
    [InlineData("https://user:password@example.org")]
    [InlineData("https://example.org:8443")]
    [InlineData("file:///etc/passwd")]
    public void Unsafe_urls_are_rejected(string url) => Assert.Throws<WebResearchException>(() => PublicWebUrl.Parse(url));

    [Theory]
    [InlineData("100.64.0.1")]
    [InlineData("198.18.0.1")]
    [InlineData("192.0.2.1")]
    [InlineData("224.0.0.1")]
    [InlineData("fc00::1")]
    [InlineData("fe80::1")]
    [InlineData("2001:db8::1")]
    [InlineData("2002:7f00:1::")]
    public void Special_use_addresses_are_rejected(string address) => Assert.False(PublicWebUrl.IsPublic(IPAddress.Parse(address)));

    [Fact]
    public void Public_addresses_and_fragment_normalization_are_supported()
    {
        Assert.True(PublicWebUrl.IsPublic(IPAddress.Parse("8.8.8.8")));
        Assert.True(PublicWebUrl.IsPublic(IPAddress.Parse("2001:4860:4860::8888")));
        Assert.Equal("https://example.org/article", PublicWebUrl.Parse("https://example.org/article#section").AbsoluteUri);
    }

    [Fact]
    public async Task Redirect_to_private_address_is_rejected_before_second_request()
    {
        var calls = 0;
        var fetcher = new TestFetcher(new Handler(_ =>
        {
            calls++;
            var response = new HttpResponseMessage(HttpStatusCode.Redirect);
            response.Headers.Location = new Uri("http://10.0.0.30/private");
            return response;
        }));
        await Assert.ThrowsAsync<WebResearchException>(() => fetcher.FetchAsync(new() { Url = "https://example.org" }));
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Extraction_removes_active_hidden_and_navigation_content()
    {
        var fetcher = new TestFetcher(new Handler(_ => new(HttpStatusCode.OK)
        { Content = new StringContent("<html><title>Title</title><body><script>evil()</script><nav>menu</nav><p hidden>secret</p><p>Hello &amp; world</p></body></html>", System.Text.Encoding.UTF8, "text/html") }));
        var result = await fetcher.FetchAsync(new() { Url = "https://example.org" });
        Assert.Equal("Title", result.Title);
        Assert.Contains("Hello & world", result.Content);
        Assert.DoesNotContain("evil", result.Content);
        Assert.DoesNotContain("secret", result.Content);
        Assert.DoesNotContain("menu", result.Content);
    }

    [Fact]
    public async Task Binary_responses_are_rejected_and_text_is_bounded()
    {
        var binary = new TestFetcher(new Handler(_ => new(HttpStatusCode.OK) { Content = new ByteArrayContent([1, 2]) }));
        await Assert.ThrowsAsync<WebResearchException>(() => binary.FetchAsync(new() { Url = "https://example.org" }));
        var text = new TestFetcher(new Handler(_ => new(HttpStatusCode.OK) { Content = new StringContent(new string('x', 4000)) }));
        var result = await text.FetchAsync(new() { Url = "https://example.org", MaxBytes = 1024 });
        Assert.True(result.Truncated);
        Assert.Equal(1024, result.Content.Length);
    }

    [Fact]
    public async Task Search_maps_searxng_query_and_bounds_deduplicated_results()
    {
        var search = new TestSearch(new Handler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Contains("format=json", request.RequestUri!.Query);
            Assert.Contains("language=fa", request.RequestUri.Query);
            Assert.Contains("time_range=day", request.RequestUri.Query);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            return new(HttpStatusCode.OK) { Content = new StringContent("""
                {"results":[{"url":"http://127.0.0.1","title":"private"},
                {"url":"https://example.org/a","title":"A","content":"Snippet"},
                {"url":"https://example.org/a","title":"Duplicate"},
                {"url":"https://example.org/b","title":"B"}]}
                """) };
        }));
        var result = await search.SearchAsync(new() { Query = "a & b", Culture = "fa", TimeRange = "day", MaxResults = 1 },
            new() { Endpoint = "https://search.internal", Token = "test-token" });
        Assert.Single(result.Results);
        Assert.Equal("Snippet", result.Results[0].Snippet);
    }

    [Theory]
    [InlineData(403)] [InlineData(429)] [InlineData(500)] [InlineData(302)]
    public async Task Search_failures_do_not_disclose_provider_body(int status)
    {
        var search = new TestSearch(new Handler(_ => new((HttpStatusCode)status) { Content = new StringContent("sensitive-provider-body") }));
        var error = await Assert.ThrowsAsync<WebResearchException>(() => search.SearchAsync(new() { Query = "test" }, new() { Endpoint = "https://search.internal" }));
        Assert.Equal("SearchUnavailable", error.Code);
        Assert.DoesNotContain("sensitive", error.ToString());
    }

    [Fact]
    public async Task Malformed_search_response_is_sanitized()
    {
        var search = new TestSearch(new Handler(_ => new(HttpStatusCode.OK) { Content = new StringContent("not JSON") }));
        var error = await Assert.ThrowsAsync<WebResearchException>(() => search.SearchAsync(new() { Query = "test" }, new() { Endpoint = "https://search.internal" }));
        Assert.Equal("InvalidSearchResponse", error.Code);
    }

    private class Handler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        { ct.ThrowIfCancellationRequested(); return Task.FromResult(respond(request)); }
    }
    private class TestFetcher(HttpMessageHandler handler) : WebContentFetcher
    { protected override HttpClient CreateClient() => new(handler); }
    private class TestSearch(HttpMessageHandler handler) : SearXngWebSearchService
    { protected override HttpClient CreateClient() => new(handler); }

    [Fact]
    public async Task Dns_answer_containing_private_address_never_connects()
    {
        var fetcher = new PrivateDnsFetcher();
        await Assert.ThrowsAsync<WebResearchException>(() => fetcher.FetchAsync(new() { Url = "https://example.org" }));
        Assert.True(fetcher.Resolved);
    }

    [Fact]
    public async Task Caller_cancellation_is_not_converted_to_unavailable()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var fetcher = new TestFetcher(new Handler(_ => throw new InvalidOperationException("Must not send")));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => fetcher.FetchAsync(new() { Url = "https://example.org" }, cancellation.Token));
    }

    private class PrivateDnsFetcher : WebContentFetcher
    {
        public bool Resolved { get; private set; }
        protected override Task<IPAddress[]> ResolveAddressesAsync(string host, CancellationToken ct)
        { Resolved = true; return Task.FromResult(new[] { IPAddress.Parse("8.8.8.8"), IPAddress.Parse("127.0.0.1") }); }
    }
}
