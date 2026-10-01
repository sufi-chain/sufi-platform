using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

public class ProviderResponseCompletionHandlerTests
{
    [Fact]
    public async Task Complete_json_is_released_when_the_connection_stays_open()
    {
        var payload = Encoding.UTF8.GetBytes("{\"id\":\"chatcmpl-callback\",\"choices\":[]}");
        using var client = CreateClient(new HoldingHandler(payload, "application/json"));

        var response = await client.GetAsync("https://gateway.test/v1/chat/completions");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal("{\"id\":\"chatcmpl-callback\",\"choices\":[]}", body);
        Assert.Equal(payload.Length, response.Content.Headers.ContentLength);
    }

    [Fact]
    public async Task Server_sent_done_is_released_without_waiting_for_the_quiet_period()
    {
        var payload = Encoding.UTF8.GetBytes("data: {\"choices\":[]}\n\ndata: [DONE]\n\n");
        using var client = CreateClient(new HoldingHandler(payload, "text/event-stream"), TimeSpan.FromSeconds(30));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        var response = await client.GetAsync("https://gateway.test/v1/chat/completions", timeout.Token);

        Assert.Contains("data: [DONE]", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Finish_reason_error_is_retried_once_and_the_successful_body_is_returned()
    {
        var error = Encoding.UTF8.GetBytes(
            "{\"choices\":[{\"finish_reason\":\"error\",\"message\":{\"role\":\"assistant\",\"content\":null},\"error\":{\"message\":\"overloaded\"}}]}");
        var success = Encoding.UTF8.GetBytes(
            "{\"choices\":[{\"finish_reason\":\"tool_calls\",\"message\":{\"role\":\"assistant\",\"content\":\"\"}}]}");
        var inner = new SequenceHandler(error, success);
        using var client = CreateClient(inner);

        var response = await client.PostAsync(
            "https://gateway.test/v1/chat/completions",
            new StringContent("{\"model\":\"openrouter/free\"}", Encoding.UTF8, "application/json"));

        Assert.Equal(2, inner.Calls);
        Assert.Contains("\"finish_reason\":\"tool_calls\"", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Finish_reason_error_is_normalized_when_the_retry_also_fails()
    {
        var error = Encoding.UTF8.GetBytes(
            "{\"choices\":[{\"finish_reason\":\"error\",\"message\":{\"role\":\"assistant\",\"content\":null},\"error\":{\"message\":\"overloaded\"}}]}");
        var inner = new SequenceHandler(error, error);
        using var client = CreateClient(inner);

        var response = await client.GetAsync("https://gateway.test/v1/chat/completions");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(2, inner.Calls);
        Assert.Contains("\"finish_reason\":\"stop\"", body, StringComparison.Ordinal);
        Assert.Contains("\"content\":\"\"", body, StringComparison.Ordinal);
        Assert.DoesNotContain("\"finish_reason\":\"error\"", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Content_length_is_released_without_waiting_for_the_connection_to_close()
    {
        var payload = Encoding.UTF8.GetBytes("{\"ok\":true}");
        using var client = CreateClient(new HoldingHandler(payload, "application/json", payload.Length), TimeSpan.FromSeconds(30));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        var response = await client.GetAsync("https://gateway.test/v1/chat/completions", timeout.Token);

        Assert.Equal("{\"ok\":true}", await response.Content.ReadAsStringAsync());
    }

    private static HttpClient CreateClient(HttpMessageHandler inner, TimeSpan? quietPeriod = null)
    {
        return new HttpClient(new ProviderResponseCompletionHandler(inner, quietPeriod ?? TimeSpan.FromMilliseconds(200)))
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    private sealed class SequenceHandler : HttpMessageHandler
    {
        private readonly byte[][] _payloads;
        public int Calls { get; private set; }

        public SequenceHandler(params byte[][] payloads)
        {
            _payloads = payloads;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var payload = _payloads[Math.Min(Calls, _payloads.Length - 1)];
            Calls++;
            var content = new ByteArrayContent(payload);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.ContentLength = payload.Length;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }
    }

    private sealed class HoldingHandler : HttpMessageHandler
    {
        private readonly byte[] _payload;
        private readonly string _mediaType;
        private readonly long? _contentLength;

        public HoldingHandler(byte[] payload, string mediaType, long? contentLength = null)
        {
            _payload = payload;
            _mediaType = mediaType;
            _contentLength = contentLength;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var content = new StreamContent(new HoldAfterPayloadStream(_payload));
            content.Headers.ContentType = new MediaTypeHeaderValue(_mediaType);
            if (_contentLength.HasValue)
            {
                content.Headers.ContentLength = _contentLength;
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }
    }

    private sealed class HoldAfterPayloadStream : Stream
    {
        private readonly byte[] _payload;
        private int _position;

        public HoldAfterPayloadStream(byte[] payload)
        {
            _payload = payload;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => _position; set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) =>
            throw new NotSupportedException();

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_position < _payload.Length)
            {
                var count = Math.Min(buffer.Length, _payload.Length - _position);
                _payload.AsSpan(_position, count).CopyTo(buffer.Span);
                _position += count;
                return count;
            }

            await Task.Delay(Timeout.Infinite, cancellationToken);
            return 0;
        }

        public override void Flush()
        {
        }
    }
}
