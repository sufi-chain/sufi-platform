using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Providers;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Timing;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class OpenAIProviderApiModeTests
{
    [Theory]
    [InlineData("webm", "audio/webm")]
    [InlineData("ogg", "audio/ogg")]
    [InlineData("mp3", "audio/mpeg")]
    [InlineData("m4a", "audio/mp4")]
    [InlineData("wav", "audio/wav")]
    public async Task Transcription_sends_audio_and_fields_in_compatible_multipart_form(string format, string mediaType)
    {
        using var handler = new CapturingHandler("""{"text":"transcribed"}""");
        var workspace = CreateWorkspace();
        var route = workspace.AddModelConfiguration(AICapabilityType.AudioTranscription, "test-asr",
            apiEndpoint: "https://api.example/v1", apiKey: "test-key");
        var result = await CreateProvider(handler).TranscribeAudioAsync(workspace, route,
            new AudioTranscriptionRequest
            {
                AudioData = Encoding.UTF8.GetBytes("audio-payload"), AudioFormat = format,
                Language = "fa", Prompt = "transcription context"
            });

        result.Text.ShouldBe("transcribed");
        handler.LastUri!.AbsolutePath.ShouldBe("/v1/audio/transcriptions");
        handler.LastContentType.ShouldStartWith("multipart/form-data; boundary=");
        var boundary = handler.LastContentType!.Split("boundary=")[1];
        handler.LastBody.ShouldStartWith("--" + boundary + "\r\n");
        handler.LastBody.ShouldEndWith("--" + boundary + "--\r\n");
        handler.LastBody.ShouldContain($"name=\"file\"; filename=\"audio.{format}\"");
        handler.LastBody.ShouldContain("Content-Type: " + mediaType);
        handler.LastBody.ShouldContain("\r\n\r\naudio-payload\r\n");
        handler.LastBody.ShouldContain("name=\"model\"\r\n\r\ntest-asr\r\n");
        handler.LastBody.ShouldContain("name=\"response_format\"\r\n\r\njson\r\n");
        handler.LastBody.ShouldContain("name=\"language\"\r\n\r\nfa\r\n");
        handler.LastBody.ShouldContain("name=\"prompt\"\r\n\r\ntranscription context\r\n");
        handler.LastBody.ShouldNotContain("filename*=");
    }

    [Theory]
    [InlineData(OpenAIApiMode.ChatCompletions, true)]
    [InlineData(OpenAIApiMode.ChatCompletions, false)]
    [InlineData(OpenAIApiMode.Responses, true)]
    [InlineData(OpenAIApiMode.Responses, false)]
    public async Task Structured_reply_contract_uses_the_selected_api_format(OpenAIApiMode mode, bool structured)
    {
        using var handler = new CapturingHandler(mode == OpenAIApiMode.Responses
            ? """{"status":"completed","output_text":"{}"}"""
            : """{"choices":[{"message":{"role":"assistant","content":"{}"},"finish_reason":"stop"}]}""");
        var provider = CreateProvider(handler);
        var workspace = CreateWorkspace();
        var route = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "test-model",
            apiEndpoint: "https://api.example/v1", apiKey: "test-key", openAIApiMode: mode);
        var response = await provider.SendChatMessageAsync(workspace, route, new ChatCompletionRequest
        {
            Messages = { new ChatMessage { Role = "user", Content = "Return JSON" } },
            ResponseSchema = structured ? new SufiAIJsonResponseSchema
            {
                Name = "reply", SchemaJson = """{"type":"object","properties":{},"additionalProperties":false}"""
            } : null
        });

        response.FinishReason.ShouldBe("stop");
        using var json = JsonDocument.Parse(handler.LastBody!);
        if (!structured)
        {
            json.RootElement.TryGetProperty("response_format", out _).ShouldBeFalse();
            json.RootElement.TryGetProperty("text", out _).ShouldBeFalse();
            return;
        }
        var format = mode == OpenAIApiMode.ChatCompletions
            ? json.RootElement.GetProperty("response_format")
            : json.RootElement.GetProperty("text").GetProperty("format");
        format.GetProperty("type").GetString().ShouldBe("json_schema");
        var contract = mode == OpenAIApiMode.ChatCompletions ? format.GetProperty("json_schema") : format;
        contract.GetProperty("name").GetString().ShouldBe("reply");
        contract.GetProperty("strict").GetBoolean().ShouldBeTrue();
        contract.GetProperty("schema").GetProperty("additionalProperties").GetBoolean().ShouldBeFalse();
    }

    [Fact]
    public async Task Responses_token_limit_is_reported_as_length()
    {
        using var handler = new CapturingHandler("""{"status":"incomplete","incomplete_details":{"reason":"max_output_tokens"},"output_text":"partial"}""");
        var workspace = CreateWorkspace();
        var route = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "test-model",
            apiEndpoint: "https://api.example/v1", apiKey: "test-key", openAIApiMode: OpenAIApiMode.Responses);
        var response = await CreateProvider(handler).SendChatMessageAsync(workspace, route, new ChatCompletionRequest());
        response.FinishReason.ShouldBe("length");
    }

    [Theory]
    [InlineData(OpenAIApiMode.ChatCompletions)]
    [InlineData(OpenAIApiMode.Responses)]
    public async Task Model_requests_use_the_configured_transport_timeout(OpenAIApiMode mode)
    {
        using var handler = new CapturingHandler(mode == OpenAIApiMode.Responses
            ? """{"output_text":"ok"}"""
            : """{"choices":[{"message":{"role":"assistant","content":"ok"},"finish_reason":"stop"}]}""");
        using var client = new HttpClient(handler);
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(client);
        var credentials = Substitute.For<IAICredentialResolver>();
        credentials.DecryptApiKey(Arg.Any<string?>()).Returns(call => call.Arg<string?>());
        var provider = new OpenAIProvider(factory, NullLogger<OpenAIProvider>.Instance,
            credentials, Substitute.For<IClock>(), transportOptions:
            Options.Create(new AITransportOptions { RequestTimeoutSeconds = 1200 }));
        var workspace = CreateWorkspace();
        var route = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "test-model",
            apiEndpoint: "https://api.example/v1", apiKey: "test-key", openAIApiMode: mode);
        await provider.SendChatMessageAsync(workspace, route, new ChatCompletionRequest
        {
            Messages = { new ChatMessage { Role = "user", Content = "hi" } }
        });
        client.Timeout.ShouldBe(TimeSpan.FromMinutes(20));
    }

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
        public string? LastBody { get; private set; }
        public string? LastContentType { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastUri = request.RequestUri;
            LastContentType = request.Content?.Headers.ContentType?.ToString();
            LastBody = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseJson, Encoding.UTF8, "application/json")
            };
        }
    }
}
