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
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.AI;

public class StreamingToolContinuationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("I will inspect the catalog.")]
    public async Task Streaming_continuation_serializes_the_call_before_its_result(string? preamble)
    {
        using var handler = new ToolStreamHandler(preamble);
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://test.invalid/v1") };
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("test-model", "test-key", httpClient: client);
        var kernel = builder.Build();
        var invocations = 0;
        kernel.Plugins.AddFromFunctions("forms", [KernelFunctionFactory.CreateFromMethod(
            () => { invocations++; return "catalog-result"; }, "get_designer_catalog")]);
        var history = new ChatHistory();
        history.AddUserMessage("Design a registration form");

        var response = await AIStreamingChatResponseReader.ReadAsync(
            kernel.GetRequiredService<IChatCompletionService>().GetStreamingChatMessageContentsAsync(
                history, new OpenAIPromptExecutionSettings
                {
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                }, kernel));

        response.Content.ShouldBe("{\"message\":\"Ready\"}");
        invocations.ShouldBe(1);
        handler.Requests.Count.ShouldBe(2);
        using var request = JsonDocument.Parse(handler.Requests[1]);
        var messages = request.RootElement.GetProperty("messages").EnumerateArray().ToArray();
        var resultIndex = Array.FindIndex(messages, m => m.GetProperty("role").GetString() == "tool");
        resultIndex.ShouldBeGreaterThan(0);
        var assistant = messages[resultIndex - 1];
        assistant.GetProperty("role").GetString().ShouldBe("assistant");
        var call = assistant.GetProperty("tool_calls").EnumerateArray().Single();
        call.GetProperty("id").GetString().ShouldBe("call_catalog");
        call.GetProperty("function").GetProperty("name").GetString().ShouldBe("forms-get_designer_catalog");
        call.GetProperty("function").GetProperty("arguments").GetString().ShouldBe("{}");
        messages[resultIndex].GetProperty("tool_call_id").GetString().ShouldBe("call_catalog");
        messages[resultIndex].GetProperty("content").GetString().ShouldBe("catalog-result");
    }

    private sealed class ToolStreamHandler(string? preamble) : HttpMessageHandler
    {
        public List<string> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
            Requests.Count.ShouldBeLessThanOrEqualTo(2);
            var stream = new StringBuilder();
            void Chunk(object delta, string? finishReason = null) => stream.Append("data: ").Append(JsonSerializer.Serialize(new
            {
                id = "completion-test", @object = "chat.completion.chunk", created = 1, model = "test-model",
                choices = new[] { new { index = 0, delta, finish_reason = finishReason } }
            })).Append("\n\n");

            if (Requests.Count == 1)
            {
                Chunk(new { role = "assistant", content = preamble });
                Chunk(new { tool_calls = new[] { new { index = 0, id = "call_catalog", type = "function",
                    function = new { name = "forms-get_designer_catalog", arguments = "{" } } } });
                Chunk(new { tool_calls = new[] { new { index = 0, function = new { arguments = "}" } } } });
                Chunk(new { }, "tool_calls");
            }
            else
            {
                Chunk(new { role = "assistant", content = "{\"message\":\"Ready\"}" });
                Chunk(new { }, "stop");
            }
            stream.Append("data: [DONE]\n\n");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(stream.ToString(), Encoding.UTF8, "text/event-stream")
            };
        }
    }
}
