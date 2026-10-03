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
        call.Mobile.ShouldBe("09121234567");
        call.TemplateId.ShouldBe(123456);
        call.ParameterName.ShouldBe("OTP");
        call.Value.ShouldBe("4321");
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

    private static OtpMessage NewOtp() => new()
    {
        Phone = "+989121234567",
        Code = "4321",
        Purpose = OtpPurposes.Login,
        Content = "Your code is 4321",
        IdempotencyKey = "Login:+989121234567:1"
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
            CancellationToken cancellationToken = default)
        {
            Calls.Add((mobile, templateId, parameterName, parameterValue));
            return Task.FromResult(new SmsIrResult { HttpStatusCode = 200, Status = 1, MessageId = 7 });
        }
    }
}
