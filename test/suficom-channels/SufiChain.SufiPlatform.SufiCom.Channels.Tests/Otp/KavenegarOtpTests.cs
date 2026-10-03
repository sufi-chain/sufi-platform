using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Channels.Sms.Kavenegar;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Channels.Otp;

public class KavenegarOtpTests
{
    [Fact]
    public async Task Lookup_Should_Post_The_Verify_Form_To_The_Lookup_Endpoint()
    {
        var handler = new RecordingHttpMessageHandler(
            """{"return":{"status":200,"message":"تایید شد"},"entries":[{"messageid":8792343,"status":5}]}""");
        var client = new KavenegarVerifyLookupClient(new HttpClient(handler));

        var result = await client.LookupAsync("test-key", "09121234567", "123456", "verify", KavenegarVerifyLookupClient.TypeSms);

        result.Success.ShouldBeTrue();
        result.MessageId.ShouldBe(8792343);
        var (request, body) = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.RequestUri!.ToString().ShouldBe("https://api.kavenegar.com/v1/test-key/verify/lookup.json");
        body.ShouldBe("receptor=09121234567&token=123456&template=verify&type=sms");
    }

    [Theory]
    [InlineData(409, true)]
    [InlineData(500, true)]
    [InlineData(424, false)]
    [InlineData(418, false)]
    public void Parse_Should_Mark_Only_Retryable_Statuses_As_Transient(int status, bool transient)
    {
        var result = KavenegarVerifyLookupClient.Parse(
            $$"""{"return":{"status":{{status}},"message":"error"},"entries":null}""",
            200);

        result.Success.ShouldBeFalse();
        result.IsTransientFailure.ShouldBe(transient);
    }

    [Fact]
    public void Parse_Should_Survive_A_Non_Json_Body()
    {
        var result = KavenegarVerifyLookupClient.Parse("<html>bad gateway</html>", 502);

        result.Success.ShouldBeFalse();
        result.IsTransientFailure.ShouldBeTrue();
    }

    [Fact]
    public void Sms_Channel_Should_Report_A_Missing_Otp_Template()
    {
        var channel = NewSmsChannel(new FakeLookupClient(), withTemplate: false);

        channel.GetOtpConfigurationError().ShouldBe(OtpProviderSettingKeys.TemplateRequiredError);
    }

    [Fact]
    public async Task Sms_Channel_Should_Not_Call_The_Gateway_Without_A_Template()
    {
        var lookup = new FakeLookupClient();
        var channel = NewSmsChannel(lookup, withTemplate: false);

        var result = await channel.SendOtpAsync(NewOtp(OtpPurposes.Login));

        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldBe(OtpProviderSettingKeys.TemplateRequiredError);
        lookup.Calls.ShouldBeEmpty();
    }

    [Fact]
    public async Task Sms_Channel_Should_Use_The_Purpose_Template_And_Local_Number()
    {
        var lookup = new FakeLookupClient();
        var channel = NewSmsChannel(lookup, withTemplate: true);

        var result = await channel.SendOtpAsync(NewOtp(OtpPurposes.Login));

        result.Success.ShouldBeTrue();
        result.ExternalId.ShouldBe("42");
        var call = lookup.Calls.ShouldHaveSingleItem();
        call.Receptor.ShouldBe("09121234567");
        call.Token.ShouldBe("123456");
        call.Template.ShouldBe("login-verify");
        call.Type.ShouldBe(KavenegarVerifyLookupClient.TypeSms);
    }

    [Fact]
    public async Task Sms_Channel_Should_Fall_Back_To_The_Default_Template_For_Other_Purposes()
    {
        var lookup = new FakeLookupClient();
        var channel = NewSmsChannel(lookup, withTemplate: true);

        await channel.SendOtpAsync(NewOtp(OtpPurposes.TwoFactor));

        lookup.Calls.ShouldHaveSingleItem().Template.ShouldBe("verify");
    }

    private static KavenegarSmsChannel NewSmsChannel(FakeLookupClient lookup, bool withTemplate)
    {
        var channel = new KavenegarSmsChannel(NullLogger<KavenegarSmsChannel>.Instance, lookup);
        var settings = new Dictionary<string, string>
        {
            ["ApiKey"] = "test-key",
            ["SenderNumber"] = "1000"
        };

        if (withTemplate)
        {
            settings[OtpProviderSettingKeys.Template] = "verify";
            settings[OtpProviderSettingKeys.TemplateLogin] = "login-verify";
        }

        channel.Configure(settings);
        return channel;
    }

    private static OtpMessage NewOtp(string purpose) => new()
    {
        Phone = "+989121234567",
        Code = "123456",
        Purpose = purpose,
        Content = "Your code is 123456",
        IdempotencyKey = "Login:+989121234567:1"
    };

    private sealed class FakeLookupClient : KavenegarVerifyLookupClient
    {
        public FakeLookupClient()
            : base(new HttpClient(new RecordingHttpMessageHandler("{}", HttpStatusCode.InternalServerError)))
        {
        }

        public List<(string Receptor, string Token, string Template, string Type)> Calls { get; } = new();

        public override Task<KavenegarLookupResult> LookupAsync(
            string apiKey,
            string receptor,
            string token,
            string template,
            string type,
            CancellationToken cancellationToken = default)
        {
            Calls.Add((receptor, token, template, type));
            return Task.FromResult(new KavenegarLookupResult { ReturnStatus = 200, MessageId = 42 });
        }
    }
}
