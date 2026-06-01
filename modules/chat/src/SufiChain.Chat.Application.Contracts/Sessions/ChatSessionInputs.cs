using System.ComponentModel.DataAnnotations;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.Chat.Sessions;

public class CreateChatSessionInput
{
    [StringLength(ChatConsts.MaxTitleLength)]
    public string? Title { get; set; }

    public AccessMode AccessMode { get; set; } = AccessMode.PublicAuthenticated;

    public ConversationKind ConversationKind { get; set; } = ConversationKind.Direct;

    public ChannelOrigin ChannelOrigin { get; set; } = ChannelOrigin.Web;

    [StringLength(ChatConsts.MaxMetadataJsonLength)]
    public string? MetadataJson { get; set; }

    public string? AnonymousVisitorId { get; set; }

    public string? AnonymousClientIpHash { get; set; }

    public List<AddChatParticipantInput> Participants { get; set; } = new();
}

public class GetChatSessionListInput : PagedAndSortedResultRequestDto
{
    public ChatSessionStatus? Status { get; set; }

    public ConversationKind? ConversationKind { get; set; }

    public AccessMode? AccessMode { get; set; }
}

public class GetMyChatSessionsInput : PagedAndSortedResultRequestDto
{
    public ChatSessionStatus? Status { get; set; }

    public ConversationKind? ConversationKind { get; set; }
}

public class CloseChatSessionInput
{
    public Guid? ClosedByUserId { get; set; }
}

public class AddChatParticipantInput
{
    public Guid? UserId { get; set; }

    [StringLength(ChatConsts.MaxAnonymousVisitorIdLength)]
    public string? AnonymousVisitorId { get; set; }

    public ChatMessageSenderKind ParticipantKind { get; set; } = ChatMessageSenderKind.Visitor;

    [StringLength(ChatConsts.MaxDisplayNameLength)]
    public string? DisplayName { get; set; }
}

public class GetOrCreateDirectSessionInput
{
    [Required]
    public Guid OtherUserId { get; set; }

    public ChannelOrigin ChannelOrigin { get; set; } = ChannelOrigin.Web;

    [StringLength(ChatConsts.MaxMetadataJsonLength)]
    public string? MetadataJson { get; set; }
}

public class CreateGroupChatSessionInput
{
    [StringLength(ChatConsts.MaxTitleLength)]
    public string? Title { get; set; }

    public ChannelOrigin ChannelOrigin { get; set; } = ChannelOrigin.Web;

    [StringLength(ChatConsts.MaxMetadataJsonLength)]
    public string? MetadataJson { get; set; }

    public List<AddChatParticipantInput> Participants { get; set; } = new();
}
