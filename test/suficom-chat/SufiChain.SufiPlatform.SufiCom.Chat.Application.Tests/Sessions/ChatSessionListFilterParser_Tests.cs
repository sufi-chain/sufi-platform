using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Sessions;

public class ChatSessionListFilterParser_Tests
{
    [Fact]
    public void Named_filters_parse_and_numeric_text_does_not_select_the_default_member()
    {
        ChatSessionListFilterParser.TryParseDefined<ConversationKind>("Assistant", out var kind).ShouldBeTrue();
        kind.ShouldBe(ConversationKind.Assistant);

        ChatSessionListFilterParser.TryParseDefined<ConversationKind>("0", out _).ShouldBeFalse();
        ChatSessionListFilterParser.TryParseDefined<ChatSessionStatus>("1", out _).ShouldBeFalse();
        ChatSessionListFilterParser.TryParseDefined<AccessMode>(" ", out _).ShouldBeFalse();
        ChatSessionListFilterParser.TryParseDefined<AccessMode>(null, out _).ShouldBeFalse();
    }
}
