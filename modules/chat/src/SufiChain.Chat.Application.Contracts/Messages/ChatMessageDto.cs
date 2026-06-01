using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.Chat.Messages;

public class ChatMessageDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public Guid SessionId { get; set; }

    public string Body { get; set; } = string.Empty;

    public ChatMessageSenderKind SenderKind { get; set; }

    public Guid? SenderUserId { get; set; }

    public string? AnonymousVisitorId { get; set; }

    public bool IsInternal { get; set; }

    public string? MetadataJson { get; set; }

    public List<Guid> AttachmentFileIds { get; set; } = new();
}
