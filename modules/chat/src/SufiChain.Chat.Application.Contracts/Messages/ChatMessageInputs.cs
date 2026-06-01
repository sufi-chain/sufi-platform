using System.ComponentModel.DataAnnotations;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.Chat.Messages;

public class SendChatMessageInput
{
    [Required]
    public Guid SessionId { get; set; }

    [Required]
    [StringLength(ChatConsts.MaxMessageBodyLength)]
    public string Body { get; set; } = string.Empty;

    public ChatMessageSenderKind SenderKind { get; set; } = ChatMessageSenderKind.Visitor;

    public Guid? SenderUserId { get; set; }

    [StringLength(ChatConsts.MaxAnonymousVisitorIdLength)]
    public string? AnonymousVisitorId { get; set; }

    public string? AnonymousClientIpHash { get; set; }

    public AccessMode AccessMode { get; set; } = AccessMode.PublicAuthenticated;

    public bool IsInternal { get; set; }

    [StringLength(ChatConsts.MaxMetadataJsonLength)]
    public string? MetadataJson { get; set; }

    public List<Guid> AttachmentFileIds { get; set; } = new();
}

public class GetChatMessageListInput : PagedAndSortedResultRequestDto
{
    [Required]
    public Guid SessionId { get; set; }

    public bool IncludeInternal { get; set; }
}
