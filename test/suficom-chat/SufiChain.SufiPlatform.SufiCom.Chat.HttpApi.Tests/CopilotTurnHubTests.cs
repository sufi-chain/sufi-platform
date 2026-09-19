using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Chat.Realtime;
using Volo.Abp.Users;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat;

public class CopilotTurnHubTests
{
    [Fact]
    public async Task Joining_and_publishing_use_the_same_authenticated_owner_group()
    {
        var user = Substitute.For<ICurrentUser>();
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var turnId = Guid.NewGuid();
        user.IsAuthenticated.Returns(true);
        user.Id.Returns(userId);
        user.TenantId.Returns(tenantId);
        var hub = new ChatHub(Substitute.For<IChatRealtimeAccessChecker>(), user)
        {
            Context = Substitute.For<HubCallerContext>(),
            Groups = Substitute.For<IGroupManager>()
        };
        hub.Context.ConnectionId.Returns("connection");
        var group = ChatRealtimeGroups.CopilotTurn(turnId, userId, tenantId);
        await hub.JoinCopilotTurnGroupAsync(turnId);
        await hub.Groups.Received(1).AddToGroupAsync("connection", group, Arg.Any<CancellationToken>());

        var context = Substitute.For<IHubContext<ChatHub, IChatHubClient>>();
        var clients = Substitute.For<IHubClients<IChatHubClient>>();
        var client = Substitute.For<IChatHubClient>();
        context.Clients.Returns(clients);
        clients.Group(group).Returns(client);
        var notifier = new SignalRChatRealtimeNotifier(context, Substitute.For<IUnitOfWorkManager>(),
            NullLogger<SignalRChatRealtimeNotifier>.Instance);
        var progress = new ChatAssistantProgressDto { TurnId = turnId, TenantId = tenantId };
        await notifier.NotifyCopilotTurnProgressAsync(userId, progress);
        await client.Received(1).AssistantProgress(progress);
        await hub.LeaveCopilotTurnGroupAsync(turnId);
        await hub.Groups.Received(1).RemoveFromGroupAsync("connection", group, Arg.Any<CancellationToken>());

        ChatRealtimeGroups.CopilotTurn(turnId, Guid.NewGuid(), tenantId).ShouldNotBe(group);
        ChatRealtimeGroups.CopilotTurn(turnId, userId, Guid.NewGuid()).ShouldNotBe(group);
        ChatRealtimeGroups.CopilotTurn(turnId, userId, null).ShouldNotBe(group);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task Anonymous_callers_and_empty_turns_are_rejected(bool authenticated, bool emptyTurn)
    {
        var user = Substitute.For<ICurrentUser>();
        user.IsAuthenticated.Returns(authenticated);
        user.Id.Returns(authenticated ? Guid.NewGuid() : (Guid?)null);
        var hub = new ChatHub(Substitute.For<IChatRealtimeAccessChecker>(), user)
        {
            Context = Substitute.For<HubCallerContext>(),
            Groups = Substitute.For<IGroupManager>()
        };
        await Should.ThrowAsync<HubException>(() => hub.JoinCopilotTurnGroupAsync(emptyTurn ? Guid.Empty : Guid.NewGuid()));
        hub.Groups.ReceivedCalls().ShouldBeEmpty();
    }
}
