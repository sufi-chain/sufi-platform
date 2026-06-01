using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SufiChain.Chat.Participants;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace SufiChain.Chat.Sessions;

public class ChatSessionManager : DomainService
{
    public const int DefaultMaxGroupParticipants = 100;

    private readonly IChatSessionRepository _sessionRepository;
    private readonly IChatParticipantRepository _participantRepository;

    public ChatSessionManager(
        IChatSessionRepository sessionRepository,
        IChatParticipantRepository participantRepository)
    {
        _sessionRepository = sessionRepository;
        _participantRepository = participantRepository;
    }

    public virtual async Task<ChatSession> CreateAsync(
        string? title,
        AccessMode accessMode,
        ConversationKind conversationKind,
        ChannelOrigin channelOrigin,
        string? metadataJson = null)
    {
        var session = new ChatSession(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            title,
            accessMode,
            conversationKind,
            channelOrigin,
            metadataJson);

        return await Task.FromResult(session);
    }

    public virtual async Task<ChatSession> GetOrCreateDirectSessionAsync(
        Guid userId,
        Guid otherUserId,
        ChannelOrigin channelOrigin = ChannelOrigin.Web,
        string? metadataJson = null)
    {
        if (userId == otherUserId)
        {
            throw new BusinessException(ChatErrorCodes.InvalidParticipant);
        }

        var existing = await _sessionRepository.FindDirectSessionByUserPairAsync(
            CurrentTenant.Id,
            userId,
            otherUserId);

        if (existing != null)
        {
            return existing;
        }

        var session = await CreateAsync(
            null,
            AccessMode.PublicAuthenticated,
            ConversationKind.Direct,
            channelOrigin,
            metadataJson);

        await _sessionRepository.InsertAsync(session, autoSave: true);

        await _participantRepository.InsertAsync(new ChatParticipant(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            session.Id,
            ChatMessageSenderKind.Visitor,
            Clock.Now,
            userId: userId), autoSave: true);

        await _participantRepository.InsertAsync(new ChatParticipant(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            session.Id,
            ChatMessageSenderKind.Visitor,
            Clock.Now,
            userId: otherUserId), autoSave: true);

        return session;
    }

    public virtual async Task ValidateDirectParticipantsAsync(IEnumerable<ChatParticipant> participants)
    {
        var participantList = participants.ToList();

        if (participantList.Count != 2 || participantList.Any(participant => !participant.UserId.HasValue))
        {
            throw new BusinessException(ChatErrorCodes.DirectSessionRequiresTwoParticipants);
        }

        await Task.CompletedTask;
    }

    public virtual async Task EnsureCanAddParticipantAsync(
        ChatSession session,
        int maxGroupParticipants = DefaultMaxGroupParticipants)
    {
        if (session.ConversationKind != ConversationKind.Group)
        {
            return;
        }

        var activeCount = await _participantRepository.GetActiveCountAsync(session.Id);
        if (activeCount >= maxGroupParticipants)
        {
            throw new BusinessException(ChatErrorCodes.GroupParticipantLimitExceeded)
                .WithData("MaxGroupParticipants", maxGroupParticipants);
        }
    }

    public virtual async Task CloseAsync(ChatSession session, Guid? closedByUserId = null)
    {
        session.Close(closedByUserId);
        await _sessionRepository.UpdateAsync(session, autoSave: true);
    }
}
