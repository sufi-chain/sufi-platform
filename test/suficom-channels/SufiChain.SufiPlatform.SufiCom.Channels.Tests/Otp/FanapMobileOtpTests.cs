using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Channels.Sms.FanapMobile;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Channels.Otp;

public class FanapMobileOtpTests
{
    [Theory]
    [InlineData("https://mysms.fanapmobile.ir:3000/PeerToPeer", "https://mysms.fanapmobile.ir")]
    [InlineData("https://sms.example.ir/", "https://sms.example.ir")]
    [InlineData(null, FanapMobileEsbClient.DefaultHost)]
    [InlineData("not a url", FanapMobileEsbClient.DefaultHost)]
    [InlineData("ftp://mysms.fanapmobile.ir", FanapMobileEsbClient.DefaultHost)]
    public void NormalizeHost_Should_Keep_Only_Scheme_And_Host(string? configured, string expected)
    {
        FanapMobileEsbClient.NormalizeHost(configured).ShouldBe(expected);
    }

    [Fact]
    public void CreateUid_Should_Be_Stable_Per_Key()
    {
        var uid = FanapMobileEsbClient.CreateUid("Login:+989121234567:abc");

        uid.Length.ShouldBe(32);
        uid.ShouldBe(FanapMobileEsbClient.CreateUid("Login:+989121234567:abc"));
        uid.ShouldNotBe(FanapMobileEsbClient.CreateUid("Login:+989121234567:abd"));
        FanapMobileEsbClient.CreateUid("").ShouldNotBe(FanapMobileEsbClient.CreateUid(""));
    }

    [Fact]
    public async Task SendOtp_Should_Post_To_The_Otp_Port_With_Basic_Auth()
    {
        var handler = new RecordingHttpMessageHandler("""{"status":0,"smsid":"sms-1"}""");
        var client = new FanapMobileEsbClient(new HttpClient(handler));

        var result = await client.SendOtpAsync(
            FanapMobileEsbClient.DefaultHost,
            "auth-string",
            "98200",
            "+989121234567",
            "Your code is 123456",
            "uid-1",
            TimeSpan.FromSeconds(10));

        result.Success.ShouldBeTrue();
        result.SmsId.ShouldBe("sms-1");
        var (request, body) = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.RequestUri!.ToString().ShouldBe("https://mysms.fanapmobile.ir:3002/Otp");
        request.Headers.GetValues("Authorization").Single().ShouldBe("Basic auth-string");
        request.Content!.Headers.ContentType!.MediaType.ShouldBe("application/json");

        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("sender").GetString().ShouldBe("98200");
        json.RootElement.GetProperty("recipient").GetString().ShouldBe("+989121234567");
        json.RootElement.GetProperty("message").GetString().ShouldBe("Your code is 123456");
        json.RootElement.GetProperty("uid").GetString().ShouldBe("uid-1");
    }

    [Fact]
    public void Parse_Should_Read_Delivery_Reports()
    {
        var result = FanapMobileEsbClient.Parse(
            """{"status":0,"dlrs":{"sms-1":{"dlr":1},"sms-2":{"dlr":2}}}""",
            200);

        result.Success.ShouldBeTrue();
        result.Deliveries["sms-1"].ShouldBe(1);
        result.Deliveries["sms-2"].ShouldBe(2);
    }

    [Theory]
    [InlineData("""{"status":5}""", 200, true)]
    [InlineData("""{"status":2}""", 200, false)]
    [InlineData("<html></html>", 502, true)]
    public void Parse_Should_Mark_Gateway_Recovery_As_Transient(string body, int httpStatus, bool transient)
    {
        var result = FanapMobileEsbClient.Parse(body, httpStatus);

        result.Success.ShouldBeFalse();
        result.IsTransientFailure.ShouldBe(transient);
    }

    [Theory]
    [InlineData(1, DeliveryState.Delivered)]
    [InlineData(2, DeliveryState.Failed)]
    [InlineData(16, DeliveryState.Failed)]
    [InlineData(0, DeliveryState.Sent)]
    [InlineData(4, DeliveryState.Sent)]
    public void MapDeliveryState_Should_Follow_The_Fanap_Dlr_Codes(int dlr, DeliveryState expected)
    {
        FanapMobileSmsChannel.MapDeliveryState(dlr).ShouldBe(expected);
    }

    [Fact]
    public async Task Channel_Should_Send_Otp_From_The_Otp_Line_With_A_Stable_Uid()
    {
        var client = new FakeEsbClient();
        var channel = new FanapMobileSmsChannel(NullLogger<FanapMobileSmsChannel>.Instance, client);
        channel.Configure(new Dictionary<string, string>
        {
            ["ApiKey"] = "auth-string",
            ["SenderNumber"] = "98100",
            [OtpProviderSettingKeys.SenderNumber] = "98200",
            ["BaseUrl"] = "https://mysms.fanapmobile.ir:3000/PeerToPeer"
        });

        var message = new OtpMessage
        {
            Phone = "09121234567",
            Code = "123456",
            Purpose = OtpPurposes.Login,
            Content = "Your code is 123456",
            IdempotencyKey = "Login:09121234567:abc"
        };

        var result = await channel.SendOtpAsync(message);

        channel.GetOtpConfigurationError().ShouldBeNull();
        result.Success.ShouldBeTrue();
        var call = client.OtpCalls.ShouldHaveSingleItem();
        call.Host.ShouldBe("https://mysms.fanapmobile.ir");
        call.Sender.ShouldBe("98200");
        call.Recipient.ShouldBe("+989121234567");
        call.Uid.ShouldBe(FanapMobileEsbClient.CreateUid("Login:09121234567:abc"));
    }

    [Fact]
    public async Task Channel_Should_Use_The_Main_Line_When_No_Otp_Line_Is_Set()
    {
        var client = new FakeEsbClient();
        var channel = new FanapMobileSmsChannel(NullLogger<FanapMobileSmsChannel>.Instance, client);
        channel.Configure(new Dictionary<string, string>
        {
            ["ApiKey"] = "auth-string",
            ["SenderNumber"] = "98100"
        });

        await channel.SendOtpAsync(new OtpMessage
        {
            Phone = "+989121234567",
            Code = "123456",
            Purpose = OtpPurposes.Login,
            Content = "Your code is 123456",
            IdempotencyKey = "k"
        });

        client.OtpCalls.ShouldHaveSingleItem().Sender.ShouldBe("98100");
    }

    [Theory]
    [InlineData("09121234567")]
    [InlineData("+989121234567")]
    [InlineData("00989121234567")]
    [InlineData("۰۰۹۸۹۱۲۱۲۳۴۵۶۷")]
    public async Task Channel_Should_Post_The_International_Number_With_Persian_Text(string phone)
    {
        var handler = new RecordingHttpMessageHandler("""{"status":0,"smsid":123456789}""");
        var logger = new CapturingLogger<FanapMobileSmsChannel>();
        var channel = NewHttpChannel(handler, logger, "super-secret-key");

        var result = await channel.SendOtpAsync(NewOtp(phone, "کد شما ۶۵۴۳۲۱"));

        result.Success.ShouldBeTrue();
        result.ExternalId.ShouldBe("123456789");
        var (request, body) = handler.Requests.ShouldHaveSingleItem();
        request.RequestUri!.ToString().ShouldBe("https://mysms.fanapmobile.ir:3002/Otp");
        request.Headers.GetValues("Authorization").Single().ShouldBe("Basic super-secret-key");
        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("recipient").GetString().ShouldBe("+989121234567");
        json.RootElement.GetProperty("message").GetString().ShouldBe("کد شما ۶۵۴۳۲۱");
        body.ShouldContain("کد شما");
        body.ShouldNotContain("\\u");
        logger.Lines.ShouldAllBe(line => !line.Contains("super-secret-key") && !line.Contains("۶۵۴۳۲۱"));
    }

    [Fact]
    public void Parse_Should_Read_A_Numeric_Sms_Id()
    {
        var result = FanapMobileEsbClient.Parse("""{"status":0,"smsid":123456789}""", 200);

        result.Success.ShouldBeTrue();
        result.SmsId.ShouldBe("123456789");
    }

    [Fact]
    public async Task Channel_Should_Report_A_Provider_Status_As_A_Localization_Key()
    {
        var handler = new RecordingHttpMessageHandler("""{"status":26}""");
        var channel = NewHttpChannel(handler, new CapturingLogger<FanapMobileSmsChannel>(), "super-secret-key");

        var result = await channel.SendOtpAsync(NewOtp("09121234567", "654321"));

        result.Success.ShouldBeFalse();
        result.IsTransientFailure.ShouldBeFalse();
        result.StatusCode.ShouldBe(26);
        result.ErrorMessage.ShouldBe(OtpFailureMessages.Format(OtpFailureMessages.FanapRejected, 26));
    }

    [Fact]
    public async Task Channel_Should_Not_Double_The_Basic_Prefix()
    {
        var handler = new RecordingHttpMessageHandler("""{"status":0,"smsid":"sms-1"}""");
        var channel = NewHttpChannel(handler, new CapturingLogger<FanapMobileSmsChannel>(), "Basic super-secret-key");

        await channel.SendOtpAsync(NewOtp("09121234567", "654321"));

        handler.Requests.ShouldHaveSingleItem().Request.Headers.GetValues("Authorization").Single()
            .ShouldBe("Basic super-secret-key");
    }

    [Theory]
    [InlineData("panel-token")]
    [InlineData("Basic panel-token")]
    [InlineData("basic panel-token")]
    [InlineData("Basic  panel-token")]
    [InlineData("Basic Basic panel-token")]
    public async Task SendOtp_Should_Send_A_Single_Basic_Prefix(string configured)
    {
        var handler = new RecordingHttpMessageHandler("""{"status":0,"smsid":"sms-1"}""");
        var client = new FanapMobileEsbClient(new HttpClient(handler));

        await client.SendOtpAsync(
            FanapMobileEsbClient.DefaultHost,
            configured,
            "98200",
            "+989121234567",
            "code",
            "uid-1",
            TimeSpan.FromSeconds(10));

        handler.Requests.ShouldHaveSingleItem().Request.Headers.GetValues("Authorization").Single()
            .ShouldBe("Basic panel-token");
    }

    [Theory]
    [InlineData("user:pass")]
    [InlineData("Basic user:pass")]
    public async Task SendOtp_Should_Base64_Encode_Username_And_Password_Once(string configured)
    {
        var encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("user:pass"));
        var handler = new RecordingHttpMessageHandler("""{"status":0,"smsid":"sms-1"}""");
        var client = new FanapMobileEsbClient(new HttpClient(handler));

        await client.SendOtpAsync(
            FanapMobileEsbClient.DefaultHost,
            configured,
            "98200",
            "+989121234567",
            "code",
            "uid-1",
            TimeSpan.FromSeconds(10));

        handler.Requests.ShouldHaveSingleItem().Request.Headers.GetValues("Authorization").Single()
            .ShouldBe("Basic " + encoded);
    }

    [Fact]
    public async Task SendOtp_Should_Not_Encode_A_Panel_Token_That_Is_Already_Base64()
    {
        var encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("user:pass"));
        var handler = new RecordingHttpMessageHandler("""{"status":0,"smsid":"sms-1"}""");
        var client = new FanapMobileEsbClient(new HttpClient(handler));

        await client.SendOtpAsync(
            FanapMobileEsbClient.DefaultHost,
            "Basic " + encoded,
            "98200",
            "+989121234567",
            "code",
            "uid-1",
            TimeSpan.FromSeconds(10));

        handler.Requests.ShouldHaveSingleItem().Request.Headers.GetValues("Authorization").Single()
            .ShouldBe("Basic " + encoded);
    }

    [Fact]
    public void Parse_Should_Read_Status_And_SmsId_Ignoring_Case()
    {
        var result = FanapMobileEsbClient.Parse("""{"Status":0,"SmsId":"sms-9"}""", 200);

        result.Success.ShouldBeTrue();
        result.SmsId.ShouldBe("sms-9");
    }

    [Fact]
    public async Task Channel_Should_Fold_A_Formatted_Otp_Sender()
    {
        var handler = new RecordingHttpMessageHandler("""{"status":0,"smsid":"sms-1"}""");
        var channel = new FanapMobileSmsChannel(
            NullLogger<FanapMobileSmsChannel>.Instance,
            new FanapMobileEsbClient(new HttpClient(handler)));
        channel.Configure(new Dictionary<string, string>
        {
            ["ApiKey"] = "panel-token",
            ["SenderNumber"] = "+98 100",
            [OtpProviderSettingKeys.SenderNumber] = "۹۸۲۰۰"
        });

        await channel.SendOtpAsync(NewOtp("09121234567", "654321"));

        using var json = JsonDocument.Parse(handler.Requests.ShouldHaveSingleItem().Body);
        json.RootElement.GetProperty("sender").GetString().ShouldBe("98200");
        json.RootElement.GetProperty("recipient").GetString().ShouldBe("+989121234567");
    }

    [Fact]
    public async Task Channel_Should_Log_The_Http_Status_And_Body_Without_Secrets()
    {
        var handler = new RecordingHttpMessageHandler(
            """{"status":26,"detail":"rejected 654321 token super-secret-key"}""",
            HttpStatusCode.Forbidden);
        var logger = new CapturingLogger<FanapMobileSmsChannel>();
        var channel = NewHttpChannel(handler, logger, "super-secret-key");

        var result = await channel.SendOtpAsync(NewOtp("09121234567", "654321"));

        result.Success.ShouldBeFalse();
        result.StatusCode.ShouldBe(26);
        var line = logger.Lines.ShouldHaveSingleItem();
        line.ShouldContain("HttpStatus=403");
        line.ShouldContain("ProviderStatus=26");
        line.ShouldContain("rejected");
        line.ShouldNotContain("super-secret-key");
        line.ShouldNotContain("654321");
    }

    [Fact]
    public async Task SendOtp_Should_Cancel_When_The_Timeout_Elapses()
    {
        var handler = new RecordingHttpMessageHandler("{}") { WaitUntilCancelled = true };
        var client = new FanapMobileEsbClient(new HttpClient(handler));

        await Should.ThrowAsync<OperationCanceledException>(() => client.SendOtpAsync(
            FanapMobileEsbClient.DefaultHost,
            "super-secret-key",
            "98200",
            "+989121234567",
            "کد شما ۶۵۴۳۲۱",
            "uid-1",
            TimeSpan.FromMilliseconds(50)));
    }

    [Fact]
    public async Task Channel_Should_Report_A_Timeout_Without_Logging_The_Token_Or_Code()
    {
        var handler = new RecordingHttpMessageHandler("{}") { WaitUntilCancelled = true };
        var logger = new CapturingLogger<FanapMobileSmsChannel>();
        var channel = NewHttpChannel(handler, logger, "super-secret-key", timeoutSeconds: "1");

        var result = await channel.SendOtpAsync(NewOtp("09121234567", "654321"));

        result.Success.ShouldBeFalse();
        result.IsTransientFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldBe(OtpFailureMessages.TimedOut);
        logger.Lines.ShouldAllBe(line => !line.Contains("super-secret-key") && !line.Contains("654321"));
    }

    private static FanapMobileSmsChannel NewHttpChannel(
        RecordingHttpMessageHandler handler,
        CapturingLogger<FanapMobileSmsChannel> logger,
        string apiKey,
        string? timeoutSeconds = null)
    {
        var channel = new FanapMobileSmsChannel(logger, new FanapMobileEsbClient(new HttpClient(handler)));
        var settings = new Dictionary<string, string>
        {
            ["ApiKey"] = apiKey,
            ["SenderNumber"] = "98100",
            [OtpProviderSettingKeys.SenderNumber] = "98200"
        };
        if (timeoutSeconds != null)
        {
            settings["TimeoutSeconds"] = timeoutSeconds;
        }

        channel.Configure(settings);
        return channel;
    }

    private static OtpMessage NewOtp(string phone, string content) => new()
    {
        Phone = phone,
        Code = "654321",
        Purpose = OtpPurposes.Login,
        Content = content,
        IdempotencyKey = "Login:" + phone + ":abc"
    };

    private sealed class FakeEsbClient : FanapMobileEsbClient
    {
        public FakeEsbClient()
            : base(new HttpClient(new RecordingHttpMessageHandler("{}", HttpStatusCode.InternalServerError)))
        {
        }

        public List<(string Host, string Sender, string Recipient, string Uid)> OtpCalls { get; } = new();

        public override Task<FanapMobileResult> SendOtpAsync(
            string host,
            string authString,
            string sender,
            string recipient,
            string message,
            string uid,
            TimeSpan timeout)
        {
            OtpCalls.Add((host, sender, recipient, uid));
            return Task.FromResult(new FanapMobileResult { HttpStatusCode = 200, Status = 0, SmsId = "sms-1" });
        }
    }
}
