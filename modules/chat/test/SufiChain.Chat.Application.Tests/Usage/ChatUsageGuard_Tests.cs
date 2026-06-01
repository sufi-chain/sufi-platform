using SufiChain.Chat.ETOs;
using SufiChain.Chat.Usage;
using SufiChain.SufiAbp.SettingManagement;
using Shouldly;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Xunit;

namespace SufiChain.Chat.Usage;

public class ChatUsageGuard_Tests : ChatApplicationTestBase<ChatApplicationTestModule>
{
    private readonly IChatUsageGuard _usageGuard;
    private readonly ISettingManager _settingManager;
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly List<ChatUsageLimitExceededEto> _publishedEvents = new();

    public ChatUsageGuard_Tests()
    {
        _usageGuard = GetRequiredService<IChatUsageGuard>();
        _settingManager = GetRequiredService<ISettingManager>();
        _distributedEventBus = GetRequiredService<IDistributedEventBus>();
        ((IEventBus)_distributedEventBus).Subscribe<ChatUsageLimitExceededEto>(eto =>
        {
            _publishedEvents.Add(eto);
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task Should_Deny_Anonymous_Ip_Session_Cap()
    {
        await ChatTestSettingHelper.SetAnonymousUsagePolicyAsync(
            _settingManager,
            maxSessionsPerUserPerDay: 100,
            maxMessagesPerSession: 1000,
            enableIpGuard: true,
            maxSessionsPerIpPerDay: 1);

        var first = await _usageGuard.CheckCanStartSessionAsync(new ChatStartSessionContext
        {
            AccessMode = AccessMode.PublicAnonymous,
            AnonymousVisitorId = ChatTestData.AnonymousVisitorId,
            AnonymousClientIpHash = ChatTestData.AnonymousIpHash
        });
        first.IsAllowed.ShouldBeTrue();

        var denied = await _usageGuard.CheckCanStartSessionAsync(new ChatStartSessionContext
        {
            AccessMode = AccessMode.PublicAnonymous,
            AnonymousVisitorId = ChatTestData.AnonymousVisitorId2,
            AnonymousClientIpHash = ChatTestData.AnonymousIpHash
        });

        denied.IsAllowed.ShouldBeFalse();
        denied.ReasonCode.ShouldBe("AnonymousIpSessionLimitExceeded");
    }

    [Fact]
    public async Task Should_Not_Deny_Authenticated_User_By_Ip()
    {
        await ChatTestSettingHelper.SetAnonymousUsagePolicyAsync(
            _settingManager,
            enableIpGuard: true,
            maxSessionsPerIpPerDay: 1);

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var result = await _usageGuard.CheckCanStartSessionAsync(new ChatStartSessionContext
            {
                UserId = ChatTestData.UserAId,
                AccessMode = AccessMode.PublicAuthenticated,
                AnonymousClientIpHash = ChatTestData.AnonymousIpHash,
                ConversationKind = ConversationKind.Direct
            });

            result.IsAllowed.ShouldBeTrue();
        }
    }

    [Fact]
    public async Task Should_Not_Deny_Internal_User_By_Ip()
    {
        await ChatTestSettingHelper.SetAnonymousUsagePolicyAsync(
            _settingManager,
            enableIpGuard: true,
            maxSessionsPerIpPerDay: 1);

        var result = await _usageGuard.CheckCanStartSessionAsync(new ChatStartSessionContext
        {
            AccessMode = AccessMode.Internal,
            AnonymousClientIpHash = ChatTestData.AnonymousIpHash,
            ConversationKind = ConversationKind.Support
        });

        result.IsAllowed.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Deny_Message_Rate_And_Publish_Limit_Event()
    {
        await ChatTestSettingHelper.SetAnonymousUsagePolicyAsync(
            _settingManager,
            maxMessagesPerSession: 1);

        var sessionId = Guid.NewGuid();

        var allowed = await _usageGuard.CheckCanSendMessageAsync(new ChatSendMessageContext
        {
            SessionId = sessionId,
            AccessMode = AccessMode.PublicAnonymous,
            AnonymousVisitorId = ChatTestData.AnonymousVisitorId,
            SenderKind = ChatMessageSenderKind.Visitor
        });
        allowed.IsAllowed.ShouldBeTrue();

        await _usageGuard.RecordMessageSentAsync(sessionId, ChatMessageSenderKind.Visitor);

        var denied = await _usageGuard.CheckCanSendMessageAsync(new ChatSendMessageContext
        {
            SessionId = sessionId,
            AccessMode = AccessMode.PublicAnonymous,
            AnonymousVisitorId = ChatTestData.AnonymousVisitorId,
            SenderKind = ChatMessageSenderKind.Visitor
        });

        denied.IsAllowed.ShouldBeFalse();
        denied.ReasonCode.ShouldBe("SessionMessageLimitExceeded");
        _publishedEvents.ShouldContain(item => item.ReasonCode == "SessionMessageLimitExceeded");
    }

    [Fact]
    public async Task Should_Reserve_Record_And_Release_Ai_Usage()
    {
        await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager);

        var sessionId = Guid.NewGuid();
        var usageGuard = (ChatUsageGuard)_usageGuard;
        var reservationId = await usageGuard.ReserveAiUsageAsync(new ChatAiOperationContext
        {
            SessionId = sessionId,
            UserId = ChatTestData.UserAId,
            AccessMode = AccessMode.PublicAuthenticated,
            ConversationKind = ConversationKind.Assistant,
            OperationKind = ChatAiOperationKind.AutoReply,
            ProviderName = "openai",
            WorkspaceName = ChatTestData.DefaultWorkspaceName
        });

        reservationId.ShouldNotBe(Guid.Empty);

        await _usageGuard.RecordAiUsageAsync(reservationId, new ChatAiUsageRecord
        {
            PromptTokens = 10,
            CompletionTokens = 20,
            TotalTokens = 30,
            ProviderName = "openai",
            WorkspaceName = ChatTestData.DefaultWorkspaceName
        });

        var reservationRepository = GetRequiredService<IChatAiUsageReservationRepository>();
        var reservation = await reservationRepository.GetAsync(reservationId);
        reservation.TotalTokens.ShouldBe(30);
        reservation.Status.ShouldBe(ChatAiUsageReservationStatus.Recorded);

        await _usageGuard.ReleaseAiReservationAsync(reservationId);
        var released = await reservationRepository.GetAsync(reservationId);
        released.Status.ShouldBe(ChatAiUsageReservationStatus.Recorded);
    }

    [Fact]
    public async Task Should_Release_Failed_Ai_Reservation()
    {
        await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager);

        var reservationId = await _usageGuard.ReserveAiUsageAsync(Guid.NewGuid(), ChatAiOperationKind.AutoReply);
        await _usageGuard.ReleaseAiReservationAsync(reservationId);

        var reservation = await GetRequiredService<IChatAiUsageReservationRepository>().GetAsync(reservationId);
        reservation.Status.ShouldBe(ChatAiUsageReservationStatus.Released);
    }

    [Fact]
    public async Task Should_Deny_Ai_When_Disabled()
    {
        await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager, enabled: false);

        var result = await _usageGuard.CheckCanInvokeAiAsync(Guid.NewGuid(), ChatAiOperationKind.AutoReply);
        result.IsAllowed.ShouldBeFalse();
        result.ReasonCode.ShouldBe("AiUnavailable");
    }
}
