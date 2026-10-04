using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Chat.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Settings;

public class ChatAiResponseWait_Tests
{
    [Fact]
    public void Invalid_or_non_positive_values_use_the_default()
    {
        ChatAiResponseWait.DefaultSeconds.ShouldBe(45);
        ChatAiResponseWait.ResolveSeconds(null).ShouldBe(45);
        ChatAiResponseWait.ResolveSeconds("").ShouldBe(45);
        ChatAiResponseWait.ResolveSeconds("nope").ShouldBe(45);
        ChatAiResponseWait.ResolveSeconds("0").ShouldBe(45);
        ChatAiResponseWait.ResolveSeconds("-5").ShouldBe(45);
    }

    [Fact]
    public void Positive_values_are_kept_and_capped()
    {
        ChatAiResponseWait.ResolveSeconds("30").ShouldBe(30);
        ChatAiResponseWait.ResolveSeconds("45").ShouldBe(45);
        ChatAiResponseWait.ResolveSeconds("99999").ShouldBe(ChatAiResponseWait.MaxSeconds);
        ChatAiResponseWait.Resolve("45").ShouldBe(TimeSpan.FromSeconds(45));
    }
}
