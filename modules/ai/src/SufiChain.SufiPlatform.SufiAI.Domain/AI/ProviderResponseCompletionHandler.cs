using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Releases a provider HTTP body once the payload is complete. Gateways often log the model
/// callback and then leave the response connection open, so the OpenAI client keeps reading
/// until its network timeout even though the JSON or SSE body is already finished.
/// </summary>
public sealed class ProviderResponseCompletionHandler : DelegatingHandler
{
    public static readonly TimeSpan DefaultQuietPeriod = TimeSpan.FromSeconds(2);

    public static ILogger Logger { get; set; } = NullLogger.Instance;

    private readonly TimeSpan _quietPeriod;
    private readonly ILogger _logger;

    public ProviderResponseCompletionHandler(HttpMessageHandler innerHandler, TimeSpan? quietPeriod = null, ILogger? logger = null)
        : base(innerHandler)
    {
        _quietPeriod = quietPeriod ?? DefaultQuietPeriod;
        _logger = logger ?? Logger;
    }

    public static HttpClient SharedClient { get; } = new(
        new ProviderResponseCompletionHandler(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(10)
        }),
        disposeHandler: false)
    {
        Timeout = Timeout.InfiniteTimeSpan
    };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var requestBody = await BufferRequestAsync(request, cancellationToken);
        var response = await base.SendAsync(request, cancellationToken);
        if (response.Content == null)
        {
            return response;
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        var (bytes, reason) = await ReadCompletedBodyAsync(response.Content, cancellationToken);
        if (response.IsSuccessStatusCode && TryReadFinishError(bytes, mediaType, out var providerMessage))
        {
            _logger.LogWarning(
                "Provider chat completion finished with an error. Retrying once. Message={ProviderMessage}",
                providerMessage);
            response.Dispose();
            using var retryRequest = CloneRequest(request, requestBody);
            response = await base.SendAsync(retryRequest, cancellationToken);
            if (response.Content == null)
            {
                return response;
            }

            mediaType = response.Content.Headers.ContentType?.MediaType;
            (bytes, reason) = await ReadCompletedBodyAsync(response.Content, cancellationToken);
            bytes = NormalizeFinishError(bytes, mediaType);
        }

        return ReleaseBody(response, bytes, mediaType, reason);
    }

    private HttpResponseMessage ReleaseBody(HttpResponseMessage response, byte[] bytes, string? mediaType, string reason)
    {
        // ByteArrayContent exposes a seekable MemoryStream. After the OpenAI client
        // reads an SSE body, disposing the enumerator tries to buffer that stream again
        // and throws because its position is no longer zero. A one-shot stream matches
        // a live HTTP body and lets that dispose complete.
        var content = new NonSeekableByteContent(bytes);
        CopyContentHeaders(response.Content.Headers, content.Headers);
        content.Headers.ContentLength = bytes.Length;
        response.Content.Dispose();
        response.Content = content;

        _logger.LogInformation(
            "Provider response body released. Bytes={ByteCount}, MediaType={MediaType}, Reason={Reason}",
            bytes.Length,
            mediaType,
            reason);
        return response;
    }

    private async Task<(byte[] Bytes, string Reason)> ReadCompletedBodyAsync(HttpContent content, CancellationToken cancellationToken)
    {
        var expectedLength = content.Headers.ContentLength;
        var mediaType = content.Headers.ContentType?.MediaType;
        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream();
        var scratch = new byte[16 * 1024];
        var reason = "EndOfStream";

        while (true)
        {
            if (expectedLength is long length && buffer.Length >= length)
            {
                reason = "ContentLength";
                break;
            }

            if (IsServerSentDone(buffer, mediaType))
            {
                reason = "ServerSentDone";
                break;
            }

            var jsonComplete = !IsEventStream(buffer, mediaType) && IsCompleteJson(buffer);
            using var readCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (jsonComplete)
            {
                readCancellation.CancelAfter(_quietPeriod);
            }

            int read;
            try
            {
                read = await stream.ReadAsync(scratch.AsMemory(), readCancellation.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && jsonComplete)
            {
                reason = "CompleteJsonQuiet";
                break;
            }
            catch (HttpRequestException) when (jsonComplete)
            {
                reason = "CompleteJsonInterrupted";
                break;
            }
            catch (IOException) when (jsonComplete)
            {
                reason = "CompleteJsonInterrupted";
                break;
            }

            if (read == 0)
            {
                reason = "EndOfStream";
                break;
            }

            buffer.Write(scratch, 0, read);
        }

        return (buffer.ToArray(), reason);
    }

    private static async Task<byte[]?> BufferRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content == null)
        {
            return null;
        }

        var bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
        var content = new ByteArrayContent(bytes);
        CopyContentHeaders(request.Content.Headers, content.Headers);
        request.Content = content;
        return bytes;
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage request, byte[]? body)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (body != null)
        {
            var content = new ByteArrayContent(body);
            if (request.Content != null)
            {
                CopyContentHeaders(request.Content.Headers, content.Headers);
            }

            clone.Content = content;
        }

        return clone;
    }

    private byte[] NormalizeFinishError(byte[] bytes, string? mediaType)
    {
        if (!TryParseChoices(bytes, mediaType, out var root, out var choices))
        {
            return bytes;
        }

        var changed = false;
        foreach (var choice in choices)
        {
            if (choice is not JsonObject choiceObject || !IsFinishError(choiceObject))
            {
                continue;
            }

            var providerMessage = ReadString(choiceObject["error"]?["message"]);
            _logger.LogWarning(
                "Provider chat completion still finished with an error after retry. Message={ProviderMessage}",
                providerMessage);
            choiceObject["finish_reason"] = "stop";
            if (choiceObject["message"] is not JsonObject message)
            {
                choiceObject["message"] = new JsonObject
                {
                    ["role"] = "assistant",
                    ["content"] = ""
                };
            }
            else if (message["content"] is null)
            {
                message["content"] = "";
            }

            changed = true;
        }

        return changed ? JsonSerializer.SerializeToUtf8Bytes(root) : bytes;
    }

    private static bool TryReadFinishError(byte[] bytes, string? mediaType, out string? providerMessage)
    {
        providerMessage = null;
        if (!TryParseChoices(bytes, mediaType, out _, out var choices))
        {
            return false;
        }

        foreach (var choice in choices)
        {
            if (choice is not JsonObject choiceObject || !IsFinishError(choiceObject))
            {
                continue;
            }

            providerMessage = ReadString(choiceObject["error"]?["message"]);
            return true;
        }

        return false;
    }

    private static bool TryParseChoices(byte[] bytes, string? mediaType, out JsonNode? root, out JsonArray? choices)
    {
        root = null;
        choices = null;
        if (bytes.Length == 0
            || bytes.Length > 4 * 1024 * 1024
            || (mediaType != null && mediaType.Contains("event-stream", StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        var start = 0;
        while (start < bytes.Length && IsJsonWhitespace(bytes[start]))
        {
            start++;
        }

        if (start >= bytes.Length || bytes[start] != (byte)'{')
        {
            return false;
        }

        try
        {
            root = JsonNode.Parse(bytes);
        }
        catch (JsonException)
        {
            return false;
        }

        choices = root?["choices"] as JsonArray;
        return choices != null;
    }

    private static bool IsFinishError(JsonObject choice)
    {
        return string.Equals(ReadString(choice["finish_reason"]), "error", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ReadString(JsonNode? node)
    {
        return node is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;
    }

    private static void CopyContentHeaders(HttpContentHeaders source, HttpContentHeaders destination)
    {
        foreach (var header in source)
        {
            if (header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)
                || header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            destination.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    private static bool IsServerSentDone(MemoryStream buffer, string? mediaType)
    {
        return IsEventStream(buffer, mediaType) && Contains(buffer, "data: [DONE]"u8);
    }

    private static bool IsEventStream(MemoryStream buffer, string? mediaType)
    {
        if (mediaType != null && mediaType.Contains("event-stream", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!TryGetWrittenSpan(buffer, out var span))
        {
            return false;
        }

        var start = 0;
        while (start < span.Length && span[start] is (byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n')
        {
            start++;
        }

        return span[start..].StartsWith("data:"u8);
    }

    private static bool IsCompleteJson(MemoryStream buffer)
    {
        if (!TryGetWrittenSpan(buffer, out var span))
        {
            return false;
        }

        var start = 0;
        while (start < span.Length && IsJsonWhitespace(span[start]))
        {
            start++;
        }

        if (start == span.Length)
        {
            return false;
        }

        try
        {
            var reader = new Utf8JsonReader(span[start..], isFinalBlock: true, state: default);
            if (!reader.Read())
            {
                return false;
            }

            reader.Skip();
            var end = start + (int)reader.BytesConsumed;
            while (end < span.Length && IsJsonWhitespace(span[end]))
            {
                end++;
            }

            return end == span.Length;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool IsJsonWhitespace(byte value)
    {
        return value is (byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n';
    }

    private static bool Contains(MemoryStream buffer, ReadOnlySpan<byte> marker)
    {
        return TryGetWrittenSpan(buffer, out var span) && span.IndexOf(marker) >= 0;
    }

    private static bool TryGetWrittenSpan(MemoryStream buffer, out ReadOnlySpan<byte> span)
    {
        if (buffer.Length == 0 || !buffer.TryGetBuffer(out var segment))
        {
            span = default;
            return false;
        }

        span = segment.AsSpan(0, (int)buffer.Length);
        return true;
    }

    private sealed class NonSeekableByteContent : HttpContent
    {
        private readonly byte[] _bytes;

        public NonSeekableByteContent(byte[] bytes) => _bytes = bytes;

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
