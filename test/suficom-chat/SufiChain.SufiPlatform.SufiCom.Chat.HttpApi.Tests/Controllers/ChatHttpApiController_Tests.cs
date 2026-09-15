using SufiChain.SufiPlatform.SufiCom.Chat.Controllers;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Participants;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.HttpApi;

public class ChatSessionController_Tests : ChatApplicationTestBase<SufiComChatHttpApiTestModule>
{
    [Fact]
    public async Task Should_Create_Session_Through_HttpApi_Controller()
    {
        var controller = GetRequiredService<ChatSessionController>();

        var session = await controller.CreateAsync(new CreateChatSessionInput
        {
            AccessMode = AccessMode.Internal,
            ConversationKind = ConversationKind.Support,
            Participants =
            {
                new AddChatParticipantInput
                {
                    AnonymousVisitorId = ChatTestData.AnonymousVisitorId,
                    ParticipantKind = ChatMessageSenderKind.Visitor
                }
            }
        });

        session.Id.ShouldNotBe(Guid.Empty);
    }
}

public class ChatMessageController_Tests : ChatApplicationTestBase<SufiComChatHttpApiTestModule>
{
    [Fact]
    public async Task Should_Send_Message_Through_HttpApi_Controller()
    {
        var sessionController = GetRequiredService<ChatSessionController>();
        var messageController = GetRequiredService<ChatMessageController>();

        var session = await sessionController.CreateAsync(new CreateChatSessionInput
        {
            AccessMode = AccessMode.Internal,
            ConversationKind = ConversationKind.Support,
            Participants =
            {
                new AddChatParticipantInput
                {
                    AnonymousVisitorId = ChatTestData.AnonymousVisitorId,
                    ParticipantKind = ChatMessageSenderKind.Visitor
                }
            }
        });

        var message = await messageController.SendAsync(new SendChatMessageInput
        {
            SessionId = session.Id,
            Body = "HttpApi message",
            SenderKind = ChatMessageSenderKind.Visitor,
            AccessMode = AccessMode.Internal,
            AnonymousVisitorId = ChatTestData.AnonymousVisitorId
        });

        message.Body.ShouldBe("HttpApi message");
    }
}
