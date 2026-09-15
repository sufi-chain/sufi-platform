using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Providers;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Timing;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class OpenAIProviderApiModeTests
{
    [Fact]
    public async Task Responses_route_posts_to_responses_not_chat_completions()
    {
        var handler = new CapturingHandler("""{"output_text":"ok"}""");
        var provider = CreateProvider(handler);
        var workspace = CreateWorkspace();
        var route = workspace.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "responses-chat",
            apiEndpoint: "https://api.example/v1",
            apiKey: "sk-route",
            openAIApiMode: OpenAIApiMode.Responses);

        var response = await provider.SendChatMessageAsync(
            workspace,
            route,
            new ChatCompletionRequest
            {
                Messages = { new ChatMessage { Role = "user", Content = "hi" } }
            });

        handler.LastUri.ShouldNotBeNull();
        handler.LastUri!.AbsolutePath.ShouldEndWith("/responses");
        handler.LastUri.AbsolutePath.ShouldNotContain("/chat/completions");
        response.Content.ShouldBe("ok");
    }

    [Fact]
    public async Task Chat_completions_route_does_not_use_workspace_api_mode()
    {
        var handler = new CapturingHandler(
            """{"choices":[{"message":{"role":"assistant","content":"ok"},"finish_reason":"stop"}]}""");
        var provider = CreateProvider(handler);
        var workspace = CreateWorkspace();
        var route = workspace.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "completions-chat",
            apiEndpoint: "https://api.example/v1",
            apiKey: "sk-route",
            openAIApiMode: OpenAIApiMode.ChatCompletions);

        await provider.SendChatMessageAsync(
            workspace,
            route,
            new ChatCompletionRequest
            {
                Messages = { new ChatMessage { Role = "user", Content = "hi" } }
            });

        handler.LastUri.ShouldNotBeNull();
        handler.LastUri!.AbsolutePath.ShouldEndWith("/chat/completions");
    }

    private static OpenAIProvider CreateProvider(CapturingHandler handler)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler));
        factory.CreateClient().Returns(_ => new HttpClient(handler));

        var credentials = Substitute.For<IAICredentialResolver>();
        credentials.DecryptApiKey(Arg.Any<string?>()).Returns(call => call.Arg<string?>());

        return new OpenAIProvider(
            factory,
            NullLogger<OpenAIProvider>.Instance,
            credentials,
            Substitute.For<IClock>());
    }

    private static Workspace CreateWorkspace()
    {
        var workspace = new Workspace(
            Guid.NewGuid(),
            "api-mode-tests",
            AIProviderType.OpenAI,
            "workspace-default");
        workspace.UpdateConfiguration(
            "workspace-default",
            "sk-workspace",
            "https://api.example/v1");
        return workspace;
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly string _responseJson;

        public CapturingHandler(string responseJson)
        {
            _responseJson = responseJson;
        }

        public Uri? LastUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseJson, Encoding.UTF8, "application/json")
            });
        }
    }
}
