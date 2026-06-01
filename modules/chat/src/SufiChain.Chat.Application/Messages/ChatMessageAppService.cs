using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Connectors;
using SufiChain.Chat.Attachments;
using SufiChain.Chat.ETOs;
using SufiChain.Chat.Mapping;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Realtime;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Authorization.Permissions;

namespace SufiChain.Chat.Messages;

[Authorize(ChatPermissions.Messages.Default)]
public class ChatMessageAppService : ChatAppService, IChatMessageAppService
{
    protected IChatMessageRepository MessageRepository { get; }
    protected IChatSessionRepository SessionRepository { get; }
    protected ChatMessageManager MessageManager { get; }
    protected IChatUsageGuard UsageGuard { get; }
    protected IChatAttachmentValidator AttachmentValidator { get; }
    protected ChatApplicationMapper Mapper { get; }
    protected IDistributedEventBus DistributedEventBus { get; }
    protected IChatRealtimeNotifier RealtimeNotifier { get; }
    protected IPermissionChecker PermissionChecker { get; }
    protected ChatOutboundMessageDispatcher OutboundMessageDispatcher { get; }

    public ChatMessageAppService(
        IChatMessageRepository messageRepository,
        IChatSessionRepository sessionRepository,
        ChatMessageManager messageManager,
        IChatUsageGuard usageGuard,
        IChatAttachmentValidator attachmentValidator,
        ChatApplicationMapper mapper,
        IDistributedEventBus distributedEventBus,
        IChatRealtimeNotifier realtimeNotifier,
        IPermissionChecker permissionChecker,
        ChatOutboundMessageDispatcher outboundMessageDispatcher)
    {
        MessageRepository = messageRepository;
        SessionRepository = sessionRepository;
        MessageManager = messageManager;
        UsageGuard = usageGuard;
        AttachmentValidator = attachmentValidator;
        Mapper = mapper;
        DistributedEventBus = distributedEventBus;
        RealtimeNotifier = realtimeNotifier;
        PermissionChecker = permissionChecker;
        OutboundMessageDispatcher = outboundMessageDispatcher;
    }

    [Authorize(ChatPermissions.Messages.Send)]
    public virtual async Task<ChatMessageDto> SendAsync(SendChatMessageInput input)
    {
        if (input.IsInternal)
        {
            await CheckPolicyAsync(ChatPermissions.Messages.SendInternal);
        }

        var session = await SessionRepository.GetAsync(input.SessionId);

        var usageResult = await UsageGuard.CheckCanSendMessageAsync(new ChatSendMessageContext
        {
            TenantId = CurrentTenant.Id,
            SessionId = input.SessionId,
            UserId = input.SenderUserId ?? CurrentUser.Id,
            AnonymousVisitorId = input.AnonymousVisitorId,
            AnonymousClientIpHash = input.AnonymousClientIpHash,
            AccessMode = input.AccessMode,
            SenderKind = input.SenderKind
        });

        await EnsureUsageAllowedAsync(input.SessionId, usageResult);

        var attachmentValidation = await ValidateAttachmentsAsync(input.SessionId, input.AttachmentFileIds);

        if (attachmentValidation.Count > 0)
        {
            var attachmentResult = await UsageGuard.CheckCanAttachFileAsync(
                input.SessionId,
                attachmentValidation.Count,
                attachmentValidation.TotalBytes,
                input.AccessMode);
            await EnsureUsageAllowedAsync(input.SessionId, attachmentResult);
        }

        var message = await MessageManager.SendAsync(
            session,
            input.Body,
            input.SenderKind,
            input.SenderUserId ?? CurrentUser.Id,
            input.AnonymousVisitorId,
            input.IsInternal,
            await PermissionChecker.IsGrantedAsync(ChatPermissions.Inbox.Reply),
            input.MetadataJson,
            input.AttachmentFileIds);

        await UsageGuard.RecordMessageSentAsync(
            message.SessionId,
            message.SenderKind,
            attachmentValidation.Count,
            attachmentValidation.TotalBytes);
        await OutboundMessageDispatcher.TryDispatchAsync(session, message);
        await PublishMessageSentAsync(message);
        var messageDto = Mapper.ToDto(message);
        await RealtimeNotifier.NotifyMessageSentAsync(messageDto);
        await RealtimeNotifier.NotifySessionUpdatedAsync(await MapSessionAsync(session));

        return messageDto;
    }

    public virtual async Task<PagedResultDto<ChatMessageDto>> GetListAsync(GetChatMessageListInput input)
    {
        var includeInternal = input.IncludeInternal && await PermissionChecker.IsGrantedAsync(ChatPermissions.Messages.ViewInternal);
        var totalCount = await MessageRepository.GetCountBySessionAsync(input.SessionId, includeInternal);
        var messages = await MessageRepository.GetListBySessionAsync(
            input.SessionId,
            includeInternal,
            input.SkipCount,
            input.MaxResultCount);

        return new PagedResultDto<ChatMessageDto>(
            totalCount,
            messages.Select(Mapper.ToDto).ToList());
    }

    [Authorize(ChatPermissions.Messages.Delete)]
    public virtual async Task DeleteAsync(Guid messageId)
    {
        await MessageRepository.DeleteAsync(messageId);
    }

    protected virtual Task<ChatAttachmentValidationResult> ValidateAttachmentsAsync(
        Guid sessionId,
        IReadOnlyList<Guid> attachmentFileIds)
    {
        return AttachmentValidator.ValidateAsync(sessionId, attachmentFileIds);
    }

    protected virtual async Task EnsureUsageAllowedAsync(Guid sessionId, ChatUsageCheckResult result)
    {
        if (!result.IsAllowed)
        {
            await RealtimeNotifier.NotifyUsageLimitExceededAsync(sessionId, Mapper.ToDto(result));
            throw new BusinessException(result.ReasonCode ?? ChatErrorCodes.UsageLimitExceeded);
        }
    }

    protected virtual async Task<ChatSessionDto> MapSessionAsync(ChatSession session)
    {
        return await Task.FromResult(Mapper.ToDto(session));
    }

    protected virtual async Task PublishMessageSentAsync(ChatMessage message)
    {
        await DistributedEventBus.PublishAsync(new ChatMessageSentEto
        {
            Id = message.Id,
            TenantId = message.TenantId,
            SessionId = message.SessionId,
            SenderKind = message.SenderKind,
            SenderUserId = message.SenderUserId,
            AnonymousVisitorId = message.AnonymousVisitorId,
            IsInternal = message.IsInternal,
            SentAt = message.CreationTime
        });
    }
}
