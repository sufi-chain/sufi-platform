using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

    [Fact]
    public async Task Transcription_keeps_duration_seconds_when_the_response_also_has_tokens()
    {
        using var handler = new CapturingHandler("""
            {"text":"hello","usage":{"cost":0.000508,"input_tokens":83,"output_tokens":30,"seconds":9.2,"total_tokens":113}}
            """);
        var workspace = CreateWorkspace();
        var route = workspace.AddModelConfiguration(AICapabilityType.AudioTranscription, "openai/whisper-large-v3",
            apiEndpoint: "https://api.example/v1", apiKey: "test-key");

        var result = await CreateProvider(handler).TranscribeAudioAsync(workspace, route, new AudioTranscriptionRequest
        {
            AudioData = Encoding.UTF8.GetBytes("audio"),
            AudioFormat = "wav"
        });

        result.BilledSeconds.ShouldBe(9.2m);
        result.InputTokens.ShouldBe(83);
        result.OutputTokens.ShouldBe(30);
        result.TotalTokens.ShouldBe(113);
        result.UsageUnavailableReason.ShouldBeNull();
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
    public async Task Chat_completion_records_the_upstream_model_id()
    {
        using var handler = new CapturingHandler(
            """{"model":"nvidia/nemotron-3.5-content-safety:free","choices":[{"message":{"role":"assistant","content":"ok"},"finish_reason":"stop"}]}""");
        var workspace = CreateWorkspace();
        var route = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "openrouter/free",
            apiEndpoint: "https://api.example/v1", apiKey: "test-key");
        var response = await CreateProvider(handler).SendChatMessageAsync(workspace, route, new ChatCompletionRequest
        {
            Messages = { new ChatMessage { Role = "user", Content = "hi" } }
        });

        response.ModelId.ShouldBe("nvidia/nemotron-3.5-content-safety:free");
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

    [Fact]
    public async Task Chat_completions_send_image_pdf_and_reasoning_effort()
    {
        using var handler = new CapturingHandler("""{"choices":[{"message":{"content":"ok"},"finish_reason":"stop"}]}""");
        var workspace = CreateWorkspace();
        var route = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "test-model",
            apiEndpoint: "https://api.example/v1", apiKey: "test-key");
        route.SetChatCapabilities(true, true, true, "low,high", "low", ModelCapabilitySource.ExternalCatalog);

        await CreateProvider(handler).SendChatMessageAsync(workspace, route, new ChatCompletionRequest
        {
            ReasoningEffort = "high",
            Messages =
            {
                new ChatMessage
                {
                    Role = "user",
                    MultiModalContent = new List<MessageContent>
                    {
                        new MessageContent { Type = "text", Text = "look" },
                        new MessageContent { Type = "image_url", ImageUrl = new ImageContent { Url = "data:image/png;base64,QQ" } },
                        new MessageContent { Type = "file", FileName = "note.pdf", FileData = "data:application/pdf;base64,QQ" }
                    }
                }
            }
        });

        handler.LastBody.ShouldContain("image_url");
        handler.LastBody.ShouldContain("\"type\":\"file\"");
        handler.LastBody.ShouldContain("note.pdf");
        handler.LastBody.ShouldContain("\"reasoning_effort\":\"high\"");
    }

    [Fact]
    public async Task Chat_completions_omit_reasoning_when_the_model_does_not_declare_it()
    {
        using var handler = new CapturingHandler("""{"choices":[{"message":{"content":"ok"},"finish_reason":"stop"}]}""");
        var workspace = CreateWorkspace();
        var route = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "nvidia-model",
            apiEndpoint: "https://api.example/v1", apiKey: "test-key");

        await CreateProvider(handler).SendChatMessageAsync(workspace, route, new ChatCompletionRequest
        {
            ReasoningEffort = "high",
            Messages = { new ChatMessage { Role = "user", Content = "hi" } }
        });

        handler.LastBody.ShouldNotBeNull();
        handler.LastBody.ShouldNotContain("reasoning");
    }

    [Fact]
    public async Task Chat_completions_map_an_undeclared_effort_onto_the_saved_default()
    {
        using var handler = new CapturingHandler("""{"choices":[{"message":{"content":"ok"},"finish_reason":"stop"}]}""");
        var workspace = CreateWorkspace();
        var route = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "test-model",
            apiEndpoint: "https://api.example/v1", apiKey: "test-key");
        route.SetChatCapabilities(true, false, true, "low,medium", "medium", ModelCapabilitySource.ExternalCatalog);

        await CreateProvider(handler).SendChatMessageAsync(workspace, route, new ChatCompletionRequest
        {
            ReasoningEffort = "high",
            Messages = { new ChatMessage { Role = "user", Content = "hi" } }
        });

        handler.LastBody.ShouldContain("\"reasoning_effort\":\"medium\"");
        handler.LastBody.ShouldNotContain("\"reasoning_effort\":\"high\"");
    }

    [Fact]
    public async Task Provider_failure_records_the_upstream_reason()
    {
        using var handler = new CapturingHandler(
            """{"error":{"message":"reasoning is not supported","type":"invalid_request_error"}}""",
            HttpStatusCode.BadRequest);
        var workspace = CreateWorkspace();
        var route = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "router",
            apiEndpoint: "https://api.example/v1", apiKey: "test-key");

        var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(() =>
            CreateProvider(handler).SendChatMessageAsync(workspace, route, new ChatCompletionRequest
            {
                Messages = { new ChatMessage { Role = "user", Content = "hi" } }
            }));

        LlmCallFailure.DescribeUsageFailure(exception).ShouldBe("HTTP 400: reasoning is not supported");
        exception.Data["StatusCode"].ShouldBe(400);
        exception.Code.ShouldBe(AIErrorCodes.ProviderRequestFailed);
    }

    [Fact]
    public async Task An_unauthorized_provider_does_not_surface_the_workspace_key()
    {
        using var handler = new CapturingHandler(
            """{"error":{"message":"Invalid API key sk-or-v1-secretvalue","code":"invalid_api_key"},"api_key":"sk-or-v1-secretvalue"}""",
            HttpStatusCode.Unauthorized);
        var workspace = CreateWorkspace();
        var route = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "openrouter/free",
            apiEndpoint: "https://api.example/v1", apiKey: "sk-or-v1-secretvalue");

        var logger = new CaptureLogger();
        var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(() =>
            CreateProvider(handler, logger).SendChatMessageAsync(workspace, route, new ChatCompletionRequest
            {
                Messages = { new ChatMessage { Role = "user", Content = "hi" } }
            }));

        exception.Code.ShouldBe(AIErrorCodes.ProviderAuthorizationFailed);
        exception.Data["StatusCode"].ShouldBe(401);
        exception.Data["ProviderMessage"]?.ToString().ShouldNotContain("sk-");
        LlmCallFailure.DescribeUsageFailure(exception).ShouldNotContain("sk-");
        LlmCallFailure.DescribeUsageFailure(exception).ShouldContain("401");
        logger.Text.ShouldContain("401");
        logger.Text.ShouldNotContain("sk-or");
        logger.Text.ShouldNotContain("secretvalue");
    }

    [Fact]
    public async Task Responses_send_input_file_and_reasoning_object()
    {
        using var handler = new CapturingHandler("""{"status":"completed","output_text":"ok"}""");
        var workspace = CreateWorkspace();
        var route = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "test-model",
            apiEndpoint: "https://api.example/v1", apiKey: "test-key", openAIApiMode: OpenAIApiMode.Responses);
        route.SetChatCapabilities(true, true, true, "low", "low", ModelCapabilitySource.Manual);

        await CreateProvider(handler).SendChatMessageAsync(workspace, route, new ChatCompletionRequest
        {
            Messages =
            {
                new ChatMessage
                {
                    Role = "user",
                    MultiModalContent = new List<MessageContent>
                    {
                        new MessageContent { Type = "file", FileName = "note.pdf", FileData = "data:application/pdf;base64,QQ" }
                    }
                }
            }
        });

        handler.LastBody.ShouldContain("input_file");
        handler.LastBody.ShouldContain("\"effort\":\"low\"");
    }

    [Fact]
    public async Task Rejects_image_when_the_saved_flag_is_not_true()
    {
        using var handler = new CapturingHandler("{}");
        var workspace = CreateWorkspace();
        var route = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "test-model",
            apiEndpoint: "https://api.example/v1", apiKey: "test-key");

        var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(() =>
            CreateProvider(handler).SendChatMessageAsync(workspace, route, new ChatCompletionRequest
            {
                Messages =
                {
                    new ChatMessage
                    {
                        Role = "user",
                        MultiModalContent = new List<MessageContent> { new MessageContent { Type = "image_url", ImageUrl = new ImageContent { Url = "data:image/png;base64,QQ" } } }
                    }
                }
            }));

        exception.Code.ShouldBe(AIErrorCodes.ImageInputNotSupported);
        handler.LastBody.ShouldBeNull();
    }

    private static OpenAIProvider CreateProvider(CapturingHandler handler, ILogger<OpenAIProvider>? logger = null)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler));
        factory.CreateClient().Returns(_ => new HttpClient(handler));

        var credentials = Substitute.For<IAICredentialResolver>();
        credentials.DecryptApiKey(Arg.Any<string?>()).Returns(call => call.Arg<string?>());

        return new OpenAIProvider(
            factory,
            logger ?? NullLogger<OpenAIProvider>.Instance,
            credentials,
            Substitute.For<IClock>());
    }

    private sealed class CaptureLogger : ILogger<OpenAIProvider>
    {
        public string Text { get; private set; } = string.Empty;

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Text += formatter(state, exception);
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
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

        private readonly HttpStatusCode _status;

        public CapturingHandler(string responseJson, HttpStatusCode status = HttpStatusCode.OK)
        {
            _responseJson = responseJson;
            _status = status;
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
            return new HttpResponseMessage(_status)
            {
                Content = new StringContent(_responseJson, Encoding.UTF8, "application/json")
            };
        }
    }
}
