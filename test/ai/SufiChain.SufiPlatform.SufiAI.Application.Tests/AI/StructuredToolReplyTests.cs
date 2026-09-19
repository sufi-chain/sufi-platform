using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.AI;

public class StructuredToolReplyTests
{
    [Theory]
    [InlineData(true, "stop")]
    [InlineData(true, "length")]
    [InlineData(true, "content_filter")]
    [InlineData(false, "stop")]
    public async Task Schema_survives_tool_continuation_and_finish_reason_is_preserved(bool structured, string finishReason)
    {
        using var handler = new ToolReplyHandler(finishReason);
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://test.invalid/v1") };
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("test-model", "test-key", httpClient: client);
        var kernel = builder.Build();
        var calls = 0;
        kernel.Plugins.AddFromFunctions("forms", [KernelFunctionFactory.CreateFromMethod(
            () => { calls++; return "catalog-result"; }, "get_designer_catalog")]);
        var history = new ChatHistory();
        history.AddSystemMessage("Return JSON with message and proposal.");
        history.AddUserMessage("Design a form");
        var settings = new OpenAIPromptExecutionSettings
        {
            Temperature = 0.2,
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };
        var executor = new AIToolChatExecutor(Substitute.For<IWorkspaceRuntimeConfigurationResolver>(), null!,
            Substitute.For<IWorkspaceGuardrailService>(), Substitute.For<IAIUsageRecorder>());
        var result = await executor.ExecuteAsync(new AIToolChatExecutionRequest
        {
            Kernel = kernel,
            Configuration = Configuration(),
            History = history,
            ExecutionSettings = settings,
            RequiresToolCalling = true,
            ResponseSchema = structured ? new SufiAIJsonResponseSchema
            {
                Name = "reply",
                SchemaJson = """{"type":"object","properties":{"message":{"type":"string"},"proposal":{"type":["string","null"]}},"required":["message","proposal"],"additionalProperties":false}"""
            } : null
        });

        result.FinishReason.ShouldBe(finishReason);
        result.Response.Content.ShouldBe("{\"message\":\"Ready\",\"proposal\":null}");
        calls.ShouldBe(1);
        handler.Requests.Count.ShouldBe(2);
        settings.ResponseFormat.ShouldBeNull(); // Per-request enrichment must not mutate shared settings.
        foreach (var body in handler.Requests)
        {
            using var json = JsonDocument.Parse(body);
            var root = json.RootElement;
            root.GetProperty("temperature").GetDouble().ShouldBe(0.2);
            root.TryGetProperty("response_format", out var format).ShouldBe(structured);
            if (!structured) continue;
            format.GetProperty("type").GetString().ShouldBe("json_schema");
            var contract = format.GetProperty("json_schema");
            contract.GetProperty("strict").GetBoolean().ShouldBeTrue();
            contract.GetProperty("name").GetString().ShouldBe("reply");
            var schema = contract.GetProperty("schema");
            schema.GetProperty("required").EnumerateArray().Select(x => x.GetString())
                .ShouldBe(new[] { "message", "proposal" });
            schema.GetProperty("additionalProperties").GetBoolean().ShouldBeFalse();
        }
        using var continuation = JsonDocument.Parse(handler.Requests[1]);
        var messages = continuation.RootElement.GetProperty("messages").EnumerateArray().ToArray();
        var toolIndex = Array.FindIndex(messages, message => message.GetProperty("role").GetString() == "tool");
        toolIndex.ShouldBeGreaterThan(0);
        messages[toolIndex].GetProperty("tool_call_id").GetString().ShouldBe("call_catalog");
        messages[toolIndex - 1].GetProperty("tool_calls")[0].GetProperty("id").GetString().ShouldBe("call_catalog");
    }

    [Fact]
    public async Task Unsupported_schema_after_a_tool_call_is_not_downgraded_or_replayed()
    {
        using var handler = new ToolReplyHandler("stop", rejectContinuation: true);
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://test.invalid/v1") };
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("test-model", "test-key", httpClient: client);
        var kernel = builder.Build();
        var calls = 0;
        kernel.Plugins.AddFromFunctions("forms", [KernelFunctionFactory.CreateFromMethod(
            () => { calls++; return "catalog-result"; }, "get_designer_catalog")]);
        var history = new ChatHistory();
        history.AddUserMessage("Return JSON");
        var executor = new AIToolChatExecutor(Substitute.For<IWorkspaceRuntimeConfigurationResolver>(), null!,
            Substitute.For<IWorkspaceGuardrailService>(), Substitute.For<IAIUsageRecorder>());
        await Should.ThrowAsync<HttpOperationException>(() => executor.ExecuteAsync(new AIToolChatExecutionRequest
        {
            Kernel = kernel, Configuration = Configuration(), History = history, RequiresToolCalling = true,
            ExecutionSettings = new OpenAIPromptExecutionSettings { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions },
            ResponseSchema = new SufiAIJsonResponseSchema
            {
                Name = "reply", SchemaJson = """{"type":"object","properties":{},"additionalProperties":false}"""
            }
        }));
        calls.ShouldBe(1);
        handler.Requests.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Buffered_usage_chunk_does_not_erase_the_final_finish_reason()
    {
        async IAsyncEnumerable<StreamingChatMessageContent> Chunks()
        {
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, "reply",
                metadata: new Dictionary<string, object?> { ["FinishReason"] = "Length" });
            await Task.Yield();
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, null,
                metadata: new Dictionary<string, object?> { ["FinishReason"] = null });
        }
        var response = await AIStreamingChatResponseReader.ReadAsync(Chunks());
        var result = new AIToolChatExecutionResult
        {
            Response = response, Configuration = Configuration(), Usage = new SufiAITokenUsage(), LatencyMs = 0
        };
        result.FinishReason.ShouldBe("length");
    }

    private static WorkspaceRuntimeConfiguration Configuration() => new()
    {
        Workspace = new Workspace(Guid.NewGuid(), "test", AIProviderType.OpenAI, "test-model"),
        ModelId = "test-model", Provider = AIProviderType.OpenAI, OpenAIApiMode = OpenAIApiMode.ChatCompletions
    };

    private sealed class ToolReplyHandler(string finishReason, bool rejectContinuation = false) : HttpMessageHandler
    {
        public List<string> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
            Requests.Count.ShouldBeLessThanOrEqualTo(2);
            if (rejectContinuation && Requests.Count == 2)
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("""{"error":{"message":"Unsupported response_format","type":"invalid_request_error","param":"response_format"}}""",
                        Encoding.UTF8, "application/json")
                };
            object message = Requests.Count == 1
                ? new { role = "assistant", content = (string?)null, tool_calls = new[]
                    { new { id = "call_catalog", type = "function", function = new { name = "forms-get_designer_catalog", arguments = "{}" } } } }
                : new { role = "assistant", content = "{\"message\":\"Ready\",\"proposal\":null}" };
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    id = "completion-test", @object = "chat.completion", created = 1, model = "test-model",
                    choices = new[] { new { index = 0, message, finish_reason = Requests.Count == 1 ? "tool_calls" : finishReason } },
                    usage = new { prompt_tokens = 10, completion_tokens = 5, total_tokens = 15 }
                }), Encoding.UTF8, "application/json")
            };
        }
    }
}
