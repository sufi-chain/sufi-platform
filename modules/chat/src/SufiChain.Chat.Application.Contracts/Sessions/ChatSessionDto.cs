using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.Chat.Sessions;

public class ChatSessionDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public string? Title { get; set; }

    public AccessMode AccessMode { get; set; }

    public ConversationKind ConversationKind { get; set; }

    public ChannelOrigin ChannelOrigin { get; set; }

    public ChatSessionStatus Status { get; set; }

    public string? MetadataJson { get; set; }

    public DateTime? LastMessageTime { get; set; }

    public List<ChatParticipantDto> Participants { get; set; } = new();
}

public class ChatSessionListDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public string? Title { get; set; }

    public AccessMode AccessMode { get; set; }

    public ConversationKind ConversationKind { get; set; }

    public ChannelOrigin ChannelOrigin { get; set; }

    public ChatSessionStatus Status { get; set; }

    public DateTime? LastMessageTime { get; set; }

    public int ParticipantCount { get; set; }
}

public class ChatParticipantDto : EntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public Guid SessionId { get; set; }

    public Guid? UserId { get; set; }

    public string? AnonymousVisitorId { get; set; }

    public ChatMessageSenderKind ParticipantKind { get; set; }

    public string? DisplayName { get; set; }

    public DateTime JoinedAt { get; set; }

    public DateTime? LeftAt { get; set; }
}
