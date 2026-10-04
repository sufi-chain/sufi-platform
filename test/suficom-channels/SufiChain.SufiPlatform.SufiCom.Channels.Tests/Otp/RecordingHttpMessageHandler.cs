using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiCom.Channels.Otp;

/// <summary>
/// Returns a fixed response and records each request with its body, so tests never reach a real gateway.
/// </summary>
internal sealed class RecordingHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseBody;

    public RecordingHttpMessageHandler(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responseBody = responseBody;
        _statusCode = statusCode;
    }

    public List<(HttpRequestMessage Request, string Body)> Requests { get; } = new();

    public Exception? Fault { get; set; }

    public bool WaitUntilCancelled { get; set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
        Requests.Add((request, body));

        if (WaitUntilCancelled)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }

        if (Fault != null)
        {
            throw Fault;
        }

        return new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
        };
    }
}
