using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Messages;

/// <summary>sufi-chain#551: a reply longer than the 16000-character body column is stored in order as several messages.</summary>
public class ChatMessageBodySplitter_Tests
{
    [Fact]
    public void A_body_within_the_limit_stays_one_part()
    {
        var body = new string('a', ChatConsts.MaxMessageBodyLength);
        ChatMessageBodySplitter.Split(body).ShouldBe(new[] { body });
    }

    [Fact]
    public void A_longer_body_splits_without_losing_characters()
    {
        var body = new string('a', 8000) + " " + new string('b', 9000) + "\n" + new string('c', 2000);
        var parts = ChatMessageBodySplitter.Split(body);

        parts.Count.ShouldBeGreaterThan(1);
        parts.ShouldAllBe(part => part.Length <= ChatConsts.MaxMessageBodyLength && part.Length > 0);
        string.Concat(parts).ShouldBe(body);
    }

    [Fact]
    public void A_surrogate_pair_is_not_split()
    {
        var pair = "\uD83D\uDE00";
        var body = new string('a', ChatConsts.MaxMessageBodyLength - 1) + pair;
        var parts = ChatMessageBodySplitter.Split(body);

        string.Concat(parts).ShouldBe(body);
        parts.ShouldAllBe(part => part.Length <= ChatConsts.MaxMessageBodyLength);
        foreach (var part in parts)
        {
            (part.Length == 0 || !char.IsHighSurrogate(part[part.Length - 1])).ShouldBeTrue();
        }
    }
}
