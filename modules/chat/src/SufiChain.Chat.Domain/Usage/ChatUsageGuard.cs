using System;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.Chat.ETOs;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Users;

namespace SufiChain.Chat.Usage;

public class ChatUsageGuard : DomainService, IChatUsageGuard
{
    protected IChatUsagePolicyResolver PolicyResolver { get; }
    protected IChatUsageWalletResolver WalletResolver { get; }
    protected IChatRateLimitCounterStore RateLimitCounterStore { get; }
    protected IChatUsageCounterRepository UsageCounterRepository { get; }
    protected IChatAiUsageReservationRepository AiUsageReservationRepository { get; }
    protected IDistributedEventBus DistributedEventBus { get; }
    protected ICurrentUser CurrentUserAccessor { get; }

    public ChatUsageGuard(
        IChatUsagePolicyResolver policyResolver,
        IChatUsageWalletResolver walletResolver,
        IChatRateLimitCounterStore rateLimitCounterStore,
        IChatUsageCounterRepository usageCounterRepository,
        IChatAiUsageReservationRepository aiUsageReservationRepository,
        IDistributedEventBus distributedEventBus,
        ICurrentUser currentUser)
    {
        PolicyResolver = policyResolver;
        WalletResolver = walletResolver;
        RateLimitCounterStore = rateLimitCounterStore;
        UsageCounterRepository = usageCounterRepository;
        AiUsageReservationRepository = aiUsageReservationRepository;
        DistributedEventBus = distributedEventBus;
        CurrentUserAccessor = currentUser;
    }

    public virtual async Task<ChatUsageCheckResult> CheckCanStartSessionAsync(
        ChatStartSessionContext context,
        CancellationToken ct = default)
    {
        var policy = await PolicyResolver.ResolveAsync(context.AccessMode, ct);

        if (context.AccessMode == AccessMode.PublicAnonymous)
        {
            if (string.IsNullOrWhiteSpace(context.AnonymousVisitorId))
            {
                return await DenyAsync(context.TenantId, null, null, "AnonymousIdentityRequired", "Chat:AnonymousIdentityRequired", LimitExceededAction.RequireAuthentication, ct);
            }

            var visitorCount = await RateLimitCounterStore.IncrementAsync(
                BuildRateKey(context.TenantId, "AnonSession", context.AnonymousVisitorId),
                TimeSpan.FromDays(1),
                ct);

            if (policy.MaxSessionsPerUserPerDay > 0 && visitorCount > policy.MaxSessionsPerUserPerDay)
            {
                return await DenyAsync(context.TenantId, null, null, "AnonymousSessionLimitExceeded", "Chat:AnonymousSessionLimitExceeded", policy.LimitExceededAction, ct);
            }

            if (policy.EnableAnonymousIpGuard && !string.IsNullOrWhiteSpace(context.AnonymousClientIpHash))
            {
                var ipCount = await RateLimitCounterStore.IncrementAsync(
                    BuildRateKey(context.TenantId, "AnonIpSession", context.AnonymousClientIpHash),
                    TimeSpan.FromDays(1),
                    ct);

                if (policy.MaxSessionsPerIpPerDay > 0 && ipCount > policy.MaxSessionsPerIpPerDay)
                {
                    return await DenyAsync(context.TenantId, null, null, "AnonymousIpSessionLimitExceeded", "Chat:AnonymousIpSessionLimitExceeded", LimitExceededAction.BlockSend, ct);
                }
            }
        }

        return ChatUsageCheckResult.Allowed();
    }

    public virtual async Task<ChatUsageCheckResult> CheckCanSendMessageAsync(
        Guid sessionId,
        ChatMessageSenderKind sender,
        CancellationToken ct = default)
    {
        return await CheckCanSendMessageAsync(
            new ChatSendMessageContext
            {
                TenantId = CurrentTenant.Id,
                SessionId = sessionId,
                UserId = CurrentUserAccessor.Id,
                AccessMode = AccessMode.PublicAuthenticated,
                SenderKind = sender
            },
            ct);
    }

    public virtual async Task<ChatUsageCheckResult> CheckCanSendMessageAsync(
        ChatSendMessageContext context,
        CancellationToken ct = default)
    {
        var sessionCount = await UsageCounterRepository.GetCountAsync(
            context.TenantId,
            BuildSessionCounterKey(context.SessionId, "Messages"),
            ChatUsageCounterPeriod.Session,
            DateTime.MinValue,
            ct);

        var policy = await PolicyResolver.ResolveAsync(context.AccessMode, ct);
        if (policy.MaxMessagesPerSession > 0 && sessionCount >= policy.MaxMessagesPerSession)
        {
            return await DenyAsync(context.TenantId, context.SessionId, context.UserId, "SessionMessageLimitExceeded", "Chat:SessionMessageLimitExceeded", policy.LimitExceededAction, ct);
        }

        if (context.AccessMode == AccessMode.PublicAnonymous)
        {
            var visitorKey = context.AnonymousVisitorId ?? context.AnonymousClientIpHash;
            if (!string.IsNullOrWhiteSpace(visitorKey) && policy.MaxMessagesBeforeSignupRequired > 0)
            {
                var visitorMessageCount = await RateLimitCounterStore.IncrementAsync(
                    BuildRateKey(context.TenantId, "AnonMessage", visitorKey),
                    TimeSpan.FromDays(1),
                    ct);

                if (visitorMessageCount > policy.MaxMessagesBeforeSignupRequired)
                {
                    return await DenyAsync(context.TenantId, context.SessionId, context.UserId, "AuthenticationRequired", "Chat:AuthenticationRequired", LimitExceededAction.RequireAuthentication, ct);
                }
            }

            if (policy.EnableAnonymousIpGuard && !string.IsNullOrWhiteSpace(context.AnonymousClientIpHash))
            {
                var ipMessageCount = await RateLimitCounterStore.IncrementAsync(
                    BuildRateKey(context.TenantId, "AnonIpMessage", context.AnonymousClientIpHash),
                    TimeSpan.FromDays(1),
                    ct);

                if (policy.MaxMessagesPerIpPerDay > 0 && ipMessageCount > policy.MaxMessagesPerIpPerDay)
                {
                    return await DenyAsync(context.TenantId, context.SessionId, context.UserId, "AnonymousIpMessageLimitExceeded", "Chat:AnonymousIpMessageLimitExceeded", LimitExceededAction.BlockSend, ct);
                }
            }
        }

        return ChatUsageCheckResult.Allowed();
    }

    public virtual async Task<ChatUsageCheckResult> CheckCanAttachFileAsync(
        Guid sessionId,
        int additionalAttachmentCount,
        long additionalBytes,
        AccessMode accessMode,
        CancellationToken ct = default)
    {
        var policy = await PolicyResolver.ResolveAsync(accessMode, ct);

        var currentAttachmentCount = await UsageCounterRepository.GetCountAsync(
            CurrentTenant.Id,
            BuildSessionCounterKey(sessionId, "Attachments"),
            ChatUsageCounterPeriod.Session,
            DateTime.MinValue,
            ct);

        if (policy.MaxAttachmentsPerSession > 0 &&
            currentAttachmentCount + additionalAttachmentCount > policy.MaxAttachmentsPerSession)
        {
            return await DenyAsync(
                CurrentTenant.Id,
                sessionId,
                CurrentUserAccessor.Id,
                "AttachmentCountLimitExceeded",
                "Chat:AttachmentCountLimitExceeded",
                policy.LimitExceededAction,
                ct);
        }

        var currentAttachmentBytes = await UsageCounterRepository.GetCountAsync(
            CurrentTenant.Id,
            BuildSessionCounterKey(sessionId, "AttachmentBytes"),
            ChatUsageCounterPeriod.Session,
            DateTime.MinValue,
            ct);

        if (policy.MaxAttachmentBytesPerSession > 0 &&
            currentAttachmentBytes + additionalBytes > policy.MaxAttachmentBytesPerSession)
        {
            return await DenyAsync(
                CurrentTenant.Id,
                sessionId,
                CurrentUserAccessor.Id,
                "AttachmentLimitExceeded",
                "Chat:AttachmentLimitExceeded",
                policy.LimitExceededAction,
                ct);
        }

        return ChatUsageCheckResult.Allowed();
    }

    public virtual async Task RecordMessageSentAsync(
        Guid sessionId,
        ChatMessageSenderKind sender,
        int attachmentCount = 0,
        long attachmentBytes = 0,
        CancellationToken ct = default)
    {
        await UsageCounterRepository.IncrementAsync(
            CurrentTenant.Id,
            BuildSessionCounterKey(sessionId, "Messages"),
            ChatUsageCounterPeriod.Session,
            DateTime.MinValue,
            DateTime.MaxValue,
            cancellationToken: ct);

        var dayStart = Clock.Now.Date;
        await UsageCounterRepository.IncrementAsync(
            CurrentTenant.Id,
            "Tenant:Messages",
            ChatUsageCounterPeriod.Day,
            dayStart,
            dayStart.AddDays(1),
            cancellationToken: ct);

        if (attachmentCount > 0)
        {
            await UsageCounterRepository.IncrementAsync(
                CurrentTenant.Id,
                BuildSessionCounterKey(sessionId, "Attachments"),
                ChatUsageCounterPeriod.Session,
                DateTime.MinValue,
                DateTime.MaxValue,
                count: attachmentCount,
                cancellationToken: ct);
        }

        if (attachmentBytes > 0)
        {
            await UsageCounterRepository.IncrementAsync(
                CurrentTenant.Id,
                BuildSessionCounterKey(sessionId, "AttachmentBytes"),
                ChatUsageCounterPeriod.Session,
                DateTime.MinValue,
                DateTime.MaxValue,
                count: attachmentBytes,
                cancellationToken: ct);
        }
    }

    public virtual async Task<ChatUsageCheckResult> CheckCanEnterAiHandoffAsync(
        Guid sessionId,
        ChatAiOperationContext context,
        CancellationToken ct = default)
    {
        var aiPolicy = await PolicyResolver.ResolveAiAsync(ct);
        if (!aiPolicy.Enabled)
        {
            return await DenyAsync(context.TenantId, sessionId, context.UserId, "AiUnavailable", "Chat:AiUnavailable", LimitExceededAction.BlockSend, ct, context.OperationKind);
        }

        if (context.AccessMode == AccessMode.PublicAnonymous)
        {
            var usagePolicy = await PolicyResolver.ResolveAsync(AccessMode.PublicAnonymous, ct);
            if (usagePolicy.MaxAiQuestionsBeforeSignupRequired > 0)
            {
                var visitorKey = context.AnonymousVisitorId ?? context.AnonymousClientIpHash;
                if (!string.IsNullOrWhiteSpace(visitorKey))
                {
                    var visitorAiCount = await RateLimitCounterStore.IncrementAsync(
                        BuildRateKey(context.TenantId, "AnonAiQuestion", visitorKey),
                        TimeSpan.FromDays(1),
                        ct);

                    if (visitorAiCount > usagePolicy.MaxAiQuestionsBeforeSignupRequired)
                    {
                        return await DenyAsync(context.TenantId, sessionId, context.UserId, "AuthenticationRequired", "Chat:AuthenticationRequired", LimitExceededAction.RequireAuthentication, ct, context.OperationKind);
                    }
                }
            }

            if (usagePolicy.EnableAnonymousIpGuard && !string.IsNullOrWhiteSpace(context.AnonymousClientIpHash))
            {
                var ipAiCount = await RateLimitCounterStore.IncrementAsync(
                    BuildRateKey(context.TenantId, "AnonIpAiSession", context.AnonymousClientIpHash),
                    TimeSpan.FromHours(1),
                    ct);

                if (usagePolicy.MaxAiSessionsPerIpPerHour > 0 && ipAiCount > usagePolicy.MaxAiSessionsPerIpPerHour)
                {
                    return await DenyAsync(context.TenantId, sessionId, context.UserId, "AnonymousIpAiLimitExceeded", "Chat:AnonymousIpAiLimitExceeded", LimitExceededAction.BlockSend, ct, context.OperationKind);
                }
            }
        }

        return ChatUsageCheckResult.Allowed();
    }

    public virtual async Task<ChatUsageCheckResult> CheckCanInvokeAiAsync(
        Guid sessionId,
        ChatAiOperationKind operation,
        CancellationToken ct = default)
    {
        var policy = await PolicyResolver.ResolveAiAsync(ct);
        if (!policy.Enabled)
        {
            return await DenyAsync(CurrentTenant.Id, sessionId, CurrentUserAccessor.Id, "AiUnavailable", "Chat:AiUnavailable", LimitExceededAction.BlockSend, ct, operation);
        }

        var replyCount = await AiUsageReservationRepository.GetSessionAiReplyCountAsync(sessionId, ct);
        if (policy.MaxRepliesPerSession > 0 && replyCount >= policy.MaxRepliesPerSession)
        {
            return await DenyAsync(CurrentTenant.Id, sessionId, CurrentUserAccessor.Id, "AiReplyLimitExceeded", "Chat:AiReplyLimitExceeded", LimitExceededAction.BlockSend, ct, operation);
        }

        return ChatUsageCheckResult.Allowed();
    }

    public virtual async Task<Guid> ReserveAiUsageAsync(
        Guid sessionId,
        ChatAiOperationKind operation,
        CancellationToken ct = default)
    {
        var context = new ChatAiOperationContext
        {
            TenantId = CurrentTenant.Id,
            SessionId = sessionId,
            UserId = CurrentUserAccessor.Id,
            AccessMode = AccessMode.PublicAuthenticated,
            ConversationKind = ConversationKind.Assistant,
            OperationKind = operation
        };

        return await ReserveAiUsageAsync(context, ct);
    }

    public virtual async Task<Guid> ReserveAiUsageAsync(
        ChatAiOperationContext context,
        CancellationToken ct = default)
    {
        var walletContext = await WalletResolver.ResolveAsync(context, ct);
        var reservation = new ChatAiUsageReservation(
            GuidGenerator.Create(),
            context,
            walletContext,
            Clock.Now);

        await AiUsageReservationRepository.InsertAsync(reservation, autoSave: true, cancellationToken: ct);
        return reservation.Id;
    }

    public virtual async Task RecordAiUsageAsync(
        Guid reservationId,
        ChatAiUsageRecord record,
        CancellationToken ct = default)
    {
        var reservation = await AiUsageReservationRepository.GetAsync(reservationId, cancellationToken: ct);
        reservation.Record(record, Clock.Now);
        await AiUsageReservationRepository.UpdateAsync(reservation, autoSave: true, cancellationToken: ct);

        var dayStart = Clock.Now.Date;
        await UsageCounterRepository.IncrementAsync(
            reservation.TenantId,
            "Tenant:AiTokens",
            ChatUsageCounterPeriod.Day,
            dayStart,
            dayStart.AddDays(1),
            tokenCount: record.TotalTokens,
            cancellationToken: ct);
    }

    public virtual async Task ReleaseAiReservationAsync(Guid reservationId, CancellationToken ct = default)
    {
        var reservation = await AiUsageReservationRepository.GetAsync(reservationId, cancellationToken: ct);
        reservation.Release();
        await AiUsageReservationRepository.UpdateAsync(reservation, autoSave: true, cancellationToken: ct);
    }

    protected virtual async Task<ChatUsageCheckResult> DenyAsync(
        Guid? tenantId,
        Guid? sessionId,
        Guid? userId,
        string reasonCode,
        string localizationKey,
        LimitExceededAction action,
        CancellationToken cancellationToken,
        ChatAiOperationKind? operationKind = null)
    {
        await DistributedEventBus.PublishAsync(
            new ChatUsageLimitExceededEto
            {
                TenantId = tenantId,
                SessionId = sessionId,
                UserId = userId,
                AiOperationKind = operationKind,
                ReasonCode = reasonCode,
                LocalizationKey = localizationKey,
                Action = action,
                OccurredAt = Clock.Now
            });

        return ChatUsageCheckResult.Denied(
            reasonCode,
            localizationKey,
            action,
            action == LimitExceededAction.RequireAuthentication);
    }

    protected virtual string BuildRateKey(Guid? tenantId, string scope, string key)
    {
        return $"{tenantId?.ToString("D") ?? "host"}:{scope}:{key}";
    }

    protected virtual string BuildSessionCounterKey(Guid sessionId, string scope)
    {
        return $"Session:{sessionId:D}:{scope}";
    }
}
