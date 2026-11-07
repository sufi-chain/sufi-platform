using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Realtime;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiCom.Chat.Usage;
using SufiChain.SufiPlatform.SufiCom.Channels;
using Shouldly;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.EntityFrameworkCore;

/// <summary>
/// sufi-chain#551: a delivery failure after the transcript commits does not duplicate it on retry.
/// </summary>
public class AssistantTranscriptDeliveryTests : ChatApplicationTestBase<AssistantTranscriptDeliveryTestModule>
{
    [Fact]
    public async Task A_post_commit_delivery_failure_then_retry_stores_one_transcript()
    {
        var session = await GetRequiredService<IChatSessionAppService>().CreateAsync(new CreateChatSessionInput
        {
            AccessMode = AccessMode.Internal,
            ConversationKind = ConversationKind.Support
        });
        var body = new string('a', ChatConsts.MaxMessageBodyLength) + "tail";
        var writer = GetRequiredService<IChatAssistantTranscriptWriter>();
        var notifier = GetRequiredService<IChatRealtimeNotifier>().ShouldBeOfType<ThrowOnTranscriptNotifyChatRealtimeNotifier>();

        var first = await writer.SaveForCurrentUserAsync(new SendAssistantTranscriptInput
        {
            SessionId = session.Id,
            Body = body
        });
        var callsAfterFirst = notifier.MessageNotifications;
        callsAfterFirst.ShouldBeGreaterThan(0);

        var second = await writer.SaveForCurrentUserAsync(new SendAssistantTranscriptInput
        {
            SessionId = session.Id,
            Body = body
        });

        notifier.MessageNotifications.ShouldBe(callsAfterFirst);
        second.Select(message => message.Id).ShouldBe(first.Select(message => message.Id));

        using var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true);
        var messages = await GetRequiredService<IChatMessageRepository>().GetListBySessionAsync(
            session.Id,
            includeInternal: true,
            maxResultCount: 20);
        await uow.CompleteAsync();

        messages.Count.ShouldBe(first.Count);
        string.Concat(messages.Select(message => message.Body)).ShouldBe(body);
    }
}

[DependsOn(typeof(SufiComChatEntityFrameworkCoreTestModule))]
public class AssistantTranscriptDeliveryTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.RemoveAll<IChatRealtimeNotifier>();
        context.Services.AddSingleton<ThrowOnTranscriptNotifyChatRealtimeNotifier>();
        context.Services.AddSingleton<IChatRealtimeNotifier>(services =>
            services.GetRequiredService<ThrowOnTranscriptNotifyChatRealtimeNotifier>());
    }
}

public class ThrowOnTranscriptNotifyChatRealtimeNotifier : IChatRealtimeNotifier
{
    public int MessageNotifications { get; private set; }

    public Task NotifyMessageSentAsync(ChatMessageDto message)
    {
        MessageNotifications++;
        throw new InvalidOperationException("post-commit");
    }

    public Task NotifySessionUpdatedAsync(ChatSessionDto session)
    {
        return Task.CompletedTask;
    }

    public Task NotifyUsageLimitExceededAsync(Guid sessionId, ChatUsageCheckResultDto result)
    {
        return Task.CompletedTask;
    }

    public Task NotifyAssistantProgressAsync(ChatAssistantProgressDto progress)
    {
        return Task.CompletedTask;
    }

    public Task NotifyHooshvareTurnProgressAsync(Guid userId, ChatAssistantProgressDto progress)
    {
        return Task.CompletedTask;
    }

    public Task NotifyOperatorQueueChangedAsync()
    {
        return Task.CompletedTask;
    }
}
