using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Shouldly;
using SufiChain.SufiPlatform.SufiFinance.Payments;
using SufiChain.SufiPlatform.SufiFinance.Payments.PayPal;
using Xunit;

namespace SufiChain.SufiPlatform.SufiFinance.Domain.Tests;

public class PayPalProviderProtocolTests
{
    [Fact]
    public async Task Request_Should_Create_Sandbox_Order_With_Public_Callback()
    {
        var handler = new RecordingHandler("""{"access_token":"access-token"}""", """{"id":"ORDER-1","status":"CREATED","links":[{"href":"https://paypal.example/approve","rel":"approve"}]}""");
        var result = await Provider(handler).RequestAsync(new(Guid.Parse("11111111-1111-1111-1111-111111111111"), 123, 10.50m, "USD", "https://foreign.example/api/finance/payments/callback/PayPal/token", GatewayEnvironment.Sandbox, Parameters()));
        result.Succeeded.ShouldBeTrue();
        result.GatewayReference.ShouldBe("ORDER-1");
        result.Transport.ShouldBeOfType<GatewayTransport.RedirectTransport>();
        handler.Requests.ShouldAllBe(x => x.RequestUri!.Host == "sandbox.example");
        handler.Bodies[1].ShouldContain("\"value\":\"10.50\"");
        handler.Bodies[1].ShouldContain("https://foreign.example/api/finance/payments/callback/PayPal/token");
    }

    [Fact]
    public async Task Callback_Should_Capture_Matching_Order()
    {
        var handler = new RecordingHandler("""{"access_token":"access-token"}""", """{"id":"ORDER-1","status":"COMPLETED","purchase_units":[{"payments":{"captures":[{"id":"CAPTURE-1","status":"COMPLETED","amount":{"currency_code":"USD","value":"10.50"}}]}}]}""");
        var result = await Provider(handler).VerifyAsync(new(Guid.NewGuid(), 123, 10.50m, "USD", "ORDER-1", GatewayEnvironment.Sandbox, Parameters(), new Dictionary<string, string> { ["token"] = "ORDER-1" }));
        result.Succeeded.ShouldBeTrue();
        result.VerifiedAmount.ShouldBe(10.50m);
        result.ProviderReference.ShouldBe("CAPTURE-1");
    }

    [Fact]
    public async Task Callback_Should_Reject_Mismatched_Order_Without_Http()
    {
        var handler = new RecordingHandler("""{"access_token":"unused"}""");
        var result = await Provider(handler).VerifyAsync(new(Guid.NewGuid(), 123, 10.50m, "USD", "ORDER-1", GatewayEnvironment.Sandbox, Parameters(), new Dictionary<string, string> { ["token"] = "tampered" }));
        result.Succeeded.ShouldBeFalse();
        result.RawCode.ShouldBe("ORDER_MISMATCH");
        handler.CallCount.ShouldBe(0);
    }

    [Fact]
    public async Task Webhook_Should_Verify_Signature_And_Return_Capture()
    {
        var handler = new RecordingHandler("""{"access_token":"access-token"}""", """{"verification_status":"SUCCESS"}""");
        var result = await Provider(handler).VerifyWebhookAsync(new("""{"id":"WH-1","event_type":"PAYMENT.CAPTURE.COMPLETED","resource":{"id":"CAPTURE-1","amount":{"currency_code":"USD","value":"10.50"},"supplementary_data":{"related_ids":{"order_id":"ORDER-1"}}}}""", Headers(), GatewayEnvironment.Sandbox, Parameters()));
        result.Succeeded.ShouldBeTrue();
        result.EventId.ShouldBe("WH-1");
        result.GatewayReference.ShouldBe("ORDER-1");
        result.ProviderReference.ShouldBe("CAPTURE-1");
        result.VerifiedAmount.ShouldBe(10.50m);
        handler.Bodies[1].ShouldContain("\"webhook_id\":\"WEBHOOK-1\"");
    }

    [Fact]
    public async Task Webhook_Should_Reject_Invalid_Signature()
    {
        var handler = new RecordingHandler("""{"access_token":"access-token"}""", """{"verification_status":"FAILURE"}""");
        var result = await Provider(handler).VerifyWebhookAsync(new("""{"id":"WH-1","event_type":"PAYMENT.CAPTURE.COMPLETED","resource":{}}""", Headers(), GatewayEnvironment.Sandbox, Parameters()));
        result.Succeeded.ShouldBeFalse();
        result.RawCode.ShouldBe("FAILURE");
    }

    [Fact]
    public async Task Refund_Should_Resolve_Capture_Then_Refund_Full_Amount()
    {
        var handler = new RecordingHandler("""{"access_token":"access-token"}""", """{"id":"ORDER-1","status":"COMPLETED","purchase_units":[{"payments":{"captures":[{"id":"CAPTURE-1","status":"COMPLETED","amount":{"currency_code":"USD","value":"10.50"}}]}}]}""", """{"id":"REFUND-1","status":"COMPLETED","amount":{"currency_code":"USD","value":"10.50"}}""");
        var result = await Provider(handler).RefundAsync(new(Guid.NewGuid(), 123, 10.50m, "USD", "ORDER-1", GatewayEnvironment.Sandbox, Parameters()));
        result.Succeeded.ShouldBeTrue();
        result.RefundedAmount.ShouldBe(10.50m);
        result.ProviderRefundReference.ShouldBe("REFUND-1");
        handler.Requests[2].RequestUri!.AbsolutePath.ShouldBe("/v2/payments/captures/CAPTURE-1/refund");
    }

    private static PayPalPaymentProvider Provider(HttpMessageHandler handler) => new(new Factory(handler), Options.Create(new PayPalPaymentProviderOptions { ApiBaseUrl = "https://production.example/", SandboxApiBaseUrl = "https://sandbox.example/" }));
    private static Dictionary<string, string> Parameters() => new() { [PayPalPaymentProvider.ClientIdParameter] = "client-id", [PayPalPaymentProvider.ClientSecretParameter] = "client-secret", [PayPalPaymentProvider.WebhookIdParameter] = "WEBHOOK-1" };
    private static Dictionary<string, string> Headers() => new(StringComparer.OrdinalIgnoreCase) { ["PAYPAL-AUTH-ALGO"] = "SHA256withRSA", ["PAYPAL-CERT-URL"] = "https://paypal.example/cert", ["PAYPAL-TRANSMISSION-ID"] = "id", ["PAYPAL-TRANSMISSION-SIG"] = "sig", ["PAYPAL-TRANSMISSION-TIME"] = "2026-07-15T00:00:00Z" };
    private sealed class Factory(HttpMessageHandler handler) : IHttpClientFactory { public HttpClient CreateClient(string name) => new(handler, false); }
    private sealed class RecordingHandler(params string[] responses) : HttpMessageHandler
    {
        public int CallCount { get; private set; }
        public List<HttpRequestMessage> Requests { get; } = [];
        public List<string> Bodies { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            Bodies.Add(request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken));
            var body = responses[Math.Min(CallCount++, responses.Length - 1)];
            return new(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        }
    }
}
