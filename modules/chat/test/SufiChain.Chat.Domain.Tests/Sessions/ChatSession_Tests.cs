using SufiChain.Chat.Sessions;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SufiChain.Chat.Sessions;

public class ChatSession_Tests
{
    [Fact]
    public void Should_Start_As_Open()
    {
        var session = new ChatSession(
            Guid.NewGuid(),
            null,
            "Title",
            AccessMode.PublicAuthenticated,
            ConversationKind.Direct,
            ChannelOrigin.Web);

        session.Status.ShouldBe(ChatSessionStatus.Open);
    }

    [Fact]
    public void Should_Throw_When_Sending_To_Closed_Session()
    {
        var session = new ChatSession(
            Guid.NewGuid(),
            null,
            null,
            AccessMode.PublicAuthenticated,
            ConversationKind.Direct,
            ChannelOrigin.Web);

        session.Close();

        var exception = Should.Throw<BusinessException>(() => session.EnsureOpen());
        exception.Code.ShouldBe(ChatErrorCodes.SessionClosed);
    }
}
