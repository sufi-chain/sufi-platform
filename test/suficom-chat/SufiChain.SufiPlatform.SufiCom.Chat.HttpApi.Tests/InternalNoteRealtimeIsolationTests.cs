using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Realtime;
using Volo.Abp;
using Volo.Abp.Users;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat;

/// <summary>
/// sufi-chain#548: an operator's internal note must never reach the visitor. The visitor widget joins the
/// public session group, so a note may only travel on the operator-only twin while a normal reply keeps
/// using the public group.
/// </summary>
public class InternalNoteRealtimeIsolationTests
{
    // ---- notifier routing --------------------------------------------------

    [Fact]
    public async Task An_internal_note_is_sent_only_to_the_operator_session_group()
    {
        var sessionId = Guid.NewGuid();
        var (notifier, clients, publicClient, operatorClient) = CreateNotifier(sessionId);

        await notifier.NotifyMessageSentAsync(Message(sessionId, isInternal: true));

        await operatorClient.Received(1).MessageReceived(Arg.Any<ChatMessageDto>());
        await publicClient.DidNotReceive().MessageReceived(Arg.Any<ChatMessageDto>());
        clients.Received(1).Group(ChatRealtimeGroups.OperatorSession(sessionId));
        clients.DidNotReceive().Group(ChatRealtimeGroups.Session(sessionId));
    }

    [Fact]
    public async Task A_visitor_visible_reply_keeps_using_the_public_session_group()
    {
        var sessionId = Guid.NewGuid();
        var (notifier, clients, publicClient, operatorClient) = CreateNotifier(sessionId);

        await notifier.NotifyMessageSentAsync(Message(sessionId, isInternal: false));

        await publicClient.Received(1).MessageReceived(Arg.Any<ChatMessageDto>());
        await operatorClient.DidNotReceive().MessageReceived(Arg.Any<ChatMessageDto>());
        clients.Received(1).Group(ChatRealtimeGroups.Session(sessionId));
        clients.DidNotReceive().Group(ChatRealtimeGroups.OperatorSession(sessionId));
    }

    [Fact]
    public void The_operator_session_group_is_not_the_public_session_group()
    {
        var sessionId = Guid.NewGuid();

        ChatRealtimeGroups.OperatorSession(sessionId)
            .ShouldNotBe(ChatRealtimeGroups.Session(sessionId));
    }

    // ---- hub join guard ----------------------------------------------------

    [Fact]
    public async Task An_operator_joins_the_operator_only_session_group()
    {
        var sessionId = Guid.NewGuid();
        var access = Substitute.For<IChatRealtimeAccessChecker>();
        access.CanActAsOperatorAsync().Returns(true);
        var hub = CreateHub(access);

        await hub.JoinOperatorSessionGroupAsync(sessionId);

        await hub.Groups.Received(1).AddToGroupAsync(
            "connection",
            ChatRealtimeGroups.OperatorSession(sessionId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_non_operator_cannot_join_the_operator_only_session_group()
    {
        var access = Substitute.For<IChatRealtimeAccessChecker>();
        access.CanActAsOperatorAsync().Returns(false);
        var hub = CreateHub(access);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            hub.JoinOperatorSessionGroupAsync(Guid.NewGuid()));

        exception.Code.ShouldBe(ChatClientError.HubOperatorAccessDenied);
        hub.Groups.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task An_empty_session_is_refused_before_the_operator_guard()
    {
        var access = Substitute.For<IChatRealtimeAccessChecker>();
        var hub = CreateHub(access);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            hub.JoinOperatorSessionGroupAsync(Guid.Empty));

        exception.Code.ShouldBe(ChatClientError.HubSessionAccessDenied);
        hub.Groups.ReceivedCalls().ShouldBeEmpty();
        await access.DidNotReceive().CanActAsOperatorAsync();
    }

    // ---- helpers -----------------------------------------------------------

    private static ChatMessageDto Message(Guid sessionId, bool isInternal) => new()
    {
        Id = Guid.NewGuid(),
        SessionId = sessionId,
        Body = isInternal ? "internal note" : "hello",
        IsInternal = isInternal
    };

    private static ChatHub CreateHub(IChatRealtimeAccessChecker access)
    {
        var user = Substitute.For<ICurrentUser>();
        var hub = new ChatHub(access, user)
        {
            Context = Substitute.For<HubCallerContext>(),
            Groups = Substitute.For<IGroupManager>()
        };
        hub.Context.ConnectionId.Returns("connection");
        return hub;
    }

    private static (SignalRChatRealtimeNotifier Notifier,
        IHubClients<IChatHubClient> Clients,
        IChatHubClient PublicClient,
        IChatHubClient OperatorClient) CreateNotifier(Guid sessionId)
    {
        var context = Substitute.For<IHubContext<ChatHub, IChatHubClient>>();
        var clients = Substitute.For<IHubClients<IChatHubClient>>();
        var publicClient = Substitute.For<IChatHubClient>();
        var operatorClient = Substitute.For<IChatHubClient>();

        context.Clients.Returns(clients);
        clients.Group(ChatRealtimeGroups.Session(sessionId)).Returns(publicClient);
        clients.Group(ChatRealtimeGroups.OperatorSession(sessionId)).Returns(operatorClient);

        var notifier = new SignalRChatRealtimeNotifier(
            context,
            Substitute.For<IUnitOfWorkManager>(),
            NullLogger<SignalRChatRealtimeNotifier>.Instance);

        return (notifier, clients, publicClient, operatorClient);
    }
}
