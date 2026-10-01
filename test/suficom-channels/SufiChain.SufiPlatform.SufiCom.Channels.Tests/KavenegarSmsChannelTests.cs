using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Channels.Sms.Kavenegar;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Channels;

public class KavenegarSmsChannelTests
{
    [Fact]
    public void Configure_Should_Accept_Api_Key_And_Sender()
    {
        var channel = new KavenegarSmsChannel(NullLogger<KavenegarSmsChannel>.Instance);
        Should.NotThrow(() => channel.Configure(new Dictionary<string, string>
        {
            ["ApiKey"] = "test-key",
            ["SenderNumber"] = "1000"
        }));
    }

    [Fact]
    public void Configure_Should_Reject_A_Missing_Api_Key()
    {
        var channel = new KavenegarSmsChannel(NullLogger<KavenegarSmsChannel>.Instance);
        var error = Should.Throw<AbpException>(() => channel.Configure(new Dictionary<string, string>
        {
            ["SenderNumber"] = "1000"
        }));
        error.Message.ShouldContain("ApiKey");
    }

    [Fact]
    public async Task GetDeliveryStatus_Should_Reject_A_Non_Numeric_External_Id()
    {
        var channel = new KavenegarSmsChannel(NullLogger<KavenegarSmsChannel>.Instance);
        channel.Configure(new Dictionary<string, string>
        {
            ["ApiKey"] = "test-key",
            ["SenderNumber"] = "1000"
        });

        await Should.ThrowAsync<ArgumentException>(() => channel.GetDeliveryStatusAsync("not-a-number"));
    }
}
