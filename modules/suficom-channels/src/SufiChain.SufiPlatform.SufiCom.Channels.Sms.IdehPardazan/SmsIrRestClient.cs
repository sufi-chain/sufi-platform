using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiCom.Channels.Sms.IdehPardazan;

/// <summary>
/// Minimal SMS.ir v1 REST client for the transactional Verify route (<c>POST /v1/send/verify</c>)
/// and the credit check (<c>GET /v1/credit</c>). Uses a shared <see cref="HttpClient"/> and sends the API key per request.
/// </summary>
public class SmsIrRestClient
{
    public const string BaseUrl = "https://api.sms.ir/v1";

    public const int DefaultTimeoutSeconds = 30;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly HttpClient SharedHttpClient = new() { Timeout = Timeout.InfiniteTimeSpan };

    private readonly HttpClient _httpClient;

    public SmsIrRestClient()
        : this(SharedHttpClient)
    {
    }

    public SmsIrRestClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public virtual Task<SmsIrResult> VerifySendAsync(
        string apiKey,
        string mobile,
        int templateId,
        string parameterName,
        string parameterValue,
        CancellationToken cancellationToken = default)
    {
        return VerifySendAsync(
            apiKey,
            mobile,
            templateId,
            parameterName,
            parameterValue,
            TimeSpan.FromSeconds(DefaultTimeoutSeconds),
            cancellationToken);
    }

    public virtual async Task<SmsIrResult> VerifySendAsync(
        string apiKey,
        string mobile,
        int templateId,
        string parameterName,
        string parameterValue,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(new
        {
            mobile,
            templateId,
            parameters = new[] { new { name = parameterName, value = parameterValue } }
        }, JsonOptions);

        var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "/send/verify")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        AddHeaders(request, apiKey);

        return await SendAsync(request, timeout, cancellationToken);
    }

    public virtual Task<SmsIrResult> GetCreditAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        return GetCreditAsync(apiKey, TimeSpan.FromSeconds(DefaultTimeoutSeconds), cancellationToken);
    }

    public virtual Task<SmsIrResult> GetCreditAsync(string apiKey, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/credit");
        AddHeaders(request, apiKey);
        return SendAsync(request, timeout, cancellationToken);
    }

    private static void AddHeaders(HttpRequestMessage request, string apiKey)
    {
        request.Headers.TryAddWithoutValidation("x-api-key", apiKey);
        // The published sample sends Accept: text/plain and still returns the JSON status envelope.
        request.Headers.TryAddWithoutValidation("Accept", "text/plain, application/json");
    }

    private async Task<SmsIrResult> SendAsync(HttpRequestMessage request, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var timeoutSource = new CancellationTokenSource(timeout > TimeSpan.Zero ? timeout : TimeSpan.FromSeconds(DefaultTimeoutSeconds));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);
        using (request)
        {
            using var response = await _httpClient.SendAsync(request, linked.Token);
            var body = await response.Content.ReadAsStringAsync(linked.Token);
            var parsed = Parse(body, (int)response.StatusCode);
            parsed.ResponseBody = body;
            return parsed;
        }
    }

    public static SmsIrResult Parse(string body, int httpStatusCode)
    {
        var result = new SmsIrResult { HttpStatusCode = httpStatusCode };

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (TryReadInt32(root, "status", out var statusValue))
            {
                result.Status = statusValue;
            }

            if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
            {
                result.Message = message.GetString();
            }

            if (root.TryGetProperty("data", out var data))
            {
                if (data.ValueKind == JsonValueKind.Object &&
                    TryReadInt64(data, "messageId", out var messageIdValue))
                {
                    result.MessageId = messageIdValue;
                }
                else if (data.ValueKind == JsonValueKind.Number && data.TryGetDecimal(out var credit))
                {
                    result.Credit = credit;
                }
            }
        }
        catch (JsonException)
        {
            result.Message = null;
        }

        return result;
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
}

public class SmsIrResult
{
    public int HttpStatusCode { get; set; }

    /// <summary>
    /// Raw response body. Callers redact this before logging.
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// SMS.ir body status; 1 means success.
    /// </summary>
    public int Status { get; set; }

    public string? Message { get; set; }

    public long? MessageId { get; set; }

    public decimal? Credit { get; set; }

    public bool Success => HttpStatusCode == 200 && Status == 1;

    /// <summary>
    /// 429 (rate limited) and 5xx (unexpected server error) may be retried.
    /// </summary>
    public bool IsTransientFailure => !Success && (HttpStatusCode == 429 || HttpStatusCode >= 500);
}
