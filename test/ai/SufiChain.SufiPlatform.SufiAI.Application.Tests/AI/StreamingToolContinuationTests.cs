using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
                Content = new EventStreamContent(stream.ToString())
            };
        }
    }

    /// <summary>
    /// StringContent is seekable. The OpenAI client throws on SSE dispose when that stream is no longer at position 0.
    /// </summary>
    private sealed class EventStreamContent : HttpContent
    {
        private readonly byte[] _bytes;

        public EventStreamContent(string body)
        {
            _bytes = Encoding.UTF8.GetBytes(body);
            Headers.ContentType = new MediaTypeHeaderValue("text/event-stream");
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            stream.WriteAsync(_bytes, 0, _bytes.Length);

        protected override bool TryComputeLength(out long length)
        {
            length = _bytes.Length;
            return true;
        }

        protected override Stream CreateContentReadStream(CancellationToken cancellationToken) =>
            new NonSeekableStream(_bytes);

        protected override Task<Stream> CreateContentReadStreamAsync() =>
            Task.FromResult<Stream>(new NonSeekableStream(_bytes));

        private sealed class NonSeekableStream : Stream
        {
            private readonly byte[] _bytes;
            private int _offset;

            public NonSeekableStream(byte[] bytes) => _bytes = bytes;

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count) =>
                Read(buffer.AsSpan(offset, count));

            public override int Read(Span<byte> buffer)
            {
                var remaining = _bytes.Length - _offset;
                if (remaining <= 0)
                {
                    return 0;
                }

                var read = Math.Min(buffer.Length, remaining);
                _bytes.AsSpan(_offset, read).CopyTo(buffer);
                _offset += read;
                return read;
            }

            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
                ValueTask.FromResult(Read(buffer.Span));

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
