using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Shouldly;
using SufiChain.SufiPlatform.SufiFinance.Payments;
using SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.AsanPardakht;
using SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.IdPay;
using SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Mellat;
using SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Melli;
using SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Parsian;
using SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Pasargad;
using SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Saman;
using SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Sepehr;
using SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Zibal;
using Xunit;

namespace SufiChain.SufiPlatform.SufiFinance.Domain.Tests;

public class ShaparakProviderProtocolTests
{
    [Fact]
    public async Task Zibal_Should_Convert_Tomans_To_Rials_And_Return_Redirect()
    {
        var handler = new RecordingHandler("""{"trackId":42,"result":100}""");
        var provider = new ShaparakZibalPaymentProvider(
            new StubHttpClientFactory(handler),
            Options.Create(new ZibalPaymentProviderOptions()));

        var result = await provider.RequestAsync(CreateRequestContext(
            "IRT",
            new Dictionary<string, string> { [ShaparakZibalPaymentProvider.MerchantParameter] = "merchant" }));

        result.Succeeded.ShouldBeTrue();
        result.GatewayReference.ShouldBe("42");
        result.Transport.ShouldBeOfType<GatewayTransport.RedirectTransport>();
        handler.LastBody.ShouldContain("\"amount\":12500");
        handler.LastBody.ShouldContain("\"callbackUrl\":\"https://edge.example/callback\"");
    }

    [Fact]
    public async Task IdPay_Sandbox_Should_Set_Sandbox_Headers_And_Convert_Amount()
    {
        var handler = new RecordingHandler("""{"id":"payment-id","link":"https://idpay.example/pay"}""");
        var provider = new ShaparakIdPayPaymentProvider(
            new StubHttpClientFactory(handler),
            Options.Create(new IdPayPaymentProviderOptions()));

        var result = await provider.RequestAsync(CreateRequestContext(
            "IRT",
            new Dictionary<string, string>(),
            GatewayEnvironment.Sandbox));

        result.Succeeded.ShouldBeTrue();
        handler.LastBody.ShouldContain("\"amount\":12500");
        handler.LastRequest!.Headers.GetValues("X-SANDBOX").Single().ShouldBe("1");
        handler.LastRequest.Headers.Contains("X-API-KEY").ShouldBeTrue();
    }

    [Fact]
    public async Task Sepehr_Should_Reject_Callback_Amount_Mismatch_Without_Http_Verification()
    {
        var handler = new RecordingHandler("""{"status":"OK","returnId":"12500"}""");
        var provider = new ShaparakSepehrPaymentProvider(
            new StubHttpClientFactory(handler),
            Options.Create(new SepehrPaymentProviderOptions()));
        var callback = new PaymentCallbackContext(
            Guid.NewGuid(), 123, 1250, "IRT", "token", GatewayEnvironment.Production,
            new Dictionary<string, string> { [ShaparakSepehrPaymentProvider.TerminalIdParameter] = "7" },
            new Dictionary<string, string>
            {
                ["respcode"] = "0",
                ["invoiceid"] = "123",
                ["terminalid"] = "7",
                ["amount"] = "999",
                ["digitalreceipt"] = "receipt"
            });

        var result = await provider.VerifyAsync(callback);

        result.Succeeded.ShouldBeFalse();
        handler.CallCount.ShouldBe(0);
    }

    [Theory]
    [InlineData("IRR", 1250, "<amount>1250</amount>")]
    [InlineData("IRT", 1250, "<amount>12500</amount>")]
    public async Task Mellat_Should_Convert_Amount_To_Rials_And_Return_Form(
        string currency,
        decimal amount,
        string expectedAmount)
    {
        var handler = new RecordingHandler(SoapReturn("0,reference"));
        var provider = new ShaparakMellatPaymentProvider(
            new StubHttpClientFactory(handler),
            Options.Create(new MellatPaymentProviderOptions()));

        var result = await provider.RequestAsync(CreateRequestContext(currency, new Dictionary<string, string>
        {
            [ShaparakMellatPaymentProvider.TerminalIdParameter] = "7",
            [ShaparakMellatPaymentProvider.UserNameParameter] = "user",
            [ShaparakMellatPaymentProvider.UserPasswordParameter] = "password"
        }, amount: amount));

        result.Succeeded.ShouldBeTrue();
        result.Transport.ShouldBeOfType<GatewayTransport.FormTransport>();
        handler.LastBody.ShouldContain(expectedAmount);
        handler.LastBody.ShouldContain("<callBackUrl>https://edge.example/callback</callBackUrl>");
    }

    [Fact]
    public async Task Saman_Should_Convert_Tomans_To_Rials_And_Return_Form()
    {
        var handler = new RecordingHandler("""{"status":1,"token":"saman-token"}""");
        var provider = new ShaparakSamanPaymentProvider(
            new StubHttpClientFactory(handler),
            Options.Create(new SamanPaymentProviderOptions()));

        var result = await provider.RequestAsync(CreateRequestContext("IRT", new Dictionary<string, string>
        {
            [ShaparakSamanPaymentProvider.TerminalIdParameter] = "123"
        }));

        result.Succeeded.ShouldBeTrue();
        result.Transport.ShouldBeOfType<GatewayTransport.FormTransport>();
        handler.LastBody.ShouldContain("\"amount\":12500");
        handler.LastBody.ShouldContain("\"redirectUrl\":\"https://edge.example/callback\"");
    }

    [Fact]
    public async Task Parsian_Should_Reject_Tampered_Callback_Without_Http_Verification()
    {
        var handler = new RecordingHandler("<response><Status>0</Status><RRN>rrn</RRN></response>");
        var provider = new ShaparakParsianPaymentProvider(
            new StubHttpClientFactory(handler),
            Options.Create(new ParsianPaymentProviderOptions()));

        var result = await provider.VerifyAsync(CreateCallbackContext(
            "IRT",
            "expected-token",
            new Dictionary<string, string> { [ShaparakParsianPaymentProvider.LoginAccountParameter] = "account" },
            new Dictionary<string, string>
            {
                ["status"] = "0",
                ["token"] = "tampered-token",
                ["orderId"] = "123",
                ["amount"] = "12500"
            }));

        result.Succeeded.ShouldBeFalse();
        handler.CallCount.ShouldBe(0);
    }

    [Fact]
    public async Task Pasargad_Should_Use_Sandbox_Host_And_Convert_Tomans_To_Rials()
    {
        var handler = new RecordingHandler(
            """{"resultCode":0,"token":"access-token"}""",
            """{"resultCode":0,"data":{"urlId":"url-id","url":"https://pay.example/redirect"}}""");
        var provider = new PasargadPaymentProvider(
            new StubHttpClientFactory(handler),
            Options.Create(new PasargadPaymentProviderOptions
            {
                ApiBaseUrl = "https://production.example/",
                SandboxApiBaseUrl = "https://sandbox.example/"
            }));

        var result = await provider.RequestAsync(CreateRequestContext(
            "IRT",
            new Dictionary<string, string>
            {
                [PasargadPaymentProvider.UsernameParameter] = "user",
                [PasargadPaymentProvider.PasswordParameter] = "password",
                [PasargadPaymentProvider.TerminalNumberParameter] = "terminal"
            },
            GatewayEnvironment.Sandbox));

        result.Succeeded.ShouldBeTrue();
        handler.Requests.ShouldAllBe(request => request.RequestUri!.Host == "sandbox.example");
        handler.Bodies.Last().ShouldContain("\"amount\":12500");
        handler.Bodies.Last().ShouldContain("\"callbackApi\":\"https://edge.example/callback\"");
    }

    [Fact]
    public async Task AsanPardakht_Should_Convert_Tomans_And_Preserve_Callback_Url()
    {
        var handler = new RecordingHandler(
            SoapNode("EncryptInAESResult", "encrypted-request"),
            SoapNode("RequestOperationResult", "0,reference"));
        var provider = new AsanPardakhtPaymentProvider(
            new StubHttpClientFactory(handler),
            Options.Create(new AsanPardakhtPaymentProviderOptions()));

        var result = await provider.RequestAsync(CreateRequestContext("IRT", AsanPardakhtParameters()));

        result.Succeeded.ShouldBeTrue();
        handler.Bodies.First().ShouldContain("123,12500,");
        handler.Bodies.First().ShouldContain("https://edge.example/callback");
        result.Transport.ShouldBeOfType<GatewayTransport.FormTransport>();
    }

    [Fact]
    public async Task Melli_Should_Reject_Tampered_Callback_Without_Http_Verification()
    {
        var handler = new RecordingHandler("""{"resCode":0,"amount":12500,"orderId":123}""");
        var provider = new MelliPaymentProvider(
            new StubHttpClientFactory(handler),
            Options.Create(new MelliPaymentProviderOptions()));

        var result = await provider.VerifyAsync(CreateCallbackContext(
            "IRT",
            "expected-token",
            MelliParameters(),
            new Dictionary<string, string>
            {
                ["ResCode"] = "0",
                ["Token"] = "tampered-token",
                ["OrderId"] = "123"
            }));

        result.Succeeded.ShouldBeFalse();
        handler.CallCount.ShouldBe(0);
    }

    [Fact]
    public async Task Melli_Should_Convert_Tomans_To_Rials_And_Return_Redirect()
    {
        var handler = new RecordingHandler("""{"resCode":0,"token":"melli-token"}""");
        var provider = new MelliPaymentProvider(
            new StubHttpClientFactory(handler),
            Options.Create(new MelliPaymentProviderOptions()));

        var result = await provider.RequestAsync(CreateRequestContext("IRT", MelliParameters()));

        result.Succeeded.ShouldBeTrue();
        result.Transport.ShouldBeOfType<GatewayTransport.RedirectTransport>();
        handler.LastBody.ShouldContain("\"amount\":12500");
        handler.LastBody.ShouldContain("\"returnUrl\":\"https://edge.example/callback\"");
    }

    private static PaymentProviderContext CreateRequestContext(
        string currency,
        IReadOnlyDictionary<string, string> parameters,
        GatewayEnvironment environment = GatewayEnvironment.Production,
        decimal amount = 1250) =>
        new(Guid.NewGuid(), 123, amount, currency, "https://edge.example/callback", environment, parameters);

    private static PaymentCallbackContext CreateCallbackContext(
        string currency,
        string gatewayReference,
        IReadOnlyDictionary<string, string> parameters,
        IReadOnlyDictionary<string, string> callbackValues) =>
        new(Guid.NewGuid(), 123, 1250, currency, gatewayReference, GatewayEnvironment.Production, parameters, callbackValues);

    private static Dictionary<string, string> AsanPardakhtParameters() => new()
    {
        [AsanPardakhtPaymentProvider.MerchantConfigurationIdParameter] = "merchant",
        [AsanPardakhtPaymentProvider.UsernameParameter] = "user",
        [AsanPardakhtPaymentProvider.PasswordParameter] = "password",
        [AsanPardakhtPaymentProvider.KeyParameter] = "key",
        [AsanPardakhtPaymentProvider.IvParameter] = "iv"
    };

    private static Dictionary<string, string> MelliParameters() => new()
    {
        [MelliPaymentProvider.TerminalIdParameter] = "terminal",
        [MelliPaymentProvider.MerchantIdParameter] = "merchant",
        [MelliPaymentProvider.TerminalKeyParameter] = Convert.ToBase64String(new byte[24])
    };

    private static string SoapReturn(string value) =>
        $"<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body><return>{value}</return></soap:Body></soap:Envelope>";

    private static string SoapNode(string name, string value) =>
        $"<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body><{name}>{value}</{name}></soap:Body></soap:Envelope>";

    private sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class RecordingHandler(params string[] responses) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public string LastBody { get; private set; } = string.Empty;
        public int CallCount { get; private set; }
        public List<HttpRequestMessage> Requests { get; } = [];
        public List<string> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequest = request;
            LastBody = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(request);
            Bodies.Add(LastBody);
            var response = responses[Math.Min(CallCount - 1, responses.Length - 1)];
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json")
            };
        }
    }
}
