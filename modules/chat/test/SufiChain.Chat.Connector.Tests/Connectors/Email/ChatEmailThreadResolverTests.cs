using Shouldly;
using Xunit;

namespace SufiChain.Chat.Connectors.Email;

public class ChatEmailThreadResolverTests
{
    [Fact]
    public void Should_prefer_in_reply_to_for_lookup_order()
    {
        var lookupIds = ChatEmailThreadResolver.BuildLookupIds("msg-003", "msg-001", "<msg-001> <msg-002>");

        lookupIds[0].ShouldBe("msg-001");
        lookupIds.ShouldContain("msg-003");
        lookupIds.ShouldContain("msg-002");
    }

    [Fact]
    public void Should_use_message_id_for_new_thread()
    {
        var threadId = ChatEmailThreadResolver.ResolveExternalThreadId("msg-new", null, null);

        threadId.ShouldBe("msg-new");
    }

    [Fact]
    public void Should_normalize_angle_bracket_message_ids()
    {
        ChatEmailThreadResolver.NormalizeMessageId("<abc@example.com>").ShouldBe("abc@example.com");
    }
}
