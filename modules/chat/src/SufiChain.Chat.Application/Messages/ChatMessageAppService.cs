using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Connectors;
using SufiChain.Chat.Attachments;
using SufiChain.Chat.ETOs;
using SufiChain.Chat.Mapping;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Realtime;
using SufiChain.Chat.Participants;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp;
using Volo.Abp.EventBus.Distributed;
using SufiChain.Chat.Features;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Settings;
using Volo.Abp.Authorization.Permissions;
using SufiChain.SufiAbp.Features;
using Volo.Abp.Settings;

namespace SufiChain.Chat.Messages;

[Authorize]
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
    protected IChatParticipantRepository ParticipantRepository { get; }
    protected IChatSessionDtoEnricher SessionDtoEnricher { get; }

    protected IFeatureChecker FeatureChecker { get; }

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
        ChatOutboundMessageDispatcher outboundMessageDispatcher,
        IChatParticipantRepository participantRepository,
        IChatSessionDtoEnricher sessionDtoEnricher,
        IFeatureChecker featureChecker)
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
        ParticipantRepository = participantRepository;
        SessionDtoEnricher = sessionDtoEnricher;
        FeatureChecker = featureChecker;
    }

    public virtual async Task<ChatMessageDto> SendAsync(SendChatMessageInput input)
    {
        await EnsureCanSendMessagesAsync();

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

        await ValidateSendContentAsync(input);

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
        await EnsureCanReadMessagesAsync();

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

    protected virtual async Task ValidateSendContentAsync(SendChatMessageInput input)
    {
        var attachmentIds = input.AttachmentFileIds ?? new List<Guid>();
        var metadata = ChatMessageMetadata.TryParse(input.MetadataJson);
        var hasLocation = metadata?.ContentKind == ChatMessageContentKind.Location;
        var hasVoice = metadata?.ContentKind == ChatMessageContentKind.Voice;
        var hasBody = !string.IsNullOrWhiteSpace(input.Body);

        if (!hasBody && attachmentIds.Count == 0 && !hasLocation)
        {
            throw new BusinessException(ChatErrorCodes.MessageContentRequired);
        }

        if (!string.IsNullOrWhiteSpace(input.Body) && input.Body.Length > ChatConsts.MaxMessageBodyLength)
        {
            throw new BusinessException(ChatErrorCodes.MessageContentRequired);
        }

        if (attachmentIds.Count > 0 || hasVoice)
        {
            if (!await FeatureChecker.IsEnabledAsync(ChatFeatures.Attachments) ||
                !await SettingProvider.IsTrueAsync(ChatSettingNames.General.EnableFileAttachments))
            {
                throw new BusinessException(ChatErrorCodes.AttachmentsDisabled);
            }

            var maxFiles = await SettingProvider.GetAsync<int>(ChatSettingNames.Attachments.MaxFilesPerMessage);
            if (maxFiles <= 0)
            {
                maxFiles = ChatSettingDefaults.MaxFilesPerMessage;
            }

            if (attachmentIds.Count > maxFiles)
            {
                throw new BusinessException(ChatErrorCodes.MaxFilesPerMessageExceeded);
            }
        }

        if (hasVoice &&
            !await SettingProvider.IsTrueAsync(ChatSettingNames.Attachments.EnableVoiceMessages))
        {
            throw new BusinessException(ChatErrorCodes.VoiceMessagesDisabled);
        }

        if (hasLocation)
        {
            if (!await SettingProvider.IsTrueAsync(ChatSettingNames.Attachments.EnableLocationSharing))
            {
                throw new BusinessException(ChatErrorCodes.LocationSharingDisabled);
            }

            if (metadata?.Location == null)
            {
                throw new BusinessException(ChatErrorCodes.MessageContentRequired);
            }
        }
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
        var participants = await ParticipantRepository.GetListBySessionAsync(session.Id);
        return await SessionDtoEnricher.EnrichAsync(session, participants);
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
