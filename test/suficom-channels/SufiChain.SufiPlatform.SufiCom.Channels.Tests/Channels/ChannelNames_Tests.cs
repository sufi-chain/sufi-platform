using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Channels;

public class ChannelNames_Tests
{
    [Fact]
    public void TelegramUser_Should_Be_Distinct_From_Telegram()
    {
        ChannelNames.TelegramUser.ShouldBe("TelegramUser");
        ChannelNames.Telegram.ShouldBe("Telegram");
        ChannelNames.TelegramUser.ShouldNotBe(ChannelNames.Telegram);
    }

    [Fact]
    public void Oss_Sms_And_Voice_Names_Are_Stable()
    {
        ChannelNames.SmsKavenegar.ShouldBe("Sms.Kavenegar");
        ChannelNames.SmsIdehPardazan.ShouldBe("Sms.IdehPardazan");
        ChannelNames.VoiceKavenegar.ShouldBe("Voice.Kavenegar");
        // Commercial OTP channel name must not leak into OSS ChannelNames.
        typeof(ChannelNames).GetField("Sms" + "Fa" + "nap" + "Mobile").ShouldBeNull();
    }
}
