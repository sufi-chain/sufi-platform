using SufiChain.Chat.AiUsage;
using SufiChain.Chat.ConversationLinks;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Participants;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;

namespace SufiChain.Chat.Mapping;

public class ChatApplicationMapper
{
    public virtual ChatSessionDto ToDto(ChatSession session, List<ChatParticipant>? participants = null)
    {
        return new ChatSessionDto
        {
            Id = session.Id,
            TenantId = session.TenantId,
            Title = session.Title,
            AccessMode = session.AccessMode,
            ConversationKind = session.ConversationKind,
            ChannelOrigin = session.ChannelOrigin,
            Status = session.Status,
            MetadataJson = session.MetadataJson,
            LastMessageTime = session.LastMessageTime,
            CreationTime = session.CreationTime,
            CreatorId = session.CreatorId,
            LastModificationTime = session.LastModificationTime,
            LastModifierId = session.LastModifierId,
            IsDeleted = session.IsDeleted,
            DeleterId = session.DeleterId,
            DeletionTime = session.DeletionTime,
            Participants = participants?.Select(ToDto).ToList() ?? new List<ChatParticipantDto>()
        };
    }

    public virtual ChatSessionListDto ToListDto(ChatSession session, int participantCount = 0)
    {
        return new ChatSessionListDto
        {
            Id = session.Id,
            TenantId = session.TenantId,
            Title = session.Title,
            AccessMode = session.AccessMode,
            ConversationKind = session.ConversationKind,
            ChannelOrigin = session.ChannelOrigin,
            Status = session.Status,
            LastMessageTime = session.LastMessageTime,
            CreationTime = session.CreationTime,
            CreatorId = session.CreatorId,
            LastModificationTime = session.LastModificationTime,
            LastModifierId = session.LastModifierId,
            IsDeleted = session.IsDeleted,
            DeleterId = session.DeleterId,
            DeletionTime = session.DeletionTime,
            ParticipantCount = participantCount
        };
    }

    public virtual ChatParticipantDto ToDto(ChatParticipant participant)
    {
        return new ChatParticipantDto
        {
            Id = participant.Id,
            TenantId = participant.TenantId,
            SessionId = participant.SessionId,
            UserId = participant.UserId,
            AnonymousVisitorId = participant.AnonymousVisitorId,
            ParticipantKind = participant.ParticipantKind,
            DisplayName = participant.DisplayName,
            JoinedAt = participant.JoinedAt,
            LeftAt = participant.LeftAt
        };
    }

    public virtual ChatMessageDto ToDto(ChatMessage message)
    {
        return new ChatMessageDto
        {
            Id = message.Id,
            TenantId = message.TenantId,
            SessionId = message.SessionId,
            Body = message.Body,
            SenderKind = message.SenderKind,
            SenderUserId = message.SenderUserId,
            AnonymousVisitorId = message.AnonymousVisitorId,
            IsInternal = message.IsInternal,
            MetadataJson = message.MetadataJson,
            AttachmentFileIds = message.AttachmentFileIds.ToList(),
            CreationTime = message.CreationTime,
            CreatorId = message.CreatorId,
            LastModificationTime = message.LastModificationTime,
            LastModifierId = message.LastModifierId,
            IsDeleted = message.IsDeleted,
            DeleterId = message.DeleterId,
            DeletionTime = message.DeletionTime
        };
    }

    public virtual ConversationLinkDto ToDto(ConversationLink link)
    {
        return new ConversationLinkDto
        {
            Id = link.Id,
            TenantId = link.TenantId,
            SessionId = link.SessionId,
            LinkedEntityType = link.LinkedEntityType,
            LinkedEntityId = link.LinkedEntityId,
            LinkRole = link.LinkRole,
            MetadataJson = link.MetadataJson
        };
    }

    public virtual ChatAiUsageReservationDto ToDto(ChatAiUsageReservation reservation)
    {
        return new ChatAiUsageReservationDto
        {
            Id = reservation.Id,
            TenantId = reservation.TenantId,
            SessionId = reservation.SessionId,
            UserId = reservation.UserId,
            OperatorUserId = reservation.OperatorUserId,
            ConversationKind = reservation.ConversationKind,
            AccessMode = reservation.AccessMode,
            OperationKind = reservation.OperationKind,
            SourceEntityType = reservation.SourceEntityType,
            SourceEntityId = reservation.SourceEntityId,
            LinkedEntityType = reservation.LinkedEntityType,
            LinkedEntityId = reservation.LinkedEntityId,
            EstimatedTokens = reservation.EstimatedTokens,
            TotalTokens = reservation.TotalTokens,
            ProviderName = reservation.ProviderName,
            WorkspaceName = reservation.WorkspaceName,
            WalletProviderName = reservation.WalletProviderName,
            WalletId = reservation.WalletId,
            BillingSubjectType = reservation.BillingSubjectType,
            BillingSubjectId = reservation.BillingSubjectId,
            IsWalletChargeRequired = reservation.IsWalletChargeRequired,
            Currency = reservation.Currency,
            DenyReason = reservation.DenyReason,
            ReservedAt = reservation.ReservedAt,
            RecordedAt = reservation.RecordedAt,
            CreationTime = reservation.CreationTime,
            CreatorId = reservation.CreatorId,
            LastModificationTime = reservation.LastModificationTime,
            LastModifierId = reservation.LastModifierId,
            IsDeleted = reservation.IsDeleted,
            DeleterId = reservation.DeleterId,
            DeletionTime = reservation.DeletionTime
        };
    }

    public virtual ChatAiUsageRecordDto ToRecordDto(ChatAiUsageReservation reservation)
    {
        return new ChatAiUsageRecordDto
        {
            Id = reservation.Id,
            ReservationId = reservation.Id,
            TenantId = reservation.TenantId,
            SessionId = reservation.SessionId,
            UserId = reservation.UserId,
            OperatorUserId = reservation.OperatorUserId,
            OperationKind = reservation.OperationKind,
            PromptTokens = reservation.PromptTokens ?? 0,
            CompletionTokens = reservation.CompletionTokens ?? 0,
            TotalTokens = reservation.TotalTokens ?? 0,
            ProviderName = reservation.ProviderName,
            WorkspaceName = reservation.WorkspaceName,
            DenyReason = reservation.DenyReason,
            RecordedAt = reservation.RecordedAt
        };
    }

    public virtual ChatUsageCheckResultDto ToDto(ChatUsageCheckResult result)
    {
        return new ChatUsageCheckResultDto
        {
            IsAllowed = result.IsAllowed,
            ReasonCode = result.ReasonCode,
            LocalizationKey = result.LocalizationKey,
            Action = result.Action,
            RequiresAuthentication = result.RequiresAuthentication
        };
    }
}
