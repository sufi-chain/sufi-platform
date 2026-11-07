using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.SufiCom.Chat;
using SufiChain.SufiPlatform.SufiCom.Chat.ETOs;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Realtime;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiCom.Chat.Usage;
using SufiChain.SufiPlatform.SufiCom.Channels;
using Shouldly;
using Volo.Abp;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.EntityFrameworkCore;

/// <summary>
/// sufi-chain#551: a failed post-commit effect is finished on retry, and completed effects stay once.
/// A usage limit reached after the first charge does not reject that retry.
/// </summary>
public class AssistantTranscriptDeliveryTests : ChatApplicationTestBase<AssistantTranscriptDeliveryTestModule>
{
    [Fact]
    public async Task An_early_usage_failure_is_recovered_on_retry_without_duplicates()
    {
        var probe = GetRequiredService<TranscriptEffectProbe>();
        var operationId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1");
        var sessionId = await CreateSessionAsync();
        probe.Reset();
        probe.ThrowOnce(ChatMessageManager.EffectUsage);

        await Should.ThrowAsync<InvalidOperationException>(() => SaveAsync(sessionId, operationId));
        probe.UsageRecorded.ShouldBe(0);
        probe.Events.ShouldBe(0);
        probe.MessageNotifications.ShouldBe(0);

        var stored = await SaveAsync(sessionId, operationId);

        stored.Count.ShouldBe(1);
        probe.UsageRecorded.ShouldBe(1);
        probe.Events.ShouldBe(1);
        probe.MessageNotifications.ShouldBe(1);
        probe.SessionNotifications.ShouldBe(1);
        await AssertOneTranscriptAsync(sessionId, stored[0].Id);
    }

    [Fact]
    public async Task A_late_session_failure_is_recovered_and_a_usage_limit_does_not_block_the_retry()
    {
        var probe = GetRequiredService<TranscriptEffectProbe>();
        var operationId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee2");
        var sessionId = await CreateSessionAsync();
        probe.Reset();
        probe.ThrowOnce(ChatMessageManager.EffectSession);

        await Should.ThrowAsync<InvalidOperationException>(() => SaveAsync(sessionId, operationId));
        probe.UsageRecorded.ShouldBe(1);
        probe.Events.ShouldBe(1);
        probe.MessageNotifications.ShouldBe(1);
        probe.SessionNotifications.ShouldBe(0);

        probe.DenySend = true;
        var stored = await SaveAsync(sessionId, operationId);

        stored.Count.ShouldBe(1);
        probe.UsageRecorded.ShouldBe(1);
        probe.Events.ShouldBe(1);
        probe.MessageNotifications.ShouldBe(1);
        probe.SessionNotifications.ShouldBe(1);
        await AssertOneTranscriptAsync(sessionId, stored[0].Id);
    }

    private Task<List<ChatMessageDto>> SaveAsync(Guid sessionId, Guid operationId)
    {
        return GetRequiredService<IChatAssistantTranscriptWriter>().SaveForCurrentUserAsync(
            new SendAssistantTranscriptInput
            {
                SessionId = sessionId,
                Body = "recover me",
                OperationId = operationId
            });
    }

    private async Task<Guid> CreateSessionAsync()
    {
        var session = await GetRequiredService<IChatSessionAppService>().CreateAsync(new CreateChatSessionInput
        {
            AccessMode = AccessMode.Internal,
            ConversationKind = ConversationKind.Support
        });
        return session.Id;
    }

    private async Task AssertOneTranscriptAsync(Guid sessionId, Guid messageId)
    {
        using var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true);
        var messages = await GetRequiredService<IChatMessageRepository>().GetListBySessionAsync(
            sessionId,
            includeInternal: true,
            maxResultCount: 20);
        await uow.CompleteAsync();
        messages.Count.ShouldBe(1);
        messages[0].Id.ShouldBe(messageId);
        messages[0].Body.ShouldBe("recover me");
    }
}

[DependsOn(typeof(SufiComChatEntityFrameworkCoreTestModule))]
public class AssistantTranscriptDeliveryTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<TranscriptEffectProbe>();
        context.Services.AddTransient<ProbingChatUsageGuard>();
        context.Services.Replace(ServiceDescriptor.Transient<IChatUsageGuard, ProbingChatUsageGuard>());
        context.Services.Replace(ServiceDescriptor.Transient<IChatRealtimeNotifier, ProbingChatRealtimeNotifier>());

        var busRegistration = context.Services.Last(service => service.ServiceType == typeof(IDistributedEventBus));
        context.Services.Remove(busRegistration);
        context.Services.AddTransient<IDistributedEventBus>(provider =>
        {
            var inner = CreateInnerBus(provider, busRegistration);
            var proxy = DispatchProxy.Create<IDistributedEventBus, ProbingEventBus>();
            var probing = (ProbingEventBus)(object)proxy;
            probing.Inner = inner;
            probing.Probe = provider.GetRequiredService<TranscriptEffectProbe>();
            return proxy;
        });
    }

    private static IDistributedEventBus CreateInnerBus(IServiceProvider provider, ServiceDescriptor registration)
    {
        if (registration.ImplementationInstance is IDistributedEventBus instance)
        {
            return instance;
        }

        if (registration.ImplementationFactory != null)
        {
            return (IDistributedEventBus)registration.ImplementationFactory(provider);
        }

        return (IDistributedEventBus)ActivatorUtilities.CreateInstance(provider, registration.ImplementationType!);
    }
}

public sealed class TranscriptEffectProbe
{
    private string? _throwOnce;

    public int UsageRecorded { get; private set; }

    public int Events { get; private set; }

    public int MessageNotifications { get; private set; }

    public int SessionNotifications { get; private set; }

    public bool DenySend { get; set; }

    public void Reset()
    {
        _throwOnce = null;
        UsageRecorded = 0;
        Events = 0;
        MessageNotifications = 0;
        SessionNotifications = 0;
        DenySend = false;
    }

    public void ThrowOnce(string effect)
    {
        _throwOnce = effect;
    }

    public void ThrowIf(string effect)
    {
        if (_throwOnce != effect)
        {
            return;
        }

        _throwOnce = null;
        throw new InvalidOperationException(effect);
    }

    public void RecordUsage()
    {
        UsageRecorded++;
    }

    public void RecordEvent()
    {
        Events++;
    }

    public void RecordMessageNotification()
    {
        MessageNotifications++;
    }

    public void RecordSessionNotification()
    {
        SessionNotifications++;
    }
}

public class ProbingChatUsageGuard : IChatUsageGuard
{
    private readonly ChatUsageGuard _inner;
    private readonly TranscriptEffectProbe _probe;

    public ProbingChatUsageGuard(ChatUsageGuard inner, TranscriptEffectProbe probe)
    {
        _inner = inner;
        _probe = probe;
    }

    public Task<ChatUsageCheckResult> CheckCanStartSessionAsync(ChatStartSessionContext context, CancellationToken ct = default)
    {
        return _inner.CheckCanStartSessionAsync(context, ct);
    }

    public Task<ChatUsageCheckResult> CheckCanSendMessageAsync(Guid sessionId, ChatMessageSenderKind sender, CancellationToken ct = default)
    {
        return _inner.CheckCanSendMessageAsync(sessionId, sender, ct);
    }

    public Task<ChatUsageCheckResult> CheckCanSendMessageAsync(ChatSendMessageContext context, CancellationToken ct = default)
    {
        if (_probe.DenySend)
        {
            return Task.FromResult(ChatUsageCheckResult.Denied("UsageLimitExceeded", ChatErrorCodes.UsageLimitExceeded));
        }

        return _inner.CheckCanSendMessageAsync(context, ct);
    }

    public Task<ChatUsageCheckResult> CheckCanAttachFileAsync(
        Guid sessionId,
        int additionalAttachmentCount,
        long additionalBytes,
        AccessMode accessMode,
        CancellationToken ct = default)
    {
        return _inner.CheckCanAttachFileAsync(sessionId, additionalAttachmentCount, additionalBytes, accessMode, ct);
    }

    public async Task RecordMessageSentAsync(
        Guid sessionId,
        ChatMessageSenderKind sender,
        int attachmentCount = 0,
        long attachmentBytes = 0,
        CancellationToken ct = default)
    {
        _probe.ThrowIf(ChatMessageManager.EffectUsage);
        await _inner.RecordMessageSentAsync(sessionId, sender, attachmentCount, attachmentBytes, ct);
        _probe.RecordUsage();
    }

    public Task<ChatUsageCheckResult> CheckCanEnterAiHandoffAsync(Guid sessionId, ChatAiOperationContext context, CancellationToken ct = default)
    {
        return _inner.CheckCanEnterAiHandoffAsync(sessionId, context, ct);
    }

    public Task<ChatUsageCheckResult> CheckCanInvokeAiAsync(Guid sessionId, ChatAiOperationKind operation, CancellationToken ct = default)
    {
        return _inner.CheckCanInvokeAiAsync(sessionId, operation, ct);
    }

    public Task<Guid> ReserveAiUsageAsync(Guid sessionId, ChatAiOperationKind operation, CancellationToken ct = default)
    {
        return _inner.ReserveAiUsageAsync(sessionId, operation, ct);
    }

    public Task RecordAiUsageAsync(Guid reservationId, ChatAiUsageRecord record, CancellationToken ct = default)
    {
        return _inner.RecordAiUsageAsync(reservationId, record, ct);
    }

    public Task ReleaseAiReservationAsync(Guid reservationId, CancellationToken ct = default)
    {
        return _inner.ReleaseAiReservationAsync(reservationId, ct);
    }
}

public class ProbingChatRealtimeNotifier : IChatRealtimeNotifier
{
    private readonly TranscriptEffectProbe _probe;

    public ProbingChatRealtimeNotifier(TranscriptEffectProbe probe)
    {
        _probe = probe;
    }

    public Task NotifyMessageSentAsync(ChatMessageDto message)
    {
        _probe.ThrowIf(ChatMessageManager.EffectRealtime);
        _probe.RecordMessageNotification();
        return Task.CompletedTask;
    }

    public Task NotifySessionUpdatedAsync(ChatSessionDto session)
    {
        _probe.ThrowIf(ChatMessageManager.EffectSession);
        _probe.RecordSessionNotification();
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

public class ProbingEventBus : DispatchProxy
{
    public IDistributedEventBus Inner { get; set; } = default!;

    public TranscriptEffectProbe Probe { get; set; } = default!;

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod == null)
        {
            throw new AbpException("Event bus method was missing.");
        }

        if (targetMethod.Name == "PublishAsync" && args?.FirstOrDefault(arg => arg is ChatMessageSentEto) is ChatMessageSentEto)
        {
            Probe.ThrowIf(ChatMessageManager.EffectEvent);
            Probe.RecordEvent();
        }

        return targetMethod.Invoke(Inner, args);
    }
}
