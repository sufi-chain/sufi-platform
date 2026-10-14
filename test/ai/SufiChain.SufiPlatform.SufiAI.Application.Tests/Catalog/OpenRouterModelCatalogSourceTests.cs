using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Catalog;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Catalog;

public class OpenRouterModelCatalogSourceTests
{
    [Fact]
    public async Task Model_list_uses_the_workspace_base_url_and_api_key()
    {
        var handler = new RecordingHandler();
        var source = new OpenRouterModelCatalogSource(
            new SingleClientFactory(handler),
            new FixedEndpointResolver("https://or-gateway.sufichain.com/v1", "sk-workspace"),
            NullLogger<OpenRouterModelCatalogSource>.Instance);

        await source.TryListAsync();

        handler.Requests.Count.ShouldBe(2);
        handler.Requests.ShouldContain(request =>
            request.RequestUri!.ToString() == "https://or-gateway.sufichain.com/v1/models?output_modalities=all");
        handler.Requests.ShouldContain(request =>
            request.RequestUri!.ToString() == "https://or-gateway.sufichain.com/v1/models?output_modalities=decisions");
        handler.Requests.ShouldAllBe(request => request.Headers.Authorization!.Parameter == "sk-workspace");
        handler.Requests.ShouldAllBe(request =>
            request.RequestUri!.Host != "openrouter.ai");
    }

    [Fact]
    public async Task HttpClient_timeout_without_caller_cancellation_is_unavailable()
    {
        var source = new OpenRouterModelCatalogSource(
            new SingleClientFactory(new TimeoutHandler()),
            new FixedEndpointResolver("https://or-gateway.sufichain.com/v1", "sk-workspace"),
            NullLogger<OpenRouterModelCatalogSource>.Instance);

        (await source.TryListAsync(CancellationToken.None)).ShouldBeNull();

        var profile = Substitute.For<IAiProviderProfile>();
        profile.ProviderType.Returns(AIProviderType.OpenRouter);
        profile.CatalogName.Returns(OpenRouterModelCatalogSource.NameValue);

        var endpoints = await new CatalogModelEndpointLookup(
                new TimeoutCatalogSource(),
                new[] { profile },
                NullLogger<CatalogModelEndpointLookup>.Instance)
            .GetSupportedEndpointsAsync(AIProviderType.OpenRouter, "openrouter/free", CancellationToken.None);

        endpoints.ShouldBeEmpty();
    }

    private sealed class FixedEndpointResolver : IOpenRouterCatalogEndpointResolver
    {
        private readonly OpenRouterCatalogEndpoint _endpoint;

        public FixedEndpointResolver(string baseUrl, string apiKey)
        {
            _endpoint = new OpenRouterCatalogEndpoint { BaseUrl = baseUrl, ApiKey = apiKey };
        }

        public Task<OpenRouterCatalogEndpoint> ResolveAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_endpoint);
        }
    }

    private sealed class SingleClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public SingleClientFactory(HttpMessageHandler handler)
        {
            _handler = handler;
        }

        public HttpClient CreateClient(string name)
        {
            return new HttpClient(_handler, disposeHandler: false)
            {
                BaseAddress = new Uri("https://openrouter.ai/api/")
            };
        }
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"data":[]}""")
            });
        }
    }

    private sealed class TimeoutHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new TaskCanceledException(
                "The request was canceled due to the configured HttpClient.Timeout.",
                new TimeoutException());
        }
    }

    private sealed class TimeoutCatalogSource : IModelCatalogSource
    {
        public Task<IReadOnlyList<ModelCatalogEntry>?> GetModelsAsync(CancellationToken cancellationToken = default)
            => throw new TaskCanceledException("timeout", new TimeoutException());

        public Task<IReadOnlyList<ModelCatalogEntry>?> GetModelsAsync(string? catalogName, CancellationToken cancellationToken = default)
            => throw new TaskCanceledException("timeout", new TimeoutException());

        public Task<ModelCatalogEntry?> FindAsync(string modelId, CancellationToken cancellationToken = default)
            => throw new TaskCanceledException("timeout", new TimeoutException());

        public Task<ModelCatalogEntry?> FindAsync(string modelId, string? catalogName, CancellationToken cancellationToken = default)
            => throw new TaskCanceledException("timeout", new TimeoutException());
    }
}
