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
}
