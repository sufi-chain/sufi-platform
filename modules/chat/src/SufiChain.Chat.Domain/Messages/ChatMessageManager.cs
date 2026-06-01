using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SufiChain.Chat.Events;
using SufiChain.Chat.Participants;
using SufiChain.Chat.Sessions;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace SufiChain.Chat.Messages;

public class ChatMessageManager : DomainService
{
    private readonly IChatMessageRepository _messageRepository;
    private readonly IChatParticipantRepository _participantRepository;
    private readonly IChatSessionRepository _sessionRepository;

    public ChatMessageManager(
        IChatMessageRepository messageRepository,
        IChatParticipantRepository participantRepository,
        IChatSessionRepository sessionRepository)
    {
        _messageRepository = messageRepository;
        _participantRepository = participantRepository;
        _sessionRepository = sessionRepository;
    }

    public virtual async Task<ChatMessage> SendAsync(
        ChatSession session,
        string body,
        ChatMessageSenderKind senderKind,
        Guid? senderUserId = null,
        string? anonymousVisitorId = null,
        bool isInternal = false,
        bool isAuthorizedOperator = false,
        string? metadataJson = null,
        IEnumerable<Guid>? attachmentFileIds = null)
    {
        session.EnsureOpen();

        if (!isAuthorizedOperator && !await _participantRepository.IsParticipantAsync(
                session.Id,
                senderUserId,
                anonymousVisitorId))
        {
            throw new BusinessException(ChatErrorCodes.ParticipantRequired);
        }

        var message = new ChatMessage(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            session.Id,
            body,
            senderKind,
            senderUserId,
            anonymousVisitorId,
            isInternal,
            metadataJson,
            attachmentFileIds);

        await _messageRepository.InsertAsync(message, autoSave: true);

        session.MarkMessageReceived(message.Id, Clock.Now);
        await _sessionRepository.UpdateAsync(session, autoSave: true);

        return message;
    }
}
