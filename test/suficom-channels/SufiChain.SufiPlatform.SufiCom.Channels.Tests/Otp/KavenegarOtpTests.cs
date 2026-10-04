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
    public void Parse_Should_Read_A_Single_Entry_Object_And_A_String_Message_Id()
    {
        var result = KavenegarVerifyLookupClient.Parse(
            """{"return":{"status":"200","message":"تایید شد"},"entries":{"messageid":"8792343","status":"5"}}""",
            200);

        result.Success.ShouldBeTrue();
        result.MessageId.ShouldBe(8792343);
        result.EntryStatus.ShouldBe(5);
    }

    [Fact]
    public async Task Lookup_Should_Keep_Persian_Token_Text_In_The_Form_Body()
    {
        var handler = new RecordingHttpMessageHandler(
            """{"return":{"status":200,"message":"تایید شد"},"entries":[{"messageid":1,"status":5}]}""");
        var client = new KavenegarVerifyLookupClient(new HttpClient(handler));

        await client.LookupAsync("test-key", "09121234567", "کد۱۲۳", "verify", KavenegarVerifyLookupClient.TypeSms);

        var body = handler.Requests.ShouldHaveSingleItem().Body;
        FormValue(body, "token").ShouldBe("کد۱۲۳");
        body.ShouldNotContain("\\u");
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

    [Theory]
    [InlineData("09121234567")]
    [InlineData("+989121234567")]
    [InlineData("۰۹۱۲۱۲۳۴۵۶۷")]
    public async Task Sms_Channel_Should_Post_The_Local_Number_And_Accept_The_Lookup(string phone)
    {
        var handler = new RecordingHttpMessageHandler(
            """{"return":{"status":200,"message":"تایید شد"},"entries":[{"messageid":8792343,"status":5}]}""");
        var logger = new CapturingLogger<KavenegarSmsChannel>();
        var channel = NewHttpChannel(handler, logger, "super-secret-key");

        var result = await channel.SendOtpAsync(NewOtp(OtpPurposes.Login, phone, "654321"));

        result.Success.ShouldBeTrue();
        result.ExternalId.ShouldBe("8792343");
        var (request, body) = handler.Requests.ShouldHaveSingleItem();
        request.RequestUri!.ToString().ShouldContain("/verify/lookup.json");
        request.RequestUri.ToString().ShouldContain("super-secret-key");
        FormValue(body, "receptor").ShouldBe("09121234567");
        FormValue(body, "token").ShouldBe("654321");
        logger.Lines.ShouldAllBe(line => !line.Contains("super-secret-key") && !line.Contains("654321"));
    }

    [Fact]
    public async Task Sms_Channel_Should_Report_A_Provider_Status_As_A_Localization_Key()
    {
        var handler = new RecordingHttpMessageHandler(
            """{"return":{"status":418,"message":"اعتبار کافی نیست"},"entries":null}""");
        var channel = NewHttpChannel(handler, new CapturingLogger<KavenegarSmsChannel>(), "super-secret-key");

        var result = await channel.SendOtpAsync(NewOtp(OtpPurposes.Login));

        result.Success.ShouldBeFalse();
        result.IsTransientFailure.ShouldBeFalse();
        result.StatusCode.ShouldBe(418);
        result.ErrorMessage.ShouldBe(OtpFailureMessages.Format(OtpFailureMessages.KavenegarRejected, 418));
    }

    [Fact]
    public async Task Lookup_Should_Cancel_When_The_Timeout_Elapses()
    {
        var handler = new RecordingHttpMessageHandler("{}") { WaitUntilCancelled = true };
        var client = new KavenegarVerifyLookupClient(new HttpClient(handler));

        await Should.ThrowAsync<OperationCanceledException>(() => client.LookupAsync(
            "super-secret-key",
            "09121234567",
            "654321",
            "verify",
            KavenegarVerifyLookupClient.TypeSms,
            TimeSpan.FromMilliseconds(50)));
    }

    [Fact]
    public async Task Sms_Channel_Should_Report_A_Timeout_Without_Logging_The_Api_Key_Or_Code()
    {
        var handler = new RecordingHttpMessageHandler("{}") { WaitUntilCancelled = true };
        var logger = new CapturingLogger<KavenegarSmsChannel>();
        var channel = NewHttpChannel(handler, logger, "super-secret-key", timeoutSeconds: "1");

        var result = await channel.SendOtpAsync(NewOtp(OtpPurposes.Login, "+989121234567", "654321"));

        result.Success.ShouldBeFalse();
        result.IsTransientFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldBe(OtpFailureMessages.TimedOut);
        logger.Lines.ShouldAllBe(line => !line.Contains("super-secret-key") && !line.Contains("654321"));
    }

    [Fact]
    public async Task Sms_Channel_Should_Report_A_Transport_Fault_Without_Logging_The_Api_Key_Or_Code()
    {
        var handler = new RecordingHttpMessageHandler("{}")
        {
            Fault = new HttpRequestException("GET https://api.kavenegar.com/v1/super-secret-key/verify/lookup.json failed")
        };
        var logger = new CapturingLogger<KavenegarSmsChannel>();
        var channel = NewHttpChannel(handler, logger, "super-secret-key");

        var result = await channel.SendOtpAsync(NewOtp(OtpPurposes.Login, "+989121234567", "654321"));

        result.Success.ShouldBeFalse();
        result.IsTransientFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldBe(OtpFailureMessages.RequestFailed);
        logger.Lines.ShouldNotBeEmpty();
        logger.Lines.ShouldAllBe(line => !line.Contains("super-secret-key") && !line.Contains("654321"));
    }

    [Fact]
    public async Task Sms_Channel_Should_Pass_TimeoutSeconds_To_The_Lookup()
    {
        var lookup = new FakeLookupClient();
        var channel = NewSmsChannel(lookup, withTemplate: true, timeoutSeconds: "12");

        await channel.SendOtpAsync(NewOtp(OtpPurposes.Login));

        lookup.Timeouts.ShouldHaveSingleItem().ShouldBe(TimeSpan.FromSeconds(12));
    }

    private static KavenegarSmsChannel NewHttpChannel(
        RecordingHttpMessageHandler handler,
        CapturingLogger<KavenegarSmsChannel> logger,
        string apiKey,
        string? timeoutSeconds = null)
    {
        var settings = new Dictionary<string, string>
        {
            ["ApiKey"] = apiKey,
            ["SenderNumber"] = "10004346",
            [OtpProviderSettingKeys.Template] = "verify",
            [OtpProviderSettingKeys.TemplateLogin] = "login-verify"
        };
        if (timeoutSeconds != null)
        {
            settings["TimeoutSeconds"] = timeoutSeconds;
        }

        var channel = new KavenegarSmsChannel(logger, new KavenegarVerifyLookupClient(new HttpClient(handler)));
        channel.Configure(settings);
        return channel;
    }

    private static string FormValue(string body, string key)
    {
        foreach (var pair in body.Split('&'))
        {
            var separator = pair.IndexOf('=');
            if (separator < 0)
            {
                continue;
            }

            var name = Uri.UnescapeDataString(pair.Substring(0, separator).Replace("+", " "));
            if (name == key)
            {
                return Uri.UnescapeDataString(pair.Substring(separator + 1).Replace("+", " "));
            }
        }

        throw new InvalidOperationException("Missing form field " + key);
    }

    private static KavenegarSmsChannel NewSmsChannel(FakeLookupClient lookup, bool withTemplate, string? timeoutSeconds = null)
    {
        var channel = new KavenegarSmsChannel(NullLogger<KavenegarSmsChannel>.Instance, lookup);
        var settings = new Dictionary<string, string>
        {
            ["ApiKey"] = "test-key",
            ["SenderNumber"] = "1000"
        };

        if (timeoutSeconds != null)
        {
            settings["TimeoutSeconds"] = timeoutSeconds;
        }

        if (withTemplate)
        {
            settings[OtpProviderSettingKeys.Template] = "verify";
            settings[OtpProviderSettingKeys.TemplateLogin] = "login-verify";
        }

        channel.Configure(settings);
        return channel;
    }

    private static OtpMessage NewOtp(string purpose, string phone = "+989121234567", string code = "123456") => new()
    {
        Phone = phone,
        Code = code,
        Purpose = purpose,
        Content = "Your code is " + code,
        IdempotencyKey = "Login:" + phone + ":1"
    };

    private sealed class FakeLookupClient : KavenegarVerifyLookupClient
    {
        public FakeLookupClient()
            : base(new HttpClient(new RecordingHttpMessageHandler("{}", HttpStatusCode.InternalServerError)))
        {
        }

        public List<(string Receptor, string Token, string Template, string Type)> Calls { get; } = new();

        public List<TimeSpan> Timeouts { get; } = new();

        public override Task<KavenegarLookupResult> LookupAsync(
            string apiKey,
            string receptor,
            string token,
            string template,
            string type,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            Calls.Add((receptor, token, template, type));
            Timeouts.Add(timeout);
            return Task.FromResult(new KavenegarLookupResult { ReturnStatus = 200, MessageId = 42 });
        }
    }
}
