using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiCom.Channels.Sms.Kavenegar;

/// <summary>
/// Calls Kavenegar Verify Lookup (<c>/v1/{apiKey}/verify/lookup.json</c>), the transactional OTP route
/// that sends through an approved template. Uses one shared <see cref="HttpClient"/> and passes the API key
/// per request, so concurrent tenants with different keys never share mutable client state.
/// </summary>
public class KavenegarVerifyLookupClient
{
    public const string BaseUrl = "https://api.kavenegar.com/v1";

    public const string TypeSms = "sms";

    public const string TypeCall = "call";

    public const int DefaultTimeoutSeconds = 30;

    // The API key is part of the URL, so the shared client must not apply its own timeout.
    // Each call passes a cancellation token from TimeoutSeconds.
    private static readonly HttpClient SharedHttpClient = new() { Timeout = Timeout.InfiniteTimeSpan };

    private readonly HttpClient _httpClient;

    public KavenegarVerifyLookupClient()
        : this(SharedHttpClient)
    {
    }

    public KavenegarVerifyLookupClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public virtual Task<KavenegarLookupResult> LookupAsync(
        string apiKey,
        string receptor,
        string token,
        string template,
        string type,
        CancellationToken cancellationToken = default)
    {
        return LookupAsync(
            apiKey,
            receptor,
            token,
            template,
            type,
            TimeSpan.FromSeconds(DefaultTimeoutSeconds),
            cancellationToken);
    }

    public virtual async Task<KavenegarLookupResult> LookupAsync(
        string apiKey,
        string receptor,
        string token,
        string template,
        string type,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}/{Uri.EscapeDataString(apiKey)}/verify/lookup.json";
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["receptor"] = receptor,
            ["token"] = token,
            ["template"] = template,
            ["type"] = type
        });

        using var timeoutSource = new CancellationTokenSource(NormalizeTimeout(timeout));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);
        using var response = await _httpClient.PostAsync(url, content, linked.Token);
        var body = await response.Content.ReadAsStringAsync(linked.Token);

        var parsed = Parse(body, (int)response.StatusCode);
        parsed.HttpStatusCode = (int)response.StatusCode;
        parsed.ResponseBody = body;
        return parsed;
    }

    private static TimeSpan NormalizeTimeout(TimeSpan timeout)
    {
        return timeout > TimeSpan.Zero ? timeout : TimeSpan.FromSeconds(DefaultTimeoutSeconds);
    }

    public static KavenegarLookupResult Parse(string body, int httpStatusCode)
    {
        var result = new KavenegarLookupResult { ReturnStatus = httpStatusCode };

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (root.TryGetProperty("return", out var returnElement))
            {
                if (TryReadInt32(returnElement, "status", out var statusValue))
                {
                    result.ReturnStatus = statusValue;
                }

                if (returnElement.TryGetProperty("message", out var message))
                {
                    result.ReturnMessage = message.GetString();
                }
            }

            if (root.TryGetProperty("entries", out var entries))
            {
                // The REST sample returns an array. The Node client sample returns one object.
                JsonElement entry;
                if (entries.ValueKind == JsonValueKind.Array && entries.GetArrayLength() > 0)
                {
                    entry = entries[0];
                }
                else if (entries.ValueKind == JsonValueKind.Object)
                {
                    entry = entries;
                }
                else
                {
                    entry = default;
                }

                if (entry.ValueKind == JsonValueKind.Object)
                {
                    if (TryReadInt64(entry, "messageid", out var messageId) ||
                        TryReadInt64(entry, "messageId", out messageId))
                    {
                        result.MessageId = messageId;
                    }

                    if (TryReadInt32(entry, "status", out var entryStatusValue))
                    {
                        result.EntryStatus = entryStatusValue;
                    }
                }
            }
        }
        catch (JsonException)
        {
            result.ReturnMessage = null;
        }

        return result;
    }

    private static bool TryReadInt64(JsonElement element, string name, out long value)
    {
        value = 0;
        if (!element.TryGetProperty(name, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out value))
        {
            return true;
        }

        return property.ValueKind == JsonValueKind.String &&
               long.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryReadInt32(JsonElement element, string name, out int value)
    {
        value = 0;
        if (!element.TryGetProperty(name, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out value))
        {
            return true;
        }

        return property.ValueKind == JsonValueKind.String &&
               int.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }
}

public class KavenegarLookupResult
{
    /// <summary>
    /// Kavenegar <c>return.status</c>; 200 means accepted.
    /// </summary>
    public int ReturnStatus { get; set; }

    public int HttpStatusCode { get; set; }

    /// <summary>
    /// Raw response body. Callers redact this before logging. The request URL contains the API key and is not stored here.
    /// </summary>
    public string? ResponseBody { get; set; }

    public string? ReturnMessage { get; set; }

    public long? MessageId { get; set; }

    public int? EntryStatus { get; set; }

    public bool Success => ReturnStatus == 200 && MessageId.HasValue;

    /// <summary>
    /// 409 means the server could not respond; 5xx are gateway failures. Both may be retried.
    /// </summary>
    public bool IsTransientFailure => !Success && (ReturnStatus == 409 || ReturnStatus >= 500);
}
