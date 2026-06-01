using System.ComponentModel.DataAnnotations;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.Chat.ConversationLinks;

public class ConversationLinkDto : EntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public Guid SessionId { get; set; }

    public string LinkedEntityType { get; set; } = string.Empty;

    public string LinkedEntityId { get; set; } = string.Empty;

    public string? LinkRole { get; set; }

    public string? MetadataJson { get; set; }
}

public class CreateConversationLinkInput
{
    [Required]
    public Guid SessionId { get; set; }

    [Required]
    [StringLength(ChatConsts.MaxLinkedEntityTypeLength)]
    public string LinkedEntityType { get; set; } = string.Empty;

    [Required]
    [StringLength(ChatConsts.MaxLinkedEntityIdLength)]
    public string LinkedEntityId { get; set; } = string.Empty;

    [StringLength(ChatConsts.MaxLinkRoleLength)]
    public string? LinkRole { get; set; }

    [StringLength(ChatConsts.MaxMetadataJsonLength)]
    public string? MetadataJson { get; set; }
}
