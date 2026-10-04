using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Channels.Sms.IdehPardazan;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Channels.Otp;

public class SmsIrOtpTests
{
    [Fact]
    public async Task Verify_Should_Post_Template_And_Code_With_The_Api_Key_Header()
    {
        var handler = new RecordingHttpMessageHandler(
            """{"status":1,"message":"موفق","data":{"messageId":89545112,"cost":1.0}}""");
        var client = new SmsIrRestClient(new HttpClient(handler));

        var result = await client.VerifySendAsync("test-key", "09121234567", 123456, "Code", "4321");

        result.Success.ShouldBeTrue();
        result.MessageId.ShouldBe(89545112);
        var (request, body) = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.RequestUri!.ToString().ShouldBe("https://api.sms.ir/v1/send/verify");
        request.Headers.GetValues("x-api-key").Single().ShouldBe("test-key");
        string.Join(",", request.Headers.GetValues("Accept")).ShouldContain("text/plain");
        request.Content!.Headers.ContentType!.CharSet.ShouldBe("utf-8");

        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("mobile").GetString().ShouldBe("09121234567");
        json.RootElement.GetProperty("templateId").GetInt32().ShouldBe(123456);
        var parameter = json.RootElement.GetProperty("parameters")[0];
        parameter.GetProperty("name").GetString().ShouldBe("Code");
        parameter.GetProperty("value").GetString().ShouldBe("4321");
    }

    [Theory]
    [InlineData(429, true)]
    [InlineData(500, true)]
    [InlineData(400, false)]
    [InlineData(401, false)]
    public void Parse_Should_Mark_Only_Retryable_Statuses_As_Transient(int httpStatus, bool transient)
    {
        var result = SmsIrRestClient.Parse("""{"status":0,"message":"error","data":null}""", httpStatus);

        result.Success.ShouldBeFalse();
        result.IsTransientFailure.ShouldBe(transient);
    }

    [Fact]
    public void Parse_Should_Read_The_Credit_Response()
    {
        var result = SmsIrRestClient.Parse("""{"status":1,"message":"موفق","data":100.5}""", 200);

        result.Success.ShouldBeTrue();
        result.Credit.ShouldBe(100.5m);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("verify")]
    public void Channel_Should_Require_A_Numeric_Template_Id(string? template)
    {
        var channel = NewChannel(new FakeRestClient(), template);

        channel.GetOtpConfigurationError().ShouldBe(OtpProviderSettingKeys.TemplateRequiredError);
    }

    [Fact]
    public async Task Channel_Should_Not_Call_The_Gateway_Without_A_Template()
    {
        var restClient = new FakeRestClient();
        var channel = NewChannel(restClient, template: null);

        var result = await channel.SendOtpAsync(NewOtp());

        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldBe(OtpProviderSettingKeys.TemplateRequiredError);
        restClient.Calls.ShouldBeEmpty();
    }

    [Fact]
    public async Task Channel_Should_Send_The_Code_Through_The_Verify_Route()
    {
        var restClient = new FakeRestClient();
        var channel = NewChannel(restClient, "123456", codeParameterName: "#OTP#");

        var result = await channel.SendOtpAsync(NewOtp());

        channel.GetOtpConfigurationError().ShouldBeNull();
        result.Success.ShouldBeTrue();
        result.ExternalId.ShouldBe("7");
        var call = restClient.Calls.ShouldHaveSingleItem();
        call.Mobile.ShouldBe("9121234567");
        call.TemplateId.ShouldBe(123456);
        call.ParameterName.ShouldBe("OTP");
        call.Value.ShouldBe("4321");
    }

    [Theory]
    [InlineData("09121234567")]
    [InlineData("+989121234567")]
    [InlineData("۰۹۱۲۱۲۳۴۵۶۷")]
    public async Task Channel_Should_Post_The_National_Number_On_The_Verify_Route(string phone)
    {
        var handler = new RecordingHttpMessageHandler(
            """{"status":1,"message":"موفق","data":{"messageId":89545112,"cost":1.0}}""");
        var logger = new CapturingLogger<IdehPardazanSmsChannel>();
        var channel = NewHttpChannel(handler, logger);

        var result = await channel.SendOtpAsync(NewOtp(phone, "654321"));

        result.Success.ShouldBeTrue();
        result.ExternalId.ShouldBe("89545112");
        var (request, body) = handler.Requests.ShouldHaveSingleItem();
        request.RequestUri!.ToString().ShouldBe("https://api.sms.ir/v1/send/verify");
        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("mobile").GetString().ShouldBe("9121234567");
        json.RootElement.GetProperty("parameters")[0].GetProperty("value").GetString().ShouldBe("654321");
        logger.Lines.ShouldAllBe(line => !line.Contains("super-secret-key") && !line.Contains("654321"));
    }

    [Fact]
    public async Task Verify_Should_Keep_Persian_Parameter_Text_As_Utf8()
    {
        var handler = new RecordingHttpMessageHandler(
            """{"status":1,"message":"موفق","data":{"messageId":1,"cost":1}}""");
        var client = new SmsIrRestClient(new HttpClient(handler));

        await client.VerifySendAsync("test-key", "9121234567", 10, "Name", "علی");

        handler.Requests.ShouldHaveSingleItem().Body.ShouldContain("علی");
        handler.Requests.Single().Body.ShouldNotContain("\\u");
    }

    [Fact]
    public async Task Channel_Should_Report_A_Provider_Error_As_A_Localization_Key()
    {
        var handler = new RecordingHttpMessageHandler(
            """{"status":0,"message":"ناموفق","data":null}""",
            HttpStatusCode.BadRequest);
        var channel = NewHttpChannel(handler, new CapturingLogger<IdehPardazanSmsChannel>());

        var result = await channel.SendOtpAsync(NewOtp());

        result.Success.ShouldBeFalse();
        result.IsTransientFailure.ShouldBeFalse();
        result.ErrorMessage.ShouldBe(OtpFailureMessages.Format(OtpFailureMessages.SmsIrRejected, 400, 0));
    }

    [Fact]
    public async Task Channel_Should_Reject_A_Non_Iranian_Number_Without_Calling_The_Gateway()
    {
        var handler = new RecordingHttpMessageHandler("{}");
        var channel = NewHttpChannel(handler, new CapturingLogger<IdehPardazanSmsChannel>());

        var result = await channel.SendOtpAsync(NewOtp("+14155550100"));

        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldBe(OtpFailureMessages.InvalidPhone);
        handler.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task Verify_Should_Cancel_When_The_Timeout_Elapses()
    {
        var handler = new RecordingHttpMessageHandler("{}") { WaitUntilCancelled = true };
        var client = new SmsIrRestClient(new HttpClient(handler));

        await Should.ThrowAsync<OperationCanceledException>(() => client.VerifySendAsync(
            "super-secret-key",
            "9121234567",
            10,
            "Code",
            "654321",
            TimeSpan.FromMilliseconds(50)));
    }

    [Fact]
    public async Task Channel_Should_Report_A_Timeout_Without_Logging_The_Api_Key_Or_Code()
    {
        var handler = new RecordingHttpMessageHandler("{}") { WaitUntilCancelled = true };
        var logger = new CapturingLogger<IdehPardazanSmsChannel>();
        var channel = NewHttpChannel(handler, logger, timeoutSeconds: "1");

        var result = await channel.SendOtpAsync(NewOtp(code: "654321"));

        result.Success.ShouldBeFalse();
        result.IsTransientFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldBe(OtpFailureMessages.TimedOut);
        logger.Lines.ShouldAllBe(line => !line.Contains("super-secret-key") && !line.Contains("654321"));
    }

    [Fact]
    public async Task Channel_Should_Report_A_Transport_Fault_Without_Logging_The_Api_Key_Or_Code()
    {
        var handler = new RecordingHttpMessageHandler("{}")
        {
            Fault = new HttpRequestException("SMS.ir failed for key super-secret-key")
        };
        var logger = new CapturingLogger<IdehPardazanSmsChannel>();
        var channel = NewHttpChannel(handler, logger);

        var result = await channel.SendOtpAsync(NewOtp(code: "654321"));

        result.Success.ShouldBeFalse();
        result.IsTransientFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldBe(OtpFailureMessages.RequestFailed);
        logger.Lines.ShouldAllBe(line => !line.Contains("super-secret-key") && !line.Contains("654321"));
    }

    private static IdehPardazanSmsChannel NewHttpChannel(
        RecordingHttpMessageHandler handler,
        CapturingLogger<IdehPardazanSmsChannel> logger,
        string? timeoutSeconds = null)
    {
        var settings = new Dictionary<string, string>
        {
            ["ApiKey"] = "super-secret-key",
            ["SenderNumber"] = "30001234",
            [OtpProviderSettingKeys.Template] = "123456",
            [OtpProviderSettingKeys.CodeParameterName] = "Code"
        };
        if (timeoutSeconds != null)
        {
            settings["TimeoutSeconds"] = timeoutSeconds;
        }

        var channel = new IdehPardazanSmsChannel(logger, new SmsIrRestClient(new HttpClient(handler)));
        channel.Configure(settings);
        return channel;
    }

    private static IdehPardazanSmsChannel NewChannel(
        FakeRestClient restClient,
        string? template,
        string? codeParameterName = null)
    {
        var channel = new IdehPardazanSmsChannel(NullLogger<IdehPardazanSmsChannel>.Instance, restClient);
        var settings = new Dictionary<string, string>
        {
            ["ApiKey"] = "test-key",
            ["SenderNumber"] = "30001234"
        };

        if (template != null)
        {
            settings[OtpProviderSettingKeys.Template] = template;
        }

        if (codeParameterName != null)
        {
            settings[OtpProviderSettingKeys.CodeParameterName] = codeParameterName;
        }

        channel.Configure(settings);
        return channel;
    }

    private static OtpMessage NewOtp(string phone = "+989121234567", string code = "4321") => new()
    {
        Phone = phone,
        Code = code,
        Purpose = OtpPurposes.Login,
        Content = "کد شما " + code,
        IdempotencyKey = "Login:" + phone + ":1"
    };

    private sealed class FakeRestClient : SmsIrRestClient
    {
        public FakeRestClient()
            : base(new HttpClient(new RecordingHttpMessageHandler("{}", HttpStatusCode.InternalServerError)))
        {
        }

        public List<(string Mobile, int TemplateId, string ParameterName, string Value)> Calls { get; } = new();

        public override Task<SmsIrResult> VerifySendAsync(
            string apiKey,
            string mobile,
            int templateId,
            string parameterName,
            string parameterValue,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            Calls.Add((mobile, templateId, parameterName, parameterValue));
            return Task.FromResult(new SmsIrResult { HttpStatusCode = 200, Status = 1, MessageId = 7 });
        }
    }
}
