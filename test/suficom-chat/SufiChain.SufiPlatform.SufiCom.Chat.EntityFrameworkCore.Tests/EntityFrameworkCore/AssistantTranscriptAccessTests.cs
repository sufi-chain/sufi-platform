using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.SufiCom.Chat.Integration;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Permissions;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiCom.Channels;
using Shouldly;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.EntityFrameworkCore;

/// <summary>
/// sufi-chain#551: storing an assistant transcript requires inbox reply or participation.
/// Messages.Send alone cannot post as the assistant into another session.
/// </summary>
public class AssistantTranscriptAccessTests : ChatApplicationTestBase<AssistantTranscriptAccessTestModule>
{
    [Fact]
    public async Task A_participant_can_store_a_transcript_in_their_own_session()
    {
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var sessionId = await CreateSessionAsync(ChatTestData.UserAId);
            var stored = await GetRequiredService<IChatAssistantTranscriptWriter>().SaveForCurrentUserAsync(
                new SendAssistantTranscriptInput
                {
                    SessionId = sessionId,
                    Body = "owned reply"
                });

            stored.Count.ShouldBe(1);
            stored[0].Body.ShouldBe("owned reply");
        }
    }

    [Fact]
    public async Task A_participant_cannot_store_a_transcript_in_another_session()
    {
        var otherSessionId = await CreateSessionAsync(ChatTestData.UserBId);
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            await CreateSessionAsync(ChatTestData.UserAId);
            await Should.ThrowAsync<AbpAuthorizationException>(() =>
                GetRequiredService<IChatAssistantTranscriptWriter>().SaveForCurrentUserAsync(
                    new SendAssistantTranscriptInput
                    {
                        SessionId = otherSessionId,
                        Body = "cross-session"
                    }));
        }

        (await ReloadAsync(otherSessionId)).ShouldBeEmpty();
    }

    [Fact]
    public async Task A_non_participant_cannot_store_a_transcript()
    {
        var sessionId = await CreateSessionAsync(ChatTestData.UserAId);
        using (CurrentUser.Change(ChatTestData.UserCId))
        {
            await Should.ThrowAsync<AbpAuthorizationException>(() =>
                GetRequiredService<IChatAssistantTranscriptWriter>().SaveForCurrentUserAsync(
                    new SendAssistantTranscriptInput
                    {
                        SessionId = sessionId,
                        Body = "not a member"
                    }));
        }

        (await ReloadAsync(sessionId)).ShouldBeEmpty();
    }

    [Fact]
    public async Task Messages_send_without_participation_cannot_store_a_transcript()
    {
        var checker = GetRequiredService<IPermissionChecker>();
        (await checker.IsGrantedAsync(ChatPermissions.Messages.Send)).ShouldBeTrue();
        (await checker.IsGrantedAsync(ChatPermissions.Inbox.Reply)).ShouldBeFalse();

        var sessionId = await CreateSessionAsync(ChatTestData.UserAId);
        using (CurrentUser.Change(ChatTestData.UserBId))
        {
            await Should.ThrowAsync<AbpAuthorizationException>(() =>
                GetRequiredService<IChatIntegrationService>().PostBotMessageAsync(new PostBotMessageInput
                {
                    SessionId = sessionId,
                    Body = "messages-send only"
                }));
        }

        (await ReloadAsync(sessionId)).ShouldBeEmpty();
    }

    private async Task<Guid> CreateSessionAsync(Guid participantUserId)
    {
        var session = await GetRequiredService<IChatSessionAppService>().CreateAsync(new CreateChatSessionInput
        {
            AccessMode = AccessMode.Internal,
            ConversationKind = ConversationKind.Support,
            Participants =
            {
                new AddChatParticipantInput
                {
                    UserId = participantUserId,
                    ParticipantKind = ChatMessageSenderKind.Visitor
                }
            }
        });
        return session.Id;
    }

    private async Task<List<ChatMessage>> ReloadAsync(Guid sessionId)
    {
        using var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true);
        var messages = await GetRequiredService<IChatMessageRepository>().GetListBySessionAsync(
            sessionId,
            includeInternal: true,
            maxResultCount: 20);
        await uow.CompleteAsync();
        return messages;
    }
}

[DependsOn(typeof(SufiComChatEntityFrameworkCoreTestModule))]
public class AssistantTranscriptAccessTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.RemoveAll<IPermissionChecker>();
        context.Services.AddSingleton<IPermissionChecker, MessagesSendOnlyPermissionChecker>();
    }
}

[DisableConventionalRegistration]
public class MessagesSendOnlyPermissionChecker : IPermissionChecker
{
    public Task<bool> IsGrantedAsync(string name)
    {
        return Task.FromResult(name == ChatPermissions.Messages.Send);
    }

    public Task<bool> IsGrantedAsync(ClaimsPrincipal? claimsPrincipal, string name)
    {
        return IsGrantedAsync(name);
    }

    public Task<MultiplePermissionGrantResult> IsGrantedAsync(string[] names)
    {
        return Task.FromResult(Grant(names));
    }

    public Task<MultiplePermissionGrantResult> IsGrantedAsync(ClaimsPrincipal? claimsPrincipal, string[] names)
    {
        return Task.FromResult(Grant(names));
    }

    private static MultiplePermissionGrantResult Grant(string[] names)
    {
        var result = new MultiplePermissionGrantResult();
        foreach (var name in names)
        {
            result.Result[name] = name == ChatPermissions.Messages.Send
                ? PermissionGrantResult.Granted
                : PermissionGrantResult.Prohibited;
        }

        return result;
    }
}
