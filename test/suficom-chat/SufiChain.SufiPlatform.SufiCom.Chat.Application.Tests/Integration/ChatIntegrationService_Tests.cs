using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using Shouldly;
using Volo.Abp.Authorization;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Integration;

public class ChatIntegrationService_Tests : ChatApplicationTestBase<SufiComChatApplicationTestModule>
{
    private readonly IChatIntegrationService _integrationService;

    public ChatIntegrationService_Tests()
    {
        _integrationService = GetRequiredService<IChatIntegrationService>();
    }

    [Fact]
    public async Task Should_Create_Filter_And_Resume_Owned_Playground_Session()
    {
        var assistantId = Guid.NewGuid();

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var session = await _integrationService.CreateOwnedPlaygroundSessionAsync(
                new CreateOwnedPlaygroundSessionInput
                {
                    Title = "Hooshvare playground",
                    AssistantId = assistantId
                });

            session.CreatorId.ShouldBe(ChatTestData.UserAId);
            session.AccessMode.ShouldBe(AccessMode.Internal);
            session.ConversationKind.ShouldBe(ConversationKind.Assistant);
            session.AssistantId.ShouldBe(assistantId);
            session.IsExternallyOrchestrated.ShouldBeTrue();
            session.Origin.ShouldBe(ChatSessionOrigin.HooshvarePlayground);
            session.Participants.ShouldContain(participant =>
                participant.UserId == ChatTestData.UserAId &&
                participant.ParticipantKind == ChatMessageSenderKind.Operator);

            var sessions = await _integrationService.GetMySessionsAsync(new GetMyChatSessionsInput
            {
                AssistantId = assistantId,
                Origin = ChatSessionOrigin.HooshvarePlayground,
                ExternalOrchestration = true,
                MaxResultCount = 10
            });

            sessions.TotalCount.ShouldBe(1);
            sessions.Items.Single().Id.ShouldBe(session.Id);

            var resumed = await _integrationService.GetOwnedSessionAsync(session.Id);
            resumed.Id.ShouldBe(session.Id);
            resumed.AssistantId.ShouldBe(assistantId);
        }
    }

    [Fact]
    public async Task Should_Reject_Cross_User_Playground_Access()
    {
        Guid sessionId;
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            sessionId = (await _integrationService.CreateOwnedPlaygroundSessionAsync(
                new CreateOwnedPlaygroundSessionInput
                {
                    AssistantId = Guid.NewGuid()
                })).Id;
        }

        using (CurrentUser.Change(ChatTestData.UserBId))
        {
            await Should.ThrowAsync<AbpAuthorizationException>(
                () => _integrationService.GetOwnedSessionAsync(sessionId));
            await Should.ThrowAsync<AbpAuthorizationException>(
                () => _integrationService.GetOwnedMessagesAsync(new GetChatMessageListInput
                {
                    SessionId = sessionId
                }));
            await Should.ThrowAsync<AbpAuthorizationException>(
                () => _integrationService.PostOwnedUserMessageAsync(new PostOwnedMessageInput
                {
                    SessionId = sessionId,
                    Body = "Not allowed"
                }));
        }
    }

    [Fact]
    public async Task Should_Persist_Exactly_Requested_Playground_Messages()
    {
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var session = await _integrationService.CreateOwnedPlaygroundSessionAsync(
                new CreateOwnedPlaygroundSessionInput
                {
                    AssistantId = Guid.NewGuid()
                });

            await _integrationService.PostOwnedUserMessageAsync(new PostOwnedMessageInput
            {
                SessionId = session.Id,
                Body = "Preview request"
            });
            await _integrationService.PostOwnedAssistantMessageAsync(new PostOwnedMessageInput
            {
                SessionId = session.Id,
                Body = "Preview response"
            });

            var messages = await _integrationService.GetOwnedMessagesAsync(new GetChatMessageListInput
            {
                SessionId = session.Id,
                MaxResultCount = 10
            });

            await _integrationService.UpdateOwnedSessionTitleAsync(
                session.Id,
                new UpdateChatSessionTitleInput
                {
                    Title = "Preview request — Preview response"
                });

            var retitled = await _integrationService.GetOwnedSessionAsync(session.Id);
            retitled.Title.ShouldBe("Preview request — Preview response");

            messages.TotalCount.ShouldBe(2);
            messages.Items.Count(message => message.SenderKind == ChatMessageSenderKind.Operator)
                .ShouldBe(1);
            messages.Items.Count(message => message.SenderKind == ChatMessageSenderKind.Assistant)
                .ShouldBe(1);
        }
    }
}
