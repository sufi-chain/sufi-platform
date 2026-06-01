using SufiChain.Chat.Messages;
using SufiChain.Chat.Participants;
using SufiChain.Chat.Sessions;
using SufiChain.SufiAbp.SettingManagement;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.Chat.Messages;

public class ChatMessageAppService_Tests : ChatApplicationTestBase<ChatApplicationTestModule>
{
    private readonly IChatMessageAppService _messageAppService;
    private readonly IChatSessionAppService _sessionAppService;

    public ChatMessageAppService_Tests()
    {
        _messageAppService = GetRequiredService<IChatMessageAppService>();
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
    }

    [Fact]
    public async Task Should_Send_Message()
    {
        var session = await CreateAuthenticatedSessionAsync();

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var message = await _messageAppService.SendAsync(new SendChatMessageInput
            {
                SessionId = session.Id,
                Body = "Hello team",
                SenderKind = ChatMessageSenderKind.Visitor,
                AccessMode = AccessMode.PublicAuthenticated
            });

            message.Body.ShouldBe("Hello team");
            message.SessionId.ShouldBe(session.Id);
        }
    }

    [Fact]
    public async Task Should_Reject_Send_On_Closed_Session()
    {
        var session = await CreateAuthenticatedSessionAsync();

        await _sessionAppService.CloseAsync(session.Id);

        var exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _messageAppService.SendAsync(new SendChatMessageInput
            {
                SessionId = session.Id,
                Body = "Too late",
                SenderKind = ChatMessageSenderKind.Visitor,
                AccessMode = AccessMode.PublicAuthenticated
            });
        });

        exception.Code.ShouldBe(ChatErrorCodes.SessionClosed);
    }

    [Fact]
    public async Task Should_Hide_Internal_Messages_From_Visitor_List()
    {
        var session = await CreateAuthenticatedSessionAsync();

        await _messageAppService.SendAsync(new SendChatMessageInput
        {
            SessionId = session.Id,
            Body = "Public note",
            SenderKind = ChatMessageSenderKind.Visitor,
            AccessMode = AccessMode.PublicAuthenticated
        });

        await _messageAppService.SendAsync(new SendChatMessageInput
        {
            SessionId = session.Id,
            Body = "Internal note",
            SenderKind = ChatMessageSenderKind.Operator,
            AccessMode = AccessMode.Internal,
            IsInternal = true
        });

        var messages = await _messageAppService.GetListAsync(new GetChatMessageListInput
        {
            SessionId = session.Id,
            IncludeInternal = false,
            MaxResultCount = 50
        });

        messages.Items.ShouldContain(message => message.Body == "Public note");
        messages.Items.ShouldNotContain(message => message.Body == "Internal note");
    }

    [Fact]
    public async Task Should_Deny_Anonymous_Message_After_Signup_Threshold()
    {
        var settingManager = GetRequiredService<ISettingManager>();
        await ChatTestSettingHelper.SetAnonymousUsagePolicyAsync(
            settingManager,
            maxMessagesBeforeSignupRequired: 1);

        var session = await _sessionAppService.CreateAsync(new CreateChatSessionInput
        {
            AccessMode = AccessMode.PublicAnonymous,
            ConversationKind = ConversationKind.Support,
            AnonymousVisitorId = ChatTestData.AnonymousVisitorId,
            Participants =
            {
                new AddChatParticipantInput
                {
                    AnonymousVisitorId = ChatTestData.AnonymousVisitorId,
                    ParticipantKind = ChatMessageSenderKind.Visitor
                }
            }
        });

        await _messageAppService.SendAsync(new SendChatMessageInput
        {
            SessionId = session.Id,
            Body = "First free message",
            SenderKind = ChatMessageSenderKind.Visitor,
            AccessMode = AccessMode.PublicAnonymous,
            AnonymousVisitorId = ChatTestData.AnonymousVisitorId
        });

        var exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _messageAppService.SendAsync(new SendChatMessageInput
            {
                SessionId = session.Id,
                Body = "Needs signup",
                SenderKind = ChatMessageSenderKind.Visitor,
                AccessMode = AccessMode.PublicAnonymous,
                AnonymousVisitorId = ChatTestData.AnonymousVisitorId
            });
        });

        exception.Code.ShouldBe("AuthenticationRequired");
    }

    private async Task<ChatSessionDto> CreateAuthenticatedSessionAsync()
    {
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            return await _sessionAppService.CreateAsync(new CreateChatSessionInput
            {
                AccessMode = AccessMode.PublicAuthenticated,
                ConversationKind = ConversationKind.Direct,
                Participants =
                {
                    new AddChatParticipantInput
                    {
                        UserId = ChatTestData.UserAId,
                        ParticipantKind = ChatMessageSenderKind.Visitor
                    },
                    new AddChatParticipantInput
                    {
                        UserId = ChatTestData.UserBId,
                        ParticipantKind = ChatMessageSenderKind.Visitor
                    }
                }
            });
        }
    }
}
