using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.ETOs;
using SufiChain.Chat.Mapping;
using SufiChain.Chat.Participants;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Realtime;
using SufiChain.Chat.Usage;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp;
using Volo.Abp.Linq;
using Volo.Abp.EventBus.Distributed;

namespace SufiChain.Chat.Sessions;

[Authorize(ChatPermissions.Sessions.Default)]
public class ChatSessionAppService : ChatAppService, IChatSessionAppService
{
    protected IChatSessionRepository SessionRepository { get; }
    protected IChatParticipantRepository ParticipantRepository { get; }
    protected ChatSessionManager SessionManager { get; }
    protected IChatUsageGuard UsageGuard { get; }
    protected ChatApplicationMapper Mapper { get; }
    protected IDistributedEventBus DistributedEventBus { get; }
    protected IChatRealtimeNotifier RealtimeNotifier { get; }

    public ChatSessionAppService(
        IChatSessionRepository sessionRepository,
        IChatParticipantRepository participantRepository,
        ChatSessionManager sessionManager,
        IChatUsageGuard usageGuard,
        ChatApplicationMapper mapper,
        IDistributedEventBus distributedEventBus,
        IChatRealtimeNotifier realtimeNotifier)
    {
        SessionRepository = sessionRepository;
        ParticipantRepository = participantRepository;
        SessionManager = sessionManager;
        UsageGuard = usageGuard;
        Mapper = mapper;
        DistributedEventBus = distributedEventBus;
        RealtimeNotifier = realtimeNotifier;
    }

    [Authorize(ChatPermissions.Sessions.Create)]
    public virtual async Task<ChatSessionDto> CreateAsync(CreateChatSessionInput input)
    {
        var usageResult = await UsageGuard.CheckCanStartSessionAsync(new ChatStartSessionContext
        {
            TenantId = CurrentTenant.Id,
            UserId = CurrentUser.Id,
            AnonymousVisitorId = input.AnonymousVisitorId,
            AnonymousClientIpHash = input.AnonymousClientIpHash,
            AccessMode = input.AccessMode,
            ConversationKind = input.ConversationKind,
            ChannelOrigin = input.ChannelOrigin
        });

        await EnsureUsageAllowedAsync(Guid.Empty, usageResult);

        var session = await SessionManager.CreateAsync(
            input.Title,
            input.AccessMode,
            input.ConversationKind,
            input.ChannelOrigin,
            input.MetadataJson);

        await SessionRepository.InsertAsync(session, autoSave: true);

        foreach (var participantInput in input.Participants)
        {
            await AddParticipantEntityAsync(session, participantInput);
        }

        await PublishSessionCreatedAsync(session);
        var sessionDto = await MapWithParticipantsAsync(session);
        await RealtimeNotifier.NotifySessionUpdatedAsync(sessionDto);
        return sessionDto;
    }

    public virtual async Task<ChatSessionDto> GetAsync(Guid id)
    {
        var session = await SessionRepository.GetAsync(id);
        return await MapWithParticipantsAsync(session);
    }

    public virtual async Task<PagedResultDto<ChatSessionListDto>> GetListAsync(GetChatSessionListInput input)
    {
        await CheckPolicyAsync(ChatPermissions.Sessions.Manage);

        var queryable = await SessionRepository.GetQueryableAsync();
        var filtered = queryable
            .WhereIf(input.Status.HasValue, session => session.Status == input.Status)
            .WhereIf(input.ConversationKind.HasValue, session => session.ConversationKind == input.ConversationKind)
            .WhereIf(input.AccessMode.HasValue, session => session.AccessMode == input.AccessMode);

        var totalCount = filtered.LongCount();
        var sessions = filtered
            .OrderByDescending(session => session.LastMessageTime ?? session.CreationTime)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<ChatSessionListDto>(
            totalCount,
            sessions.Select(session => Mapper.ToListDto(session)).ToList());
    }

    [Authorize(ChatPermissions.Sessions.Close)]
    public virtual async Task CloseAsync(Guid id, CloseChatSessionInput? input = null)
    {
        var session = await SessionRepository.GetAsync(id);
        if (session.Status == ChatSessionStatus.Closed)
        {
            return;
        }

        var closedByUserId = input?.ClosedByUserId ?? CurrentUser.Id;
        await SessionManager.CloseAsync(session, closedByUserId);
        await PublishSessionClosedAsync(session, closedByUserId);
        await RealtimeNotifier.NotifySessionUpdatedAsync(await MapWithParticipantsAsync(session));
    }

    [Authorize(ChatPermissions.Sessions.Manage)]
    public virtual async Task AddParticipantAsync(Guid sessionId, AddChatParticipantInput input)
    {
        var session = await SessionRepository.GetAsync(sessionId);
        await SessionManager.EnsureCanAddParticipantAsync(session);
        await AddParticipantEntityAsync(session, input);
        await RealtimeNotifier.NotifySessionUpdatedAsync(await MapWithParticipantsAsync(session));
    }

    [Authorize(ChatPermissions.Sessions.Manage)]
    public virtual async Task RemoveParticipantAsync(Guid sessionId, Guid participantId)
    {
        var participant = await ParticipantRepository.GetAsync(participantId);
        if (participant.SessionId != sessionId)
        {
            throw new BusinessException(ChatErrorCodes.InvalidParticipant);
        }

        participant.Leave(Clock.Now);
        await ParticipantRepository.UpdateAsync(participant, autoSave: true);
        await RealtimeNotifier.NotifySessionUpdatedAsync(await MapWithParticipantsAsync(await SessionRepository.GetAsync(sessionId)));
    }

    public virtual async Task<PagedResultDto<ChatSessionListDto>> GetMySessionsAsync(GetMyChatSessionsInput input)
    {
        if (!CurrentUser.Id.HasValue)
        {
            return new PagedResultDto<ChatSessionListDto>(0, new List<ChatSessionListDto>());
        }

        var sessions = await SessionRepository.GetSessionsForParticipantAsync(
            CurrentTenant.Id,
            CurrentUser.Id,
            skipCount: input.SkipCount,
            maxResultCount: input.MaxResultCount);

        var filtered = sessions
            .WhereIf(input.Status.HasValue, session => session.Status == input.Status)
            .WhereIf(input.ConversationKind.HasValue, session => session.ConversationKind == input.ConversationKind)
            .ToList();

        return new PagedResultDto<ChatSessionListDto>(
            filtered.Count,
            filtered.Select(session => Mapper.ToListDto(session)).ToList());
    }

    [Authorize(ChatPermissions.Sessions.Create)]
    public virtual async Task<ChatSessionDto> GetOrCreateDirectSessionAsync(GetOrCreateDirectSessionInput input)
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new BusinessException(ChatErrorCodes.ParticipantRequired);
        }

        var usageResult = await UsageGuard.CheckCanStartSessionAsync(new ChatStartSessionContext
        {
            TenantId = CurrentTenant.Id,
            UserId = CurrentUser.Id,
            AccessMode = AccessMode.PublicAuthenticated,
            ConversationKind = ConversationKind.Direct,
            ChannelOrigin = input.ChannelOrigin
        });

        await EnsureUsageAllowedAsync(Guid.Empty, usageResult);

        var session = await SessionManager.GetOrCreateDirectSessionAsync(
            CurrentUser.Id.Value,
            input.OtherUserId,
            input.ChannelOrigin,
            input.MetadataJson);

        var sessionDto = await MapWithParticipantsAsync(session);
        await RealtimeNotifier.NotifySessionUpdatedAsync(sessionDto);
        return sessionDto;
    }

    [Authorize(ChatPermissions.Sessions.Create)]
    public virtual async Task<ChatSessionDto> CreateGroupSessionAsync(CreateGroupChatSessionInput input)
    {
        var createInput = new CreateChatSessionInput
        {
            Title = input.Title,
            AccessMode = AccessMode.PublicAuthenticated,
            ConversationKind = ConversationKind.Group,
            ChannelOrigin = input.ChannelOrigin,
            MetadataJson = input.MetadataJson,
            Participants = input.Participants
        };

        if (CurrentUser.Id.HasValue && createInput.Participants.All(participant => participant.UserId != CurrentUser.Id))
        {
            createInput.Participants.Add(new AddChatParticipantInput
            {
                UserId = CurrentUser.Id,
                ParticipantKind = ChatMessageSenderKind.Visitor
            });
        }

        return await CreateAsync(createInput);
    }

    public virtual async Task<ChatSessionDto> JoinGroupSessionAsync(Guid sessionId)
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new BusinessException(ChatErrorCodes.ParticipantRequired);
        }

        var session = await SessionRepository.GetAsync(sessionId);
        await SessionManager.EnsureCanAddParticipantAsync(session);

        var isParticipant = await ParticipantRepository.IsParticipantAsync(session.Id, CurrentUser.Id);
        if (!isParticipant)
        {
            await AddParticipantEntityAsync(session, new AddChatParticipantInput
            {
                UserId = CurrentUser.Id,
                ParticipantKind = ChatMessageSenderKind.Visitor
            });
        }

        var sessionDto = await MapWithParticipantsAsync(session);
        await RealtimeNotifier.NotifySessionUpdatedAsync(sessionDto);
        return sessionDto;
    }

    public virtual async Task LeaveGroupSessionAsync(Guid sessionId)
    {
        if (!CurrentUser.Id.HasValue)
        {
            return;
        }

        var participants = await ParticipantRepository.GetListBySessionAsync(sessionId);
        var participant = participants.FirstOrDefault(item => item.UserId == CurrentUser.Id && item.LeftAt == null);
        if (participant == null)
        {
            return;
        }

        participant.Leave(Clock.Now);
        await ParticipantRepository.UpdateAsync(participant, autoSave: true);
        await RealtimeNotifier.NotifySessionUpdatedAsync(await MapWithParticipantsAsync(await SessionRepository.GetAsync(sessionId)));
    }

    protected virtual async Task AddParticipantEntityAsync(ChatSession session, AddChatParticipantInput input)
    {
        await ParticipantRepository.InsertAsync(
            new ChatParticipant(
                GuidGenerator.Create(),
                CurrentTenant.Id,
                session.Id,
                input.ParticipantKind,
                Clock.Now,
                input.UserId,
                input.AnonymousVisitorId,
                input.DisplayName),
            autoSave: true);
    }

    protected virtual async Task<ChatSessionDto> MapWithParticipantsAsync(ChatSession session)
    {
        var participants = await ParticipantRepository.GetListBySessionAsync(session.Id);
        return Mapper.ToDto(session, participants);
    }

    protected virtual async Task EnsureUsageAllowedAsync(Guid sessionId, ChatUsageCheckResult result)
    {
        if (!result.IsAllowed)
        {
            if (sessionId != Guid.Empty)
            {
                await RealtimeNotifier.NotifyUsageLimitExceededAsync(sessionId, Mapper.ToDto(result));
            }

            throw new BusinessException(result.ReasonCode ?? ChatErrorCodes.UsageLimitExceeded);
        }
    }

    protected virtual async Task PublishSessionCreatedAsync(ChatSession session)
    {
        await DistributedEventBus.PublishAsync(new ChatSessionCreatedEto
        {
            Id = session.Id,
            TenantId = session.TenantId,
            Title = session.Title,
            AccessMode = session.AccessMode,
            ConversationKind = session.ConversationKind,
            ChannelOrigin = session.ChannelOrigin,
            CreatedAt = session.CreationTime
        });
    }

    protected virtual async Task PublishSessionClosedAsync(ChatSession session, Guid? closedByUserId)
    {
        await DistributedEventBus.PublishAsync(new ChatSessionClosedEto
        {
            Id = session.Id,
            TenantId = session.TenantId,
            ClosedByUserId = closedByUserId,
            ClosedAt = Clock.Now
        });
    }
}
