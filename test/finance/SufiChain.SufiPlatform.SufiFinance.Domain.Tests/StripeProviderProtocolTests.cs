using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Shouldly;
using SufiChain.SufiPlatform.SufiFinance.Payments;
using SufiChain.SufiPlatform.SufiFinance.Payments.Stripe;
using Xunit;

namespace SufiChain.SufiPlatform.SufiFinance.Domain.Tests;

public class StripeProviderProtocolTests
{
    [Fact]
    public async Task Request_Should_Create_Checkout_Session_With_Minor_Units_And_Callback()
    {
        var handler = new RecordingHandler("""{"id":"cs_test_123","url":"https://checkout.stripe.example/session"}""");
        var provider = CreateProvider(handler);
        var result = await provider.RequestAsync(new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            123, 12.34m, "USD",
            "https://edge.example/api/finance/payments/callback/Stripe/token",
            GatewayEnvironment.Sandbox, Parameters()));

        result.Succeeded.ShouldBeTrue();
        result.GatewayReference.ShouldBe("cs_test_123");
        result.Transport.ShouldBeOfType<GatewayTransport.RedirectTransport>();
        handler.LastRequest!.Headers.Authorization!.Parameter.ShouldBe("sk_test_secret");
        handler.LastRequest.Headers.GetValues("Idempotency-Key").Single()
            .ShouldBe("sufifinance-payment-11111111111111111111111111111111");
        handler.LastBody.ShouldContain("line_items%5B0%5D%5Bprice_data%5D%5Bunit_amount%5D=1234");
        handler.LastBody.ShouldContain("session_id=%7BCHECKOUT_SESSION_ID%7D");
    }

    [Fact]
    public async Task Callback_Should_Retrieve_Paid_Session()
    {
        var handler = new RecordingHandler(
            """{"id":"cs_test_123","payment_status":"paid","payment_intent":"pi_123","amount_total":1234,"currency":"usd"}""");
        var provider = CreateProvider(handler);
        var result = await provider.VerifyAsync(new(
            Guid.NewGuid(), 123, 12.34m, "USD", "cs_test_123",
            GatewayEnvironment.Sandbox, Parameters(),
            new Dictionary<string, string> { ["session_id"] = "cs_test_123" }));

        result.Succeeded.ShouldBeTrue();
        result.VerifiedAmount.ShouldBe(12.34m);
        result.ProviderReference.ShouldBe("pi_123");
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Get);
    }

    [Fact]
    public async Task Webhook_Should_Reject_Invalid_Signature()
    {
        var result = await CreateProvider(new RecordingHandler("{}")).VerifyWebhookAsync(new(
            CompletedEventBody(),
            new Dictionary<string, string> { ["Stripe-Signature"] = "t=1700000000,v1=bad" },
            GatewayEnvironment.Sandbox,
            Parameters()));

        result.Succeeded.ShouldBeFalse();
        result.MessageKey.ShouldBe("Payments:Provider:Stripe:InvalidWebhookSignature");
    }

    [Fact]
    public async Task Webhook_Should_Verify_Signature_And_Return_Completion_Data()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1_700_000_000);
        var body = CompletedEventBody();
        var provider = CreateProvider(new RecordingHandler("{}"), new FixedTimeProvider(now));
        var result = await provider.VerifyWebhookAsync(new(
            body,
            new Dictionary<string, string> { ["Stripe-Signature"] = Sign(body, now.ToUnixTimeSeconds(), "whsec_secret") },
            GatewayEnvironment.Sandbox,
            Parameters()));

        result.Succeeded.ShouldBeTrue();
        result.EventId.ShouldBe("evt_123");
        result.GatewayReference.ShouldBe("cs_test_123");
        result.VerifiedAmount.ShouldBe(12.34m);
        result.CurrencyCode.ShouldBe("USD");
        result.ProviderReference.ShouldBe("pi_123");
    }

    [Fact]
    public async Task Refund_Should_Retrieve_Payment_Intent_Then_Create_Full_Refund()
    {
        var handler = new RecordingHandler(
            """{"id":"cs_test_123","payment_intent":"pi_123"}""",
            """{"id":"re_123","status":"succeeded","amount":1234}""");
        var result = await CreateProvider(handler).RefundAsync(new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            123, 12.34m, "USD", "cs_test_123",
            GatewayEnvironment.Sandbox, Parameters()));

        result.Succeeded.ShouldBeTrue();
        result.RefundedAmount.ShouldBe(12.34m);
        result.ProviderRefundReference.ShouldBe("re_123");
        handler.Requests[0].Method.ShouldBe(HttpMethod.Get);
        handler.Requests[1].Headers.GetValues("Idempotency-Key").Single()
            .ShouldBe("sufifinance-refund-11111111111111111111111111111111");
        handler.Bodies[1].ShouldContain("payment_intent=pi_123");
        handler.Bodies[1].ShouldContain("amount=1234");
    }

    private static StripePaymentProvider CreateProvider(HttpMessageHandler handler, TimeProvider? timeProvider = null) =>
        new(
            new StubHttpClientFactory(handler),
            Options.Create(new StripePaymentProviderOptions { ApiBaseUrl = "https://api.stripe.example/v1/" }),
            timeProvider);

    private static Dictionary<string, string> Parameters() => new()
    {
        [StripePaymentProvider.SecretKeyParameter] = "sk_test_secret",
        [StripePaymentProvider.PublishableKeyParameter] = "pk_test_public",
        [StripePaymentProvider.WebhookSecretParameter] = "whsec_secret"
    };

    private static string CompletedEventBody() =>
        """{"id":"evt_123","type":"checkout.session.completed","data":{"object":{"id":"cs_test_123","payment_status":"paid","payment_intent":"pi_123","amount_total":1234,"currency":"usd"}}}""";

    private static string Sign(string body, long timestamp, string secret)
    {
        var hash = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret),
            Encoding.UTF8.GetBytes($"{timestamp.ToString(CultureInfo.InvariantCulture)}.{body}"));
        return $"t={timestamp.ToString(CultureInfo.InvariantCulture)},v1={Convert.ToHexString(hash).ToLowerInvariant()}";
    }

    private sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class RecordingHandler(params string[] responses) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public string LastBody { get; private set; } = string.Empty;
        public List<HttpRequestMessage> Requests { get; } = [];
        public List<string> Bodies { get; } = [];
        private int CallCount { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastBody = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(request);
            Bodies.Add(LastBody);
            var response = responses[Math.Min(CallCount, responses.Length - 1)];
            CallCount++;
            return new(HttpStatusCode.OK) { Content = new StringContent(response, Encoding.UTF8, "application/json") };
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
