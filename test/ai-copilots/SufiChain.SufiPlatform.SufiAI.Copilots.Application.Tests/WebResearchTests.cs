using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using SufiChain.SufiPlatform.SufiAI.Web;
using SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Copilots;

public class WebResearchTests
{
    [Theory]
    [InlineData("Search the web for current releases", true)]
    [InlineData("What are today's prices?", true)]
    [InlineData("اخبار امروز را جستجو کن", true)]
    [InlineData("ابحث عن أخبار اليوم", true)]
    [InlineData("Busca en la web las noticias de hoy", true)]
    [InlineData("What is the capital of France?", false)]
    [InlineData("Thanks", false)]
    [InlineData("Translate: today's latest news", false)]
    [InlineData("Summarize this text: prices rose today", false)]
    [InlineData("Please translate: latest news", false)]
    [InlineData("Summarize today's news", true)]
    [InlineData("What about today?", false)]
    public void Policy_distinguishes_research_from_supplied_text(string message, bool required) =>
        Assert.Equal(required, new WebSearchDecisionPolicy().Decide(message).Required);

    [Fact]
    public void Direct_url_summary_skips_search()
    {
        var decision = new WebSearchDecisionPolicy().Decide("Summarize https://example.org/article");
        Assert.Equal("DirectUrl", decision.Reason);
        Assert.Single(decision.Urls);
    }

    [Fact]
    public async Task Copilot_and_platform_opt_in_are_both_required()
    {
        var search = Substitute.For<IWebSearchService>();
        var fetcher = Substitute.For<IWebContentFetcher>();
        foreach (var enabled in new[] { false, true })
        {
            var service = Create(search, fetcher, new() { Enabled = enabled });
            var result = await service.ResearchAsync("latest news", new(), default);
            Assert.False(result.Diagnostics.Attempted);
        }
        var disabled = await Create(search, fetcher, new()).ResearchAsync("latest news", new() { UseWebSearch = true }, default);
        Assert.False(disabled.Diagnostics.Attempted);
        Assert.Empty(search.ReceivedCalls());
        Assert.Empty(fetcher.ReceivedCalls());
    }

    [Fact]
    public async Task Direct_fetch_has_sources_without_calling_search_provider()
    {
        var search = Substitute.For<IWebSearchService>();
        var fetcher = Substitute.For<IWebContentFetcher>();
        fetcher.FetchAsync(Arg.Any<WebFetchRequest>(), Arg.Any<CancellationToken>()).Returns(new WebFetchResponse
            { CanonicalUrl = "https://example.org/article", Content = "Evidence", Title = "Article" });
        var result = await Create(search, fetcher, new() { Enabled = true }).ResearchAsync(
            "Read https://example.org/article", new() { UseWebSearch = true }, default);
        Assert.True(result.Diagnostics.Used);
        Assert.Single(result.Diagnostics.Sources);
        Assert.True(result.Diagnostics.Sources[0].Fetched);
        Assert.Contains("untrusted", result.Context);
        Assert.Empty(search.ReceivedCalls());
    }

    [Fact]
    public async Task Failed_page_retains_search_snippet_and_reports_partial_failure()
    {
        var search = Substitute.For<IWebSearchService>();
        search.SearchAsync(Arg.Any<WebSearchRequest>(), Arg.Any<WebResearchOptions>(), Arg.Any<CancellationToken>())
            .Returns(new WebSearchResponse { Results = [new() { Url = "https://example.org", Title = "Source", Snippet = "Evidence" }] });
        var fetcher = Substitute.For<IWebContentFetcher>();
        fetcher.FetchAsync(Arg.Any<WebFetchRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromException<WebFetchResponse>(new WebResearchException("FetchFailed")));
        var result = await Create(search, fetcher, new() { Enabled = true }).ResearchAsync("latest news", new() { UseWebSearch = true }, default);
        Assert.True(result.Diagnostics.Used);
        Assert.Equal(1, result.Diagnostics.FetchFailureCount);
        Assert.False(result.Diagnostics.Sources[0].Fetched);
        Assert.Contains("Evidence", result.Context);
    }

    [Fact]
    public async Task Unavailable_search_adds_limitation_and_cancellation_propagates()
    {
        var search = Substitute.For<IWebSearchService>();
        var fetcher = Substitute.For<IWebContentFetcher>();
        search.SearchAsync(Arg.Any<WebSearchRequest>(), Arg.Any<WebResearchOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<WebSearchResponse>(new WebResearchException("SearchUnavailable")));
        var service = Create(search, fetcher, new() { Enabled = true });
        var result = await service.ResearchAsync("latest news", new() { UseWebSearch = true }, default);
        Assert.Equal("SearchUnavailable", result.Diagnostics.FailureCode);
        Assert.Contains("Do not claim", result.Context);
        search.SearchAsync(Arg.Any<WebSearchRequest>(), Arg.Any<WebResearchOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromCanceled<WebSearchResponse>(new CancellationToken(true)));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ResearchAsync("latest news", new() { UseWebSearch = true }, new CancellationToken(true)));
    }

    [Fact]
    public void Web_policy_changes_fingerprint_and_legacy_definitions_remain_off()
    {
        var resolver = new CopilotEffectiveConfigurationResolver(null!, null!, null!);
        var legacy = resolver.Deserialize("{}");
        Assert.False(legacy.UseWebSearch);
        var before = resolver.Fingerprint(legacy);
        legacy.UseWebSearch = true;
        legacy.WebSearchMaxResults = 2;
        Assert.NotEqual(before, resolver.Fingerprint(legacy));
        Assert.Equal(2, resolver.Deserialize(resolver.Serialize(legacy)).WebSearchMaxResults);
    }

    private static CopilotWebResearchService Create(IWebSearchService search, IWebContentFetcher fetcher, WebResearchOptions options)
    {
        var provider = Substitute.For<WebResearchOptionsProvider>(Substitute.For<ISettingProvider>());
        provider.GetAsync().Returns(options);
        var progress = Substitute.For<ICopilotTurnProgressReporter>();
        progress.ReportAsync(Arg.Any<CopilotTurnProgressDto>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        return new(new WebSearchDecisionPolicy(), provider, search, fetcher, progress);
    }
}
